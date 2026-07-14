# Hotel Stay API Documentation

## Overview

The Hotel Stay API provides endpoints for searching hotel rooms, making reservations, and retrieving reservation details. The API is built using .NET 8 Minimal API and follows REST principles.

**Base URL:** `http://localhost:5263`

**Swagger UI:** [http://localhost:5263/index.html](http://localhost:5263/index.html)

**API Version:** v1

---

## Table of Contents

- [Authentication](#authentication)
- [Endpoints](#endpoints)
  - [Hotel Endpoints](#hotel-endpoints)
  - [Lookup Endpoints](#lookup-endpoints)
- [Data Models](#data-models)
- [Error Responses](#error-responses)

---

## Authentication

Currently, the API does not require authentication. All endpoints are publicly accessible.

---

## Endpoints

### Hotel Endpoints

#### 1. Search Hotels

Search for available hotel rooms based on destination, check-in/check-out dates, and optional room type.

**Endpoint:** `GET /hotels/search`

**Query Parameters:**

| Parameter | Type | Required | Description |
|-----------|------|----------|-------------|
| `destination` | string | Yes | Destination code (e.g., "BOM") |
| `checkIn` | date | Yes | Check-in date (ISO 8601 format: `YYYY-MM-DD`) |
| `checkOut` | date | Yes | Check-out date (ISO 8601 format: `YYYY-MM-DD`) |
| `roomType` | string | No | Room type: `Standard`, `Deluxe`, or `Suite` |

**Example Request:**

```bash
GET http://localhost:5263/hotels/search?destination=Mumbai&checkIn=2026-08-01&checkOut=2026-08-05&roomType=Deluxe
```

**Example Response:** `200 OK`

```json
{
  "results": [
    {
      "room": {
        "roomId": "11111111-3333-3333-3333-333333333333",
        "provider": "PremierStays",
        "destination": "BOM",
        "location": "India",
        "roomType": 2,
        "checkIn": "2026-08-07T00:00:00+05:30",
        "checkOut": "2026-08-12T00:00:00+05:30",
        "perNightRate": 8000,
        "currency": "INR",
        "cancellationPolicy": "FreeCancellation48H",
        "amenities": [
          "WiFi",
          "AC",
          "TV",
          "Pool",
          "Gym",
          "Spa",
          "Butler"
        ],
        "starRating": 5
      },
      "totalNights": 2,
      "totalPrice": 16000
    },
    {
      "room": {
        "roomId": "44444444-3333-3333-3333-333333333333",
        "provider": "BudgetNests",
        "destination": "BOM",
        "location": "India",
        "roomType": 2,
        "checkIn": "2026-08-07T00:00:00+05:30",
        "checkOut": "2026-08-12T00:00:00+05:30",
        "perNightRate": 6000,
        "currency": "INR",
        "cancellationPolicy": "Flexible",
        "amenities": [],
        "starRating": null
      },
      "totalNights": 2,
      "totalPrice": 12000
    }
  ]
}
```

**Error Responses:**

- `400 Bad Request` - Missing or invalid parameters
  ```json
  {
    "error": "Destination is required"
  }
  ```

---

#### 2. Reserve Hotel

Create a new hotel reservation for a specific room.

**Endpoint:** `POST /hotels/reserve`

**Request Body:**

```json
{
  "roomId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "checkIn": "2026-08-01T00:00:00",
  "checkOut": "2026-08-05T00:00:00",
  "document": {
    "holderName": "John Doe",
    "type": "Passport",
    "number": "AB1234567"
  }
}
```

**Example Request:**

```bash
POST http://localhost:5263/hotels/reserve
Content-Type: application/json

{
  "roomId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "checkIn": "2026-08-01T00:00:00",
  "checkOut": "2026-08-05T00:00:00",
  "document": {
    "holderName": "John Doe",
    "type": "Passport",
    "number": "AB1234567"
  }
}
```

**Example Response:** `201 Created`

```json
{
  "referenceNumber": "REF-3dde8e7a"
}
```

**Error Responses:**

- `400 Bad Request` - Invalid request body
- `404 Not Found` - Room not found
- `422 Unprocessable Entity` - Business logic validation error (e.g., document mismatch)

---

#### 3. Get Reservation

Retrieve reservation details by reference number.

**Endpoint:** `GET /hotels/reservation/{reference}`

**Path Parameters:**

| Parameter | Type | Required | Description |
|-----------|------|----------|-------------|
| `reference` | string | Yes | Reservation reference number |

**Example Request:**

```bash
GET http://localhost:5263/hotels/reservation/RSV-20260708-ABC123
```

**Example Response:** `200 OK`

```json
{
  "referenceNumber": "RSV-20260708-ABC123",
  "roomId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "destination": "Mumbai",
  "location": "Bandra West",
  "roomType": "Deluxe",
  "roomCheckIn": "2026-08-01T00:00:00",
  "roomCheckOut": "2026-08-05T00:00:00",
  "checkIn": "2026-08-01T00:00:00",
  "checkOut": "2026-08-05T00:00:00",
  "numberOfNights": 4,
  "totalPrice": 600.00,
  "currency": "USD",
  "provider": "PremierStays",
  "document": {
    "holderName": "John Doe",
    "type": "Passport",
    "number": "AB1234567"
  },
  "reservationTimestamp": "2026-07-08 14:30:00"
}
```

**Error Responses:**

- `404 Not Found` - Reservation not found
  ```json
  {
    "type": "https://tools.ietf.org/html/rfc9110#section-15.5.5",
    "title": "Not Found",
    "status": 404,
    "detail": "Reservation with reference 'RSV-INVALID' not found"
  }
  ```

---

### Lookup Endpoints

#### 4. Get Countries

Retrieve a list of all available countries.

**Endpoint:** `GET /lookups/countries`

**Example Request:**

```bash
GET http://localhost:5263/lookups/countries
```

**Example Response:** `200 OK`

```json
[
  {
    "code": "IN",
    "name": "India"
  },
  {
    "code": "GB",
    "name": "United Kingdom"
  },
  {
    "code": "US",
    "name": "United States"
  },
  {
    "code": "JP",
    "name": "Japan"
  },
  {
    "code": "FR",
    "name": "France"
  }
]
```

---

#### 5. Get Cities

Retrieve a list of all cities, optionally filtered by country code.

**Endpoint:** `GET /lookups/cities`

**Query Parameters:**

| Parameter | Type | Required | Description |
|-----------|------|----------|-------------|
| `countryCode` | string | No | Filter cities by country code (e.g., "IN", "GB") |

**Example Request (All Cities):**

```bash
GET http://localhost:5263/lookups/cities
```

**Example Request (Cities by Country):**

```bash
GET http://localhost:5263/lookups/cities?countryCode=IN
```

**Example Response:** `200 OK`

```json
[
  {
    "code": "BOM",
    "name": "Mumbai",
    "countryCode": "IN"
  },
  {
    "code": "DEL",
    "name": "Delhi",
    "countryCode": "IN"
  },
  {
    "code": "BLR",
    "name": "Bangalore",
    "countryCode": "IN"
  }
]
```

---

## Data Models

### Room

Represents a hotel room with pricing and details.

```typescript
{
  "roomId": "guid",              // Unique room identifier
  "provider": "string",           // Hotel provider name
  "destination": "string",        // City name
  "location": "string",           // Specific location within city
  "roomType": "RoomType",         // Standard | Deluxe | Suite
  "checkIn": "datetime",          // Check-in date
  "checkOut": "datetime",         // Check-out date
  "perNightRate": "decimal",      // Rate per night
  "currency": "string",           // Currency code (e.g., "USD")
  "cancellationPolicy": "string", // Cancellation terms
  "amenities": ["string"],        // List of amenities
  "starRating": "int?",           // Hotel star rating (1-5)
  "numberOfNights": "int",        // Computed: days between check-in and check-out
  "totalPrice": "decimal"         // Computed: perNightRate * numberOfNights
}
```

### ReservationRequest

Request body for creating a reservation.

```typescript
{
  "roomId": "guid",               // Room to reserve
  "checkIn": "datetime",          // Check-in date
  "checkOut": "datetime",         // Check-out date
  "document": {                   // Guest document information
    "holderName": "string",       // Full name as on document
    "type": "DocumentType",       // Passport | NationalId
    "number": "string"            // Document number
  }
}
```

### ReservationResponse

Response containing reservation details.

```typescript
{
  "referenceNumber": "string",    // Unique reservation reference
  "roomId": "guid",               // Reserved room ID
  "destination": "string",        // City name
  "location": "string",           // Hotel location
  "roomType": "RoomType",         // Standard | Deluxe | Suite
  "roomCheckIn": "datetime",      // Original room check-in
  "roomCheckOut": "datetime",     // Original room check-out
  "checkIn": "datetime",          // Reservation check-in
  "checkOut": "datetime",         // Reservation check-out
  "numberOfNights": "int",        // Number of nights
  "totalPrice": "decimal",        // Total price
  "currency": "string",           // Currency code
  "provider": "string",           // Hotel provider
  "document": {                   // Guest document
    "holderName": "string",
    "type": "DocumentType",
    "number": "string"
  },
  "reservationTimestamp": "string" // Format: yyyy-MM-dd HH:mm:ss
}
```

### Country

```typescript
{
  "code": "string",   // ISO country code (e.g., "IN")
  "name": "string"    // Country name
}
```

### City

```typescript
{
  "code": "string",        // City code (e.g., "BOM")
  "name": "string",        // City name
  "countryCode": "string"  // Parent country code
}
```

---

## Enums

### RoomType

- `Standard`
- `Deluxe`
- `Suite`

### DocumentType

- `Passport`
- `NationalId`

---

## Error Responses

The API uses standard HTTP status codes and returns problem details for errors:

### 400 Bad Request

Invalid request parameters or body.

```json
{
  "error": "Destination is required"
}
```

### 404 Not Found

Resource not found.

```json
{
  "type": "https://tools.ietf.org/html/rfc9110#section-15.5.5",
  "title": "Not Found",
  "status": 404,
  "detail": "Reservation with reference 'RSV-INVALID' not found"
}
```

### 422 Unprocessable Entity

Business logic validation error.

```json
{
  "type": "https://tools.ietf.org/html/rfc9110#section-15.5.22",
  "title": "Unprocessable Entity",
  "status": 422,
  "detail": "Document mismatch: reservation document does not match guest document"
}
```

### 500 Internal Server Error

Unexpected server error. Check logs for details.

```json
{
  "type": "https://tools.ietf.org/html/rfc9110#section-15.6.1",
  "title": "An error occurred while processing your request.",
  "status": 500
}
```

---

## Testing with Swagger UI

The API includes Swagger UI for interactive testing:

1. Navigate to [http://localhost:5263/index.html](http://localhost:5263/index.html)
2. Expand any endpoint
3. Click "Try it out"
4. Fill in the parameters
5. Click "Execute"
6. View the response

---

## Common Workflow Example

### Step 1: Get Available Countries

```bash
GET http://localhost:5263/countries
```

### Step 2: Get Cities in a Country

```bash
GET http://localhost:5263/cities?countryCode=IN
```

### Step 3: Search for Hotels

```bash
GET http://localhost:5263/hotels/search?destination=Mumbai&checkIn=2026-08-01&checkOut=2026-08-05&roomType=Deluxe
```

### Step 4: Reserve a Room

```bash
POST http://localhost:5263/hotels/reserve
Content-Type: application/json

{
  "roomId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "checkIn": "2026-08-01T00:00:00",
  "checkOut": "2026-08-05T00:00:00",
  "document": {
    "holderName": "John Doe",
    "type": "Passport",
    "number": "AB1234567"
  }
}
```

### Step 5: Retrieve Reservation

```bash
GET http://localhost:5263/hotels/reservation/RSV-20260708-ABC123
```

---

## Additional Notes

- All dates should be in ISO 8601 format
- The API uses UTC for all timestamps
- Room availability is determined by the configured hotel providers
- Reservation reference numbers follow the format: `RSV-YYYYMMDD-XXXXXX`
- All monetary values are decimal with 2 decimal places

---

## Support

For issues or questions, please refer to the [README.md](README.md) or check the project repository.
