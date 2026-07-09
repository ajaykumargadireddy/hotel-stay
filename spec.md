# Hotel Stay Specifications

This document serves as the entry point for all specifications in the Hotel Stay project.

---

## Table of Contents

- [API Specifications](#api-specifications)
  - [Hotel Availability API](#hotel-availability-api) (4 specs)
- [UI Specifications](#ui-specifications)
  - [Angular Hotel Booking UI](#angular-hotel-booking-ui) (4 specs)
- [Specification Workflow](#specification-workflow)
- [Navigation](#navigation)
- [Quick Links](#quick-links)
- [Summary](#summary)

---

## API Specifications

### In Progress

#### Hotel Availability API
**Change:** `add-hotel-availability-api`  
**Location:** `openspec/changes/add-hotel-availability-api/specs/`

The Hotel Availability API enables travellers to search for hotel rooms across multiple providers, make reservations with document validation, and retrieve reservation details.

**Specifications:**
1. **[Hotel Search](openspec/changes/add-hotel-availability-api/specs/hotel-search.md)** — Multi-provider search with filtering and pricing (`GET /hotels/search`)
2. **[Hotel Reservation](openspec/changes/add-hotel-availability-api/specs/hotel-reservation.md)** — Room reservation with document validation (`POST /hotels/reserve`)
3. **[Reservation Lookup](openspec/changes/add-hotel-availability-api/specs/reservation-lookup.md)** — Retrieve reservation by reference (`GET /hotels/reservation/{ref}`)
4. **[Lookups](openspec/changes/add-hotel-availability-api/specs/lookups.md)** — Master data endpoints for countries and cities (`GET /lookups/countries`, `GET /lookups/cities`)

**Additional Resources:**
- **[Proposal](openspec/changes/add-hotel-availability-api/proposal.md)** — Problem statement, business requirements, scope definition
- **[Design](openspec/changes/add-hotel-availability-api/design.md)** — Onion Architecture, interface placement, mapper strategy
- **[Tasks](openspec/changes/add-hotel-availability-api/tasks.md)** — Implementation checklist

### Archived

<!-- After archiving, completed API specs will be listed here with links to openspec/specs/ -->

_No archived API specifications yet._

---

## UI Specifications

### In Progress

#### Angular Hotel Booking UI
**Change:** `add-angular-ui`  
**Location:** `openspec/changes/add-angular-ui/specs/`

A complete Angular single-page application (latest stable version) providing a modern, responsive interface for the Hotel Stay booking platform.

**Specifications:**
1. **[Search Page](openspec/changes/add-angular-ui/specs/search-page-spec.md)** — Hotel search form with country/city dropdowns, date validation, and sortable results display
2. **[Reservation Page](openspec/changes/add-angular-ui/specs/reservation-page-spec.md)** — Room booking form with document validation (Passport/National ID based on destination)
3. **[Bookings Page](openspec/changes/add-angular-ui/specs/bookings-page-spec.md)** — Lookup existing reservations by reference number
4. **[Confirmation Page](openspec/changes/add-angular-ui/specs/confirmation-page-spec.md)** — Display booking confirmation and reservation details

**Additional Resources:**
- **[Proposal](openspec/changes/add-angular-ui/proposal.md)** — Problem statement, solution overview, technology stack
- **[Design](openspec/changes/add-angular-ui/design.md)** — Component architecture, data flow, technical decisions
- **[Tasks](openspec/changes/add-angular-ui/tasks.md)** — Implementation checklist (37 tasks, ~50 hours)

### Archived

<!-- After archiving, completed UI specs will be listed here -->

_No archived UI specifications yet._

---

## Specification Workflow

### During Development (Current State)
- **Location:** `openspec/changes/{change-name}/specs/*.md`
- **Purpose:** Work-in-progress specifications tied to active changes
- **Management:** Managed via OpenSpec CLI (`openspec` commands)

### After Archiving
- **Location:** `openspec/specs/*.md`
- **Purpose:** Completed, production-ready specifications
- **Archive Command:** `openspec archive --change {change-name}`
- **Post-Archive:** This file automatically updates to point to archived locations

---

## Navigation

### API Change: Hotel Availability API

- [Proposal](openspec/changes/add-hotel-availability-api/proposal.md) — Why and what changes
- [Design](openspec/changes/add-hotel-availability-api/design.md) — How to implement (architecture, decisions, trade-offs)
- [Specs](openspec/changes/add-hotel-availability-api/specs/) — Observable behavior (requirements, scenarios, contracts)
  - [Hotel Search](openspec/changes/add-hotel-availability-api/specs/hotel-search.md)
  - [Hotel Reservation](openspec/changes/add-hotel-availability-api/specs/hotel-reservation.md)
  - [Reservation Lookup](openspec/changes/add-hotel-availability-api/specs/reservation-lookup.md)
  - [Lookups](openspec/changes/add-hotel-availability-api/specs/lookups.md)
- [Tasks](openspec/changes/add-hotel-availability-api/tasks.md) — Implementation checklist

### UI Change: Angular Hotel Booking UI

- [Proposal](openspec/changes/add-angular-ui/proposal.md) — Problem statement, solution overview, technology stack
- [Design](openspec/changes/add-angular-ui/design.md) — Component architecture, data flow, technical decisions
- [Specs](openspec/changes/add-angular-ui/specs/) — User interface behavior and interactions
  - [Search Page](openspec/changes/add-angular-ui/specs/search-page-spec.md)
  - [Reservation Page](openspec/changes/add-angular-ui/specs/reservation-page-spec.md)
  - [Bookings Page](openspec/changes/add-angular-ui/specs/bookings-page-spec.md)
  - [Confirmation Page](openspec/changes/add-angular-ui/specs/confirmation-page-spec.md)
- [Tasks](openspec/changes/add-angular-ui/tasks.md) — Implementation checklist

---

## Quick Links

- **Project Setup:** [README.md](README.md) — Installation, running the app, prerequisites
- **API Documentation:** [API.md](API.md) — Endpoint reference with examples
- **OpenSpec Configuration:** [openspec/config.yaml](openspec/config.yaml) — Project conventions and rules

---

## Summary

This project implements a complete hotel booking system with:
- ✅ **Backend API** — .NET 8 Minimal API with Onion Architecture
  - Multi-provider hotel search (PremierStays, BudgetNests)
  - Document-validated reservations
  - Reference-based lookup
  - Lookup APIs for countries and cities
- ✅ **Frontend UI** — Angular single-page application
  - Responsive hotel search with country/city cascade
  - Booking flow with document validation
  - Reservation lookup and confirmation pages
  - Full end-to-end booking experience

**Total Specifications:** 4 API specs + 4 UI specs = **8 complete specifications**
