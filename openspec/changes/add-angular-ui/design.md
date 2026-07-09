# Design: Add Angular UI for Hotel Booking

**Change ID:** `add-angular-ui`  
**Status:** Design Phase  
**Last Updated:** 2026-07-08

---

## Overview

This document outlines the architecture, component design, data flow, and UI/UX decisions for the Angular frontend of the Hotel Stay application.

---

## Architecture Decisions

### 1. Standalone Components Architecture

**Decision:** Use Angular standalone components (latest version) instead of NgModules

**Rationale:**
- Simpler, more modern Angular approach
- Reduced boilerplate code
- Better tree-shaking and bundle sizes
- Easier to understand for new developers
- Official Angular recommendation going forward

**Implementation:**
```typescript
// app.config.ts
export const appConfig: ApplicationConfig = {
  providers: [
    provideRouter(routes),
    provideHttpClient(withInterceptors([apiInterceptor])),
    provideAnimations()
  ]
};
```

### 2. Reactive Forms over Template-Driven Forms

**Decision:** Use Reactive Forms exclusively

**Rationale:**
- More powerful validation capabilities
- Easier to test (pure functions)
- Better type safety with TypeScript
- Required for complex cross-field validation (check-in/check-out dates)
- Better suited for dynamic forms

**Example:**
```typescript
searchForm = this.fb.group({
  country: ['', Validators.required],
  city: [{ value: '', disabled: true }, Validators.required],
  checkIn: ['', Validators.required],
  checkOut: ['', Validators.required],
  roomType: ['']
}, {
  validators: [dateRangeValidator()]
});
```

### 3. RxJS for State Management

**Decision:** Use RxJS BehaviorSubjects and services for state management

**Rationale:**
- Sufficient for application complexity
- No additional learning curve (NgRx)
- Simple and straightforward
- Easy to upgrade to NgRx later if needed

**Pattern:**
```typescript
@Injectable({ providedIn: 'root' })
export class HotelService {
  private searchResultsSubject = new BehaviorSubject<Room[]>([]);
  searchResults$ = this.searchResultsSubject.asObservable();
  
  private loadingSubject = new BehaviorSubject<boolean>(false);
  loading$ = this.loadingSubject.asObservable();
}
```

### 4. Styling with Tailwind CSS

**Decision:** Use Tailwind CSS instead of Angular Material

**Rationale:**
- More design flexibility
- Smaller bundle size
- Easier to create custom UI
- Modern utility-first approach
- No component library lock-in

**Configuration:**
```javascript
// tailwind.config.js
module.exports = {
  content: ['./src/**/*.{html,ts}'],
  theme: {
    extend: {
      colors: {
        primary: '#2563eb',
        secondary: '#7c3aed',
        accent: '#f59e0b'
      }
    }
  }
};
```

---

## Component Hierarchy

```
AppComponent
├── NavbarComponent
│
├── SearchPageComponent
│   ├── SearchFormComponent
│   │   ├── DropdownComponent (destination)
│   │   ├── DatePickerComponent (check-in)
│   │   ├── DatePickerComponent (check-out)
│   │   └── DropdownComponent (room type)
│   │
│   └── RoomListComponent
│       ├── LoadingSpinnerComponent (if loading)
│       ├── EmptyStateComponent (if no results)
│       └── RoomCardComponent (for each room)
│           ├── ProviderBadgeComponent
│           ├── RoomDetailsComponent
│           └── ButtonComponent (Book Now)
│
├── ReservationPageComponent
│   ├── RoomSummaryComponent (selected room details)
│   └── ReservationFormComponent
│       ├── InputComponent (holder name)
│       ├── DropdownComponent (document type)
│       ├── InputComponent (document number)
│       └── ButtonComponent (Confirm)
│
├── ConfirmationPageComponent
│   └── ReservationSummaryComponent
│       ├── ReservationDetailsComponent
│       └── ButtonComponent (View Reservation / New Search)
│
└── LookupPageComponent
    ├── InputComponent (reference number)
    ├── ButtonComponent (Search)
    └── ReservationSummaryComponent (if found)
```

