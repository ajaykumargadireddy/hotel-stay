# Spec: Reservation Lookup

## Observable Behavior

The reservation lookup endpoint retrieves stored reservation details by reference number from in-memory storage.

### Requirements

- **SHALL** accept reference number as URL path parameter (any string)
- **SHALL** retrieve reservation from in-memory repository (ConcurrentDictionary)
- **SHALL** return 200 OK with full reservation details when found
- **SHALL** return 404 Not Found when reference number does not exist

### Response Schema (camelCase JSON — default .NET minimal API format)

```json
{
  "referenceNumber": "string (REF-{8-char-GUID})",
  "roomId": "string (GUID)",
  "destination": "string (cityCode, e.g., BOM, LON, NYC)",
  "location": "string (country name)",
  "roomType": "Standard|Deluxe|Suite",
  "checkIn": "DateTime",
  "checkOut": "DateTime",
  "numberOfNights": "integer",
  "totalPrice": "decimal",
  "currency": "string (ISO 4217)",
  "provider": "string",
  "document": {
    "holderName": "string",
    "type": "Passport|NationalId",
    "number": "string"
  },
  "reservationTimestamp": "DateTime"
}
```

---

## Scenarios

### Scenario 1: Successful lookup of existing reservation

**GIVEN** reservation with reference number `REF-3fa85f64` exists in storage  
**AND** reservation was created for London Deluxe room  
**WHEN** client sends GET `/hotels/reservation/REF-3fa85f64`  
**THEN** response status is 200 OK  
**AND** response body contains:
```json
{
  "referenceNumber": "REF-3fa85f64",
  "roomId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "destination": "LON",
  "location": "United Kingdom",
  "roomType": "Deluxe",
  "checkIn": "2024-03-15T15:00:00",
  "checkOut": "2024-03-20T11:00:00",
  "numberOfNights": 5,
  "totalPrice": 750.00,
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

---

### Scenario 2: Lookup of domestic (India) reservation

**GIVEN** reservation with reference number `REF-9a1b2c3d` exists  
**AND** reservation was created for Mumbai Standard room (cityCode: BOM) with National ID  
**WHEN** client sends GET `/hotels/reservation/REF-9a1b2c3d`  
**THEN** response status is 200 OK  
**AND** response includes:
- `destination: "BOM"`, `location: "India"`
- `currency: "INR"`
- `document.type: "NationalId"`

---

### Scenario 3: Reference number not found (404 Not Found)

**GIVEN** reference number `REF-99999999` does not exist in storage  
**WHEN** client sends GET `/hotels/reservation/REF-99999999`  
**THEN** response status is 404 Not Found  
**AND** response body is ProblemDetails:
```json
{
  "type": "https://tools.ietf.org/html/rfc7231#section-6.5.4",
  "title": "Not Found",
  "status": 404,
  "detail": "Reservation not found"
}
```

---

### Scenario 4: Case-insensitive reference number lookup

**GIVEN** reservation exists with reference number `REF-3fa85f64`  
**WHEN** client sends GET `/hotels/reservation/REF-3FA85F64` (uppercase hex)  
**THEN** response status is 200 OK  
**AND** lookup succeeds (case-insensitive matching)  
**AND** response `referenceNumber` is normalized to lowercase: `"REF-3fa85f64"`  

---

### Scenario 5: Lookup immediately after reservation creation

**GIVEN** client just created reservation via POST `/hotels/reserve`  
**AND** received `referenceNumber: "REF-3fa85f64"` in response  
**WHEN** client immediately sends GET `/hotels/reservation/REF-3fa85f64`  
**THEN** response status is 200 OK  
**AND** all reservation details match the original reserve response  
**AND** `reservationTimestamp` matches the creation time  

---

## Notes

- **In-memory storage**: Reservations stored in `ConcurrentDictionary<string, Reservation>` for challenge scope
- **Data persistence**: Reservations lost on server restart (acceptable for challenge; no database required)
- **Reference number format**: `REF-{first 8 chars of GUID}` where GUID chars are lowercase hex (a-f, 0-9)
- **Thread-safety**: ConcurrentDictionary ensures safe concurrent reads/writes
- **Correlation ID**: Automatically propagated via `X-Correlation-Id` header for request tracing
