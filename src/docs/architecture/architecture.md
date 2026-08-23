# Architecture: Backend

## Layering

Module structure convention (adopted 2026-07-30): every module lives as one or more flat projects directly under `src/` (no `src/Modules/` nesting) — either a single `<Module>` (or `<Module>.Api`, when kept as a deliberate future-microservice-extraction candidate) project internally organized by folder, or, if complex enough, split Clean-Architecture-style into `<Module>.Domain`/`.Application`/`.Infrastructure`/`.Api`. Every module also gets a `<Module>.Contracts` seam project — the only project other modules or the host may reference.

Two modules are built so far, both single-project (not split Domain/Application/Infrastructure/Api). Full internal layering, public contract, and module-specific conventions/risks now live in each module's own doc — this section keeps only a structural summary:

| Module | Structure | Notes |
|---|---|---|
| Identity | Single project (`src/Identity.Api`, deliberately named with the `.Api` suffix) + `src/Identity.Contracts` (seam) | See [modules/Identity.md](modules/Identity.md) for full layering (`Entities/`, `Application/`, `Services/`, `Jwt/`, `Controllers/`), public contract, and conventions. CQRS command and service-class patterns coexist inconsistently for what should be one approach — not yet reconciled. |
| Notifications | Single project (`src/Notifications.Api`, same `.Api`-suffix convention) + `src/Notifications.Contracts` (seam) | See [modules/Notifications.md](modules/Notifications.md) for full layering (`Entities/`, `Services/`, `Controllers/`, `SignalR/`), public contract, and conventions. Controllers split by **audience** (admin vs. self-service) rather than by resource — a pattern distinct from Identity's. |

Below the module layer, `src/Shared` (leaf) and `src/Persistence` (depends on `Shared`) remain the pre-module shared kernel; `src/Infrastructure` (depends on `Shared`) is cross-cutting infrastructure, no longer holding EF Core concerns (moved to `Persistence` in the 2026-07 refactor). `Persistence`'s `QueryableResultExtensions.ToPagedAsync` clamps `pageSize` to a max of 100 (`Math.Min(pageSize, 100)`) — affects any paginated endpoint, including `UserService.SearchAsync` and `NotificationService`'s list queries.

## Dependency Direction

Expected direction `Api → Application → Domain`; not enforceable by the compiler since both modules are single projects — but the discipline holds informally: `Controllers/` in both modules call only into services/`Mediator`, never directly into `Entities`/`Data`/DbContext. The "modules must not reference another module's internals" rule **is now verified with a second module**: `Identity` and `Notifications` don't reference each other at all (`Notifications` uses opaque `fromUserId`/`toUserId` strings, no FK or cross-module service call) — no cross-module boundary violation found. See [dependency-graph.md](dependency-graph.md) for the full project-reference diagram, verified via each `.csproj`'s `ProjectReference` entries.

## Key Design Patterns

- **Mediator pattern** via vendor `Light.Mediator` (`IMediator`/`ISender`/`IPublisher`/`IRequest<T>`/`INotification`), with `ValidationBehaviour<TRequest,TResponse>` as a pipeline behavior running FluentValidation before the handler.
- **Result pattern** via vendor `Light.Contracts.Result`/`Result<T>`/`PagedResult<T>` instead of throwing for expected failure cases.
- **Domain events** via `BaseEntity.AddDomainEvent`/`ClearDomainEvents` (vendor `Light.Domain`), dispatched through `DispatchDomainEventsExtensions.DispatchDomainEvents` (`src/Persistence/Extensions/`) — intended to run inside a module's `SaveChangesAsync`, alongside `TrackingExtensions.AuditEntries` for audit/soft-delete stamping. Not currently wired into `AppIdentityDbContext`: Identity instead publishes its one domain event (`UserCreatedEvent`) manually from a CQRS command handler, bypassing this convention.
- **Extension-method-based DI registration** — each feature area exposes `Add<Feature>`/`Use<Feature>` extension methods on `IServiceCollection`/`IApplicationBuilder` rather than a monolithic startup class.
- **CQRS + traditional service classes coexisting** in `Identity` (one mediator command for user creation vs. `UserService`/`RoleService` for everything else) — an inconsistency, not an intentional dual pattern. See `modules/Identity.md`.
- **Audience-split controllers + real-time push** in `Notifications` — an admin controller (explicit permissions) and a self-service controller (permission-less, hard-scoped via `ICurrentUser`) over the same table, plus a push-only SignalR hub for live delivery. See `modules/Notifications.md`.

