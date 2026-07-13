# Spec: Hotel Reservation

## Observable Behavior

The reservation endpoint validates room availability, enforces document validation rules based on location (India = domestic, non-India = international), creates reservations with unique reference numbers, and stores them in-memory.

### Requirements

- **SHALL** accept JSON body with `roomId` (GUID) and `document` object
- **SHALL** validate `roomId` exists in provider catalogs (query all providers via `GetRoomById`)
- **SHALL** validate document type matches location rules:
  - **India locations (domestic)**: Accept `NationalId` OR `Passport`
  - **Non-India locations (international)**: Require `Passport` only
- **SHALL** generate unique reference number in format `REF-{first 8 chars of GUID}`
- **SHALL** calculate `numberOfNights` from room's check-in/check-out dates
- **SHALL** calculate `totalPrice` as `perNightRate * numberOfNights`
- **SHALL** set `reservationTimestamp` to current server time in `yyyy-MM-dd HH:mm:ss` format
- **SHALL** store reservation in-memory (ConcurrentDictionary)
- **SHALL** return 201 Created with JSON body containing `referenceNumber`
- **SHALL** return 400 Bad Request when `roomId` or `document` fields are missing/invalid
- **SHALL** return 404 Not Found when `roomId` does not exist in any provider catalog
- **SHALL** return 422 Unprocessable Entity when document type violates location rules

### Request Schema

```json
{
  "roomId": "string (GUID)",
  "document": {
    "holderName": "string",
    "type": "Passport|NationalId",
    "number": "string (5-20 chars, alphanumeric)"
  }
}
```

### Response Schema (camelCase JSON — default .NET minimal API format)

```json
{
  "referenceNumber": "string (REF-{8-char-GUID})"
}
```

**Note:** To retrieve full reservation details, use `GET /hotels/reservation/{referenceNumber}`

---

## Scenarios

### Scenario 1: Successful reservation for international destination with Passport

**GIVEN** room `3fa85f64-5717-4562-b3fc-2c963f66afa6` exists in PremierStays catalog  
**AND** room destination is London (cityCode: LON, location: United Kingdom, international)  
**AND** room is available for dates 2024-03-15 to 2024-03-20  
**WHEN** client sends POST `/hotels/reserve` with:
```json
{
  "roomId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "document": {
    "holderName": "John Doe",
    "type": "Passport",
    "number": "AB1234567"
  }
}
```
**THEN** response status is 201 Created  
**AND** response body contains:
```json
{
  "referenceNumber": "REF-3fa85f64"
}
```

---

### Scenario 2: Successful reservation for domestic destination (India) with National ID

**GIVEN** room `9a1b2c3d-4e5f-6a7b-8c9d-0e1f2a3b4c5d` exists in BudgetNests catalog  
**AND** room destination is Mumbai (cityCode: BOM, location: India, domestic)  
**AND** room is available for dates 2024-04-01 to 2024-04-05  
**WHEN** client sends POST `/hotels/reserve` with:
```json
{
  "roomId": "9a1b2c3d-4e5f-6a7b-8c9d-0e1f2a3b4c5d",
  "document": {
    "holderName": "Rajesh Kumar",
    "type": "NationalId",
    "number": "AADHAAR123456"
  }
}
```
**THEN** response status is 201 Created  
**AND** response body contains `referenceNumber` matching pattern `REF-[a-f0-9]{8}`
```json
{
  "referenceNumber": "REF-9a1b2c3d"
}
```

---

### Scenario 3: Domestic destination (India) accepts Passport as well

**GIVEN** room destination is Mumbai (cityCode: BOM, location: India, domestic)  
**WHEN** client reserves with `document.type: "Passport"`  
**THEN** response status is 201 Created  
**AND** reservation succeeds (Passport is valid for domestic)  

---

### Scenario 4: International destination rejects National ID (422 Unprocessable Entity)

