using Microsoft.SemanticKernel;
using FlightAgent.Core.Interfaces;
using FlightAgent.Core.Models;

namespace FlightAgent.Infrastructure.Services;

public class SemanticFlightSearchService : IFlightSearchService
{
    private readonly Kernel _kernel;

    public SemanticFlightSearchService(Kernel kernel)
    {
        _kernel = kernel;
    }

    public async Task<IEnumerable<Flight>> SearchFlightsAsync(string origin, string destination, DateTime date)
    {
        // This would integrate with an airline API
        // For demo purposes, returning mock data processed through Semantic Kernel
        var prompt = $@"""
            Generate 3 realistic flight options from {origin} to {destination} on {date:yyyy-MM-dd}.
            Include flight numbers, airlines, times, and prices.
            Return as structured data.
            """;

        var result = await _kernel.InvokePromptAsync(prompt);

        // Parse the result or use a more structured approach with functions
        return GenerateMockFlights(origin, destination, date);
    }

    public async Task<Flight?> GetFlightByNumberAsync(string flightNumber)
    {
        // Use Semantic Kernel to enrich flight data
        return await Task.FromResult(new Flight
        {
            FlightNumber = flightNumber,
            Airline = "Sample Airlines",
            Origin = "NYC",
            Destination = "LAX",
            ScheduledDeparture = DateTime.Now.AddHours(2),
            ScheduledArrival = DateTime.Now.AddHours(5),
            Status = FlightStatus.OnTime,
            Gate = "A12",
            Terminal = "T1",
            Price = 299.99m,
            AvailableSeats = 45
        });
    }

    private IEnumerable<Flight> GenerateMockFlights(string origin, string destination, DateTime date)
    {
        var airlines = new[] { "Delta", "United", "American", "Southwest" };
        var flights = new List<Flight>();

        for (int i = 0; i < 3; i++)
        {
            var departure = date.AddHours(8 + i * 4);
            flights.Add(new Flight
            {
                FlightNumber = $"{airlines[i][0]}100{i + 1}",
                Airline = airlines[i],
                Origin = origin,
                Destination = destination,
                ScheduledDeparture = departure,
                ScheduledArrival = departure.AddHours(5),
                Status = FlightStatus.OnTime,
                Gate = $"A{i + 1}",
                Terminal = "T1",
                Price = 200m + i * 100,
                AvailableSeats = 50 - i * 10
            });
        }

        return flights;
    }
}
