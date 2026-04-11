using System.Text.Json.Serialization;

namespace FlightAgent.Infrastructure.Models;

/// <summary>
/// Represents the Open-Meteo API response for current weather data.
/// Maps snake_case JSON fields to PascalCase properties.
/// </summary>
public class WeatherData
{
    [JsonPropertyName("current")]
    public CurrentWeather Current { get; set; } = new();
}

/// <summary>
/// Current weather conditions from Open-Meteo API.
/// </summary>
public class CurrentWeather
{
    /// <summary>Temperature at 2m height in Celsius.</summary>
    [JsonPropertyName("temperature_2m")]
    public double Temperature { get; set; }

    /// <summary>WMO weather code (0-99) indicating conditions.</summary>
    [JsonPropertyName("weathercode")]
    public int WeatherCode { get; set; }

    /// <summary>Wind speed at 10m height in km/h.</summary>
    [JsonPropertyName("windspeed_10m")]
    public double WindSpeed { get; set; }

    /// <summary>Wind direction at 10m height in degrees (0-360).</summary>
    [JsonPropertyName("winddirection_10m")]
    public int WindDirection { get; set; }
}
