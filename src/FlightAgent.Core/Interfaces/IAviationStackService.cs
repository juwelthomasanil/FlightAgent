using FlightAgent.Core.Models;

namespace FlightAgent.Core.Interfaces;

public interface IAviationStackService
{
    Task<FlightInfo?> GetFlightStatusAsync(
        string flightNumber,
        string date,
        CancellationToken cancellationToken = default);
}
