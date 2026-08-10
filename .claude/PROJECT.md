# Repository Overview

> Template. Populated incrementally, only when analysis is explicitly requested (see [workflows/analyze-solution.md](workflows/analyze-solution.md), [skills/analyze-frontend.md](skills/analyze-frontend.md), [skills/analyze-client.md](skills/analyze-client.md), [workflows/analyze-folder.md](workflows/analyze-folder.md)). Do not fill this in speculatively.
>
> This file holds only the cross-cutting summary. Scoped detail lives in:
> - [PROJECT-BACKEND.md](PROJECT-BACKEND.md) — backend modules, key projects, backend cross-cutting concerns
> - [PROJECT-CLIENTS.md](PROJECT-CLIENTS.md) — client apps
>
> Load only the file(s) relevant to the current task's scope — see [context-backend](commands/context-backend.md)/[context-frontend](commands/context-frontend.md)/[context-full](commands/context-full.md) commands.

## Summary

- **Type**: Starter template monorepo — C#/.NET backend (Modular Monolith) + one or more frontend clients.
- **Backend**: `src/` — ASP.NET Core Web API, API-only MVC controllers. See [PROJECT-BACKEND.md](PROJECT-BACKEND.md).
- **Clients**: `clients/<app-name>/` — one or more apps; the primary one is Next.js, TypeScript/React. See [PROJECT-CLIENTS.md](PROJECT-CLIENTS.md).
- **Status**: Two backend modules (`Identity`, `Notifications`) and composition-root host (`StarterKit.WebApi`) built. The one client app (`clients/admin/`, a Next.js admin dashboard) is actively developed with real integration against both modules — auth/session, Users/Roles CRUD, and real-time Notifications (SignalR) — no longer a UI shell. Verify current state before describing further additions as built.

## Known Entry Points

> Backend: hosted API. Clients: each app's entry/routes. Only list ones confirmed to exist.

- **Backend**: see [PROJECT-BACKEND.md § Backend Entry Points](PROJECT-BACKEND.md#backend-entry-points).
- **Clients**: none catalogued yet — see [PROJECT-CLIENTS.md](PROJECT-CLIENTS.md).

## Open Questions / Gaps

> Cross-cutting only. Backend-specific gaps live in [PROJECT-BACKEND.md § Backend Open Questions / Gaps](PROJECT-BACKEND.md#backend-open-questions--gaps); client-specific gaps in [PROJECT-CLIENTS.md § Client Open Questions / Gaps](PROJECT-CLIENTS.md#client-open-questions--gaps).

- None currently open cross-cutting. (The prior "full-stack integration hasn't been designed yet" item is resolved — see [PROJECT-CLIENTS.md § Client Open Questions / Gaps](PROJECT-CLIENTS.md#client-open-questions--gaps) and `docs/generated/clients/admin/overview.md` § Auth Flow / Backend Integration for the built state.)

---
_Last updated: 2026-08-10 (ROT resync — corrected Status/Open Questions, stale since 2026-08-01: added the `Notifications` module, corrected `clients/admin/`'s status from UI-shell to actively-integrated, resolved the full-stack-integration open question)._
