# Hotel Stay UI

Angular 20 front-end for the Hotel Stay booking platform. Provides search, reservation, and confirmation screens backed by the `HotelStay.Api` .NET service.

## Prerequisites

- **Node.js** 20.19+ (Angular 20 supports Node 20 and 22)
- **npm** 10+
- **HotelStay.Api** running locally on `http://localhost:5263`

Verify:

```powershell
node --version
npm --version
```

## Install

```powershell
cd hotel-stay-ui
npm install
```

## Run (Development)

Start the .NET API first (from the repo root):

```powershell
dotnet run --project HotelStay.Api
```

Then start the Angular dev server:

```powershell
cd hotel-stay-ui
npm start
```

Open [http://localhost:4200](http://localhost:4200). The API must be running or requests will fail with a network error.

> CORS is enabled on the API for `http://localhost:4200` in `HotelStay.Api/Program.cs`.

## Build (Production)

```powershell
npm run build
```

Output is emitted to `dist/hotel-stay-ui/`. The production build uses `environments/environment.prod.ts` (base URL `/api`), assuming the app is served behind a reverse proxy that forwards `/api` to the .NET service.

## Test

Run unit tests once in headless Chrome:

```powershell
npx ng test --watch=false --browsers=ChromeHeadless
```

Or interactively with the default watcher:

```powershell
npm test
```

## Project Structure

```
src/app/
├── components/           # Reusable UI components
│   ├── room-card/        # Single room display with book button
│   ├── room-list/        # Sortable list with empty/loading states
│   ├── search-form/      # Country/city cascade + date/room-type inputs
│   └── reservation-form/ # Guest details + document validation
├── pages/                # Route-level components
│   ├── search-page/      # Landing page (search + results)
│   ├── reservation-page/ # Complete booking
│   └── confirmation-page/# Success + reference number
├── services/             # HTTP-backed API clients
│   ├── hotel.service.ts  # Search, reserve, get reservation
│   └── lookup.service.ts # Countries & cities (cached)
├── models/               # TypeScript interfaces & enums
├── validators/           # Reusable reactive-form validators
│   ├── date-range.validator.ts
│   ├── past-date.validator.ts
│   └── document-type.validator.ts
├── interceptors/         # HTTP interceptors
│   └── api.interceptor.ts# Prepends base URL + correlation ID
├── app.config.ts         # Root providers (router, HTTP client)
├── app.routes.ts         # Route definitions (lazy-loaded)
└── app.ts                # Root shell (header + <router-outlet>)
```

## Configuration

| Environment       | File                                | API base URL          |
|-------------------|-------------------------------------|-----------------------|
| Development       | `src/environments/environment.ts`   | `http://localhost:5263` |
| Production        | `src/environments/environment.prod.ts` | `/api`             |

Change these values if the .NET API runs on a different host or port.

## Domestic vs International Destinations

Client-side document validation mirrors the API rules:

- **Domestic** (country code `IN`): Passport **or** National ID accepted
- **International** (all other country codes): Passport only

See `src/app/validators/document-type.validator.ts` — `DOMESTIC_COUNTRY_CODES` controls this list.

## Troubleshooting

- **"Cannot reach the server" error** — Ensure `dotnet run --project HotelStay.Api` is running and reachable at `http://localhost:5263`.
- **CORS errors in browser console** — Verify the CORS policy `AngularUi` is registered in `HotelStay.Api/Program.cs` and the Angular dev server is running on port 4200.
- **Node version warnings** — Angular CLI 20 requires Node 20.19+ or 22.12+. Use `nvm` (or reinstall Node) if you see engine warnings.
