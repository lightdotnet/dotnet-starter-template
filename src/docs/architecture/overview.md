# Backend Solution Overview

## Purpose

ASP.NET Core (C#) Modular Monolith backend for the StarterKit template. The pre-module shared kernel (`src/Shared`, `src/Infrastructure`) has since gained `src/Persistence` (EF Core concerns split out of `Infrastructure`), three business modules — `Identity` (`src/Identity.Api` + `src/Identity.Contracts`), `Notifications` (`src/Notifications.Api` + `src/Notifications.Contracts`), and `Organization` (`src/Organization.Api` + `src/Organization.Contracts`) — and a working composition-root host, `src/StarterKit.WebApi` (`Program.cs`).

Each module now has its own deep-dive doc under `modules/<ModuleName>.md` — this file and the other solution-wide docs keep only a summary; see the linked module doc for full detail (layering, public contract, data access, dependencies, conventions).

## Modules

| Module | Path | Responsibility | Status |
|---|---|---|---|
| Identity | `src/Identity.Api/Identity.Api.csproj` + `src/Identity.Contracts/Identity.Contracts.csproj` | Users, roles, claims, JWT auth/token issuance, plus a permission catalog (`PermissionsController`, `GET permissions`). See [modules/Identity.md](modules/Identity.md). | Built, tested (`tests/Identity.Tests`, 100 tests). Internal layering still informal — all writes + user search go through mediator commands/queries whose handlers still delegate to the service classes (see modules/Identity.md, D1). |
| Notifications | `src/Notifications.Api/Notifications.Api.csproj` + `src/Notifications.Contracts/Notifications.Contracts.csproj` | Notification storage + real-time push over SignalR; admin "browse + send" surface (permission-gated) and a self-service "my notifications" surface (auto-scoped, no permission). See [modules/Notifications.md](modules/Notifications.md). | Built. No automated test coverage yet. Two known gaps: no "mark all read" endpoint, no path ever sets `Archived` status. |
| Organization | `src/Organization.Api/Organization.Api.csproj` + `src/Organization.Contracts/Organization.Contracts.csproj` | Companies, a self-referencing department/team hierarchy (`OrgUnit`, unified via a `Type` discriminator), company-scoped configurable employee levels, and employees — including membership history in the hierarchy and an optional link to an Identity login. See [modules/Organization.md](modules/Organization.md). | Built, tested (`tests/Organization.Tests`, 46 tests). CQRS handlers hold their logic directly (no service-class indirection) — a deliberate deviation from Identity/Notifications' D1 pattern. |

## Shared/Host Projects

| Project | Path | Responsibility |
|---|---|---|
| Shared | `src/Shared` | Shared kernel: base entity/DTO wrappers over vendor `Light.Domain` types, `ICurrentUser`/`IDateTime` abstractions, `PageQuery`/`SearchQuery`, permission-based authorization building blocks (`SuperUserPolicy`, `AccessControl`, `CurrentUserBase`, `AuthorizationHandler`), mediator pipeline behaviors (`LoggingBehaviour`, `ValidationBehaviour`), constants. Leaf project — no dependencies. |
| Infrastructure | `src/Infrastructure` | Cross-cutting infrastructure: CORS, health checks, Serilog bootstrap logging (`AppLogging`), Mapster config, module/endpoint base classes (`AppModule`, `AppModuleEndpoint`), API controller base classes, Basic Auth attribute. Depends on `Shared`. EF Core/DbContext concerns moved out to `Persistence` during the 2026-07 refactor — no longer here. |
| Persistence | `src/Persistence` | EF Core provider configuration (`DbContextExtensions`/`DbProvider`, Sqlite `DateTimeOffset` workaround), DbContext base class (`Context/BaseDbContext`), audit/soft-delete tracking + domain-event dispatch (meant to run inside each module's `SaveChangesAsync`), generic paging/result helpers, migration-time runtime support. Depends on `Shared`. |
| StarterKit.WebApi | `src/StarterKit.WebApi` | Composition-root host — the only executable/deployable project. Depends on `Identity.Api`, `Notifications.Api`, `Organization.Api`, `Infrastructure`, `Shared`. |

## Dependency Graph

One-way throughout: `Api`/`Contracts` projects → `Infrastructure`/`Persistence` → `Shared`, and `StarterKit.WebApi` (composition-root host) → all three business modules. `Shared` is a leaf; `Notifications.Contracts` is a true leaf; `Identity.Contracts` is not (references `Shared` to reach a transitive package). `Organization.Api` → `Identity.Contracts` is the module's one cross-module dependency (employee-login integration via `IUserService`), mirroring `Identity.Api` → `Notifications.Contracts` — both reach only the other module's `Contracts` seam. No circular references or cross-module boundary violations found — see [dependency-graph.md](dependency-graph.md) for the full project-reference diagram and package references.

## Entry Points

`src/StarterKit.WebApi/Program.cs` — the composition-root host. Builds a `WebApplication`, configures Serilog bootstrap logging, calls `builder.Services.ConfigureServices(builder.Configuration)`, wires `AddLowercaseControllers`/`AddDefaultJsonOptions`/`AddInvalidModelStateHandler`, then `app.ConfigurePipelines()`, `app.UseWebSockets()`, and `app.MapEndpoints(builder.Configuration.GetValue<bool>("AllowAnonymous"))` before `app.Run()`.

## Data Access

One `DbContext` per module is the intended default.

| Module | DbContext | Provider | Notes |
|---|---|---|---|
| Identity | `IdentityDbContext` | Same provider selection as below. | Extends ASP.NET Identity's `IdentityDbContext<...>` directly. See [modules/Identity.md § Data Access](modules/Identity.md#data-access) for full detail (soft-delete deviation, indexes, migration history). |
| Notifications | `NotificationDbContext` | Configured via `Persistence.DbContextExtensions.AddConfiguredDbContext`/`GetDbProvider` (`InMemory`/`PostgreSQL`/`MSSQL`/`Sqlite`, selected via `IConfiguration["DbProvider"]`) | Extends `Persistence/Context/BaseDbContext.cs` (unlike Identity). Shares the same physical database/connection string as `Identity` (`DbConnectionNames.Identity` aliases `DbConnectionNames.Default`), separated by schema (`system`) + table only. See [modules/Notifications.md § Data Access](modules/Notifications.md#data-access) for full detail (indexing gap, column lengths). |
| Organization | `OrganizationDbContext` | Same provider selection as above (`DbConnectionNames.Organization`, also aliasing `Default`). | Extends `Persistence/Context/BaseDbContext.cs`. Shares the same physical database as Identity/Notifications, separated by schema (`organization`) + table. See [modules/Organization.md § Data Access](modules/Organization.md#data-access) for full detail (five tables, the Restrict→Cascade FK fix, seed data). |

## External Dependencies

- **`Lightsoft.*` (namespace `Light.*`)** — private vendor package family: `Lightsoft.Mediator`/`.Contracts` (mediator + `INotification`/`IPublisher`), `Lightsoft.Result` (`Result`/`Result<T>`/`Paged<T>`/`PagedResult<T>`), `Lightsoft.SharedKernel` (`Light.Domain` base types), `Lightsoft.AspNetCore.Authorization` (permission-policy authorization), `Lightsoft.AspNetCore.Modularity` (`IModuleEndpoint`, `AppModule`), `Lightsoft.AspNetCore.Extensions` (CORS helpers), `Lightsoft.AspNetCore.Swagger` (host only), `Lightsoft.EntityFrameworkCore`, `Lightsoft.Serilog`, `Lightsoft.FileGenerator`, `Lightsoft.EventBus`, `Lightsoft.ActiveDirectory` (Identity module only).
- **EF Core providers** (`Persistence`): `Microsoft.EntityFrameworkCore.InMemory`/`.Sqlite`/`.SqlServer`, `Npgsql.EntityFrameworkCore.PostgreSQL`, `SQLitePCLRaw.bundle_e_sqlite3`.
- **`Microsoft.AspNetCore.Identity.EntityFrameworkCore`** (`Identity.Api`) — ASP.NET Identity base types (`IdentityUser`, `IdentityRole`, `IdentityDbContext<...>`) that the module's entities/`AppIdentityDbContext` extend.
- **`FluentValidation`** (`Shared`) — backs `ValidationBehaviour<,>`; **`FluentValidation.DependencyInjectionExtensions`** (host) registers validators.
- **`Mapster`** (`Shared`) — object mapping; configured in `Infrastructure/Mappings/MapsterSettings.cs`.
- **`AspNetCore.HealthChecks.UI.Client`** (`Infrastructure`, host) — health check endpoint response formatting.
- **`Spectre.Console`** (host) — startup console banner (`Program.cs`).
- **`Microsoft.AspNetCore.SignalR`** (`Notifications.Api`) — ASP.NET Core shared framework, not a separate NuGet package; backs the module's real-time push hub. See [modules/Notifications.md](modules/Notifications.md).

## Client Integration

`clients/admin/` (a Next.js admin dashboard app) consumes all three modules: `Identity` (users/roles/auth), `Notifications` (`notification`/`user_notification` REST endpoints plus a direct WebSocket connection to `/signalr-hub`), and `Organization` (`company`/`org_unit`/`employee_level`/`employee` REST endpoints). See [../../../clients/admin/docs/architecture/overview.md](../../../clients/admin/docs/architecture/overview.md) for the frontend-side detail.

## Notes

<!-- manual: content below this line is human-authored and must be preserved verbatim during sync -->

---
_Last synced: 2026-09-05_
