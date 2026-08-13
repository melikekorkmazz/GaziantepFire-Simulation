using GaziantepFire.Application.Interfaces;
using GaziantepFire.Domain.Entities;
using GaziantepFire.Domain.Enums;
using GaziantepFire.Persistence.Context;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Net.Http.Json;
using System.Text.Json.Serialization;

namespace GaziantepFire.Infrastructure.Services;

public class IncidentSyncService : IIncidentSyncService
{
    private const string IhbarlarApiUrl = "https://acikveriapi.gaziantep.bel.tr/api/Itfaiye/Ihbarlar";
    private const string YanginNoktalariApiUrl = "https://acikveriapi.gaziantep.bel.tr/api/Itfaiye/YanginNoktalari";

    private readonly GaziantepFireDbContext _context;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<IncidentSyncService> _logger;

    private static readonly Dictionary<string, (double Lat, double Lng)> DistrictCenters = new(StringComparer.OrdinalIgnoreCase)
    {
        ["SAHINBEY"]    = (37.0662, 37.3833),
        ["SEHITKAMIL"]  = (37.0850, 37.4100),
        ["NIZIP"]       = (37.0000, 37.7850),
        ["OGUZELI"]     = (36.9660, 37.5030),
        ["ISLAHIYE"]    = (37.0240, 36.6360),
        ["ARABAN"]      = (37.4260, 37.6910),
        ["NURDAGI"]     = (37.1820, 36.7320),
        ["YAVUZELI"]    = (37.2970, 37.5660),
        ["KARKAMIS"]    = (36.8450, 37.9920),
        ["BEGENDEREZ"]  = (36.7720, 36.8550),
        ["HALFETI"]     = (37.2530, 37.8760),
        ["GAZIANTEP"]   = (37.0662, 37.3833),
    };

    public IncidentSyncService(
        GaziantepFireDbContext context,
        IHttpClientFactory httpClientFactory,
        ILogger<IncidentSyncService> logger)
    {
        _context = context;
        _httpClientFactory = httpClientFactory;
        _logger = logger;
    }

    public async Task<int> SyncTodayAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("[IncidentSync] Starting daily sync at {Time}", DateTime.Now);

        var newIncidents = new List<Incident>();
        var client = _httpClientFactory.CreateClient("GaziantepApi");
        
        var existingExternalIds = await _context.Incidents
            .Where(i => i.ExternalId != null)
            .Select(i => i.ExternalId!.Value)
            .ToHashSetAsync(cancellationToken);
            
        var neighborhoods = await _context.Neighborhoods
            .Include(n => n.District)
            .ToListAsync(cancellationToken);

        int totalParsed = 0;
        int failed = 0;

        // Fetch Yangin Noktalari
        try
        {
            var response = await client.GetFromJsonAsync<YanginNoktalariResponse>(YanginNoktalariApiUrl, cancellationToken);
            if (response?.Data != null)
            {
                totalParsed += response.Data.Count;
                foreach (var item in response.Data)
                {
                    if (existingExternalIds.Contains(item.Id)) continue;
                    newIncidents.Add(MapYanginNoktalari(item, neighborhoods));
                    existingExternalIds.Add(item.Id);
                }
            }
        }
        catch (Exception ex)
        {
            failed++;
            _logger.LogError(ex, "[IncidentSync] Failed to fetch or parse Yangin Noktalari API");
        }

        // Fetch Ihbarlar
        try
        {
            var response = await client.GetFromJsonAsync<IhbarlarResponse>(IhbarlarApiUrl, cancellationToken);
            if (response?.Data?.Data != null)
            {
                totalParsed += response.Data.Data.Count;
                foreach (var item in response.Data.Data)
                {
                    if (existingExternalIds.Contains(item.Id)) continue;
                    newIncidents.Add(MapIhbarlar(item, neighborhoods));
                    existingExternalIds.Add(item.Id);
                }
            }
        }
        catch (Exception ex)
        {
            failed++;
            _logger.LogError(ex, "[IncidentSync] Failed to fetch or parse Ihbarlar API");
        }

        int inserted = 0;
        int missingCoords = 0;
        
        foreach (var inc in newIncidents)
        {
            if (inc.CoordinateSource == CoordinateSource.Synthetic)
                missingCoords++;
        }

        if (newIncidents.Count > 0)
        {
            await _context.Incidents.AddRangeAsync(newIncidents, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);
            inserted = newIncidents.Count;
        }

        _logger.LogInformation(
            "[IncidentSync]\nAPI records parsed: {TotalParsed}\nInserted: {Inserted}\nMissing coordinates: {MissingCoords}\nSynthetic coordinates generated: {MissingCoords}\nFailed API calls: {Failed}",
            totalParsed, inserted, missingCoords, missingCoords, failed);

