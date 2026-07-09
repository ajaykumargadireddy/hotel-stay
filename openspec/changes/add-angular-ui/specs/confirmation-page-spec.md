# Spec: Confirmation Page

## Observable Behavior

The confirmation page displays reservation details after successful booking or when accessed via direct URL with reference number.

### Requirements

#### Page Initialization - From Reservation Flow

- **SHALL** receive reservation data via router state after successful booking
- **SHALL** display reservation details immediately when data is in state
- **SHALL** not make API call if reservation data is available in state

#### Page Initialization - Direct URL Access

- **SHALL** extract reference number from route parameter (`:reference`)
- **SHALL** make API call to `GET /hotels/reservation/:reference` if no state data
- **SHALL** display loading spinner while fetching reservation
- **SHALL** redirect to home page ("/") if reference parameter is missing
- **SHALL** handle API errors appropriately

#### Confirmation Display

- **SHALL** display success message: "Booking Confirmed!" or similar
- **SHALL** display checkmark icon or success indicator
- **SHALL** display complete reservation details:
  - Reference number (prominent, large font, copyable)
  - Guest name
  - Document type
  - Document number (optionally masked)
  - Provider name with badge
  - Room type
  - Destination and location
  - Check-in date (formatted: "Aug 1, 2026")
  - Check-out date (formatted: "Aug 5, 2026")
  - Number of nights
  - Per-night rate with currency
  - Total price (prominent)
  - Cancellation policy
  - Reservation timestamp (formatted)
  - Star rating (if available)
  - Amenities as chips/badges
- **SHALL** provide "Copy Reference Number" button/action
- **SHALL** provide "Back to Home" or "Search More Hotels" button
- **SHALL** optionally provide "View My Bookings" link to bookings page

#### Reference Number Display

- **SHALL** display reference number in large, bold text
- **SHALL** provide visual indication it can be copied
- **SHALL** show copy success feedback when copied
- **SHALL** format reference as displayed by API (e.g., "REF-a1b2c3d4")

#### Loading State

- **SHALL** display loading spinner centered on page
- **SHALL** display loading text: "Loading reservation details..."
- **SHALL** disable all interactions during loading

#### Error Handling - Reservation Not Found (404)

- **SHALL** display error heading: "Reservation Not Found"
- **SHALL** display error message: "The reservation REF-{reference} could not be found."
- **SHALL** display "Back to Home" button
- **SHALL** optionally display "Try Another Reference" button linking to bookings page

#### Error Handling - Other Errors

- **SHALL** display generic error heading: "Unable to Load Reservation"
- **SHALL** display error message: "We couldn't retrieve your reservation details. Please try again."
- **SHALL** display "Back to Home" button
- **SHALL** optionally display "Try Again" button to retry API call

#### Post-Booking Actions

- **SHALL** provide clear next steps or call-to-action
- **SHALL** encourage user to save or screenshot reference number
- **SHALL** optionally provide email/print options (future enhancement)

#### Accessibility

- **SHALL** announce success message to screen readers
- **SHALL** enable keyboard navigation
- **SHALL** show focus indicators on interactive elements
- **SHALL** use semantic HTML for success/error states

---

## Scenarios

### Scenario 1: Display confirmation after successful booking

**GIVEN** user has completed reservation form  
**AND** API returned successful reservation response  
**WHEN** user is redirected to `/confirmation/REF-a1b2c3d4`  
**AND** reservation data is passed via router state  
**THEN** confirmation page displays immediately (no loading)  
**AND** success message "Booking Confirmed!" is displayed  
**AND** checkmark icon or success indicator is shown  
**AND** reference number "REF-a1b2c3d4" is displayed prominently  
**AND** complete reservation details are displayed:
  - Guest: "Rajesh Kumar"
  - Document: "National ID: ABCD1234567"
  - Provider: "PremierStays"
  - Room: "Deluxe Room"
  - Location: "Mumbai, India"
  - Check-in: "Aug 1, 2026"
  - Check-out: "Aug 5, 2026"
  - Nights: "4 nights"
  - Total: "₹32,000"
  - Policy: "Free cancellation up to 48 hours before check-in"
