# Codebase Concerns

**Analysis Date:** 2026-03-24

## Tech Debt

**Solution File Configuration:**
- Issue: The `FlightAgent.sln` references an old project path `FlightAgent.csproj` at the root, but the actual project has been restructured into a Clean Architecture layout under `src/`
- Files: `D:/Project/FlightAgent/FlightAgent.sln`
- Impact: Solution file is out of sync with actual project structure; building from solution may fail
- Fix approach: Recreate solution file to reference the three new project files:
  - `src/FlightAgent.Api/FlightAgent.Api.csproj`
  - `src/FlightAgent.Core/FlightAgent.Core.csproj`
  - `src/FlightAgent.Infrastructure/FlightAgent.Infrastructure.csproj`

**Stubbed Infrastructure Services:**
- Issue: Both infrastructure services use mock data and in-memory storage instead of real integrations
- Files:
  - `D:/Project/FlightAgent/src/FlightAgent.Infrastructure/Services/SemanticFlightSearchService.cs` (lines 28-29, 51-76)
  - `D:/Project/FlightAgent/src/FlightAgent.Infrastructure/Services/SemanticBookingService.cs` (lines 10, 39)
- Impact: Application cannot persist bookings or search real flight data; data is lost on restart
- Fix approach: Implement actual database persistence and airline API integrations

**Unconfigured AI Service:**
- Issue: Semantic Kernel is instantiated but not configured with any AI service provider
- Files: `D:/Project/FlightAgent/src/FlightAgent.Infrastructure/DependencyInjection.cs` (lines 11-25)
- Impact: AI-powered features will fail at runtime; the commented-out configuration shows no provider is wired up
- Fix approach: Configure Azure OpenAI or OpenAI provider with actual API keys and endpoints

## Known Bugs

**Unimplemented Flight Search Result Parsing:**
- Symptoms: Flight search calls Semantic Kernel but ignores the AI result and returns hardcoded mock data
- Files: `D:/Project/FlightAgent/src/FlightAgent.Infrastructure/Services/SemanticFlightSearchService.cs` (lines 26-29)
- Trigger: Calling `SearchFlightsAsync` with any parameters
- Current mitigation: Mock data generation provides consistent test responses

**Missing Booking Validation:**
- Symptoms: `CreateBookingAsync` does not verify if the flight exists or has available seats
- Files: `D:/Project/FlightAgent/src/FlightAgent.Infrastructure/Services/SemanticBookingService.cs` (lines 17-41)
- Trigger: Creating a booking for any flight number
- Current mitigation: None; accepts any flight number without validation

## Security Considerations

**No API Authentication:**
- Risk: All endpoints are completely open; no authentication or authorization configured
- Files: `D:/Project/FlightAgent/src/FlightAgent.Api/Program.cs`
- Current mitigation: None
- Recommendations: Add JWT Bearer authentication, API key validation, or OAuth2 integration

**Missing Input Validation:**
- Risk: Route parameters and request bodies are not validated
- Files:
  - `D:/Project/FlightAgent/src/FlightAgent.Core/Models/Flight.cs`
  - `D:/Project/FlightAgent/src/FlightAgent.Core/Models/Booking.cs`
- Current mitigation: None
- Recommendations: Add FluentValidation or DataAnnotations; validate email format, required fields, price ranges

**Hardcoded Price Calculation:**
- Risk: Pricing logic is embedded in code and not configurable
- Files: `D:/Project/FlightAgent/src/FlightAgent.Infrastructure/Services/SemanticBookingService.cs` (lines 59-65)
- Current mitigation: None
- Recommendations: Move pricing to database or external configuration service

## Performance Bottlenecks

**In-Memory Booking Storage:**
- Problem: Bookings stored in a `List<Booking>` with linear search
- Files: `D:/Project/FlightAgent/src/FlightAgent.Infrastructure/Services/SemanticBookingService.cs` (lines 10, 45-46, 51)
- Cause: `FirstOrDefault` on List is O(n); will degrade with scale
- Improvement path: Replace with database-backed repository with indexed queries

