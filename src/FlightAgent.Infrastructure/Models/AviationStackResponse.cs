using System.Text.Json.Serialization;

namespace FlightAgent.Infrastructure.Models;

public class AviationStackResponse
{
    [JsonPropertyName("data")]
    public List<FlightData> Data { get; set; } = new();
}

public class FlightData
{
    [JsonPropertyName("flight_date")]
    public string FlightDate { get; set; } = "";

    [JsonPropertyName("flight_status")]
    public string? FlightStatus { get; set; }

    [JsonPropertyName("departure")]
    public AirportTimes Departure { get; set; } = new();

    [JsonPropertyName("arrival")]
    public AirportTimes Arrival { get; set; } = new();

    [JsonPropertyName("airline")]
    public AirlineInfo Airline { get; set; } = new();

    [JsonPropertyName("flight")]
    public FlightIdentifier Flight { get; set; } = new();
}

public class AirportTimes
{
    [JsonPropertyName("iata")]
    public string Iata { get; set; } = "";

    [JsonPropertyName("airport")]
    public string Airport { get; set; } = "";

    [JsonPropertyName("delay")]
    public int Delay { get; set; }

    [JsonPropertyName("scheduled")]
    public string Scheduled { get; set; } = "";

    [JsonPropertyName("actual")]
    public string? Actual { get; set; }
}

public class AirlineInfo
{
    [JsonPropertyName("name")]
    public string Name { get; set; } = "";

    [JsonPropertyName("iata")]
    public string Iata { get; set; } = "";
}

public class FlightIdentifier
{
    [JsonPropertyName("number")]
    public string Number { get; set; } = "";

    [JsonPropertyName("iata")]
    public string Iata { get; set; } = "";
}