**AND** "Copy Reference Number" button is displayed  
**AND** "Back to Home" button is displayed  
**AND** no API call is made (data from state)  

---

### Scenario 2: Direct access via URL - successful load

**GIVEN** user navigates directly to `/confirmation/REF-a1b2c3d4`  
**AND** no reservation data is in router state  
**WHEN** page initializes  
**THEN** loading spinner is displayed  
**AND** loading text "Loading reservation details..." is shown  
**AND** API call is made to `GET /hotels/reservation/REF-a1b2c3d4`  
**WHEN** API returns reservation data  
**THEN** loading spinner disappears  
**AND** confirmation display is shown with all reservation details  
**AND** reference number "REF-a1b2c3d4" is displayed  

---

### Scenario 3: Direct access via URL - reservation not found

**GIVEN** user navigates to `/confirmation/REF-notfound`  
**AND** no reservation data is in state  
**WHEN** page initializes  
**THEN** loading spinner is displayed  
**AND** API call is made to `GET /hotels/reservation/REF-notfound`  
**WHEN** API returns 404 status  
**THEN** loading spinner disappears  
**AND** error heading "Reservation Not Found" is displayed  
**AND** error message is displayed: "The reservation REF-notfound could not be found."  
**AND** "Back to Home" button is displayed  
**AND** optionally "Try Another Reference" button is displayed  

---

### Scenario 4: Direct access via URL - server error

**GIVEN** user navigates to `/confirmation/REF-12345678`  
**WHEN** API returns 500 status  
**THEN** loading spinner disappears  
**AND** error heading "Unable to Load Reservation" is displayed  
**AND** error message: "We couldn't retrieve your reservation details. Please try again."  
**AND** "Back to Home" button is displayed  
**AND** optionally "Try Again" button is displayed  

---

### Scenario 5: Access without reference number in URL

**GIVEN** user navigates to `/confirmation` (no reference parameter)  
**WHEN** page initializes  
**THEN** page redirects to home page "/"  
**AND** no confirmation or error is displayed  

---

### Scenario 6: Copy reference number to clipboard

**GIVEN** user is viewing confirmation page  
**AND** reference number "REF-a1b2c3d4" is displayed  
**WHEN** user clicks "Copy Reference Number" button  
**THEN** reference number is copied to clipboard  
**AND** success feedback is displayed: "Copied!" or checkmark animation  
**AND** feedback disappears after 2-3 seconds  
**AND** button may temporarily show "Copied!" text  

---

### Scenario 7: Navigate back to home from confirmation

**GIVEN** user is viewing confirmation page  
**WHEN** user clicks "Back to Home" or "Search More Hotels" button  
**THEN** page navigates to home page "/"  
**AND** search form is displayed  
**AND** user can start new search  

---

### Scenario 8: Navigate to bookings page from confirmation

**GIVEN** user is viewing confirmation page  
**AND** "View My Bookings" link is displayed  
**WHEN** user clicks "View My Bookings"  
**THEN** page navigates to `/bookings`  
**AND** bookings lookup page is displayed  

---

### Scenario 9: Display international reservation details

**GIVEN** user completed booking for London, UK room  
**AND** used passport for international destination  
**WHEN** confirmation page is displayed  
**THEN** all details are shown including:
  - Document: "Passport: GB987654321"
  - Location: "London, United Kingdom"
  - Currency: "GBP" or "£"
  - Policy: International cancellation policy  

---

### Scenario 10: Display room amenities

**GIVEN** user is viewing confirmation for PremierStays Deluxe room  
**WHEN** confirmation page displays  
**THEN** amenities are shown as chips/badges:
  - [WiFi] [AC] [TV] [Pool] [Gym] [Spa]  
**AND** amenities have appropriate styling (colored badges)  

---

### Scenario 11: Display cancellation policy prominently

