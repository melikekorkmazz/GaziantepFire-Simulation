namespace GaziantepFire.Domain.Entities;

public class Neighborhood
{
    public int Id { get; set; }
    public int DistrictId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string PolygonBoundaryGeoJson { get; set; } = string.Empty;
    public double RiskLevel { get; set; }

    public District? District { get; set; }
    public ICollection<Incident> Incidents { get; set; } = new List<Incident>();
}
