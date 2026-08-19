# Architecture

> Template. Populated incrementally as modules/client apps are actually built, only when explicitly requested (see [skills/review-architecture.md](skills/review-architecture.md), [agents/architecture-reviewer.md](agents/architecture-reviewer.md), [agents/nextjs-architect.md](agents/nextjs-architect.md)). The *intended* shape is described below (agreed for this template); this file records **verified cross-cutting** facts. Scoped detail lives in:
> - [ARCHITECTURE-BACKEND.md](ARCHITECTURE-BACKEND.md) — module structure convention, layering, dependency direction, shared kernel, data access, API surface
> - [ARCHITECTURE-CLIENTS.md](ARCHITECTURE-CLIENTS.md) — client app index, structure, key areas
>
> Load only the file(s) relevant to the current task's scope — see [context-backend](commands/context-backend.md)/[context-frontend](commands/context-frontend.md)/[context-full](commands/context-full.md) commands.

## Scope of This Document

This file aggregates **verified** architectural facts that are genuinely cross-cutting (span both backend and clients, or describe the integration boundary). Never assume the intended shape below has been fully realized in code — check before asserting.

## Intended Shape (agreed template design, not yet necessarily built)

- **Backend** (`src/`): ASP.NET Core, C#, Modular Monolith. Flat projects directly under `src/` (no `src/Modules/` nesting) — see [ARCHITECTURE-BACKEND.md § Module Structure Convention](ARCHITECTURE-BACKEND.md#backend--module-structure-convention) — plus a shared/building-blocks project(s) and a composition-root host project.
- **Clients** (`clients/`): one or more frontend apps, each in `clients/<app-name>/`. The primary one (`clients/web/`) is Next.js (App Router), TypeScript/React. Do not assume there's only one client app.
- **Integration**: backend MVC controllers are API-only (JSON, no Razor views); each client app is a consumer of that API, never a UI rendered by the backend.

## Integration — API Contract

> How each client consumes the backend API: hand-written fetch calls, a generated typed client (e.g. from OpenAPI), shared DTO types, etc. See [agents/api-contract-reviewer.md](agents/api-contract-reviewer.md).

| App | Client generation strategy | Base URL / env config | Auth flow |
|---|---|---|---|
| admin | Hand-written, one file per feature (`features/<name>/api/<feature>.api.ts`) — no OpenAPI-generated client | Two named backend clients (`identityApi`/`notificationsApi` via `lib/server/api-clients.ts`), each with its own server-only base-URL env var (`IDENTITY_API_BASE_URL`/`NOTIFICATIONS_API_BASE_URL`, base URL owns the version prefix); real-time notifications need client-exposed `NEXT_PUBLIC_SIGNALR_HUB_URL` | Encrypted httpOnly cookie session (`admin_session`, AES-256-GCM), permissions/roles decoded from the access-token JWT; `src/proxy.ts` proactively refreshes a near-expiry token; SignalR handshake gets a short-lived token via a dedicated Server Action — see [DEVELOPMENT.md § Full-Stack Integration Conventions](DEVELOPMENT.md#full-stack-integration-conventions) for detail |

---
_Last synced: 2026-08-19. This root file holds only Scope, Intended Shape, and Integration (cross-cutting) — see [ARCHITECTURE-BACKEND.md](ARCHITECTURE-BACKEND.md)/[ARCHITECTURE-CLIENTS.md](ARCHITECTURE-CLIENTS.md) for the split-out detail._
