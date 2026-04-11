using FluentAssertions;
using Xunit;
using FlightAgent.Infrastructure.Plugins;

namespace FlightAgent.Infrastructure.Tests.Plugins;

/// <summary>
/// Tests for AirportPlugin — validates airport lookup, case insensitivity, and null returns.
/// </summary>
public class AirportPluginTests
{
    private readonly AirportPlugin _sut = new();

    [Fact]
    public async Task GetAirportInfoAsync_ReturnsInfo_ForValidIataCode()
    {
        // Arrange
        var iataCode = "JFK";

        // Act
        var result = await _sut.GetAirportInfoAsync(iataCode);

        // Assert
        result.Should().NotBeNull();
        result.Should().Contain("John F. Kennedy");
        result.Should().Contain("JFK");
        result.Should().Contain("New York");
    }

    [Fact]
    public async Task GetAirportInfoAsync_ReturnsNull_ForInvalidIataCode()
    {
        // Act
        var result = await _sut.GetAirportInfoAsync("XXX");

        // Assert
        result.Should().BeNull();
    }

    [Theory]
    [InlineData("jfk")]
    [InlineData("JFK")]
    [InlineData("Jfk")]
    public async Task GetAirportInfoAsync_IsCaseInsensitive(string iataCode)
    {
        // Act
        var result = await _sut.GetAirportInfoAsync(iataCode);

        // Assert
        result.Should().NotBeNull();
        result.Should().Contain("John F. Kennedy");
    }

    [Fact]
    public async Task GetAirportInfoByIataCodeAsync_ReturnsAirportInfo_ForValidCode()
    {
        // Act
        var result = await _sut.GetAirportInfoByIataCodeAsync("LHR");

        // Assert
        result.Should().NotBeNull();
        result!.IataCode.Should().Be("LHR");
        result.Name.Should().Contain("Heathrow");
        result.City.Should().Be("London");
        result.Country.Should().Be("UK");
        result.Lat.Should().BeApproximately(51.47, 0.01);
        result.Lon.Should().BeApproximately(-0.45, 0.01);
        result.Timezone.Should().Be("Europe/London");
    }

    [Fact]
    public async Task GetAirportInfoByIataCodeAsync_ReturnsNull_ForInvalidCode()
    {
        // Act
        var result = await _sut.GetAirportInfoByIataCodeAsync("ZZZ");

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public void AirportData_Contains50Airports()
    {
        // Arrange
        var airports = FlightAgent.Infrastructure.Data.AirportData.LoadAirports();

        // Assert
        airports.Should().HaveCount(50);
    }
}
