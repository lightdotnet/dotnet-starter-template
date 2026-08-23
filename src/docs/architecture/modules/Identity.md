# Module Overview: Identity

## Purpose

Owns users, roles, claims, and JWT-based authentication/session management: user CRUD and search, role/claim assignment, login/refresh-token issuance and validation, session listing/revocation, and a permission catalog (`PermissionsController` → `GET permissions`) that other modules' permission providers plug into via the same vendor contract.

## Internal Layering

Identity is a **single-project module** (not split Domain/Application/Infrastructure/Api) — the template's 4-row project table doesn't apply as-is; adapted to the module's actual two projects:

| Project | Responsibility | Notes |
|---|---|---|
| `Identity.Contracts` | DTOs (`UserDto`, `RoleDto`, `TokenDto`, etc.), request types (`SearchUserQuery` — `SearchValue` with `[StringLength(256)]`), service interfaces (`IUserService` — incl. `SearchAsync`, `IRoleService`, `IServiceClaimService` — unimplemented/unregistered), permission catalog (`Authorization/IdentityPermissions` — `Group = "identity"` const plus nested `Roles`/`Users` static classes, e.g. `identity.roles.view`; `Authorization/IdentityPermissionProvider : IPermissionDefinitionProvider`). The module's only seam project — but not a leaf, see Dependencies. |
| `Identity.Api` | Single project organized by folder: `Entities/` (`User`, `Role`, `RoleClaim`, `UserClaim`, `UserLogin`, `UserRole`, `UserToken`, `UserSession`), `Data/` (`IdentityDbContext`, renamed from `AppIdentityDbContext`), `Application/Users/Commands` (one CQRS command, `CreateUser`) and `Application/Users/EventHandlers` (`UserCreatedEventHandler`, see Notable Conventions), `Services/` (`UserService`, `RoleService`), `Jwt/` (`AuthenticationService`/`IAuthenticationService`, `UserSessionService`/`IUserSessionService`, `JwtTokenIssuer`, `JwtSigningService` — internal, wraps `IOptions<JwtOptions>`, `JwtHelper` — refresh-token generation + claim reading), `Controllers/` (`UserController`, `RoleController`, `TokenController`, `UserProfileController`, `PermissionsController`). | `.Api` suffix kept deliberately — anticipated candidate for future extraction into an independent identity service, not a fresh deviation from convention. |

## Public Contract

