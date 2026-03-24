# Architecture

**Analysis Date:** 2026-03-24

## Pattern Overview

**Overall:** Clean Architecture (Layered)

**Key Characteristics:**
- Dependency direction flows inward: Api -> Infrastructure -> Core
- Core project has no external dependencies (pure domain models)
- Infrastructure depends only on Core (implements interfaces)
- Api depends on both Core and Infrastructure
- Uses Minimal APIs pattern for endpoint registration
- Integrates Microsoft Semantic Kernel for AI-powered operations

## Layers

**Core Layer (Domain):**
- Purpose: Contains pure domain models and service contracts
- Location: `src/FlightAgent.Core/`
- Contains: Entities, value objects, enums, interfaces
- Depends on: Nothing (zero external dependencies)
- Used by: Infrastructure, Api

**Infrastructure Layer:**
- Purpose: Implements domain interfaces using external services
- Location: `src/FlightAgent.Infrastructure/`
- Contains: Service implementations, external SDK integration
- Depends on: `FlightAgent.Core`
- Used by: Api

**Api Layer (Presentation):**
- Purpose: HTTP interface, minimal API endpoints, OpenAPI documentation
- Location: `src/FlightAgent.Api/`
- Contains: Endpoint definitions, DI configuration, middleware
- Depends on: `FlightAgent.Core`, `FlightAgent.Infrastructure`
- Used by: External clients (browsers, mobile apps, other services)

## Data Flow

**Flight Search Flow:**
1. HTTP Request -> Minimal API Endpoint (`Program.cs`)
2. `IFlightSearchService.SearchFlightsAsync()` called
3. `SemanticFlightSearchService` implementation invoked
4. Uses `Microsoft.SemanticKernel.Kernel` for AI processing
5. Returns `IEnumerable<Flight>` to caller

**Booking Flow:**
1. HTTP Request -> Minimal API Endpoint
2. `IBookingService.CreateBookingAsync()` called
3. `SemanticBookingService` implementation invoked
4. Processes booking with Semantic Kernel validation
5. Stores in-memory (currently no persistence layer)
6. Returns `Booking` entity

**State Management:**
- In-memory state only (no database configured)
- `SemanticBookingService` uses `List<Booking>` for bookings
- Services are registered as Scoped (`AddScoped`)
- Semantic Kernel registered as Singleton

## Key Abstractions

**Service Interfaces (Contracts):**
- Purpose: Define service contracts in Core, implement in Infrastructure
- Examples: `src/FlightAgent.Core/Interfaces/IFlightSearchService.cs`
- Pattern: Interface segregation with focused method sets

**Domain Models:**
- Purpose: Plain C# classes representing flight/booking entities
- Examples: `src/FlightAgent.Core/Models/Flight.cs`, `src/FlightAgent.Core/Models/Booking.cs`
- Pattern: Mutable classes with init-only setters, default values

**Semantic Kernel Integration:**
- Purpose: AI-powered flight search and booking validation
- Pattern: Constructor-injected `Kernel` instance
- Examples: `SemanticFlightSearchService`, `SemanticBookingService`

## Entry Points

**Application Entry:**
- Location: `src/FlightAgent.Api/Program.cs`
- Triggers: `dotnet run` or IIS/kestrel
- Responsibilities: Service registration, middleware pipeline, endpoint mapping

**Service Registration:**
- Location: `src/FlightAgent.Infrastructure/DependencyInjection.cs`
- Method: `AddInfrastructure()` extension method
- Triggers: Called from `Program.cs` during startup
- Responsibilities: Semantic Kernel setup, service registrations

## Error Handling

**Strategy:** Not explicitly implemented

**Patterns:**
- Services return nullable types (e.g., `Task<Flight?>`) for not-found scenarios
- No global exception handling middleware configured
- No custom exception types defined

## Cross-Cutting Concerns

**Logging:** Microsoft.Extensions.Logging (default ASP.NET Core)
- Configured in `appsettings.json`
- No custom logging abstractions

**Validation:** Not implemented
- No validation attributes on models
- No FluentValidation or similar frameworks

**Authentication:** Not implemented
- No JWT, OAuth, or session handling

**OpenAPI/Swagger:**
- Configured in `Program.cs`
- Available at `/openapi/v1.json` in development
- Uses `Microsoft.AspNetCore.OpenApi` package

---

*Architecture analysis: 2026-03-24*