**GIVEN** user is viewing confirmation  
**WHEN** room has "FreeCancellation48H" policy  
**THEN** policy is displayed: "Free cancellation up to 48 hours before check-in"  
**AND** policy text is highlighted or in prominent box  
**WHEN** room has "NonRefundable" policy  
**THEN** policy is displayed: "Non-refundable"  
**AND** policy has warning styling (orange/red)  

---

### Scenario 12: Reservation timestamp display

**GIVEN** reservation was created at "2026-07-08T14:30:00"  
**WHEN** confirmation page displays  
**THEN** timestamp is shown as: "Booked on Jul 8, 2026 at 2:30 PM"  
**OR** relative time: "Booked just now" / "Booked 5 minutes ago"  

---

### Scenario 13: Responsive layout for mobile

**GIVEN** user views confirmation page on mobile device  
**WHEN** page renders  
**THEN** layout is single column  
**AND** reference number is full width and prominent  
**AND** all details are vertically stacked  
**AND** buttons are full width  
**AND** text is readable without zooming  

---

### Scenario 14: Print-friendly view (optional)

**GIVEN** user wants to print confirmation  
**WHEN** user triggers browser print (Ctrl+P)  
**THEN** page applies print-friendly styles  
**AND** unnecessary elements are hidden (navigation, buttons)  
**AND** reservation details are clearly formatted  
**AND** reference number is prominent  

---

## Data Models

### ReservationResponse (from state or API)

```typescript
{
  referenceNumber: string;
  room: {
    roomId: string;
    provider: string;
    destination: string;
    location: string;
    roomType: number;
    checkIn: string;
    checkOut: string;
    perNightRate: number;
    currency: string;
    cancellationPolicy: string;
    amenities: string[];
    starRating?: number;
  };
  document: {
    holderName: string;
    type: "Passport" | "NationalId";
    number: string;
  };
  checkIn: string;
  checkOut: string;
  numberOfNights: number;
  totalPrice: number;
  reservationTimestamp: string;
}
```

---

## Navigation

- **From Reservation Flow:** Reservation Page → POST success → Confirmation Page (with state)
- **From Direct URL:** `/confirmation/:reference` → Fetch from API → Display
- **From Bookings:** Optional link after lookup
- **Exit:** Back to Home (`/`) or Bookings (`/bookings`)

---

## API Endpoints Used

- `GET /hotels/reservation/:reference` — Retrieve reservation by reference
  - Used only when accessed via direct URL without state
  - Success: 200 OK
  - Not Found: 404 Not Found
  - Server Error: 500 Internal Server Error

---

## UI Components

### Success State Layout

```
┌─────────────────────────────────────────┐
│              ✓                          │
│       Booking Confirmed!                │
│                                         │
│   REF-a1b2c3d4  [Copy]                 │
│                                         │
│   ┌─────────────────────────────────┐  │
│   │ Guest Details                   │  │
│   │ Name: Rajesh Kumar              │  │
│   │ Document: National ID ABCD12*** │  │
│   └─────────────────────────────────┘  │
│                                         │
│   ┌─────────────────────────────────┐  │
│   │ Booking Details                 │  │
│   │ Provider: [PremierStays]        │  │
│   │ Room: Deluxe Room               │  │
│   │ Location: Mumbai, India ★★★★★   │  │
│   │ Check-in: Aug 1, 2026           │  │
│   │ Check-out: Aug 5, 2026          │  │
│   │ Nights: 4 nights                │  │
│   │                                 │  │
│   │ Rate: ₹8,000 / night            │  │
│   │ Total: ₹32,000                  │  │
│   │                                 │  │
│   │ Policy: Free cancellation       │  │
│   │         up to 48h before        │  │
│   │                                 │  │
│   │ Amenities: [WiFi] [AC] [TV]     │  │
│   └─────────────────────────────────┘  │
│                                         │
│   Booked on Jul 8, 2026 at 2:30 PM     │
│                                         │
│   [Back to Home]  [View My Bookings]   │
└─────────────────────────────────────────┘
```
