# Spec: Reservation Page (Booking Flow)

## Observable Behavior

The reservation page enables users to complete a room booking by entering guest details and document information with validation based on destination type (domestic vs international).

### Requirements

#### Page Initialization

- **SHALL** receive room data, check-in date, check-out date, and country code via router state
- **SHALL** redirect to home page ("/") if room data is missing from state
- **SHALL** redirect to home page ("/") if check-in or check-out dates are missing
- **SHALL** determine if destination is international based on country code
- **SHALL** display room summary at top of page

#### Room Summary Display

- **SHALL** display selected room details:
  - Provider name with badge
  - Room type (Standard/Deluxe/Suite)
  - Destination and location
  - Star rating (if available)
  - Check-in date (formatted)
  - Check-out date (formatted)
  - Number of nights
  - Per-night rate with currency
  - Total price (prominent)
  - Cancellation policy

#### Reservation Form

- **SHALL** display reservation form with the following fields:
  - Guest Name / Holder Name (required, min 2 characters)
  - Document Type dropdown (required)
  - Document Number (required, min 5 characters, max 20 characters)
- **SHALL** display "Complete Reservation" or "Confirm Booking" submit button
- **SHALL** disable submit button when form is invalid
- **SHALL** disable submit button and show loading state during submission
- **SHALL** preserve form values if submission fails
- **SHALL** display inline validation errors below invalid fields

#### Document Type Validation - International Destination

- **SHALL** show only "Passport" option in document type dropdown for international destinations
- **SHALL** automatically select "Passport" for international destinations
- **SHALL** disable document type dropdown for international destinations (single option)
- **SHALL** display helper text: "International destinations require a passport"
- **SHALL** return 422 error if non-Passport document type is submitted for international destination

#### Document Type Validation - Domestic Destination

- **SHALL** show both "Passport" and "National ID" options for domestic destinations (India)
- **SHALL** allow user to select either "Passport" or "National ID"
- **SHALL** enable document type dropdown with multiple options
- **SHALL** accept both document types for domestic destinations

#### Form Validation

- **SHALL** validate guest name is not empty
- **SHALL** validate guest name has minimum 2 characters
- **SHALL** validate document type is selected
- **SHALL** validate document number is not empty
- **SHALL** validate document number has minimum 5 characters
- **SHALL** validate document number has maximum 20 characters
- **SHALL** display validation errors only after field is touched or form submission attempted
- **SHALL** mark all fields as touched when submit button is clicked with invalid form

#### Submission Flow

- **WHEN** user submits valid form
- **SHALL** call `POST /hotels/reserve` with:
  - `roomId` (from room data)
  - `checkIn` (from router state)
  - `checkOut` (from router state)
  - `document.holderName` (from form)
  - `document.type` (from form - enum: Passport or NationalId)
  - `document.number` (from form)
- **SHALL** display loading indicator during API call
- **SHALL** disable form inputs during submission
- **SHALL** disable submit button during submission
- **SHALL** navigate to `/confirmation/:referenceNumber` on success
- **SHALL** pass reservation response via router state to confirmation page

#### Error Handling

- **SHALL** display error message when API returns 422 (document validation failed)
- **SHALL** display error message when API returns 400 (bad request)
- **SHALL** display error message when API returns 404 (room not found)
- **SHALL** display error message when API returns 500 (server error)
- **SHALL** display generic error message when network error occurs
- **SHALL** enable retry by re-enabling form after error
- **SHALL** keep form values after error for easy correction

#### Accessibility

- **SHALL** enable keyboard navigation through all form fields
- **SHALL** show focus indicators on all focusable elements
- **SHALL** associate labels with form inputs using `for` attribute
- **SHALL** announce validation errors to screen readers
- **SHALL** support Enter key to submit form

---

## Scenarios

### Scenario 1: Load reservation page with domestic room (India)

**GIVEN** user has selected a room in Mumbai, India from search results  
**AND** check-in date is "2026-08-01"  
**AND** check-out date is "2026-08-05"  
**WHEN** user clicks "Book Now"  
**THEN** page navigates to `/reserve/:roomId`  
**AND** room summary is displayed at top showing all room details  
**AND** reservation form is displayed  
**AND** document type dropdown shows "Passport" and "National ID" options  
**AND** document type dropdown is enabled (multiple options)  
**AND** "Passport" is selected by default  
**AND** guest name field is empty  
**AND** document number field is empty  
**AND** submit button is disabled (form is invalid)  
**AND** no validation errors are visible initially  