---

## Data Flow Architecture

### Search Flow

```
User Input (SearchFormComponent)
  ↓
Form Validation (Reactive Forms)
  ↓
SearchFormComponent.onSubmit()
  ↓
HotelService.searchHotels(request)
  ↓
HTTP Request → API GET /hotels/search
  ↓
API Response (Room[])
  ↓
HotelService.searchResultsSubject.next(rooms)
  ↓
RoomListComponent (subscribes to searchResults$)
  ↓
Display RoomCardComponent for each room
```

### Reservation Flow

```
User clicks "Book Now" on RoomCardComponent
  ↓
Router.navigate(['/reserve', roomId])
  ↓
ReservationPageComponent loads
  ↓
Retrieve room data from route state
  ↓
Display RoomSummaryComponent
  ↓
User fills ReservationFormComponent
  ↓
Form Validation (document type based on destination)
  ↓
ReservationFormComponent.onSubmit()
  ↓
HotelService.reserveRoom(request)
  ↓
HTTP Request → API POST /hotels/reserve
  ↓
API Response (ReservationResponse)
  ↓
Router.navigate(['/confirmation', referenceNumber], { state })
  ↓
ConfirmationPageComponent displays success
```

### Lookup Flow

```
User enters reference number
  ↓
Form Validation
  ↓
LookupPageComponent.onSearch()
  ↓
HotelService.getReservation(reference)
  ↓
HTTP Request → API GET /hotels/reservation/{reference}
  ↓
API Response (ReservationResponse) or 404
  ↓
Display ReservationSummaryComponent or ErrorMessageComponent
```

---

## Component Specifications

### 1. SearchFormComponent

**Responsibility:** Collect search criteria from user

**Inputs:** None (uses LookupService internally)

**Outputs:**
- `search: EventEmitter<HotelSearchRequest>` — Emits when form is valid and submitted

**State:**
- `searchForm: FormGroup` — Reactive form with validation
- `countries: Country[]` — List of available countries
- `cities: City[]` — Filtered list of cities based on selected country
- `isLoading: boolean` — Loading state during API call

**Validation Rules:**
- Country: Required
- City: Required, disabled until country selected
- Check-in: Required, cannot be in the past
- Check-out: Required, must be after check-in
- Room Type: Optional

