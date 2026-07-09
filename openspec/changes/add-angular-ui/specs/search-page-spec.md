# Spec: Hotel Search Page

## Observable Behavior

The search page enables users to search for available hotel rooms by destination, dates, and optional room type, then displays results with sorting capabilities.

### Requirements

#### Search Form

- **SHALL** display a search form with the following fields:
  - Country dropdown (required)
  - City dropdown (required, disabled until country selected) - this is the destination
  - Check-in date picker (required)
  - Check-out date picker (required)
  - Room type dropdown (optional)
- **SHALL** populate country dropdown with countries from `GET /countries` API
- **SHALL** populate city dropdown with cities from `GET /cities?countryCode={selectedCountry}` API when country is selected
- **SHALL** disable city dropdown until a country is selected
- **SHALL** clear city selection and reset dropdown when country changes
- **SHALL** re-fetch cities when country selection changes
- **SHALL** populate room type dropdown with: "Any", "Standard", "Deluxe", "Suite"
- **SHALL** validate that country is selected before enabling city dropdown
- **SHALL** validate that city is selected before enabling search
- **SHALL** validate that check-in date is not in the past
- **SHALL** validate that check-out date is after check-in date
- **SHALL** display inline validation errors below invalid fields
- **SHALL** disable search button when form is invalid
- **SHALL** show loading indicator on search button during API call
- **SHALL** preserve form values after search

#### Results Display

- **SHALL** display loading spinner while fetching results
- **SHALL** hide loading spinner when results are received
- **SHALL** display "No rooms available" empty state when results array is empty
- **SHALL** display room cards in a grid layout (1 column mobile, 2 tablet, 3 desktop)
- **SHALL** display result count: "{count} rooms available"
- **SHALL** sort results by total price (ascending) by default
- **SHALL** provide sort controls: "Sort by Price", "Sort by Rating"
- **SHALL** update results display when sort option changes

#### Room Card

Each room card **SHALL** display:
- Provider name with badge styling
- Room type (Standard/Deluxe/Suite)
- Destination and location
- Star rating (if available)
- Per-night rate with currency
- Total price (prominent display)
- Number of nights
- Cancellation policy
- Amenities as chips/tags
- "Book Now" button

#### Interactions

- **SHALL** enable keyboard navigation through all interactive elements
- **SHALL** show focus indicators on all focusable elements
- **SHALL** navigate to `/reserve/:roomId` when "Book Now" is clicked
- **SHALL** pass selected room data via router state
- **SHALL** display error message when API call fails
- **SHALL** enable retry when error occurs

---

## Scenarios

### Scenario 1: Initial page load

**GIVEN** user navigates to the home page  
**WHEN** page loads  
**THEN** search form is displayed  
**AND** country dropdown shows placeholder "Select country"  
**AND** city dropdown is disabled with placeholder "Select city"  
**AND** check-in and check-out date pickers are empty  
**AND** room type dropdown shows "Any room type"  
**AND** search button is disabled  
**AND** no results are displayed  
**AND** no loading spinner is visible  

---

### Scenario 2: Country selection enables city dropdown

**GIVEN** user is on the search page  
**AND** city dropdown is disabled  
**WHEN** user selects country "India"  
**THEN** API call is made to `GET /cities?countryCode=IN`  
**AND** city dropdown is enabled  
**AND** city dropdown shows cities: "Mumbai", "Delhi", "Bangalore"  
**AND** city dropdown shows placeholder "Select city"  

---

### Scenario 3: Changing country resets city selection

**GIVEN** user has selected country "India"  
**AND** user has selected city "Mumbai"  
**WHEN** user changes country to "Japan"  
**THEN** API call is made to `GET /cities?countryCode=JP`  
**AND** city selection is cleared  
**AND** city dropdown shows cities: "Tokyo", "Osaka"  
**AND** city dropdown shows placeholder "Select city"  

---

### Scenario 4: Form validation - missing country and city

**GIVEN** user is on the search page  
**WHEN** user selects check-in date "2026-08-01"  
**AND** user selects check-out date "2026-08-05"  
**BUT** user does not select a country  
**THEN** city dropdown remains disabled  
**AND** search button remains disabled  
**AND** no error message is shown (until user touches fields)  

