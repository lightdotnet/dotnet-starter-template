# Architecture: Backend

## Layering

Module structure convention (adopted 2026-07-30): every module lives as one or more flat projects directly under `src/` (no `src/Modules/` nesting) — either a single `<Module>` (or `<Module>.Api`, when kept as a deliberate future-microservice-extraction candidate) project internally organized by folder, or, if complex enough, split Clean-Architecture-style into `<Module>.Domain`/`.Application`/`.Infrastructure`/`.Api`. Every module also gets a `<Module>.Contracts` seam project — the only project other modules or the host may reference.

`Identity` is the only module built so far, and it is a single project (`src/Identity.Api`, deliberately named with the `.Api` suffix) plus `src/Identity.Contracts`:

| Module | Structure | Domain | Application | Infrastructure | Api | Notes |
|---|---|---|---|---|---|---|
| Identity | Single project (`src/Identity.Api`) + `src/Identity.Contracts` (seam) | `Entities/` (`User`, `Role`, `RoleClaim`, `UserClaim`, `UserLogin`, `UserRole`, `UserToken`, `UserSession`) | `Application/Users/Commands` (one CQRS command) + `Services/` (`UserService` — incl. new `SearchAsync(SearchUserQuery, pageNumber, pageSize)` paginated search, `RoleService`); `Jwt/` (`AuthenticationService`/`IAuthenticationService` — login/refresh-token orchestration, now depends on `IDateTime` not `TimeProvider`; `UserSessionService`/`IUserSessionService` — session persistence/listing/revocation, also `IDateTime`-based; `JwtTokenIssuer` — claims collection + delegates signing to `JwtSigningService`; `JwtSigningService` — new internal class wrapping `IOptions<JwtOptions>`, exposes `Generate(claims, expiresAt)`/`Validate(token, expired)`, consolidating the `issuer`/`secretKey`/`roleClaimType` params that used to be threaded loosely through the whole chain; `JwtHelper` — now only `GenerateRefreshToken()` and `ReadClaims(jwt)`) | `Data/` (`IdentityDbContext`, renamed from `AppIdentityDbContext`; table/schema constants) | `Controllers/` (`UserController` — incl. new `GET user/search`, `RoleController`, `TokenController`, `UserProfileController`) | CQRS command and service-class patterns coexist inconsistently for what should be one approach (finding D1 in `reviews/2026-07-30-backend-project-analysis.md`). `ClaimTypeConstants.TokenId` was renamed `"tid"`→`"jti"` (the old value collided with `JwtSecurityTokenHandler`'s legacy `DefaultInboundClaimTypeMap`, silently remapping it to a tenant-id claim URI and breaking session-id lookup). `Identity.Api.csproj` now declares `<InternalsVisibleTo Include="Identity.Tests" />` so `AuthenticationService`/`UserSessionService`/`JwtTokenIssuer`/`JwtSigningService` (all `internal`) can be unit-tested directly. |

Below the module layer, `src/Shared` (leaf) and `src/Persistence` (depends on `Shared`) remain the pre-module shared kernel; `src/Infrastructure` (depends on `Shared`) is cross-cutting infrastructure, no longer holding EF Core concerns (moved to `Persistence` in the 2026-07 refactor). `Persistence`'s `QueryableResultExtensions.ToPagedAsync` now clamps `pageSize` to a max of 100 (`Math.Min(pageSize, 100)`, previously unbounded) — affects any current/future paginated endpoint, including `UserService.SearchAsync`.

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
Identity.Tests -> Identity.Api
Identity.Tests -> Shared
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
- `src/Identity.Contracts` (leaf) — first real example of the per-module `Contracts` seam: DTOs (`UserDto`, `RoleDto`, `TokenDto`, etc.), request types (incl. new `SearchUserQuery` — `SearchValue` with `[StringLength(256)]`), service interfaces (`IUserService` — incl. new `SearchAsync`, `IRoleService`, `IServiceClaimService` — the last currently unimplemented/unregistered).

## Module/Route Boundaries

`Identity` is the only module built, so cross-module boundary rules are unverified against a second module. Its API surface: `UserController`, `RoleController`, `TokenController` rely on `VersionedApiController`'s default `[controller]`-token route convention (exact resulting template unverified — vendor base class not inspected). `UserController` now also exposes `GET user/search` (paginated, `Identity.Contracts/SearchUserQuery.cs` — `SearchValue` with `[StringLength(256)]`) alongside the pre-existing unbounded `GET user` — a still-open decision on the relationship between the two (finding D4 in `reviews/2026-07-30-backend-project-analysis.md`). `UserProfileController` explicitly overrides its route to `api/v{version:apiVersion}/user_profile` (a deliberate readability choice, not an inconsistency) and scopes every action to the caller via `ICurrentUser.UserId` — `GET` (own profile, also checks token validity via `ICurrentUser.SessionId`/`IUserSessionService.IsTokenValidAsync` + `Active` status), `GET token/list` (own sessions), `PUT token/revoke` (revoke own session). `StarterKit.WebApi` is the only host wiring these up via `app.MapEndpoints(...)`.

## Known Architectural Risks / Debt

| Finding | Severity | Notes |
|---|---|---|
| `Persistence/MigrationSupport/MigrationsExtensions.AddMigrationsServices` registers mediator handlers via `Assembly.GetExecutingAssembly()` (the `Persistence` assembly) | Medium | Won't pick up handlers defined in module assemblies like `Identity.Api` — revisit once modules with domain-event handlers rely on this. |
| `ApiControllerBase`/`VersionedApiController` (`src/Infrastructure/Endpoints/`) duplicate an identical `_mediator` lazy-property | Low | Likely unavoidable — they derive from two different vendor base classes. |
| `IdentityDbContext` passes `enableSoftDelete: false` to `TrackingExtensions.AuditEntries` despite `User` implementing `ISoftDelete`, and `AuthenticationService` checks `user.Deleted != null` | Medium | Likely bug — soft-deleted users may not be filtered as expected. Actually dead code today (`AuthenticationService` short-circuits on `user is null` before reaching that check), so it's an audit-trail-loss risk, not a login-bypass risk. See `reviews/2026-07-30-backend-project-analysis.md` (finding D2). |
| CQRS command + traditional service classes coexist inconsistently in `Identity` | Low/Medium | One pattern should be chosen (finding D1). User has said this will be implemented separately. |
| `GET user` (unbounded list) and `GET user/search` (paginated) now coexist on `UserController` for the same read use case | Low | Organic drift, not yet a decision — see finding D4 in the same review. |
| `IServiceClaimService` has no registered implementation; `IdentityClaimQueryExtensions.CheckUserHasClaimAsync` is dead code; `DispatchDomainEvents` convention is never called from `IdentityDbContext.SaveChanges[Async]` | Low | Findings P4/P5/P6 — see `reviews/2026-07-30-backend-project-analysis.md` for detail (not duplicated here). |

**Resolved this session** (kept out of the table above, see the review file's changelog for detail): the former "`JwtTokens` table has no index on `Token`" hot-path risk (P1) was resolved — session-validity lookups now go by `UserSession` primary key via the session id read from `ICurrentUser.SessionId` (claim `ClaimTypeConstants.TokenId`, `"jti"`), trusting `builder.MapControllers().RequireAuthorization()` (`src/Infrastructure/InfrastructureModule.cs`) and the JWT Bearer scheme to have already verified the token's signature before the controller runs — no `Token`-column scan needed.

## Notes

<!-- manual: content below this line is human-authored and must be preserved verbatim during sync -->

---
_Generated: 2026-08-02 (resynced — `tests/Identity.Tests` added, `Identity.Api` grants it `InternalsVisibleTo`) — scope: Backend — see .claude/CLAUDE.md for update rules._
