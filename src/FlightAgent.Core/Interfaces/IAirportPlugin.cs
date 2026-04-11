using FlightAgent.Core.Models;

namespace FlightAgent.Core.Interfaces;

/// <summary>
/// Provides airport information lookup by IATA code.
/// Thin interface for DI registration and Moq testability.
/// </summary>
public interface IAirportPlugin
{
    /// <summary>
    /// Gets formatted airport information by IATA code.
    /// </summary>
    /// <param name="iataCode">The 3-letter IATA airport code (e.g., JFK, LHR, CDG).</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Formatted airport info string, or null if not found.</returns>
    Task<string?> GetAirportInfoAsync(string iataCode, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets raw AirportInfo record by IATA code (used by WeatherPlugin for coordinates).
    /// </summary>
    /// <param name="iataCode">The 3-letter IATA airport code.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>AirportInfo record, or null if not found.</returns>
    Task<AirportInfo?> GetAirportInfoByIataCodeAsync(string iataCode, CancellationToken cancellationToken = default);
}
