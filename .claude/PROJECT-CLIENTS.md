# Project — Clients

> Template. Populated incrementally, only when analysis is explicitly requested (see [skills/analyze-frontend.md](skills/analyze-frontend.md), [skills/analyze-client.md](skills/analyze-client.md)). Do not fill this in speculatively. This is the clients half of [PROJECT.md](PROJECT.md) — see that file for the cross-cutting summary and for [PROJECT-BACKEND.md](PROJECT-BACKEND.md) (backend modules).

## Client Apps

> One entry per app discovered under `clients/`. Do not enumerate until an `analyze-frontend`/`analyze-client` pass has run. There may be more than one — never collapse this table to a single implied "the frontend."

| App | Path | Responsibility | Stack | Status |
|---|---|---|---|---|
| admin | `clients/admin/` | Admin dashboard — UI shell only (design-token system, dark mode, reusable component library, collapsible/responsive sidebar + full-width topbar). No calls to `src/Identity.Api` yet — all data on the one existing page (`/`) is mock data. | Next.js 16 (App Router, Turbopack), React 19, TypeScript, Tailwind CSS v4, shadcn-generated primitives on `radix-ui` + `class-variance-authority`, `next-themes`, pnpm | Scaffolded — see [docs/generated/clients/admin/overview.md](docs/generated/clients/admin/overview.md) |

## Client Cross-Cutting Concerns

> Shared conventions across client apps (e.g. a shared API client package reused across clients), only once more than one app exists and a real shared pattern is verified.

- None verified yet — only one client app (`admin`) exists so far.

## Client Open Questions / Gaps

- `clients/admin/` is UI-shell-only — real auth/data wiring to `src/Identity.Api` (API client layer, CORS/env config, token handling) hasn't been designed yet.

---
_Last updated: 2026-08-01 — split out of PROJECT.md into a clients-only file; content otherwise unchanged (`clients/admin/` still the only app, UI-shell only)._
