using System.Globalization;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using FlightAgent.Core.Interfaces;
using FlightAgent.Core.Models;
using FlightAgent.Infrastructure.Models;

namespace FlightAgent.Infrastructure.Services;

public class AviationStackService : IAviationStackService
{
    private readonly HttpClient _httpClient;
    private readonly string _apiKey;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public AviationStackService(HttpClient httpClient, IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(httpClient);
        _httpClient = httpClient;

        _apiKey = configuration["AviationStack:ApiKey"]
            ?? throw new InvalidOperationException("AviationStack API key not configured. Run: dotnet user-secrets set AviationStack:ApiKey \"your-key\"");
    }

    public async Task<FlightInfo?> GetFlightStatusAsync(
        string flightNumber,
        string date,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNullOrWhiteSpace(flightNumber);
        ArgumentNullException.ThrowIfNullOrWhiteSpace(date);

        var url = $"flights?access_key={Uri.EscapeDataString(_apiKey)}&flight_iata={Uri.EscapeDataString(flightNumber)}&flight_date={Uri.EscapeDataString(date)}&limit=1";

        using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        cts.CancelAfter(TimeSpan.FromSeconds(8));

        var response = await _httpClient.GetAsync(url, cts.Token);
        response.EnsureSuccessStatusCode();

        var json = await response.Content.ReadAsStringAsync(cts.Token);
        var result = JsonSerializer.Deserialize<AviationStackResponse>(json, JsonOptions);

        var flight = result?.Data?.FirstOrDefault();
        if (flight == null)
        {
            return null;
        }

        return MapToFlightInfo(flight);
    }

    private static FlightInfo MapToFlightInfo(FlightData data) => new(
        FlightNumber: data.Flight.Iata,
        Airline: data.Airline.Name,
        DepartureAirport: data.Departure.Iata,
        ArrivalAirport: data.Arrival.Iata,
        ScheduledDeparture: DateTime.Parse(data.Departure.Scheduled, CultureInfo.InvariantCulture),
        ActualDeparture: DateTime.TryParse(data.Departure.Actual, CultureInfo.InvariantCulture, DateTimeStyles.None, out var actual) ? actual : null,
        Status: MapStatus(data.FlightStatus),
        DelayMinutes: data.Departure.Delay
    );

    private static string MapStatus(string? status) => status switch
    {
        "active" or "scheduled" or "landed" => "on_time",
        "delayed" => "delayed",
        "cancelled" => "cancelled",
        _ => "unknown"
    };
}
