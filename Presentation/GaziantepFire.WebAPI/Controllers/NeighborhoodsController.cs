using GaziantepFire.Application.DTOs;
using GaziantepFire.Domain.Entities;
using GaziantepFire.Domain.Enums;
using GaziantepFire.Persistence.Context;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GaziantepFire.WebAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class NeighborhoodsController : ControllerBase
{
    private readonly GaziantepFireDbContext _context;

    public NeighborhoodsController(GaziantepFireDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<NeighborhoodMapItemDto>>> GetAll()
    {
        var items = await _context.Neighborhoods
            .Include(n => n.District)
            .Include(n => n.Incidents)
            .Select(n => new NeighborhoodMapItemDto
            {
                Id = n.Id,
                Name = n.Name,
                DistrictId = n.DistrictId,
                DistrictName = n.District != null ? n.District.Name : string.Empty,
                PolygonBoundaryGeoJson = n.PolygonBoundaryGeoJson,
                RiskLevel = n.RiskLevel,
                FireCount = n.Incidents.Count(i => i.IncidentType == IncidentType.Fire),
                RescueCount = n.Incidents.Count(i => i.IncidentType == IncidentType.Rescue)
            })
            .ToListAsync();

        return Ok(items);
    }

    [HttpGet("{id}/details")]
    public async Task<ActionResult<NeighborhoodDetailDto>> GetDetails(int id)
    {
        var neighborhood = await _context.Neighborhoods
            .Include(n => n.District)
            .Include(n => n.Incidents)
            .FirstOrDefaultAsync(n => n.Id == id);

        if (neighborhood == null)
        {
            return NotFound(new { Message = $"Neighborhood with Id {id} not found." });
        }

        var fireCount = neighborhood.Incidents.Count(i => i.IncidentType == IncidentType.Fire);
        var rescueCount = neighborhood.Incidents.Count(i => i.IncidentType == IncidentType.Rescue);

        var avgResponseTime = neighborhood.Incidents.Any()
            ? Math.Round(neighborhood.Incidents.Average(i => i.ResponseTimeInMinutes), 1)
            : 6.0;

        // Find nearest fire station
        var allStations = await _context.FireStations.ToListAsync();
        string nearestStationName = "Şahinbey Merkez İtfaiye İstasyonu";

        if (allStations.Any())
        {
            double nLat = neighborhood.Incidents.FirstOrDefault()?.Latitude ?? 37.0425;
            double nLng = neighborhood.Incidents.FirstOrDefault()?.Longitude ?? 37.3512;

            var nearest = allStations
                .OrderBy(s => Math.Pow(s.Latitude - nLat, 2) + Math.Pow(s.Longitude - nLng, 2))
                .FirstOrDefault();

            if (nearest != null)
            {
                nearestStationName = nearest.Name;
            }
        }

        var details = new NeighborhoodDetailDto
        {
            Id = neighborhood.Id,
            Name = neighborhood.Name,
            DistrictName = neighborhood.District?.Name ?? "Gaziantep",
            RiskLevel = neighborhood.RiskLevel,
            FireCount = fireCount,
            RescueCount = rescueCount,
            AverageResponseTimeMinutes = avgResponseTime,
            NearestStationName = nearestStationName,
            PolygonBoundaryGeoJson = neighborhood.PolygonBoundaryGeoJson
        };

        return Ok(details);
    }
}
