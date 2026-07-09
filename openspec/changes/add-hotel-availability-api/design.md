# Design: Hotel Availability API

## Context

This is the **initial implementation** of the Hotel Availability feature for the SkyRoute challenge. No existing hotel booking system is present—this establishes the foundational domain model, service layer, and API contracts.

**Constraints:**
- **In-memory storage only** — No database; reservations stored in `ConcurrentDictionary<string, Reservation>`
- **Stub providers only** — No real hotel API integrations; PremierStays and BudgetNests return deterministic stub data
- **Challenge scope** — Must run fully offline on a local machine; exclude production concerns (auth, CI/CD, Docker, etc.)
- **Interview context** — Must support live change tasks (e.g., adding a third provider) without reworking core flow

**Stakeholders:**
- SkyRoute evaluation team (assesses design, architecture, AI tooling usage)
- Travellers (search hotels, reserve rooms, view reservations via Angular frontend)

---

## Goals / Non-Goals

### Goals

1. **Multi-provider hotel search** — Aggregate results from PremierStays and BudgetNests, filter unavailable rooms, normalize formats (PascalCase vs snake_case), return unified JSON
2. **Document-validated reservations** — International destinations require Passport; domestic accept National ID or Passport; return 422 on mismatch
3. **Reservation retrieval** — Lookup by reference number (`REF-{8-char-GUID}`)
4. **Extensible provider abstraction** — Adding a third provider requires one new `IHotelProvider` implementation + DI registration, no core service changes
5. **Clean layer separation** — Full Onion architecture with compiler-enforced dependency direction; testable in isolation per layer
6. **Static mapper strategy** — One mapper per entity in Application layer, handling all transformation directions
7. **Domain-driven validation** — `Reservation.Reserve(...)` encapsulates all reservation invariants (document validation, date range, timestamp generation)

### Non-Goals

- **Persistence** — No database; in-memory `ConcurrentDictionary` is sufficient for challenge scope
- **Authentication/Authorization** — No login, roles, or user identity
- **Real hotel APIs** — No external integrations, credentials, or rate limiting
- **Production infrastructure** — No CI/CD, Docker, health checks, or observability beyond basic logging
- **Advanced reservation features** — No cancellation workflow, modification, or payment processing
- **OpenAPI/Swagger** — No auto-generated API documentation
- **E2E or integration tests** — xUnit tests scoped to unit-level per layer

---

## Decisions

### 1. **Onion Architecture (Full 4-Project Setup)**

**Decision:** Use full Onion architecture with strict dependency direction enforced by project references.

**Layers:**
```
HotelStay.Domain          ← no dependencies (pure domain)
HotelStay.Application     ← references Domain
HotelStay.Infrastructure  ← references Domain + Application
HotelStay.Api             ← references Application + Infrastructure (composition root)
HotelStay.Tests           ← references all projects
```

**Rationale:**
- **Testability** — Domain and Application can be tested without Infrastructure or API concerns
- **Extensibility** — Adding new providers, repositories, or endpoints doesn't touch domain logic
- **Interview context** — Demonstrates understanding of layered architecture; easy to explain live
- **Compiler-enforced** — Project references prevent accidental dependencies (e.g., Domain cannot reference Application)

**Alternative Considered:** Pragmatic 3-project Onion (merge Application + Infrastructure) — Rejected: separate layers make provider/repository swap and testing boundaries clearer

---

### 2. **Interface Placement: `IReservationRepository` in Domain, `IHotelProvider` in Application**

**Decision:**
- `IReservationRepository` belongs in **Domain** (HotelStay.Domain/Abstractions/)
- `IHotelProvider` belongs in **Application** (HotelStay.Application/Abstractions/)

**IHotelProvider Contract:**
```csharp
public interface IHotelProvider
{
    IEnumerable<Room> Search(HotelSearchRequest request);
    Room? GetRoomById(Guid roomId);  // For reservation validation
}
```

