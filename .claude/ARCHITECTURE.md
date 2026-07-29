# Architecture

> Template. Populated incrementally as modules/client apps are actually built, only when explicitly requested (see [skills/review-architecture.md](skills/review-architecture.md), [agents/architecture-reviewer.md](agents/architecture-reviewer.md), [agents/nextjs-architect.md](agents/nextjs-architect.md)). The *intended* shape is described below (agreed for this template); this file records **verified** facts about what's actually been built — do not treat the intended shape as already implemented until checked.

## Scope of This Document

This file aggregates **verified** architectural facts only, split into Backend, Clients, and Integration. Never assume the intended shape below has been fully realized in code — check before asserting.

## Intended Shape (agreed template design, not yet necessarily built)

- **Backend** (`src/`): ASP.NET Core, C#, Modular Monolith. `src/Modules/<ModuleName>/{Domain,Application,Infrastructure,Api}` per module, plus a shared/building-blocks project and a composition-root host project.
- **Clients** (`clients/`): one or more frontend apps, each in `clients/<app-name>/`. The primary one (`clients/web/`) is Next.js (App Router), TypeScript/React. Do not assume there's only one client app.
- **Integration**: backend MVC controllers are API-only (JSON, no Razor views); each client app is a consumer of that API, never a UI rendered by the backend.

## Backend — Layering (per module)

| Module | Domain | Application | Infrastructure | Api | Notes |
|---|---|---|---|---|---|
| _unknown_ | | | | | Not yet scaffolded/verified |

## Backend — Dependency Direction

> Verified via project references, not inferred from folder names. Expected direction: `Api → Application → Domain`; `Infrastructure → Application` (implements interfaces) and `→ Domain`. Modules must not reference another module's `Domain`/`Infrastructure` directly.

- _unknown_

## Backend — Shared Kernel / Building Blocks

> E.g. base entities, shared abstractions, common EF Core conventions used across modules — only list what's confirmed in code, with file references.

- _unknown_

## Backend — Data Access

> One `DbContext` per module is the intended default. Do not assume a single repo-wide context.

| Module | DbContext | Provider | Notes |
|---|---|---|---|
| _unknown_ | | | |

## Backend — API Surface

> REST/JSON, API-only controllers. Versioning strategy per `.claude/DEVELOPMENT.md` once observed.

| Module | Route prefix | Versioning | Notes |
|---|---|---|---|
| _unknown_ | | | |

## Clients — Index

> One row per app actually found under `clients/`. Do not enumerate until an `analyze-frontend`/`analyze-client` pass has run.

| App | Path | Purpose | Stack | Status |
|---|---|---|---|---|
| _unknown_ | | | | not yet analyzed |

## Clients — Structure (per app)

> Router style (App vs Pages), data-fetching approach (server components, React Query/SWR, server actions), and state management — only document what's actually observed, per app.

| App | Router | Data fetching | State management | Styling |
|---|---|---|---|---|
| _unknown_ | | | | |

## Clients — Key Areas (per app)

| App | Area/Route | Path | Responsibility | Notes |
|---|---|---|---|---|
| _unknown_ | | | | |

## Integration — API Contract

> How each client consumes the backend API: hand-written fetch calls, a generated typed client (e.g. from OpenAPI), shared DTO types, etc. See [agents/api-contract-reviewer.md](agents/api-contract-reviewer.md).

| App | Client generation strategy | Base URL / env config | Auth flow |
|---|---|---|---|
| _unknown_ | | | |

## Architectural Risks / Debt

> Findings from `review-architecture` runs go here, tagged with date and scope (backend/client app/integration).

- _unknown_

---
_Last updated: never (template not yet populated with verified facts)_
