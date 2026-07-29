# Architecture

> Template. Populated incrementally as modules/client apps are actually built, only when explicitly requested (see [skills/review-architecture.md](skills/review-architecture.md), [agents/architecture-reviewer.md](agents/architecture-reviewer.md), [agents/nextjs-architect.md](agents/nextjs-architect.md)). The *intended* shape is described below (agreed for this template); this file records **verified** facts about what's actually been built — do not treat the intended shape as already implemented until checked.

## Scope of This Document

This file aggregates **verified** architectural facts only, split into Backend, Clients, and Integration. Never assume the intended shape below has been fully realized in code — check before asserting.

## Intended Shape (agreed template design, not yet necessarily built)

- **Backend** (`src/`): ASP.NET Core, C#, Modular Monolith. `src/Modules/<ModuleName>/{Domain,Application,Infrastructure,Api}` per module, plus a shared/building-blocks project and a composition-root host project.
- **Clients** (`clients/`): one or more frontend apps, each in `clients/<app-name>/`. The primary one (`clients/web/`) is Next.js (App Router), TypeScript/React. Do not assume there's only one client app.
- **Integration**: backend MVC controllers are API-only (JSON, no Razor views); each client app is a consumer of that API, never a UI rendered by the backend.

## Backend — Layering (per module)

| Module | Domain | Application | Infrastructure | Api | Notes |
|---|---|---|---|---|---|
| _none yet_ | | | | | No `src/Modules/*` exist yet — only the pre-module shared kernel (`src/Shared`, `src/Infrastructure`) has been built. |

## Backend — Dependency Direction

> Verified via project references, not inferred from folder names. Expected direction: `Api → Application → Domain`; `Infrastructure → Application` (implements interfaces) and `→ Domain`. Modules must not reference another module's `Domain`/`Infrastructure` directly.

- Verified today: `src/Infrastructure/Infrastructure.csproj` → `src/Shared/Shared.csproj` (single `ProjectReference`). `Shared` has no project references (leaf). `tests/Framework.Tests/Framework.Tests.csproj` → both `Shared` and `Infrastructure`.
- The intended per-module direction (`Api → Application → Domain`, `Infrastructure → Application`/`Domain`) is unverified — no module exists yet to check it against.

## Backend — Shared Kernel / Building Blocks

> E.g. base entities, shared abstractions, common EF Core conventions used across modules — only list what's confirmed in code, with file references.

- **`src/Shared`** (leaf project, no dependencies) — base entity/DTO wrappers over vendor `Light.Domain` (`Entities/AuditableEntity.cs`, `Entities/DomainEvent.cs`), `ICurrentUser`/`IDateTime` abstractions, `Status` value object, `PageQuery`/`IPage`, `ValidationBehaviour<,>` (FluentValidation pipeline behavior for the vendor mediator), permission-authorization building blocks under `Authorization/` (`SuperUserPolicy`, `AccessControl`, `CurrentUserBase`, internal `PolicyProvider`/`AuthorizationHandler`), `Constants/` (`ClaimTypeConstants`, `CronTimeConstants`), `Utilities/ReflectionHelper`.
- **`src/Infrastructure`** (depends on `Shared`) — EF Core provider wiring (`Database/DbContextExtensions.cs`, `DbProvider.cs`, `BaseDbContext.cs`, `SqliteDbContextExtensions.cs`), audit/soft-delete tracking (`Database/TrackingExtensions.cs`) and domain-event dispatch (`Database/DispatchDomainEventsExtensions.cs`) meant to be called from each module's own `DbContext.SaveChangesAsync`, `Cors/`, `HealthChecks/`, `Mappings/MapsterSettings.cs`, `Modularity/AppModule.cs` + `AppModuleEndpoint.cs` (module registration base classes), `Endpoints/` (API controller base classes, `BasicAuthAttribute`), `AppLogging.cs` (static Serilog bootstrap logger).
- No module has been scaffolded yet to confirm how it will actually consume these building blocks.

## Backend — Data Access

> One `DbContext` per module is the intended default. Do not assume a single repo-wide context.

| Module | DbContext | Provider | Notes |
|---|---|---|---|
| _none yet_ | | | No module `DbContext` exists yet. `src/Infrastructure/Database/BaseDbContext.cs` is the intended base class (applies the Sqlite `DateTimeOffset` fix in `OnModelCreating`); `DbContextExtensions.AddConfiguredDbContext<TContext>`/`GetDbProvider` support `InMemory`/`PostgreSQL`/`MSSQL`/`Sqlite` via an `IConfiguration["DbProvider"]` switch. |

## Backend — API Surface

> REST/JSON, API-only controllers. Versioning strategy per `.claude/DEVELOPMENT.md` once observed.

| Module | Route prefix | Versioning | Notes |
|---|---|---|---|
| _unknown_ | | | |

## Clients — Index

> One row per app actually found under `clients/`. Do not enumerate until an `analyze-frontend`/`analyze-client` pass has run.

| App | Path | Purpose | Stack | Status |
|---|---|---|---|---|
| _unknown_ | | | | not yet analyzed |

## Clients — Structure (per app)

> Router style (App vs Pages), data-fetching approach (server components, React Query/SWR, server actions), and state management — only document what's actually observed, per app.

| App | Router | Data fetching | State management | Styling |
|---|---|---|---|---|
| _unknown_ | | | | |

## Clients — Key Areas (per app)

| App | Area/Route | Path | Responsibility | Notes |
|---|---|---|---|---|
| _unknown_ | | | | |

## Integration — API Contract

> How each client consumes the backend API: hand-written fetch calls, a generated typed client (e.g. from OpenAPI), shared DTO types, etc. See [agents/api-contract-reviewer.md](agents/api-contract-reviewer.md).

| App | Client generation strategy | Base URL / env config | Auth flow |
|---|---|---|---|
| _unknown_ | | | |

## Architectural Risks / Debt

> Findings from `review-architecture` runs go here, tagged with date and scope (backend/client app/integration).

- **2026-07-29, backend, `src/Infrastructure/Database/MigrationsExtensions.cs`**: `AddMigrationsServices` registers mediator handlers via `Assembly.GetExecutingAssembly()`, which resolves to the `Infrastructure` assembly itself, not any future module assembly. Once modules with domain-event handlers exist, migration-time event dispatch may silently find no handlers — revisit then.
- **2026-07-29, backend, `src/Infrastructure/Endpoints/ApiControllerBase.cs` + `VersionedApiController.cs`**: both duplicate an identical `_mediator` backing-field + lazy `Mediator` property. Likely unavoidable since they derive from two different vendor base classes (`Light.AspNetCore.Mvc.ApiControllerBase` vs `...VersionedApiController`); flagged in case the vendor library later offers a shared base to consolidate into.

---
_Last updated: 2026-07-29 — backend shared-kernel scope (`src/Shared`, `src/Infrastructure`); no modules/clients built yet._
