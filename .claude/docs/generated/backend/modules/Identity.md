# Module Overview: Identity

## Purpose

Owns users, roles, claims, and JWT-based authentication/session management: user CRUD and search, role/claim assignment, login/refresh-token issuance and validation, session listing/revocation, and a permission catalog (`PermissionsController` → `GET permissions`) that other modules' permission providers plug into via the same vendor contract.

## Internal Layering

Identity is a **single-project module** (not split Domain/Application/Infrastructure/Api) — the template's 4-row project table doesn't apply as-is; adapted to the module's actual two projects:

| Project | Responsibility | Notes |
|---|---|---|
| `Identity.Contracts` | DTOs (`UserDto`, `RoleDto`, `TokenDto`, etc.), request types (`SearchUserQuery` — `SearchValue` with `[StringLength(256)]`), service interfaces (`IUserService` — incl. `SearchAsync`, `IRoleService`, `IServiceClaimService` — unimplemented/unregistered), permission catalog (`Authorization/IdentityPermissions` — `Group = "identity"` const plus nested `Roles`/`Users` static classes, e.g. `identity.roles.view`; `Authorization/IdentityPermissionProvider : IPermissionDefinitionProvider`). The module's only seam project — but **no longer a leaf**, see Dependencies. |
| `Identity.Api` | Single project organized by folder: `Entities/` (`User`, `Role`, `RoleClaim`, `UserClaim`, `UserLogin`, `UserRole`, `UserToken`, `UserSession`), `Data/` (`IdentityDbContext`, renamed from `AppIdentityDbContext`), `Application/Users/Commands` (one CQRS command), `Services/` (`UserService`, `RoleService`), `Jwt/` (`AuthenticationService`/`IAuthenticationService`, `UserSessionService`/`IUserSessionService`, `JwtTokenIssuer`, `JwtSigningService` — internal, wraps `IOptions<JwtOptions>`, `JwtHelper` — refresh-token generation + claim reading), `Controllers/` (`UserController`, `RoleController`, `TokenController`, `UserProfileController`, `PermissionsController`). | `.Api` suffix kept deliberately — anticipated candidate for future extraction into an independent identity service (not a fresh deviation from convention). |

## Public Contract

