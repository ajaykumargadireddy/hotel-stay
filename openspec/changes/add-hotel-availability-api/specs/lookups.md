# Spec: Lookup Endpoints

## Observable Behavior

The lookup endpoints provide reference data for countries and cities, supporting frontend selection UI (dropdowns, autocomplete) and destination filtering.

### Requirements

#### Countries Endpoint

- **SHALL** return list of all countries with `code` and `name` properties
- **SHALL** return 200 OK with camelCase JSON array (default .NET minimal API format)
- **SHALL** include 5 countries: India (IND), United Kingdom (GBR), United States (USA), France (FRA), Japan (JPN)

#### Cities Endpoint

- **SHALL** return list of all cities with `code`, `name`, and `countryCode` properties
- **SHALL** return 200 OK with camelCase JSON array (default .NET minimal API format)
- **SHALL** include cities from multiple countries: Mumbai (BOM), London (LON), New York (NYC), etc.
- **SHALL** link cities to countries via `countryCode` field

### Response Schemas (camelCase JSON — default .NET minimal API format)

**Countries Response:**
```json
[
  {
    "code": "string (ISO 3166-1 alpha-3, e.g., IND, GBR, USA)",
    "name": "string (country name)"
  }
]
```

**Cities Response:**
```json
[
  {
    "code": "string (3-letter city code, e.g., BOM, LON, NYC)",
    "name": "string (city name)",
    "countryCode": "string (ISO 3166-1 alpha-3)"
  }
]
```

---

## Scenarios

### Scenario 1: Get all countries

**GIVEN** system has 5 countries configured  
**WHEN** client sends GET `/lookups/countries`  
**THEN** response status is 200 OK  
**AND** response body contains array of 5 countries:

**Example Response:**
```json
[
  {
    "code": "IND",
    "name": "India"
  },
  {
    "code": "GBR",
    "name": "United Kingdom"
  },
  {
    "code": "USA",
    "name": "United States"
  },
  {
    "code": "FRA",
    "name": "France"
  },
  {
    "code": "JPN",
    "name": "Japan"
  }
]
```

---

**GIVEN** system has cities from multiple countries configured  
**WHEN** client sends GET `/lookups/cities`  
**THEN** response status is 200 OK  
**AND** response body contains array of cities with country codes:

**Example Response:**
```json
[
  {
    "code": "BOM",
    "name": "Mumbai",
    "countryCode": "IND"
  },
  {
    "code": "DEL",
    "name": "Delhi",
    "countryCode": "IND"
  },
  {
    "code": "LON",
    "name": "London",
    "countryCode": "GBR"
  },
  {
    "code": "NYC",
    "name": "New York",
    "countryCode": "USA"
  },
  {
    "code": "PAR",
    "name": "Paris",
    "countryCode": "FRA"
  },
  {
    "code": "TYO",
    "name": "Tokyo",
    "countryCode": "JPN"
  }
]
```

---

### Scenario 2: Frontend integration - City dropdown with country grouping

**GIVEN** frontend fetches countries and cities on page load  
**WHEN** client calls GET `/lookups/countries` and GET `/lookups/cities`  
**THEN** frontend can group cities by `countryCode`  
**AND** frontend can display city names with country context (e.g., "Mumbai, India")  
**AND** frontend can filter hotel search by `cityCode` (e.g., `?destination=BOM`)

---

## Integration with Hotel Search

**Lookup endpoints enable:**
- **City selection** — User selects city from dropdown populated by `/lookups/cities`
- **Destination filtering** — Selected city's `code` (e.g., "BOM") is passed as `?destination=BOM` query parameter to `/hotels/search`
- **Country context** — UI displays "Mumbai, India" or "London, United Kingdom" by joining city and country data
- **Validation** — Frontend can validate city codes exist before allowing search

**Example Frontend Flow:**
1. On page load: fetch `/lookups/countries` and `/lookups/cities`
2. Build dropdown: group cities by country, display as "Mumbai, India"
3. User selects: "Mumbai, India" → extract `cityCode = "BOM"`
4. Search hotels: GET `/hotels/search?destination=BOM&checkIn=...&checkOut=...&roomType=...`
5. Display results: Show "Mumbai" and "India" from lookup data in result cards
