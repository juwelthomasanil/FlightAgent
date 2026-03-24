namespace FlightAgent.Core.Models;

public class Booking
{
    public string BookingReference { get; set; } = string.Empty;
    public string FlightNumber { get; set; } = string.Empty;
    public Passenger Passenger { get; set; } = new();
    public SeatClass SeatClass { get; set; }
    public decimal TotalPrice { get; set; }
    public BookingStatus Status { get; set; }
    public DateTime BookingDate { get; set; }
}

public class Passenger
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PassportNumber { get; set; } = string.Empty;
}

public enum SeatClass
{
    Economy,
    Business,
    FirstClass
}

public enum BookingStatus
{
    Confirmed,
    Pending,
    Cancelled,
    CheckedIn
}
