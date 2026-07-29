<!--
Template: Architecture (Backend or one Client App)
Used by: skills/review-architecture.md, agents/architecture-reviewer.md, agents/nextjs-architect.md
Output location: .claude/docs/generated/backend/architecture.md or .claude/docs/generated/clients/<app-name>/architecture.md
Do not populate this file itself — copy its structure into the generated output.
-->

# Architecture: <Backend | Client app name>

## Layering

_Backend: layers actually observed per module (Domain/Application/Infrastructure/Api). Client app: route/component/data-layer organization actually observed._

## Dependency Direction

_Verified via project references (backend) or actual imports (client app) — direction diagram or table._

## Key Design Patterns

_Patterns actually in use (e.g. CQRS, mediator, repository on the backend; server actions, colocated data fetching in this client app) — only if verified in code._

## Shared Kernel / Common Building Blocks Used

_Which shared/common project(s) or packages this depends on, and how — for a client app, include any package shared with sibling apps under `clients/`._

## Module/Route Boundaries

_Backend: which modules exist and their boundary rules. Client app: which route groups/features exist and how they're isolated._

## Known Architectural Risks / Debt

| Finding | Severity | Notes |
|---|---|---|

## Notes

<!-- manual: content below this line is human-authored and must be preserved verbatim during sync -->

---
_Generated: <date> — scope: <Backend | client app "<app-name>"> — see .claude/CLAUDE.md for update rules._