**Rationale:**
- **`IReservationRepository` in Domain** — Canonical DDD pattern (Microsoft eShop, Vernon Palermo); persistence of an aggregate's own entities is a domain concern; Domain defines the contract, Infrastructure implements it
- **`IHotelProvider` in Application** — External gateway, integration concern; domain has no concept of "provider"—it only knows about `Room` entities; Application orchestrates providers to fulfill search use cases
- **`GetRoomById` method** — Enables reservation service to fetch Room details by roomId without re-querying all providers; each provider checks its own catalog

**Alternative Considered:** Both interfaces in Application — Rejected: loses the DDD separation between aggregate persistence (domain responsibility) and external integration (application responsibility)

---

### 3. **Static Mapper Classes — One Mapper Per Entity in Application Layer**

**Decision:** Use explicit static mapper classes in **Application layer only**, with one mapper per entity/aggregate handling all transformation directions.

**Mapper Organization:**
```
HotelStay.Application/
  - DTOs/
      - HotelSearchRequest.cs       (input DTO for search - no mapper needed)
      - HotelSearchResponse.cs      (output DTO - Room serializes directly or inline mapping)
      - ReservationRequest.cs       (input DTO for reserve)
      - ReservationResponse.cs      (output DTO)
  - Mappers/
      - ReservationMapper.cs        (prepares params for factory; maps Domain → Response)

HotelStay.Infrastructure/Providers/PremierStays/
  - PremierStaysMapper.cs           (Provider JSON → Domain Room)

HotelStay.Infrastructure/Providers/BudgetNests/
  - BudgetNestsMapper.cs            (Provider JSON → Domain Room)

HotelStay.Api/
  - Endpoints/                      (thin HTTP layer - extracts request data, calls Application DTOs directly)
```

**Rationale:**
- **One mapper per entity** — `ReservationMapper` handles ALL reservation-related mapping (prepares Document/params for domain factory, domain → response); reduces file sprawl
- **No search mapper needed** — `Room` is the normalized domain model; providers convert their JSON → Room; search service aggregates Rooms; Room serializes directly to JSON or inline mapping used if response shape differs
- **Service calls factory** — `ReservationService` calls `Reservation.Reserve(...)` directly; `ReservationMapper` only prepares value objects (e.g., `Document.Create(...)`) from request DTO
- **Nested Document mapping inline** — `ReservationMapper` handles nested `DocumentDto` → `Document` value object inline; no separate DocumentMapper needed
- **No circular dependencies** — Application DTOs define the contract; API layer extracts HTTP data and passes to Application DTOs directly; no API→Application mappers needed
- **Filter/search DTOs in Application** — `HotelSearchRequest` is a DTO in Application layer; `IHotelProvider` uses it directly (interface in Application, implementation in Infrastructure = no cycle)
- **Explicit** — No magic; mapping logic is visible and searchable
- **Testable** — Pure functions easy to unit test (input → output)
- **Interview context** — Demonstrates understanding of layer boundaries; no library to explain

**Alternative Considered:** Separate mappers for request→domain and domain→response — Rejected: creates unnecessary file sprawl; one mapper per entity is cleaner

---

### 4. **Provider Static Catalog with Provider-Specific Casing and Date Range Matching**

**Decision:** Each provider maintains a **static catalog** of rooms with pre-generated GUIDs. **Each provider has its own model class with its own casing convention** (simulating real-world provider JSON formats), and its own mapper that normalizes to a unified Domain `Room`. Search queries filter by checking if request dates fall **within** the stub data date ranges.

