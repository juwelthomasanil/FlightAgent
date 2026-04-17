using FlightAgent.Core.Models;

namespace FlightAgent.Core.Interfaces;

public interface IFlightPlugin
{
    Task<FlightInfo?> GetFlightStatusAsync(
        string flightNumber,
        string date,
        CancellationToken cancellationToken = default);
}
