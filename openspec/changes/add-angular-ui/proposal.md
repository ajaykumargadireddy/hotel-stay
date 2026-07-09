# Proposal: Add Angular UI for Hotel Booking

**Change ID:** `add-angular-ui`  
**Type:** Frontend Implementation  
**Status:** Proposed  
**Created:** 2026-07-08

---

## Problem Statement

The Hotel Stay application currently has a fully functional backend API but lacks a user interface. Users need a modern, responsive web application to:
- Search for available hotel rooms across multiple providers
- View and compare results with pricing and amenities
- Make reservations with proper document validation
- Receive confirmation with reservation details

Without a frontend, the application cannot be used by end users and doesn't meet the challenge requirements specified in GitHub issue #1.

---

## Proposed Solution

Implement a **complete Angular application** (latest stable version) with standalone components that provides:

1. **Hotel Search Interface** — Form-based search with destination, dates, and room type filters
2. **Results Display** — Sortable list of available rooms with provider details, pricing, and amenities
3. **Reservation Flow** — Multi-step booking process with document validation
4. **Confirmation Screen** — Reservation summary with reference number
5. **Reservation Lookup** — Retrieve existing reservations by reference

### Technology Stack

- **Framework:** Angular (latest stable version with standalone components, signals)
- **Styling:** Tailwind CSS or Angular Material
- **HTTP Client:** Angular HttpClient with interceptors
- **Routing:** Angular Router with lazy loading
- **State Management:** Angular services with RxJS
- **Form Validation:** Reactive Forms with custom validators
- **Testing:** Jasmine + Karma (unit), Playwright (e2e)

---

## User Journeys

### Journey 1: Search and Book a Room

1. User lands on home page with search form
2. User selects country from dropdown
3. User selects city (destination) from filtered dropdown
4. User enters check-in and check-out dates
4. User optionally selects room type filter
5. System validates dates (check-out after check-in)
6. User clicks "Search"
7. System displays loading indicator
8. System shows results sorted by total price
9. User can sort by price or filter by provider
10. User clicks "Book Now" on a room
11. System navigates to reservation form
12. User enters guest name, document type, document number
13. System validates document type based on destination
14. User clicks "Confirm Reservation"
15. System displays confirmation with reference number

### Journey 2: View Existing Reservation

1. User navigates to "My Reservations"
2. User enters reservation reference number
3. System retrieves and displays reservation details
4. User sees full booking information including cancellation policy

### Journey 3: Handle Errors

1. User submits search without country or city
2. System shows validation error inline
3. User tries to book international destination with National ID
4. System shows document type validation error
5. User enters invalid reservation reference
6. System shows "Reservation not found" message

---

## Component Architecture

### Pages (Route Components)

```
app/
├── pages/
│   ├── search-page/           # Main search + results page
│   ├── reservation-page/      # Reservation form page
│   ├── confirmation-page/     # Success confirmation page
│   └── lookup-page/           # Reservation lookup page
```

### Feature Components

```
app/
├── components/
│   ├── search-form/           # Destination, dates, room type inputs
│   ├── room-card/             # Individual room display
│   ├── room-list/             # List of room cards with sorting
│   ├── reservation-form/      # Guest + document details form
│   ├── reservation-summary/   # Booking confirmation display
│   └── navbar/                # Navigation header
```

### Shared Components

```
app/
├── shared/
│   ├── components/
│   │   ├── date-picker/       # Custom date input
│   │   ├── dropdown/          # Reusable select component
│   │   ├── button/            # Styled button component
│   │   ├── loading-spinner/   # Loading indicator
│   │   └── error-message/     # Error display component
```

---

## Services

### Core Services

```typescript
app/
├── services/
│   ├── hotel.service.ts          # API calls for search, reserve, lookup
│   ├── lookup.service.ts         # API calls for countries, cities
│   ├── validation.service.ts     # Business logic for document validation
│   └── error-handler.service.ts  # Centralized error handling
```

### Service Responsibilities

**HotelService**
- `searchHotels(request: HotelSearchRequest): Observable<Room[]>`
- `reserveRoom(request: ReservationRequest): Observable<ReservationResponse>`
- `getReservation(reference: string): Observable<ReservationResponse>`

