using GaziantepFire.Application.DTOs;

namespace GaziantepFire.Application.Interfaces;

public interface ISimulationService
{
    Task<SimulationResponseDto> CalculateNearestStationsAsync(SimulationRequestDto request, int count = 3);
}
