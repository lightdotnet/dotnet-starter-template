# Integration — Backend ↔ Clients

Cross-cutting facts that span both `src/` and `clients/*` — the integration boundary itself, not owned by either project. See [src/CLAUDE.md](../src/CLAUDE.md) and [clients/admin/CLAUDE.md](../clients/admin/CLAUDE.md) for each side's own architecture.

## Intended Shape

- **Backend** (`src/`): ASP.NET Core, C#, Modular Monolith. Flat projects directly under `src/`, plus a shared/building-blocks project(s) and a composition-root host project.
- **Clients** (`clients/`): one or more frontend apps, each in `clients/<app-name>/`. The primary one (`clients/admin/`) is Next.js (App Router), TypeScript/React. Do not assume there's only one client app.
- **Integration**: backend MVC controllers are API-only (JSON, no Razor views); each client app is a consumer of that API, never a UI rendered by the backend. No shared source, no shared DB access, no in-process calls between `src/` and `clients/*`.

## API Contract

| App | Client generation strategy | Base URL / env config | Auth flow |
|---|---|---|---|
| admin | Hand-written, one file per feature (`features/<name>/api/<feature>.api.ts`) — no OpenAPI-generated client | Two named backend clients (`identityApi`/`notificationsApi` via `lib/server/api-clients.ts`), each with its own server-only base-URL env var (`IDENTITY_API_BASE_URL`/`NOTIFICATIONS_API_BASE_URL`, base URL owns the version prefix); real-time notifications need client-exposed `NEXT_PUBLIC_SIGNALR_HUB_URL` | Encrypted httpOnly cookie session (`admin_session`, AES-256-GCM), permissions/roles decoded from the access-token JWT; `src/proxy.ts` enforces the session cap, `components/layout/session-gate.tsx` proactively refreshes a near-expiry token client-side; SignalR handshake gets a short-lived token via a dedicated Server Action — see [clients/admin/docs/architecture/overview.md § Auth Flow](../clients/admin/docs/architecture/overview.md#auth-flow) for detail |

---
_Last synced: 2026-08-22_
