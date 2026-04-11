using FlightAgent.Infrastructure;
using Microsoft.SemanticKernel;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

// Register infrastructure services (includes health checks)
builder.Services.AddInfrastructure(builder.Configuration);

// Register agent plugins
builder.Services.AddFlightAgentPlugins();

var app = builder.Build();

// Wire plugins to Semantic Kernel
var kernel = app.Services.GetRequiredService<Microsoft.SemanticKernel.Kernel>();
var airportPlugin = app.Services.GetRequiredService<FlightAgent.Core.Interfaces.IAirportPlugin>();
var weatherPlugin = app.Services.GetRequiredService<FlightAgent.Core.Interfaces.IWeatherPlugin>();

kernel.Plugins.AddFromObject(airportPlugin, "AirportPlugin");
kernel.Plugins.AddFromObject(weatherPlugin, "WeatherPlugin");

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

var summaries = new[]
{
    "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
};

// Map health check endpoint with JSON response
app.MapHealthChecks("/health", new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions
{
    ResponseWriter = async (context, report) =>
    {
        context.Response.ContentType = "application/json";
        var result = new
        {
            status = report.Status.ToString(),
            checks = report.Entries.Select(e => new
            {
                name = e.Key,
                status = e.Value.Status.ToString(),
                description = e.Value.Description,
                duration = e.Value.Duration.ToString()
            }),
            totalDuration = report.TotalDuration.ToString()
        };
        await context.Response.WriteAsJsonAsync(result);
    }
});

app.MapGet("/weatherforecast", () =>
{
    var forecast =  Enumerable.Range(1, 5).Select(index =>
        new WeatherForecast
        (
            DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
            Random.Shared.Next(-20, 55),
            summaries[Random.Shared.Next(summaries.Length)]
        ))
        .ToArray();
    return forecast;
})
.WithName("GetWeatherForecast");

app.Run();

record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}
