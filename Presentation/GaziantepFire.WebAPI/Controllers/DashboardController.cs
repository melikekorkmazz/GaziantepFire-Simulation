using GaziantepFire.Application.DTOs;
using GaziantepFire.Domain.Enums;
using GaziantepFire.Persistence.Context;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GaziantepFire.WebAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class DashboardController : ControllerBase
{
    private readonly GaziantepFireDbContext _context;

    public DashboardController(GaziantepFireDbContext context)
    {
        _context = context;
    }

    [HttpGet("stats")]
    public async Task<ActionResult<DashboardStatsDto>> GetStats()
    {
        var totalFires = await _context.Incidents.CountAsync(i => i.IncidentType == IncidentType.Fire);
        var totalRescues = await _context.Incidents.CountAsync(i => i.IncidentType == IncidentType.Rescue);
        
        var avgResponseTime = await _context.Incidents.AnyAsync()
            ? Math.Round(await _context.Incidents.AverageAsync(i => i.ResponseTimeInMinutes), 1)
            : 0.0;

        var mostRisky = await _context.Neighborhoods
            .OrderByDescending(n => n.RiskLevel)
            .FirstOrDefaultAsync();

        var busiestDistrictGroup = await _context.Incidents
            .Include(i => i.Neighborhood)
            .ThenInclude(n => n!.District)
            .Where(i => i.Neighborhood != null && i.Neighborhood.District != null)
            .GroupBy(i => i.Neighborhood!.District!.Name)
            .Select(g => new { DistrictName = g.Key, Count = g.Count() })
            .OrderByDescending(x => x.Count)
            .FirstOrDefaultAsync();

        var stats = new DashboardStatsDto
        {
            TotalFires = totalFires,
            TotalRescues = totalRescues,
            AverageResponseTimeMinutes = avgResponseTime,
            MostRiskyNeighborhoodName = mostRisky?.Name ?? "Karataş Mah.",
            MostRiskyNeighborhoodScore = mostRisky?.RiskLevel ?? 8.9,
            BusiestDistrictName = busiestDistrictGroup?.DistrictName ?? "Şahinbey",
            BusiestDistrictIncidentCount = busiestDistrictGroup?.Count ?? (totalFires + totalRescues)
        };

        return Ok(stats);
    }
}