**No Caching:**
- Problem: Flight searches and bookings have no caching layer
- Files: Infrastructure service layer
- Cause: Every request hits the mock generator or would hit external APIs
- Improvement path: Add Redis or in-memory caching for flight search results

## Fragile Areas

**Enum Duplication:**
- Files:
  - `D:/Project/FlightAgent/src/FlightAgent.Core/Models/Booking.cs` (lines 22-27, 29-35)
  - `D:/Project/FlightAgent/src/FlightAgent.Core/Interfaces/IFlightSearchService.cs` (lines 16-21)
- Why fragile: `SeatClass` enum is duplicated between interface and model file
- Safe modification: Consolidate enums into Core models and reference from interfaces
- Test coverage: No unit tests exist to catch enum mismatch issues

**Sample Weather Endpoint in Production:**
- Files: `D:/Project/FlightAgent/src/FlightAgent.Api/Program.cs` (lines 17-34)
- Why fragile: Sample `WeatherForecast` endpoint still active; uses `Random.Shared` which is not thread-safe for cryptographic purposes
- Safe modification: Remove before production or move to debug-only compilation
- Test coverage: No integration tests for actual API endpoints

## Scaling Limits

**In-Memory State:**
- Current capacity: Unlimited bookings in memory (until OOM)
- Limit: Process memory; data lost on restart
- Scaling path: Move to persistent database (SQL Server, PostgreSQL, Cosmos DB)

**Single Instance:**
- Current capacity: Single server instance
- Limit: No horizontal scaling support
- Scaling path: Add distributed caching and stateless service design

## Dependencies at Risk

**.NET 10 Preview:**
- Risk: Project targets `net10.0` which is not yet released/stable
- Impact: Build/runtime issues on machines without .NET 10 SDK preview installed
- Migration plan: Consider downgrading to .NET 8 (LTS) for production stability

**SemanticKernel 1.40.0:**
- Risk: Rapidly evolving library; APIs may change
- Impact: Breaking changes in future versions
- Migration plan: Pin version and review release notes before upgrading

## Missing Critical Features

**No Database:**
- Problem: No Entity Framework or other ORM configured
- Blocks: Production deployment, data persistence, multi-instance scaling

**No Real Flight API Integration:**
- Problem: No connectors to airline reservation systems (Amadeus, Sabre, etc.)
- Blocks: Actual flight search functionality

**No Email/Notification Service:**
- Problem: Booking confirmations cannot be sent
- Blocks: Complete booking workflow

**No Logging Framework:**
- Problem: Only basic console logging configured in `appsettings.json`
- Blocks: Production monitoring and troubleshooting

## Test Coverage Gaps

**No Test Projects:**
- What's not tested: Entire application has zero test coverage
- Files: All source files in `src/` directory
- Risk: Any change could break functionality without detection
- Priority: High

**Untested Service Layer:**
- What's not tested:
  - `SemanticFlightSearchService` logic
  - `SemanticBookingService` CRUD operations
  - Dependency injection configuration
- Files:
  - `D:/Project/FlightAgent/src/FlightAgent.Infrastructure/Services/SemanticFlightSearchService.cs`
  - `D:/Project/FlightAgent/src/FlightAgent.Infrastructure/Services/SemanticBookingService.cs`
  - `D:/Project/FlightAgent/src/FlightAgent.Infrastructure/DependencyInjection.cs`
- Risk: Core business logic has no regression protection
- Priority: High

**Untyped API Responses:**
- What's not tested: The `WeatherForecast` endpoint returns anonymous records with no contract testing
- Files: `D:/Project/FlightAgent/src/FlightAgent.Api/Program.cs`
- Risk: API contract changes break consumers silently
- Priority: Medium

---

*Concerns audit: 2026-03-24*
