# Repository Overview

> Template. Populated incrementally, only when analysis is explicitly requested (see [workflows/analyze-solution.md](workflows/analyze-solution.md), [skills/analyze-frontend.md](skills/analyze-frontend.md), [skills/analyze-client.md](skills/analyze-client.md), [workflows/analyze-folder.md](workflows/analyze-folder.md)). Do not fill this in speculatively.

## Summary

- **Type**: Starter template monorepo — C#/.NET backend (Modular Monolith) + one or more frontend clients.
- **Backend**: `src/` — ASP.NET Core Web API, API-only MVC controllers.
- **Clients**: `clients/<app-name>/` — one or more apps; the primary one is Next.js, TypeScript/React.
- **Status**: Template scaffold stage — verify current state before describing it as built; do not assume the folders above exist until checked.

## Backend Modules

> One entry per module discovered under `src/Modules/`. Do not enumerate until an `analyze-module`/`analyze-solution` pass has actually run for that module.

| Module | Path | Responsibility | Status |
|---|---|---|---|
| _none yet_ | `src/Modules/` | — | verified: `src/Modules/` does not exist yet — only the shared kernel below has been built so far |

## Backend Key Projects

> Populated per-project as `analyze-project` runs are performed (e.g. shared/building-blocks, composition-root host). Do not assume completeness.

| Project | Path | Responsibility | Depends on |
|---|---|---|---|
| Shared | `src/Shared/Shared.csproj` | Shared kernel: base entity/DTO/value-object wrappers around the vendor `Light.Domain` types, `ICurrentUser`/`IDateTime` abstractions, permission-based authorization building blocks (`SuperUserPolicy`, `AccessControl`, `CurrentUserBase`, `AuthorizationHandler`), FluentValidation pipeline behavior, constants. No composition-root/host yet. | (none — leaf project) |
| Infrastructure | `src/Infrastructure/Infrastructure.csproj` | Cross-cutting infrastructure: EF Core provider configuration (`DbContextExtensions`/`DbProvider`, Sqlite `DateTimeOffset` workaround), audit/soft-delete tracking + domain-event dispatch for `DbContext.SaveChanges`, CORS, health checks, Serilog bootstrap logging (`AppLogging`), Mapster config, module/endpoint base classes (`AppModule`, `AppModuleEndpoint`), API controller base classes, Basic Auth attribute. | Shared |
| Framework.Tests | `tests/Framework.Tests/Framework.Tests.csproj` | xUnit v3 test project covering `Shared` and `Infrastructure`; folder layout mirrors the target project (`Shared/`, `Infrastructure/`), each with its own `TestSupport/` if needed. | Shared, Infrastructure |

## Client Apps

> One entry per app discovered under `clients/`. Do not enumerate until an `analyze-frontend`/`analyze-client` pass has run. There may be more than one — never collapse this table to a single implied "the frontend."

| App | Path | Responsibility | Stack | Status |
|---|---|---|---|---|
| _unknown_ | | | | not yet analyzed |

## Cross-Cutting Concerns

> Only note items actually verified in code (e.g. shared logging, shared base entities, shared EF Core conventions, a shared API client package reused across clients). Do not guess.

- **Base entities**: `StarterKit.Entities.AuditableEntity`/`AuditableEntity<T>` and `DomainEvent` are thin wrappers around vendor `Light.Domain.Entities.*` types (`src/Shared/Entities/`) — modules should depend on these, not the vendor types directly.
- **Current user abstraction**: `ICurrentUser` (`src/Shared/ICurrentUser.cs`) with `CurrentUserBase` (`src/Shared/Authorization/CurrentUserBase.cs`) deriving everything from a `ClaimsPrincipal`. Two implementations exist: `ServerCurrentUser` (HTTP-context-backed, `src/Infrastructure/Services/`) and `MigratorCurrentUser` (synthetic principal for EF Core migrations, `src/Infrastructure/Database/`).
- **Permission-based authorization**: `SuperUserPolicy`/`AccessControl`/`AuthorizationHandler`/`PolicyProvider` (`src/Shared/Authorization/`), built on vendor `Light.AspNetCore.Authorization`.
- **Audit/soft-delete tracking**: `TrackingExtensions.AuditEntries` (`src/Infrastructure/Database/`) — call before `SaveChanges` to stamp `Created`/`LastModified`/`*By` fields and apply soft-delete via `ISoftDelete`. Modules will need to wire this into their own `DbContext.SaveChangesAsync` override.
- **Domain event dispatch**: `DispatchDomainEventsExtensions.DispatchDomainEvents` (`src/Infrastructure/Database/`) publishes queued `BaseEvent`s via `Light.Mediator.IPublisher` and clears them — same "call it from `SaveChanges`" pattern as tracking.
- **DB provider abstraction**: `DbContextExtensions.GetDbProvider`/`AddConfiguredDbContext`/`ConfigureDatabase` (`src/Infrastructure/Database/`) switch between InMemory/PostgreSQL/MSSQL/Sqlite based on an `IConfiguration["DbProvider"]` value — no module `DbContext` uses this yet.
- **Bootstrap logging**: `AppLogging` (`src/Infrastructure/AppLogging.cs`) is a static Serilog logger (console + rolling file) for pre-host/startup logging (`Information`/`Warning` helpers), separate from per-request `ILogger<T>` DI.

## Known Entry Points

> Backend: hosted API. Clients: each app's entry/routes. Only list ones confirmed to exist.

- _none yet_ — no composition-root/host project exists under `src/` yet (no `Program.cs`); `Infrastructure.InfrastructureModule.MapEndpoints`/`AddSharedInfrastructure` are ready to be wired into one once it's added.

## Open Questions / Gaps

- No composition-root host project yet (no `Program.cs`/API entry point) — `InfrastructureModule` exists to be called from one.
- No `src/Modules/*` yet — the one-`DbContext`-per-module convention, module layering, and cross-module boundary rules are all unverified until a first module is scaffolded.
- No `clients/` app yet.
- `MigrationsExtensions.AddMigrationsServices` registers mediator handlers via `Assembly.GetExecutingAssembly()` (the `Infrastructure` assembly) — worth re-checking once real module assemblies with domain-event handlers exist, since it won't pick those up automatically.

---
_Last updated: 2026-07-29 — backend shared-kernel + test project scope (`src/Shared`, `src/Infrastructure`, `tests/Framework.Tests`); no modules/host/clients yet._
