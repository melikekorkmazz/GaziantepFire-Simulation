using GaziantepFire.Domain.Entities;
using Microsoft.Extensions.Logging;
using System.Xml.Linq;

namespace GaziantepFire.Infrastructure.Services;

/// <summary>
/// Parses KML files exported from Google Maps, QGIS, or Gaziantep Belediyesi
/// Açık Harita into Domain entities.
///
/// Expected KML files in wwwroot/data/:
///   districts.kml      → District entities  (Polygon Placemarks)
///   neighborhoods.kml  → Neighborhood entities (Polygon Placemarks)
///   stations.kml       → FireStation entities (Point Placemarks)
///
/// The parser handles both simple KML and extended-data KML structures.
/// </summary>
public class KmlImportService
{
    private readonly ILogger<KmlImportService> _logger;
    private static readonly XNamespace Kml = "http://www.opengis.net/kml/2.2";

    public KmlImportService(ILogger<KmlImportService> logger)
    {
        _logger = logger;
    }

    public List<District> ParseDistricts(string kmlPath)
    {
        if (!File.Exists(kmlPath))
        {
            _logger.LogWarning("[KmlImport] Districts KML not found at {Path}. Using mock data.", kmlPath);
            return new List<District>();
        }

        var doc = XDocument.Load(kmlPath);
        var districts = new List<District>();
        int id = 1;

        foreach (var placemark in GetPlacemarks(doc))
        {
            var name = GetName(placemark);
            var polygon = ExtractPolygonGeoJson(placemark);
            if (string.IsNullOrEmpty(polygon)) continue;

            districts.Add(new District
            {
                Id = id++,
                Name = name,
                PolygonBoundaryGeoJson = polygon
            });
        }

        _logger.LogInformation("[KmlImport] Parsed {Count} districts from {Path}", districts.Count, kmlPath);
        return districts;
    }

    public List<Neighborhood> ParseNeighborhoods(string kmlPath, List<District> districts)
    {
        if (!File.Exists(kmlPath))
        {
            _logger.LogWarning("[KmlImport] Neighborhoods KML not found at {Path}. Using mock data.", kmlPath);
            return new List<Neighborhood>();
        }

        var doc = XDocument.Load(kmlPath);
        var neighborhoods = new List<Neighborhood>();
        int id = 1;

        // Build a lookup: district name (upper) → district ID
        var districtLookup = districts.ToDictionary(
            d => d.Name.ToUpperInvariant(),
            d => d.Id);

        foreach (var placemark in GetPlacemarks(doc))
        {
            var name = GetName(placemark);
            var polygon = ExtractPolygonGeoJson(placemark);
            if (string.IsNullOrEmpty(polygon)) continue;

            // Try to resolve parent district from extended data or name prefix
            var districtId = ResolveDistrictId(placemark, name, districtLookup)
                             ?? districts.FirstOrDefault()?.Id
                             ?? 1;

            neighborhoods.Add(new Neighborhood
            {
                Id = id++,
                DistrictId = districtId,
                Name = name,
                PolygonBoundaryGeoJson = polygon,
                RiskLevel = 5.0  // Default risk; updated by analysis
            });
        }

        _logger.LogInformation("[KmlImport] Parsed {Count} neighborhoods from {Path}", neighborhoods.Count, kmlPath);
        return neighborhoods;
    }

    public List<FireStation> ParseFireStations(string kmlPath)
    {
        if (!File.Exists(kmlPath))
        {
            _logger.LogWarning("[KmlImport] Stations KML not found at {Path}. Using mock data.", kmlPath);
            return new List<FireStation>();
        }

        var doc = XDocument.Load(kmlPath);
        var stations = new List<FireStation>();
        int id = 1;

        foreach (var placemark in GetPlacemarks(doc))
        {
            var name = GetName(placemark);
            var point = GetPoint(placemark);
            if (point == null) continue;

            stations.Add(new FireStation
            {
                Id = id++,
                Name = name,
                Latitude = point.Value.Lat,
                Longitude = point.Value.Lng,
                Capacity = 10  // Default; override via extended data if present
            });
        }

        _logger.LogInformation("[KmlImport] Parsed {Count} fire stations from {Path}", stations.Count, kmlPath);
        return stations;
    }

    // ── KML helpers ─────────────────────────────────────────────────────────

