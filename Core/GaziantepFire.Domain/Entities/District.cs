namespace GaziantepFire.Domain.Entities;

public class District
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string PolygonBoundaryGeoJson { get; set; } = string.Empty;

    public ICollection<Neighborhood> Neighborhoods { get; set; } = new List<Neighborhood>();
}
