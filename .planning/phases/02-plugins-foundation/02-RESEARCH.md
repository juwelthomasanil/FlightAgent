# Phase 2: Plugins Foundation - Research

**Researched:** 2026-04-11
**Domain:** Semantic Kernel Plugins, Open-Meteo API, IMemoryCache
**Confidence:** HIGH

## Summary

This phase implements the core plugin architecture for FlightAgent using Semantic Kernel's native plugin pattern. Two plugins will be created: `AirportPlugin` (hardcoded dataset of 50 major airports) and `WeatherPlugin` (Open-Meteo API integration with caching). The plugins expose methods via `[KernelFunction]` attributes that the LLM can discover and invoke.

**Key Findings:**
- Semantic Kernel reflects on concrete classes, not interfaces — attributes must be on the concrete plugin class
- Open-Meteo API is free, requires no API key, has 10,000 requests/day limit
- IMemoryCache with 15-minute absolute expiry is the standard approach for weather data caching
- WMO weather codes 0-99 map to human-readable conditions (0=Clear sky, 1=Mainly clear, etc.)

**Primary recommendation:** Use concrete plugin classes with `[KernelFunction]` attributes, register via `AddFromObject()`, implement `IMemoryCache` with `AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(15)`.

---

## User Constraints (from CONTEXT.md)

### Locked Decisions
- **D-01:** No common `IPlugin` interface. Each plugin is a plain C# class with `[KernelFunction]` methods
- **D-02:** Use separate specific interfaces (`IAirportPlugin`, `IWeatherPlugin`) for testability via Moq
- **D-03:** No abstract base class — Semantic Kernel reflects on concrete class methods, not interfaces
- **D-04:** `[KernelFunction]` and `[Description]` attributes stay on concrete classes only
- **D-05:** Hardcoded dataset of 50 major global airport hubs
- **D-06:** Fields per airport: IATA code, name, city, country, latitude, longitude, timezone
- **D-07:** Store as `Dictionary<string, AirportInfo>` where key is IATA code
- **D-09:** Coordinates are mandatory — WeatherPlugin needs them to call Open-Meteo
- **D-10:** Use Open-Meteo API (free, no API key required)
- **D-11:** Endpoint: `/v1/forecast` with `current=temperature_2m,weathercode,windspeed_10m,winddirection_10m`
- **D-12:** Current weather only — no forecast in v1
- **D-15:** Use `IMemoryCache` (Microsoft.Extensions.Caching.Memory, in-box)
- **D-16:** Cache per airport code with 15-minute absolute expiry
- **D-19:** Airport code not found → return `null` from method
- **D-24:** Use extension method pattern like existing `AddInfrastructure()`
- **D-28:** Resolve concrete type from DI when registering with SK, not the interface

### Claude's Discretion
- Exact weather data formatting string (human-readable output from plugin)
- Airport selection for the 50 major hubs (common international airports)
- Open-Meteo API response parsing implementation details
- MemoryCache configuration specifics (size limits, compaction)

### Deferred Ideas (OUT OF SCOPE)
- Weather forecast data (multi-day) — out of scope for v1
- Airport fuzzy search by city name — Phase 2 supports IATA only
- Additional weather fields (humidity, pressure, UV index)
- Plugin versioning or dynamic loading
- Database persistence for airport data

---

## Phase Requirements

| ID | Description | Research Support |
|----|-------------|------------------|
| REQ-006 | Weather Plugin: Open-Meteo API, 15-minute cache, temperature/conditions/visibility | Open-Meteo `/v1/forecast` endpoint with `current` parameter; IMemoryCache with `AbsoluteExpirationRelativeToNow` |
| REQ-007 | Airport Plugin: hardcoded dataset, IATA code lookup, timezone info | Dictionary<string, AirportInfo> pattern; 50 major hubs from aviation reference data |

---

## Standard Stack

