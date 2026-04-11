using Microsoft.SemanticKernel;
using System.ComponentModel;
using FlightAgent.Core.Interfaces;
using FlightAgent.Core.Models;

namespace FlightAgent.Infrastructure.Plugins;

/// <summary>
/// Provides airport information lookup using a hardcoded dataset of 50 major global airports.
/// Implements IAirportPlugin for DI/testability and exposes [KernelFunction] for Semantic Kernel.
/// </summary>
public class AirportPlugin : IAirportPlugin
{
    private readonly Dictionary<string, AirportInfo> _airports;

    public AirportPlugin()
    {
        _airports = Data.AirportData.LoadAirports();
    }

    /// <inheritdoc />
    [KernelFunction("get_airport_info")]
    [Description("Gets information about an airport by its IATA code (e.g., JFK, LHR, NRT). Returns null if not found.")]
    public Task<string?> GetAirportInfoAsync(
        [Description("The 3-letter IATA airport code (e.g., JFK, LHR, CDG)")] string iataCode,
        CancellationToken cancellationToken = default)
    {
        if (_airports.TryGetValue(iataCode, out var airport))
        {
            var result = $"{airport.Name} ({airport.IataCode}) - {airport.City}, {airport.Country}. " +
                        $"Coordinates: {airport.Lat}, {airport.Lon}. Timezone: {airport.Timezone}";
            return Task.FromResult<string?>(result);
        }

        return Task.FromResult<string?>(null);
    }

    /// <inheritdoc />
    public Task<AirportInfo?> GetAirportInfoByIataCodeAsync(
        string iataCode,
        CancellationToken cancellationToken = default)
    {
        if (_airports.TryGetValue(iataCode, out var airport))
        {
            return Task.FromResult<AirportInfo?>(airport);
        }

        return Task.FromResult<AirportInfo?>(null);
    }
}