**Template Structure:**
```html
<form [formGroup]="searchForm" (ngSubmit)="onSubmit()">
  <div class="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-5 gap-4">
    <!-- Country Dropdown -->
    <app-dropdown
      formControlName="country"
      [options]="countries"
      placeholder="Select country"
      label="Country"
      (change)="onCountryChange()"
    ></app-dropdown>
    
    <!-- City Dropdown -->
    <app-dropdown
      formControlName="city"
      [options]="cities"
      placeholder="Select city"
      label="City (Destination)"
      [disabled]="!searchForm.get('country')?.value"
    ></app-dropdown>
    
    <!-- Check-in Date -->
    <app-date-picker
      formControlName="checkIn"
      label="Check-in"
      [minDate]="today"
    ></app-date-picker>
    
    <!-- Check-out Date -->
    <app-date-picker
      formControlName="checkOut"
      label="Check-out"
      [minDate]="minCheckOutDate"
    ></app-date-picker>
    
    <!-- Room Type Dropdown -->
    <app-dropdown
      formControlName="roomType"
      [options]="roomTypes"
      placeholder="Any room type"
      label="Room Type"
    ></app-dropdown>
  </div>
  
  <button type="submit" [disabled]="searchForm.invalid || isLoading">
    Search Hotels
  </button>
  
  <app-error-message *ngIf="searchForm.errors?.['invalidDateRange']">
    Check-out date must be after check-in date
  </app-error-message>
</form>

<!-- TypeScript -->
```typescript
onCountryChange(): void {
  const selectedCountry = this.searchForm.get('country')?.value;
  if (selectedCountry) {
    this.lookupService.getCities(selectedCountry).subscribe(cities => {
      this.cities = cities;
      this.searchForm.get('city')?.reset();
      this.searchForm.get('city')?.enable();
    });
  } else {
    this.cities = [];
    this.searchForm.get('city')?.disable();
  }
}
```

### 2. RoomCardComponent

**Responsibility:** Display single room result with all details

**Inputs:**
- `room: Room` — Room data to display

**Outputs:**
- `book: EventEmitter<Room>` — Emits when "Book Now" clicked

**Template Structure:**
```html
<div class="room-card border rounded-lg shadow-md p-4 hover:shadow-lg transition">
  <!-- Provider Badge -->
  <app-provider-badge [provider]="room.provider"></app-provider-badge>
  
  <!-- Room Header -->
  <div class="flex justify-between items-start">
    <div>
      <h3 class="text-xl font-bold">{{ room.roomType }}</h3>
      <p class="text-gray-600">{{ room.destination }} - {{ room.location }}</p>
      <div class="flex items-center mt-1">
        <app-star-rating [rating]="room.starRating"></app-star-rating>
      </div>
    </div>
    
    <!-- Pricing -->
    <div class="text-right">
      <p class="text-sm text-gray-600">{{ room.perNightRate | currency }} / night</p>
      <p class="text-2xl font-bold text-primary">{{ room.totalPrice | currency }}</p>
      <p class="text-xs text-gray-500">{{ room.numberOfNights }} nights</p>
    </div>
  </div>
  
  <!-- Amenities -->
  <div class="mt-3 flex flex-wrap gap-2">
    <span *ngFor="let amenity of room.amenities" 
          class="px-2 py-1 bg-gray-100 rounded text-sm">
      {{ amenity }}
    </span>
  </div>
  
  <!-- Cancellation Policy -->
  <div class="mt-3 text-sm text-gray-600">
    <span class="font-semibold">Cancellation:</span> {{ room.cancellationPolicy }}
  </div>
  
  <!-- Action Button -->
  <button (click)="book.emit(room)" 
          class="mt-4 w-full bg-primary text-white py-2 rounded hover:bg-primary-dark">
    Book Now
  </button>
</div>
```

### 3. RoomListComponent

**Responsibility:** Display list of rooms with sorting

**Inputs:**
- `rooms: Room[]` — List of rooms to display
- `loading: boolean` — Loading state

**State:**
- `sortBy: 'price' | 'rating'` — Current sort option
- `sortOrder: 'asc' | 'desc'` — Sort direction

**Methods:**
- `sortRooms(sortBy: string)` — Sort rooms by criteria
- `filterByProvider(provider: string)` — Filter by provider (future enhancement)

**Template Structure:**
```html
<div class="room-list">
  <!-- Loading State -->
  <app-loading-spinner *ngIf="loading"></app-loading-spinner>
  
  <!-- Empty State -->
  <app-empty-state *ngIf="!loading && rooms.length === 0">
    No rooms available for your search criteria
  </app-empty-state>
  
  <!-- Results Header -->
  <div *ngIf="!loading && rooms.length > 0" class="flex justify-between items-center mb-4">
    <h2 class="text-2xl font-bold">{{ rooms.length }} rooms available</h2>
    
    <!-- Sort Controls -->
    <div class="flex gap-2">
      <button (click)="sortRooms('price')" 
              [class.active]="sortBy === 'price'">
        Sort by Price
      </button>
      <button (click)="sortRooms('rating')" 
              [class.active]="sortBy === 'rating'">
        Sort by Rating
      </button>
    </div>
  </div>
  
  <!-- Room Cards Grid -->
  <div class="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6">
    <app-room-card 
      *ngFor="let room of rooms" 
      [room]="room"
      (book)="onBook($event)"
    ></app-room-card>
  </div>