- `UserController` — `GET user` (unbounded list) **and** `GET user/search` (paginated, `SearchUserQuery`) coexist for the same read use case — organic drift, not a resolved decision (see Notable Conventions).
- `RoleController`, `TokenController` — role and token management; exact route/verb inventory not re-verified in this pass (see module code directly for the full list).
- `UserProfileController` — explicitly overrides its route to `api/v{version:apiVersion}/user_profile` (deliberate readability choice). Scopes every action to the caller via `ICurrentUser.UserId`: `GET` (own profile, also checks token validity via `ICurrentUser.SessionId`/`IUserSessionService.IsTokenValidAsync` + `Active` status), `GET token/list` (own sessions), `PUT token/revoke` (revoke own session).
- `PermissionsController` — `GET permissions`, returns `IPermissionDefinitionProvider.Define()` (the module's own permission catalog).

All controllers rely on `VersionedApiController`'s default `[controller]`-token route convention except `UserProfileController` (explicit override, above).

## Data Access

`IdentityDbContext` (`src/Identity.Api/Data/IdentityDbContext.cs`) extends ASP.NET Identity's `IdentityDbContext<...>` directly — can't also extend `Persistence/Context/BaseDbContext.cs` (single inheritance), so it re-applies the Sqlite `DateTimeOffset` fix manually. Configured via `Persistence.DbContextExtensions.AddConfiguredDbContext`/`GetDbProvider` (`InMemory`/`PostgreSQL`/`MSSQL`/`Sqlite`, selected via `IConfiguration["DbProvider"]`; default in `appsettings.json` is `MSSQL`, pointing at a local `(localdb)\mssqllocaldb` instance). `User` has an index on `Created`, `UserSessions` an index on `UserId`. Soft-delete is passed as `enableSoftDelete: false` despite `User` implementing `ISoftDelete` — see Notable Conventions. MSSQL migrations were reset to a fresh baseline this session (`src/Migrations/MSSQL/Identity/20260801104131_CreateIdentitySchema.cs`); Sqlite/PostgreSQL got an incremental `AddUserCreatedIndex` migration on top of their existing baseline.

## Dependencies

| Depends on | Type | Why |
|---|---|---|
| `Shared` | project (`Identity.Contracts → Shared`) | Base abstractions; also the sole reason `Identity.Contracts` references anything — reaches `Shared`'s transitive `Lightsoft.AspNetCore.Authorization` package for `IdentityPermissionProvider` (see Notable Conventions — undeclared transitive dependency). |
| `Infrastructure` | project (`Identity.Api → Infrastructure`) | `VersionedApiController`, `AppModule`/`AppModuleEndpoint` base classes. |
| `Persistence` | project (`Identity.Api → Persistence`) | `BaseDbContext`-adjacent helpers, `AddConfiguredDbContext`, audit/paging extensions. |
| Vendor `Lightsoft.ActiveDirectory` | package | Referenced; usage not verified in this pass. |
| Vendor `Lightsoft.SharedKernel` | package | `Light.Domain` base types for entities. |
| `Microsoft.AspNetCore.Identity.EntityFrameworkCore` | package | `IdentityUser`/`IdentityRole`/`IdentityDbContext<...>` base types the module's entities/DbContext extend. |
| Vendor `Lightsoft.Result` (`Identity.Contracts`) | package | `Result`/`Result<T>` return types for `IUserService`/`IRoleService`/`IServiceClaimService`. |

## Depended On By

`StarterKit.WebApi` (composition-root host) and `Identity.Tests` (98 tests: `Extensions/`, `Jwt/`, `Entities/`, `Services/`, `Controllers/` — `Identity.Api.csproj` grants `InternalsVisibleTo` so the test project can reach `internal` JWT orchestration classes). No other business module references `Identity.Api`/`Identity.Contracts` — confirmed `Notifications` does not (opaque `fromUserId`/`toUserId` strings, no cross-module call). Client-side integration was not re-inspected as part of backend-only syncs — see `.claude/docs/generated/clients/admin/` for the admin client's actual usage.

## Notable Conventions

- **CQRS + traditional service classes coexist** for what should be one consistent approach — one mediator command (`Application/Users/Commands`) for user creation vs. `UserService`/`RoleService` for everything else. Not yet reconciled; user has said this will be addressed separately.
- `ClaimTypeConstants.TokenId` was renamed `"tid"` → `"jti"` — the old value collided with `JwtSecurityTokenHandler`'s legacy `DefaultInboundClaimTypeMap`, silently remapping it to a tenant-id claim URI and breaking session-id lookup.
- `IdentityPermissionProvider` (`Identity.Contracts`) uses vendor `Light.AspNetCore.Authorization` types (`IPermissionDefinitionProvider`, `PermissionDefinition`) without `Identity.Contracts.csproj` declaring `Lightsoft.AspNetCore.Authorization` as a direct `PackageReference` — it rides in transitively via the `ProjectReference` to `Shared`. Same pattern as `Identity.Api`'s use of `Light.EntityFrameworkCore.Extensions.WhereIf` (`UserService.SearchAsync`, supplied transitively via `Persistence`) — avoid repeating this in future modules; declare packages a project actually uses directly.
- `GET user` (unbounded) and `GET user/search` (paginated) coexist on `UserController` for the same read use case — organic drift, not yet a resolved decision.
- `IdentityDbContext` passes `enableSoftDelete: false` to `TrackingExtensions.AuditEntries` despite `User` implementing `ISoftDelete`, and `AuthenticationService` checks `user.Deleted != null` — likely bug (audit-trail-loss risk), but currently dead code in practice since `AuthenticationService` short-circuits on `user is null` before reaching that check, so it is not a login-bypass risk.
- `IServiceClaimService` has no registered implementation; `IdentityClaimQueryExtensions.CheckUserHasClaimAsync` is dead code; the repo-wide `DispatchDomainEvents` convention (`Persistence`) is never called from `IdentityDbContext.SaveChanges[Async]` — Identity instead publishes its one domain event (`UserCreatedEvent`) manually from the CQRS command handler, bypassing that convention.
- `Identity.Api.csproj` declares `<InternalsVisibleTo Include="Identity.Tests" />` so `AuthenticationService`/`UserSessionService`/`JwtTokenIssuer`/`JwtSigningService` (all `internal`) can be unit-tested directly.

## Notes

<!-- manual: content below this line is human-authored and must be preserved verbatim during sync -->

---
_Generated: 2026-08-03 — scope: module "Identity" (split out of the solution-wide backend docs into its own per-module file) — see .claude/CLAUDE.md for update rules._
