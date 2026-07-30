# Repository Overview

> Template. Populated incrementally, only when analysis is explicitly requested (see [workflows/analyze-solution.md](workflows/analyze-solution.md), [skills/analyze-frontend.md](skills/analyze-frontend.md), [skills/analyze-client.md](skills/analyze-client.md), [workflows/analyze-folder.md](workflows/analyze-folder.md)). Do not fill this in speculatively.

## Summary

- **Type**: Starter template monorepo — C#/.NET backend (Modular Monolith) + one or more frontend clients.
- **Backend**: `src/` — ASP.NET Core Web API, API-only MVC controllers.
- **Clients**: `clients/<app-name>/` — one or more apps; the primary one is Next.js, TypeScript/React.
- **Status**: First backend module (`Identity`) and composition-root host (`StarterKit.WebApi`) built; `clients/` not started yet — verify current state before describing further additions as built.

## Backend Modules

> Modules are flat projects directly under `src/` (no `src/Modules/` nesting) — see [ARCHITECTURE.md § Backend — Module Structure Convention](ARCHITECTURE.md#backend--module-structure-convention). One entry per module actually built.

| Module | Path | Responsibility | Status |
|---|---|---|---|
| Identity | `src/Identity.Api/Identity.Api.csproj` + `src/Identity.Contracts/Identity.Contracts.csproj` | Users, roles, claims, JWT auth/token issuance. Single-project module (Entities/Data/Application/Services/Jwt/Controllers all in `Identity.Api`) + a `Contracts` seam project (DTOs, `IUserService`/`IRoleService`/`IServiceClaimService`). Kept the `.Api` name deliberately — anticipated candidate for future extraction into an independent identity service. | Built, but internal layering is still informal and CQRS/service-class patterns coexist inconsistently. See [reviews/2026-07-30-backend-project-analysis.md](reviews/2026-07-30-backend-project-analysis.md) for the full list of pending cleanup items. No test coverage yet from `tests/Framework.Tests`. |

## Backend Key Projects

> Populated per-project as `analyze-project` runs are performed (e.g. shared/building-blocks, composition-root host). Do not assume completeness.

| Project | Path | Responsibility | Depends on |
|---|---|---|---|
| Shared | `src/Shared/Shared.csproj` | Shared kernel: base entity/DTO/value-object wrappers around the vendor `Light.Domain` types, `ICurrentUser`/`IDateTime` abstractions, permission-based authorization building blocks (`SuperUserPolicy`, `AccessControl`, `CurrentUserBase`, `AuthorizationHandler`), FluentValidation pipeline behavior, constants. | (none — leaf project) |
| Infrastructure | `src/Infrastructure/Infrastructure.csproj` | Cross-cutting infrastructure: CORS, health checks, Serilog bootstrap logging (`AppLogging`), Mapster config, module/endpoint base classes (`AppModule`, `AppModuleEndpoint`), API controller base classes, Basic Auth attribute. **EF Core/DbContext concerns moved out to `Persistence` (below) during the 2026-07 refactor** — this row previously (incorrectly) listed them here. | Shared |
| Persistence | `src/Persistence/Persistence.csproj` | EF Core provider configuration (`DbContextExtensions`/`DbProvider`, Sqlite `DateTimeOffset` workaround), DbContext base class (`Context/BaseDbContext`), audit/soft-delete tracking + domain-event dispatch for `DbContext.SaveChanges`, generic paging/result helpers, migration-time runtime support (`Migrations/` — design-time EF projects live separately under top-level `src/Migrations/`, excluded from this analysis pass). | Shared |
| Identity.Contracts | `src/Identity.Contracts/Identity.Contracts.csproj` | Public seam for the Identity module: DTOs, request types, `IUserService`/`IRoleService`/`IServiceClaimService` (the last currently unimplemented). Leaf project — confirmed no `ProjectReference`s. | (none — leaf project) |
| Identity.Api | `src/Identity.Api/Identity.Api.csproj` | The Identity module itself (see Backend Modules table above). | Identity.Contracts, Infrastructure, Persistence |
| StarterKit.WebApi | `src/StarterKit.WebApi/StarterKit.WebApi.csproj` | Composition-root host (`Program.cs`) — the only executable/deployable project. | Identity.Api, Infrastructure, Shared |
| Framework.Tests | `tests/Framework.Tests/Framework.Tests.csproj` | xUnit v3 test project covering `Shared`, `Infrastructure`, `Persistence` only — **no `Identity.Api`/`Identity.Contracts` coverage yet**. | Shared, Infrastructure, Persistence |

## Client Apps

> One entry per app discovered under `clients/`. Do not enumerate until an `analyze-frontend`/`analyze-client` pass has run. There may be more than one — never collapse this table to a single implied "the frontend."

| App | Path | Responsibility | Stack | Status |
|---|---|---|---|---|
| _unknown_ | | | | not yet analyzed |

## Cross-Cutting Concerns

> Only note items actually verified in code (e.g. shared logging, shared base entities, shared EF Core conventions, a shared API client package reused across clients). Do not guess.

- **Base entities**: `StarterKit.Entities.AuditableEntity`/`AuditableEntity<T>` and `DomainEvent` are thin wrappers around vendor `Light.Domain.Entities.*` types (`src/Shared/Entities/`) — modules should depend on these, not the vendor types directly. Note: `Identity`'s entities (`User`, `Role`, etc.) mostly can't use this directly since they must extend ASP.NET Identity's own base types (`IdentityUser`, `IdentityRole`) — a documented exception, not drift.
- **Current user abstraction**: `ICurrentUser` (`src/Shared/ICurrentUser.cs`) with `CurrentUserBase` (`src/Shared/Authorization/CurrentUserBase.cs`) deriving everything from a `ClaimsPrincipal`.
- **Permission-based authorization**: `SuperUserPolicy`/`AccessControl`/`AuthorizationHandler`/`PolicyProvider` (`src/Shared/Authorization/`), built on vendor `Light.AspNetCore.Authorization`.
- **Audit/soft-delete tracking**: `TrackingExtensions.AuditEntries` (`src/Persistence/Extensions/TrackingExtensions.cs` — moved here from `src/Infrastructure/Database/` during the 2026-07 refactor) — call before `SaveChanges` to stamp `Created`/`LastModified`/`*By` fields and apply soft-delete via `ISoftDelete`. `AppIdentityDbContext` calls this today but passes `enableSoftDelete: false` even though `User` implements `ISoftDelete` and `TokenService` checks `Deleted != null` — flagged as a likely bug, see [reviews/2026-07-30-backend-project-analysis.md](reviews/2026-07-30-backend-project-analysis.md).
- **Domain event dispatch**: `DispatchDomainEventsExtensions.DispatchDomainEvents` (`src/Persistence/Extensions/`) publishes queued `BaseEvent`s via `Light.Mediator.IPublisher` and clears them — same "call it from `SaveChanges`" pattern as tracking. Not currently called from `AppIdentityDbContext`; Identity instead publishes its one domain event (`UserCreatedEvent`) manually from a CQRS command handler, bypassing this convention.
- **DB provider abstraction**: `DbContextExtensions.GetDbProvider`/`AddConfiguredDbContext`/`ConfigureDatabase` (`src/Persistence/`) switch between InMemory/PostgreSQL/MSSQL/Sqlite based on an `IConfiguration["DbProvider"]` value — used today by `AppIdentityDbContext`.
- **Bootstrap logging**: `AppLogging` (`src/Infrastructure/AppLogging.cs`) is a static Serilog logger (console + rolling file) for pre-host/startup logging (`Information`/`Warning` helpers), separate from per-request `ILogger<T>` DI.

## Known Entry Points

> Backend: hosted API. Clients: each app's entry/routes. Only list ones confirmed to exist.

- **Backend**: `src/StarterKit.WebApi/Program.cs` — the composition-root host, referencing `Identity.Api`, `Infrastructure`, `Shared`.

## Open Questions / Gaps

- No `clients/` app yet.
- Module-structure convention is now decided (see [ARCHITECTURE.md](ARCHITECTURE.md#backend--module-structure-convention)), but `Identity` — the only module built — predates it and doesn't fully conform yet (naming, internal layering discipline). Treat as pending cleanup, not a second convention to reconcile.
- Cross-module boundary enforcement (via `<Module>.Contracts`) is unverified in practice — only one module exists so far.
- `MigrationsExtensions.AddMigrationsServices` registers mediator handlers via `Assembly.GetExecutingAssembly()` (now the `Persistence` assembly) — worth re-checking once real module assemblies with domain-event handlers exist, since it won't pick those up automatically.
- Several correctness/naming/dependency-hygiene findings from the 2026-07-30 four-agent review are still open — see [reviews/2026-07-30-backend-project-analysis.md](reviews/2026-07-30-backend-project-analysis.md) for the full prioritized list (not duplicated here to avoid two copies drifting apart).

---
_Last updated: 2026-07-30 — module structure convention adopted; scope now covers `src/Shared`, `src/Infrastructure`, `src/Persistence`, `src/Identity.Contracts`, `src/Identity.Api`, `src/StarterKit.WebApi`, `tests/Framework.Tests`; still no `clients/`._
