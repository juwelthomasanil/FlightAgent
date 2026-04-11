using FlightAgent.Core.Models;

namespace FlightAgent.Infrastructure.Data;

/// <summary>
/// Static dataset of 50 major global airport hubs.
/// Data is hardcoded per D-07 (no external files/database for v1).
/// </summary>
public static class AirportData
{
    /// <summary>
    /// Loads the hardcoded airport dataset.
    /// </summary>
    /// <returns>Dictionary keyed by IATA code (case-insensitive) with AirportInfo values.</returns>
    public static Dictionary<string, AirportInfo> LoadAirports()
    {
        return new Dictionary<string, AirportInfo>(StringComparer.OrdinalIgnoreCase)
        {
            // ── North America (15) ──────────────────────────────────────────
            ["JFK"] = new("JFK", "John F. Kennedy International Airport", "New York", "USA", 40.6413, -73.7781, "America/New_York"),
            ["LAX"] = new("LAX", "Los Angeles International Airport", "Los Angeles", "USA", 33.9425, -118.4081, "America/Los_Angeles"),
            ["ORD"] = new("ORD", "O'Hare International Airport", "Chicago", "USA", 41.9742, -87.9073, "America/Chicago"),
            ["ATL"] = new("ATL", "Hartsfield-Jackson Atlanta International Airport", "Atlanta", "USA", 33.6407, -84.4277, "America/New_York"),
            ["DFW"] = new("DFW", "Dallas/Fort Worth International Airport", "Dallas", "USA", 32.8998, -97.0403, "America/Chicago"),
            ["DEN"] = new("DEN", "Denver International Airport", "Denver", "USA", 39.8561, -104.6737, "America/Denver"),
            ["SFO"] = new("SFO", "San Francisco International Airport", "San Francisco", "USA", 37.6213, -122.3790, "America/Los_Angeles"),
            ["SEA"] = new("SEA", "Seattle-Tacoma International Airport", "Seattle", "USA", 47.4502, -122.3088, "America/Los_Angeles"),
            ["MIA"] = new("MIA", "Miami International Airport", "Miami", "USA", 25.7959, -80.2870, "America/New_York"),
            ["BOS"] = new("BOS", "Logan International Airport", "Boston", "USA", 42.3656, -71.0096, "America/New_York"),
            ["LAS"] = new("LAS", "Harry Reid International Airport", "Las Vegas", "USA", 36.0840, -115.1537, "America/Los_Angeles"),
            ["PHX"] = new("PHX", "Phoenix Sky Harbor International Airport", "Phoenix", "USA", 33.4373, -112.0078, "America/Phoenix"),
            ["YYZ"] = new("YYZ", "Toronto Pearson International Airport", "Toronto", "Canada", 43.6777, -79.6248, "America/Toronto"),
            ["YVR"] = new("YVR", "Vancouver International Airport", "Vancouver", "Canada", 49.1967, -123.1815, "America/Vancouver"),
            ["MEX"] = new("MEX", "Mexico City International Airport", "Mexico City", "Mexico", 19.4363, -99.0721, "America/Mexico_City"),

            // ── Europe (15) ──────────────────────────────────────────────────
            ["LHR"] = new("LHR", "London Heathrow Airport", "London", "UK", 51.4700, -0.4543, "Europe/London"),
            ["CDG"] = new("CDG", "Charles de Gaulle Airport", "Paris", "France", 49.0097, 2.5479, "Europe/Paris"),
            ["FRA"] = new("FRA", "Frankfurt Airport", "Frankfurt", "Germany", 50.0379, 8.5622, "Europe/Berlin"),
            ["AMS"] = new("AMS", "Amsterdam Schiphol Airport", "Amsterdam", "Netherlands", 52.3105, 4.7683, "Europe/Amsterdam"),
            ["MAD"] = new("MAD", "Adolfo Suárez Madrid–Barajas Airport", "Madrid", "Spain", 40.4983, -3.5676, "Europe/Madrid"),
            ["BCN"] = new("BCN", "Josep Tarradellas Barcelona–El Prat Airport", "Barcelona", "Spain", 41.2974, 2.0833, "Europe/Madrid"),
            ["FCO"] = new("FCO", "Leonardo da Vinci–Fiumicino Airport", "Rome", "Italy", 41.8003, 12.2389, "Europe/Rome"),
            ["MUC"] = new("MUC", "Munich Airport", "Munich", "Germany", 48.3537, 11.7750, "Europe/Berlin"),
            ["ZRH"] = new("ZRH", "Zurich Airport", "Zurich", "Switzerland", 47.4647, 8.5492, "Europe/Zurich"),
            ["VIE"] = new("VIE", "Vienna International Airport", "Vienna", "Austria", 48.1103, 16.5697, "Europe/Vienna"),
            ["CPH"] = new("CPH", "Copenhagen Airport", "Copenhagen", "Denmark", 55.6180, 12.6508, "Europe/Copenhagen"),
            ["ARN"] = new("ARN", "Stockholm Arlanda Airport", "Stockholm", "Sweden", 59.6519, 17.9186, "Europe/Stockholm"),
            ["DUB"] = new("DUB", "Dublin Airport", "Dublin", "Ireland", 53.4264, -6.2499, "Europe/Dublin"),
            ["IST"] = new("IST", "Istanbul Airport", "Istanbul", "Turkey", 41.2753, 28.7519, "Europe/Istanbul"),
            ["SVO"] = new("SVO", "Sheremetyevo International Airport", "Moscow", "Russia", 55.9726, 37.4146, "Europe/Moscow"),

            // ── Asia-Pacific (15) ────────────────────────────────────────────
            ["NRT"] = new("NRT", "Narita International Airport", "Tokyo", "Japan", 35.7720, 140.3929, "Asia/Tokyo"),
            ["HND"] = new("HND", "Tokyo Haneda Airport", "Tokyo", "Japan", 35.5494, 139.7798, "Asia/Tokyo"),
            ["ICN"] = new("ICN", "Incheon International Airport", "Seoul", "South Korea", 37.4602, 126.4407, "Asia/Seoul"),
            ["SIN"] = new("SIN", "Singapore Changi Airport", "Singapore", "Singapore", 1.3644, 103.9915, "Asia/Singapore"),
            ["HKG"] = new("HKG", "Hong Kong International Airport", "Hong Kong", "China", 22.3080, 113.9185, "Asia/Hong_Kong"),
            ["PEK"] = new("PEK", "Beijing Capital International Airport", "Beijing", "China", 40.0799, 116.6031, "Asia/Shanghai"),
            ["PVG"] = new("PVG", "Shanghai Pudong International Airport", "Shanghai", "China", 31.1443, 121.8083, "Asia/Shanghai"),
            ["BKK"] = new("BKK", "Suvarnabhumi Airport", "Bangkok", "Thailand", 13.6900, 100.7501, "Asia/Bangkok"),
            ["KUL"] = new("KUL", "Kuala Lumpur International Airport", "Kuala Lumpur", "Malaysia", 2.7456, 101.7099, "Asia/Kuala_Lumpur"),
            ["DEL"] = new("DEL", "Indira Gandhi International Airport", "New Delhi", "India", 28.5562, 77.1000, "Asia/Kolkata"),
            ["BOM"] = new("BOM", "Chhatrapati Shivaji Maharaj International Airport", "Mumbai", "India", 19.0896, 72.8656, "Asia/Kolkata"),
            ["SYD"] = new("SYD", "Sydney Kingsford Smith Airport", "Sydney", "Australia", -33.9461, 151.1772, "Australia/Sydney"),
            ["MEL"] = new("MEL", "Melbourne Airport", "Melbourne", "Australia", -37.6690, 144.8410, "Australia/Melbourne"),
            ["AKL"] = new("AKL", "Auckland Airport", "Auckland", "New Zealand", -37.0082, 174.7850, "Pacific/Auckland"),
            ["TPE"] = new("TPE", "Taiwan Taoyuan International Airport", "Taipei", "Taiwan", 25.0797, 121.2342, "Asia/Taipei"),

            // ── Middle East / Africa (5) ─────────────────────────────────────
            ["DXB"] = new("DXB", "Dubai International Airport", "Dubai", "UAE", 25.2532, 55.3657, "Asia/Dubai"),
            ["DOH"] = new("DOH", "Hamad International Airport", "Doha", "Qatar", 25.2731, 51.6081, "Asia/Qatar"),
            ["AUH"] = new("AUH", "Abu Dhabi International Airport", "Abu Dhabi", "UAE", 24.4330, 54.6511, "Asia/Dubai"),
            ["JNB"] = new("JNB", "O.R. Tambo International Airport", "Johannesburg", "South Africa", -26.1392, 28.2460, "Africa/Johannesburg"),
            ["CAI"] = new("CAI", "Cairo International Airport", "Cairo", "Egypt", 30.1219, 31.4056, "Africa/Cairo"),
        };
    }
}
