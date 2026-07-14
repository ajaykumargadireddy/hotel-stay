# Tasks: Hotel Availability API

Implementation tasks for the Hotel Availability API change, grouped by layer.

---

## 1. Domain Layer

### 1.1 Value Objects and Enums

- [x] 1.1.1 Create `RoomType` enum with values: Standard, Deluxe, Suite
- [x] 1.1.2 Create `DocumentType` enum with values: Passport, NationalId
- [x] 1.1.3 Create `Document` value object with properties: HolderName, Type, Number
- [x] 1.1.4 Create `Room` value object with properties: RoomId, Provider, Destination, Location, RoomType, CheckIn, CheckOut, PerNightRate, Currency, CancellationPolicy, Amenities, StarRating

### 1.2 Domain Entities

- [x] 1.2.1 Create `Reservation` entity with properties: ReferenceNumber, Room, Document, ReservationTimestamp
- [x] 1.2.2 Implement `Reservation.Reserve(...)` static factory method with validation: document type vs location (India=domestic, non-India=international), check-out > check-in, document number non-empty
- [x] 1.2.3 Add computed property `NumberOfNights` to Reservation (calculated from Room.CheckIn/CheckOut)
- [x] 1.2.4 Add computed property `TotalPrice` to Reservation (Room.PerNightRate * NumberOfNights)

### 1.3 Domain Exceptions

- [x] 1.3.1 Create `DomainValidationException` for general validation errors (400)
- [x] 1.3.2 Create `DocumentMismatchException` for document type validation errors (422)
- [x] 1.3.3 Create `ReservationNotFoundException` for lookup failures (404)

### 1.4 Domain Abstractions

- [x] 1.4.1 Create `IReservationRepository` interface in Domain with methods: Add(Reservation), GetByReference(string)
- [x] 1.4.2 Add computed properties to Room value object: NumberOfNights (calculated from CheckIn/CheckOut), TotalPrice (PerNightRate * NumberOfNights)

---

## 2. Application Layer

### 2.1 DTOs (Request/Response)

- [x] 2.1.1 Create `HotelSearchRequest` DTO with properties: Destination, CheckIn, CheckOut, RoomType
- [x] 2.1.2 Create `DocumentDto` with properties: HolderName, Type, Number
- [x] 2.1.3 Create `ReservationRequest` DTO with properties: RoomId, Document (DocumentDto)
- [x] 2.1.4 Create `ReservationResponse` DTO with flattened room fields + document + referenceNumber + reservationTimestamp

### 2.2 Application Abstractions

- [x] 2.2.1 Create `IHotelProvider` interface in Application with methods: Search(HotelSearchRequest), GetRoomById(Guid)
- [x] 2.2.2 Create `IHotelSearchService` interface with method: Search(HotelSearchRequest)
- [x] 2.2.3 Create `IReservationService` interface with methods: Reserve(ReservationRequest), GetByReference(string)

### 2.3 Application Services

- [x] 2.3.1 Implement `HotelSearchService`: query all providers in parallel, aggregate results (returns IEnumerable<Room> - no mapping needed)
- [x] 2.3.2 Implement `ReservationService.Reserve`: call GetRoomById on all providers until found, validate roomId exists, call ReservationMapper.ToDocument, call Reservation.Reserve factory, persist via repository, return mapped response
- [x] 2.3.3 Implement `ReservationService.GetByReference`: retrieve from repository, throw ReservationNotFoundException if not found, return mapped response

### 2.4 Mappers

- [x] 2.4.1 Create `ReservationMapper` with methods: ToDocument(DocumentDto) → Document, ToResponse(Reservation) → ReservationResponse (flatten Room fields)
- [x] 2.4.2 Add reference number generation helper: GenerateReferenceNumber() → "REF-{first 8 chars of new GUID}"

---

## 3. Infrastructure Layer

### 3.1 Provider Stub Data

- [x] 3.1.1 Create `PremierStaysStubData` with static catalog Dictionary<Guid, RoomCatalogEntry> for 5+ rooms across India (Mumbai/INR), UK (London/GBP), US (New York/USD), covering all 3 room types
- [x] 3.1.2 Create `BudgetNestsStubData` with static catalog Dictionary<Guid, RoomCatalogEntry> for 5+ rooms across same locations, different GUIDs
- [x] 3.1.3 Ensure catalog entries include Provider, Location, Destination, RoomType, Currency based on location rules

### 3.2 Provider Implementations

- [x] 3.2.1 Implement `PremierStaysProvider`: Search filters catalog by destination (case-insensitive), dates (exact match), room type; returns Room value objects
- [x] 3.2.2 Implement `PremierStaysProvider.GetRoomById`: lookup GUID in catalog, return Room or null
- [x] 3.2.3 Create `PremierStaysMapper` to convert catalog entry → Room value object
- [x] 3.2.4 Implement `BudgetNestsProvider`: Search filters catalog (same logic as PremierStays)
- [x] 3.2.5 Implement `BudgetNestsProvider.GetRoomById`: lookup GUID in catalog, return Room or null
- [x] 3.2.6 Create `BudgetNestsMapper` to convert catalog entry → Room value object

### 3.3 Repository Implementation

- [x] 3.3.1 Implement `InMemoryReservationRepository` using ConcurrentDictionary<string, Reservation> keyed by reference number
- [x] 3.3.2 Implement Add method (stores reservation)
- [x] 3.3.3 Implement GetByReference method (retrieves or returns null)

### 3.4 Dependency Injection