## Shared Kernel / Common Building Blocks Used

- `src/Shared` (leaf) — `ICurrentUser`/`IDateTime` abstractions, `CurrentUserBase` (claims-driven), `Status` value object, `PageQuery`/`IPage`, `BaseDto`/`BaseDto<TId>`, `AffectedRowsResult`, entity wrappers (`AuditableEntity`, `AuditableEntity<T>`, `DomainEvent`), permission authorization (`SuperUserPolicy`, `AccessControl`, internal `PolicyProvider`/`AuthorizationHandler`), constants (`ClaimTypeConstants`, `CronTimeConstants`), `Utilities.ReflectionHelper`.
- `src/Infrastructure` (depends on `Shared`) — CORS (`Cors/`), health checks (`HealthChecks/`), Mapster config (`Mappings/MapsterSettings.cs`), module/endpoint base classes (`Modularity/AppModule.cs`, `AppModuleEndpoint.cs`), API controller base classes + `BasicAuthAttribute` (`Endpoints/`), static Serilog bootstrap logger (`AppLogging.cs`).
- `src/Persistence` (depends on `Shared`) — EF Core provider wiring (`DbContextExtensions.cs`, `DbProvider.cs`, `DbConnectionNames.cs`), DbContext base class (`Context/BaseDbContext.cs` — `OnModelCreating` sealed; derived contexts override `ConfigureModel(ModelBuilder)` instead), `TrackingExtensions`/`DispatchDomainEventsExtensions`, generic paging/result helpers (`Extensions/QueryableResultExtensions.cs`), migration-time runtime support (`MigrationSupport/`).
- `src/Identity.Contracts` (depends on `Shared` — not a leaf) and `src/Notifications.Contracts` (depends on `Shared` — a true leaf) — the per-module `Contracts` seam. Full inventory of each in `modules/Identity.md` / `modules/Notifications.md`.

## Module/Route Boundaries

Two modules now exist, and the "modules must not reference another module's internals" rule holds between them: `Identity` and `Notifications` don't reference each other. Full route/permission inventory per module lives in `modules/Identity.md` / `modules/Notifications.md` § Public Contract. `StarterKit.WebApi` is the only host wiring both up via `app.MapEndpoints(...)`; `Notifications` additionally maps a SignalR hub at `/signalr-hub`.

## Known Architectural Risks / Debt

Module-specific risks/debt now live in each module's own doc (see `modules/Identity.md` / `modules/Notifications.md` § Notable Conventions) — only genuinely cross-cutting risks (not owned by a single module) are tracked here:

| Finding | Severity | Notes |
|---|---|---|
| `Persistence/MigrationSupport/MigrationsExtensions.AddMigrationsServices` registers mediator handlers via `Assembly.GetExecutingAssembly()` (the `Persistence` assembly) | Medium | Won't pick up handlers defined in module assemblies like `Identity.Api`/`Notifications.Api` — revisit once modules with domain-event handlers rely on this. |
| `ApiControllerBase`/`VersionedApiController` (`src/Infrastructure/Endpoints/`) duplicate an identical `_mediator` lazy-property | Low | Likely unavoidable — they derive from two different vendor base classes. |

## Notes

<!-- manual: content below this line is human-authored and must be preserved verbatim during sync -->

---
_Last synced: 2026-08-19_