**Provider Format Normalization:**
- **PremierStays** returns PascalCase JSON → `PremierStaysRoomEntry` (C# PascalCase properties like `RoomId`, `CheckIn`, `PerNightRate`) → `PremierStaysMapper.ToRoom(entry)` → unified `Room`
- **BudgetNests** returns snake_case JSON → `BudgetNestsRoomEntry` (C# snake_case fields like `room_id`, `check_in`, `per_night_rate`) → `BudgetNestsMapper.ToRoom(entry)` → unified `Room`

This structure demonstrates the real challenge of aggregating from multiple providers with different data formats. The Domain `Room` value object is the single canonical representation used by Application services.

**Date Range Matching Logic:**
- Stub data defines room availability with `CheckIn` and `CheckOut` dates (e.g., `"2024-03-15"` to `"2024-03-20"`)
- Search request provides desired `checkIn` and `checkOut` dates
- Room matches if: `request.CheckIn >= stub.CheckIn` AND `request.CheckOut <= stub.CheckOut`
- This allows partial date overlap testing (e.g., stub has March 15–20 range, request for March 16–19 matches)

**RoomType Enum:**
```csharp
public enum RoomType
{
    Standard,
    Deluxe,
    Suite
}
```

**File Layout:**
```
HotelStay.Infrastructure/Providers/
  PremierStays/
    PremierStaysRoomEntry.cs      // PascalCase model
    PremierStaysStubData.cs       // static catalog
    PremierStaysMapper.cs         // Entry → Room
    PremierStaysProvider.cs       // IHotelProvider
  BudgetNests/
    BudgetNestsRoomEntry.cs       // snake_case model
    BudgetNestsStubData.cs        // static catalog
    BudgetNestsMapper.cs          // Entry → Room
    BudgetNestsProvider.cs        // IHotelProvider
```

**Rationale:**
- **Simulates real provider integration** — Different casing per provider mirrors what happens when integrating with real hotel APIs (e.g., Expedia PascalCase vs Booking.com snake_case)
- **Deterministic** — Same search criteria always return same `roomId` → testable
- **Date flexibility** — Range matching allows testing partial overlaps without exact date equality
- **Reservation validation** — Can verify `roomId` exists in catalog during reservation via `IHotelProvider.GetRoomById(Guid)`
- **Extensibility** — Third provider adds its own model + mapper + catalog, no shared state or shared type
- **Simplified maintenance** — Date literals in `yyyy-MM-dd` format (no time component) are cleaner; parsed to DateTime at runtime
- **Challenge scope** — Avoids complexity of dynamic availability simulation

**Alternative Considered:** Shared `RoomCatalogEntry` type used by both providers — Rejected: hides the format normalization challenge; doesn't demonstrate the value of the mapper layer

**Alternative Considered:** Exact date matching (request dates must equal stub dates) — Rejected: too rigid; can't test different date ranges without duplicate stub data

**Alternative Considered:** Generate `roomId` on-the-fly per search — Rejected: can't validate `roomId` during reservation; breaks "verify roomId exists" requirement

---

### 5. **DateTime Fields with Default .NET Minimal API JSON Response (camelCase)**

**Decision:** Use `DateTime` C# type for check-in, check-out, and reservation timestamp fields. Use **default .NET minimal API response format** (no custom `JsonSerializerOptions` configuration — camelCase via `JsonSerializerDefaults.Web`). Stub data stores dates in simplified `yyyy-MM-dd` format; providers parse to DateTime at runtime.

**Rationale:**
- **DateTime over strings** — Leverages .NET's date/time parsing and validation; clients send ISO 8601 DateTime (e.g., `2024-03-15T00:00:00`)
- **Default .NET minimal API format** — ASP.NET Core minimal APIs use `JsonSerializerDefaults.Web` by default, which serializes with camelCase; no explicit configuration needed
- **Frontend friendly** — camelCase matches JavaScript/TypeScript conventions naturally, ideal for the Angular frontend
- **Simplified stub data** — Date literals in `yyyy-MM-dd` format (e.g., `"2024-03-15"`) are easier to maintain; parsed to `DateTime.Parse` with default time `00:00:00`
- **Consistency** — All date fields use the same format: search request dates, room check-in/check-out, reservation timestamp

**Alternative Considered:** PascalCase via `PropertyNamingPolicy = null` — Rejected: requires explicit configuration; camelCase is the framework default and matches frontend expectations

**Alternative Considered:** String dates with `yyyy-MM-dd HH:mm:ss` format — Rejected: loses type safety; manual parsing fragile; DateTime is standard for .NET APIs

---

### 6. **Location-Based Provider and Currency with City Codes**

**Decision:** Room entity has a `Destination` (cityCode, e.g., "BOM", "LON", "NYC") and `Location` (country name) field. Provider selection and currency are determined by country/location. Lookup endpoints provide Country and City master data.

**Location Rules:**
- **Domestic (India)** — Cities in India (e.g., Mumbai/BOM, Delhi/DEL, Bangalore/BLR); currency INR
- **International (Non-India)** — Cities outside India (e.g., London/LON, New York/NYC); currency based on country (USD for US, GBP for UK, EUR for Europe, JPY for Japan)

**City Codes:**
- `BOM` = Mumbai, India
- `LON` = London, United Kingdom
- `NYC` = New York, United States
- Additional cities can be added via lookup data

**Lookup Endpoints:**
```
GET /lookups/countries  → Returns list of countries with code and name
GET /lookups/cities     → Returns list of cities with code, name, and countryCode
```

**Destination Validation:**
- Search accepts `destination` as cityCode (e.g., `?destination=BOM`)
- International destinations (non-India) require Passport; domestic (India) accept National ID or Passport
- `Room.Location` determines currency and document validation rules

**Provider Assignment:**
- Providers can serve multiple locations; static catalog defines which rooms belong to which city/country
- Currency determined by room's location/country, not hardcoded per provider

**Rationale:**
- **Realistic** — Reflects real-world multi-currency, multi-country scenarios with city codes
- **Extensibility** — Adding new countries/cities only requires updating lookup data and stub catalogs
- **Frontend integration** — Lookup endpoints support dropdown/autocomplete UIs; city codes enable consistent destination filtering
- **Clear validation rules** — India vs non-India determines document requirements
- **Challenge scope** — No currency conversion required; just display native currency per location

**Alternative Considered:** Free-text city names without codes — Rejected: ambiguous (multiple cities with same name); codes provide unique identifiers

**Alternative Considered:** Hardcode provider-to-currency mapping — Rejected: coupling provider to currency limits flexibility; location-based is more realistic

---

### 7. **Reservation Request: Only `roomId` + `document` Required**

**Decision:** Reserve endpoint accepts only `{ roomId, document }`—destination, dates, room type, provider are already in the Room model retrieved from the catalog.

**API Contracts:**

**Search Request/Response (camelCase JSON — default .NET minimal API format):**
```json
GET /hotels/search?destination=LON&checkIn=2024-03-16T00:00:00&checkOut=2024-03-19T00:00:00&roomType=Deluxe

Response 200 OK:
{
  "results": [
    {
      "roomId": "22222222-2222-2222-2222-222222222222",
      "destination": "LON",
      "location": "United Kingdom",
      "roomType": "Deluxe",
      "checkIn": "2024-03-16T00:00:00",
      "checkOut": "2024-03-19T00:00:00",
      "perNightRate": 150.00,
      "numberOfNights": 3,
      "totalPrice": 450.00,
      "currency": "GBP",
      "provider": "PremierStays",
      "cancellationPolicy": "FreeCancellation",
      "amenities": ["WiFi", "Heating", "Pool", "Gym"],
      "starRating": 4
    }
  ]
}
```

**Reserve Request/Response (camelCase JSON — default .NET minimal API format):**
```json
POST /hotels/reserve
{
  "roomId": "22222222-2222-2222-2222-222222222222",
  "document": {
    "holderName": "John Doe",
    "type": "Passport",
    "number": "AB1234567"
  }
}

Response 201 Created:
{
  "referenceNumber": "REF-22222222",
  "roomId": "22222222-2222-2222-2222-222222222222",
  "destination": "LON",
  "location": "United Kingdom",
  "roomType": "Deluxe",
  "checkIn": "2024-03-16T00:00:00",
  "checkOut": "2024-03-19T00:00:00",
  "numberOfNights": 3,
  "totalPrice": 450.00,
  "currency": "GBP",
  "provider": "PremierStays",
  "document": {
    "holderName": "John Doe",
    "type": "Passport",
    "number": "AB1234567"
  },
  "reservationTimestamp": "2024-03-10T14:30:00"
}
```

**Lookup Response (camelCase JSON — default .NET minimal API format):**
```json
GET /hotels/reservation/REF-22222222

Response 200 OK:
{
  "referenceNumber": "REF-22222222",
  "roomId": "22222222-2222-2222-2222-222222222222",
  "destination": "LON",
  "location": "United Kingdom",
  "roomType": "Deluxe",
  "checkIn": "2024-03-16T00:00:00",
  "checkOut": "2024-03-19T00:00:00",
  "numberOfNights": 3,
  "totalPrice": 450.00,
  "currency": "GBP",
  "provider": "PremierStays",
  "document": {
    "holderName": "John Doe",
    "type": "Passport",
    "number": "AB1234567"
  },
  "reservationTimestamp": "2024-03-10T14:30:00"
}
```

**Rationale:**
- **DRY** — Room data already exists in provider catalog; no need to duplicate
- **Validation simplicity** — Service fetches Room by `roomId`, validates document against `Room.Destination`
- **Less error-prone** — Client can't send mismatched destination/dates/room type

**Alternative Considered:** Include full room details in reserve request — Rejected: redundant data; client could send inconsistent fields

---

### 8. **Domain-Driven `Reservation.Reserve(...)` Validation**

**Decision:** `Reservation.Reserve(...)` static factory method encapsulates all reservation invariants: document validation, date range validation, timestamp generation. **Service calls the factory directly**; mapper only prepares value objects from DTOs.

**Signature:**
```csharp
public static Reservation Reserve(
    string referenceNumber,
    Guid roomId,
    Room room,
    Document document,
    DateTime reservationTimestamp)
{
    // Validate document type matches destination (India = domestic, non-India = international)
    bool isInternational = !room.Location.Equals("India", StringComparison.OrdinalIgnoreCase);
    if (isInternational && document.Type != DocumentType.Passport)
        throw new DocumentMismatchException("International destinations require Passport");

    // Validate date range (check-out > check-in)
    if (room.CheckOut <= room.CheckIn)
        throw new DomainValidationException("Check-out must be after check-in");

    // Validate document not empty
    if (string.IsNullOrWhiteSpace(document.Number))
        throw new DomainValidationException("Document number is required");

    return new Reservation { /* ... */ };
}
```

**Service Flow:**
```csharp
// ReservationService.cs
public ReservationResponse Reserve(ReservationRequest request)
{
    // 1. Fetch Room by roomId from providers
    Room? room = _providers.Select(p => p.GetRoomById(request.RoomId))
                           .FirstOrDefault(r => r != null);
    if (room == null) throw new DomainValidationException("Invalid roomId");

    // 2. Prepare value objects via mapper
    Document document = ReservationMapper.ToDocument(request.Document);

    // 3. Call domain factory (service responsibility, not mapper)
    Reservation reservation = Reservation.Reserve(
        referenceNumber: GenerateReferenceNumber(),
        roomId: request.RoomId,
        room: room,
        document: document,
        reservationTimestamp: DateTime.Now
    );

    // 4. Persist and return
    _repository.Add(reservation);
    return ReservationMapper.ToResponse(reservation);
}
```

**Rationale:**
- **Always-valid model** — Impossible to construct invalid `Reservation` in memory
- **Single source of truth** — All validation logic in one place, no scattered validators
- **Service orchestrates** — Service calls factory; mapper is just a helper for DTO→ValueObject conversion
- **Testable** — Unit test `Reservation.Reserve(...)` directly without service/API layers

**Alternative Considered:** Mapper calls factory and returns Reservation — Rejected: conflates mapping with orchestration; service should own the factory call

---

### 9. **Exception Middleware → ProblemDetails Mapping**

**Decision:** Global `ExceptionHandlingMiddleware` catches domain exceptions and maps to HTTP ProblemDetails.

**Mapping:**
- `DomainValidationException` → 400 Bad Request
- `DocumentMismatchException` → 422 Unprocessable Entity
- `ReservationNotFoundException` → 404 Not Found

**Implementation:**
```csharp
// Api/Middleware/ExceptionHandlingMiddleware.cs
public async Task InvokeAsync(HttpContext context, RequestDelegate next)
{
    try
    {
        await next(context);
    }
    catch (DocumentMismatchException ex)
    {
        context.Response.StatusCode = StatusCodes.Status422UnprocessableEntity;
        await context.Response.WriteAsJsonAsync(ProblemDetailsFactory.Create(ex));
    }
    catch (DomainValidationException ex)
    {
        context.Response.StatusCode = StatusCodes.Status400BadRequest;
        await context.Response.WriteAsJsonAsync(ProblemDetailsFactory.Create(ex));
    }
    // ... etc
}
```

**Rationale:**
- **Thin endpoints** — Endpoints don't need try-catch; middleware handles uniformly
- **Consistent error format** — All errors return ProblemDetails (RFC 7807)
- **Domain purity** — Domain throws exceptions; middleware translates to HTTP

**Alternative Considered:** Result<T, Error> pattern (railway-oriented programming) — Rejected: adds complexity; exceptions are idiomatic for validation errors in C#

---

### 10. **Correlation ID Middleware**

**Decision:** `CorrelationIdMiddleware` reads `X-Correlation-Id` from request header; generates GUID if not provided; adds to logger scope; returns in response header.

**Rationale:**
- **Debuggability** — Trace requests across logs without distributed tracing infrastructure
- **UX-friendly** — Generate if missing (frontend doesn't need to send)
- **Standard practice** — Common pattern in ASP.NET Core APIs

**Alternative Considered:** Require clients to send correlation ID (return 400 if missing) — Rejected: burdens frontend; auto-generation is friendlier

---

### 11. **Testing Strategy: Layer-Scoped, No Mocking in Domain Tests**

**Decision:**
- **Domain tests** — Test real domain models; verify `Reservation.Reserve(...)` invariants directly
- **Application tests** — Test service orchestration; mock `IHotelProvider`, `IReservationRepository`
- **Infrastructure tests** — Test provider stubs (determinism, filtering, mapping), repository behavior
- **API tests** — Test HTTP contracts, request/response mapping, middleware, error status codes

**Rationale:**
- **Fast feedback** — Domain tests run instantly (no mocks, no I/O)
- **Clear test scope** — Each layer tests its own responsibility
- **Isolation** — Application tests don't re-test domain logic; Infrastructure tests don't re-test services

**Alternative Considered:** Integration tests (full stack end-to-end) — Rejected: explicitly out of scope (challenge non-goal)

---

## Risks / Trade-offs

### 1. **In-Memory Storage → Data Loss on Restart**
**Risk:** Reservations lost when API stops  
**Mitigation:** Acceptable for challenge scope; note "Future Scope: Persistent storage" in proposal  
**Trade-off:** Simplicity vs durability

### 2. **Static Catalog → Manual Maintenance**
**Risk:** Adding new destinations/rooms requires editing stub data files  
**Mitigation:** Deterministic data is more valuable than dynamic generation for testability  
**Trade-off:** Flexibility vs predictability

### 3. **No RoomId Expiry → Can Reserve Stale RoomId**
**Risk:** Client saves `roomId` from old search, reserves later even if availability changed  
**Mitigation:** Static catalog means availability doesn't change; acceptable for stub providers  
**Trade-off:** Realism vs simplicity

### 4. **No Concurrency Control → Double-Booking Possible**
**Risk:** Two clients reserve same `roomId` simultaneously; both succeed  
**Mitigation:** `ConcurrentDictionary` ensures reference number uniqueness; room double-booking is possible but acceptable for challenge scope  
**Trade-off:** Correctness vs complexity  
**Future Scope:** Room availability locking (check-and-reserve atomic operation)

### 5. **Provider Stub Determinism → Limited Test Scenarios**
**Risk:** Static catalog may not cover all edge cases (e.g., sold-out rooms, price spikes)  
**Mitigation:** Add explicit test-focused catalog entries (e.g., "unavailable" room for BudgetNests filtering test)  
**Trade-off:** Coverage vs maintainability

### 6. **No Currency Conversion → Mixed-Currency Results**
**Risk:** Search results show USD, EUR, GBP mixed; hard to compare prices  
**Mitigation:** Display native currency; sorting by total price still works (assumes similar magnitude)  
**Trade-off:** Realism vs implementation cost  
**Future Scope:** Currency conversion service

### 7. **Static Mappers → Boilerplate Code**
**Risk:** Lots of mapper files (20+ methods across layers)  
**Mitigation:** Challenge scope is small; manual mapping is manageable and explicit  
**Trade-off:** Verbosity vs clarity

---

## Open Questions

### Resolved Questions

1. **Search response fields** — ✅ Include `totalPrice`, `numberOfNights`, `provider`, `location`
2. **Reservation response fields** — ✅ Same structure as search + `referenceNumber`, `document`, `reservationTimestamp`
3. **RoomType enum** — ✅ `Standard`, `Deluxe`, `Suite`
4. **Destination model** — ✅ String property; computed `IsInternational` based on location (India = domestic, non-India = international)
5. **Domestic vs International** — ✅ India cities = domestic, non-India = international
6. **Location field** — ✅ Room has `Location` (country); determines currency and document validation rules

### Minor Questions (Can Defer to Implementation)

1. **Destination case sensitivity** — Should "london", "London", "LONDON" all work?  
   **Suggestion:** Case-insensitive matching; normalize to title case in responses

2. **Document number validation** — Length constraints? Format (alphanumeric only)?  
   **Suggestion:** Non-empty + trimmed + length 5-20 chars; no regex for challenge scope

3. **Correlation ID requirement** — Generate if client doesn't send, or return 400?  
   **Suggestion:** Generate if missing (friendlier for frontend)

4. **Provider filtering placement** — Filter `"available": false` at provider level or service level?  
   **Suggestion:** Provider level (provider responsibility to return only available rooms)

5. **BudgetNests missing fields** — Return `null`, `[]`, or omit `amenities` and `starRating`?  
   **Suggestion:** Return `[]` for amenities, `null` for starRating (explicit vs omission)

6. **Stub data determinism** — Should prices vary by day-of-week, season, or stay constant?  
   **Suggestion:** Fully deterministic (same input → same output) for testability

---

## Migration Plan

**N/A** — This is the initial implementation; no existing system to migrate from.

**Deployment:**
1. Clone repository
2. Run `dotnet restore` and `dotnet build`
3. Run `dotnet test` (all tests pass)
4. Run `dotnet run --project HotelStay.Api`
5. Frontend: `cd hotel-stay-ui && npm install && ng serve`

**Rollback:**  
**N/A** — No previous version to roll back to.

---

## Next Steps

1. ✅ **Create specs** — 3 endpoint-focused spec files created:
   - `specs/hotel-search.md` — Search endpoint behavior with provider aggregation, filtering, pricing
   - `specs/hotel-reservation.md` — Reserve endpoint behavior with document validation rules (India vs non-India)
   - `specs/reservation-lookup.md` — Lookup endpoint behavior with reference number validation

2. Create **tasks.md** breaking down implementation into checkboxes (grouped by Domain, Application, Infrastructure, API, Tests, Documentation)

3. **Implement** via `/opsx:apply` (or manually following tasks)

4. **Archive** with `/opsx:archive` to merge specs into `openspec/specs/`