---

### Scenario 5: Form validation - missing city

**GIVEN** user is on the search page  
**WHEN** user selects country "India"  
**AND** user selects check-in date "2026-08-01"  
**AND** user selects check-out date "2026-08-05"  
**BUT** user does not select a city  
**THEN** search button remains disabled  
**AND** error message may be shown if user touched city field  

---

### Scenario 6: Form validation - invalid date range

**GIVEN** user is on the search page  
**WHEN** user selects country "India"  
**AND** user selects city "Mumbai"  
**AND** user selects check-in date "2026-08-05"  
**AND** user selects check-out date "2026-08-01"  
**THEN** error message is displayed: "Check-out date must be after check-in date"  
**AND** search button is disabled  
**AND** error message appears below check-out date field  

---

### Scenario 7: Form validation - past check-in date

**GIVEN** user is on the search page  
**AND** today's date is "2026-07-08"  
**WHEN** user selects country "India"  
**AND** user selects city "Mumbai"  
**AND** user selects check-in date "2026-07-01" (past date)  
**AND** user selects check-out date "2026-07-05"  
**THEN** error message is displayed: "Check-in date cannot be in the past"  
**AND** search button is disabled  

---

### Scenario 8: Successful search with results

**GIVEN** user is on the search page  
**WHEN** user selects country "India"  
**AND** user selects city "Mumbai"  
**AND** user selects check-in date "2026-08-01"  
**AND** user selects check-out date "2026-08-05"  
**AND** user clicks "Search Hotels" button  
**THEN** loading spinner is displayed  
**AND** search button shows loading state  
**AND** search button is disabled  
**WHEN** API returns 2 rooms  
**THEN** loading spinner disappears  
**AND** "2 rooms available" is displayed  
**AND** 2 room cards are displayed in grid layout  
**AND** each room card shows all required information  
**AND** results are sorted by total price (ascending)  
**AND** search form values are preserved  

**Example Room Card Display:**
```
┌─────────────────────────────────────────┐
│ [PremierStays Badge]            ★★★★☆   │
│                                         │
│ Deluxe Room                             │
│ Mumbai - Bandra West                    │
│                                         │
│ $150 / night          Total: $600       │
│                       4 nights          │
│                                         │
│ [WiFi] [TV] [AC] [Mini Bar]             │
│                                         │
│ Cancellation: Free up to 24h before    │
│                                         │
│         [Book Now]                      │
└─────────────────────────────────────────┘
```

---

### Scenario 9: Search with no results

**GIVEN** user is on the search page  
**WHEN** user submits search for destination with no available rooms  
**AND** API returns empty results array  
**THEN** loading spinner disappears  
**AND** empty state is displayed with message: "No rooms available for your search"  
**AND** empty state suggests: "Try changing your dates or destination"  
**AND** no room cards are displayed  

---

### Scenario 10: Sort results by price

**GIVEN** search results are displayed  
**AND** results are initially sorted by price ascending  
**WHEN** user clicks "Sort by Price" button again  
**THEN** results are re-sorted by price descending  
**AND** room cards reorder from highest to lowest price  
**AND** sort button indicates descending order  

---

### Scenario 11: Sort results by rating

**GIVEN** search results are displayed  
**AND** results are sorted by price  
**WHEN** user clicks "Sort by Rating" button  
**THEN** results are re-sorted by star rating descending  
**AND** 5-star rooms appear first  
**AND** rooms without ratings appear last  
**AND** sort button indicates active state  

---

### Scenario 12: Book a room

**GIVEN** search results are displayed  
**WHEN** user clicks "Book Now" on a room card  
**THEN** user is navigated to `/reserve/{roomId}`  
**AND** selected room data is passed via router state  

---

### Scenario 13: API error handling

**GIVEN** user is on the search page  
**WHEN** user submits a valid search  
**AND** API returns 500 Internal Server Error  
**THEN** loading spinner disappears  
**AND** error message is displayed: "Unable to search hotels. Please try again."  
**AND** retry button is displayed  
**WHEN** user clicks retry  
**THEN** search is re-attempted with same parameters  

