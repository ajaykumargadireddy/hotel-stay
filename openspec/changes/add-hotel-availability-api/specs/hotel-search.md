# Spec: Hotel Search

## Observable Behavior

The hotel search endpoint aggregates availability across multiple providers (PremierStays, BudgetNests), filters results, normalizes formats, and calculates pricing.

### Requirements

- **SHALL** accept query parameters: `destination` (cityCode), `checkIn` (DateTime), `checkOut` (DateTime), `roomType`
- **SHALL** query all registered providers (PremierStays, BudgetNests) in parallel
- **SHALL** filter rooms where request check-in falls within stub data check-in range AND request check-out falls within stub data check-out range
- **SHALL** normalize provider-specific formats (PremierStays returns PascalCase JSON, BudgetNests returns snake_case JSON) into a unified domain `Room` model
- **SHALL** return response using default .NET minimal API JSON format (camelCase)
- **SHALL** calculate `numberOfNights` from check-in/check-out dates
- **SHALL** calculate `totalPrice` as `perNightRate * numberOfNights`
- **SHALL** include provider name in each result for frontend display
- **SHALL** return 200 OK with empty results array when no rooms match criteria
- **SHALL** return 400 Bad Request when required query parameters are missing
- **SHALL** return 400 Bad Request when date format is invalid or dates are out of range

### Response Schema (camelCase JSON — default .NET minimal API format)

> **Note:** Provider stub data uses different casing conventions:
> - PremierStays: PascalCase (`RoomId`, `CheckIn`, `PerNightRate`)
> - BudgetNests: snake_case (`room_id`, `check_in`, `per_night_rate`)
>
> Each provider has its own model and mapper that normalizes to a unified `Room` domain type. The API response uses camelCase (the default .NET minimal API JSON format via `JsonSerializerDefaults.Web`).

```json
{
  "results": [
    {
      "roomId": "string (GUID)",
      "destination": "string (cityCode, e.g., BOM, LON, NYC)",
      "location": "string (country name)",
      "roomType": "Standard|Deluxe|Suite",
      "checkIn": "DateTime",
      "checkOut": "DateTime",
      "perNightRate": "decimal",
      "numberOfNights": "integer",
      "totalPrice": "decimal",
      "currency": "string (ISO 4217, e.g., INR, GBP, USD, EUR, JPY)",
      "provider": "string (PremierStays|BudgetNests)",
      "cancellationPolicy": "string",
      "amenities": ["string array"],
      "starRating": "integer|null"
    }
  ]
}
```

---

## Scenarios

### Scenario 1: Successful search with multiple provider results

**GIVEN** PremierStays has 2 Deluxe rooms in London (cityCode: LON)  
**AND** BudgetNests has 1 Deluxe room in London  
**AND** stub data has date ranges that cover the requested dates  
**WHEN** client sends GET `/hotels/search?destination=LON&checkIn=2024-03-16T00:00:00&checkOut=2024-03-19T00:00:00&roomType=Deluxe`  
**THEN** response status is 200 OK  
**AND** response contains results from both providers (filtered by date range)  
**AND** each result includes `roomId`, `destination`, `location`, `roomType`, `checkIn`, `checkOut`, `perNightRate`, `numberOfNights`, `totalPrice`, `currency`, `provider`, `cancellationPolicy`, `amenities`, `starRating`  
**AND** `totalPrice` equals `perNightRate * numberOfNights` for each result  
**AND** all results have `currency: "GBP"`, `location: "United Kingdom"`, `destination: "LON"`  