**LookupService**
- `getCountries(): Observable<Country[]>`
- `getCities(countryCode?: string): Observable<City[]>`
- Cache lookup data in memory

**ValidationService**
- `isDocumentTypeValid(destination: string, documentType: DocumentType): boolean`
- `validateDateRange(checkIn: Date, checkOut: Date): ValidationResult`
- Client-side validation matching server-side rules

---

## Data Models (TypeScript Interfaces)

```typescript
// models/room.model.ts
export interface Room {
  roomId: string;
  provider: string;
  destination: string;
  location: string;
  roomType: RoomType;
  checkIn: Date;
  checkOut: Date;
  perNightRate: number;
  currency: string;
  cancellationPolicy: string;
  amenities: string[];
  starRating?: number;
  numberOfNights: number;
  totalPrice: number;
}

// models/reservation.model.ts
export interface ReservationRequest {
  roomId: string;
  checkIn: Date;
  checkOut: Date;
  document: Document;
}

export interface ReservationResponse {
  referenceNumber: string;
  roomId: string;
  destination: string;
  location: string;
  roomType: RoomType;
  roomCheckIn: Date;
  roomCheckOut: Date;
  checkIn: Date;
  checkOut: Date;
  numberOfNights: number;
  totalPrice: number;
  currency: string;
  provider: string;
  document: Document;
  reservationTimestamp: string;
}

// models/document.model.ts
export interface Document {
  holderName: string;
  type: DocumentType;
  number: string;
}

// models/enums.ts
export enum RoomType {
  Standard = 'Standard',
  Deluxe = 'Deluxe',
  Suite = 'Suite'
}

export enum DocumentType {
  Passport = 'Passport',
  NationalId = 'NationalId'
}

// models/lookup.model.ts
export interface Country {
  code: string;
  name: string;
}

export interface City {
  code: string;
  name: string;
  countryCode: string;
}
```

---

## Routing Structure

```typescript
const routes: Routes = [
  {
    path: '',
    component: SearchPageComponent,
    title: 'Search Hotels'
  },
  {
    path: 'reserve/:roomId',
    component: ReservationPageComponent,
    title: 'Complete Reservation'
  },
  {
    path: 'confirmation/:reference',
    component: ConfirmationPageComponent,
    title: 'Booking Confirmed'
  },
  {
    path: 'reservations',
    component: LookupPageComponent,
    title: 'My Reservations'
  },
  {
    path: '**',
    redirectTo: ''
  }
];
```

---

## Key Features

### 1. Search Form Validation
- **Required fields:** Country, City, Check-in, Check-out
- **Date validation:** Check-out must be after check-in
- **Minimum stay:** At least 1 night
- **Date range:** Cannot select past dates
- **Real-time validation:** Show errors as user types

### 2. Results Display
- **Default sort:** Total price (ascending)
- **Sort options:** Price, Star rating, Provider
- **Empty state:** "No rooms available for your search"
- **Loading state:** Skeleton loaders or spinner
- **Provider badge:** Visual indicator for PremierStays vs BudgetNests
- **Room details:** All fields from API displayed clearly

### 3. Document Validation
- **Dynamic validation:** Document type options based on destination
- **International destinations:** Only Passport allowed
- **Domestic destinations:** Passport OR National ID allowed
- **Client-side validation:** Immediate feedback
- **Server-side validation:** Handle 422 errors gracefully

### 4. Responsive Design
- **Mobile-first approach:** Works on phones, tablets, desktops
- **Breakpoints:** 640px (mobile), 768px (tablet), 1024px (desktop)
- **Touch-friendly:** Large tap targets, swipe gestures
- **Accessible:** WCAG 2.1 AA compliance

### 5. Error Handling
- **Network errors:** Retry mechanism with user feedback
- **Validation errors:** Clear inline messages
- **404 errors:** User-friendly "not found" pages
- **500 errors:** Generic error message with support info

---

## API Integration

