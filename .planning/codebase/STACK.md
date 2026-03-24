# Technology Stack

**Analysis Date:** 2026-03-24

## Languages

**Primary:**
- C# 13 - All application code (.cs files)
- Razor (implicit via ASP.NET Core) - HTML generation

## Runtime

**Environment:**
- .NET 10.0 - Latest .NET version

**Package Manager:**
- NuGet (via `dotnet` CLI)
- Lockfile: Not detected (no `packages.lock.json`)

## Frameworks

**Core:**
- ASP.NET Core 10.0 - Web API framework
  - Location: `D:/Project/FlightAgent/src/FlightAgent.Api/FlightAgent.Api.csproj`
  - Uses minimal APIs pattern

**AI/ML:**
- Microsoft SemanticKernel 1.40.0 - AI orchestration SDK
  - Used in: Api and Infrastructure projects
  - Enables LLM integration capabilities

## Key Dependencies

**Critical:**
- `Microsoft.AspNetCore.OpenApi` 10.0.3 - OpenAPI/Swagger documentation
  - Location: `D:/Project/FlightAgent/src/FlightAgent.Api/FlightAgent.Api.csproj`
  - Provides auto-generated API documentation at `/openapi/v1.json`

- `Microsoft.SemanticKernel` 1.40.0 - AI orchestration
  - Location: Api and Infrastructure projects
  - Version: 1.40.0 (explicit version matching)

- `Microsoft.SemanticKernel.Plugins.Core` 1.40.0 - Core plugins
  - Location: `D:/Project/FlightAgent/src/FlightAgent.Infrastructure/FlightAgent.Infrastructure.csproj`

**Project References:**
- Three-layer architecture with project references:
  - Api -> Core, Infrastructure
  - Infrastructure -> Core
  - Core (no dependencies)

## Configuration

**Environment:**
- `appsettings.json` - Production configuration
- `appsettings.Development.json` - Development-specific
- Environment variable: `ASPNETCORE_ENVIRONMENT`

**Build:**
- `FlightAgent.sln` - Solution file
- `ImplicitUsings` enabled in all projects
- `Nullable` reference types enabled

**Build Configuration:**
- Debug|Any CPU
- Release|Any CPU

## Platform Requirements

**Development:**
- .NET 10.0 SDK
- Visual Studio 2022 17.5+ (per solution file format)
- HTTPS development certificate

**Production:**
- ASP.NET Core hosting (Kestrel or IIS)
- HTTPS redirection enabled
- OpenAPI docs served in development only

## Service URLs

**HTTP:** http://localhost:5120
**HTTPS:** https://localhost:7179

---

*Stack analysis: 2026-03-24*
