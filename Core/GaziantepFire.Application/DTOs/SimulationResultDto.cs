namespace GaziantepFire.Application.DTOs;

public class SimulationResultDto
{
    public int StationId { get; set; }
    public string StationName { get; set; } = string.Empty;
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public double DistanceInKm { get; set; }
    public double EstimatedTimeInMinutes { get; set; }
}
