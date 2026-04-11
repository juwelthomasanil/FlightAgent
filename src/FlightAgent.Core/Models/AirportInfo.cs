namespace FlightAgent.Core.Models;

/// <summary>
/// Represents airport information with location data.
/// Used by AirportPlugin for lookups and WeatherPlugin for coordinate-based API calls.
/// </summary>
/// <param name="IataCode">3-letter IATA code (e.g., JFK, LHR).</param>
/// <param name="Name">Full airport name.</param>
/// <param name="City">City where the airport is located.</param>
/// <param name="Country">Country where the airport is located.</param>
/// <param name="Lat">Latitude coordinate (mandatory for WeatherPlugin API calls).</param>
/// <param name="Lon">Longitude coordinate (mandatory for WeatherPlugin API calls).</param>
/// <param name="Timezone">IANA timezone identifier (e.g., America/New_York).</param>
public record AirportInfo(
    string IataCode,
    string Name,
    string City,
    string Country,
    double Lat,
    double Lon,
    string Timezone);
