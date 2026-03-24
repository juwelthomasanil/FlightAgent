namespace FlightAgent.Core.Models;

public class Flight
{
    public string FlightNumber { get; set; } = string.Empty;
    public string Airline { get; set; } = string.Empty;
    public string Origin { get; set; } = string.Empty;
    public string Destination { get; set; } = string.Empty;
    public DateTime ScheduledDeparture { get; set; }
    public DateTime ScheduledArrival { get; set; }
    public FlightStatus Status { get; set; }
    public string Gate { get; set; } = string.Empty;
    public string Terminal { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public int AvailableSeats { get; set; }
}

public enum FlightStatus
{
    OnTime,
    Delayed,
    Cancelled,
    Boarding,
    Departed,
    Arrived
}
