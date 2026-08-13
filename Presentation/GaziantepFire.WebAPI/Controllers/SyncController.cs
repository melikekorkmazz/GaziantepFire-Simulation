using GaziantepFire.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace GaziantepFire.WebAPI.Controllers;

/// <summary>
/// Provides a manual trigger for the incident synchronization pipeline.
/// Intended for development, testing, and on-demand backfills.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class SyncController : ControllerBase
{
    private readonly IIncidentSyncService _syncService;
    private readonly ILogger<SyncController> _logger;

    public SyncController(IIncidentSyncService syncService, ILogger<SyncController> logger)
    {
        _syncService = syncService;
        _logger = logger;
    }

    /// <summary>
    /// Manually triggers an incident sync from the Gaziantep open data API.
    /// Returns the number of new incidents saved.
    /// </summary>
    [HttpPost("incidents")]
    public async Task<IActionResult> TriggerIncidentSync(CancellationToken ct)
    {
        _logger.LogInformation("[SyncController] Manual sync triggered via API");
        var count = await _syncService.SyncTodayAsync(ct);
        return Ok(new
        {
            Message = $"Sync completed. {count} new incident(s) saved.",
            NewIncidentCount = count,
            SyncedAt = DateTime.UtcNow
        });
    }
}
