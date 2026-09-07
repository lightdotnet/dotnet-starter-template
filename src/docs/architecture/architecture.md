# Architecture: Backend

Layering, dependency direction, and cross-cutting patterns. Per-module internals, public contracts,
and module-specific debt live in each module doc under [modules/](modules/); the project-reference
graph and the cross-module dependency inventory live in [dependency-graph.md](dependency-graph.md).

## Layering

Module structure convention (adopted 2026-07-30): every module is one or more flat projects directly
under `src/` (no `src/Modules/` nesting) — either a single `<Module>` / `<Module>.Api` project
organized by folder, or, if complex enough, split Clean-Architecture-style into
`<Module>.Domain`/`.Application`/`.Infrastructure`/`.Api`. Every module also gets a
`<Module>.Contracts` seam — the only project other modules or the host may reference.

Five modules exist, all single-project (`.Api` suffix kept as a deliberate future-extraction
candidate). Structural summary only — full layering per module doc:

| Module | Structure | CQRS shape | Tests |
|---|---|---|---|
| Identity | `Identity.Api` + `.Contracts` | Handlers delegate to `UserService`/`RoleService` — a half-migration ([known-debt.md](../known-debt.md) D1) | `tests/Identity.Tests`, 100 |
| Notifications | `Notifications.Api` + `.Contracts` | Same half-migration as Identity; controllers split by **audience** (admin vs. self-service) not resource | none yet |
| Organization | `Organization.Api` + `.Contracts` (seam split into per-feature subfolders) | Handlers own their `OrganizationDbContext` logic directly — no service layer | `tests/Organization.Tests`, 63 |
| Approval | `Approval.Api` + `.Contracts` | Write path behind `IApprovalService` (must be DI-reachable cross-module); read-path handlers own their logic directly | `tests/Approval.Tests`, 57 |
| LeaveManagement | `LeaveManagement.Api` + `.Contracts` | Handlers own their logic directly (Organization's shape); inject `IApprovalService` + `IOrgDirectoryService` straight into constructors | `tests/LeaveManagement.Tests`, 30 |

Below the module layer: `src/Shared` (leaf) and `src/Persistence` (→ `Shared`) are the pre-module
shared kernel; `src/Infrastructure` (→ `Shared`) is cross-cutting infra, no longer holding EF Core
concerns (moved to `Persistence` in the 2026-07 refactor). `Persistence`'s
`QueryableResultExtensions.ToPagedAsync` clamps `pageSize` to 100 — affects every paginated endpoint
across all five modules.

## Dependency Direction

Expected `Api → Application → Domain`; not compiler-enforceable (all five modules are single
projects) but the discipline holds informally — `Controllers/` call only services/`Mediator`, never
`Entities`/`Data`/DbContext directly.

The "no module references another module's internals" rule is **verified across all five modules** —
every cross-module dependency reaches only the target's `Contracts` seam, with no reference the other
direction and no cycle. The full list of the five compliant cross-module edges (importer, consumer,
purpose) is in [dependency-graph.md § Cross-Module Boundary Violations](dependency-graph.md#cross-module-boundary-violations-backend-only);
the project-reference diagram is in [§ Circular References](dependency-graph.md#circular-references).

## Key Design Patterns

- **Mediator** via vendor `Light.Mediator`, with two pipeline behaviors registered in
  `StarterKit.WebApi/ConfigureExtensions.cs` (outermost first): `LoggingBehaviour` (logs request type
  + elapsed time only, never bodies) then `ValidationBehaviour` (FluentValidation).
- **Result pattern** via vendor `Light.Contracts.Result`/`Result<T>`/`PagedResult<T>` instead of
  throwing for expected failures.
- **Domain events** via `BaseEntity.AddDomainEvent`, meant to dispatch inside a module's
  `SaveChangesAsync` (`DispatchDomainEventsExtensions`, `src/Persistence`). Not currently wired that
  way anywhere: `Identity` and `Approval` publish their events manually via `IPublisher` from a
  handler/service; `Organization` and `LeaveManagement` don't use domain events at all
  (`LeaveManagement` reconciles status against `Approval` on every read instead, since `Approval`'s
  events aren't exposed via its `Contracts`). The `Identity` case is tracked as
  [known-debt.md](../known-debt.md) P6.
- **Extension-method DI registration** — each feature area exposes `Add<Feature>`/`Use<Feature>`
  rather than a monolithic startup class.
- **CQRS at the controller boundary, three shapes.** Every controller action across all five modules
  binds a `Contracts` DTO and dispatches an `internal` mediator command/query. What the handler does
  differs: `Identity`/`Notifications` forward to a service class (the half-migration,
  [known-debt.md](../known-debt.md) D1); `Organization`/`LeaveManagement` hold the DbContext logic
  directly; `Approval` splits by audience — write-path handlers forward to `IApprovalService` (it
  must be DI-reachable cross-module), read-path handlers hold their logic directly. See each module
  doc for detail.
- **Audience-split controllers + real-time push** in `Notifications` — an admin controller (explicit
  permissions) and a self-service controller (permission-less, hard-scoped via `ICurrentUser`) over
  one table, plus a push-only SignalR hub at `/signalr-hub`.

## Shared Kernel / Common Building Blocks

- **`src/Shared`** (leaf) — `ICurrentUser`/`IDateTime`, `CurrentUserBase` (claims-driven, incl. an
  `EmployeeId` accessor backing the employee-login link), `Status` value object, `PageQuery`/`IPage`/
  `SearchQuery`, `BaseDto`/`BaseDto<TId>`, `AffectedRowsResult`, entity wrappers, the mediator
  pipeline behaviors, permission authorization (`SuperUserPolicy`, `AccessControl`, internal
  `PolicyProvider`/`AuthorizationHandler`), constants (`ClaimTypeConstants` incl.
  `EmployeeId = "employee_id"`, `CronTimeConstants`), `ReflectionHelper`.
- **`src/Infrastructure`** (→ `Shared`) — CORS, health checks, Mapster config, module/endpoint base
  classes (`AppModule`, `AppModuleEndpoint`), API controller base classes + `BasicAuthAttribute`,
  static Serilog bootstrap (`AppLogging`).
- **`src/Persistence`** (→ `Shared`) — EF Core provider wiring (`DbContextExtensions`, `DbProvider`,
  `DbConnectionNames` — `Identity`/`Catalog`/`Organization`/`Approval`/`LeaveManagement`, all
  aliasing `Default`), `BaseDbContext` (`OnModelCreating` sealed; derived contexts override
  `ConfigureModel`), `TrackingExtensions`/`DispatchDomainEventsExtensions`, paging/result helpers,
  migration-time runtime support.
- **`<Module>.Contracts`** — the per-module seam. None is a true leaf (every one references `Shared`);
  `Shared` is the only true leaf. Full inventory per module doc.

## Module/Route Boundaries

`StarterKit.WebApi` is the only host — it wires all five modules via `app.MapEndpoints(...)` and maps
the `Notifications` SignalR hub at `/signalr-hub`. The full route/permission inventory per module
lives in each module doc's § Public Contract. Cross-module boundary compliance is covered in
Dependency Direction above.

## Known Architectural Risks / Debt

Module-specific debt lives in each module doc's § Notable Conventions and the canonical
[known-debt.md](../known-debt.md). Only genuinely cross-cutting risks not owned by a single module
are tracked here:

| Finding | Severity | Notes |
|---|---|---|
| `Persistence/MigrationSupport/MigrationsExtensions.AddMigrationsServices` registers mediator handlers via `Assembly.GetExecutingAssembly()` (the `Persistence` assembly) | Medium | Won't pick up handlers in module assemblies — revisit once a module with domain-event handlers relies on migration-time dispatch. |
| `ApiControllerBase`/`VersionedApiController` (`src/Infrastructure/Endpoints/`) duplicate an identical `_mediator` lazy property | Low | Likely unavoidable — they derive from two different vendor base classes. |

## Notes

<!-- manual: content below this line is human-authored and must be preserved verbatim during sync -->

---
_Last synced: 2026-09-07_
