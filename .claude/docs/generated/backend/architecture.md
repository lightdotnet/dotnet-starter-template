# Architecture: Backend

## Layering

Module structure convention (adopted 2026-07-30): every module lives as one or more flat projects directly under `src/` (no `src/Modules/` nesting) — either a single `<Module>` (or `<Module>.Api`, when kept as a deliberate future-microservice-extraction candidate) project internally organized by folder, or, if complex enough, split Clean-Architecture-style into `<Module>.Domain`/`.Application`/`.Infrastructure`/`.Api`. Every module also gets a `<Module>.Contracts` seam project — the only project other modules or the host may reference.

`Identity` is the only module built so far, and it is a single project (`src/Identity.Api`, deliberately named with the `.Api` suffix) plus `src/Identity.Contracts`:

| Module | Structure | Domain | Application | Infrastructure | Api | Notes |
|---|---|---|---|---|---|---|
| Identity | Single project (`src/Identity.Api`) + `src/Identity.Contracts` (seam) | `Entities/` (`User`, `Role`, `RoleClaim`, `UserClaim`, `UserLogin`, `UserRole`, `UserToken`, `JwtToken`) | `Application/Users/Commands` (one CQRS command) + `Services/` (`UserService`, `RoleService`); `Jwt/` (`AuthenticationService`/`IAuthenticationService` — login/refresh-token orchestration; `UserSessionService`/`IUserSessionService` — session persistence/listing/revocation; `JwtTokenIssuer` — claims + JWT issuance) | `Data/` (`AppIdentityDbContext`, table/schema constants) | `Controllers/` (`UserController`, `RoleController`, `TokenController`, `UserProfileController`) | CQRS command and service-class patterns coexist inconsistently for what should be one approach (finding D1 in `reviews/2026-07-30-backend-project-analysis.md`). `Jwt/` was renamed from `TokenService`→`AuthenticationService`, `ITokenService`→`IAuthenticationService`, `JwtTokenManager`→`UserSessionService` (plus a new `IUserSessionService` interface and a new `JwtTokenIssuer` split out for claims/token issuance). |

Below the module layer, `src/Shared` (leaf) and `src/Persistence` (depends on `Shared`) remain the pre-module shared kernel; `src/Infrastructure` (depends on `Shared`) is cross-cutting infrastructure, no longer holding EF Core concerns (moved to `Persistence` in the 2026-07 refactor).

## Dependency Direction

Verified via `ProjectReference` entries in each `.csproj`:

```text
Infrastructure -> Shared
Persistence -> Shared
Identity.Contracts (leaf — no ProjectReferences)
Identity.Api -> Identity.Contracts
Identity.Api -> Infrastructure
Identity.Api -> Persistence
StarterKit.WebApi -> Identity.Api
StarterKit.WebApi -> Infrastructure
StarterKit.WebApi -> Shared
Framework.Tests -> Shared
Framework.Tests -> Infrastructure
Framework.Tests -> Persistence
```

Expected direction `Api → Application → Domain`; `Infrastructure → Application`/`Domain` is not enforceable by the compiler since `Identity` is a single project — but the discipline holds informally: `Controllers/` call only into services/`Mediator`/`IAuthenticationService`/`IUserSessionService`/`IUserService`, never directly into `Entities`/`Data`/`AppIdentityDbContext` (confirmed including for the new `UserProfileController`). The "modules must not reference another module's internals" rule is unverified in practice — `Identity` is still the only module, so there's no second module to test the boundary against.

## Key Design Patterns

- **Mediator pattern** via vendor `Light.Mediator` (`IMediator`/`ISender`/`IPublisher`/`IRequest<T>`/`INotification`), with `ValidationBehaviour<TRequest,TResponse>` as a pipeline behavior running FluentValidation before the handler.
- **Result pattern** via vendor `Light.Contracts.Result`/`Result<T>`/`PagedResult<T>` instead of throwing for expected failure cases.
- **Domain events** via `BaseEntity.AddDomainEvent`/`ClearDomainEvents` (vendor `Light.Domain`), dispatched through `DispatchDomainEventsExtensions.DispatchDomainEvents` (`src/Persistence/Extensions/`) — intended to run inside a module's `SaveChangesAsync`, alongside `TrackingExtensions.AuditEntries` for audit/soft-delete stamping. Not currently wired into `AppIdentityDbContext`: Identity instead publishes its one domain event (`UserCreatedEvent`) manually from a CQRS command handler, bypassing this convention.
- **Extension-method-based DI registration** — each feature area exposes `Add<Feature>`/`Use<Feature>` extension methods on `IServiceCollection`/`IApplicationBuilder` rather than a monolithic startup class.
- **CQRS + traditional service classes coexisting** in `Identity` (one mediator command for user creation vs. `UserService`/`RoleService` for everything else) — an inconsistency, not an intentional dual pattern (finding D1).