        return inserted;
    }

    private Incident MapYanginNoktalari(YanginItem item, List<Neighborhood> neighborhoods)
    {
        var mahalleUpper = item.MahalleAdi?.ToUpperInvariant() ?? string.Empty;
        var ilceUpper = item.IlceAdi?.ToUpperInvariant() ?? string.Empty;

        var neighborhood = neighborhoods.FirstOrDefault(n => 
            n.Name.ToUpperInvariant().Contains(mahalleUpper) && 
            n.District != null && 
            n.District.Name.ToUpperInvariant().Contains(ilceUpper));
            
        neighborhood ??= neighborhoods.FirstOrDefault(n => 
            n.District != null && n.District.Name.ToUpperInvariant().Contains(ilceUpper)) 
            ?? neighborhoods.First();

        var coords = GenerateDeterministicSyntheticCoordinate(item.Id, neighborhood, ilceUpper);

        return new Incident
        {
            ExternalId = item.Id,
            IncidentType = IncidentType.Fire,
            SubType = item.YanginTuruTxt ?? item.TuruTxt ?? string.Empty,
            NeighborhoodId = neighborhood.Id,
            Latitude = coords.Lat,
            Longitude = coords.Lng,
            CoordinateSource = CoordinateSource.Synthetic,
            CreatedAt = item.BildirimTarihi,
            ResponseTimeInMinutes = item.RaporMudahaledk ?? 0
        };
    }

    private Incident MapIhbarlar(IhbarItem item, List<Neighborhood> neighborhoods)
    {
        var ilceUpper = item.IlceAdi?.ToUpperInvariant() ?? string.Empty;

        var neighborhood = neighborhoods.FirstOrDefault(n => 
            n.District != null && n.District.Name.ToUpperInvariant().Contains(ilceUpper)) 
            ?? neighborhoods.First();

        var type = item.Tur?.ToLowerInvariant().Contains("yangın") == true ? IncidentType.Fire : IncidentType.Rescue;
        var coords = GenerateDeterministicSyntheticCoordinate(item.Id, neighborhood, ilceUpper);

        return new Incident
        {
            ExternalId = item.Id,
            IncidentType = type,
            SubType = item.AltTur ?? item.Tur ?? string.Empty,
            NeighborhoodId = neighborhood.Id,
            Latitude = coords.Lat,
            Longitude = coords.Lng,
            CoordinateSource = CoordinateSource.Synthetic,
            CreatedAt = item.BildirimTarihi,
            ResponseTimeInMinutes = 0
        };
    }

    private (double Lat, double Lng) GenerateDeterministicSyntheticCoordinate(int externalId, Neighborhood neighborhood, string ilceUpper)
    {
        var random = new Random(externalId);

        double minLng = 1000, maxLng = -1000, minLat = 1000, maxLat = -1000;
        bool hasBounds = false;

        if (!string.IsNullOrEmpty(neighborhood.PolygonBoundaryGeoJson))
        {
            try
            {
                using var doc = System.Text.Json.JsonDocument.Parse(neighborhood.PolygonBoundaryGeoJson);
                var root = doc.RootElement;
                if (root.TryGetProperty("coordinates", out var coordsArray) && coordsArray.ValueKind == System.Text.Json.JsonValueKind.Array)
                {
                    // GeoJSON format: coordinates[0] is the outer ring (array of [lng, lat])
                    var ring = coordsArray[0];
                    if (ring.ValueKind == System.Text.Json.JsonValueKind.Array)
                    {
                        foreach (var pt in ring.EnumerateArray())
                        {
                            if (pt.GetArrayLength() >= 2)
                            {
                                double lng = pt[0].GetDouble();
                                double lat = pt[1].GetDouble();
                                if (lng < minLng) minLng = lng;
                                if (lng > maxLng) maxLng = lng;
                                if (lat < minLat) minLat = lat;
                                if (lat > maxLat) maxLat = lat;
                                hasBounds = true;
                            }
                        }
                    }
                }
            }
            catch { /* Ignore parsing errors and fallback */ }
        }

        if (hasBounds)
        {
            var lat = minLat + (maxLat - minLat) * random.NextDouble();
            var lng = minLng + (maxLng - minLng) * random.NextDouble();
            return (Math.Round(lat, 6), Math.Round(lng, 6));
        }

        // Fallback to district center if we can't do anything better
        if (!DistrictCenters.TryGetValue(ilceUpper, out var center))
        {
            center = DistrictCenters["GAZIANTEP"];
        }

        // Generate synthetic coordinates around the center point deterministicly
        // Using a 0.03 degree spread (roughly 3-4km) around the district center
        var latOffset = (random.NextDouble() - 0.5) * 0.03;
        var lngOffset = (random.NextDouble() - 0.5) * 0.03;

        return (Math.Round(center.Lat + latOffset, 6), Math.Round(center.Lng + lngOffset, 6));
    }


    // API Models
    private sealed class YanginNoktalariResponse
    {
        [JsonPropertyName("success")]
        public bool Success { get; set; }

        [JsonPropertyName("data")]
        public List<YanginItem>? Data { get; set; }
    }

    private sealed class YanginItem
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }

        [JsonPropertyName("bildirimTarihi")]
        public DateTime BildirimTarihi { get; set; }

        [JsonPropertyName("turuTxt")]
        public string? TuruTxt { get; set; }

        [JsonPropertyName("yanginTuruTxt")]
        public string? YanginTuruTxt { get; set; }

        [JsonPropertyName("mahalleAdi")]
        public string? MahalleAdi { get; set; }

        [JsonPropertyName("ilceAdi")]
        public string? IlceAdi { get; set; }
        
        [JsonPropertyName("raporMudahaledk")]
        public int? RaporMudahaledk { get; set; }
    }

    private sealed class IhbarlarResponse
    {
        [JsonPropertyName("success")]
        public bool Success { get; set; }

        [JsonPropertyName("data")]
        public IhbarlarDataWrapper? Data { get; set; }
    }

    private sealed class IhbarlarDataWrapper
    {
        [JsonPropertyName("totalCount")]
        public int TotalCount { get; set; }

        [JsonPropertyName("data")]
        public List<IhbarItem>? Data { get; set; }
    }

    private sealed class IhbarItem
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }

        [JsonPropertyName("bildirimTarihi")]
        public DateTime BildirimTarihi { get; set; }

        [JsonPropertyName("tur")]
        public string? Tur { get; set; }

        [JsonPropertyName("altTur")]
        public string? AltTur { get; set; }

        [JsonPropertyName("ilceAdi")]
        public string? IlceAdi { get; set; }
    }
}
