## Why

This implements the core Hotel Availability feature for the SkyRoute challenge (GitHub Issue #1). The system must allow travellers to search hotels across multiple providers, reserve rooms with document validation, and retrieve reservation details. This is the foundational booking capability required before any additional travel features can be added.

## What Changes

- **Hotel Search API** — Query endpoint that aggregates results from PremierStays (PascalCase JSON) and BudgetNests (snake_case JSON) providers, normalizes provider-specific formats into a unified Domain `Room`, filters by date range matching (request dates within stub data ranges), and returns unified results using default .NET minimal API response format (camelCase JSON); accepts destination as cityCode (e.g., BOM, LON, NYC)
- **Reservation API** — POST endpoint that validates document type against destination (international requires Passport, domestic accepts National ID), creates reservation with unique reference number format `REF-{8-char-GUID}`, and stores in-memory
- **Reservation Lookup API** — GET endpoint that retrieves reservation details by reference number using default .NET minimal API response format
- **Lookup APIs** — GET endpoints for Countries (`/lookups/countries`) and Cities (`/lookups/cities`) providing master data for frontend dropdowns; supports destination selection by city code
- **Provider abstraction** — `IHotelProvider` interface with two stub implementations (PremierStays with PascalCase model, BudgetNests with snake_case model), each with its own dedicated mapper that normalizes to unified Domain `Room`; extensible for third-party providers
- **Document validation domain rules** — Validates document type matches destination classification (India = domestic accepts NationalId or Passport; non-India = international requires Passport only)
- **Domain-driven reservation model** — `Reservation.Reserve(...)` method encapsulates all reservation invariants including document validation, date range validation, and timestamp generation
- **Unified response models** — Normalize provider-specific data (PremierStays PascalCase, BudgetNests snake_case) into a single Domain `Room` type serialized as camelCase JSON (default .NET minimal API); city codes for destinations
- **Error handling middleware** — Global exception handler mapping domain exceptions to ProblemDetails: `DomainValidationException` → 400, `DocumentMismatchException` → 422, `ReservationNotFoundException` → 404
- **Correlation ID middleware** — `X-Correlation-Id` header propagation and structured logging

## Capabilities

### New Capabilities

- `hotel-search`: Hotel availability search across multiple providers with format normalization (PremierStays PascalCase, BudgetNests snake_case → unified Domain Room), date range filtering, and default .NET minimal API camelCase JSON response; accepts destination as cityCode (see specs/hotel-search.md)
- `hotel-reservation`: Room reservation with document validation (India=domestic accepts National ID or Passport; non-India=international requires Passport), reference generation, and in-memory storage (see specs/hotel-reservation.md)
- `reservation-lookup`: Retrieve reservation details by reference number in default .NET minimal API camelCase JSON (see specs/reservation-lookup.md)
- `lookup-countries`: Get list of countries with code and name for frontend selection (see specs/lookups.md)
- `lookup-cities`: Get list of cities with code, name, and countryCode for frontend selection; supports destination filtering by city code (see specs/lookups.md)

### Modified Capabilities

<!-- No existing specs to modify — this is the initial implementation -->

### Implementation Details (Not Separate Specs)

The following are implementation details included within the 4 endpoint specs above:
- **Provider abstraction**: `IHotelProvider` interface with PremierStays and BudgetNests stub implementations (covered in hotel-search spec)
- **Document validation**: Domain rules for India vs non-India locations (covered in hotel-reservation spec scenarios)
- **Error handling**: Exception middleware mapping domain errors to HTTP ProblemDetails (covered in error scenarios across all specs)
- **Date range matching**: Request dates within stub data date ranges (covered in hotel-search spec)

## Impact

**New Code:**
- **Domain layer:** `Reservation`, `Document`, `Room` entities/value objects; `Country`, `City` lookup entities; `RoomType`, `DocumentType` enums; `IReservationRepository` interface; domain exceptions (`DomainValidationException`, `DocumentMismatchException`, `ReservationNotFoundException`)
- **Application layer:** `IHotelProvider`, `IHotelSearchService`, `IReservationService` interfaces; `HotelSearchService`, `ReservationService` implementations; request/response DTOs for search (with DateTime fields), reservation, and lookup operations; `ReservationMapper` for Domain ↔ DTO transformations
- **Infrastructure layer:** `PremierStaysProvider` with `PremierStaysRoomEntry` (PascalCase model) + `PremierStaysMapper`; `BudgetNestsProvider` with `BudgetNestsRoomEntry` (snake_case model) + `BudgetNestsMapper`; date range matching logic (request dates within stub data ranges); `InMemoryReservationRepository`
- **API layer:** Hotel search, reserve, and lookup endpoints; lookup endpoints for countries and cities; `ExceptionHandlingMiddleware`, `CorrelationIdMiddleware`; default .NET minimal API JSON response format (camelCase via `JsonSerializerDefaults.Web`)
- **Tests:** xUnit tests for domain invariants (reservation validation, document validation, date range matching), service orchestration (mocking providers/repositories), provider behavior (stub determinism, filtering, PascalCase and snake_case format normalization), API contracts (endpoint responses, error status codes, camelCase JSON)

**Affected Dependencies:**
- .NET 8 SDK
- `Microsoft.AspNetCore.OpenApi` (for ProblemDetails)
- xUnit test framework

**Non-Goals (Explicitly Skipped for Challenge Scope):**
- Authentication/authorization
- Real hotel API integrations or credentials
- Database persistence (using in-memory `ConcurrentDictionary`)
- CI/CD pipelines
- Docker containerization
- Rate limiting or throttling
- Health checks or observability beyond basic logging
- OpenAPI/Swagger documentation generation
- End-to-end or integration tests
- Code coverage reporting

**Future Scope:**
- **IDocument pattern** — Introduce interface for extensible document verification; allow `PassportDocument`, `NationalIdDocument` to implement type-specific validation (format rules, expiry dates); support additional document types (Driver's License, Visa)
- **Third provider integration** — Add new `IHotelProvider` implementation without modifying core search/reservation flow
- **Persistent storage** — Replace `InMemoryReservationRepository` with Entity Framework or Dapper-based repository
- **Cancellation workflow** — API endpoint and domain logic for cancelling reservations based on policy rules
- **Search result caching** — Cache provider responses to avoid duplicate queries within a session
- **Room availability locking** — Prevent double-booking during reservation window
