# AI Tooling & Prompts

## AI Tools Used

| Tool | Purpose | Integration Type |
|------|---------|------------------|
| **GitHub Copilot** | Code generation, autocomplete, test scaffolding | IDE-integrated (VS Code) |
| **OpenSpec CLI** | Spec-driven workflow, proposal generation, design documentation | CLI + GitHub Copilot Chat |

---

## Documentation Structure

This project follows a **spec-driven approach** where all design rationale, architecture decisions, and key judgement calls are documented in dedicated OpenSpec files. This document serves as an index to those files.

### Design Rationale & Architecture Decisions

All design decisions are documented in the following locations:

#### API Implementation
- **Design Document**: [openspec/changes/add-hotel-availability-api/design.md](openspec/changes/add-hotel-availability-api/design.md)
  - Onion Architecture layer placement
  - Interface placement rationale
  - Provider normalization strategy (PascalCase vs snake_case)
  - Mapper pattern decisions
  - Domain validation approach
  - Error handling middleware design
  - Reference number generation strategy

- **Proposal**: [openspec/changes/add-hotel-availability-api/proposal.md](openspec/changes/add-hotel-availability-api/proposal.md)
  - Problem statement and business requirements
  - Solution overview and scope

- **Specifications**: [openspec/changes/add-hotel-availability-api/specs/](openspec/changes/add-hotel-availability-api/specs/)
  - Hotel search endpoint requirements
  - Reservation endpoint with document validation rules
  - Reservation lookup endpoint
  - Lookup endpoints (countries, cities)

#### UI Implementation
- **Design Document**: [openspec/changes/add-angular-ui/design.md](openspec/changes/add-angular-ui/design.md)
  - Component architecture
  - Standalone components decision
  - Reactive forms with custom validators
  - HTTP interceptor strategy
  - Lazy loading rationale

- **Proposal**: [openspec/changes/add-angular-ui/proposal.md](openspec/changes/add-angular-ui/proposal.md)
  - UI requirements and user flows
  - Technology stack decisions

#### Future Improvements & Reflection
- **Reflection Document**: [reflections.md](reflections.md)
  - What would be improved with more time
  - Database persistence strategy
  - User management system
  - Document upload and blob storage
  - Testing automation improvements
  - GitHub MCP integration

---

## AI Workflow

### Spec-First Design with OpenSpec

OpenSpec generated proposals, design documents, specifications, and task breakdowns. All artifacts were reviewed and validated before implementation.

**Key Design Decisions**: Navigate to the design files above to see:
- Architecture rationale and layer placement decisions
- Provider abstraction and normalization strategy
- Domain validation approach and error handling design
- Technology stack choices and trade-off analysis

### GitHub Copilot for Implementation

Copilot provided inline suggestions for code generation, test scaffolding, and boilerplate reduction.

**Interaction Model**: Inline autocomplete (not conversational prompts) - Copilot reads file context and OpenSpec specs to provide contextual suggestions.

---

## Navigation Guide

For detailed information about specific aspects of the project:

| **Topic** | **Document** |
|-----------|-------------|
| **All Architecture Decisions** | [API design.md](openspec/changes/add-hotel-availability-api/design.md) |
| **UI Architecture & Components** | [UI design.md](openspec/changes/add-angular-ui/design.md) |
| **Business Requirements** | [API proposal.md](openspec/changes/add-hotel-availability-api/proposal.md), [UI proposal.md](openspec/changes/add-angular-ui/proposal.md) |
| **Endpoint Specifications** | [spec.md](spec.md) (index to all specs) |
| **Future Enhancements** | [reflections.md](reflections.md) |

---

## Summary

This case study demonstrates a **modern, spec-driven AI workflow** where design rationale is comprehensively documented in OpenSpec artifacts rather than scattered chat transcripts. This approach prioritizes **persistent, versioned design documentation** over ephemeral prompt logs.
