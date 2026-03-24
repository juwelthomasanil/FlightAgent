# Codebase Structure

**Analysis Date:** 2026-03-24

## Directory Layout

```
D:/Project/FlightAgent/
├── FlightAgent.sln              # Solution file (single project reference)
├── src/
│   ├── FlightAgent.Api/         # Presentation layer (HTTP endpoints)
│   │   ├── Program.cs           # Entry point, minimal APIs
│   │   ├── FlightAgent.Api.csproj
│   │   ├── appsettings.json     # Production config
│   │   ├── appsettings.Development.json
│   │   └── Properties/
│   │       └── launchSettings.json
│   ├── FlightAgent.Core/        # Domain layer (models, interfaces)
│   │   ├── FlightAgent.Core.csproj
│   │   ├── Interfaces/
│   │   │   └── IFlightSearchService.cs
│   │   └── Models/
│   │       ├── Flight.cs
│   │       └── Booking.cs
│   └── FlightAgent.Infrastructure/  # Data/service layer
│       ├── FlightAgent.Infrastructure.csproj
│       ├── DependencyInjection.cs   # DI registration extension
│       └── Services/
│           ├── SemanticFlightSearchService.cs
│           └── SemanticBookingService.cs
├── tests/
│   └── FlightAgent.Tests/       # Empty test project directory
└── .planning/codebase/          # This documentation
```

## Directory Purposes

**src/FlightAgent.Api:**
- Purpose: HTTP interface, minimal API endpoints
- Contains: `Program.cs`, configuration, launch settings
- Key files: `src/FlightAgent.Api/Program.cs`
- Depends on: Core, Infrastructure

**src/FlightAgent.Core:**
- Purpose: Domain models and service contracts
- Contains: Entities, enums, interfaces
- Key files: `src/FlightAgent.Core/Models/Flight.cs`, `src/FlightAgent.Core/Interfaces/IFlightSearchService.cs`
- Depends on: Nothing (zero dependencies)

**src/FlightAgent.Infrastructure:**
- Purpose: Service implementations, external integrations
- Contains: Semantic Kernel services, DI configuration
- Key files: `src/FlightAgent.Infrastructure/Services/SemanticFlightSearchService.cs`, `src/FlightAgent.Infrastructure/DependencyInjection.cs`
- Depends on: Core

**tests/FlightAgent.Tests:**
- Purpose: Test projects (currently empty)
- Contains: None

## Key File Locations

**Entry Points:**
- `src/FlightAgent.Api/Program.cs`: Application startup, minimal API registration

**Configuration:**
- `src/FlightAgent.Api/appsettings.json`: Logging, allowed hosts
- `src/FlightAgent.Api/appsettings.Development.json`: Development overrides
- `src/FlightAgent.Api/Properties/launchSettings.json`: HTTP/HTTPS URLs

**Core Logic:**
- `src/FlightAgent.Core/Models/Flight.cs`: Flight entity definition
- `src/FlightAgent.Core/Models/Booking.cs`: Booking, Passenger entities
- `src/FlightAgent.Core/Interfaces/IFlightSearchService.cs`: Service contracts

**Infrastructure:**
- `src/FlightAgent.Infrastructure/DependencyInjection.cs`: DI registration
- `src/FlightAgent.Infrastructure/Services/SemanticFlightSearchService.cs`: Flight search implementation
- `src/FlightAgent.Infrastructure/Services/SemanticBookingService.cs`: Booking implementation

**Project Files:**
- `src/FlightAgent.Api/FlightAgent.Api.csproj`: Web SDK, OpenAPI, SemanticKernel
- `src/FlightAgent.Core/FlightAgent.Core.csproj`: Plain class library
- `src/FlightAgent.Infrastructure/FlightAgent.Infrastructure.csproj`: SemanticKernel SDK

## Naming Conventions

**Files:**
- PascalCase for C# files: `Flight.cs`, `IFlightSearchService.cs`
- Match class name to file name exactly

**Directories:**
- PascalCase: `Models/`, `Interfaces/`, `Services/`
- Singular nouns preferred

**Projects:**
- Pattern: `FlightAgent.{Layer}`
- Root namespace matches project name

## Where to Add New Code

**New Domain Model:**
- Primary code: `src/FlightAgent.Core/Models/{ModelName}.cs`
- Pattern: Plain class with init-only properties, default values

**New Service Interface:**
- Primary code: `src/FlightAgent.Core/Interfaces/I{ServiceName}.cs`
- Pattern: Async methods returning `Task<T>` or `Task`

**New Service Implementation:**
- Implementation: `src/FlightAgent.Infrastructure/Services/{ServiceName}.cs`
- Registration: Add to `src/FlightAgent.Infrastructure/DependencyInjection.cs`
- Pattern: Constructor-inject dependencies, implement interface

**New API Endpoint:**
- Location: `src/FlightAgent.Api/Program.cs`
- Pattern: Use `app.Map{Method}("route", handler)`
- Include `.WithName("EndpointName")` for OpenAPI

**New Enum:**
- Co-locate with related entity in `src/FlightAgent.Core/Models/`
- Pattern: PascalCase enum name and values

**Utilities/Helpers:**
- Consider static classes in appropriate layer
- Core utilities: `src/FlightAgent.Core/` (new folder)
- Infrastructure utilities: `src/FlightAgent.Infrastructure/` (new folder)

## Special Directories

**obj/ and bin/:**
- Purpose: Build artifacts
- Generated: Yes
- Committed: No (should be in .gitignore)

**.planning/codebase/:**
- Purpose: Architecture documentation
- Generated: No (manual)
- Committed: Yes

**tests/FlightAgent.Tests:**
- Purpose: Test projects
- Status: Empty directory, ready for test project addition

---

*Structure analysis: 2026-03-24*
