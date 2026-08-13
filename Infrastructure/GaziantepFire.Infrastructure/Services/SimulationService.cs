using GaziantepFire.Application.DTOs;
using GaziantepFire.Application.Interfaces;
using GaziantepFire.Persistence.Context;
using Microsoft.EntityFrameworkCore;

namespace GaziantepFire.Infrastructure.Services;

public class SimulationService : ISimulationService
{
    private readonly GaziantepFireDbContext _context;

    // Ortalama şehir içi itfaiye hızı: 45 km/s
    private const double AverageSpeedKmH = 45.0;

    public SimulationService(GaziantepFireDbContext context)
    {
        _context = context;
    }

    public async Task<SimulationResponseDto> CalculateNearestStationsAsync(SimulationRequestDto request, int count = 3)
    {
        var stations = await _context.FireStations.ToListAsync();

        var stationResults = stations.Select(station =>
        {
            var distance = CalculateHaversineDistance(request.Latitude, request.Longitude, station.Latitude, station.Longitude);
            var estimatedTimeHours = distance / AverageSpeedKmH;
            var estimatedTimeMinutes = estimatedTimeHours * 60;

            return new SimulationResultDto
            {
                StationId = station.Id,
                StationName = station.Name,
                Latitude = station.Latitude,
                Longitude = station.Longitude,
                DistanceInKm = Math.Round(distance, 2),
                EstimatedTimeInMinutes = Math.Round(estimatedTimeMinutes, 1)
            };
        })
        .OrderBy(r => r.DistanceInKm)
        .Take(count)
        .ToList();

        // Rastgele araç önerisi mantığı (gerçek bir algoritmada olay türüne göre değişir, şimdilik mock)
        var recommendedVehicles = new List<string> { "🚒 Su Tankeri", "🚒 İlk Müdahale Aracı" };
        if (new Random().Next(100) > 50) recommendedVehicles.Add("🚑 Ambulans Desteği");
        if (new Random().Next(100) > 70) recommendedVehicles.Add("🚒 Merdivenli Araç");

        return new SimulationResponseDto
        {
            Stations = stationResults,
            RecommendedVehicles = recommendedVehicles
        };
    }

    private static double CalculateHaversineDistance(double lat1, double lon1, double lat2, double lon2)
    {
        var r = 6371; // Earth radius in km
        var dLat = ToRadians(lat2 - lat1);
        var dLon = ToRadians(lon2 - lon1);
        var a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                Math.Cos(ToRadians(lat1)) * Math.Cos(ToRadians(lat2)) *
                Math.Sin(dLon / 2) * Math.Sin(dLon / 2);
        var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
        return r * c;
    }

    private static double ToRadians(double angle)
    {
        return Math.PI * angle / 180.0;
    }
}
