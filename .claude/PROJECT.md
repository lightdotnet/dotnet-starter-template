# Repository Overview

> Template. Populated incrementally, only when analysis is explicitly requested (see [workflows/analyze-solution.md](workflows/analyze-solution.md), [skills/analyze-frontend.md](skills/analyze-frontend.md), [skills/analyze-client.md](skills/analyze-client.md), [workflows/analyze-folder.md](workflows/analyze-folder.md)). Do not fill this in speculatively.

## Summary

- **Type**: Starter template monorepo — C#/.NET backend (Modular Monolith) + one or more frontend clients.
- **Backend**: `src/` — ASP.NET Core Web API, API-only MVC controllers.
- **Clients**: `clients/<app-name>/` — one or more apps; the primary one is Next.js, TypeScript/React.
- **Status**: Template scaffold stage — verify current state before describing it as built; do not assume the folders above exist until checked.

## Backend Modules

> One entry per module discovered under `src/Modules/`. Do not enumerate until an `analyze-module`/`analyze-solution` pass has actually run for that module.

| Module | Path | Responsibility | Status |
|---|---|---|---|
| _unknown_ | | | not yet analyzed |

## Backend Key Projects

> Populated per-project as `analyze-project` runs are performed (e.g. shared/building-blocks, composition-root host). Do not assume completeness.

| Project | Path | Responsibility | Depends on |
|---|---|---|---|
| _unknown_ | | | |

## Client Apps

> One entry per app discovered under `clients/`. Do not enumerate until an `analyze-frontend`/`analyze-client` pass has run. There may be more than one — never collapse this table to a single implied "the frontend."

| App | Path | Responsibility | Stack | Status |
|---|---|---|---|---|
| _unknown_ | | | | not yet analyzed |

## Cross-Cutting Concerns

> Only note items actually verified in code (e.g. shared logging, shared base entities, shared EF Core conventions, a shared API client package reused across clients). Do not guess.

- _unknown_

## Known Entry Points

> Backend: hosted API. Clients: each app's entry/routes. Only list ones confirmed to exist.

- _unknown_

## Open Questions / Gaps

- _unknown — track things discovered as "needs analysis" here so future sessions don't re-derive them from scratch._

---
_Last updated: never (template not yet populated)_
