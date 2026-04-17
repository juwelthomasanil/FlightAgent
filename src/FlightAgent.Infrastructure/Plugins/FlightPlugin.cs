using Microsoft.SemanticKernel;
using System.ComponentModel;
using FlightAgent.Core.Interfaces;
using FlightAgent.Core.Models;

namespace FlightAgent.Infrastructure.Plugins;

public class FlightPlugin : IFlightPlugin
{
    private readonly IAviationStackService _aviationStackService;

    public FlightPlugin(IAviationStackService aviationStackService)
    {
        _aviationStackService = aviationStackService;
    }

    [KernelFunction("get_flight_status")]
    [Description("Gets real-time status for a flight by its IATA flight number (e.g., AA1004, BA178, KL071). " +
        "Returns flight status (on_time/delayed/cancelled), departure/arrival times in ISO 8601 format, " +
        "delay minutes, and IATA airport codes for departure and arrival. " +
        "Call this when the user asks about a specific flight's status, delays, or arrival/departure times.")]
    public async Task<FlightInfo?> GetFlightStatusAsync(
        [Description("The IATA flight number including airline prefix (e.g., AA1004, KL071, BA178)")] string flightNumber,
        [Description("The flight date in YYYY-MM-DD format (e.g., 2026-04-17)")] string date,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(flightNumber) || string.IsNullOrWhiteSpace(date))
        {
            return null;
        }

        return await _aviationStackService.GetFlightStatusAsync(flightNumber, date, cancellationToken);
    }
}
