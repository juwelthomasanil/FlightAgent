Phase 3 plan solid. Three additions from Phase 2 learnings not in the doc:
Add explicitly:

IFlightPlugin interface — Phase 2 has IAirportPlugin, IWeatherPlugin. Consistency demands IFlightPlugin. Dual registration pattern same as before:

csharpservices.AddSingleton<IFlightPlugin, FlightPlugin>();
kernel.Plugins.AddFromObject(sp.GetRequiredService<FlightPlugin>());

IHttpClientFactory from day one — Phase 2 bug was raw HttpClient singleton in WeatherPlugin. AviationStackService must use IHttpClientFactory from commit 03-02, not fixed later in review.
Null guard + InvariantCulture in 03-02 — flightNumber needs ArgumentNullException.ThrowIfNullOrWhiteSpace at entry. If URL builds with any doubles (lat/lon), InvariantCulture required — same locale bug risk.

Commit 03-02 scope expands slightly:

FlightInfo model
AviationStackService with IHttpClientFactory
IFlightPlugin interface
Null guards baked in

Everything else in Phase 3 doc stands. 5 commits, correct scope boundary (no Agent Framework wiring), Polly in 03-04, health check real ping in 03-05.

Yes. Fix two real bugs before discuss.
Bug 1 — null cached for 15min (WeatherPlugin ~L103):
csharp// WRONG — caches null, blocks retries
var weather = await FetchWeatherAsync(...);
_cache.Set(cacheKey, weather, TimeSpan.FromMinutes(15));

// FIX — only cache on success
var weather = await FetchWeatherAsync(...);
if (weather != null)
    _cache.Set(cacheKey, weather, TimeSpan.FromMinutes(15));
return weather;
Bug 2 — GetAsync no timeout (WeatherPlugin ~L119):
csharp// FIX — pass CancellationToken with timeout
using var cts = CancellationTokenSource.CreateLinkedTokenSource(ct);
cts.CancelAfter(TimeSpan.FromSeconds(8));
var response = await _httpClient.GetAsync(url, cts.Token);
Test smell — swallowed HttpRequestException (~L79-108):
Not a blocker but note for Phase 3 test standards — exceptions must assert, not silently pass.
Fix both bugs → commit:
fix(02-06): cache null guard and timeout in WeatherPlugin


Plan split is good. Two checks:
Correct:

flight_iata param — right, not flight_number
HTTPS URL — required by AviationStack free tier
Microsoft.Extensions.Http.Resilience — correct package for .NET 10
Ratio-based circuit breaker — correct Polly v8 API

Verify before execute:
03-02 is interface only — confirm 03-03 has IHttpClientFactory injection, not raw HttpClient. Phase 2 bug must not repeat.
03-06 DI registration — confirm it includes:
csharpservices.AddSingleton<IFlightPlugin, FlightPlugin>();
kernel.Plugins.AddFromObject(sp.GetRequiredService<FlightPlugin>());
Both lines. Same dual-registration pattern as AirportPlugin/WeatherPlugin.
03-07 tests — confirm flightNumber null guard test exists. Caveman found WeatherPlugin missing it pre-fix.
03-08 resilience tests — confirm circuit breaker test simulates 5 failures, verifies breaker opens. Not just "Polly configured" assertion.
One gap: User secrets setup (AviationStack:ApiKey) not in any plan. Should be 03-01 or a pre-step. Phase 3 fails on first run without it.
Add to 03-01: dotnet user-secrets set "AviationStack:ApiKey" "...".
Otherwise solid. Execute.