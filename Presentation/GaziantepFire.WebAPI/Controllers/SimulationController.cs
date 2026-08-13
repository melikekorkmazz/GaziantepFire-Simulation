using GaziantepFire.Application.DTOs;
using GaziantepFire.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace GaziantepFire.WebAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SimulationController : ControllerBase
{
    private readonly ISimulationService _simulationService;

    public SimulationController(ISimulationService simulationService)
    {
        _simulationService = simulationService;
    }

    [HttpPost("calculate-incident")]
    public async Task<IActionResult> CalculateIncident([FromBody] SimulationRequestDto request)
    {
        var results = await _simulationService.CalculateNearestStationsAsync(request, 3);
        return Ok(results);
    }
}
