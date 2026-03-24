using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.SemanticKernel;
using FlightAgent.Core.Interfaces;

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

        // Register services
        services.AddScoped<IFlightSearchService, Services.SemanticFlightSearchService>();
        services.AddScoped<IBookingService, Services.SemanticBookingService>();

        return services;
    }
}
