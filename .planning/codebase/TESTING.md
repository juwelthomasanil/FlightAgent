# Testing Patterns

**Analysis Date:** 2026-03-24

## Test Framework

**Status:** Test project exists but contains no test files

**Test Project Location:** `/d/Project/FlightAgent/tests/FlightAgent.Tests/`

**Current State:**
- Project directory exists at `tests/FlightAgent.Tests/`
- No `.csproj` file present
- No test files present
- Directory is empty

**Recommended Setup:**

Since the project is using .NET 10, the following testing stack is recommended:

**Runner:**
- xUnit 2.9+ (industry standard for .NET)
- Config: Create `tests/FlightAgent.Tests/FlightAgent.Tests.csproj`

**Assertion Library:**
- xUnit built-in assertions
- FluentAssertions (optional, for more readable assertions)

**Run Commands:**
```bash
# Run all tests
dotnet test

# Run with verbosity
dotnet test --verbosity normal

# Run specific test project
dotnet test tests/FlightAgent.Tests/FlightAgent.Tests.csproj

# Run with filter
dotnet test --filter "FullyQualifiedName~SemanticFlightSearchService"
```

## Test File Organization

**Location:**
- Mirror source structure under `tests/FlightAgent.Tests/`
- Co-locate tests with parallel folder structure

**Naming:**
- Test files: `{ClassName}Tests.cs` (e.g., `SemanticFlightSearchServiceTests.cs`)
- Test classes: Public, matching the class under test name + "Tests"
- Test methods: Descriptive names describing behavior

**Recommended Structure:**
```
tests/FlightAgent.Tests/
├── Core/
│   └── Models/
│       ├── FlightTests.cs
│       └── BookingTests.cs
├── Infrastructure/
│   └── Services/
│       ├── SemanticFlightSearchServiceTests.cs
│       └── SemanticBookingServiceTests.cs
└── Api/
    └── Endpoints/
        └── WeatherForecastTests.cs
```

## Test Structure

**Suite Organization:**
```csharp
public class SemanticFlightSearchServiceTests
{
    private readonly Mock<Kernel> _kernelMock;
    private readonly SemanticFlightSearchService _service;

    public SemanticFlightSearchServiceTests()
    {
        _kernelMock = new Mock<Kernel>();
        _service = new SemanticFlightSearchService(_kernelMock.Object);
    }

    [Fact]
    public async Task SearchFlightsAsync_WithValidParameters_ReturnsFlights()
    {
        // Arrange
        var origin = "NYC";
        var destination = "LAX";
        var date = DateTime.Now.AddDays(1);

        // Act
        var result = await _service.SearchFlightsAsync(origin, destination, date);

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(3);
    }
}
```

**Patterns:**
- Use constructor for common setup
- Use `Fact` for single test cases
- Use `Theory` for parameterized tests with multiple inputs
- Follow Arrange-Act-Assert pattern

## Mocking

**Framework:** Moq (recommended)

**Installation:**
```bash
dotnet add tests/FlightAgent.Tests/ package Moq
dotnet add tests/FlightAgent.Tests/ package Microsoft.SemanticKernel
```

**Patterns:**
```csharp
// Mock external dependencies
var kernelMock = new Mock<Kernel>();
kernelMock.Setup(k => k.InvokePromptAsync(It.IsAny<string>(), It.IsAny<PromptExecutionSettings>()))
    .ReturnsAsync(new FunctionResult(...));

// Mock return values for async methods
kernelMock.Setup(k => k.InvokePromptAsync(It.IsAny<string>(), null, null, null, null, default))
    .ReturnsAsync(new FunctionResult(..., "mock result"));
```

**What to Mock:**
- External services (Semantic Kernel, HTTP clients)
- Time-dependent operations (use `TimeProvider` abstraction)
- Random number generation

**What NOT to Mock:**
- Plain DTOs and models (use real instances)
- Simple internal utilities
- Extension methods

## Fixtures and Factories

