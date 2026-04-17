# Phase 3: Flight Search Service - Research

**Researched:** 2026-04-17
**Domain:** AviationStack API integration with .NET HTTP resilience
**Confidence:** MEDIUM-HIGH

## Summary

Phase 3 adds real-time flight data via AviationStack API with Polly v8 resilience patterns. Key technical decisions needed:

1. **API Parameter**: AviationStack uses `flight_iata` (e.g., "AA1004"), not `flight_number`. The flight number includes airline prefix.
2. **Resilience Package**: `Microsoft.Extensions.Http.Resilience` is the official .NET 8+ package wrapping Polly v8 — not `Polly.Extensions.AddResiliencePipeline()` directly.
3. **HTTPS Required**: AviationStack docs indicate HTTPS on all plans (the CONTEXT says `http://` but official docs show `https://`).
4. **Existing Health Check**: Currently pings httpbin.org — needs a second AviationStack-specific health check.
5. **FlightInfo Mapping**: AviationStack returns `flight.iata` ("AA1004") and `flight.number` ("1004"), plus `departure.delay` in minutes.

## User Constraints (from CONTEXT.md)

### Locked Decisions
- D-01: AviationStack API for flight data
- D-02: API key in .NET user secrets
- D-03: Base URL `http://api.aviationstack.com/v1/` (NOTE: official docs use HTTPS)
- D-04: `FlightInfo` record with specified fields
- D-06: Concrete class with `[KernelFunction]` attributes
- D-08: `GetFlightStatus(flight_number, date)` returning FlightInfo
- D-09: Timeout 8 seconds
- D-10: Retry 3 attempts with exponential backoff
- D-11: Circuit breaker 5 failures / 30 second window
- D-12: Use `AddResiliencePipeline()` extension from Polly.Extensions
- D-13: Update `/health` endpoint to ping AviationStack
- D-14: Health check returns degraded if API unreachable
- D-15-17: All tests use Moq, no live API calls

### Claude's Discretion
- FlightInfo exact field mapping from AviationStack response
- How to handle multiple flights returned (AviationStack returns array)
- Health check implementation details (new class vs extend existing)

### Deferred Ideas
None.

## Architectural Responsibility Map

| Capability | Primary Tier | Secondary Tier | Rationale |
|------------|-------------|----------------|-----------|
| AviationStack API calls | Infrastructure | — | HTTP calls, resilience pipeline |
| FlightInfo model | Core | — | Shared DTO |
| FlightPlugin | Infrastructure | Core | [KernelFunction] + thin interface |
| Resilience pipeline | Infrastructure | — | Polly v8 / HttpClientFactory |
| Health check | Infrastructure | API | External API connectivity |
| User secrets | API | — | dotnet user-secrets at dev time |

## Standard Stack

### Core
| Library | Version | Purpose | Why Standard |
|---------|---------|---------|--------------|
| `Microsoft.Extensions.Http.Resilience` | Latest (9.x aligned) | Resilience for HttpClient | Official .NET 8+ package wrapping Polly v8 |
| `Polly` | 8.x (via above) | Retry, circuit breaker, timeout | Underlies Microsoft.Extensions.Http.Resilience |
| `Microsoft.Extensions.Caching.Memory` | 9.0.0 | Flight result caching | Already in project |

### Supporting
| Library | Version | Purpose | When to Use |
|---------|---------|---------|-------------|
| `Moq` | Latest | Mocking for tests | All service/plugin tests |
| `FluentAssertions` | Latest | Readable assertions | Test assertions |

### Alternatives Considered
| Instead of | Could Use | Tradeoff |
|------------|-----------|----------|
| `Microsoft.Extensions.Http.Resilience` | Raw Polly v8 with `AddResiliencePipeline()` | Microsoft package is officially recommended, better DI integration |
| AviationStack | AeroDataBox, FlightAware | AviationStack chosen per D-01; alternatives if API has issues |

**Installation:**
```bash
dotnet add package Microsoft.Extensions.Http.Resilience
```

