namespace GaziantepFire.Application.DTOs;

public class StationSuggestionDto
{
    public int Index { get; set; }
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public string Reason { get; set; } = string.Empty;
    public double CurrentAvgResponseTime { get; set; }
    public double EstimatedNewResponseTime { get; set; }
}
