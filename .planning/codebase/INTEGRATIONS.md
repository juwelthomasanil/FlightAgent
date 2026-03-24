# External Integrations

**Analysis Date:** 2026-03-24

## APIs & External Services

**AI Services:**
- Microsoft SemanticKernel - AI orchestration SDK
  - SDK: `Microsoft.SemanticKernel` 1.40.0
  - Location: `D:/Project/FlightAgent/src/FlightAgent.Api/FlightAgent.Api.csproj`, `D:/Project/FlightAgent/src/FlightAgent.Infrastructure/FlightAgent.Infrastructure.csproj`
  - Auth: API keys expected via configuration (not detected in source)
  - Purpose: LLM integration and AI capabilities

**API Documentation:**
- OpenAPI/Swagger
  - SDK: `Microsoft.AspNetCore.OpenApi` 10.0.3
  - Endpoint: `/openapi/v1.json` (development only)
  - Location: `D:/Project/FlightAgent/src/FlightAgent.Api/Program.cs`

## Data Storage

**Databases:**
- Not detected - No ORM or database packages referenced

**File Storage:**
- Not detected - Local filesystem only

**Caching:**
- Not detected - No caching packages referenced

## Authentication & Identity

**Auth Provider:**
- Not detected - No authentication packages referenced

## Monitoring & Observability

**Error Tracking:**
- Not detected

**Logs:**
- Microsoft.Extensions.Logging via ASP.NET Core
  - Configuration: `D:/Project/FlightAgent/src/FlightAgent.Api/appsettings.json`
  - Default: Information level
  - ASP.NET Core: Warning level

## CI/CD & Deployment

**Hosting:**
- Self-hosted (Kestrel via `dotnet run`)
- Development ports: 5120 (HTTP), 7179 (HTTPS)

**CI Pipeline:**
- Not detected - No CI configuration files found

## Environment Configuration

**Required env vars:**
- `ASPNETCORE_ENVIRONMENT` - Set to "Development" in dev mode

**Secrets location:**
- Not detected - No `secrets.json` or vault references
- SemanticKernel API keys expected (provider-specific configuration)

## Webhooks & Callbacks

**Incoming:**
- Not detected

**Outgoing:**
- Not detected

## Integration Points

**DependencyInjection.cs:**
- Location: `D:/Project/FlightAgent/src/FlightAgent.Infrastructure/DependencyInjection.cs`
- Likely contains SemanticKernel service registration

**Sample Endpoint:**
- `GET /weatherforecast` - Returns sample weather data
  - Location: `D:/Project/FlightAgent/src/FlightAgent.Api/Program.cs` (lines 22-34)
  - No external dependencies

---

*Integration audit: 2026-03-24*