## Architecture Patterns

### System Architecture Diagram

```
[API Layer]
    |
    |-- GET /health --> [Health Check] --> [ExternalApiHealthCheck]
    |                                      |
    |                                      v
    |                           [AviationStack API (httpbin fallback)]
    |
    |-- Semantic Kernel
            |
            v
    [FlightPlugin.GetFlightStatus(flight_number, date)]
            |
            v
    [IAviationStackService]
            |
            v
    [HttpClient + Resilience Pipeline]
            |
            v
    [AviationStack API: https://api.aviationstack.com/v1/flights]
```

### Recommended Project Structure
```
src/
├── FlightAgent.Core/
│   ├── Models/
│   │   └── FlightInfo.cs          # D-04: FlightInfo record
│   └── Interfaces/
│       ├── IFlightPlugin.cs       # Thin interface for Moq
│       └── IAviationStackService.cs # AviationStack API service interface
├── FlightAgent.Infrastructure/
│   ├── Plugins/
│   │   └── FlightPlugin.cs        # D-06: Concrete with [KernelFunction]
│   ├── Services/
│   │   └── AviationStackService.cs # D-05: API fetch + FlightInfo mapping
│   └── Health/
│       └── AviationStackHealthCheck.cs # D-13: AviationStack ping
```

### Pattern 1: HttpClientFactory + Microsoft.Extensions.Http.Resilience

The modern .NET 8+ approach replaces raw Polly with the Microsoft resilience layer:

```csharp
// In DependencyInjection.cs or Program.cs
services.AddHttpClient<IAviationStackService, AviationStackService>(client =>
{
    client.BaseAddress = new Uri("https://api.aviationstack.com/v1/");
})
.AddResilienceHandler("AviationStackPipeline", builder =>
{
    // D-10: Retry 3 attempts with exponential backoff
    builder.AddRetry(new HttpRetryStrategyOptions
    {
        MaxRetryAttempts = 3,
        BackoffType = DelayBackoffType.Exponential,
        UseJitter = true, // Recommended by Microsoft
        ShouldHandle = new PredicateBuilder<HttpResponseMessage>()
            .Handle<HttpRequestException>()
            .HandleResult(r => !r.IsSuccessStatusCode)
    });

    // D-09: Timeout 8 seconds per call
    builder.AddTimeout(TimeSpan.FromSeconds(8));

    // D-11: Circuit breaker 5 failures / 30 second recovery
    builder.AddCircuitBreaker(new HttpCircuitBreakerStrategyOptions
    {
        FailureRatio = 0.5,  // 50% failure rate triggers break
        SamplingDuration = TimeSpan.FromSeconds(30),
        MinimumThroughput = 5,  // Need at least 5 attempts
        BreakDuration = TimeSpan.FromSeconds(30)
    });
});
```