### HTTP Interceptor
```typescript
// interceptors/api.interceptor.ts
export const apiInterceptor: HttpInterceptorFn = (req, next) => {
  // Add base URL
  const apiReq = req.clone({
    url: `${environment.apiUrl}${req.url}`
  });
  
  // Add correlation ID
  const correlationId = crypto.randomUUID();
  const reqWithHeader = apiReq.clone({
    setHeaders: {
      'X-Correlation-Id': correlationId
    }
  });
  
  // Handle errors globally
  return next(reqWithHeader).pipe(
    catchError((error: HttpErrorResponse) => {
      // Log error with correlation ID
      console.error(`[${correlationId}] API Error:`, error);
      return throwError(() => error);
    })
  );
};
```

### Environment Configuration
```typescript
// environments/environment.ts
export const environment = {
  production: false,
  apiUrl: 'http://localhost:5263'
};

// environments/environment.prod.ts
export const environment = {
  production: true,
  apiUrl: '/api' // Configure for production deployment
};
```

---

## Testing Strategy

### Unit Tests (Jasmine + Karma)
- **Component tests:** User interactions, input validation, output emissions
- **Service tests:** API calls, error handling, data transformation
- **Pipe tests:** Custom pipes for date formatting, currency
- **Validator tests:** Custom form validators
- **Coverage target:** 80%+ code coverage

### E2E Tests (Playwright)
- **Happy path:** Complete booking flow from search to confirmation
- **Error scenarios:** Invalid inputs, network failures
- **Edge cases:** Date boundaries, document validation
- **Cross-browser:** Chrome, Firefox, Safari
- **Responsive:** Mobile and desktop viewports

### Test Examples
```typescript
// search-form.component.spec.ts
describe('SearchFormComponent', () => {
  it('should disable search button when destination is empty', () => {
    component.searchForm.controls['destination'].setValue('');
    expect(component.searchForm.invalid).toBeTruthy();
  });
  
  it('should show error when check-out is before check-in', () => {
    component.searchForm.patchValue({
      checkIn: new Date('2026-08-05'),
      checkOut: new Date('2026-08-01')
    });
    expect(component.searchForm.hasError('invalidDateRange')).toBeTruthy();
  });
});

// hotel.service.spec.ts
describe('HotelService', () => {
  it('should return rooms from search API', (done) => {
    const mockResponse: Room[] = [/* mock data */];
    service.searchHotels(mockRequest).subscribe(rooms => {
      expect(rooms.length).toBe(2);
      expect(rooms[0].provider).toBe('PremierStays');
      done();
    });
    
    const req = httpMock.expectOne('/hotels/search?...');
    req.flush(mockResponse);
  });
});
```

---

## Project Structure

```
hotel-stay-ui/
├── src/
│   ├── app/
│   │   ├── components/           # Shared/feature components
│   │   ├── pages/                # Route components
│   │   ├── services/             # API and business logic services
│   │   ├── models/               # TypeScript interfaces
│   │   ├── validators/           # Custom form validators
│   │   ├── interceptors/         # HTTP interceptors
│   │   ├── pipes/                # Custom pipes
│   │   ├── guards/               # Route guards (if needed)
│   │   ├── app.component.ts      # Root component
│   │   ├── app.config.ts         # App configuration
│   │   └── app.routes.ts         # Route definitions
│   ├── assets/                   # Images, fonts, icons
│   ├── environments/             # Environment configs
│   ├── styles.scss               # Global styles
│   └── index.html                # Entry HTML
├── e2e/                          # Playwright tests
├── angular.json                  # Angular CLI config
├── tsconfig.json                 # TypeScript config
├── tailwind.config.js            # Tailwind CSS config
├── package.json                  # Dependencies
└── README.md                     # Setup instructions
```

---

## Development Approach

### Phase 1: Project Setup (Task 1)
- Initialize Angular project with standalone components
- Configure Tailwind CSS or Angular Material
- Set up HTTP interceptor
- Configure environment files
- Add ESLint and Prettier

### Phase 2: Lookup Services (Task 2)
- Implement LookupService
- Create shared dropdown component
- Test API integration for countries/cities

### Phase 3: Search Feature (Tasks 3-4)
- Create search form component
- Implement date validation
- Create room card and room list components
- Implement sorting functionality
- Add loading and empty states