</div>
```

### 4. ReservationFormComponent

**Responsibility:** Collect guest and document details

**Inputs:**
- `destination: string` — Destination to determine valid document types
- `isInternational: boolean` — Whether destination is international

**Outputs:**
- `reserve: EventEmitter<ReservationRequest>` — Emits when form is valid and submitted

**State:**
- `reservationForm: FormGroup` — Reactive form
- `allowedDocumentTypes: DocumentType[]` — Based on destination

**Validation Rules:**
- Holder Name: Required, min 2 chars, only letters and spaces
- Document Type: Required, must be valid for destination
- Document Number: Required, alphanumeric, min 5 chars

**Template Structure:**
```html
<form [formGroup]="reservationForm" (ngSubmit)="onSubmit()">
  <h2 class="text-2xl font-bold mb-4">Guest Details</h2>
  
  <!-- Holder Name -->
  <div class="mb-4">
    <label>Full Name (as on document)</label>
    <input type="text" formControlName="holderName" 
           class="w-full border rounded px-3 py-2">
    <app-error-message *ngIf="holderName.invalid && holderName.touched">
      Please enter a valid name
    </app-error-message>
  </div>
  
  <!-- Document Type -->
  <div class="mb-4">
    <label>Document Type</label>
    <app-dropdown formControlName="documentType" 
                  [options]="allowedDocumentTypes">
    </app-dropdown>
    <p class="text-sm text-gray-600 mt-1">
      <span *ngIf="isInternational">Passport required for international travel</span>
      <span *ngIf="!isInternational">Passport or National ID accepted</span>
    </p>
  </div>
  
  <!-- Document Number -->
  <div class="mb-4">
    <label>Document Number</label>
    <input type="text" formControlName="documentNumber" 
           class="w-full border rounded px-3 py-2">
    <app-error-message *ngIf="documentNumber.invalid && documentNumber.touched">
      Please enter a valid document number
    </app-error-message>
  </div>
  
  <button type="submit" [disabled]="reservationForm.invalid">
    Confirm Reservation
  </button>
</form>
```

### 5. ReservationSummaryComponent

**Responsibility:** Display complete reservation details

**Inputs:**
- `reservation: ReservationResponse` — Reservation data

**Template Structure:**
```html
<div class="reservation-summary bg-white rounded-lg shadow-lg p-6">
  <!-- Success Icon -->
  <div class="text-center mb-6">
    <div class="success-icon">✓</div>
    <h2 class="text-3xl font-bold text-green-600">Booking Confirmed!</h2>
  </div>
  
  <!-- Reference Number -->
  <div class="bg-gray-50 p-4 rounded mb-6 text-center">
    <p class="text-sm text-gray-600">Reservation Reference</p>
    <p class="text-2xl font-mono font-bold">{{ reservation.referenceNumber }}</p>
  </div>
  
  <!-- Booking Details Grid -->
  <div class="grid grid-cols-2 gap-4">
    <div>
      <p class="text-gray-600 text-sm">Provider</p>
      <p class="font-semibold">{{ reservation.provider }}</p>
    </div>
    <div>
      <p class="text-gray-600 text-sm">Room Type</p>
      <p class="font-semibold">{{ reservation.roomType }}</p>
    </div>
    <div>
      <p class="text-gray-600 text-sm">Destination</p>
      <p class="font-semibold">{{ reservation.destination }}</p>
    </div>
    <div>
      <p class="text-gray-600 text-sm">Location</p>
      <p class="font-semibold">{{ reservation.location }}</p>
    </div>
    <div>
      <p class="text-gray-600 text-sm">Check-in</p>
      <p class="font-semibold">{{ reservation.checkIn | date:'mediumDate' }}</p>
    </div>
    <div>
      <p class="text-gray-600 text-sm">Check-out</p>
      <p class="font-semibold">{{ reservation.checkOut | date:'mediumDate' }}</p>
    </div>
    <div>
      <p class="text-gray-600 text-sm">Number of Nights</p>
      <p class="font-semibold">{{ reservation.numberOfNights }}</p>
    </div>
    <div>
      <p class="text-gray-600 text-sm">Total Price</p>
      <p class="font-semibold text-xl text-primary">
        {{ reservation.totalPrice | currency }}
      </p>
    </div>
  </div>
  
  <!-- Guest Details -->
  <div class="mt-6 pt-6 border-t">
    <h3 class="font-bold mb-3">Guest Information</h3>
    <p><span class="text-gray-600">Name:</span> {{ reservation.document.holderName }}</p>
    <p><span class="text-gray-600">Document:</span> 
       {{ reservation.document.type }} - {{ reservation.document.number }}
    </p>
  </div>
  
  <!-- Cancellation Policy -->
  <div class="mt-6 p-4 bg-yellow-50 rounded border border-yellow-200">
    <p class="text-sm font-semibold">Cancellation Policy</p>
    <p class="text-sm">{{ cancellationPolicy }}</p>
  </div>
  
  <!-- Timestamp -->
  <p class="text-center text-sm text-gray-500 mt-6">
    Booked on {{ reservation.reservationTimestamp }}
  </p>