---

### Scenario 2: Load reservation page with international room (United Kingdom)

**GIVEN** user has selected a room in London, UK from search results  
**AND** check-in date is "2026-09-10"  
**AND** check-out date is "2026-09-15"  
**WHEN** user clicks "Book Now"  
**THEN** page navigates to `/reserve/:roomId`  
**AND** room summary is displayed  
**AND** reservation form is displayed  
**AND** document type dropdown shows only "Passport" option  
**AND** document type dropdown is disabled or appears as read-only  
**AND** "Passport" is automatically selected  
**AND** helper text displays: "International destinations require a passport"  
**AND** submit button is disabled  

---

### Scenario 3: Load reservation page without room data (direct navigation)

**GIVEN** user directly navigates to `/reserve/some-room-id` via URL  
**AND** no room data is present in router state  
**WHEN** page initializes  
**THEN** page redirects to home page "/"  
**AND** no reservation form is displayed  

---

### Scenario 4: Complete reservation for domestic destination with National ID

**GIVEN** user is on reservation page for Mumbai, India room  
**AND** document type dropdown allows "Passport" and "National ID"  
**WHEN** user enters guest name "Rajesh Kumar"  
**AND** user selects document type "National ID"  
**AND** user enters document number "ABCD1234567"  
**AND** user clicks "Confirm Booking"  
**THEN** form is validated successfully  
**AND** API call is made to `POST /hotels/reserve` with:
```json
{
  "roomId": "11111111-3333-3333-3333-333333333333",
  "checkIn": "2026-08-01",
  "checkOut": "2026-08-05",
  "document": {
    "holderName": "Rajesh Kumar",
    "type": "NationalId",
    "number": "ABCD1234567"
  }
}
```
**AND** loading indicator is displayed  
**AND** submit button shows loading state and is disabled  
**AND** form inputs are disabled during submission  
**WHEN** API returns success with reference number "REF-a1b2c3d4"  
**THEN** page navigates to `/confirmation/REF-a1b2c3d4`  
**AND** reservation response is passed via router state  

---

### Scenario 5: Complete reservation for domestic destination with Passport

**GIVEN** user is on reservation page for Mumbai, India room  
**WHEN** user enters guest name "Priya Sharma"  
**AND** user selects document type "Passport"  
**AND** user enters document number "P12345678"  
**AND** user clicks "Confirm Booking"  
**THEN** form is validated successfully  
**AND** API call is made with `document.type: "Passport"`  
**WHEN** API returns success  
**THEN** page navigates to confirmation page  

---

### Scenario 6: Complete reservation for international destination with Passport

**GIVEN** user is on reservation page for London, UK room  
**AND** document type is automatically set to "Passport"  
**WHEN** user enters guest name "John Smith"  
**AND** user enters document number "GB987654321"  
**AND** user clicks "Confirm Booking"  
**THEN** form is validated successfully  
**AND** API call is made with `document.type: "Passport"`  
**WHEN** API returns success  
**THEN** page navigates to confirmation page  

---

### Scenario 7: Form validation - empty guest name

**GIVEN** user is on reservation page  
**WHEN** user focuses on guest name field  
**AND** user leaves guest name field empty (blur event)  
**THEN** validation error is displayed: "Guest name is required"  
**AND** error message appears below guest name field in red  
**AND** submit button remains disabled  

---

### Scenario 8: Form validation - guest name too short

**GIVEN** user is on reservation page  
**WHEN** user enters guest name "A" (1 character)  
**AND** user moves to next field  
**THEN** validation error is displayed: "Guest name must be at least 2 characters"  
**AND** submit button remains disabled  

---

### Scenario 9: Form validation - empty document number

**GIVEN** user is on reservation page  
**WHEN** user enters guest name "John Doe"  
**AND** user selects document type "Passport"  
**AND** user leaves document number field empty  
**AND** user clicks submit button  
**THEN** validation error is displayed: "Document number is required"  
**AND** error appears below document number field  
**AND** form is not submitted  
**AND** all fields are marked as touched  

