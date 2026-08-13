namespace GaziantepFire.Application.Interfaces;

/// <summary>
/// Service contract for fetching fire incident data from the
/// Gaziantep Municipality Open Data API and persisting new records.
/// </summary>
public interface IIncidentSyncService
{
    /// <summary>
    /// Fetches today's incidents from the external API,
    /// deduplicates by ExternalId and saves new ones to the database.
    /// </summary>
    Task<int> SyncTodayAsync(CancellationToken cancellationToken = default);
}
