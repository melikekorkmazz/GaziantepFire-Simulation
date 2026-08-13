using GaziantepFire.Domain.Entities;
using GaziantepFire.Domain.Enums;
using GaziantepFire.Persistence.Context;
using Microsoft.EntityFrameworkCore;

namespace GaziantepFire.Persistence.Seed;

/// <summary>
/// Seeds base static data on first startup.
/// District/Neighborhood/Station data is injected from outside (WebAPI startup)
/// after KML parsing — this class only saves them and adds bootstrap incidents.
/// </summary>
public static class GaziantepFireSeedData
{
    /// <summary>
    /// Called from WebAPI Program.cs with already-parsed entity lists.
    /// Falls back to built-in mock data when lists are empty.
    /// </summary>
    public static async Task SeedAsync(
        GaziantepFireDbContext context,
        IReadOnlyList<District> districts,
        IReadOnlyList<Neighborhood> neighborhoods,
        IReadOnlyList<FireStation> stations)
    {
        if (!await context.Districts.AnyAsync())
        {
            var seedDistricts = districts.Any() ? districts : GetMockDistricts();
            await context.Districts.AddRangeAsync(seedDistricts);
            await context.SaveChangesAsync();
        }

        if (!await context.Neighborhoods.AnyAsync())
        {
            var seedNeighborhoods = neighborhoods.Any()
                ? neighborhoods
                : GetMockNeighborhoods(await context.Districts.ToListAsync());
            await context.Neighborhoods.AddRangeAsync(seedNeighborhoods);
            await context.SaveChangesAsync();
        }

        if (!await context.FireStations.AnyAsync())
        {
            var seedStations = stations.Any() ? stations : GetMockStations();
            // Clear explicit Ids to let DB handle auto-increment if needed
            foreach (var s in seedStations)
            {
                s.Id = 0;
            }
            await context.FireStations.AddRangeAsync(seedStations);
            await context.SaveChangesAsync();
        }
    }

    // ── Mock fallbacks ─────────────────────────────────────────────────────

    private static List<District> GetMockDistricts() => new()
    {
        new District { Id = 1, Name = "Şahinbey",   PolygonBoundaryGeoJson = "{\"type\":\"Polygon\",\"coordinates\":[[[37.30,37.00],[37.38,37.00],[37.38,37.06],[37.30,37.06],[37.30,37.00]]]}" },
        new District { Id = 2, Name = "Şehitkamil", PolygonBoundaryGeoJson = "{\"type\":\"Polygon\",\"coordinates\":[[[37.32,37.07],[37.45,37.07],[37.45,37.15],[37.32,37.15],[37.32,37.07]]]}" },
        new District { Id = 3, Name = "Oğuzeli",    PolygonBoundaryGeoJson = "{\"type\":\"Polygon\",\"coordinates\":[[[37.45,36.90],[37.55,36.90],[37.55,37.00],[37.45,37.00],[37.45,36.90]]]}" },
    };

    private static List<Neighborhood> GetMockNeighborhoods(List<District> districts)
    {
        var s = districts.FirstOrDefault(d => d.Name.Contains("Şahinbey"))?.Id   ?? districts[0].Id;
        var k = districts.FirstOrDefault(d => d.Name.Contains("Şehitkamil"))?.Id ?? (districts.Count > 1 ? districts[1].Id : districts[0].Id);
        return new()
        {
            new Neighborhood { Id = 1, DistrictId = s, Name = "Karataş Mah.",    PolygonBoundaryGeoJson = "{\"type\":\"Polygon\",\"coordinates\":[[[37.33,37.02],[37.36,37.02],[37.36,37.04],[37.33,37.04],[37.33,37.02]]]}", RiskLevel = 8.9 },
            new Neighborhood { Id = 2, DistrictId = s, Name = "Yeditepe Mah.",   PolygonBoundaryGeoJson = "{\"type\":\"Polygon\",\"coordinates\":[[[37.34,37.04],[37.37,37.04],[37.37,37.06],[37.34,37.06],[37.34,37.04]]]}", RiskLevel = 7.2 },
            new Neighborhood { Id = 3, DistrictId = k, Name = "İbrahimli Mah.",  PolygonBoundaryGeoJson = "{\"type\":\"Polygon\",\"coordinates\":[[[37.33,37.08],[37.36,37.08],[37.36,37.11],[37.33,37.11],[37.33,37.08]]]}", RiskLevel = 5.4 },
            new Neighborhood { Id = 4, DistrictId = k, Name = "Güvenevler Mah.", PolygonBoundaryGeoJson = "{\"type\":\"Polygon\",\"coordinates\":[[[37.36,37.07],[37.39,37.07],[37.39,37.09],[37.36,37.09],[37.36,37.07]]]}", RiskLevel = 6.8 },
            new Neighborhood { Id = 5, DistrictId = k, Name = "Gazikent Mah.",   PolygonBoundaryGeoJson = "{\"type\":\"Polygon\",\"coordinates\":[[[37.40,37.09],[37.44,37.09],[37.44,37.12],[37.40,37.12],[37.40,37.09]]]}", RiskLevel = 8.1 },
        };
    }

    private static List<FireStation> GetMockStations() => new()
    {
        new FireStation { Id = 1, Name = "Şahinbey Merkez İtfaiye İstasyonu", Latitude = 37.0425, Longitude = 37.3512, Capacity = 12 },
        new FireStation { Id = 2, Name = "Şehitkamil İtfaiye İstasyonu",      Latitude = 37.0850, Longitude = 37.3780, Capacity = 10 },
        new FireStation { Id = 3, Name = "Organize Sanayi İtfaiye İstasyonu", Latitude = 37.1230, Longitude = 37.4410, Capacity = 15 },
    };
}