**GIVEN** room destination is Tokyo (location: Japan, international)  
**WHEN** client sends POST `/hotels/reserve` with:
```json
{
  "roomId": "valid-tokyo-room-guid",
  "document": {
    "holderName": "John Doe",
    "type": "NationalId",
    "number": "DL1234567"
  }
}
```
**THEN** response status is 422 Unprocessable Entity  
**AND** response body is ProblemDetails:
```json
{
  "type": "https://tools.ietf.org/html/rfc7231#section-6.5.3",
  "title": "Document Mismatch",
  "status": 422,
  "detail": "International destinations require Passport"
}
```

---

### Scenario 5: Invalid roomId (404 Not Found)

**GIVEN** roomId `00000000-0000-0000-0000-000000000000` does not exist in any provider catalog  
**WHEN** client sends POST `/hotels/reserve` with invalid roomId  
**THEN** response status is 404 Not Found  
**AND** response body is ProblemDetails with `title: "Not Found"`, `detail: "Room not found"`  

---

### Scenario 6: Missing required field - roomId (400 Bad Request)

**GIVEN** request body missing `roomId` field  
**WHEN** client sends POST `/hotels/reserve` with:
```json
{
  "document": {
    "holderName": "John Doe",
    "type": "Passport",
    "number": "AB1234567"
  }
}
```
**THEN** response status is 400 Bad Request  
**AND** response body is ProblemDetails with `detail: "RoomId is required"`  

---

### Scenario 7: Missing required field - document (400 Bad Request)

**GIVEN** request body missing `document` field  
**WHEN** client sends POST `/hotels/reserve` with only `roomId`  
**THEN** response status is 400 Bad Request  
**AND** response body is ProblemDetails with `detail: "Document is required"`  

---

### Scenario 8: Empty document number (400 Bad Request)

**GIVEN** request body has `document.number` as empty string or whitespace  
**WHEN** client sends POST `/hotels/reserve`  
**THEN** response status is 400 Bad Request  
**AND** response body is ProblemDetails with `detail: "Document number is required"`  

---

### Scenario 9: Document number too short (400 Bad Request)

**GIVEN** request body has `document.number: "AB1"` (less than 5 chars)  
**WHEN** client sends POST `/hotels/reserve`  
**THEN** response status is 400 Bad Request  
**AND** response body is ProblemDetails with `detail: "Document number must be between 5 and 20 characters"`  

---

### Scenario 10: Unique reference number generation

**GIVEN** two clients reserve different rooms simultaneously  
**WHEN** both reservations succeed  
**THEN** each reservation has a unique `referenceNumber`  
**AND** no collision occurs (ConcurrentDictionary ensures thread-safety)  

---

## Document Validation Rules

### India (Domestic) Locations
- **Location:** "India"
- **Accepted Document Types:** `Passport` OR `NationalId`
- **Cities (examples):** Mumbai, Delhi, Bangalore, Chennai, Kolkata

### International (Non-India) Locations
- **Locations:** Any country except India (e.g., "United Kingdom", "United States", "Japan", "France")
- **Required Document Type:** `Passport` ONLY (rejects `NationalId`)
- **Cities (examples):** London, New York, Tokyo, Paris, Los Angeles

### Validation Logic
```
IF room.Location != "India" (case-insensitive) THEN
  international = true
  REQUIRE document.Type == "Passport"
ELSE
  domestic = true
  ACCEPT document.Type IN ["Passport", "NationalId"]
```

---

## Notes

- **Reference number format**: First 8 characters of a newly generated GUID, prefixed with `REF-`
- **In-memory storage**: Uses `ConcurrentDictionary<string, Reservation>` keyed by reference number
- **No double-booking prevention**: Room can be reserved multiple times (acceptable for challenge scope)
- **Date validation**: Check-out > check-in validated by domain factory method `Reservation.Reserve(...)`
- **Correlation ID**: Automatically added to logs via `X-Correlation-Id` middleware
