using GaziantepFire.Application.DTOs;

namespace GaziantepFire.Application.Interfaces;

/// <summary>
/// Service contract for computing optimal fire station placement suggestions
/// based on geographic incident density and distance analysis.
/// </summary>
public interface IStationOptimizationService
{
    /// <summary>
    /// Returns <paramref name="count"/> optimal station coordinates
    /// computed from incident density clustering and coverage gap analysis.
    /// </summary>
    Task<IEnumerable<StationSuggestionDto>> GetOptimalStationSuggestionsAsync(int count);
}
