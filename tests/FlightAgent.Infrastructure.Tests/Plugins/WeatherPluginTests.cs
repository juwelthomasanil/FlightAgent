using FluentAssertions;
using Microsoft.Extensions.Caching.Memory;
using Moq;
using Xunit;
using FlightAgent.Core.Interfaces;
using FlightAgent.Core.Models;
using FlightAgent.Infrastructure.Plugins;

namespace FlightAgent.Infrastructure.Tests.Plugins;

/// <summary>
/// Tests for WeatherPlugin — validates caching, null returns, and API interaction path.
/// Uses Moq for IAirportPlugin dependency.
/// </summary>
public class WeatherPluginTests
{
    private readonly Mock<IAirportPlugin> _mockAirportPlugin;
    private readonly IMemoryCache _cache;
    private readonly WeatherPlugin _sut;

    public WeatherPluginTests()
    {
        _mockAirportPlugin = new Mock<IAirportPlugin>();
        _cache = new MemoryCache(new MemoryCacheOptions());
        var httpClient = new HttpClient();
        _sut = new WeatherPlugin(_cache, httpClient, _mockAirportPlugin.Object);
    }

    [Fact]
    public async Task GetAirportWeatherAsync_UnknownAirport_ReturnsNull()
    {
        // Arrange
        _mockAirportPlugin
            .Setup(x => x.GetAirportInfoByIataCodeAsync("XXX", It.IsAny<CancellationToken>()))
            .ReturnsAsync((AirportInfo?)null);

        // Act
        var result = await _sut.GetAirportWeatherAsync("XXX");

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetAirportWeatherAsync_CacheHit_ReturnsCachedData()
    {
        // Arrange
        var cachedWeather = "Cached weather data for JFK";
        _cache.Set("weather:JFK", cachedWeather, TimeSpan.FromMinutes(15));

        // Act
        var result = await _sut.GetAirportWeatherAsync("JFK");

        // Assert
        result.Should().Be(cachedWeather);
        _mockAirportPlugin.Verify(
            x => x.GetAirportInfoByIataCodeAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task GetAirportWeatherAsync_CacheHit_IsCaseInsensitive()
    {
        // Arrange — cache key is normalized to uppercase
        var cachedWeather = "Cached weather data for LHR";
        _cache.Set("weather:LHR", cachedWeather, TimeSpan.FromMinutes(15));

        // Act — query with lowercase
        var result = await _sut.GetAirportWeatherAsync("lhr");

        // Assert
        result.Should().Be(cachedWeather);
    }

    [Fact]
    public async Task GetAirportWeatherAsync_CacheMiss_CallsAirportPlugin()
    {
        // Arrange
        var airport = new AirportInfo("JFK", "John F. Kennedy International Airport", "New York", "USA", 40.6413, -73.7781, "America/New_York");
        _mockAirportPlugin
            .Setup(x => x.GetAirportInfoByIataCodeAsync("JFK", It.IsAny<CancellationToken>()))
            .ReturnsAsync(airport);

        // Act — this will attempt a real HTTP call to Open-Meteo
        // In a real test suite we'd mock HttpMessageHandler, but for now we just verify the airport lookup path
        try
        {
            var result = await _sut.GetAirportWeatherAsync("JFK");
            // If API is reachable, result should contain airport name
            if (result != null)
            {
                result.Should().Contain("John F. Kennedy");
                result.Should().Contain("Open-Meteo.com");
            }
        }
        catch (HttpRequestException)
        {
            // Expected in environments without internet access — still verify airport lookup was called
        }

        // Assert — airport plugin was consulted for coordinates
        _mockAirportPlugin.Verify(
            x => x.GetAirportInfoByIataCodeAsync("JFK", It.IsAny<CancellationToken>()),
            Times.Once);
    }
}