### Core
| Library | Version | Purpose | Why Standard |
|---------|---------|---------|--------------|
| Microsoft.SemanticKernel | 1.40.0 | AI plugin framework | Microsoft official; `[KernelFunction]` reflection |
| Microsoft.Extensions.Caching.Memory | 9.0.0 | In-memory caching | In-box with ASP.NET Core; no external deps |
| System.Text.Json | 10.0.0 | JSON deserialization | Built-in; source generator support |

### Installation
```bash
# Semantic Kernel already installed (1.40.0 verified)
# Memory caching already available via Microsoft.Extensions.Caching.Memory
# No additional packages required
```

**Version verification:**
- `Microsoft.SemanticKernel` 1.40.0 confirmed in `FlightAgent.Infrastructure.csproj` [VERIFIED: codebase]
- `Microsoft.Extensions.Caching.Memory` included via `Microsoft.Extensions.*` 9.0.0 packages [VERIFIED: codebase]

---

## Architecture Patterns

### Plugin Class Structure
```
src/
├── FlightAgent.Core/
│   ├── Interfaces/
│   │   ├── IAirportPlugin.cs      # Thin interface for DI/Moq
│   │   └── IWeatherPlugin.cs      # Thin interface for DI/Moq
│   └── Models/
│       └── AirportInfo.cs         # Record: IataCode, Name, City, Country, Lat, Lon, Timezone
├── FlightAgent.Infrastructure/
│   ├── Plugins/
│   │   ├── AirportPlugin.cs       # Concrete class with [KernelFunction]
│   │   └── WeatherPlugin.cs       # Concrete class with [KernelFunction]
│   └── DependencyInjection.cs    # AddFlightAgentPlugins() extension
```

### Pattern 1: Semantic Kernel Plugin with Attributes
**What:** Concrete class with `[KernelFunction]` and `[Description]` attributes
**When to use:** When exposing functionality to LLM for potential invocation
**Example:**
```csharp
// Source: https://learn.microsoft.com/en-us/semantic-kernel/concepts/plugins/adding-native-plugins
using Microsoft.SemanticKernel;
using System.ComponentModel;

public class AirportPlugin
{
    private readonly Dictionary<string, AirportInfo> _airports;

    public AirportPlugin()
    {
        _airports = LoadHardcodedAirports();
    }

    [KernelFunction("get_airport_info")]
    [Description("Gets information about an airport by its IATA code (e.g., JFK, LHR, NRT). Returns null if not found.")]
    public Task<string?> GetAirportInfoAsync(
        [Description("The 3-letter IATA airport code (e.g., JFK, LHR, CDG)")] string iataCode,
        CancellationToken cancellationToken = default)
    {
        if (_airports.TryGetValue(iataCode.ToUpperInvariant(), out var airport))
        {
            return Task.FromResult<string?>($"{airport.Name} ({airport.IataCode}) - {airport.City}, {airport.Country}. Coordinates: {airport.Lat}, {airport.Lon}. Timezone: {airport.Timezone}");
        }
        return Task.FromResult<string?>(null);
    }
}
```

### Pattern 2: IMemoryCache with Absolute Expiration
**What:** Cache weather data with 15-minute absolute expiry per airport
**When to use:** When external API calls should be minimized and data freshness is acceptable
**Example:**
```csharp
// Source: https://learn.microsoft.com/en-us/aspnet/core/performance/caching/memory
public class WeatherPlugin
{
    private readonly IMemoryCache _cache;
    private readonly HttpClient _httpClient;
    private readonly IAirportPlugin _airportPlugin;
    private static readonly TimeSpan CacheDuration = TimeSpan.FromMinutes(15);

    public WeatherPlugin(IMemoryCache cache, HttpClient httpClient, IAirportPlugin airportPlugin)
    {
        _cache = cache;
        _httpClient = httpClient;
        _airportPlugin = airportPlugin;
    }

    [KernelFunction("get_airport_weather")]
    [Description("Gets current weather for an airport by its IATA code.")]
    public async Task<string?> GetAirportWeatherAsync(string iataCode, CancellationToken ct = default)
    {
        var cacheKey = $"weather:{iataCode.ToUpperInvariant()}";
        
        if (_cache.TryGetValue(cacheKey, out string? cachedWeather))
        {
            return cachedWeather;
        }

        // Fetch from API...
        var weather = await FetchWeatherAsync(iataCode, ct);
        
        if (weather != null)
        {
            var options = new MemoryCacheEntryOptions()
                .SetAbsoluteExpiration(CacheDuration);
            _cache.Set(cacheKey, weather, options);
        }
        
        return weather;
    }
}
```