**Source:** [Microsoft Learn - Build resilient HTTP apps](https://learn.microsoft.com/en-us/dotnet/core/resilience/http-resilience)

### Pattern 2: AviationStack API Response Mapping

AviationStack `GET /v1/flights?flight_iata={flight_number}&flight_date={date}` returns:

```json
{
  "data": [{
    "flight_date": "2026-04-17",
    "flight_status": "active",
    "departure": {
      "iata": "JFK",
      "airport": "John F. Kennedy International",
      "terminal": "4", "gate": "B22",
      "delay": 15,
      "scheduled": "2026-04-17T08:00:00+00:00",
      "estimated": "2026-04-17T08:15:00+00:00",
      "actual": "2026-04-17T08:15:00+00:00"
    },
    "arrival": {
      "iata": "LAX",
      "airport": "Los Angeles International",
      "terminal": "5", "gate": "A12",
      "delay": 0,
      "scheduled": "2026-04-17T11:30:00+00:00"
    },
    "airline": { "name": "American Airlines", "iata": "AA" },
    "flight": { "number": "1004", "iata": "AA1004" }
  }]
}
```

Mapping to `FlightInfo`:
- `flight_number` = `flight.iata` ("AA1004") or combine `airline.iata` + `flight.number`
- `airline` = `airline.name`
- `departure_airport` = `departure.iata`
- `arrival_airport` = `arrival.iata`
- `scheduled_departure` = `departure.scheduled` (parse ISO 8601)
- `actual_departure` = `departure.actual` (nullable)
- `status` = map `flight_status`: "active"->"on_time", "scheduled"->"on_time", "delayed"->"delayed", "cancelled"->"cancelled"
- `delay_minutes` = `departure.delay` (0 if not present)

**Source:** [AviationStack Documentation](https://aviationstack.com/documentation)

### Pattern 3: FlightPlugin (following WeatherPlugin pattern)

```csharp
public class FlightPlugin : IFlightPlugin
{
    private readonly IAviationStackService _aviationStackService;

    public FlightPlugin(IAviationStackService aviationStackService)
    {
        _aviationStackService = aviationStackService;
    }

    [KernelFunction("get_flight_status")]
    [Description("Gets real-time status for a flight by its IATA flight number (e.g., AA1004, BA178). " +
        "Returns flight status (on_time/delayed/cancelled), departure/arrival times, delay minutes, and airport codes. " +
        "Call this when the user asks about a specific flight's status, delays, or arrival/departure times.")]
    public async Task<FlightInfo?> GetFlightStatusAsync(
        [Description("The IATA flight number including airline prefix (e.g., AA1004, KL071, BA178)")] string flightNumber,
        [Description("The flight date in YYYY-MM-DD format")] string date,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(flightNumber) || string.IsNullOrWhiteSpace(date))
            return null;

        return await _aviationStackService.GetFlightStatusAsync(flightNumber, date, cancellationToken);
    }
}
```

**Source:** WeatherPlugin.cs pattern (D-06, D-07 reference)

### Pattern 4: Health Check Extension

```csharp
// In DependencyInjection.cs - extend existing health checks
services.AddHealthChecks()
    .AddCheck<Health.ExternalApiHealthCheck>("httpbin", tags: new[] { "external" })
    .AddCheck<Health.AviationStackHealthCheck>("aviationstack", tags: new[] { "aviationstack" });

// AviationStackHealthCheck implementation
public class AviationStackHealthCheck : IHealthCheck
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IConfiguration _configuration;

    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken ct)
    {
        try
        {
            var apiKey = _configuration["AviationStack:ApiKey"];
            var url = $"https://api.aviationstack.com/v1/flights?access_key={apiKey}&flight_iata=AA100&limit=1";
            using var cts = CancellationTokenSource.CreateLinkedTokenSource(ct);
            cts.CancelAfter(TimeSpan.FromSeconds(5));

            var client = _httpClientFactory.CreateClient();
            var response = await client.GetAsync(url, cts.Token);

            if (response.IsSuccessStatusCode)
                return HealthCheckResult.Healthy("AviationStack API is reachable");

            return HealthCheckResult.Degraded($"AviationStack returned {response.StatusCode}");
        }
        catch (Exception ex)
        {
            return HealthCheckResult.Degraded($"AviationStack unreachable: {ex.Message}");
        }
    }
}
```

## Don't Hand-Roll

| Problem | Don't Build | Use Instead | Why |
|---------|-------------|-------------|-----|
| HTTP resilience | Custom retry/timeout/cb | `Microsoft.Extensions.Http.Resilience` | Battle-tested, Microsoft-maintained |
| JSON parsing | String manipulation | `System.Text.Json` with `JsonPropertyName` | Already in project |
| API key storage | Hardcode or config file | `dotnet user-secrets` | D-02 per decisions |

**Key insight:** Polly v8 in .NET 8+ is accessed via `Microsoft.Extensions.Http.Resilience`, not directly. The `AddResiliencePipeline()` mentioned in D-12 is a lower-level Polly API; the HttpClientFactory integration uses `AddResilienceHandler()`.

## Common Pitfalls

### Pitfall 1: Using `flight_number` instead of `flight_iata`
**What goes wrong:** AviationStack ignores `flight_number` parameter — must use `flight_iata` (includes airline prefix).
**Why it happens:** AviationStack documentation lists both parameters; `flight_number` alone returns no results.
**How to avoid:** Use `flight_iata={airline_iata}{flight_number}` e.g., "AA1004" not "1004".
**Warning signs:** Empty `data` array in response despite valid flight.

### Pitfall 2: Missing HTTPS
**What goes wrong:** Requests fail with 403 if using HTTP (per AviationStack docs, HTTPS is required on all plans).
**Why it happens:** D-03 says `http://` but official docs show HTTPS.
**How to avoid:** Use `https://api.aviationstack.com/v1/` — this is what official docs specify.

### Pitfall 3: Circuit breaker "advanced ratio-based" only
**What goes wrong:** Polly v8 circuit breaker is ratio-based, not consecutive-failure based.
**Why it happens:** Polly v8 removed the consecutive-failure breaker — only sampling-duration + failure-ratio approach.
**How to avoid:** Use `MinimumThroughput = 5` and `FailureRatio = 0.5` to approximate "5 failures triggers break".
**Warning signs:** Circuit never opens despite failures — check `MinimumThroughput` is met.

### Pitfall 4: API key in query string logged
**What goes wrong:** API key appears in server logs when appended to URL.
**Why it happens:** `?access_key=XXX` in URL gets logged.
**How to avoid:** Use `HttpClient.DefaultRequestHeaders` or `Authorization` header instead of query param. Check if AviationStack supports header auth.

### Pitfall 5: Multiple flights returned
**What goes wrong:** Same flight number on different days returns multiple results.
**Why it happens:** AviationStack returns all flights matching criteria.
**How to avoid:** Always filter by `flight_date` AND take first result, or use `limit=1`.

## Code Examples

### AviationStackService (interface + implementation)

```csharp
// Core/Interfaces/IAviationStackService.cs
public interface IAviationStackService
{
    Task<FlightInfo?> GetFlightStatusAsync(string flightNumber, string date, CancellationToken ct = default);
}

// Infrastructure/Services/AviationStackService.cs
public class AviationStackService : IAviationStackService
{
    private readonly HttpClient _httpClient;
    private readonly string _apiKey;

    public AviationStackService(HttpClient httpClient, IConfiguration configuration)
    {
        _httpClient = httpClient;
        _apiKey = configuration["AviationStack:ApiKey"] ?? throw new InvalidOperationException("AviationStack API key not configured");
    }

    public async Task<FlightInfo?> GetFlightStatusAsync(string flightNumber, string date, CancellationToken ct)
    {
        // flight_iata includes airline prefix (e.g., "AA1004" not "1004")
        var url = $"/flights?access_key={_apiKey}&flight_iata={flightNumber}&flight_date={date}&limit=1";

        var response = await _httpClient.GetAsync(url, ct);
        response.EnsureSuccessStatusCode();

        var json = await response.Content.ReadAsStringAsync(ct);
        var result = JsonSerializer.Deserialize<AviationStackResponse>(json);

        var flight = result?.Data?.FirstOrDefault();
        if (flight == null) return null;

        return MapToFlightInfo(flight);
    }

    private FlightInfo MapToFlightInfo(FlightData data) => new(
        FlightNumber: data.Flight.Iata,
        Airline: data.Airline.Name,
        DepartureAirport: data.Departure.Iata,
        ArrivalAirport: data.Arrival.Iata,
        ScheduledDeparture: DateTime.Parse(data.Departure.Scheduled),
        ActualDeparture: DateTime.TryParse(data.Departure.Actual, out var actual) ? actual : null,
        Status: MapStatus(data.FlightStatus),
        DelayMinutes: data.Departure.Delay
    );

    private static string MapStatus(string? status) => status switch
    {
        "active" or "scheduled" or "landed" => "on_time",
        "delayed" => "delayed",
        "cancelled" => "cancelled",
        _ => "unknown"
    };
}
```

### AviationStack API Response Models

```csharp
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
    public FlightInfo2 Flight { get; set; } = new();
}

public class AirportTimes
{
    [JsonPropertyName("iata")]
    public string Iata { get; set; } = "";

    [JsonPropertyName("airport")]
    public string Airport { get; set; } = "";

    [JsonPropertyName("terminal")]
    public string? Terminal { get; set; }

    [JsonPropertyName("gate")]
    public string? Gate { get; set; }

    [JsonPropertyName("delay")]
    public int Delay { get; set; }

    [JsonPropertyName("scheduled")]
    public string Scheduled { get; set; } = "";

    [JsonPropertyName("estimated")]
    public string Estimated { get; set; } = "";

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

public class FlightInfo2
{
    [JsonPropertyName("number")]
    public string Number { get; set; } = "";

    [JsonPropertyName("iata")]
    public string Iata { get; set; } = "";
}
```

## Assumptions Log

| # | Claim | Section | Risk if Wrong |
|---|-------|---------|---------------|
| A1 | AviationStack uses `flight_iata` parameter (includes airline prefix) | Pattern 2 | API returns empty results |
| A2 | HTTPS is required (D-03 says http:// but docs say https://) | Architecture | 403 errors from API |
| A3 | D-12 "AddResiliencePipeline() from Polly.Extensions" refers to Microsoft.Extensions.Http.Resilience AddResilienceHandler | Standard Stack | Package reference would be wrong |
| A4 | AviationStack accepts `flight_date` as YYYY-MM-DD query param for filtering | Pattern 2 | No filtering, multiple results |
| A5 | API key via user-secrets maps to `configuration["AviationStack:ApiKey"]` | Implementation | Null reference if key missing |

**If this table is empty:** All claims in this research were verified or cited — no user confirmation needed.

## Open Questions

1. **D-03 URL protocol conflict**
   - What we know: D-03 says `http://api.aviationstack.com/v1/` but official docs show HTTPS required on all plans.
   - What's unclear: Whether the `http://` in decisions is a typo or intentional for some reason.
   - Recommendation: Use HTTPS — this is what the official documentation specifies and what actually works.

2. **Flight number format for LLM prompts**
   - What we know: AviationStack needs `flight_iata` like "AA1004".
   - What's unclear: How to handle cases where user provides just "1004" without airline code.
   - Recommendation: Require full flight number (airline + number) in `[Description]` for LLM.

3. **Multiple results handling**
   - What we know: AviationStack can return multiple flights (different days, codeshares).
   - What's unclear: Whether to return first match, most recent, or error on multiple.
   - Recommendation: Use `limit=1` and return first result; log warning if multiple received.

## Environment Availability

> Step 2.6: SKIPPED (no external dependencies identified beyond existing project tools)

The phase uses only NuGet packages already available in the project. No external tools, services, or CLIs beyond the AviationStack API itself (which is the subject of the implementation).

| Dependency | Required By | Available | Version | Fallback |
|------------|------------|-----------|---------|----------|
| dotnet CLI | Build, test | Yes | 10.x | — |
| AviationStack API | Flight data | Internet required | v1 | Return null/error gracefully |

**Missing dependencies with no fallback:**
- None blocking.

**Missing dependencies with fallback:**
- None identified.

## Validation Architecture

### Test Framework
| Property | Value |
|----------|-------|
| Framework | xUnit + Moq + FluentAssertions (standard .NET test stack) |
| Config file | `tests/FlightAgent.Infrastructure.Tests/` |
| Quick run command | `dotnet test --filter "FlightPlugin|AviationStackService" --verbosity normal` |
| Full suite command | `dotnet test` |

### Phase Requirements -> Test Map
| Req ID | Behavior | Test Type | Automated Command | File Exists? |
|--------|----------|-----------|-------------------|-------------|
| REQ-002 | AviationStackService fetches and maps flight status | Unit | `dotnet test AviationStackServiceTests.cs` | No — needs creation |
| REQ-002 | FlightPlugin returns FlightInfo from service | Unit | `dotnet test FlightPluginTests.cs` | No — needs creation |
| REQ-004 | Resilience pipeline handles timeouts | Unit | `dotnet test ResilienceTests.cs` | No — needs creation |
| REQ-004 | Circuit breaker opens after failures | Unit | `dotnet test CircuitBreakerTests.cs` | No — needs creation |

### Sampling Rate
- **Per task commit:** Quick run command with filter
- **Per wave merge:** Full suite command
- **Phase gate:** Full suite green before `/gsd-verify-work`

### Wave 0 Gaps
- [ ] `tests/FlightAgent.Infrastructure.Tests/AviationStackServiceTests.cs` — covers REQ-002 (API mapping)
- [ ] `tests/FlightAgent.Infrastructure.Tests/FlightPluginTests.cs` — covers REQ-002 (plugin method)
- [ ] `tests/FlightAgent.Infrastructure.Tests/ResilienceTests.cs` — covers REQ-004 (timeout/retry/circuit)
- [ ] `tests/FlightAgent.Infrastructure.Tests/Usings.cs` or `GlobalUsings.cs` — FluentAssertions setup

*(If no gaps: "None — existing test infrastructure covers all phase requirements")*

## Security Domain

### Applicable ASVS Categories

| ASVS Category | Applies | Standard Control |
|---------------|---------|-----------------|
| V2 Authentication | No | N/A — API key (not user auth) |
| V3 Session Management | No | N/A |
| V4 Access Control | No | N/A |
| V5 Input Validation | Yes | Validate flight_number format (non-empty, alphanumeric), date format (YYYY-MM-DD) |
| V6 Cryptography | Yes | AviationStack requires HTTPS (TLS 1.2+) |

### Known Threat Patterns for .NET HttpClient + External API

| Pattern | STRIDE | Standard Mitigation |
|---------|--------|---------------------|
| API key exposure in logs | Information Disclosure | Use header auth or HttpClient default headers; avoid query string |
| API key in user-secrets | Secrets Management | dotnet user-secrets (D-02), not source control |
| DOS via infinite retry | Denial of Service | Timeout (8s) + circuit breaker prevents runaway |
| Sensitive data in error messages | Information Disclosure | Sanitize exception messages before returning to client |

## Sources

### Primary (HIGH confidence)
- [Microsoft Learn - Build resilient HTTP apps](https://learn.microsoft.com/en-us/dotnet/core/resilience/http-resilience) — Official .NET resilience documentation
- [AviationStack Documentation](https://aviationstack.com/documentation) — API endpoint structure, parameters, response format
- [Polly v8 Migration Guide](https://www.pollydocs.org/migration-v8.html) — Policy to Strategy terminology, configuration changes

### Secondary (MEDIUM confidence)
- [GitHub - dotnet/extensions - Microsoft.Extensions.Http.Resilience README](https://github.com/dotnet/extensions/blob/main/src/Libraries/Microsoft.Extensions.Http.Resilience/README.md) — Package details
- [Polly GitHub Migration v8](https://github.com/App-vNext/Polly/blob/main/docs/migration-v8.md) — Polly v7 to v8 changes

### Tertiary (LOW confidence)
- [Blog: Using HttpClientFactory with Polly v8](https://anktsrkr.github.io/post/use-httpclientfactory-with-pollyv8-to-implement-resilient-http-requests/) — Community example, not official docs
- [AviationStack FAQ](https://aviationstack.com/faq) — Pricing, rate limits

## Metadata

**Confidence breakdown:**
- Standard stack: MEDIUM-HIGH — Microsoft.Extensions.Http.Resilience is verified; specific version needs `dotnet add package` to confirm latest
- Architecture: HIGH — WeatherPlugin/AirportPlugin patterns confirmed in codebase
- Pitfalls: MEDIUM-HIGH — AviationStack API details from official docs; some edge cases need validation

**Research date:** 2026-04-17
**Valid until:** 2026-05-17 (API docs stable, .NET resilience patterns mature)
