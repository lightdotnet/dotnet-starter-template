# Project — Clients

> Template. Populated incrementally, only when analysis is explicitly requested (see [skills/analyze-frontend.md](skills/analyze-frontend.md), [skills/analyze-client.md](skills/analyze-client.md)). Do not fill this in speculatively. This is the clients half of [PROJECT.md](PROJECT.md) — see that file for the cross-cutting summary and for [PROJECT-BACKEND.md](PROJECT-BACKEND.md) (backend modules).

## Client Apps

> One entry per app discovered under `clients/`. Do not enumerate until an `analyze-frontend`/`analyze-client` pass has run. There may be more than one — never collapse this table to a single implied "the frontend."

| App | Path | Responsibility | Stack | Status |
|---|---|---|---|---|
| admin | `clients/admin/` | Admin dashboard — real backend integration against `src/Identity.Api`: encrypted-cookie auth with proactive token refresh, full Users/Roles CRUD, real-time Notifications (SignalR) with a management page, permission-gated nav. Dashboard page (`/`) is the one remaining mock-data exception. | Next.js 16 (App Router), React 19, TypeScript, Tailwind CSS v4, shadcn-generated primitives on `radix-ui` + `class-variance-authority`, `next-themes`, `@microsoft/signalr`, pnpm | Actively developed — see [docs/generated/clients/admin/overview.md](docs/generated/clients/admin/overview.md) for full detail |

## Client Cross-Cutting Concerns

> Shared conventions across client apps (e.g. a shared API client package reused across clients), only once more than one app exists and a real shared pattern is verified.

- None verified yet — only one client app (`admin`) exists so far.

## Client Open Questions / Gaps

- ~~`clients/admin/` is UI-shell-only — real auth/data wiring to `src/Identity.Api` hasn't been designed yet~~ — resolved; see [docs/generated/clients/admin/overview.md](docs/generated/clients/admin/overview.md) (Auth Flow, Backend Integration sections).
- Remaining gaps, per [docs/generated/clients/admin/architecture.md](docs/generated/clients/admin/architecture.md#known-architectural-risks--debt): dashboard page still renders mock data; no automated test suite; `/settings` nav entry has no route yet.

---
_Last updated: 2026-08-04 — corrected the `admin` row and Open Questions to match its current state (real auth/CRUD/notifications, not UI-shell/mock-data); pointers added to the generated admin docs rather than re-deriving detail here._