---

### Scenario 10: Form validation - document number too short

**GIVEN** user is on reservation page  
**WHEN** user enters guest name "Jane Smith"  
**AND** user enters document number "ABC" (3 characters)  
**AND** user moves to next field  
**THEN** validation error is displayed: "Document number must be at least 5 characters"  
**AND** submit button remains disabled  

---

### Scenario 11: Form validation - document number too long

**GIVEN** user is on reservation page  
**WHEN** user enters document number "ABCDEFGHIJ1234567890X" (21 characters)  
**AND** user moves to next field  
**THEN** validation error is displayed: "Document number must be at most 20 characters"  
**AND** submit button remains disabled  

---

### Scenario 12: API error - document validation failed (422)

**GIVEN** user is on reservation page for international destination  
**WHEN** user completes form with valid data  
**AND** user clicks "Confirm Booking"  
**AND** API returns 422 status with error:
```json
{
  "status": 422,
  "title": "Document Validation Error",
  "detail": "International destinations require Passport"
}
```
**THEN** loading state disappears  
**AND** error message is displayed at top of form: "Document validation failed."  
**OR** specific error message from API: "International destinations require Passport"  
**AND** form is re-enabled for correction  
**AND** form values are preserved  
**AND** submit button is re-enabled  

---

### Scenario 13: API error - room not found (404)

**GIVEN** user is on reservation page  
**WHEN** user submits valid reservation form  
**AND** API returns 404 status (room no longer available)  
**THEN** loading state disappears  
**AND** error message is displayed: "Room not found or no longer available"  
**AND** form is re-enabled  
**AND** user can navigate back to search or retry  

---

### Scenario 14: API error - server error (500)

**GIVEN** user is on reservation page  
**WHEN** user submits valid reservation form  
**AND** API returns 500 status  
**THEN** loading state disappears  
**AND** error message is displayed: "Unable to complete reservation. Please try again."  
**AND** form is re-enabled for retry  

---

### Scenario 15: API error - network error

**GIVEN** user is on reservation page  
**WHEN** user submits valid reservation form  
**AND** network request fails (no internet connection)  
**THEN** loading state disappears  
**AND** error message is displayed: "Unable to complete reservation. Please check your connection."  
**AND** form is re-enabled  

---

### Scenario 16: Clear validation errors when correcting fields

**GIVEN** user has submitted form with invalid data  
**AND** validation errors are displayed  
**WHEN** user corrects guest name field  
**AND** guest name now meets validation requirements  
**THEN** validation error for guest name disappears  
**AND** submit button becomes enabled when all fields are valid  

---

### Scenario 17: Form enables submit button when all fields valid

**GIVEN** user is on reservation page  
**AND** all fields are initially empty  
**AND** submit button is disabled  
**WHEN** user enters valid guest name "Sarah Johnson"  
**AND** user selects document type "Passport"  
**AND** user enters valid document number "P123456789"  
**THEN** submit button becomes enabled  
**AND** submit button shows normal state (not disabled)  

---

## Data Models

### ReservationRequest (sent to API)

```typescript
{
  roomId: string;           // UUID of selected room
  checkIn: string;          // ISO date string (e.g., "2026-08-01")
  checkOut: string;         // ISO date string
  document: {
    holderName: string;     // Guest name (min 2 chars)
    type: "Passport" | "NationalId";
    number: string;         // Document number (5-20 chars)
  }
}
```

### ReservationResponse (from API)

```typescript
{
  referenceNumber: string;  // Format: "REF-xxxxxxxx"
  room: { /* full room object */ };
  document: { /* document object */ };
  checkIn: string;
  checkOut: string;
  numberOfNights: number;
  totalPrice: number;
  reservationTimestamp: string;
}
```

---

## Navigation

- **From:** Search Page → Click "Book Now" on room card
- **To Success:** Confirmation Page (`/confirmation/:referenceNumber`)
- **To Cancel:** Back to Search Page (`/`) or browser back button
- **Error State:** Remain on reservation page with error message

---

## API Endpoints Used

- `POST /hotels/reserve` — Create new reservation
  - Success: 201 Created
  - Client Error: 400 Bad Request, 422 Unprocessable Entity
  - Not Found: 404 Not Found
  - Server Error: 500 Internal Server Error