### Phase 4: Reservation Feature (Tasks 5-6)
- Create reservation form component
- Implement document validation logic
- Handle reservation API calls
- Navigate to confirmation page

### Phase 5: Confirmation & Lookup (Tasks 7-8)
- Create confirmation page
- Create reservation lookup page
- Display reservation details

### Phase 6: Polish & Testing (Tasks 9-10)
- Add responsive design
- Implement error handling
- Write unit tests
- Write E2E tests
- Update documentation

---

## Success Criteria

### Functional
- ✅ User can search for hotels with valid form inputs
- ✅ System displays results sorted by price
- ✅ User can complete reservation with valid document
- ✅ System shows confirmation with reference number
- ✅ User can retrieve reservation by reference
- ✅ System validates document type based on destination
- ✅ System handles all error scenarios gracefully

### Technical
- ✅ Application runs on `npm start` from clean clone
- ✅ All API endpoints integrate correctly
- ✅ Code follows Angular style guide
- ✅ Unit test coverage > 80%
- ✅ E2E tests cover happy path
- ✅ Responsive design works on mobile and desktop
- ✅ No console errors in browser
- ✅ Accessible (keyboard navigation, screen readers)

### User Experience
- ✅ Intuitive navigation and flow
- ✅ Clear error messages
- ✅ Fast loading times (< 3s)
- ✅ Visual feedback for all actions
- ✅ Professional, modern UI design

---

## Open Questions

1. **Styling Framework:** Tailwind CSS (utility-first) vs Angular Material (component library)?
   - **Recommendation:** Tailwind CSS for custom design flexibility
   
2. **State Management:** Simple services vs NgRx?
   - **Recommendation:** Start with services + RxJS, add NgRx only if complexity increases
   
3. **Date Picker:** Native HTML5 vs third-party library (e.g., ngx-daterangepicker)?
   - **Recommendation:** Start with native HTML5 date input for simplicity
   
4. **Deployment:** How should the Angular app be served?
   - **Option A:** Separate dev server (ng serve) with CORS enabled on .NET API
   - **Option B:** Serve Angular from .NET API's wwwroot folder
   - **Recommendation:** Option A during development, Option B for production

5. **Error Recovery:** Should search results be cached to allow back navigation?
   - **Recommendation:** Yes, use Angular Router state to preserve search results

---

## Dependencies

### Production Dependencies
```json
{
  "@angular/core": "^17.0.0",
  "@angular/common": "^17.0.0",
  "@angular/router": "^17.0.0",
  "@angular/forms": "^17.0.0",
  "rxjs": "^7.8.0",
  "tslib": "^2.6.0"
}
```

### Development Dependencies
```json
{
  "@angular/cli": "^17.0.0",
  "@angular/compiler-cli": "^17.0.0",
  "typescript": "~5.2.0",
  "@playwright/test": "^1.40.0",
  "karma": "~6.4.0",
  "jasmine-core": "~5.1.0"
}
```

---

## Timeline Estimate

| Phase | Tasks | Estimated Effort |
|-------|-------|------------------|
| Project Setup | Angular init, config, base structure | 2-3 hours |
| Lookup Services | API integration, shared components | 2-3 hours |
| Search Feature | Form, validation, results display | 4-6 hours |
| Reservation Feature | Form, validation, API integration | 3-4 hours |
| Confirmation & Lookup | Display pages, API integration | 2-3 hours |
| Polish & Testing | Responsive design, tests, docs | 4-6 hours |
| **Total** | | **17-25 hours** |

---

## Next Steps

1. Review and approve this proposal
2. Create design document with UI mockups
3. Create detailed specifications for each component
4. Generate task checklist from specifications
5. Begin implementation following OpenSpec workflow

---

## References

- [GitHub Issue #1](https://github.com/ajaykumargadireddy/hotel-stay/issues/1) — Original challenge requirements
- [API Documentation](../../../API.md) — Backend API specification
- [Angular Documentation](https://angular.io/docs) — Official Angular guide
- [Angular Style Guide](https://angular.io/guide/styleguide) — Best practices
