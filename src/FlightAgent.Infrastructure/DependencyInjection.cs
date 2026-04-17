using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Http.Resilience;
using Microsoft.SemanticKernel;
using FlightAgent.Core.Interfaces;
using FlightAgent.Infrastructure.Plugins;
using FlightAgent.Infrastructure.Services;
using System.Net.Http;

namespace FlightAgent.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        // Configure Semantic Kernel
        var kernelBuilder = Kernel.CreateBuilder();

        // Add AI service (configure with your preferred provider)
        // Example with Azure OpenAI:
        // kernelBuilder.AddAzureOpenAIChatCompletion(
        //     deploymentName: configuration["AzureOpenAI:DeploymentName"]!,
        //     endpoint: configuration["AzureOpenAI:Endpoint"]!,
        //     apiKey: configuration["AzureOpenAI:ApiKey"]!);

        // Example with OpenAI:
        // kernelBuilder.AddOpenAIChatCompletion(
        //     modelId: configuration["OpenAI:ModelId"] ?? "gpt-4",
        //     apiKey: configuration["OpenAI:ApiKey"]!);

        services.AddSingleton(kernelBuilder.Build());

        // Register HttpClient for health checks and external API calls
        services.AddHttpClient();

        // Register services
        services.AddScoped<IFlightSearchService, Services.SemanticFlightSearchService>();
        services.AddScoped<IBookingService, Services.SemanticBookingService>();

        // Register health checks with custom external API check
        services.AddHealthChecks()
            .AddCheck<Health.ExternalApiHealthCheck>("external_api", tags: new[] { "external" });

        return services;
    }

    /// <summary>
    /// Registers agent plugins and their dependencies into the DI container.
    /// Note: This registers the services for dependency injection, but does not add them to Semantic Kernel.
    /// SK wiring must happen in Program.cs where the Kernel instance is available.
    /// </summary>
    public static IServiceCollection AddFlightAgentPlugins(this IServiceCollection services)
    {
        services.AddMemoryCache(); // Required by WeatherPlugin
        services.AddSingleton<IAirportPlugin, Plugins.AirportPlugin>();
        services.AddSingleton<IWeatherPlugin, Plugins.WeatherPlugin>();

        return services;
    }
}
