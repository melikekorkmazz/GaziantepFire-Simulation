using GaziantepFire.Application.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace GaziantepFire.Infrastructure.BackgroundServices;

/// <summary>
/// .NET BackgroundService that triggers the <see cref="IIncidentSyncService"/>
/// once per day at 02:00 local time to pull new incidents from the
/// Gaziantep Municipality open data API.
/// </summary>
public class DailyIncidentSyncHostedService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<DailyIncidentSyncHostedService> _logger;

    // Target hour (24h format, local time) at which the nightly sync fires.
    private const int SyncHour = 2;

    public DailyIncidentSyncHostedService(
        IServiceScopeFactory scopeFactory,
        ILogger<DailyIncidentSyncHostedService> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("[DailySyncService] Background service started. Will sync at {Hour:D2}:00 every day.", SyncHour);

        while (!stoppingToken.IsCancellationRequested)
        {
            var now = DateTime.Now;
            var nextRun = now.Date.AddHours(SyncHour);

            // If today's sync time has already passed, schedule for tomorrow
            if (now >= nextRun)
                nextRun = nextRun.AddDays(1);

            var delay = nextRun - now;
            _logger.LogInformation("[DailySyncService] Next sync scheduled at {NextRun:yyyy-MM-dd HH:mm} (in {Hours:F1} hours)",
                nextRun, delay.TotalHours);

            try
            {
                await Task.Delay(delay, stoppingToken);
            }
            catch (OperationCanceledException)
            {
                break;
            }

            if (stoppingToken.IsCancellationRequested) break;

            await RunSyncAsync(stoppingToken);
        }

        _logger.LogInformation("[DailySyncService] Background service stopped.");
    }

    private async Task RunSyncAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("[DailySyncService] Starting nightly sync at {Time}", DateTime.Now);

        try
        {
            // IIncidentSyncService is scoped — must resolve within a scope
            using var scope = _scopeFactory.CreateScope();
            var syncService = scope.ServiceProvider.GetRequiredService<IIncidentSyncService>();
            var count = await syncService.SyncTodayAsync(cancellationToken);
            _logger.LogInformation("[DailySyncService] Nightly sync complete. Saved {Count} new incidents.", count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[DailySyncService] Nightly sync failed.");
        }
    }
}
