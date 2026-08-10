# Architecture — Clients

> Template. Populated incrementally as client apps are actually built, only when explicitly requested (see [agents/nextjs-architect.md](agents/nextjs-architect.md), [skills/analyze-frontend.md](skills/analyze-frontend.md), [skills/analyze-client.md](skills/analyze-client.md)). This is the clients half of [ARCHITECTURE.md](ARCHITECTURE.md) — see that file for the cross-cutting scope/summary and for [ARCHITECTURE-BACKEND.md](ARCHITECTURE-BACKEND.md) (backend modules).

## Clients — Index

> One row per app actually found under `clients/`. Do not enumerate until an `analyze-frontend`/`analyze-client` pass has run.

| App | Path | Purpose | Stack | Status |
|---|---|---|---|---|
| admin | `clients/admin/` | Admin dashboard | Next.js 16 (App Router), React 19, TypeScript, Tailwind CSS v4 | Actively developed, fully analyzed — see [docs/generated/clients/admin/architecture.md](docs/generated/clients/admin/architecture.md) |

## Clients — Structure (per app)

> Router style (App vs Pages), data-fetching approach (server components, React Query/SWR, server actions), and state management — only document what's actually observed, per app.

| App | Router | Data fetching | State management | Styling |
|---|---|---|---|---|
| admin | App Router, rooted at `src/app/` — see [docs/generated/clients/admin/architecture.md](docs/generated/clients/admin/architecture.md#layering) | Server-only, hand-written per endpoint (`features/*/api/*.ts` via `lib/server/http.ts`), Server Actions for writes, plus a direct SignalR WebSocket for real-time notifications — no client-side data-fetching library — see [docs/generated/clients/admin/overview.md](docs/generated/clients/admin/overview.md#backend-integration) | Local component state + React Context, no global state library — see [docs/generated/clients/admin/overview.md](docs/generated/clients/admin/overview.md#structure) | Tailwind CSS v4, CSS-first config (confirmed) |

## Clients — Key Areas (per app)

> See [docs/generated/clients/admin/overview.md](docs/generated/clients/admin/overview.md#key-routesareas) for the full, per-route table (kept there to avoid duplicating detail across two docs).

| App | Area/Route | Path | Responsibility | Notes |
|---|---|---|---|---|
| admin | Dashboard, Users, Roles, Notifications, Profile, Auth | `src/app/**` | See generated overview's Key Routes/Areas table | Full detail lives in the generated doc, not duplicated here |

## Clients — Architectural Risks / Debt

> Findings from `review-architecture`/`nextjs-architect` runs go here, tagged with date and app.

- See [docs/generated/clients/admin/architecture.md](docs/generated/clients/admin/architecture.md#known-architectural-risks--debt) for the full, maintained list (e.g. `proxy.ts`'s runtime pin is unverified, no automated test suite, `/settings` nav entry has no route). Not duplicated here to avoid drift between two copies of the same list.

---
_Last updated: 2026-08-10 (ROT resync — removed the stale "dashboard still mock data" item: the Dashboard feature was deleted and replaced by a real Home page on 2026-08-10)._
