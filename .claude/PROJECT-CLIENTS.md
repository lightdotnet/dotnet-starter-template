# Project — Clients

> Template. Populated incrementally, only when analysis is explicitly requested (see [skills/analyze-frontend.md](skills/analyze-frontend.md), [skills/analyze-client.md](skills/analyze-client.md)). Do not fill this in speculatively. This is the clients half of [PROJECT.md](PROJECT.md) — see that file for the cross-cutting summary and for [PROJECT-BACKEND.md](PROJECT-BACKEND.md) (backend modules).

## Client Apps

> One entry per app discovered under `clients/`. Do not enumerate until an `analyze-frontend`/`analyze-client` pass has run. There may be more than one — never collapse this table to a single implied "the frontend."

| App | Path | Responsibility | Stack | Status |
|---|---|---|---|---|
| admin | `clients/admin/` | Admin dashboard — real backend integration against both `src/Identity.Api` and `src/Notifications.Api`: encrypted-cookie auth with proactive token refresh, full Users/Roles CRUD, real-time Notifications (SignalR) with both a topbar bell and a two-pane inbox on the Home page (`/`), permission-gated nav. No remaining mock-data page — the former Dashboard was replaced by a real Home page. | Next.js 16 (App Router), React 19, TypeScript, Tailwind CSS v4, shadcn-generated primitives on `radix-ui` + `class-variance-authority`, `next-themes`, `@microsoft/signalr`, pnpm | Actively developed — see [docs/generated/clients/admin/overview.md](docs/generated/clients/admin/overview.md) for full detail |

## Client Cross-Cutting Concerns

> Shared conventions across client apps (e.g. a shared API client package reused across clients), only once more than one app exists and a real shared pattern is verified.

- None verified yet — only one client app (`admin`) exists so far.

## Client Open Questions / Gaps

- ~~`clients/admin/` is UI-shell-only — real auth/data wiring to `src/Identity.Api` hasn't been designed yet~~ — resolved; see [docs/generated/clients/admin/overview.md](docs/generated/clients/admin/overview.md) (Auth Flow, Backend Integration sections).
- ~~Dashboard page (`/`) still renders mock data~~ — resolved 2026-08-10: the Dashboard feature was deleted and replaced by a real Home page (session-backed profile summary + a live notification inbox).
- Remaining gaps, per [docs/generated/clients/admin/architecture.md](docs/generated/clients/admin/architecture.md#known-architectural-risks--debt): no automated test suite; `/settings` nav entry has no route yet.

---
_Last updated: 2026-08-10 (ROT resync — the dashboard/mock-data references were stale since the 2026-08-10 Home-page replacement; corrected the `admin` row and Open Questions accordingly)._