    private static IEnumerable<XElement> GetPlacemarks(XDocument doc)
    {
        // Handle both namespaced and non-namespaced KML
        var placemarks = doc.Descendants(Kml + "Placemark").ToList();
        if (!placemarks.Any())
            placemarks = doc.Descendants("Placemark").ToList();
        return placemarks;
    }

    private static string GetName(XElement placemark)
    {
        var name = placemark.Element(Kml + "name")?.Value
            ?? placemark.Element("name")?.Value;

        if (string.IsNullOrWhiteSpace(name) || Guid.TryParse(name, out _))
        {
            var desc = placemark.Element(Kml + "description")?.Value
                ?? placemark.Element("description")?.Value;

            if (!string.IsNullOrWhiteSpace(desc))
            {
                var match = System.Text.RegularExpressions.Regex.Match(desc, @"<td>AD</td>\s*<td>([^<]+)</td>", System.Text.RegularExpressions.RegexOptions.IgnoreCase);
                if (match.Success) return match.Groups[1].Value.Trim();
            }
        }

        return string.IsNullOrWhiteSpace(name) ? "Bilinmiyor" : name.Trim();
    }

    private static (double Lat, double Lng)? GetPoint(XElement placemark)
    {
        var coordEl = placemark.Descendants(Kml + "coordinates").FirstOrDefault()
                   ?? placemark.Descendants("coordinates").FirstOrDefault();
        if (coordEl == null) return null;

        var parts = coordEl.Value.Trim().Split(',');
        if (parts.Length < 2) return null;
        if (!double.TryParse(parts[0], System.Globalization.NumberStyles.Any,
                System.Globalization.CultureInfo.InvariantCulture, out double lng)) return null;
        if (!double.TryParse(parts[1], System.Globalization.NumberStyles.Any,
                System.Globalization.CultureInfo.InvariantCulture, out double lat)) return null;
        return (lat, lng);
    }

    /// <summary>Extracts the Polygon coordinates and converts to GeoJSON format.</summary>
    private static string ExtractPolygonGeoJson(XElement placemark)
    {
        var coordEl = placemark.Descendants(Kml + "coordinates").FirstOrDefault()
                   ?? placemark.Descendants("coordinates").FirstOrDefault();
        if (coordEl == null) return string.Empty;

        var rawCoords = coordEl.Value.Trim();
        var pairs = rawCoords
            .Split(new[] { ' ', '\n', '\r', '\t' }, StringSplitOptions.RemoveEmptyEntries)
            .Select(c => c.Trim())
            .Where(c => c.Contains(','))
            .Select(c =>
            {
                var parts = c.Split(',');
                if (parts.Length < 2) return null;
                if (!double.TryParse(parts[0], System.Globalization.NumberStyles.Any,
                        System.Globalization.CultureInfo.InvariantCulture, out double lng2)) return null;
                if (!double.TryParse(parts[1], System.Globalization.NumberStyles.Any,
                        System.Globalization.CultureInfo.InvariantCulture, out double lat2)) return null;
                return (string?)$"[{lng2.ToString(System.Globalization.CultureInfo.InvariantCulture)},{lat2.ToString(System.Globalization.CultureInfo.InvariantCulture)}]";
            })
            .Where(p => p != null)
            .ToList();

        if (!pairs.Any()) return string.Empty;

        var coordArray = string.Join(",", pairs);
        return $"{{\"type\":\"Polygon\",\"coordinates\":[[{coordArray}]]}}";
    }

    private static int? ResolveDistrictId(
        XElement placemark,
        string name,
        Dictionary<string, int> districtLookup)
    {
        // Try ExtendedData → ilce / district field
        var extendedData = placemark.Descendants(Kml + "SimpleData")
            .Concat(placemark.Descendants("SimpleData"));

        foreach (var sd in extendedData)
        {
            var attr = sd.Attribute("name")?.Value?.ToUpperInvariant();
            if (attr is "ILCE" or "DISTRICT" or "ILCEADI")
            {
                var val = sd.Value.ToUpperInvariant();
                if (districtLookup.TryGetValue(val, out int did))
                    return did;

                // Partial match
                var match = districtLookup.Keys.FirstOrDefault(k => val.Contains(k) || k.Contains(val));
                if (match != null) return districtLookup[match];
            }
        }

        // Fallback: check if any district name appears in the neighborhood name
        foreach (var (key, id) in districtLookup)
            if (name.ToUpperInvariant().Contains(key)) return id;

        return null;
    }
}
