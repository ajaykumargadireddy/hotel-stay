# Hotel Stay — SkyRoute Challenge

Spec-driven Hotel Availability API implementation using .NET 8 Minimal API, Angular, OpenSpec, and full Onion Architecture.

---

## What to Do Just after Clone

### 1. Prerequisites

Ensure you have the following installed:

| Tool | Version | Verify Command |
|------|---------|----------------|
| **Node.js** | 20.19.0 or higher | `node --version` |
| **.NET SDK** | 8.0 or higher | `dotnet --version` |
| **OpenSpec CLI** | Latest | `openspec --version` |

### 2. Install OpenSpec

OpenSpec is the spec-driven workflow tool for this project. Install globally:

```powershell
npm install -g @fission-ai/openspec@latest
```

Verify installation:

```powershell
openspec --version
```

### 3. Restore .NET Dependencies

From the repository root:

```powershell
dotnet restore
```

This restores NuGet packages for all projects in the solution.

### 4. Build the Solution

```powershell
dotnet build
```

### 5. Run Tests

```powershell
dotnet test
```

### 6. Run the API

```powershell
cd HotelStay.Api
dotnet run
```

The API will be available at `https://localhost:5001` (or the port shown in the console).

**Swagger UI:** Navigate to [http://localhost:5263/index.html](http://localhost:5263/index.html) for interactive API testing.

### 7. Run the Angular UI

In a second terminal, from the repository root:

```powershell
cd hotel-stay-ui
npm install
npm start
```

Open [http://localhost:4200](http://localhost:4200) in your browser.

> The .NET API must be running (Step 6) for the UI to function. CORS is pre-configured on the API to allow `http://localhost:4200`.

See [`hotel-stay-ui/README.md`](hotel-stay-ui/README.md) for UI-specific details (build, test, project structure).

---

## API Documentation

Comprehensive API documentation with request/response examples is available in **[API.md](API.md)**.

The documentation includes:
- All endpoint specifications
- Request/response examples
- Data models and schemas
- Error handling patterns
- Common workflow examples

For interactive testing, use the Swagger UI at [http://localhost:5263/index.html](http://localhost:5263/index.html) when the API is running.

---

## Project Architecture

This project uses **full Onion Architecture** with strict dependency direction:

- **HotelStay.Domain** — No dependencies (pure domain)
- **HotelStay.Application** — References Domain
- **HotelStay.Infrastructure** — References Domain + Application
- **HotelStay.Api** — References Application + Infrastructure (composition root)
- **HotelStay.Tests** — References all projects

For detailed architecture rules, domain conventions, interface placement, and testing strategy, see [`openspec/config.yaml`](openspec/config.yaml).

---

## OpenSpec Workflow

This project uses OpenSpec for spec-driven development. The active change lives in `openspec/changes/`, and canonical specs are in `openspec/specs/`.

### Common Commands

```powershell
# View active changes
openspec list

# Check change status
openspec status --change <change-name>

# View interactive dashboard
openspec view
```

### Using OpenSpec with GitHub Copilot

In VS Code Copilot Chat:

```
/opsx:explore              # Explore and plan before proposing
/opsx:propose <feature>    # Create a new change proposal
/opsx:apply                # Implement the change
/opsx:archive              # Archive completed change
```

For more on OpenSpec, see the [official documentation](https://github.com/Fission-AI/OpenSpec/blob/HEAD/docs/README.md).

---

## Tech Stack

### Backend
- **Runtime:** .NET 8 (C#)
- **API Framework:** ASP.NET Core Minimal API
- **Architecture:** Onion Architecture (Domain, Application, Infrastructure, API layers)
- **OpenAPI/Swagger:** Swashbuckle.AspNetCore 6.5.0
- **Logging:** Karambolo.Extensions.Logging.File 3.4.0 with correlation IDs
- **Storage:** In-memory ConcurrentDictionary (demo purposes)
- **Testing:** xUnit 2.5.3, Moq 4.20.70, Coverlet 6.0.0 (code coverage)
- **Error Handling:** RFC 7807 ProblemDetails format

### Frontend
- **Framework:** Angular 20.3.0
- **Language:** TypeScript 5.9.2
- **Components:** Standalone components (no NgModules)
- **Routing:** Lazy-loaded routes with route guards
- **Forms:** Reactive forms with custom validators (date range, document type)
- **HTTP:** HttpClient with error interceptor
- **State Management:** Service-based with RxJS
- **Testing:** Jasmine 5.9.0, Karma 6.4.0
- **Build:** Angular CLI 20.3.31

### Development Tools
- **AI Tooling:** GitHub Copilot (IDE-integrated) + OpenSpec CLI (spec-driven workflow)
- **Version Control:** Git with conventional commits
- **Package Managers:** NuGet (.NET), npm (Angular)
- **Code Quality:** Prettier (Angular), EditorConfig

### Standards & Conventions
- **API Format:** JSON with camelCase (default .NET Minimal API)
- **Date Format:** ISO 8601 (yyyy-MM-dd)
- **HTTP Status Codes:** 200 (OK), 400 (Bad Request), 404 (Not Found), 422 (Unprocessable Entity), 500 (Internal Server Error)
- **Architecture:** Strict dependency direction (Domain → Application → Infrastructure → API)

---

## Contributing

All changes follow the OpenSpec workflow:

1. Use `/opsx:propose` to create a change with proposal, specs, design, and tasks.
2. Implement via `/opsx:apply` or manually following the generated tasks.
3. Archive with `/opsx:archive` to merge specs into `openspec/specs/`.

See [`openspec/config.yaml`](openspec/config.yaml) for project-specific rules and conventions.

---

# Cheat-sheet quick date lookup

Run this one-liner in PowerShell to print today's concrete stub dates:

```powershell
$today = (Get-Date).Date
[pscustomobject]@{
  India_CheckIn  = $today.AddDays(30).ToString('yyyy-MM-dd')
  India_CheckOut = $today.AddDays(35).ToString('yyyy-MM-dd')
  UK_CheckIn     = $today.AddDays(45).ToString('yyyy-MM-dd')
  UK_CheckOut    = $today.AddDays(50).ToString('yyyy-MM-dd')
  US_CheckIn     = $today.AddDays(60).ToString('yyyy-MM-dd')
  US_CheckOut    = $today.AddDays(65).ToString('yyyy-MM-dd')
  Japan_CheckIn  = $today.AddDays(75).ToString('yyyy-MM-dd')
  Japan_CheckOut = $today.AddDays(80).ToString('yyyy-MM-dd')
} | Format-List
```

## About spec.md and prompts.md

**Note:** [`spec.md`](spec.md) and [`prompts.md`](prompts.md) serve as **index files** to the OpenSpec structure, as per OpenSpec's design philosophy:

- **[`spec.md`](spec.md)** — Central index to all specifications, linking to individual spec files in `openspec/changes/*/specs/` and design documents in `openspec/changes/*/design.md`
- **[`prompts.md`](prompts.md)** — Index to AI tooling documentation, linking to design rationale, architecture decisions, and AI workflow documentation stored in OpenSpec change folders

This indexing approach keeps the repository root clean while providing easy navigation to comprehensive design documentation and specifications maintained by OpenSpec. All detailed technical decisions, architecture rationale, and implementation specifications live in the respective OpenSpec change directories (`openspec/changes/`), following OpenSpec's principle of **persistent, versioned design documentation**.
