# Spec: Bookings Page (Reservation Lookup)

## Observable Behavior

The bookings page enables users to retrieve existing reservation details by entering a reference number.

### Requirements

#### Page Layout

- **SHALL** display page heading: "My Bookings" or "Find Your Reservation"
- **SHALL** display search form for reference number lookup
- **SHALL** display reservation details when found
- **SHALL** provide option to clear search and start new lookup

#### Search Form

- **SHALL** display single text input field for reference number (required, min 3 characters)
- **SHALL** display "Search" or "Find Reservation" button
- **SHALL** validate reference number field is not empty
- **SHALL** validate reference number has minimum 3 characters
- **SHALL** disable search button when form is invalid
- **SHALL** display inline validation error below field when invalid and touched
- **SHALL** show loading indicator on button during API call
- **SHALL** preserve reference number in input after search

#### Reservation Display (When Found)

- **SHALL** display complete reservation details including:
  - Reference number (prominent display)
  - Guest name
  - Document type and number (masked: "P*****789")
  - Room details (provider, type, destination)
  - Check-in date (formatted)
  - Check-out date (formatted)
  - Number of nights
  - Per-night rate with currency
  - Total price
  - Cancellation policy
  - Reservation timestamp (formatted)
  - Star rating (if available)
  - Amenities
- **SHALL** display "Clear" or "Search Again" button to reset the form
- **SHALL** display "Back to Home" link/button

#### Empty State (Initial)

- **SHALL** display empty state when no search performed
- **SHALL** show placeholder text in input: "Enter reference number (e.g., REF-a1b2c3d4)"
- **SHALL** display helper text: "Enter your reservation reference number to view details"

#### Error Handling

- **SHALL** display error message when reservation not found (404)
- **SHALL** display specific message: "Reservation not found. Please check the reference number."
- **SHALL** display generic error message for other API errors
- **SHALL** keep reference number in input field after error
- **SHALL** allow immediate retry with corrected reference number
- **SHALL** display error with appropriate styling (red border, error icon)

#### Form Behavior

- **SHALL** clear reservation details when "Clear" button is clicked
- **SHALL** clear error message when "Clear" button is clicked
- **SHALL** reset form to initial empty state when cleared
- **SHALL** focus reference number input after clear

#### Accessibility

- **SHALL** enable keyboard navigation
- **SHALL** support Enter key to submit search
- **SHALL** show focus indicators on all interactive elements
- **SHALL** announce errors to screen readers
- **SHALL** associate label with input using `for` attribute

---

## Scenarios

### Scenario 1: Initial page load

**GIVEN** user navigates to bookings page ("/bookings")  
**WHEN** page loads  
**THEN** page heading "My Bookings" is displayed  
**AND** reference number input field is empty  
**AND** input placeholder shows "Enter reference number (e.g., REF-a1b2c3d4)"  
**AND** helper text is displayed: "Enter your reservation reference number to view details"  
**AND** search button is disabled  
**AND** no reservation details are displayed  
**AND** no error message is displayed  

---

### Scenario 2: Form validation - empty reference number

**GIVEN** user is on bookings page  
**WHEN** user clicks in reference number field  
**AND** user leaves field empty (blur event)  
**THEN** validation error is displayed: "Reference number is required"  
**AND** search button remains disabled  

---

### Scenario 3: Form validation - reference number too short

**GIVEN** user is on bookings page  
**WHEN** user enters "AB" (2 characters) in reference number field  
**AND** user moves to next field or blurs input  
**THEN** validation error is displayed: "Reference number must be at least 3 characters"  
**AND** search button remains disabled  

---

### Scenario 4: Form validation - valid reference enables button

**GIVEN** user is on bookings page  
**WHEN** user enters "REF-a1b2c3d4" in reference number field  
**THEN** validation error (if any) disappears  
**AND** search button becomes enabled  

---

### Scenario 5: Successful reservation lookup

**GIVEN** user is on bookings page  
**WHEN** user enters reference number "REF-a1b2c3d4"  
**AND** user clicks "Search" button  
**THEN** loading indicator is displayed on button  
**AND** button is disabled during search  
**AND** API call is made to `GET /hotels/reservation/REF-a1b2c3d4`  
**WHEN** API returns reservation data:
```json
{
  "referenceNumber": "REF-a1b2c3d4",
  "room": {
    "roomId": "11111111-3333-3333-3333-333333333333",
    "provider": "PremierStays",
    "destination": "BOM",
    "location": "India",
    "roomType": 2,
    "perNightRate": 8000,
    "currency": "INR",
    "cancellationPolicy": "FreeCancellation48H",
    "amenities": ["WiFi", "AC", "TV", "Pool"],
    "starRating": 5
  },
  "document": {
    "holderName": "Rajesh Kumar",
    "type": "NationalId",
    "number": "ABCD1234567"
  },
  "checkIn": "2026-08-01T00:00:00",
  "checkOut": "2026-08-05T00:00:00",
  "numberOfNights": 4,
  "totalPrice": 32000,
  "reservationTimestamp": "2026-07-08T14:30:00"
}
```
**THEN** loading indicator disappears  
**AND** reservation details are displayed  
**AND** reference number "REF-a1b2c3d4" is prominently shown  
**AND** guest name "Rajesh Kumar" is displayed  
**AND** document shows "National ID: ABCD1234567" or masked version  
**AND** room details show "PremierStays - Deluxe Room"  
**AND** dates show "Aug 1, 2026 - Aug 5, 2026"  
**AND** nights show "4 nights"  
**AND** total price shows "₹32,000" or "INR 32,000"  
**AND** "Clear" or "Search Again" button is displayed  
**AND** reference number remains in input field  

