using Microsoft.SemanticKernel;
using FlightAgent.Core.Interfaces;
using FlightAgent.Core.Models;

namespace FlightAgent.Infrastructure.Services;

public class SemanticBookingService : IBookingService
{
    private readonly Kernel _kernel;
    private readonly List<Booking> _bookings = new();

    public SemanticBookingService(Kernel kernel)
    {
        _kernel = kernel;
    }

    public async Task<Booking> CreateBookingAsync(string flightNumber, Passenger passenger, SeatClass seatClass)
    {
        var booking = new Booking
        {
            BookingReference = Guid.NewGuid().ToString()[..8].ToUpper(),
            FlightNumber = flightNumber,
            Passenger = passenger,
            SeatClass = seatClass,
            TotalPrice = CalculatePrice(seatClass),
            Status = BookingStatus.Confirmed,
            BookingDate = DateTime.UtcNow
        };

        // Use Semantic Kernel to process booking intent or validate
        var prompt = $@"""
            Process booking for passenger {passenger.FirstName} {passenger.LastName}
            on flight {flightNumber} in {seatClass} class.
            Confirm the booking is valid.
            """;

        await _kernel.InvokePromptAsync(prompt);

        _bookings.Add(booking);
        return booking;
    }

    public Task<Booking?> GetBookingAsync(string bookingReference)
    {
        var booking = _bookings.FirstOrDefault(b => b.BookingReference == bookingReference);
        return Task.FromResult(booking);
    }

    public Task CancelBookingAsync(string bookingReference)
    {
        var booking = _bookings.FirstOrDefault(b => b.BookingReference == bookingReference);
        if (booking != null)
        {
            booking.Status = BookingStatus.Cancelled;
        }
        return Task.CompletedTask;
    }

    private static decimal CalculatePrice(SeatClass seatClass) => seatClass switch
    {
        SeatClass.Economy => 299.99m,
        SeatClass.Business => 899.99m,
        SeatClass.FirstClass => 1999.99m,
        _ => 299.99m
    };
}
