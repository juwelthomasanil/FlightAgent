using FluentAssertions;
using Xunit;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.SemanticKernel;
using FlightAgent.Core.Interfaces;
using FlightAgent.Infrastructure;

namespace FlightAgent.Infrastructure.Tests;

public class DependencyInjectionTests
{
    [Fact]
    public void AddFlightAgentPlugins_RegistersPluginsAndDependencies()
    {
        // Arrange
        var services = new ServiceCollection();
        
        // Act
        services.AddFlightAgentPlugins();
        services.AddHttpClient(); // Required by WeatherPlugin
        var serviceProvider = services.BuildServiceProvider();

        // Assert
        serviceProvider.GetService<IAirportPlugin>().Should().NotBeNull();
        serviceProvider.GetService<IWeatherPlugin>().Should().NotBeNull();
    }

    [Fact]
    public void Plugins_CanBeAddedToSemanticKernel()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddFlightAgentPlugins();
        services.AddHttpClient();
        var serviceProvider = services.BuildServiceProvider();

        var kernelBuilder = Kernel.CreateBuilder();
        kernelBuilder.Services.AddSingleton(serviceProvider.GetRequiredService<IAirportPlugin>());
        kernelBuilder.Services.AddSingleton(serviceProvider.GetRequiredService<IWeatherPlugin>());
        var kernel = kernelBuilder.Build();

        // Act
        var airportPluginInstance = serviceProvider.GetRequiredService<IAirportPlugin>();
        var weatherPluginInstance = serviceProvider.GetRequiredService<IWeatherPlugin>();

        kernel.Plugins.AddFromObject(airportPluginInstance, "AirportPlugin");
        kernel.Plugins.AddFromObject(weatherPluginInstance, "WeatherPlugin");

        // Assert
        kernel.Plugins.Should().Contain(p => p.Name == "AirportPlugin");
        kernel.Plugins.Should().Contain(p => p.Name == "WeatherPlugin");
        
        // Detailed verification of kernel functions
        var airportFunctions = kernel.Plugins["AirportPlugin"].Select(f => f.Name);
        airportFunctions.Should().Contain("get_airport_info");

        var weatherFunctions = kernel.Plugins["WeatherPlugin"].Select(f => f.Name);
        weatherFunctions.Should().Contain("get_airport_weather");
    }
}
