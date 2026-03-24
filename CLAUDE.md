# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

This is an ASP.NET Core Web API project named **FlightAgent** built with .NET 10. It uses minimal APIs and includes OpenAPI/Swagger support for API documentation.

## Development Commands

### Build and Run

```bash
# Build the project
dotnet build

# Run the application (development mode)
dotnet run

# Run with hot reload
dotnet watch run

# Run with specific profile
dotnet run --launch-profile https
dotnet run --launch-profile http
```

The application runs on:
- HTTP: http://localhost:5120
- HTTPS: https://localhost:7179 (when using https profile)

### Testing

```bash
# Run all tests (when test projects are added)
dotnet test

# Run tests in verbose mode
dotnet test --verbosity normal

# Run specific test project
dotnet test <ProjectName>.csproj
```

### Package Management

```bash
# Restore NuGet packages
dotnet restore

# Add a package
dotnet add package <PackageName>

# List outdated packages
dotnet list package --outdated
```

## Architecture

### Project Structure

- **Program.cs**: Application entry point using minimal API pattern
- **FlightAgent.csproj**: Project file targeting `net10.0`
- **appsettings.json**: Production configuration
- **appsettings.Development.json**: Development-specific configuration
- **Properties/launchSettings.json**: Launch profiles for development
- **FlightAgent.http**: HTTP request file for testing endpoints (VS Code REST Client extension)

### Key Dependencies

- `Microsoft.AspNetCore.OpenApi` (10.0.3): Provides OpenAPI/Swagger documentation

### Patterns

- **Minimal APIs**: Uses `app.MapGet()` and similar methods for endpoint registration
- **Record Types**: Uses C# records for DTOs (e.g., `WeatherForecast` record)
- **OpenAPI**: Swagger documentation available at `/openapi/v1.json` in development mode

## API Endpoints

The project currently includes a sample endpoint:

- `GET /weatherforecast` - Returns a sample weather forecast (5 days of random data)

## Testing Endpoints

Use the `FlightAgent.http` file with VS Code REST Client extension, or use curl:

```bash
curl http://localhost:5120/weatherforecast
```
