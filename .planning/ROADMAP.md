# Roadmap: FlightAgent

**Current Version:** 1.0.0
**Last Updated:** 2026-03-24

## Phase Summary

| Phase | Name | Goal | Status |
|-------|------|------|--------|
| 1 | Core Infrastructure | Setup project structure, DI, and health checks | Planned |
| 2 | Plugins Foundation | Implement Airport and Weather plugins | Planned |
| 3 | Flight Search Service | Build aviation API integration with resilience | Planned |
| 4 | Agent Framework | Integrate Microsoft Agent Framework for NLU | Planned |
| 5 | Minimal API | Create API endpoints with Swagger | Planned |
| 6 | Testing | Unit tests with Moq and FluentAssertions | Planned |

## Phase 1: Core Infrastructure

**Goal:** Establish project structure, dependency injection, and health monitoring.

**Requirements:** REQ-009

**Success Criteria:**
- Clean Architecture structure verified
- DI container configured
- Health check endpoint responds 200

**Estimated Effort:** Small

## Phase 2: Plugins Foundation

**Goal:** Implement AirportPlugin and WeatherPlugin with hardcoded and external data.

**Requirements:** REQ-006, REQ-007

**Success Criteria:**
- AirportPlugin returns airport info by IATA code
- WeatherPlugin fetches from Open-Meteo API
- Both plugins registered in DI

**Estimated Effort:** Small

## Phase 3: Flight Search Service

**Goal:** Build service to query live aviation APIs with circuit breaker and retry patterns.

**Requirements:** REQ-002, REQ-004

**Success Criteria:**
- Aviation API client implemented
- Circuit breaker pattern working
- Retry with exponential backoff configured
- Graceful error handling

**Estimated Effort:** Medium

## Phase 4: Agent Framework

**Goal:** Integrate Microsoft Agent Framework for natural language understanding.

**Requirements:** REQ-001, REQ-003

**Success Criteria:**
- Agent Framework configured
- Intent classification working
- Entity extraction (flight numbers, airports) functional
- SemanticFlightSearchService updated

**Estimated Effort:** Medium

## Phase 5: Minimal API

**Goal:** Expose flight query endpoint with Swagger documentation.

**Requirements:** REQ-005

**Success Criteria:**
- POST /api/flights/query working
- Swagger UI available
- Request/response models documented
- Validation rules in place

**Estimated Effort:** Small

## Phase 6: Testing

**Goal:** Comprehensive unit test coverage using Moq and FluentAssertions.

**Requirements:** REQ-008

**Success Criteria:**
- All services have unit tests
- Mock external dependencies
- Resilience patterns tested
- FluentAssertions used throughout

**Estimated Effort:** Medium

---

*Roadmap: v1.0.0*
