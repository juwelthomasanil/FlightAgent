# Requirements: FlightAgent v1.0.0

**Analysis Date:** 2026-03-24

## In Scope (v1)

### REQ-001: Natural Language Flight Search
**Priority:** Must
**Description:** Accept plain English queries about flights and understand user intent.

**Acceptance Criteria:**
- Accept queries like "Is flight KL071 delayed?"
- Accept queries like "When does AA123 arrive?"
- Accept queries like "Is there a delay on the London to New York flight?"
- Parse flight numbers, airport codes, and city names
- Return structured response with flight status

### REQ-002: Flight Status Queries
**Priority:** Must
**Description:** Retrieve and display current flight status information.

**Acceptance Criteria:**
- Query live aviation APIs for real-time data
- Display: flight number, airline, departure/arrival times, status (on-time, delayed, cancelled)
- Handle flight not found responses gracefully

### REQ-003: Semantic Understanding (Microsoft Agent Framework)
**Priority:** Must
**Description:** Use AI to understand natural language intent and extract entities.

**Acceptance Criteria:**
- Integrate Microsoft Agent Framework
- Extract entities: flight number, airport codes, dates
- Classify intent: status_check, arrival_time, delay_inquiry

### REQ-004: Resilience Handling
**Priority:** Must
**Description:** Graceful degradation when external APIs fail.

**Acceptance Criteria:**
- Implement circuit breaker pattern
- Retry logic with exponential backoff
- Return user-friendly error messages when services are down
- Cache recent responses for fallback

### REQ-005: Minimal API Endpoint
**Priority:** Must
**Description:** ASP.NET Core minimal API exposing flight query endpoint.

**Acceptance Criteria:**
- POST /api/flights/query accepts natural language query
- Returns JSON response with flight information
- Swagger/OpenAPI documentation
- Request/response validation

### REQ-006: Weather Plugin
**Priority:** Must
**Description:** Provide weather information for airports using Open-Meteo API.

**Acceptance Criteria:**
- Accept airport code and return current weather
- Integration with Open-Meteo API (no API key required)
- Cache weather data for 15 minutes
- Include temperature, conditions, visibility

### REQ-007: Airport Plugin
**Priority:** Must
**Description:** Provide airport information from hardcoded dataset.

**Acceptance Criteria:**
- Hardcoded dataset of major airports (IATA code, name, city, country, coordinates)
- Support lookup by IATA code or city name
- Include timezone information

### REQ-008: Unit Tests
**Priority:** Must
**Description:** Comprehensive unit test coverage using Moq and FluentAssertions.

**Acceptance Criteria:**
- Test coverage for all services
- Mock external API calls
- Use FluentAssertions for readable assertions
- Test resilience patterns (circuit breaker, retries)

### REQ-009: Health Check Endpoint
**Priority:** Must
**Description:** Health monitoring endpoint for deployment verification.

**Acceptance Criteria:**
- GET /health returns service status
- Check external API connectivity
- Return appropriate HTTP status codes (200 healthy, 503 degraded)

## Out of Scope

### v2 (Future)
- Azure Function endpoint
- Azure AI Foundry integration
- Real-time push notifications
- Historical flight data analytics
- Price comparison across airlines
- Multi-city complex itineraries

### Explicitly Excluded
- Booking flights
- Payment processing
- User accounts/authentication
- Python scripts
- Frontend UI
- Database persistence
- Authentication/authorization

---

*Requirements document: v1.0.0*