- [x] 3.4.1 Create `ServiceCollectionExtensions` in Infrastructure/DependencyInjection with AddInfrastructure method
- [x] 3.4.2 Register IReservationRepository → InMemoryReservationRepository (singleton)
- [x] 3.4.3 Register IHotelProvider → PremierStaysProvider (singleton)
- [x] 3.4.4 Register IHotelProvider → BudgetNestsProvider (singleton)
- [x] 3.4.5 Register IHotelSearchService → HotelSearchService (scoped)
- [x] 3.4.6 Register IReservationService → ReservationService (scoped)

---

## 4. API Layer

### 4.1 Middleware

- [x] 4.1.1 Implement `CorrelationIdMiddleware`: read X-Correlation-Id from request header, generate GUID if missing, add to logger scope, add to response header
- [x] 4.1.2 Implement `ExceptionHandlingMiddleware`: catch DomainValidationException → 400, DocumentMismatchException → 422, ReservationNotFoundException → 404, return ProblemDetails (RFC 7807)
- [x] 4.1.3 Create `ApplicationBuilderExtensions` with UseCustomMiddleware method to register both middleware
- [x] 4.1.4 Register middleware in Program.cs (after routing, before endpoints)

### 4.2 Endpoints

- [x] 4.2.1 Create `GET /hotels/search` endpoint: bind query params to HotelSearchRequest, call HotelSearchService, return 200 with `{ results: Room[] }` (Room serializes directly)
- [x] 4.2.2 Add validation to search endpoint: required params (destination, checkIn, checkOut, roomType), valid date format, checkOut > checkIn (return 400 ProblemDetails if invalid)
- [x] 4.2.3 Create `POST /hotels/reserve` endpoint: bind JSON body to ReservationRequest, call ReservationService.Reserve, return 201 with ReservationResponse
- [x] 4.2.4 Add validation to reserve endpoint: required fields (roomId, document), document number length 5-20 chars (return 400 ProblemDetails if invalid)
- [x] 4.2.5 Create `GET /hotels/reservation/{referenceNumber}` endpoint: accept any string, call ReservationService.GetByReference, return 200 with ReservationResponse or 404 if not found
- [x] 4.2.6 Add case-insensitive reference number handling in lookup endpoint

### 4.3 API Configuration

- [x] 4.3.1 Update Program.cs: add Infrastructure DI registration, configure JSON serialization (camelCase, ignore nulls), add CORS if needed
- [x] 4.3.2 Update appsettings.json: configure structured logging with correlation ID placeholder

---

## 5. Tests

### 5.1 Domain Tests

- [x] 5.1.1 Test `Reservation.Reserve` succeeds for India location with National ID
- [x] 5.1.2 Test `Reservation.Reserve` succeeds for India location with Passport
- [x] 5.1.3 Test `Reservation.Reserve` succeeds for non-India location with Passport
- [x] 5.1.4 Test `Reservation.Reserve` throws DocumentMismatchException for non-India location with National ID
- [x] 5.1.5 Test `Reservation.Reserve` throws DomainValidationException when checkOut <= checkIn
- [x] 5.1.6 Test `Reservation.Reserve` throws DomainValidationException when document number is empty
- [x] 5.1.7 Test `NumberOfNights` computed property calculates correctly
- [x] 5.1.8 Test `TotalPrice` computed property calculates correctly (perNightRate * numberOfNights)

### 5.2 Application Service Tests

- [x] 5.2.1 Test `HotelSearchService.Search` aggregates results from multiple providers (returns Room[])
- [x] 5.2.2 Test `HotelSearchService.Search` returns empty array when no providers have matching rooms
- [x] 5.2.3 Test Room computed properties: NumberOfNights and TotalPrice calculate correctly
- [x] 5.2.4 Test `ReservationService.Reserve` calls GetRoomById on all providers until found
- [x] 5.2.5 Test `ReservationService.Reserve` throws DomainValidationException when roomId not found in any provider
- [x] 5.2.6 Test `ReservationService.Reserve` generates unique reference number with correct format (REF-{8-char-hex})
- [x] 5.2.7 Test `ReservationService.Reserve` persists reservation via repository
- [x] 5.2.8 Test `ReservationService.GetByReference` retrieves reservation successfully
- [x] 5.2.9 Test `ReservationService.GetByReference` throws ReservationNotFoundException when not found

### 5.3 Infrastructure Tests

- [x] 5.3.1 Test `PremierStaysProvider.Search` filters by destination (case-insensitive)
- [x] 5.3.2 Test `PremierStaysProvider.Search` filters by room type
- [x] 5.3.3 Test `PremierStaysProvider.Search` returns deterministic results (same input → same output)
- [x] 5.3.4 Test `PremierStaysProvider.GetRoomById` returns Room when GUID exists
- [x] 5.3.5 Test `PremierStaysProvider.GetRoomById` returns null when GUID not found
- [x] 5.3.6 Test `BudgetNestsProvider.Search` filters correctly (same tests as PremierStays)
- [x] 5.3.7 Test `BudgetNestsProvider.GetRoomById` returns Room when GUID exists
- [x] 5.3.8 Test `InMemoryReservationRepository.Add` stores reservation
- [x] 5.3.9 Test `InMemoryReservationRepository.GetByReference` retrieves reservation
- [x] 5.3.10 Test `InMemoryReservationRepository.GetByReference` returns null when not found
- [x] 5.3.11 Test `InMemoryReservationRepository` is thread-safe (concurrent Add/Get operations)

---

## 6. Documentation

- [x] 6.1 API documentation available in `API.md` with overview of all endpoints (search, reserve, lookup, countries, cities)
- [x] 6.2 Example requests and responses documented in `API.md` for each endpoint
- [x] 6.3 Local testing instructions provided in `API.md` with Swagger UI access
- [x] 6.4 Error response examples documented in `API.md` for common scenarios
- [x] 6.5 Quick start available via Swagger UI at http://localhost:5263/index.html
