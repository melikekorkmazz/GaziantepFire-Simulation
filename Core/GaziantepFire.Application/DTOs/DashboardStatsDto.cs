namespace GaziantepFire.Application.DTOs;

public class DashboardStatsDto
{
    public int TotalFires { get; set; }
    public int TotalRescues { get; set; }
    public double AverageResponseTimeMinutes { get; set; }
    public string MostRiskyNeighborhoodName { get; set; } = string.Empty;
    public double MostRiskyNeighborhoodScore { get; set; }
    public string BusiestDistrictName { get; set; } = string.Empty;
    public int BusiestDistrictIncidentCount { get; set; }
}
