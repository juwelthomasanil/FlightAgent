# Phase 3: Flight Search Service - Context

**Gathered:** 2026-04-16
**Status:** Ready for planning

<domain>
## Phase Boundary

Implement AviationStack API integration for real-time flight data. Phase 2 plugins (AirportPlugin, WeatherPlugin) already exist. This phase adds FlightPlugin with real API calls, resilience patterns, and health check verification. No LLM dependency in tests.

</domain>

<decisions>
## Implementation Decisions

### API Integration
- **D-01:** AviationStack API for flight data (real-time status, delays)
- **D-02:** API key stored in .NET user secrets (`dotnet user-secrets`)
- **D-03:** Base URL: `http://api.aviationstack.com/v1/` (or https per API docs)

### FlightInfo Model
- **D-04:** `FlightInfo` record with: flight_number, airline, departure_airport, arrival_airport, scheduled_departure, actual_departure, status (on_time/delayed/cancelled), delay_minutes
- **D-05:** AviationStackService fetches and maps API response to FlightInfo

### FlightPlugin
- **D-06:** Concrete class with `[KernelFunction]` attributes
- **D-07:** `[Description]` must be precise/prompt-engineered for LLM — describe exactly what the function returns and when to call it
- **D-08:** Methods: `GetFlightStatus(flight_number, date)` returning FlightInfo

### Resilience Pipeline (Polly v8)
- **D-09:** Timeout: 8 seconds per call
- **D-10:** Retry: 3 attempts with exponential backoff
- **D-11:** Circuit breaker: 5 failures / 30 second recovery window
- **D-12:** Use `AddResiliencePipeline()` extension from Polly.Extensions

### Health Check
- **D-13:** Update `/health` endpoint to ping AviationStack API
- **D-14:** Health check returns degraded if API unreachable

### Testing
- **D-15:** All tests use Moq — no live API calls, no LLM dependency
- **D-16:** Test AviationStackService with mocked HTTP responses
- **D-17:** Test FlightPlugin by mocking the service interface

</decisions>

<canonical_refs>
## Canonical References

**Downstream agents MUST read these before planning or implementing.**

- `.planning/phases/02-plugins-foundation/02-CONTEXT.md` — Plugin architecture patterns, registration
- `.planning/ROADMAP.md` — Phase 3 goal and success criteria
- `src/Plugins/` — Existing plugin implementations for reference

</canonical_refs>

<code_context>
## Existing Code Insights

### Reusable Assets
- `AirportPlugin`: concrete class with `[KernelFunction]`, registration via extension method
- `WeatherPlugin`: `IMemoryCache` for caching, `HttpClient` for API calls
- `IWeatherService` / `IAirportService` interfaces: thin interfaces for Moq testing

### Established Patterns
- Extension method for DI registration: `services.AddAirportPlugin()`
- Two-step SK registration: `kernel.Plugins.AddFromType<AirportPlugin>();`
- Caching via `IMemoryCache` with absolute expiry

### Integration Points
- FlightPlugin must follow same registration pattern as AirportPlugin/WeatherPlugin
- Health check already exists at `/health` — needs AviationStack ping added

</code_context>

<specifics>
## Specific Ideas

- AviationStack chosen over other flight APIs for pricing/freemium model
- Circuit breaker prevents cascade failures if AviationStack goes down
- LLM-free tests critical for CI/CD and offline development

</specifics>

<deferred>
## Deferred Ideas

None — all decisions captured above.

</deferred>

---

*Phase: 03-flight-search-service*
*Context gathered: 2026-04-16*
