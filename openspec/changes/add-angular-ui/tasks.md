# Tasks: Angular UI for Hotel Booking

Implementation tasks for the Angular UI change, grouped by layer.

---

## 1. Project Setup

### 1.1 Angular Project Initialization

- [x] 1.1.1 Run `npx @angular/cli@latest new hotel-stay-ui` in workspace root (standalone components, SCSS, no SSR)
- [x] 1.1.2 Verify app runs on `http://localhost:4200` via `ng serve`
- [x] 1.1.3 Enable CORS on `.NET` API for `http://localhost:4200`

### 1.2 Environment & HTTP Configuration

- [x] 1.2.1 Create `src/environments/environment.ts` with `apiUrl: 'http://localhost:5263'`
- [x] 1.2.2 Create `src/environments/environment.prod.ts` with production `apiUrl`
- [x] 1.2.3 Create `src/app/interceptors/api.interceptor.ts` to prepend base URL and add `X-Correlation-Id` header
- [x] 1.2.4 Register interceptor in `app.config.ts` via `provideHttpClient(withInterceptors(...))`

---

## 2. Models Layer

### 2.1 Enums

- [x] 2.1.1 Create `RoomType` enum with values: Standard, Deluxe, Suite
- [x] 2.1.2 Create `DocumentType` enum with values: Passport, NationalId

### 2.2 Interfaces

- [x] 2.2.1 Create `Document` interface with properties: holderName, type, number
- [x] 2.2.2 Create `Room` interface with properties: roomId, provider, destination, location, roomType, checkIn, checkOut, perNightRate, currency, cancellationPolicy, amenities, starRating, numberOfNights, totalPrice
- [x] 2.2.3 Create `HotelSearchRequest` interface with properties: destination, checkIn, checkOut, roomType
- [x] 2.2.4 Create `ReservationRequest` interface with properties: roomId, document
- [x] 2.2.5 Create `ReservationResponse` interface with flattened room fields + document + referenceNumber + reservationTimestamp
- [x] 2.2.6 Create `Country` interface with properties: code, name
- [x] 2.2.7 Create `City` interface with properties: code, name, countryCode

---

## 3. Services Layer

### 3.1 HotelService

- [x] 3.1.1 Generate `HotelService` via `ng g service services/hotel`
- [x] 3.1.2 Implement `searchHotels(request: HotelSearchRequest): Observable<Room[]>` calling `GET /hotels/search`
- [x] 3.1.3 Implement `reserveRoom(request: ReservationRequest): Observable<ReservationResponse>` calling `POST /hotels/reserve`
- [x] 3.1.4 Implement `getReservation(reference: string): Observable<ReservationResponse>` calling `GET /hotels/reservation/{reference}`

### 3.2 LookupService

- [x] 3.2.1 Generate `LookupService` via `ng g service services/lookup`
- [x] 3.2.2 Implement `getCountries(): Observable<Country[]>` calling `GET /countries`
- [x] 3.2.3 Implement `getCities(countryCode?: string): Observable<City[]>` calling `GET /cities?countryCode={code}`
- [x] 3.2.4 Add in-memory caching for countries and cities

---

## 4. Shared Components

### 4.1 RoomCardComponent

- [x] 4.1.1 Generate component: `ng g component components/room-card --standalone`
- [x] 4.1.2 Add `@Input() room: Room` and `@Output() book: EventEmitter<Room>`
- [x] 4.1.3 Display provider badge, room type, destination/location, per-night rate, total price
- [x] 4.1.4 Display cancellation policy, amenities (if present), star rating (if present)
- [x] 4.1.5 Add "Book Now" button that emits `book` event

---

## 5. Search Feature

### 5.1 SearchFormComponent

- [x] 5.1.1 Generate component: `ng g component components/search-form --standalone`
- [x] 5.1.2 Create reactive form with fields: country, city, checkIn, checkOut, roomType
- [x] 5.1.3 Populate country dropdown from `LookupService.getCountries()`
- [x] 5.1.4 Populate city dropdown from `LookupService.getCities(countryCode)` when country changes
- [x] 5.1.5 Disable city dropdown until country is selected; reset city when country changes
- [x] 5.1.6 Add validators: country required, city required, checkIn required (not in past), checkOut required (after checkIn)
- [x] 5.1.7 Implement custom `dateRangeValidator` for cross-field date validation
- [x] 5.1.8 Emit `search: EventEmitter<HotelSearchRequest>` on valid submit

### 5.2 RoomListComponent

- [x] 5.2.1 Generate component: `ng g component components/room-list --standalone`
- [x] 5.2.2 Add `@Input() rooms: Room[]` and `@Input() loading: boolean`
- [x] 5.2.3 Add `@Output() book: EventEmitter<Room>` forwarded from RoomCardComponent
- [x] 5.2.4 Implement sort by total price (asc/desc toggle)
- [x] 5.2.5 Display loading spinner when `loading` is true
- [x] 5.2.6 Display empty state "No rooms available" when rooms array is empty
- [x] 5.2.7 Render RoomCardComponent for each room in responsive grid

### 5.3 SearchPageComponent

- [x] 5.3.1 Generate component: `ng g component pages/search-page --standalone`
- [x] 5.3.2 Include SearchFormComponent and RoomListComponent
- [x] 5.3.3 Call `HotelService.searchHotels()` on search event, manage loading state
- [x] 5.3.4 Handle book event: navigate to `/reserve/{roomId}` with room data in router state
- [x] 5.3.5 Handle API errors and display error message