### Pattern 3: Extension Method Registration
**What:** Follow existing `AddInfrastructure()` pattern for plugin registration
**When to use:** Keeping DI registration clean and centralized
**Example:**
```csharp
// Source: Existing codebase pattern (DependencyInjection.cs)
public static class DependencyInjection
{
    public static IServiceCollection AddFlightAgentPlugins(this IServiceCollection services)
    {
        // Register plugin interfaces and implementations
        services.AddSingleton<IAirportPlugin, AirportPlugin>();
        services.AddSingleton<IWeatherPlugin, WeatherPlugin>();
        
        // MemoryCache already registered in AddInfrastructure
        // HttpClient already registered in AddInfrastructure
        
        return services;
    }
}
```

### Pattern 4: SK Plugin Registration in Program.cs
**What:** Register concrete plugin instances with the Kernel
**When to use:** After DI container is built, wire plugins into Semantic Kernel
**Example:**
```csharp
// Source: https://learn.microsoft.com/en-us/semantic-kernel/concepts/plugins/adding-native-plugins
var app = builder.Build();

// Resolve plugins from DI and register with Kernel
using (var scope = app.Services.CreateScope())
{
    var sp = scope.ServiceProvider;
    var kernel = sp.GetRequiredService<Kernel>();
    
    // SK reflects on concrete class, so resolve concrete type
    var airportPlugin = sp.GetRequiredService<IAirportPlugin>();
    kernel.Plugins.AddFromObject(airportPlugin, "Airport");
    
    var weatherPlugin = sp.GetRequiredService<IWeatherPlugin>();
    kernel.Plugins.AddFromObject(weatherPlugin, "Weather");
}
```

### Anti-Patterns to Avoid
- **Interface with `[KernelFunction]`:** SK reflects on concrete classes, not interfaces — attributes on interfaces are ignored
- **Async void:** Always use `Task` or `Task<T>` for async plugin methods
- **Throwing exceptions:** Return error strings the LLM can read; don't throw
- **Cache without expiry:** Always set `AbsoluteExpiration` or `AbsoluteExpirationRelativeToNow`
- **Manual cache key concatenation:** Use consistent patterns like `$"weather:{iataCode.ToUpperInvariant()}"`

---

## Don't Hand-Roll

| Problem | Don't Build | Use Instead | Why |
|---------|-------------|-------------|-----|
| Weather data caching | Custom cache implementation | `IMemoryCache` with `MemoryCacheEntryOptions` | Thread-safe, memory-managed, standard expiration policies |
| WMO code mapping | Switch statement for all 99 codes | Dictionary lookup with fallback | Easier maintenance; only implement codes 0, 1, 2, 3, 45, 48, 51-57, 61-67, 71-77, 80-82, 85-86, 95-99 |
| HTTP resilience | Retry loops in plugin | Polly (already in requirements) | Circuit breaker, exponential backoff, standardized patterns |
| JSON parsing | Manual string parsing | `System.Text.Json` | Source generators, performance, standard |
| Plugin discovery | Reflection over assemblies | `kernel.Plugins.AddFromObject()` | Official SK pattern, tested, maintained |

**Key insight:** The WMO weather code mapping is a solved problem — use a Dictionary with the 26 relevant codes rather than a 99-case switch statement.

---

## Open-Meteo API Reference

### Endpoint
```
GET https://api.open-meteo.com/v1/forecast
```

### Required Parameters for Current Weather
| Parameter | Value | Purpose |
|-----------|-------|---------|
| `latitude` | double | Airport latitude |
| `longitude` | double | Airport longitude |
| `current` | comma-separated string | Fields: `temperature_2m,weathercode,windspeed_10m,winddirection_10m` |

