namespace FlightAgent.Core.Interfaces;

public interface IFlightSearchService
{
    Task<IEnumerable<Models.Flight>> SearchFlightsAsync(string origin, string destination, DateTime date);
    Task<Models.Flight?> GetFlightByNumberAsync(string flightNumber);
}

public interface IBookingService
{
    Task<Models.Booking> CreateBookingAsync(string flightNumber, Models.Passenger passenger, Models.SeatClass seatClass);
    Task<Models.Booking?> GetBookingAsync(string bookingReference);
    Task CancelBookingAsync(string bookingReference);
}
