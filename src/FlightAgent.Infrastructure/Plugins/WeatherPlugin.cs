using Microsoft.Extensions.Caching.Memory;
using Microsoft.SemanticKernel;
using System.ComponentModel;
using System.Globalization;
using System.Text.Json;
using FlightAgent.Core.Interfaces;
using FlightAgent.Infrastructure.Models;

namespace FlightAgent.Infrastructure.Plugins;

/// <summary>
/// Provides current weather information for airports via Open-Meteo API.
/// Implements IWeatherPlugin for DI/testability and exposes [KernelFunction] for Semantic Kernel.
/// Results are cached per airport with 15-minute absolute expiry.
/// </summary>
public class WeatherPlugin : IWeatherPlugin
{
    private readonly IMemoryCache _cache;
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IAirportPlugin _airportPlugin;
    private static readonly TimeSpan CacheDuration = TimeSpan.FromMinutes(15);

    private static readonly Dictionary<int, string> WmoWeatherCodes = new()
    {
        [0] = "Clear sky",
        [1] = "Mainly clear",
        [2] = "Partly cloudy",
        [3] = "Overcast",
        [45] = "Foggy",
        [48] = "Rime fog",
        [51] = "Light drizzle",
        [53] = "Drizzle",
        [55] = "Heavy drizzle",
        [56] = "Light freezing drizzle",
        [57] = "Freezing drizzle",
        [61] = "Light rain",
        [63] = "Rain",
        [65] = "Heavy rain",
        [66] = "Light freezing rain",
        [67] = "Freezing rain",
        [71] = "Light snow",
        [73] = "Snow",
        [75] = "Heavy snow",
        [77] = "Snow grains",
        [80] = "Light rain showers",
        [81] = "Rain showers",
        [82] = "Heavy rain showers",
        [85] = "Light snow showers",
        [86] = "Snow showers",
        [95] = "Thunderstorm",
        [96] = "Thunderstorm with light hail",
        [99] = "Thunderstorm with hail"
    };

    public WeatherPlugin(
        IMemoryCache cache,
        IHttpClientFactory httpClientFactory,
        IAirportPlugin airportPlugin)
    {
        _cache = cache;
        _httpClientFactory = httpClientFactory;
        _airportPlugin = airportPlugin;
    }

    /// <inheritdoc />
    [KernelFunction("get_airport_weather")]
    [Description("Gets current weather for an airport by its IATA code. Returns temperature, conditions, and wind. Returns null if airport not found.")]
    public async Task<string?> GetAirportWeatherAsync(
        [Description("The 3-letter IATA airport code (e.g., JFK, LHR, CDG)")] string iataCode,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(iataCode))
        {
            return null;
        }

        var cacheKey = $"weather:{iataCode.ToUpperInvariant()}";

        // Check cache per D-17
        if (_cache.TryGetValue(cacheKey, out string? cachedWeather))
        {
            return cachedWeather;
        }

        // Get airport coordinates via AirportPlugin per D-14
        var airport = await _airportPlugin.GetAirportInfoByIataCodeAsync(iataCode, cancellationToken);
        if (airport == null)
        {
            return null; // D-19: unknown airport code returns null
        }

        // Fetch weather from Open-Meteo API
        var weather = await FetchWeatherAsync(airport, cancellationToken);

        if (weather == null)
        {
            return null;
        }

        // Cache the result per D-16, D-17
        var cacheOptions = new MemoryCacheEntryOptions()
            .SetAbsoluteExpiration(CacheDuration);
        _cache.Set(cacheKey, weather, cacheOptions);

        return weather;
    }

    private async Task<string?> FetchWeatherAsync(Core.Models.AirportInfo airport, CancellationToken ct)
    {
        // Build Open-Meteo URL per D-11 (using InvariantCulture for lat/lon formatting)
        var lat = airport.Lat.ToString("G", CultureInfo.InvariantCulture);
        var lon = airport.Lon.ToString("G", CultureInfo.InvariantCulture);
        var url = $"https://api.open-meteo.com/v1/forecast" +
                  $"?latitude={lat}&longitude={lon}" +
                  $"&current=temperature_2m,weathercode,windspeed_10m,winddirection_10m";

        // Per D-21: let exceptions bubble — Polly handles resilience at registration level
        using var cts = CancellationTokenSource.CreateLinkedTokenSource(ct);
        cts.CancelAfter(TimeSpan.FromSeconds(8));
        using var httpClient = _httpClientFactory.CreateClient();
        var response = await httpClient.GetAsync(url, cts.Token);
        response.EnsureSuccessStatusCode();

        var json = await response.Content.ReadAsStringAsync(ct);
        var data = JsonSerializer.Deserialize<WeatherData>(json);

        if (data?.Current == null)
        {
            return null;
        }

        // Map WMO code to human-readable condition
        var conditions = WmoWeatherCodes.TryGetValue(data.Current.WeatherCode, out var desc)
            ? desc
            : "Unknown conditions";

        // Format human-readable output
        var result = $"Current weather at {airport.Name} ({airport.IataCode}): " +
                     $"{data.Current.Temperature:F1}°C, {conditions}. " +
                     $"Wind: {data.Current.WindSpeed:F1} km/h from {data.Current.WindDirection}°. " +
                     $"Data by Open-Meteo.com";

        return result;
    }
}