---

### Scenario 14: Network error handling

**GIVEN** user is on the search page  
**WHEN** user submits a valid search  
**AND** network request fails (no connection)  
**THEN** loading spinner disappears  
**AND** error message is displayed: "Network error. Please check your connection and try again."  
**AND** retry button is displayed  

---

### Scenario 15: Responsive layout - mobile

**GIVEN** user is on the search page  
**AND** viewport width is 375px (mobile)  
**WHEN** search results are displayed  
**THEN** search form fields stack vertically  
**AND** room cards display in single column  
**AND** all content is readable without horizontal scroll  
**AND** touch targets are minimum 44x44px  

---

### Scenario 16: Responsive layout - tablet

**GIVEN** user is on the search page  
**AND** viewport width is 768px (tablet)  
**WHEN** search results are displayed  
**THEN** search form displays 2-3 fields per row  
**AND** room cards display in 2 columns  

---

### Scenario 17: Responsive layout - desktop

**GIVEN** user is on the search page  
**AND** viewport width is 1280px (desktop)  
**WHEN** search results are displayed  
**THEN** search form displays 5 fields in one row (country, city, check-in, check-out, room type)  
**AND** room cards display in 3 columns  

---

### Scenario 18: Accessibility - keyboard navigation

**GIVEN** user is on the search page with results  
**WHEN** user presses Tab key  
**THEN** focus moves through interactive elements in logical order:
1. Country dropdown
2. City dropdown
3. Check-in date picker
4. Check-out date picker
5. Room type dropdown
6. Search button
7. Sort by Price button
8. Sort by Rating button
9. First room's Book Now button
10. Second room's Book Now button
11. etc.  
**AND** focus indicator is clearly visible on focused element  

---

### Scenario 19: Accessibility - screen reader support

**GIVEN** user is using a screen reader  
**WHEN** search results are loaded  
**THEN** screen reader announces: "2 rooms available"  
**AND** each room card is announced with all key information  
**AND** form validation errors are announced immediately  
**AND** loading state is announced: "Searching for hotels..."  

---

## UI Components Breakdown

### SearchPageComponent
**Responsibility:** Container for search form and results

**Children:**
- SearchFormComponent
- RoomListComponent

**State:**
- `searchResults: Room[]`
- `isLoading: boolean`
- `errorMessage: string | null`

---

### SearchFormComponent
**Responsibility:** Collect search criteria with validation and handle country/city cascading dropdowns

**Inputs:** None (uses LookupService internally)

**Outputs:**
- `search: EventEmitter<HotelSearchRequest>`

**State:**
- `searchForm: FormGroup`
- `countries: Country[]` — List of all countries
- `cities: City[]` — Filtered cities based on selected country
- `selectedCountry: string | null`

**Validation Rules:**
- Country: Required
- City: Required, disabled until country selected
- Check-in: Required, not in past
- Check-out: Required, after check-in
- Room type: Optional

**Behavior:**
- When country changes: Clear city selection, fetch cities for new country
- City dropdown disabled until country selected

---

### RoomListComponent
**Responsibility:** Display and sort room results

**Inputs:**
- `rooms: Room[]`
- `loading: boolean`

**Outputs:**
- `book: EventEmitter<Room>`

**State:**
- `sortBy: 'price' | 'rating'`
- `sortOrder: 'asc' | 'desc'`

---

### RoomCardComponent
**Responsibility:** Display single room details

**Inputs:**
- `room: Room`

**Outputs:**
- `book: EventEmitter<Room>`

---

## Acceptance Criteria

- ✅ All form validations work correctly
- ✅ Search calls `/hotels/search` API with correct parameters
- ✅ Results display in responsive grid
- ✅ Sorting functionality works for price and rating
- ✅ Loading states visible during async operations
- ✅ Empty state displays when no results
- ✅ Error handling for API failures
- ✅ Navigation to reservation page works
- ✅ Keyboard navigation supported
- ✅ Screen reader accessible
- ✅ Responsive on mobile, tablet, desktop
- ✅ All touch targets meet 44x44px minimum
- ✅ Unit tests cover all scenarios
- ✅ E2E test covers happy path
