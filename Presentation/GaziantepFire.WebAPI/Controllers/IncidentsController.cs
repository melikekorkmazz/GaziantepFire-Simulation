using GaziantepFire.Domain.Entities;
using GaziantepFire.Domain.Enums;
using GaziantepFire.Persistence.Context;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GaziantepFire.WebAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class IncidentsController : ControllerBase
{
    private readonly GaziantepFireDbContext _context;

    public IncidentsController(GaziantepFireDbContext context)
    {
        _context = context;
    }

    [HttpGet("fires")]
    public async Task<IActionResult> GetFires([FromQuery] DateTime? startDate, [FromQuery] DateTime? endDate)
    {
        var query = _context.Incidents
            .Include(i => i.Neighborhood)
            .ThenInclude(n => n.District)
            .Where(i => i.IncidentType == IncidentType.Fire);

        if (startDate.HasValue)
        {
            query = query.Where(i => i.CreatedAt >= startDate.Value);
        }

        if (endDate.HasValue)
        {
            query = query.Where(i => i.CreatedAt <= endDate.Value);
        }

        var firesList = await query
            .OrderByDescending(i => i.CreatedAt)
            .ToListAsync();
            
        Console.WriteLine($"[DEBUG] Database fire records matched query: {firesList.Count}");

        var fires = firesList
            .Select(i => new
            {
                i.Id,
                i.ExternalId,
                i.SubType,
                NeighborhoodName = i.Neighborhood != null ? i.Neighborhood.Name : string.Empty,
                DistrictName = i.Neighborhood != null && i.Neighborhood.District != null ? i.Neighborhood.District.Name : string.Empty,
                i.Latitude,
                i.Longitude,
                i.CreatedAt,
                CoordinateSource = i.CoordinateSource.ToString()
            })
            .ToList();

        return Ok(fires);
    }

    [HttpPost("sync")]
    public async Task<IActionResult> SyncIncidents([FromServices] GaziantepFire.Application.Interfaces.IIncidentSyncService syncService)
    {
        var count = await syncService.SyncTodayAsync();
        return Ok(new { inserted = count });
    }
}