</div>
```

---

## Service Architecture

### HotelService

```typescript
@Injectable({ providedIn: 'root' })
export class HotelService {
  private readonly apiUrl = environment.apiUrl;
  
  private searchResultsSubject = new BehaviorSubject<Room[]>([]);
  searchResults$ = this.searchResultsSubject.asObservable();
  
  private loadingSubject = new BehaviorSubject<boolean>(false);
  loading$ = this.loadingSubject.asObservable();
  
  constructor(private http: HttpClient) {}
  
  searchHotels(request: HotelSearchRequest): Observable<Room[]> {
    this.loadingSubject.next(true);
    
    const params = new HttpParams()
      .set('destination', request.destination)
      .set('checkIn', request.checkIn.toISOString().split('T')[0])
      .set('checkOut', request.checkOut.toISOString().split('T')[0]);
    
    if (request.roomType) {
      params.set('roomType', request.roomType);
    }
    
    return this.http.get<{ results: Room[] }>(`${this.apiUrl}/hotels/search`, { params })
      .pipe(
        map(response => response.results),
        tap(rooms => {
          this.searchResultsSubject.next(rooms);
          this.loadingSubject.next(false);
        }),
        catchError(error => {
          this.loadingSubject.next(false);
          throw error;
        })
      );
  }
  
  reserveRoom(request: ReservationRequest): Observable<ReservationResponse> {
    return this.http.post<ReservationResponse>(
      `${this.apiUrl}/hotels/reserve`,
      request
    );
  }
  
  getReservation(reference: string): Observable<ReservationResponse> {
    return this.http.get<ReservationResponse>(
      `${this.apiUrl}/hotels/reservation/${reference}`
    );
  }
}
```

### LookupService

```typescript
@Injectable({ providedIn: 'root' })
export class LookupService {
  private readonly apiUrl = environment.apiUrl;
  
  // Cache lookup data
  private countriesCache: Country[] | null = null;
  private citiesCache: City[] | null = null;
  
  constructor(private http: HttpClient) {}
  
  getCountries(): Observable<Country[]> {
    if (this.countriesCache) {
      return of(this.countriesCache);
    }
    
    return this.http.get<Country[]>(`${this.apiUrl}/countries`)
      .pipe(
        tap(countries => this.countriesCache = countries)
      );
  }
  
