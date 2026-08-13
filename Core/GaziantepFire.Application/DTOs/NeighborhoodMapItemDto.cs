namespace GaziantepFire.Application.DTOs;

public class NeighborhoodMapItemDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public int DistrictId { get; set; }
    public string DistrictName { get; set; } = string.Empty;
    public string PolygonBoundaryGeoJson { get; set; } = string.Empty;
    public double RiskLevel { get; set; }
    public int FireCount { get; set; }
    public int RescueCount { get; set; }
}
