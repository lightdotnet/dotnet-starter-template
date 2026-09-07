# Integration — Backend ↔ Clients

Cross-cutting facts that span both `src/` and `clients/*` — the integration boundary itself, not owned by either project. See [src/CLAUDE.md](../src/CLAUDE.md) and [clients/admin/CLAUDE.md](../clients/admin/CLAUDE.md) for each side's own architecture.

## Intended Shape

- **Backend** (`src/`): ASP.NET Core, C#, Modular Monolith. Flat projects directly under `src/`, plus a shared/building-blocks project(s) and a composition-root host project.
- **Clients** (`clients/`): one or more frontend apps, each in `clients/<app-name>/`. The primary one (`clients/admin/`) is Next.js (App Router), TypeScript/React. Do not assume there's only one client app.
- **Integration**: backend MVC controllers are API-only (JSON, no Razor views); each client app is a consumer of that API, never a UI rendered by the backend. No shared source, no shared DB access, no in-process calls between `src/` and `clients/*`.

## API Contract

| App | Client generation strategy | Base URL / env config | Auth flow |
|---|---|---|---|
| admin | Hand-written, one consolidated `<feature>.api.ts` per feature under `modules/<domain>/<feature>/api/` — no OpenAPI-generated client | Five named backend clients (`identityApi`/`notificationsApi`/`organizationApi`/`approvalApi`/`leaveManagementApi` via `lib/server/`), each with its own server-only base-URL env var (`IDENTITY_API_BASE_URL`/`NOTIFICATIONS_API_BASE_URL`/`ORGANIZATION_API_BASE_URL`/`APPROVAL_API_BASE_URL`/`LEAVE_MANAGEMENT_API_BASE_URL`; the base URL owns its full path/version prefix). Real-time notifications use a **server-only** `SIGNALR_HUB_URL` (not `NEXT_PUBLIC_`-inlined) — read server-side and handed to the browser at connect time by a Server Action, so it stays a runtime setting | Encrypted httpOnly cookie session (`admin_session`, AES-256-GCM), permissions/roles decoded from the access-token JWT; `src/proxy.ts` enforces the session cap, `components/layout/session-gate.tsx` proactively refreshes a near-expiry token client-side; the SignalR handshake gets a short-lived token via a dedicated Server Action — see [clients/admin/docs/architecture/overview.md § Auth Flow](../clients/admin/docs/architecture/overview.md#auth-flow) for detail |

## Notable cross-cutting facts

- **Notification deep links.** A `Notification.Url` starting with `/` is an app-relative deep link the admin client renders as a `next/link` (e.g. the Approval module sends `/approvals/requests/{id}` so clicking the notification opens that request). External/absolute URLs stay plain non-navigating rows.
- **`employee_id` claim.** When an employee is linked to an Identity login, that user carries an `employee_id` claim in the JWT; the Approval module's self-service create reads it to stamp `RequesterEmployeeId` server-side, and LeaveManagement uses it to scope a caller's own requests.
- **Leave requests delegate approval, not the reverse.** The admin client's `/leave-requests` pages call `LeaveManagement.Api` for CRUD only; the multi-level decision workflow runs in `Approval.Api` (LeaveManagement drives it server-side via `IApprovalService`). The leave-request detail page links out to `/approvals/requests/{approvalRequestId}` — decisions happen in the existing `/approvals` UI, not in a leave-specific screen.

---
_Last synced: 2026-09-07_
