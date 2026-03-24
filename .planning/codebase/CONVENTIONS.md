# Coding Conventions

**Analysis Date:** 2026-03-24

## Naming Patterns

**Files:**
- Use PascalCase for file names (e.g., `Flight.cs`, `Booking.cs`, `DependencyInjection.cs`)
- Match file name to primary type name (e.g., `SemanticFlightSearchService.cs` contains `SemanticFlightSearchService` class)

**Classes:**
- PascalCase (e.g., `SemanticFlightSearchService`, `Booking`, `Flight`)
- Concrete service implementations use descriptive prefixes (e.g., `SemanticFlightSearchService`, `SemanticBookingService`)

**Interfaces:**
- PascalCase with `I` prefix (e.g., `IFlightSearchService`, `IBookingService`)
- Located in `Interfaces/` directory within Core project

**Enums:**
- PascalCase for enum name and values (e.g., `SeatClass`, `FlightStatus`, `BookingStatus`)
- Values use PascalCase (e.g., `FirstClass`, `OnTime`)

**Properties:**
- PascalCase for public properties (e.g., `FlightNumber`, `BookingReference`, `ScheduledDeparture`)
- Auto-initialized with default values: `public string FlightNumber { get; set; } = string.Empty;`

**Methods:**
- PascalCase for public methods (e.g., `SearchFlightsAsync`, `CreateBookingAsync`, `GetBookingAsync`)
- Async methods suffixed with `Async`
- Private methods use PascalCase (e.g., `CalculatePrice`)

**Variables:**
- camelCase for local variables and parameters (e.g., `origin`, `destination`, `flightNumber`, `kernel`)
- Private fields prefixed with underscore: `_kernel`, `_bookings`

**Namespaces:**
- Follow folder structure: `FlightAgent.Core.Models`, `FlightAgent.Infrastructure.Services`

## Code Style

**Formatting:**
- 4-space indentation
- Opening braces on same line (K&R style)
- Single blank line between type declarations

**Project Settings:**
- `Nullable` enabled across all projects
- `ImplicitUsings` enabled (reduces explicit using statements)
- `TargetFramework` is `net10.0`

**String Initialization:**
- Use `string.Empty` for default string values, not `""` or `null`

**Expression-bodied Members:**
- Use expression-bodied syntax for simple switch expressions and single-line returns:
```csharp
private static decimal CalculatePrice(SeatClass seatClass) => seatClass switch
{
    SeatClass.Economy => 299.99m,
    SeatClass.Business => 899.99m,
    SeatClass.FirstClass => 1999.99m,
    _ => 299.99m
};
```

**String Interpolation:**
- Use `$""` for interpolated strings with raw string literals for multi-line prompts:
```csharp
var prompt = $@"""
    Process booking for passenger {passenger.FirstName} {passenger.LastName}
    on flight {flightNumber} in {seatClass} class.
    Confirm the booking is valid.
    """;
```

## Import Organization

**Using Statements:**
- Implicit usings are enabled, so only explicit usings for external packages are needed
- Group by: System libraries first, then NuGet packages, then project references

**Order:**
1. System namespaces (implicit via `ImplicitUsings`)
2. NuGet package namespaces (e.g., `Microsoft.SemanticKernel`)
3. Project references (e.g., `FlightAgent.Core.Interfaces`)

## Error Handling

**Patterns:**
- Null-conditional returns: `var booking = _bookings.FirstOrDefault(...); return Task.FromResult(booking);`
- Silent failures for non-critical operations (e.g., `CancelBookingAsync` does nothing if booking not found)
- No explicit exception handling currently in codebase

**Null Safety:**
- Nullable reference types enabled
- Model properties initialized with non-null defaults
- Service methods return nullable types when appropriate: `Task<Flight?>`, `Task<Booking?>`

## Logging

**Framework:** Built-in ASP.NET Core logging via `ILogger`

**Configuration:**
- Log level: `Information` for Default, `Warning` for `Microsoft.AspNetCore`
- Configured in `appsettings.json` and `appsettings.Development.json`

**Patterns:**
- No explicit logging calls in current service implementations
- Rely on framework-level request logging

## Comments

**When to Comment:**
- Use comments for TODOs and configuration hints (see `DependencyInjection.cs`)
- Comment out example code for future reference
- Inline comments for business logic explanation

**Example:**
```csharp
// Add AI service (configure with your preferred provider)
// Example with Azure OpenAI:
// kernelBuilder.AddAzureOpenAIChatCompletion(...);
```

**XML Documentation:**
- Not currently used in codebase
- Consider adding for public APIs

## Function Design

**Size:**
- Keep methods focused (under 30 lines)
- Extract private methods for complex logic (e.g., `GenerateMockFlights`)

**Parameters:**
- Use primitive types for identifiers (e.g., `string flightNumber`)
- Use model types for complex data (e.g., `Passenger passenger`)

**Return Values:**
- Async methods return `Task<T>` or `Task`
- Collection returns use `IEnumerable<T>` or `List<T>`
- Nullable return types for lookup methods that may fail

**Dependency Injection:**
- Constructor injection preferred
- Store dependencies in private readonly fields

## Module Design

**Project Structure:**
- **Core**: Models and interfaces (no dependencies)
- **Infrastructure**: Service implementations (depends on Core)
- **Api**: Web API entry point (depends on Core and Infrastructure)

**Dependency Direction:**
```
Api -> Infrastructure -> Core
       \------------->/
```

**Extension Methods:**
- Use static classes for DI registration (e.g., `DependencyInjection.AddInfrastructure()`)

**Service Registration:**
- Scoped lifetime for services: `services.AddScoped<IFlightSearchService, SemanticFlightSearchService>();`
- Singleton for Kernel: `services.AddSingleton(kernelBuilder.Build());`

---

*Convention analysis: 2026-03-24*