  getCities(countryCode?: string): Observable<City[]> {
    // If cached and no filter, return cache
    if (this.citiesCache && !countryCode) {
      return of(this.citiesCache);
    }
    
    let params = new HttpParams();
    if (countryCode) {
      params = params.set('countryCode', countryCode);
    }
    
    return this.http.get<City[]>(`${this.apiUrl}/cities`, { params })
      .pipe(
        tap(cities => {
          if (!countryCode) {
            this.citiesCache = cities;
          }
        })
      );
  }
}
```

### ValidationService

```typescript
@Injectable({ providedIn: 'root' })
export class ValidationService {
  private internationalDestinations = ['Tokyo', 'London', 'Paris'];
  private domesticDestinations = ['Mumbai', 'Delhi'];
  
  constructor(private lookupService: LookupService) {
    // Initialize destinations from API
    this.loadDestinations();
  }
  
  private loadDestinations(): void {
    this.lookupService.getCities().subscribe(cities => {
      // Classify cities as international or domestic
      // Based on countryCode (assume 'IN' is domestic)
      this.domesticDestinations = cities
        .filter(c => c.countryCode === 'IN')
        .map(c => c.name);
      
      this.internationalDestinations = cities
        .filter(c => c.countryCode !== 'IN')
        .map(c => c.name);
    });
  }
  
  isInternationalDestination(destination: string): boolean {
    return this.internationalDestinations.includes(destination);
  }
  
  isDomesticDestination(destination: string): boolean {
    return this.domesticDestinations.includes(destination);
  }
  
  isDocumentTypeValid(destination: string, documentType: DocumentType): boolean {
    if (this.isInternationalDestination(destination)) {
      // International requires Passport
      return documentType === DocumentType.Passport;
    }
    
    // Domestic accepts both
    return true;
  }
  
  getAllowedDocumentTypes(destination: string): DocumentType[] {
    if (this.isInternationalDestination(destination)) {
      return [DocumentType.Passport];
    }
    
    return [DocumentType.Passport, DocumentType.NationalId];
  }
}
```

---

## Custom Validators

### Date Range Validator

```typescript
export function dateRangeValidator(): ValidatorFn {
  return (control: AbstractControl): ValidationErrors | null => {
    const checkIn = control.get('checkIn')?.value;
    const checkOut = control.get('checkOut')?.value;
    
    if (!checkIn || !checkOut) {
      return null; // Let required validator handle empty fields
    }
    
    const checkInDate = new Date(checkIn);
    const checkOutDate = new Date(checkOut);
    
    if (checkOutDate <= checkInDate) {
      return { invalidDateRange: true };
    }
    
    return null;
  };
}
```

### Document Type Validator

```typescript
export function documentTypeValidator(
  validationService: ValidationService,
  destinationControl: AbstractControl
): ValidatorFn {
  return (control: AbstractControl): ValidationErrors | null => {
    const documentType = control.value;
    const destination = destinationControl.value;
    
    if (!documentType || !destination) {
      return null;
    }
    
    const isValid = validationService.isDocumentTypeValid(
      destination,
      documentType
    );
    
    return isValid ? null : { invalidDocumentType: true };
  };
}
```

---

## Error Handling Strategy

### HTTP Error Interceptor

```typescript
export const errorInterceptor: HttpInterceptorFn = (req, next) => {
  return next(req).pipe(
    catchError((error: HttpErrorResponse) => {
      let errorMessage = 'An unexpected error occurred';
      
      if (error.error instanceof ErrorEvent) {
        // Client-side error
        errorMessage = `Error: ${error.error.message}`;
      } else {
        // Server-side error
        switch (error.status) {
          case 400:
            errorMessage = 'Invalid request. Please check your inputs.';
            break;
          case 404:
            errorMessage = error.error?.detail || 'Resource not found';
            break;
          case 422:
            errorMessage = error.error?.detail || 'Validation error';
            break;
          case 500:
            errorMessage = 'Server error. Please try again later.';
            break;
        }
      }
      
      // Show error to user (could use a toast service)
      console.error('API Error:', errorMessage, error);
      
      return throwError(() => new Error(errorMessage));
    })
  );
};
```

### Component Error Handling Pattern

```typescript
export class SearchPageComponent {
  errorMessage: string | null = null;
  
