# AI Tooling & Prompts

## AI Tools Used

| Tool | Purpose | Integration Type |
|------|---------|------------------|
| **GitHub Copilot** | Code generation, autocomplete, test scaffolding | IDE-integrated (VS Code) |
| **OpenSpec CLI** | Spec-driven workflow, proposal generation, design documentation | CLI + GitHub Copilot Chat |

---

## Documentation Structure

This project follows a **spec-driven approach** where all design rationale, architecture decisions, and key judgement calls are documented in dedicated OpenSpec files. This document serves as an index to those files.

## Significant Prompts Issued

### Spec-driven prompts (OpenSpec CLI + Copilot Chat)
- `/opsx:propose add-hotel-availability-api`  → generated proposal, design, specs, tasks
- `/opsx:propose add-angular-ui`               → generated UI proposal + 4 page specs
- `/opsx:apply add-hotel-availability-api`     → implementation from tasks.md
- `/opsx:apply add-angular-ui`                 → implementation from tasks.md
- `/opsx:archive add-hotel-availability-api`
- `/opsx:archive add-angular-ui`

### Ad-hoc prompts (post-implementation, Copilot Chat)

#### Integration tests
- *"Add end-to-end integration tests using `WebApplicationFactory<Program>` covering the full Search → Reserve → Lookup workflow for both domestic and international scenarios"*  → produced `HotelWorkflowIntegrationTests.cs` with 10 test cases
- *"Add a test case that verifies a domestic reservation with a National ID succeeds, and an international reservation with a National ID returns 422"*  → added `CompleteWorkflow_InternationalWithNationalId_ShouldReturn422` and `CompleteWorkflow_DomesticWithPassport_ShouldSucceed`
- *"Add an integration test that verifies price calculation in the reservation response matches the search result for the same room"*  → added `CompleteWorkflow_ReservationPriceCalculation_ShouldMatchSearchResults`

#### Backend unit test coverage
- *"Add unit tests for `HotelSearchService` — verify it queries all providers in parallel and aggregates results, and returns an empty list when no providers match"*  → produced `HotelSearchServiceTests.cs`
- *"Add unit tests for `BudgetNestsProvider` and `PremierStaysProvider` — test filtering by destination, room type, and date range using Theory data covering every supported city"*  → produced `BudgetNestsProviderTests.cs` and `PremierStaysProviderTests.cs`

#### Angular UI component test coverage
- *"Add unit tests for `ReservationFormComponent` — cover document type restrictions per destination type, form validation rules, emit behaviour on valid/invalid submit, and `ngOnChanges` forcing Passport when switching to international"*  → produced `reservation-form.component.spec.ts`
- *"Add unit tests for `ConfirmationPageComponent` — cover the state-based fast path (navigation state), the API fallback path, 404 vs generic error distinction, and redirect when reference param is missing"*  → produced `confirmation-page.component.spec.ts`
- *"Add unit tests for `BookingsPageComponent` — cover form validation (required, minlength), `onSearch` happy path and all error branches (404, API detail, generic), and loading/error state transitions"*  → produced `bookings-page.component.spec.ts`

#### Review comment fixes
- *"Review flagged a gap in stub/test data coverage — a specific scenario was not represented. Add the missing data entry so that edge case is exercised"*  → extended stub data to cover the missing scenario; existing tests confirmed unaffected
- *"Review identified missing unit test coverage for a component/service. Scaffold tests covering the key behaviours and edge cases"*  → added unit tests covering happy path, error branches, and boundary conditions
- *"Review pointed out an untested code path based on a specific condition or input combination. Add a targeted test case for that condition"*  → added test case reproducing the exact condition; verified it passes against the existing implementation

## Key Judgement Calls

One line per material decision — rationale intentionally lives in the linked design document (single source of truth).

| Judgement Call | Rationale lives in |
|----------------|--------------------|
| Full Onion Architecture with strict dependency direction | [API design.md](openspec/changes/add-hotel-availability-api/design.md) |
| `Reservation.Reserve(...)` static factory encapsulating all reservation invariants (document ↔ location, date range, availability window) | [API design.md](openspec/changes/add-hotel-availability-api/design.md) |
| `IHotelProvider` abstraction + DI of `IEnumerable<IHotelProvider>` so a third provider plugs in without touching core flow | [API design.md](openspec/changes/add-hotel-availability-api/design.md) |
| Anti-corruption layer per provider — provider-shaped entry types (PascalCase / snake_case) mapped to a unified `Room` at the Infrastructure boundary | [API design.md](openspec/changes/add-hotel-availability-api/design.md) |
| Domain exceptions mapped to RFC 7807 ProblemDetails via `ExceptionHandlingMiddleware` | [API design.md](openspec/changes/add-hotel-availability-api/design.md) |
| In-memory `ConcurrentDictionary` repository as a deliberate deferral of persistence (scope: offline demo) | [API design.md](openspec/changes/add-hotel-availability-api/design.md) |
| Reference-number generation strategy (`REF-<guid8>`) | [API design.md](openspec/changes/add-hotel-availability-api/design.md) |
| Angular standalone components + lazy-loaded routes (no NgModules) | [UI design.md](openspec/changes/add-angular-ui/design.md) |
| Reactive forms with custom validators (`date-range`, `document-type`, `past-date`) | [UI design.md](openspec/changes/add-angular-ui/design.md) |
| HTTP interceptor for base URL + centralised error surfacing | [UI design.md](openspec/changes/add-angular-ui/design.md) |

## Linked Design Artifacts

| Change | Proposal | Design | Specs |
|--------|----------|--------|-------|
| Hotel Availability API | [proposal.md](openspec/changes/add-hotel-availability-api/proposal.md) | [design.md](openspec/changes/add-hotel-availability-api/design.md) | [specs/](openspec/changes/add-hotel-availability-api/specs/) |
| Angular Hotel Booking UI | [proposal.md](openspec/changes/add-angular-ui/proposal.md) | [design.md](openspec/changes/add-angular-ui/design.md) | [specs/](openspec/changes/add-angular-ui/specs/) |

Future enhancements and honest reflection: [reflection.md](reflection.md).

---

## AI Workflow

### Spec-First Design with OpenSpec

OpenSpec generated proposals, design documents, specifications, and task breakdowns. All artifacts were reviewed and validated before implementation.

### GitHub Copilot for Implementation

Copilot was used in two complementary modes:

- **Conversational (Copilot Chat)** — Driving the OpenSpec workflow via the `/opsx:*` slash-commands listed in the *Significant Prompts Issued* section above, plus ad-hoc follow-ups for refactors, test scaffolding, and targeted fixes.
- **Inline autocomplete** — File-context-aware suggestions during implementation, grounded in the committed OpenSpec specs and design documents.

---

## Summary

This case study uses a **spec-driven AI workflow**: [`prompts.md`](prompts.md) captures the *instructions issued to the AI and the key judgement calls made*; the linked OpenSpec `design.md` files capture the *rationale* behind each decision. The two are complementary — no rationale is duplicated here.
