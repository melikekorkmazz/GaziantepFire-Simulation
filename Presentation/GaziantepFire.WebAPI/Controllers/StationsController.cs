using GaziantepFire.Application.DTOs;
using GaziantepFire.Application.Interfaces;
using GaziantepFire.Persistence.Context;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GaziantepFire.WebAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class StationsController : ControllerBase
{
    private readonly GaziantepFireDbContext _context;
    private readonly IStationOptimizationService _optimizationService;

    public StationsController(GaziantepFireDbContext context, IStationOptimizationService optimizationService)
    {
        _context = context;
        _optimizationService = optimizationService;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<object>>> GetAll()
    {
        var stations = await _context.FireStations
            .Select(s => new
            {
                s.Id,
                s.Name,
                s.Latitude,
                s.Longitude,
                s.Capacity
            })
            .ToListAsync();

        return Ok(stations);
    }

    /// <summary>
    /// Returns optimal coordinates for <paramref name="count"/> new fire stations
    /// based on geographic incident density and coverage gap analysis.
    /// </summary>
    [HttpGet("suggestions")]
    public async Task<ActionResult<IEnumerable<StationSuggestionDto>>> GetSuggestions([FromQuery] int count = 3)
    {
        if (count < 1 || count > 10)
        {
            return BadRequest(new { Message = "count must be between 1 and 10." });
        }

        var suggestions = await _optimizationService.GetOptimalStationSuggestionsAsync(count);
        return Ok(suggestions);
    }
}
