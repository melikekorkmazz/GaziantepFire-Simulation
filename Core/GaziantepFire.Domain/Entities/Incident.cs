using GaziantepFire.Domain.Enums;

namespace GaziantepFire.Domain.Entities;

public class Incident
{
    public int Id { get; set; }
    public int? ExternalId { get; set; }   // Gaziantep API ihbar ID'si
    public IncidentType IncidentType { get; set; }
    public string SubType { get; set; } = string.Empty;  // altTur (Bina/Ev, Araç vb.)
    public int NeighborhoodId { get; set; }
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public CoordinateSource CoordinateSource { get; set; }
    public DateTime CreatedAt { get; set; }
    public int ResponseTimeInMinutes { get; set; }

    public Neighborhood? Neighborhood { get; set; }
}
