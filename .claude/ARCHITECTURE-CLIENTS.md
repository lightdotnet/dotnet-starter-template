# Architecture — Clients

> Template. Populated incrementally as client apps are actually built, only when explicitly requested (see [agents/nextjs-architect.md](agents/nextjs-architect.md), [skills/analyze-frontend.md](skills/analyze-frontend.md), [skills/analyze-client.md](skills/analyze-client.md)). This is the clients half of [ARCHITECTURE.md](ARCHITECTURE.md) — see that file for the cross-cutting scope/summary and for [ARCHITECTURE-BACKEND.md](ARCHITECTURE-BACKEND.md) (backend modules).

## Clients — Index

> One row per app actually found under `clients/`. Do not enumerate until an `analyze-frontend`/`analyze-client` pass has run.

| App | Path | Purpose | Stack | Status |
|---|---|---|---|---|
| admin | `clients/admin/` | Admin dashboard | Next.js 16 (App Router, Turbopack), React 19, TypeScript, Tailwind CSS v4 | Scaffolded, UI-shell only — see [PROJECT-CLIENTS.md](PROJECT-CLIENTS.md) |

## Clients — Structure (per app)

> Router style (App vs Pages), data-fetching approach (server components, React Query/SWR, server actions), and state management — only document what's actually observed, per app.

| App | Router | Data fetching | State management | Styling |
|---|---|---|---|---|
| admin | Not yet analyzed in detail | Not yet analyzed | Not yet analyzed | Tailwind CSS v4 (confirmed) |

## Clients — Key Areas (per app)

| App | Area/Route | Path | Responsibility | Notes |
|---|---|---|---|---|
| _unknown_ | | | | Not yet analyzed |

## Clients — Architectural Risks / Debt

> Findings from `review-architecture`/`nextjs-architect` runs go here, tagged with date and app.

- None recorded yet.

---
_Last updated: 2026-08-01 — split out of ARCHITECTURE.md into a clients-only file; content otherwise unchanged (still awaiting a full `analyze-client` pass on `clients/admin/`)._
