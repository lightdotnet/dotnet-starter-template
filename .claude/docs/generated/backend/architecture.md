# Architecture: Backend

## Layering

No `src/Modules/*` exist yet, so per-module Domain/Application/Infrastructure/Api layering is unverified. What exists today is a pre-module shared kernel split into two projects: `src/Shared` (leaf, no dependencies) and `src/Infrastructure` (depends on `Shared`).

## Dependency Direction

Verified via `ProjectReference` entries in each `.csproj`:

```text
Infrastructure -> Shared
Framework.Tests -> Shared
Framework.Tests -> Infrastructure
```

`Shared` has no outgoing project references. The intended per-module direction (`Api → Application → Domain`; `Infrastructure → Application`/`Domain`) is unverified — no module exists to check it against.

## Key Design Patterns

- **Mediator pattern** via vendor `Light.Mediator` (`IMediator`/`ISender`/`IPublisher`/`IRequest<T>`/`INotification`), with `ValidationBehaviour<TRequest,TResponse>` as a pipeline behavior running FluentValidation before the handler.
- **Result pattern** via vendor `Light.Contracts.Result`/`Result<T>`/`PagedResult<T>` instead of throwing for expected failure cases (not-found, validation, etc. have dedicated factory methods).
- **Domain events** via `BaseEntity.AddDomainEvent`/`ClearDomainEvents` (vendor `Light.Domain`) queued on entities and dispatched through `DispatchDomainEventsExtensions.DispatchDomainEvents` (intended to run inside a module's `SaveChangesAsync` override, alongside `TrackingExtensions.AuditEntries` for audit/soft-delete stamping).
- **Extension-method-based DI registration** — each feature area exposes `Add<Feature>`/`Use<Feature>` extension methods on `IServiceCollection`/`IApplicationBuilder` rather than a monolithic startup class.

## Shared Kernel / Common Building Blocks Used

- `src/Shared`: `ICurrentUser`/`IDateTime` abstractions, `CurrentUserBase` (claims-driven), `Status` value object, `PageQuery`/`IPage`, `BaseDto`/`BaseDto<TId>`, `AffectedRowsResult`, entity wrappers (`AuditableEntity`, `AuditableEntity<T>`, `DomainEvent`), permission authorization (`SuperUserPolicy`, `AccessControl`, internal `PolicyProvider`/`AuthorizationHandler`), constants (`ClaimTypeConstants`, `CronTimeConstants`), `Utilities.ReflectionHelper`.
- `src/Infrastructure`: EF Core provider wiring (`DbContextExtensions`, `DbProvider`, `BaseDbContext`, `SqliteDbContextExtensions`), `TrackingExtensions`/`DispatchDomainEventsExtensions`, `Cors`, `HealthChecks`, `Mappings.MapsterSettings`, `Modularity.AppModule`/`AppModuleEndpoint`, `Endpoints.ApiControllerBase`/`VersionedApiController`/`BasicAuthAttribute`, `AppLogging` (static bootstrap logger), `Services.DateTimeService`/`ServerCurrentUser`.

## Module/Route Boundaries

No modules exist yet, so module boundary rules are unverified in practice. No routes/controllers are wired to a host yet either (`ApiControllerBase`/`VersionedApiController` are unused base classes today).

## Known Architectural Risks / Debt

| Finding | Severity | Notes |
|---|---|---|
| `MigrationsExtensions.AddMigrationsServices` registers mediator handlers from `Assembly.GetExecutingAssembly()` (the `Infrastructure` assembly) | Medium | Won't pick up handlers defined in future module assemblies — revisit once modules with domain-event handlers exist. |
| `ApiControllerBase`/`VersionedApiController` duplicate an identical `_mediator` lazy-property | Low | Likely unavoidable — they derive from two different vendor base classes (`Light.AspNetCore.Mvc.ApiControllerBase` / `...VersionedApiController`). |

## Notes

<!-- manual: content below this line is human-authored and must be preserved verbatim during sync -->

---
_Generated: 2026-07-29 — scope: Backend — see .claude/CLAUDE.md for update rules._
