# Architecture

> Template. Populated incrementally as modules/client apps are actually built, only when explicitly requested (see [skills/review-architecture.md](skills/review-architecture.md), [agents/architecture-reviewer.md](agents/architecture-reviewer.md), [agents/nextjs-architect.md](agents/nextjs-architect.md)). The *intended* shape is described below (agreed for this template); this file records **verified** facts about what's actually been built — do not treat the intended shape as already implemented until checked.

## Scope of This Document

This file aggregates **verified** architectural facts only, split into Backend, Clients, and Integration. Never assume the intended shape below has been fully realized in code — check before asserting.

## Intended Shape (agreed template design, not yet necessarily built)

- **Backend** (`src/`): ASP.NET Core, C#, Modular Monolith. Flat projects directly under `src/` (no `src/Modules/` nesting) — see [Backend — Module Structure Convention](#backend--module-structure-convention) below — plus a shared/building-blocks project(s) and a composition-root host project.
- **Clients** (`clients/`): one or more frontend apps, each in `clients/<app-name>/`. The primary one (`clients/web/`) is Next.js (App Router), TypeScript/React. Do not assume there's only one client app.
- **Integration**: backend MVC controllers are API-only (JSON, no Razor views); each client app is a consumer of that API, never a UI rendered by the backend.

## Backend — Module Structure Convention

> Adopted 2026-07-30, replacing the earlier (never-built) `src/Modules/<Name>/{Domain,Application,Infrastructure,Api}` idea. See [reviews/2026-07-30-backend-project-analysis.md](reviews/2026-07-30-backend-project-analysis.md) for the analysis that prompted this.

- Every module lives as one or more **flat projects directly under `src/`** — never nested under a `src/Modules/` folder.
- **Single-project module**: for a module simple enough not to need assembly-level isolation between layers, use one project internally organized by folder (`Entities/` or `Domain/`, `Application/`, `Data/` or `Infrastructure/`, `Controllers/` or `Api/`). Name it `<Module>` by default — or `<Module>.Api` (e.g. `src/Identity.Api`) when the module is deliberately kept as a candidate for future extraction into an independent microservice: the `.Api` suffix then already previews the name that standalone service's own API project would have, minimizing rename churn if/when that extraction happens. Pick the naming style per module on purpose (default vs. extraction candidate), not by accident.
- **Split module**: for a module complex/large enough to justify assembly-level layering (team ownership, independent testability, avoiding a god-project), split it Clean-Architecture-style into `src/<Module>.Domain`, `src/<Module>.Application`, `src/<Module>.Infrastructure`, `src/<Module>.Api`.
- **Every module — single or split — also gets a `<Module>.Contracts` project.** This is the only project other modules or the composition-root host are allowed to reference from outside the module; it exposes the module's public DTOs and service interfaces (e.g. `src/Identity.Contracts`). Referencing anything else that belongs to another module (its `Domain`/`Infrastructure`/`Api` project, or its single-project internals) is a boundary violation.
- Choosing single-project vs. split is a per-module judgment call (entity count, expected growth, team size, testing isolation needs) — there is no blanket rule forcing every module into the same shape.
- **Current state**: `Identity` is the only module built so far, and it is a single project — deliberately named `Identity.Api` (not just `Identity`) since it's an anticipated candidate for future extraction into an independent identity service. Its own internal layering is informal (folder-based, not enforced by the compiler) and has not yet been cleaned up to consistently follow the folder convention above (e.g. it currently mixes CQRS commands and traditional service classes for what should be one pattern — flagged in the review).

## Backend — Layering (per module)

| Module | Structure | Domain | Application | Infrastructure | Api | Notes |
|---|---|---|---|---|---|---|
| Identity | Single project (`src/Identity.Api` — name deliberately kept, see convention above) + `src/Identity.Contracts` (seam) | `Entities/` (folder: `User`, `Role`, `RoleClaim`, `UserClaim`, `UserLogin`, `UserRole`, `UserToken`, `JwtToken`) | `Application/Users/Commands` (one CQRS command) + `Services/` (`UserService`, `RoleService` — traditional service classes); `Jwt/` (token issuing/validation) | `Data/` (`AppIdentityDbContext`, table/schema constants) | `Controllers/` (`UserController`, `RoleController`, `TokenController`) | CQRS command and service-class patterns coexist inconsistently for what should be one approach — see [reviews/2026-07-30-backend-project-analysis.md](reviews/2026-07-30-backend-project-analysis.md) (finding D1). |

