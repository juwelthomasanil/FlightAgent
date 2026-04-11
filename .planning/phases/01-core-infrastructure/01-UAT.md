---
status: testing
phase: 01-core-infrastructure
source:
  - 01-01-SUMMARY.md
started: 2026-04-10T14:30:00Z
updated: 2026-04-10T14:30:00Z
---

## Current Test

number: 1
name: Cold Start Smoke Test
expected: |
  Kill any running server/service. Clear ephemeral state (temp DBs, caches, lock files). Start the application from scratch. Server boots without errors, any seed/migration completes, and a primary query (health check, homepage load, or basic API call) returns live data.
awaiting: user response

## Tests

### 1. Cold Start Smoke Test
expected: |
  Kill any running server/service. Clear ephemeral state (temp DBs, caches, lock files). Start the application from scratch. Server boots without errors, any seed/migration completes, and a primary query (health check, homepage load, or basic API call) returns live data.
result: pending

### 2. Health Check Endpoint Returns JSON
expected: |
  Run the application and navigate to the /health endpoint. The response is valid JSON containing: "status" field (Healthy/Degraded/Unhealthy), "checks" array with individual check results (name, status, description, duration), and "totalDuration" field.
result: pending

### 3. Health Check Includes External API Verification
expected: |
  The /health endpoint JSON response includes a check named "ExternalApiHealthCheck" with status and description fields. The check verifies connectivity to httpbin.org/get.
result: pending

### 4. Solution Builds Successfully
expected: |
  Run "dotnet build FlightAgent.sln" from the project root. The build completes with 0 errors. (Warnings are acceptable.)
result: pending

### 5. Clean Architecture DI Flow Established
expected: |
  Review Program.cs in the Api project. It contains a call to builder.Services.AddInfrastructure(builder.Configuration) that registers Infrastructure services. The dependency flow is Api -> Infrastructure -> Core.
result: pending

## Summary

total: 5
passed: 0
issues: 0
pending: 5
skipped: 0

## Gaps

[none yet]
