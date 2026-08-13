namespace GaziantepFire.Application.DTOs;

public class SimulationResponseDto
{
    public IEnumerable<SimulationResultDto> Stations { get; set; } = new List<SimulationResultDto>();
    public List<string> RecommendedVehicles { get; set; } = new List<string>();
}