## Backend — Dependency Direction

> Verified via project references, not inferred from folder names. Expected direction: `Api → Application → Domain`; `Infrastructure → Application` (implements interfaces) and `→ Domain`. Modules must not reference another module's internals directly — only its `Contracts` project.

- Verified 2026-07-30 (acyclic): `Shared` is a leaf. `Infrastructure` → `Shared`. `Persistence` → `Shared`. `Identity.Contracts` is a leaf (no `ProjectReference`s — confirms it works as a real seam). `Identity.Api` → `Identity.Contracts`, `Infrastructure`, `Persistence`. `StarterKit.WebApi` (composition-root host) → `Identity.Api`, `Infrastructure`, `Shared`. `tests/Framework.Tests` → `Shared`, `Infrastructure`, `Persistence` only — **no test project references `Identity.Api`/`Identity.Contracts` yet**.
- The "modules must not reference another module's internals" rule is unverified in practice — `Identity` is the only module so far, so there's no second module to test the boundary against.
- Identity's internal layering is informal (see table above): `Controllers/` do call only into services/`Mediator`/`ITokenService` (not directly into `Entities`/`Data`/`AppIdentityDbContext`) — that discipline holds today, but nothing enforces it since it's one assembly.

## Backend — Shared Kernel / Building Blocks

> E.g. base entities, shared abstractions, common EF Core conventions used across modules — only list what's confirmed in code, with file references.

- **`src/Shared`** (leaf project, no dependencies) — base entity/DTO wrappers over vendor `Light.Domain` (`Entities/AuditableEntity.cs`, `Entities/DomainEvent.cs`), `ICurrentUser`/`IDateTime` abstractions, `Status` value object, `PageQuery`/`IPage`, `ValidationBehaviour<,>` (FluentValidation pipeline behavior for the vendor mediator), permission-authorization building blocks under `Authorization/` (`SuperUserPolicy`, `AccessControl`, `CurrentUserBase`, internal `PolicyProvider`/`AuthorizationHandler`), `Constants/` (`ClaimTypeConstants`, `CronTimeConstants`), `Utilities/ReflectionHelper`.
- **`src/Infrastructure`** (depends on `Shared`) — CORS (`Cors/`), health checks (`HealthChecks/`), Mapster config (`Mappings/MapsterSettings.cs`), module/endpoint base classes (`Modularity/AppModule.cs` + `AppModuleEndpoint.cs`), API controller base classes + `BasicAuthAttribute` (`Endpoints/`), static Serilog bootstrap logger (`AppLogging.cs`). **Note (corrected 2026-07-30)**: EF Core provider wiring, the DbContext base class, and tracking/domain-event dispatch previously described here now live in `src/Persistence` (moved during the refactor) — do not look for `Infrastructure/Database/*` anymore.
- **`src/Persistence`** (depends on `Shared`) — EF Core provider wiring (`DbContextExtensions.cs`, `DbProvider.cs`, `DbConnectionNames.cs`), DbContext base class (`Context/BaseDbContext.cs` — applies the Sqlite `DateTimeOffset` fix in `OnModelCreating`), audit/soft-delete tracking (`Extensions/TrackingExtensions.cs`) and domain-event dispatch (`Extensions/DispatchDomainEventsExtensions.cs`) meant to be called from each module's own `DbContext.SaveChangesAsync`, generic paging/result helpers (`Extensions/QueryableResultExtensions.cs`), migration-time runtime support (`Migrations/MigrationsExtensions.cs`, `MigratorCurrentUser.cs` — not to be confused with the unrelated top-level `src/Migrations/` design-time EF projects; a rename of this folder is proposed in the review to reduce that confusion), shared schema/connection-name constants (`Schemas.cs`, `DbConnectionNames.cs`).
- **`src/Identity.Contracts`** (leaf project) — first real example of the per-module `<Module>.Contracts` seam: public DTOs (`UserDto`, `RoleDto`, `TokenDto`, etc.), request types, and service interfaces (`IUserService`, `IRoleService`, `IServiceClaimService` — the last currently unimplemented/unregistered). Confirmed as a true leaf (no `ProjectReference`s), which is what makes it usable as a cross-module seam once a second module exists.
- Only one module (`Identity`) has been built so far, so cross-module consumption of a `Contracts` project is unverified in practice.