  onSearch(request: HotelSearchRequest): void {
    this.errorMessage = null;
    
    this.hotelService.searchHotels(request).subscribe({
      next: (rooms) => {
        // Success handling
      },
      error: (error) => {
        this.errorMessage = error.message;
        // Optionally show toast notification
      }
    });
  }
}
```

---

## Responsive Design Breakpoints

```css
/* Mobile: < 640px */
.room-card {
  grid-template-columns: 1fr;
}

/* Tablet: 640px - 1023px */
@media (min-width: 640px) {
  .room-list {
    grid-template-columns: repeat(2, 1fr);
  }
}

/* Desktop: >= 1024px */
@media (min-width: 1024px) {
  .room-list {
    grid-template-columns: repeat(3, 1fr);
  }
  
  .search-form {
    grid-template-columns: repeat(4, 1fr);
  }
}
```

---

## Performance Considerations

### 1. Lazy Loading Routes

```typescript
const routes: Routes = [
  {
    path: '',
    loadComponent: () => import('./pages/search-page/search-page.component')
      .then(m => m.SearchPageComponent)
  },
  {
    path: 'reserve/:roomId',
    loadComponent: () => import('./pages/reservation-page/reservation-page.component')
      .then(m => m.ReservationPageComponent)
  }
];
```

### 2. OnPush Change Detection

```typescript
@Component({
  selector: 'app-room-card',
  standalone: true,
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: '...'
})
export class RoomCardComponent {
  @Input() room!: Room;
}
```

### 3. TrackBy Function for ngFor

```typescript
trackByRoomId(index: number, room: Room): string {
  return room.roomId;
}
```

```html
<app-room-card 
  *ngFor="let room of rooms; trackBy: trackByRoomId"
  [room]="room">
</app-room-card>
```

---

## Accessibility Requirements

### WCAG 2.1 AA Compliance

1. **Keyboard Navigation**
   - All interactive elements accessible via Tab/Shift+Tab
   - Focus indicators visible
   - Logical tab order

2. **ARIA Labels**
   - Form inputs have associated labels
   - Buttons have descriptive text or aria-label
   - Error messages linked to inputs via aria-describedby

3. **Color Contrast**
   - Text minimum 4.5:1 contrast ratio
   - Large text (18pt+) minimum 3:1 contrast ratio
   - Interactive elements have focus indicators

4. **Screen Reader Support**
   - Semantic HTML (nav, main, section)
   - ARIA live regions for dynamic content
   - Image alt text where applicable

**Example:**
```html
<form role="search" aria-label="Hotel search form">
  <label for="destination">Destination</label>
  <input 
    id="destination"
    type="text"
    formControlName="destination"
    aria-required="true"
    aria-describedby="destination-error"
  >
  <div id="destination-error" role="alert" *ngIf="destination.invalid">
    Please select a destination
  </div>
</form>
```

---

## Testing Strategy

### Unit Testing Pattern

```typescript
describe('SearchFormComponent', () => {
  let component: SearchFormComponent;
  let fixture: ComponentFixture<SearchFormComponent>;
  let lookupService: jasmine.SpyObj<LookupService>;
  
  beforeEach(async () => {
    const lookupServiceSpy = jasmine.createSpyObj('LookupService', ['getCities']);
    
    await TestBed.configureTestingModule({
      imports: [SearchFormComponent],
      providers: [
        { provide: LookupService, useValue: lookupServiceSpy }
      ]
    }).compileComponents();
    
    fixture = TestBed.createComponent(SearchFormComponent);
    component = fixture.componentInstance;
    lookupService = TestBed.inject(LookupService) as jasmine.SpyObj<LookupService>;
  });
  
  it('should emit search event when form is valid', () => {
    const searchSpy = jasmine.createSpy('search');
    component.search.subscribe(searchSpy);
    
    component.searchForm.patchValue({
      destination: 'Mumbai',
      checkIn: '2026-08-01',
      checkOut: '2026-08-05',
      roomType: 'Deluxe'
    });
    
    component.onSubmit();
    
    expect(searchSpy).toHaveBeenCalledWith(jasmine.objectContaining({
      destination: 'Mumbai'
    }));
  });
});
```

### E2E Testing Pattern

```typescript
import { test, expect } from '@playwright/test';