**Example Response (camelCase — default .NET minimal API JSON):**
```json
{
  "results": [
    {
      "roomId": "22222222-2222-2222-2222-222222222222",
      "destination": "LON",
      "location": "United Kingdom",
      "roomType": "Deluxe",
      "checkIn": "2024-03-16T00:00:00",
      "checkOut": "2024-03-19T00:00:00",
      "perNightRate": 150.00,
      "numberOfNights": 3,
      "totalPrice": 450.00,
      "currency": "GBP",
      "provider": "PremierStays",
      "cancellationPolicy": "FreeCancellation",
      "amenities": ["WiFi", "Heating", "Pool", "Gym"],
      "starRating": 4
    },
    {
      "roomId": "55555555-2222-2222-2222-222222222222",
      "destination": "LON",
      "location": "United Kingdom",
      "roomType": "Deluxe",
      "checkIn": "2024-03-16T00:00:00",
      "checkOut": "2024-03-19T00:00:00",
      "perNightRate": 120.00,
      "numberOfNights": 3,
      "totalPrice": 360.00,
      "currency": "GBP",
      "provider": "BudgetNests",
      "cancellationPolicy": "Flexible",
      "amenities": ["WiFi"],
      "starRating": null
    }
  ]
}
```

---

### Scenario 2: Search returns empty results when no rooms match

**GIVEN** no providers have rooms in cityCode "ANT"  
**WHEN** client sends GET `/hotels/search?destination=ANT&checkIn=2024-03-15T00:00:00&checkOut=2024-03-20T00:00:00&roomType=Suite`  
**THEN** response status is 200 OK  
**AND** response body is `{ "results": [] }`  

---

### Scenario 3: Search across India (domestic) locations

**GIVEN** PremierStays has Standard rooms in Mumbai (cityCode: BOM)  
**AND** BudgetNests has Standard rooms in Mumbai  
**WHEN** client sends GET `/hotels/search?destination=BOM&checkIn=2024-04-02T00:00:00&checkOut=2024-04-04T00:00:00&roomType=Standard`  
**THEN** response status is 200 OK  
**AND** all results have `destination: "BOM"`, `location: "India"`, `currency: "INR"`  
**AND** `totalPrice` equals `perNightRate * numberOfNights`  

---

### Scenario 4: Missing required query parameter (destination)

**GIVEN** no destination parameter provided  
**WHEN** client sends GET `/hotels/search?checkIn=2024-03-15T00:00:00&checkOut=2024-03-20T00:00:00&roomType=Deluxe`  
**THEN** response status is 400 Bad Request  
**AND** response body is ProblemDetails with `title: "Validation Error"`, `detail: "Destination is required"`  

---

### Scenario 5: Invalid date format

**GIVEN** invalid checkIn date format  
**WHEN** client sends GET `/hotels/search?destination=LON&checkIn=invalid-date&checkOut=2024-03-20T00:00:00&roomType=Deluxe`  
**THEN** response status is 400 Bad Request  
**AND** response body is ProblemDetails with `title: "Validation Error"`, `detail: "Invalid date format. Expected ISO 8601 DateTime"`  

---

### Scenario 6: Check-out date before check-in date

**GIVEN** checkOut date is before checkIn date  
**WHEN** client sends GET `/hotels/search?destination=LON&checkIn=2024-03-20T00:00:00&checkOut=2024-03-15T00:00:00&roomType=Deluxe`  
**THEN** response status is 400 Bad Request  
**AND** response body is ProblemDetails with `title: "Validation Error"`, `detail: "Check-out must be after check-in"`  

---

### Scenario 7: Provider returns mixed currencies for different locations

**GIVEN** PremierStays has rooms in London (GBP), New York (USD), and Tokyo (JPY)  
**WHEN** client searches multiple destinations (assume separate requests or provider returns multiple)  
**THEN** each result reflects location-specific currency  
**AND** London rooms have `currency: "GBP"`  
**AND** New York rooms have `currency: "USD"`  
**AND** Tokyo rooms have `currency: "JPY"`  

---

## Notes

- **Case-insensitive destination matching**: "london", "London", "LONDON" should all work; normalize to title case in responses
- **Provider filtering**: Providers are responsible for filtering out unavailable rooms before returning results
- **Deterministic stub data**: Same search criteria always return same results (testability requirement)
- **BudgetNests missing fields**: Return `amenities: []`, `starRating: null` for explicit empty values