---

## 6. Reservation Feature

### 6.1 ReservationFormComponent

- [x] 6.1.1 Generate component: `ng g component components/reservation-form --standalone`
- [x] 6.1.2 Add `@Input() destination: string` and `@Input() isInternational: boolean`
- [x] 6.1.3 Create reactive form with fields: holderName, documentType, documentNumber
- [x] 6.1.4 Add validators: all fields required, holderName min 2 chars, documentNumber min 5 chars
- [x] 6.1.5 Filter document type options based on destination (international → Passport only, domestic → both)
- [x] 6.1.6 Display helper text explaining document requirements based on destination
- [x] 6.1.7 Emit `reserve: EventEmitter<ReservationRequest>` on valid submit

### 6.2 ReservationPageComponent

- [x] 6.2.1 Generate component: `ng g component pages/reservation-page --standalone`
- [x] 6.2.2 Read roomId from route params and room data from router state
- [x] 6.2.3 Display selected room summary (destination, dates, price)
- [x] 6.2.4 Include ReservationFormComponent with destination and isInternational inputs
- [x] 6.2.5 Call `HotelService.reserveRoom()` on reserve event
- [x] 6.2.6 Handle 422 error and display validation message inline
- [x] 6.2.7 On success, navigate to `/confirmation/{reference}` with reservation data in router state

---

## 7. Confirmation Feature

### 7.1 ConfirmationPageComponent

- [x] 7.1.1 Generate component: `ng g component pages/confirmation-page --standalone`
- [x] 7.1.2 Read reference from route params and reservation data from router state
- [x] 7.1.3 Display reference number prominently
- [x] 7.1.4 Display provider, total price, cancellation policy (per issue requirements)
- [x] 7.1.5 Display additional details: dates, room type, guest name, document info
- [x] 7.1.6 Add "New Search" button navigating to `/`

---

## 8. Routing & Layout

### 8.1 Routes

- [x] 8.1.1 Configure route `''` → SearchPageComponent (page title: "Search Hotels")
- [x] 8.1.2 Configure route `'reserve/:roomId'` → ReservationPageComponent (page title: "Complete Reservation")
- [x] 8.1.3 Configure route `'confirmation/:reference'` → ConfirmationPageComponent (page title: "Booking Confirmed")
- [x] 8.1.4 Add wildcard route redirecting to `''`

### 8.2 Layout

- [x] 8.2.1 Update `AppComponent` template with header (app title) and router outlet
- [x] 8.2.2 Apply base styling in `styles.scss` (font, colors, spacing)
- [x] 8.2.3 Ensure responsive layout works on mobile and desktop breakpoints

---

## 9. Validators

- [x] 9.1 Create `src/app/validators/date-range.validator.ts` returning `{ invalidDateRange: true }` when checkOut <= checkIn
- [x] 9.2 Create `src/app/validators/past-date.validator.ts` returning `{ pastDate: true }` when date is before today
- [x] 9.3 Create `src/app/validators/document-type.validator.ts` returning `{ invalidDocumentType: true }` when international destination has non-Passport document

---

## 10. Tests

### 10.1 Service Tests

- [x] 10.1.1 Test `HotelService.searchHotels` calls correct endpoint with query params
- [x] 10.1.2 Test `HotelService.reserveRoom` posts request body correctly
- [x] 10.1.3 Test `HotelService.getReservation` calls endpoint with reference number
- [x] 10.1.4 Test `LookupService.getCountries` returns countries from API
- [x] 10.1.5 Test `LookupService.getCities` filters by countryCode when provided
- [x] 10.1.6 Test `LookupService` caches responses on subsequent calls

### 10.2 Component Tests

- [x] 10.2.1 Test `SearchFormComponent` disables submit when form invalid
- [x] 10.2.2 Test `SearchFormComponent` city dropdown disabled until country selected
- [x] 10.2.3 Test `SearchFormComponent` resets city when country changes
- [x] 10.2.4 Test `SearchFormComponent` emits search event with correct payload on submit
- [x] 10.2.5 Test `ReservationFormComponent` restricts document types to Passport for international destinations
- [x] 10.2.6 Test `ReservationFormComponent` allows Passport and NationalId for domestic destinations
- [x] 10.2.7 Test `ReservationFormComponent` emits reserve event with correct payload on submit
- [x] 10.2.8 Test `RoomListComponent` sorts rooms by total price
- [x] 10.2.9 Test `RoomListComponent` shows empty state when rooms array is empty

### 10.3 Validator Tests

- [x] 10.3.1 Test `dateRangeValidator` returns error when checkOut <= checkIn
- [x] 10.3.2 Test `dateRangeValidator` returns null when checkOut > checkIn
- [x] 10.3.3 Test `documentTypeValidator` returns error for international destination with NationalId
- [x] 10.3.4 Test `documentTypeValidator` returns null for valid combinations

---

## 11. Documentation

- [x] 11.1 Create `hotel-stay-ui/README.md` with prerequisites (Node version), install, run, and build commands
- [x] 11.2 Add project structure overview to `hotel-stay-ui/README.md`
- [x] 11.3 Update root `README.md` with section on running the Angular UI alongside the API
- [x] 11.4 Document CORS configuration requirement in root `README.md`
