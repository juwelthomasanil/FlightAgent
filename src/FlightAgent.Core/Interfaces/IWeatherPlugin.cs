namespace FlightAgent.Core.Interfaces;

/// <summary>
/// Provides current weather information for airports via Open-Meteo API.
/// Thin interface for DI registration and Moq testability.
/// </summary>
public interface IWeatherPlugin
{
    /// <summary>
    /// Gets current weather for an airport by its IATA code.
    /// </summary>
    /// <param name="iataCode">The 3-letter IATA airport code (e.g., JFK, LHR, CDG).</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Formatted weather info string, or null if airport not found or API error.</returns>
    Task<string?> GetAirportWeatherAsync(string iataCode, CancellationToken cancellationToken = default);
}