## Backend — Data Access

> One `DbContext` per module is the intended default. Do not assume a single repo-wide context.

| Module | DbContext | Provider | Notes |
|---|---|---|---|
| Identity | `AppIdentityDbContext` (`src/Identity.Api/Data/AppIdentityDbContext.cs`) | Configured via `Persistence.DbContextExtensions.AddConfiguredDbContext`/`GetDbProvider` (`InMemory`/`PostgreSQL`/`MSSQL`/`Sqlite` via `IConfiguration["DbProvider"]`) | Extends `Microsoft.AspNetCore.Identity.EntityFrameworkCore.IdentityDbContext<...>` directly, so it **cannot** also extend `Persistence/Context/BaseDbContext.cs` (single inheritance) — it re-applies the Sqlite `DateTimeOffset` fix manually instead, a known/accepted exception. Soft-delete is currently passed as `enableSoftDelete: false` to `TrackingExtensions.AuditEntries`, even though `User` implements `ISoftDelete` and `TokenService` checks `user.Deleted != null` — flagged as a likely bug in the 2026-07-30 review (finding D2). `JwtTokens` table has no index on `Token` or `(UserId, RefreshToken)`, a hot-path performance risk (finding P1). |

## Backend — API Surface

> REST/JSON, API-only controllers. Versioning strategy per `.claude/DEVELOPMENT.md` once observed.

| Module | Route prefix | Versioning | Notes |
|---|---|---|---|
| Identity | Unverified — not yet confirmed against actual `[Route]` attributes | Unverified | Controllers: `UserController`, `RoleController`, `TokenController` (`src/Identity.Api/Controllers/`). Route prefixes/versioning not yet checked in this pass — verify before documenting further. |

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

- **2026-07-29, backend, `src/Persistence/Migrations/MigrationsExtensions.cs`** (moved from `src/Infrastructure/Database/` during the refactor): `AddMigrationsServices` registers mediator handlers via `Assembly.GetExecutingAssembly()`, which resolves to the `Persistence` assembly itself, not any module assembly. Once modules with domain-event handlers exist, migration-time event dispatch may silently find no handlers — revisit then.
- **2026-07-29, backend, `src/Infrastructure/Endpoints/ApiControllerBase.cs` + `VersionedApiController.cs`**: both duplicate an identical `_mediator` backing-field + lazy `Mediator` property. Likely unavoidable since they derive from two different vendor base classes (`Light.AspNetCore.Mvc.ApiControllerBase` vs `...VersionedApiController`); flagged in case the vendor library later offers a shared base to consolidate into.
- **2026-07-30, backend, module structure resolved**: the earlier open question of "what does a module actually look like" is now answered — see [Backend — Module Structure Convention](#backend--module-structure-convention). `Identity` predates the convention and doesn't fully match it yet (misnamed `Identity.Api`, informal internal layering); treat as a pending cleanup, not a second convention.
- **2026-07-30, backend, full findings**: a four-agent pass (dependency/architecture/naming/EF Core) produced a prioritized list of correctness, naming, and dependency-hygiene issues in `Identity`/`Persistence` — see [reviews/2026-07-30-backend-project-analysis.md](reviews/2026-07-30-backend-project-analysis.md) for the complete list (not duplicated here to avoid drift between two copies).

---
_Last updated: 2026-07-30 — module structure convention adopted; backend scope now includes `src/Shared`, `src/Infrastructure`, `src/Persistence`, `src/Identity.Contracts`, `src/Identity.Api`, `src/StarterKit.WebApi`; still no `clients/` built._
