# Phase 2: Plugins Foundation - Context

**Gathered:** 2026-04-11
**Status:** Ready for planning

<domain>
## Phase Boundary

Implement AirportPlugin and WeatherPlugin with hardcoded and external data. These plugins will be registered with the Semantic Kernel and called by the LLM via [KernelFunction] attributes. The plugins are the core of the entire project — they're what the LLM calls to provide flight-related information.

</domain>

<decisions>
## Implementation Decisions

### Plugin Architecture Pattern
- **D-01:** No common `IPlugin` interface. Each plugin is a plain C# class with `[KernelFunction]` methods.
- **D-02:** Use separate specific interfaces (`IAirportPlugin`, `IWeatherPlugin`) for testability via Moq.
- **D-03:** No abstract base class — Semantic Kernel reflects on concrete class methods, not interfaces.
- **D-04:** `[KernelFunction]` and `[Description]` attributes stay on concrete classes only.

### Airport Data Scope
- **D-05:** Hardcoded dataset of 50 major global airport hubs.
- **D-06:** Fields per airport: IATA code, name, city, country, latitude, longitude, timezone.
- **D-07:** Store as `Dictionary<string, AirportInfo>` where key is IATA code.
- **D-08:** AirportInfo record type:
  ```csharp
  public record AirportInfo(string IataCode, string Name, string City, string Country, double Lat, double Lon, string Timezone);
  ```
- **D-09:** Coordinates are mandatory — WeatherPlugin needs them to call Open-Meteo.

### Weather Data Details
- **D-10:** Use Open-Meteo API (free, no API key required).
- **D-11:** Endpoint: `/v1/forecast` with `current=temperature_2m,weathercode,windspeed_10m,winddirection_10m`.
- **D-12:** Current weather only — no forecast in v1.
- **D-13:** Weather fields: temperature, conditions (WMO weather code mapped to human-readable string), wind speed, wind direction.
- **D-14:** WeatherPlugin depends on AirportPlugin data (coordinates lookup by IATA code).

### Caching Strategy
- **D-15:** Use `IMemoryCache` (Microsoft.Extensions.Caching.Memory, in-box).
- **D-16:** Cache per airport code with 15-minute absolute expiry.
- **D-17:** Cache flow: check cache → hit: return cached data → miss: call API → cache result → return.
- **D-18:** Mid-request expiry is correct behavior — fetch fresh data, not a failure case.

### Error Handling
- **D-19:** Airport code not found → return `null` from method.
- **D-20:** Let the LLM communicate "I don't have information for that airport code" gracefully.
- **D-21:** Weather API down → Polly handles retries; after exhaustion, let exception bubble.
- **D-22:** No Result pattern for v1 — use clean null returns and Polly resilience.
- **D-23:** No "return stale on failure" path for v1.

### Plugin Registration
- **D-24:** Use extension method pattern like existing `AddInfrastructure()`.
- **D-25:** Create `AddFlightAgentPlugins()` extension method in Infrastructure project.
- **D-26:** Two-step registration:
  1. Register plugin classes in DI: `services.AddSingleton<IAirportPlugin, AirportPlugin>()`
  2. Register with SK Kernel: `kernel.Plugins.AddFromObject(sp.GetRequiredService<AirportPlugin>())`
- **D-27:** SK-specific wiring in `Program.cs` or dedicated extension — Infrastructure shouldn't hardcode Kernel dependencies.
- **D-28:** Resolve concrete type from DI when registering with SK, not the interface (SK needs concrete for reflection).

### Interface Design
- **D-29:** Thin interfaces matching public method signatures:
  ```csharp
  public interface IAirportPlugin
  {
      Task<string> GetAirportInfoAsync(string iataCode, CancellationToken ct = default);
  }

  public interface IWeatherPlugin
  {
      Task<string> GetAirportWeatherAsync(string iataCode, CancellationToken ct = default);
  }
  ```
- **D-30:** Interfaces are for DI and Moq only — SK doesn't use them.

### Claude's Discretion
- Exact weather data formatting string (human-readable output from plugin).
- Airport selection for the 50 major hubs (common international airports).
- Open-Meteo API response parsing implementation details.
- MemoryCache configuration specifics (size limits, compaction).

</decisions>

<specifics>
## Specific Ideas

- Plugins are the core of the project — what the LLM calls and what interviewers will examine first.
- Adding interfaces now costs 10 minutes but provides clean Moq-ready seams from the start.
- Avoid refactoring working code in Phase 6 just to add testability — do it right in Phase 2.
- WeatherPlugin internally depends on AirportPlugin data for coordinates lookup.
- Open-Meteo's `/v1/forecast` endpoint with current weather parameters covers all requirements.

</specifics>

<canonical_refs>
## Canonical References

### Plugin Requirements
- `.planning/REQUIREMENTS.md` §REQ-006 — Weather Plugin: Open-Meteo API, 15-minute cache, temperature/conditions/visibility
- `.planning/REQUIREMENTS.md` §REQ-007 — Airport Plugin: hardcoded dataset, IATA code lookup, timezone info

### Semantic Kernel Integration
- `.planning/REQUIREMENTS.md` §REQ-003 — Microsoft Agent Framework for NLU capabilities
- Existing `DependencyInjection.cs` — Pattern for DI registration via extension methods
- Existing `SemanticFlightSearchService.cs` — Example of Kernel injection in constructor

### Existing Code Patterns
- `src/FlightAgent.Infrastructure/DependencyInjection.cs` — Extension method pattern for service registration
- `src/FlightAgent.Core/Interfaces/` — Interface location pattern
- `src/FlightAgent.Core/Models/` — Model/record type location pattern

</canonical_refs>

<code_context>
## Existing Code Insights

### Reusable Assets
- `DependencyInjection.cs` pattern: Can create `AddFlightAgentPlugins()` following same pattern
- `FlightAgent.Core/Interfaces/` location: Add `IAirportPlugin` and `IWeatherPlugin` here
- `FlightAgent.Core/Models/` location: Add `AirportInfo` record here
- Existing `IMemoryCache` registration: Available via `AddInfrastructure()` already calls `AddHttpClient()`

### Established Patterns
- **Extension method registration**: Services registered via `AddInfrastructure()` extension method
- **DI scoping**: Services use appropriate lifetime (Singleton for plugins)
- **Semantic Kernel**: Kernel injected via constructor, plugins registered with `kernel.Plugins.AddFromObject()`
- **Health checks**: Pattern exists for external API connectivity checks

### Integration Points
- **AirportPlugin** → Core.Models.AirportInfo (new)
- **WeatherPlugin** → AirportPlugin (for coordinates lookup)
- **DI Registration** → Infrastructure.DependencyInjection (extension method)
- **SK Registration** → API.Program.cs (kernel.Plugins.AddFromObject calls)

</code_context>

<deferred>
## Deferred Ideas

- Weather forecast data (multi-day) — out of scope for v1, consider for v2
- Airport fuzzy search by city name — Phase 2 supports IATA only, city search could be future enhancement
- Additional weather fields (humidity, pressure, UV index) — not required per REQ-006
- Plugin versioning or dynamic loading — not needed for v1 static plugins
- Database persistence for airport data — explicitly excluded per REQUIREMENTS.md

</deferred>

---

*Phase: 02-plugins-foundation*
*Context gathered: 2026-04-11*