---

### Scenario 6: Reservation not found (404)

**GIVEN** user is on bookings page  
**WHEN** user enters reference number "REF-notfound"  
**AND** user clicks "Search" button  
**AND** API returns 404 status  
**THEN** loading indicator disappears  
**AND** error message is displayed: "Reservation not found. Please check the reference number."  
**AND** error styling is applied (red border on input, error icon)  
**AND** no reservation details are displayed  
**AND** reference number "REF-notfound" remains in input field  
**AND** search button is re-enabled for retry  

---

### Scenario 7: Server error during lookup (500)

**GIVEN** user is on bookings page  
**WHEN** user enters valid reference number  
**AND** user clicks "Search" button  
**AND** API returns 500 status  
**THEN** loading indicator disappears  
**AND** error message is displayed: "Unable to retrieve reservation. Please try again."  
**AND** reference number remains in input field  
**AND** search button is re-enabled  

---

### Scenario 8: Network error during lookup

**GIVEN** user is on bookings page  
**WHEN** user enters valid reference number  
**AND** user clicks "Search" button  
**AND** network request fails (no connection)  
**THEN** loading indicator disappears  
**AND** error message is displayed: "Unable to connect. Please check your internet connection."  
**AND** search button is re-enabled  

---

### Scenario 9: Clear search results

**GIVEN** user has successfully retrieved a reservation  
**AND** reservation details are displayed  
**WHEN** user clicks "Clear" or "Search Again" button  
**THEN** reservation details disappear  
**AND** reference number input field is cleared  
**AND** form returns to initial empty state  
**AND** error message (if any) is cleared  
**AND** search button is disabled  
**AND** focus moves to reference number input  

---

### Scenario 10: Retry search after error

**GIVEN** user received "Reservation not found" error  
**AND** error message is displayed  
**WHEN** user corrects reference number to "REF-correct123"  
**AND** user clicks "Search" button again  
**THEN** previous error message is cleared  
**AND** new API call is made with corrected reference  
**AND** loading indicator is displayed  

---

### Scenario 11: Search using Enter key

**GIVEN** user is on bookings page  
**WHEN** user enters valid reference number "REF-12345678"  
**AND** user presses Enter key (without clicking button)  
**THEN** search is triggered  
**AND** API call is made to retrieve reservation  
**AND** loading indicator is displayed  

---

### Scenario 12: Navigate back to home

**GIVEN** user is viewing reservation details on bookings page  
**WHEN** user clicks "Back to Home" link  
**THEN** page navigates to home page ("/")  
**AND** user can start new search  

---

### Scenario 13: Direct navigation with reference in URL (optional enhancement)

**GIVEN** system supports URL parameter for reference (e.g., `/bookings?ref=REF-abc123`)  
**WHEN** user navigates to `/bookings?ref=REF-abc123`  
**THEN** reference number field is pre-populated with "REF-abc123"  
**AND** automatic search is triggered  
**AND** reservation details are displayed if found  

---

## Data Models

### API Request

```typescript
GET /hotels/reservation/:reference
// Path parameter: reference number string
```

### API Response (Success - 200)

```typescript
{
  referenceNumber: string;
  room: {
    roomId: string;
    provider: string;
    destination: string;
    location: string;
    roomType: number;  // Enum: 0=Standard, 1=Deluxe, 2=Suite
    checkIn: string;   // ISO date-time
    checkOut: string;  // ISO date-time
    perNightRate: number;
    currency: string;
    cancellationPolicy: string;
    amenities: string[];
    starRating: number;
  };
  document: {
    holderName: string;
    type: string;      // "Passport" or "NationalId"
    number: string;
  };
  checkIn: string;
  checkOut: string;
  numberOfNights: number;
  totalPrice: number;
  reservationTimestamp: string;  // ISO date-time
}
```

### API Response (Error - 404)

```typescript
{
  status: 404,
  title: "Reservation Not Found",
  detail: "Reservation with reference 'REF-notfound' not found"
}
```

---

## Navigation

- **Entry:** Direct navigation to `/bookings` via menu/link
- **Exit:** Back to home `/` or close page

---

## API Endpoints Used

- `GET /hotels/reservation/:reference` — Retrieve reservation by reference number
  - Success: 200 OK
  - Not Found: 404 Not Found
  - Server Error: 500 Internal Server Error
