namespace GaziantepFire.Application.DTOs;

public class NeighborhoodDetailDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string DistrictName { get; set; } = string.Empty;
    public double RiskLevel { get; set; }
    public int FireCount { get; set; }
    public int RescueCount { get; set; }
    public double AverageResponseTimeMinutes { get; set; }
    public string NearestStationName { get; set; } = string.Empty;
    public string PolygonBoundaryGeoJson { get; set; } = string.Empty;
}