- `UserController` — `GET user` (unbounded list) **and** `GET user/search` (paginated, `SearchUserQuery`) coexist for the same read use case — organic drift, not a resolved decision (see Notable Conventions).
- `RoleController`, `TokenController` — role and token management; `TokenController` is routed at `api/v{version:apiVersion}/auth` (`auth/token/get`, `auth/token/refresh`), not a `token`-prefixed route. Exact route/verb inventory for `RoleController` not re-verified in this pass (see module code directly for the full list).
- `UserProfileController` — explicitly overrides its route to `api/v{version:apiVersion}/user_profile` (deliberate readability choice). Scopes every action to the caller via `ICurrentUser.UserId`: `GET` (own profile, also checks token validity via `ICurrentUser.SessionId`/`IUserSessionService.IsTokenValidAsync` + `Active` status), `GET token/list` (own sessions), `PUT token/revoke` (revoke own session).
- `PermissionsController` — `GET permissions`, returns `IPermissionDefinitionProvider.Define()` (the module's own permission catalog).

All controllers rely on `VersionedApiController`'s default `[controller]`-token route convention except `TokenController` and `UserProfileController` (explicit overrides, above).

## Data Access

`IdentityDbContext` (`src/Identity.Api/Data/IdentityDbContext.cs`) extends ASP.NET Identity's `IdentityDbContext<...>` directly — can't also extend `Persistence/Context/BaseDbContext.cs` (single inheritance), so it re-applies the Sqlite `DateTimeOffset` fix manually. Configured via `Persistence.DbContextExtensions.AddConfiguredDbContext`/`GetDbProvider` (`InMemory`/`PostgreSQL`/`MSSQL`/`Sqlite`, selected via `IConfiguration["DbProvider"]`; default in `appsettings.json` is `MSSQL`, pointing at a local `(localdb)\mssqllocaldb` instance). `User` has an index on `Created`, `UserSessions` an index on `UserId`. Soft-delete is passed as `enableSoftDelete: false` despite `User` implementing `ISoftDelete` — see Notable Conventions. MSSQL currently has a single baseline migration (`src/Migrations/MSSQL/Identity/20260801104131_CreateIdentitySchema.cs`); Sqlite/PostgreSQL each have an earlier baseline plus one incremental `AddUserCreatedIndex` migration on top of it.

## Dependencies

| Depends on | Type | Why |
|---|---|---|
| `Shared` | project (`Identity.Contracts → Shared`) | Base abstractions; also the sole reason `Identity.Contracts` references anything — reaches `Shared`'s transitive `Lightsoft.AspNetCore.Authorization` package for `IdentityPermissionProvider` (see Notable Conventions — undeclared transitive dependency). |
| `Infrastructure` | project (`Identity.Api → Infrastructure`) | `VersionedApiController`, `AppModule`/`AppModuleEndpoint` base classes. |
| `Persistence` | project (`Identity.Api → Persistence`) | `BaseDbContext`-adjacent helpers, `AddConfiguredDbContext`, audit/paging extensions. |
| `Notifications.Contracts` | project (`Identity.Api → Notifications.Contracts`) | The module's only dependency on another business module. `Application/Users/EventHandlers/UserCreatedEventHandler.cs` takes an `IMailService` (from `Notifications.Contracts.Services`) and calls `SendFromSystemAsync(...)` to send a welcome email when a user is created; wrapped in try/catch, a send failure only logs (`ILogger<UserCreatedEventHandler>`) and does not fail the request. Consumed strictly through `Notifications.Contracts` — the module's `Contracts` seam — not `Notifications.Api` itself, so this is compliant with the "only reference another module's `Contracts`" rule, not a boundary violation. See `../dependency-graph.md` for the project-reference-level detail and `Notifications.md` for `IMailService`/`MailService` themselves. |
| Vendor `Lightsoft.ActiveDirectory` | package | Referenced; usage not verified in this pass. |
| Vendor `Lightsoft.SharedKernel` | package | `Light.Domain` base types for entities. |
| `Microsoft.AspNetCore.Identity.EntityFrameworkCore` | package | `IdentityUser`/`IdentityRole`/`IdentityDbContext<...>` base types the module's entities/DbContext extend. |
| Vendor `Lightsoft.Result` (`Identity.Contracts`) | package | `Result`/`Result<T>` return types for `IUserService`/`IRoleService`/`IServiceClaimService`. |

## Depended On By

`StarterKit.WebApi` (composition-root host) and `Identity.Tests` (`Identity.Api.csproj` grants `InternalsVisibleTo` so the test project can reach `internal` JWT orchestration classes directly, across `Extensions/`, `Jwt/`, `Entities/`, `Services/`, `Controllers/`). No other business module references `Identity.Api`/`Identity.Contracts` — confirmed `Notifications` does not (opaque `fromUserId`/`toUserId` strings, no cross-module call). The reverse does exist (`Identity.Api → Notifications.Contracts`, see Dependencies above) — the boundary discipline is one-directional, not mutual isolation. Client-side integration is not re-inspected as part of backend-only syncs — see `../../../../clients/admin/docs/` for the admin client's actual usage.

## Notable Conventions

- **CQRS + traditional service classes coexist** for what should be one consistent approach — one mediator command (`Application/Users/Commands`) for user creation vs. `UserService`/`RoleService` for everything else. Tracked as open debt (`../../known-debt.md`, D1); not yet reconciled.
- `ClaimTypeConstants.TokenId` is `"jti"`, not `"tid"` — `"tid"` collides with `JwtSecurityTokenHandler`'s legacy `DefaultInboundClaimTypeMap`, which silently remaps it to a tenant-id claim URI and would break session-id lookup.
- `IdentityPermissionProvider` (`Identity.Contracts`) uses vendor `Light.AspNetCore.Authorization` types (`IPermissionDefinitionProvider`, `PermissionDefinition`) without `Identity.Contracts.csproj` declaring `Lightsoft.AspNetCore.Authorization` as a direct `PackageReference` — it rides in transitively via the `ProjectReference` to `Shared`. Same pattern as `Identity.Api`'s use of `Light.EntityFrameworkCore.Extensions.WhereIf` (`UserService.SearchAsync`, supplied transitively via `Persistence`) — avoid repeating this in future modules; declare packages a project actually uses directly.
- `GET user` (unbounded) and `GET user/search` (paginated) coexist on `UserController` for the same read use case — organic drift, not yet a resolved decision (`../../known-debt.md`, D4).
- `IdentityDbContext` passes `enableSoftDelete: false` to `TrackingExtensions.AuditEntries` despite `User` implementing `ISoftDelete`, and `AuthenticationService` checks `user.Deleted != null` — an audit-trail-loss risk (`../../known-debt.md`, D2), but currently dead code in practice since `AuthenticationService` short-circuits on `user is null` before reaching that check, so it is not a login-bypass risk.
- `IServiceClaimService` has no registered implementation (commented out in `DependencyInjection.cs`); `IdentityClaimQueryExtensions.CheckUserHasClaimAsync` is dead code; the repo-wide `DispatchDomainEvents` convention (`Persistence`) is never called from `IdentityDbContext.SaveChanges[Async]` — Identity instead publishes its one domain event (`UserCreatedEvent`) manually from the CQRS command handler, bypassing that convention. All tracked in `../../known-debt.md` (P4, P5, P6).
- `Identity.Api.csproj` declares `<InternalsVisibleTo Include="Identity.Tests" />` so `AuthenticationService`/`UserSessionService`/`JwtTokenIssuer`/`JwtSigningService` (all `internal`) can be unit-tested directly.
- `UserCreatedEventHandler` (`Application/Users/EventHandlers/`, an `INotificationHandler<UserCreatedEvent>`) sends a welcome email via `Notifications.Contracts.Services.IMailService` (`SendFromSystemAsync`) when `notification.Email` is non-empty, generating a hardcoded HTML body inline (`GenerateWelcomeEmailBody` — includes a placeholder password string, not the user's actual password). A send failure is caught and logged, not surfaced to the caller — user creation itself cannot fail because of a mail-send error. This is the module's only real cross-module `Contracts` consumption (see Dependencies).

## Notes

<!-- manual: content below this line is human-authored and must be preserved verbatim during sync -->

---
_Last synced: 2026-08-19_
