# Backend Solution Overview

ASP.NET Core (C#) Modular Monolith backend for the StarterKit template — one solution
(`StarterKit.slnx`), one deployable process (`src/StarterKit.WebApi`). This file is the map; layering
and patterns are in [architecture.md](architecture.md), the project-reference graph in
[dependency-graph.md](dependency-graph.md), and each module's internals in
[modules/<Module>.md](modules/).

## Modules

| Module | Projects | Responsibility | Status |
|---|---|---|---|
| Identity | `src/Identity.Api` + `.Contracts` | Users, roles, claims, JWT auth/token issuance, permission catalog | Built, tested (100). Internal layering still informal — [known-debt.md](../known-debt.md) D1 |
| Notifications | `src/Notifications.Api` + `.Contracts` | Notification storage + real-time SignalR push; admin + self-service surfaces over one table | Built, no tests yet. No "mark all read", nothing sets `Archived` |
| Organization | `src/Organization.Api` + `.Contracts` | Companies, a self-referencing department/team hierarchy (`OrgUnit`), company-scoped employee levels, employees (membership history + optional Identity-login link) | Built, tested (63). Also exposes `IOrgDirectoryService`, a second cross-module seam consumed by LeaveManagement |
| Approval | `src/Approval.Api` + `.Contracts` | A generic, reusable multi-level approval engine — the caller resolves the approver chain and drives the workflow via `IApprovalService`; not tied to any request type | Built, tested (57) |
| LeaveManagement | `src/LeaveManagement.Api` + `.Contracts` | Self-service CRUD for employee leave requests; delegates the entire approval workflow to Approval via `IApprovalService`, resolves approvers/names via Organization's `IOrgDirectoryService` — no decide endpoint of its own | Built, tested (30) |

## Shared / Host Projects

| Project | Responsibility |
|---|---|
| `src/Shared` | Shared kernel: entity/DTO wrappers over vendor `Light.Domain`, `ICurrentUser`/`IDateTime`, `PageQuery`/`SearchQuery`, permission-authorization building blocks (incl. `CurrentUserBase`'s `EmployeeId` claim accessor — see [modules/Identity.md](modules/Identity.md#notable-conventions)), mediator pipeline behaviors, constants. Leaf — no dependencies |
| `src/Infrastructure` | Cross-cutting infra: CORS, health checks, Serilog bootstrap, Mapster config, module/endpoint + API controller base classes, Basic Auth attribute. → `Shared`. EF Core concerns moved out to `Persistence` (2026-07) |
| `src/Persistence` | EF Core provider config, `BaseDbContext`, audit/soft-delete tracking + domain-event dispatch (meant to run inside each module's `SaveChangesAsync`), paging/result helpers, migration-time support. → `Shared` |
| `src/StarterKit.WebApi` | Composition-root host — the only executable. → all five modules, `Infrastructure`, `Shared` |

## Dependency Graph

One-way throughout: `Api`/`Contracts` → `Infrastructure`/`Persistence` → `Shared`, and
`StarterKit.WebApi` → all five business modules. `Shared` is the only true leaf. Five compliant
business-module-to-business-module dependencies exist, each reaching only the target's `Contracts`
seam — the full list and the project-reference diagram are in
[dependency-graph.md](dependency-graph.md). No circular references or boundary violations.

## Entry Points

`src/StarterKit.WebApi/Program.cs` — builds a `WebApplication`, configures Serilog bootstrap logging,
calls `ConfigureServices` / `AddLowercaseControllers` / `AddDefaultJsonOptions` /
`AddInvalidModelStateHandler`, then `ConfigurePipelines()`, `UseWebSockets()`, and
`MapEndpoints(config.GetValue<bool>("AllowAnonymous"))` before `Run()`.

## Data Access

One `DbContext` per module. `Identity`, `Notifications`, `Organization`, `Approval`, and
`LeaveManagement` share one physical database (each `DbConnectionNames.*` aliases `Default`),
separated by schema + table (`system` / `organization` / `approval` / `leave`; Identity uses its own).
Provider is configurable per environment (`InMemory`/`PostgreSQL`/`MSSQL`/`Sqlite` via
`IConfiguration["DbProvider"]`).

| Module | DbContext | Base | Detail |
|---|---|---|---|
| Identity | `IdentityDbContext` | ASP.NET Identity's `IdentityDbContext<...>` | [modules/Identity.md § Data Access](modules/Identity.md#data-access) |
| Notifications | `NotificationDbContext` | `Persistence` `BaseDbContext` | [modules/Notifications.md § Data Access](modules/Notifications.md#data-access) |
| Organization | `OrganizationDbContext` | `BaseDbContext` | [modules/Organization.md § Data Access](modules/Organization.md#data-access) |
| Approval | `ApprovalDbContext` | `BaseDbContext` | [modules/Approval.md § Data Access](modules/Approval.md#data-access) |
| LeaveManagement | `LeaveManagementDbContext` | `BaseDbContext` | [modules/LeaveManagement.md § Data Access](modules/LeaveManagement.md#data-access) |

## External Dependencies

- **`Lightsoft.*` (namespace `Light.*`)** — private vendor family: `.Mediator`/`.Contracts` (mediator
  + `IPublisher`), `.Result` (`Result`/`PagedResult<T>`), `.SharedKernel` (`Light.Domain` base
  types), `.AspNetCore.Authorization` (permission-policy authz), `.AspNetCore.Modularity`
  (`IModuleEndpoint`, `AppModule`), `.AspNetCore.Extensions` (CORS), `.AspNetCore.Swagger` (host),
  `.EntityFrameworkCore`, `.Serilog`, `.ActiveDirectory` (Identity only). Suspected-dead:
  `.EventBus`, `.FileGenerator` — see [known-debt.md](../known-debt.md).
- **EF Core providers** (`Persistence`) — InMemory / Sqlite / SqlServer / Npgsql.
- **`Microsoft.AspNetCore.Identity.EntityFrameworkCore`** (`Identity.Api`) — ASP.NET Identity base
  types.
- **`FluentValidation`** (`Shared`) — backs `ValidationBehaviour`; DI extensions register validators
  in the host.
- **`Mapster`** (`Shared`) — object mapping, configured in `Infrastructure/Mappings/MapsterSettings.cs`.
- **`AspNetCore.HealthChecks.UI.Client`**, **`Spectre.Console`** (startup banner) — host.
- **`Microsoft.AspNetCore.SignalR`** (`Notifications.Api`) — shared-framework reference, not a NuGet
  package.

## Client Integration

`clients/admin/` (a Next.js admin dashboard) consumes all five modules over HTTP — `Identity`,
`Notifications` (REST + a direct WebSocket to `/signalr-hub`), `Organization`, `Approval`, and
`LeaveManagement`. See [../../../clients/admin/docs/architecture/overview.md](../../../clients/admin/docs/architecture/overview.md).

## Notes

<!-- manual: content below this line is human-authored and must be preserved verbatim during sync -->

---
_Last synced: 2026-09-07_