test('complete booking flow', async ({ page }) => {
  // Navigate to home page
  await page.goto('http://localhost:4200');
  
  // Fill search form
  await page.selectOption('[name="destination"]', 'Mumbai');
  await page.fill('[name="checkIn"]', '2026-08-01');
  await page.fill('[name="checkOut"]', '2026-08-05');
  await page.click('button[type="submit"]');
  
  // Wait for results
  await expect(page.locator('.room-card')).toBeVisible();
  
  // Click first "Book Now" button
  await page.locator('.room-card').first().locator('button').click();
  
  // Fill reservation form
  await page.fill('[name="holderName"]', 'John Doe');
  await page.selectOption('[name="documentType"]', 'Passport');
  await page.fill('[name="documentNumber"]', 'AB1234567');
  await page.click('button[type="submit"]');
  
  // Verify confirmation page
  await expect(page.locator('text=Booking Confirmed')).toBeVisible();
  await expect(page.locator('.reference-number')).toHaveText(/RSV-\d{8}-\w+/);
});
```

---

## Build and Deployment

### Development Build

```bash
ng serve
# Runs on http://localhost:4200
# Hot reload enabled
```

### Production Build

```bash
ng build --configuration production
# Output: dist/hotel-stay-ui/
# Minified, tree-shaken, optimized
```

### Integration with .NET API

**Option 1: CORS (Development)**
```csharp
// In .NET API Program.cs
builder.Services.AddCors(options => {
    options.AddPolicy("AngularApp", policy => {
        policy.WithOrigins("http://localhost:4200")
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});
```

**Option 2: Serve from .NET (Production)**
```csharp
// Copy Angular build to wwwroot
app.UseStaticFiles();
app.UseRouting();
app.MapFallbackToFile("index.html");
```

---

## Open Design Decisions

### 1. Real-time vs. Manual Refresh
**Question:** Should search results auto-refresh if user changes filters?
**Recommendation:** Manual refresh (search button) for predictable behavior

### 2. Booking Timeout
**Question:** Should there be a timeout for completing reservation after selecting a room?
**Recommendation:** Not initially; add if rooms have limited availability

### 3. Multi-language Support
**Question:** Should UI support multiple languages?
**Recommendation:** English only for MVP; use i18n infrastructure for future

### 4. Pagination
**Question:** Should results be paginated if many rooms returned?
**Recommendation:** Not initially; implement if >20 results become common

---

## Future Enhancements

1. **User Accounts**
   - Login/registration
   - Booking history
   - Saved preferences

2. **Advanced Filtering**
   - Price range slider
   - Amenities filter
   - Star rating filter

3. **Booking Modifications**
   - Cancel reservation
   - Modify dates
   - Change guest details

4. **Notifications**
   - Email confirmation
   - SMS notifications
   - Booking reminders

5. **Reviews and Ratings**
   - User reviews
   - Rating system
   - Photo uploads

---

## References

- [Angular Best Practices](https://angular.io/guide/styleguide)
- [Tailwind CSS Documentation](https://tailwindcss.com/docs)
- [WCAG 2.1 Guidelines](https://www.w3.org/WAI/WCAG21/quickref/)
- [RxJS Best Practices](https://rxjs.dev/guide/overview)