### Optional Parameters
| Parameter | Recommended Value | Purpose |
|-----------|-------------------|---------|
| `temperature_unit` | `celsius` (default) | Temperature scale |
| `wind_speed_unit` | `kmh` (default) | Wind speed units |

### Response Format
```json
{
  "latitude": 40.7128,
  "longitude": -74.006,
  "current": {
    "time": "2026-04-11T14:30",
    "temperature_2m": 18.5,
    "weathercode": 1,
    "windspeed_10m": 12.3,
    "winddirection_10m": 245
  },
  "current_units": {
    "temperature_2m": "°C",
    "weathercode": "wmo code",
    "windspeed_10m": "km/h",
    "winddirection_10m": "°"
  }
}
```

### Rate Limits (Free Tier)
| Limit | Value |
|-------|-------|
| Per Minute | 600 calls |
| Per Hour | 5,000 calls |
| Per Day | 10,000 calls |
| Per Month | 300,000 calls |

**Attribution required:** `Weather data by Open-Meteo.com` [CITED: https://open-meteo.com/en/terms]

---

## WMO Weather Code Mapping

Complete mapping for Open-Meteo weather codes:

```csharp
// Source: https://gist.github.com/stellasphere/9490c195ed2b53c707087c8c2db4ec0c
private static readonly Dictionary<int, string> WmoWeatherCodes = new()
{
    [0] = "Clear sky",
    [1] = "Mainly clear",
    [2] = "Partly cloudy",
    [3] = "Overcast",
    [45] = "Foggy",
    [48] = "Rime fog",
    [51] = "Light drizzle",
    [53] = "Drizzle",
    [55] = "Heavy drizzle",
    [56] = "Light freezing drizzle",
    [57] = "Freezing drizzle",
    [61] = "Light rain",
    [63] = "Rain",
    [65] = "Heavy rain",
    [66] = "Light freezing rain",
    [67] = "Freezing rain",
    [71] = "Light snow",
    [73] = "Snow",
    [75] = "Heavy snow",
    [77] = "Snow grains",
    [80] = "Light rain showers",
    [81] = "Rain showers",
    [82] = "Heavy rain showers",
    [85] = "Light snow showers",
    [86] = "Snow showers",
    [95] = "Thunderstorm",
    [96] = "Thunderstorm with light hail",
    [99] = "Thunderstorm with hail"
};

public static string GetWeatherDescription(int code)
{
    return WmoWeatherCodes.TryGetValue(code, out var desc) 
        ? desc 
        : "Unknown conditions";
}
```

---

## Common Pitfalls

### Pitfall 1: SK Reflects on Concrete, Not Interface
**What goes wrong:** Putting `[KernelFunction]` on `IAirportPlugin` interface — SK won't discover it
**Why it happens:** Semantic Kernel uses reflection to find methods on the actual object instance, not interface definitions
**How to avoid:** Keep attributes on concrete `AirportPlugin` class only; interfaces are for DI/Moq
**Warning signs:** Plugin methods not appearing in LLM function list

### Pitfall 2: Cache Key Collisions
**What goes wrong:** Using same cache key for different data types or airports
**Why it happens:** Missing prefix/namespacing in cache keys
**How to avoid:** Use format `$"{type}:{identifier}"` e.g., `$"weather:{iataCode.ToUpperInvariant()}"`
**Warning signs:** Weather data for JFK appearing when requesting LHR

### Pitfall 3: Open-Meteo Coordinate Precision
**What goes wrong:** Sending too many decimal places causes cache misses for same location
**Why it happens:** Open-Meteo returns rounded coordinates in response; cache key should use original lookup coordinates
**How to avoid:** Use airport coordinates from hardcoded dataset as cache key basis
**Warning signs:** Same airport weather fetched multiple times despite caching

### Pitfall 4: Null Return Handling
**What goes wrong:** LLM receives empty string instead of clear "not found" message
**Why it happens:** Returning null vs empty string vs error message
**How to avoid:** Per D-19/D-20: return `null` (Task.FromResult<string?>(null)) and let LLM communicate gracefully
**Warning signs:** LLM says "I found information: " with blank content

### Pitfall 5: WeatherPlugin Circular Dependency
**What goes wrong:** Constructor injection creating circular dependency between plugins
**Why it happens:** Both plugins depending on each other
**How to avoid:** WeatherPlugin depends on AirportPlugin (for coordinates), but AirportPlugin has no dependency on WeatherPlugin
**Warning signs:** DI container throws circular dependency exception at startup

---

## Code Examples

### AirportInfo Record
```csharp
// Source: CONTEXT.md D-08
namespace FlightAgent.Core.Models;

public record AirportInfo(
    string IataCode, 
    string Name, 
    string City, 
    string Country, 
    double Lat, 
    double Lon, 
    string Timezone);
```

### Sample Airport Dataset (First 10)
```csharp
// Source: Aviation reference data - major global hubs
private static Dictionary<string, AirportInfo> LoadHardcodedAirports() => new(StringComparer.OrdinalIgnoreCase)
{
    ["JFK"] = new("JFK", "John F. Kennedy International Airport", "New York", "USA", 40.6413, -73.7781, "America/New_York"),
    ["LHR"] = new("LHR", "London Heathrow Airport", "London", "UK", 51.4700, -0.4543, "Europe/London"),
    ["NRT"] = new("NRT", "Narita International Airport", "Tokyo", "Japan", 35.7647, 140.3864, "Asia/Tokyo"),
    ["CDG"] = new("CDG", "Charles de Gaulle Airport", "Paris", "France", 49.0097, 2.5479, "Europe/Paris"),
    ["DXB"] = new("DXB", "Dubai International Airport", "Dubai", "UAE", 25.2532, 55.3657, "Asia/Dubai"),
    ["SIN"] = new("SIN", "Singapore Changi Airport", "Singapore", "Singapore", 1.3644, 103.9915, "Asia/Singapore"),
    ["LAX"] = new("LAX", "Los Angeles International Airport", "Los Angeles", "USA", 33.9416, -118.4085, "America/Los_Angeles"),
    ["AMS"] = new("AMS", "Amsterdam Airport Schiphol", "Amsterdam", "Netherlands", 52.3105, 4.7683, "Europe/Amsterdam"),
    ["FRA"] = new("FRA", "Frankfurt Airport", "Frankfurt", "Germany", 50.0379, 8.5622, "Europe/Berlin"),
    ["HKG"] = new("HKG", "Hong Kong International Airport", "Hong Kong", "China", 22.3080, 113.9185, "Asia/Hong_Kong"),
    // ... 40 more airports
};
```

### Weather API Call
```csharp
// Source: Open-Meteo API documentation
private async Task<WeatherData?> FetchWeatherAsync(double lat, double lon, CancellationToken ct)
{
    var url = $"https://api.open-meteo.com/v1/forecast" +
              $"?latitude={lat}&longitude={lon}" +
              $"&current=temperature_2m,weathercode,windspeed_10m,winddirection_10m";
    
    var response = await _httpClient.GetAsync(url, ct);
    response.EnsureSuccessStatusCode();
    
    var json = await response.Content.ReadAsStringAsync(ct);
    return JsonSerializer.Deserialize<WeatherData>(json);
}

public class WeatherData
{
    [JsonPropertyName("current")]
    public CurrentWeather Current { get; set; } = new();
}

public class CurrentWeather
{
    [JsonPropertyName("temperature_2m")]
    public double Temperature { get; set; }
    
    [JsonPropertyName("weathercode")]
    public int WeatherCode { get; set; }
    
    [JsonPropertyName("windspeed_10m")]
    public double WindSpeed { get; set; }
    
    [JsonPropertyName("winddirection_10m")]
    public int WindDirection { get; set; }
}
```

---

## Assumptions Log

| # | Claim | Section | Risk if Wrong |
|---|-------|---------|---------------|
| A1 | 50 major airports sufficient for v1 | Airport Data Scope | May need expansion if user expects specific regional airports |
| A2 | Open-Meteo rate limits (10k/day) sufficient for demo | Open-Meteo API | If demo has heavy traffic, may hit limits |
| A3 | String return type preferred for LLM readability | Plugin Patterns | Complex objects may need JSON serialization in description |
| A4 | Celsius default acceptable for temperature | Open-Meteo API | User may prefer Fahrenheit; can add parameter later |

---

## Environment Availability

| Dependency | Required By | Available | Version | Fallback |
|------------|------------|-----------|---------|----------|
| Open-Meteo API | WeatherPlugin | ✓ | — | None (required) |
| IMemoryCache | Weather caching | ✓ | 9.0.0 | — (in-box) |
| HttpClient | Open-Meteo calls | ✓ | — | Already registered in DI |

**Missing dependencies with no fallback:** None

**Missing dependencies with fallback:** None

---

## Validation Architecture

### Phase Requirements → Test Map

| Req ID | Behavior | Test Type | Automated Command | File Exists? |
|--------|----------|-----------|-------------------|--------------|
| REQ-006 | WeatherPlugin returns formatted weather | unit | `dotnet test --filter "WeatherPluginTests"` | ❌ Wave 0 |
| REQ-006 | Weather cached for 15 minutes | unit | `dotnet test --filter "WeatherCacheTests"` | ❌ Wave 0 |
| REQ-007 | AirportPlugin returns airport info | unit | `dotnet test --filter "AirportPluginTests"` | ❌ Wave 0 |
| REQ-007 | Airport lookup by IATA code | unit | `dotnet test --filter "AirportLookupTests"` | ❌ Wave 0 |

### Wave 0 Gaps
- [ ] `tests/FlightAgent.Infrastructure.Tests/Plugins/AirportPluginTests.cs`
- [ ] `tests/FlightAgent.Infrastructure.Tests/Plugins/WeatherPluginTests.cs`
- [ ] Test project `FlightAgent.Infrastructure.Tests` not yet created
- [ ] Moq package needs installation for interface mocking
- [ ] FluentAssertions package needs installation

---

## Sources

### Primary (HIGH confidence)
- [Open-Meteo Documentation](https://open-meteo.com/en/docs) - API endpoint, parameters, response format
- [WMO Weather Code Gist](https://gist.github.com/stellasphere/9490c195ed2b53c707087c8c2db4ec0c) - Complete code mapping 0-99
- [Microsoft Semantic Kernel Plugins](https://learn.microsoft.com/en-us/semantic-kernel/concepts/plugins/adding-native-plugins) - KernelFunction attribute usage
- [ASP.NET Core Memory Cache](https://learn.microsoft.com/en-us/aspnet/core/performance/caching/memory) - IMemoryCache patterns

### Secondary (MEDIUM confidence)
- [Open-Meteo Terms](https://open-meteo.com/en/terms) - Rate limits, attribution requirements
- [Semantic Kernel Plugin Best Practices](https://www.devleader.ca/2026/03/08/semantic-kernel-plugin-best-practices-and-patterns-for-c-developers) - 2026 patterns
- [Microsoft Learn KernelFunctionAttribute](https://learn.microsoft.com/fr-fr/dotnet/api/microsoft.semantickernel.kernelfunctionattribute) - API reference

### Verified from Codebase
- `DependencyInjection.cs` - Extension method pattern
- `SemanticFlightSearchService.cs` - Kernel injection pattern
- `ExternalApiHealthCheck.cs` - HttpClient usage pattern
- `FlightAgent.Infrastructure.csproj` - Package versions

---

## Metadata

**Confidence breakdown:**
- Standard stack: HIGH - Verified from codebase and official docs
- Architecture: HIGH - Based on existing codebase patterns
- Pitfalls: MEDIUM-HIGH - From official SK documentation and best practices

**Research date:** 2026-04-11
**Valid until:** 2026-07-11 (Semantic Kernel releases monthly, Open-Meteo stable)
