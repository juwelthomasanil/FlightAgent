namespace FlightAgent.Core.Models;

public record FlightInfo(
    string FlightNumber,
    string Airline,
    string DepartureAirport,
    string ArrivalAirport,
    DateTime ScheduledDeparture,
    DateTime? ActualDeparture,
    string Status,
    int DelayMinutes);