## Shared Kernel / Common Building Blocks Used

- `src/Shared` (leaf) — `ICurrentUser`/`IDateTime` abstractions, `CurrentUserBase` (claims-driven), `Status` value object, `PageQuery`/`IPage`, `BaseDto`/`BaseDto<TId>`, `AffectedRowsResult`, entity wrappers (`AuditableEntity`, `AuditableEntity<T>`, `DomainEvent`), permission authorization (`SuperUserPolicy`, `AccessControl`, internal `PolicyProvider`/`AuthorizationHandler`), constants (`ClaimTypeConstants`, `CronTimeConstants`), `Utilities.ReflectionHelper`.
- `src/Infrastructure` (depends on `Shared`) — CORS (`Cors/`), health checks (`HealthChecks/`), Mapster config (`Mappings/MapsterSettings.cs`), module/endpoint base classes (`Modularity/AppModule.cs`, `AppModuleEndpoint.cs`), API controller base classes + `BasicAuthAttribute` (`Endpoints/`), static Serilog bootstrap logger (`AppLogging.cs`).
- `src/Persistence` (depends on `Shared`) — EF Core provider wiring (`DbContextExtensions.cs`, `DbProvider.cs`, `DbConnectionNames.cs`), DbContext base class (`Context/BaseDbContext.cs` — `OnModelCreating` sealed; derived contexts override `ConfigureModel(ModelBuilder)` instead), `TrackingExtensions`/`DispatchDomainEventsExtensions`, generic paging/result helpers (`Extensions/QueryableResultExtensions.cs`), migration-time runtime support (`MigrationSupport/`).
- `src/Identity.Contracts` (leaf) — first real example of the per-module `Contracts` seam: DTOs (`UserDto`, `RoleDto`, `TokenDto`, etc.), request types, service interfaces (`IUserService`, `IRoleService`, `IServiceClaimService` — the last currently unimplemented/unregistered).

## Module/Route Boundaries

`Identity` is the only module built, so cross-module boundary rules are unverified against a second module. Its API surface: `UserController`, `RoleController`, `TokenController` rely on `VersionedApiController`'s default `[controller]`-token route convention (exact resulting template unverified — vendor base class not inspected). `UserProfileController` explicitly overrides its route to `api/v{version:apiVersion}/user_profile` (a deliberate readability choice, not an inconsistency) and scopes every action to the caller via `ICurrentUser.UserId` — `GET` (own profile, also checks token validity + `Active` status), `GET token/list` (own sessions), `PUT token/revoke` (revoke own session). `StarterKit.WebApi` is the only host wiring these up via `app.MapEndpoints(...)`.

## Known Architectural Risks / Debt

| Finding | Severity | Notes |
|---|---|---|
| `Persistence/MigrationSupport/MigrationsExtensions.AddMigrationsServices` registers mediator handlers via `Assembly.GetExecutingAssembly()` (the `Persistence` assembly) | Medium | Won't pick up handlers defined in module assemblies like `Identity.Api` — revisit once modules with domain-event handlers rely on this. |
| `ApiControllerBase`/`VersionedApiController` (`src/Infrastructure/Endpoints/`) duplicate an identical `_mediator` lazy-property | Low | Likely unavoidable — they derive from two different vendor base classes. |
| `AppIdentityDbContext` passes `enableSoftDelete: false` to `TrackingExtensions.AuditEntries` despite `User` implementing `ISoftDelete`, and `AuthenticationService` checks `user.Deleted != null` | Medium | Likely bug — soft-deleted users may not be filtered as expected. See `reviews/2026-07-30-backend-project-analysis.md` (finding D2). |
| `JwtTokens` table has no index on `Token` or `(UserId, RefreshToken)` | Medium (perf) | Hot-path lookup risk (finding P1 in the same review). |
| CQRS command + traditional service classes coexist inconsistently in `Identity` | Low/Medium | One pattern should be chosen (finding D1). |

## Notes

<!-- manual: content below this line is human-authored and must be preserved verbatim during sync -->

---
_Generated: 2026-08-01 — scope: Backend — see .claude/CLAUDE.md for update rules._
