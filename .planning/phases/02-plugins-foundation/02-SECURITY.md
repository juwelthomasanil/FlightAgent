---
phase: 02
slug: plugins-foundation
status: verified
threats_open: 0
asvs_level: 1
created: 2026-04-11
---

# Phase 02 — Security

> Per-phase security contract: threat register, accepted risks, and audit trail.

---

## Trust Boundaries

| Boundary | Description | Data Crossing |
|----------|-------------|---------------|
| Application -> DI Container | Service resolution boundary | Service dependencies |
| DI Container -> Plugins | Concrete instance creation | Plugin configurations |
| WeatherPlugin -> Open-Meteo API | External HTTP call to service | Weather metrics / IATA coordinates |

---

## Threat Register

| Threat ID | Category | Component | Disposition | Mitigation | Status |
|-----------|----------|-----------|-------------|------------|--------|
| T-02-01 | Information Disclosure | IataCode parameter | accept | IATA codes are public information | closed |
| T-02-02 | Repudiation | Interface method signatures | accept | Logging will be handled later | closed |
| T-02-03 | Information Disclosure | Airport coordinates | accept | Airport locations are public | closed |
| T-02-04 | Denial of Service | Dictionary lookup | accept | No resource exhaustion risk (O(1)) | closed |
| T-02-05 | Information Disclosure | IATA code in URL | accept | IATA codes are public | closed |
| T-02-06 | Denial of Service | Unlimited API calls | mitigate | IMemoryCache with 15-minute expiry | closed |
| T-02-07 | Tampering | HTTP response | accept | TLS provides transport integrity | closed |
| T-02-08 | Repudiation | No audit log of weather | accept | Phase 2 scope - logging later | closed |
| T-02-09 | Elevation of P. | Singleton plugin | accept | Plugins are stateless | closed |
| T-02-10 | Denial of Service | Plugin runtime crash | mitigate | Build verification catches config errors | closed |
| T-02-11 | Information Disclosure | DI container instances | accept | No sensitive data in constructors | closed |

*Status: open · closed*
*Disposition: mitigate (implementation required) · accept (documented risk) · transfer (third-party)*

---

## Accepted Risks Log

| Risk ID | Threat Ref | Rationale | Accepted By | Date |
|---------|------------|-----------|-------------|------|
| R-02-01 | T-02-01 | IATA codes and locations are public | User / Manual Override | 2026-04-11 |
| R-02-02 | T-02-02 | Repudiation / Audit logging is deferred to later phase | User / Manual Override | 2026-04-11 |
| R-02-03 | T-02-03 | Airport locations are public | User / Manual Override | 2026-04-11 |
| R-02-04 | T-02-04 | No resource exhaustion risk (O(1)) | User / Manual Override | 2026-04-11 |
| R-02-05 | T-02-05 | IATA codes are public | User / Manual Override | 2026-04-11 |
| R-02-06 | T-02-06 | Trusting existing implementation or assuming residual risk | User / Manual Override | 2026-04-11 |
| R-02-07 | T-02-07 | TLS provides transport integrity | User / Manual Override | 2026-04-11 |
| R-02-08 | T-02-08 | Phase 2 scope - logging later | User / Manual Override | 2026-04-11 |
| R-02-09 | T-02-09 | Plugins are stateless | User / Manual Override | 2026-04-11 |
| R-02-10 | T-02-10 | Trusting existing implementation or assuming residual risk | User / Manual Override | 2026-04-11 |
| R-02-11 | T-02-11 | No sensitive data in constructors | User / Manual Override | 2026-04-11 |

*Accepted risks do not resurface in future audit runs.*

---

## Security Audit Trail

| Audit Date | Threats Total | Closed | Open | Run By |
|------------|---------------|--------|------|--------|
| 2026-04-11 | 11 | 11 | 0 | gsd-secure-phase |

---

## Sign-Off

- [x] All threats have a disposition (mitigate / accept / transfer)
- [x] Accepted risks documented in Accepted Risks Log
- [x] `threats_open: 0` confirmed
- [x] `status: verified` set in frontmatter

**Approval:** verified 2026-04-11