**Test Data:**
```csharp
public static class TestData
{
    public static Flight CreateFlight(string flightNumber = "AA123")
    {
        return new Flight
        {
            FlightNumber = flightNumber,
            Airline = "Test Airlines",
            Origin = "NYC",
            Destination = "LAX",
            ScheduledDeparture = DateTime.UtcNow.AddHours(2),
            ScheduledArrival = DateTime.UtcNow.AddHours(5),
            Status = FlightStatus.OnTime,
            Price = 299.99m,
            AvailableSeats = 50
        };
    }

    public static Passenger CreatePassenger(string firstName = "John", string lastName = "Doe")
    {
        return new Passenger
        {
            FirstName = firstName,
            LastName = lastName,
            Email = "john.doe@example.com",
            PassportNumber = "AB123456"
        };
    }
}
```

**Location:**
- Create `tests/FlightAgent.Tests/TestData/` directory
- Or use static classes within test projects

## Coverage

**Requirements:** No coverage target currently enforced

**Recommended Target:**
- Core project: 80%+ coverage
- Infrastructure: 70%+ coverage
- API: Integration tests for endpoints

**View Coverage:**
```bash
# Generate coverage report
dotnet test --collect:"XPlat Code Coverage"

# View with ReportGenerator (requires tool installation)
dotnet tool install -g dotnet-reportgenerator-globaltool
reportgenerator -reports:"**/coverage.cobertura.xml" -targetdir:"coveragereport"
```

## Test Types

**Unit Tests:**
- Scope: Single class/method in isolation
- Mock all external dependencies
- Fast execution (< 100ms per test)
- Location: `tests/FlightAgent.Tests/Core/`, `tests/FlightAgent.Tests/Infrastructure/`

**Integration Tests:**
- Scope: Multiple layers working together
- Use test doubles for external services (Semantic Kernel)
- Test DI container wiring
- Location: `tests/FlightAgent.Tests/Integration/`

**E2E/API Tests:**
- Scope: Full HTTP request/response cycle
- Use `WebApplicationFactory<Program>` for in-memory server
- Test endpoint routing and serialization
- Location: `tests/FlightAgent.Tests/Api/`

**Example API Test:**
```csharp
public class WeatherForecastTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;

    public WeatherForecastTests(WebApplicationFactory<Program> factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task GetWeatherForecast_ReturnsFiveDays()
    {
        var response = await _client.GetAsync("/weatherforecast");
        response.EnsureSuccessStatusCode();

        var forecasts = await response.Content.ReadFromJsonAsync<WeatherForecast[]>();
        forecasts.Should().HaveCount(5);
    }
}
```

## Common Patterns

**Async Testing:**
```csharp
[Fact]
public async Task AsyncMethod_WhenCalled_CompletesSuccessfully()
{
    // Arrange
    var service = CreateService();

    // Act
    var result = await service.DoSomethingAsync();

    // Assert
    result.Should().NotBeNull();
}
```

**Error Testing:**
```csharp
[Fact]
public async Task GetBookingAsync_WithInvalidReference_ReturnsNull()
{
    var result = await _service.GetBookingAsync("INVALID");
    result.Should().BeNull();
}

[Fact]
public async Task CreateBookingAsync_WithNullPassenger_ThrowsArgumentNullException()
{
    await Assert.ThrowsAsync<ArgumentNullException>(
        () => _service.CreateBookingAsync("FL123", null!, SeatClass.Economy));
}
```

**Collection Testing:**
```csharp
[Fact]
public async Task SearchFlightsAsync_ReturnsExpectedFlightProperties()
{
    var flights = await _service.SearchFlightsAsync("NYC", "LAX", DateTime.Now);

    flights.Should().AllSatisfy(flight =>
    {
        flight.Origin.Should().Be("NYC");
        flight.Destination.Should().Be("LAX");
        flight.FlightNumber.Should().NotBeNullOrEmpty();
    });
}
```

## Next Steps

1. **Create test project file:** `tests/FlightAgent.Tests/FlightAgent.Tests.csproj`
2. **Add NuGet packages:** xUnit, Moq, Microsoft.NET.Test.Sdk
3. **Add first test:** Start with model validation tests
4. **Add service tests:** Mock Semantic Kernel dependencies
5. **Add integration tests:** Test DI registration and service resolution

---

*Testing analysis: 2026-03-24*
