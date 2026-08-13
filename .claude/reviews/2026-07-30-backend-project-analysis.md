# Backend Project Analysis & Refactor Proposals — 2026-07-30

> **Archival.** This file is a point-in-time analysis, kept for context on already-resolved items. The live, current-state list of open backend debt/pending decisions is [docs/KNOWN_DEBT.md](../docs/KNOWN_DEBT.md) — update that file, not this one, when debt is found or resolved.

**Scope**: all backend projects under `src/` on branch `feat-dev/identity-module`, including `Identity.Api` / `Identity.Contracts`. `src/Migrations/**` generally excluded from analysis per request (exceptions noted inline where a change elsewhere forced a mechanical edit there).

This file now tracks **pending items only**. Resolved/implemented items (module-structure convention, N1–N6 naming, P2/P3 Schemas removal, Sqlite/PostgreSQL migration fix, the `IdentityDbContext`/framework name-collision bug, D3's `TokenService`/`JwtTokenManager` rename+split into `AuthenticationService`/`JwtTokenIssuer`/`UserSessionService` which also resolved P7/P8/P9/P11, and P14 — `GetUserTokensAsync`/`RevokeAsync` now wired into `UserProfileController`'s `token/list`/`token/revoke` endpoints) have been removed — see `ARCHITECTURE.md` § Backend — Module Structure Convention and git history for that work. Post-D3, the user further renamed `GenerateTokenByAsync`→`GenerateTokenAsync` and swapped the ad-hoc `TimeNow` seam for `TimeProvider` — see new findings P15/P16 below (the `TimeProvider` swap introduced a DI registration gap and a duplicate clock abstraction).

**2026-08-01 re-review**: six-agent pass (architecture/code-quality/EF Core/security/performance/API design) against the current working tree, covering the `TimeProvider` follow-through plus a new uncommitted feature — paginated user search (`Identity.Contracts/SearchUserQuery.cs`, `IUserService.SearchAsync`/`UserService.SearchAsync`, `UserController` `search` endpoint) — and the accompanying `AuthenticationService`/`UserSessionService` simplification. Found one **security regression** (P17 — introduced by the simplification, not present before) plus a priority escalation on existing P1 (P18), new pagination/search hardening items (P19–P23, P27), minor consistency nits (P24–P26), and a new open decision (D4). D1/D2/P1/P4–P16 unchanged; not re-litigated below except where noted.

**2026-08-01 fixes applied**: P10, P12, P17, P20, P21, P22, P23 (the controller action was already using `[HttpGet("search")]` by the time this pass ran — confirmed resolved, not re-flagged), P24, P25, P26, and P27 have been implemented and are removed from the pending list below — see git history on `feat-dev/identity-module`. **P19**: `entity.HasIndex(x => x.Created)` was added, and a clean `Users.Created` index migration now exists for **all three providers** — Sqlite/PostgreSQL got an incremental `AddUserCreatedIndex` migration; MSSQL needed a full baseline reset instead (see resolved **D5** below) since its migration history was stale in a way that made an incremental `dotnet ef migrations add` produce a broken diff. P19 fully resolved.

**2026-08-01 D5 resolved**: initial attempts to add the MSSQL index surfaced `dotnet ef migrations add` producing a bloated, broken diff for that provider — investigated as D5 ("MSSQL migration history is stale"). That write-up **overstated the problem**: comparing the old migration's `Up()`/`CreateTable` against `git show HEAD` confirmed it already created all the audit columns and the `UserSessions` table correctly — the earlier claim that "a fresh MSSQL database would be missing `UserSessions`, login likely doesn't work end-to-end" was wrong. The actual (narrower) fault was that only `DbContextModelSnapshot.cs` (internally named `ApplicationDbContextModelSnapshot`, the EF bookkeeping file used to diff for the *next* `migrations add`) was stale — out of sync with both the current model and the migration's own, already-correct `.Designer.cs`. Two rounds of hand-patching that one file (adding just the `Created` index) each surfaced a fresh bug — a copy-pasted duplicate index landing on `Roles`, then an index referencing a `Created` property never declared in that same snapshot — confirming hand-patching was the wrong approach. Resolved by deleting the stale `00000000000000_CreateIdentitySchema.cs`/`.Designer.cs`/`DbContextModelSnapshot.cs` and scaffolding fresh via `dotnet ef migrations add CreateIdentitySchema` (per `docs/migrations.md`), producing `20260801104131_CreateIdentitySchema.cs`/`.Designer.cs` + a correctly-named `IdentityDbContextModelSnapshot.cs`. Verified: all 8 tables (including `UserSessions`), all audit columns, `IX_Users_Created`/`IX_UserSessions_UserId` with EF's default naming (matching Sqlite/PostgreSQL), `MSSQL.csproj` builds clean. D5 removed from Decisions needed.

**2026-08-01 P1/P18 resolved**: `UserSessionService.IsTokenValidAsync` now looks up `UserSession` by PK via the session id (`ClaimTypeConstants.TokenId`, read from `ICurrentUser.SessionId` in `UserProfileController.Get()`) instead of scanning the unindexable `Token` column — both removed from the pending list below. The implementation differs slightly from the design originally sketched in P1: no manual full-string equality check against the stored token is needed, because every controller already sits behind `builder.MapControllers().RequireAuthorization()` (`Infrastructure/InfrastructureModule.cs:28`) and the ASP.NET Core JWT Bearer scheme, which verifies the token's *signature* before the action runs — that middleware is the real trust boundary now, not a string comparison inside `IsTokenValidAsync`. P1's optional `CryptographicOperations.FixedTimeEquals` hardening note is accordingly moot (no string comparison left to time-attack).

Implementing this surfaced and fixed a genuine, unrelated bug: `ClaimTypeConstants.TokenId` was `"tid"`, which collides with `JwtSecurityTokenHandler`'s built-in `DefaultInboundClaimTypeMap` — a legacy WS-Federation compatibility table that silently remaps the short claim name `"tid"` to `http://schemas.microsoft.com/identity/claims/tenantid` on every token validation (Microsoft added it for Azure AD tenant-id interop). This broke `ClaimsPrincipalExtensions.GetSessionId()`, which looked up the literal string `"tid"`. Fixed by renaming the constant to `"jti"` (the standard JWT-ID registered claim, not present in that legacy map). A preemptive `P28` was raised for the same legacy-map collision on `ClaimTypeConstants.Email = "email"`, then dropped: `JwtTokenIssuer.GetUserClaimsAsync` never adds an `Email` claim to this module's tokens in the first place (only `UserId`/`UserName`/permission claims/`jti`), so there's nothing for the mapping to remap today — worth re-checking only if an `Email` claim is ever added to the token later.

Separately (not from the original review, but done in the same session): `issuer`/`secretKey`/`roleClaimType` were factored out of the `AuthenticationService` → `IUserSessionService` → `JwtTokenIssuer` → `JwtHelper` parameter chain into one new `JwtSigningService` (`Identity.Api/Jwt/JwtSigningService.cs`, `Generate`/`Validate`); `JwtHelper` now only holds `GenerateRefreshToken()` and `ReadClaims()`.

**2026-08-01 P15/P16 resolved**: P15 turned out to be a false positive — running the app showed `TimeProvider` resolves fine with no explicit registration. Root cause: `Identity.Api/DependencyInjection.cs:23` calls `.AddIdentityCore<User>(...)`, and ASP.NET Core Identity's own `AddIdentityCore` extension registers a default `TimeProvider.System` via `TryAddSingleton` internally (it needs one for lockout timestamps) — so the DI graph was always satisfied, just via an incidental registration rather than an explicit one. P15 removed as not-a-bug. That still left P16's actual complaint (two clock seams in one module) standing, so it was fixed per its own recommendation: `AuthenticationService`/`UserSessionService` now depend on `IDateTime` instead of `TimeProvider` (`dateTime.UtcNow` replacing every `timeProvider.GetUtcNow()` call site); `TimeProvider` no longer appears anywhere under `Identity.Api/Jwt/`. Both P15 and P16 removed from the pending list below.

## Current project graph (verified, acyclic)

```
Shared (leaf)
  ← Infrastructure
  ← Persistence
Identity.Contracts (leaf)
  ← Identity.Api  (also → Infrastructure, Persistence)
       ← StarterKit.WebApi (host, also → Infrastructure, Shared)
```

---

## Decisions needed

### D1. CQRS vs. service classes inside Identity — pick one pattern
`Identity.Api` currently has both: `UserService`/`RoleService` (traditional service classes, used directly by controllers) and a single CQRS command (`Application/Users/Commands/CreateUser.cs`) that just wraps `IUserService.CreateAsync` to publish `UserCreatedEvent`. Every other operation (roles, tokens, other user operations) bypasses the command pattern entirely. This reads as an unfinished migration rather than a deliberate layering. Recommend either:
- Drop the CQRS wrapper — publish `UserCreatedEvent` directly from `UserService.CreateAsync`; or
- Commit to CQRS for all Identity writes and demote services to internal/read-only helpers.

*(User has said this will be implemented separately.)*

### D2. Soft-delete is wired up but never actually happens — audit-trail loss, not a login-bypass risk
`AuthenticationService.GetTokenAsync`/`RefreshTokenAsync` both short-circuit on `user is null` *before* calling `CheckInvalidUser` (`AuthenticationService.cs:24,74`), so a hard-deleted user is already rejected via the null check — `CheckInvalidUser`'s `user.Deleted != null` branch is dead code, but that's incidental, not a security hole.

The actual cost of disabling soft-delete (`IdentityDbContext` calls `AuditEntries(..., enableSoftDelete: false)`):
- `User.Delete()` (`Entities/User.cs:55-65`) anonymizes the row first (nulls `UserName`/`Email`/`PhoneNumber`/names/`PasswordHash`, locks `Status`) — a soft-delete-and-anonymize pattern. But `UserService.DeleteAsync` (`Services/UserService.cs:170-186`) then immediately hard-deletes that same just-anonymized row — the anonymization work is wasted.
- Once the row is gone, anything that stamped `CreatedBy`/`LastModifiedBy`/a `UserId` FK with this user's Id (e.g. `UserSessions.UserId`, audit fields on any future module's entities) becomes an orphaned reference — an audit log showing "created by `<UserId>`" can no longer resolve who that was.
- `User.Deleted`/`DeletedBy` end up permanently `null` — dead schema; a future admin/audit feature filtering `WHERE Deleted IS NOT NULL` would always get zero rows, silently.

Decide: flip `enableSoftDelete: true` in `IdentityDbContext.SaveChanges[Async]` (keeps the anonymized tombstone; `AuthenticationService`'s check becomes the real enforcement point) — or, if hard delete is genuinely intended, remove `ISoftDelete`/the `Deleted`/`DeletedBy` columns/the dead check.

This is a **one-line change**, not new logic: `TrackingExtensions.AuditEntries` (`Persistence/Extensions/TrackingExtensions.cs:19-31`) already does the "who/when" stamping automatically when `enableSoftDelete: true` is passed, and `IdentityDbContext` already passes the right `currentUser.UserId`/`clock.AuditTime` arguments today — just the boolean flag needs flipping (or the cleanup path taken instead).

### D4. `GET user` (unbounded list) vs. `GET user/search` (paginated) now coexist — decide the relationship (new, 2026-08-01)

`UserController` exposes two different "list users" mechanisms for what's functionally the same read use case: the pre-existing `GET user` (`UserController.cs:25-29`, returns a plain unpaginated `IEnumerable<UserDto>`) and the new `GET user/search` (returns a capped, paginated `PagedResult<UserDto>`). Not a breaking change today — both are additive — but it's organic drift rather than a decision: is `GET user` an intentional "admin/all" escape hatch that should stay, or should it be deprecated now that a paginated search exists? Left as-is, a second module will have no precedent to follow for "how do we expose a list endpoint here."

---

## Correctness / performance

| # | Finding | File(s) | Recommendation |
|---|---|---|---|
| P4 | `IServiceClaimService` has no registered implementation — commented out in DI | `Identity.Api/DependencyInjection.cs`, `Identity.Contracts/Services/IServiceClaimService.cs` | Either implement and register it, or remove the unused contract until it's needed. *(User: later.)* |
| P5 | `IdentityClaimQueryExtensions.CheckUserHasClaimAsync` is dead code (never called) | `Identity.Api/Services/IdentityClaimQueryExtensions.cs` | Wire it into a permission check or remove it. *(User: later.)* |
| P6 | `DispatchDomainEvents` convention is never called from `IdentityDbContext.SaveChanges[Async]`; `UserCreatedEvent` is instead published manually via MediatR from the command handler, bypassing it | `Persistence/Extensions/DispatchDomainEventsExtensions.cs`, `Identity.Api/Data/IdentityDbContext.cs`, `Application/Users/Commands/CreateUser.cs` | Related to D1 — once a pattern is picked, make domain-event dispatch consistent across Identity. |

---

## Dependency / package hygiene

- **Likely dead package references** (no source usage found): `Lightsoft.EventBus` (`Shared.csproj`), `Lightsoft.FileGenerator` (`Infrastructure.csproj`). Verify and remove if confirmed unused.
- **Undeclared transitive dependencies** — several projects compile only because a `ProjectReference` happens to bring a package along, not because they declare it themselves:
  - `Infrastructure` uses `Mapster` and `Lightsoft.AspNetCore.Authorization`/`Lightsoft.Result` without declaring either (rides on `Shared`'s references).
  - `Identity.Api` uses `Lightsoft.AspNetCore.Authorization`, `Lightsoft.Result`, `Lightsoft.Extensions` (via `GlobalUsings.cs`) without declaring any of them.
  - **New site, 2026-08-01**: `UserService.SearchAsync`'s new `WhereIf` call (`Identity.Api/Services/UserService.cs:1,22`) uses `Light.Specification`, which is provided by the `Lightsoft.EntityFrameworkCore` package — declared by `Persistence.csproj`, not by `Identity.Api.csproj`, and only compiles today because it rides in transitively through the `Persistence` `ProjectReference`.
  - `StarterKit.WebApi` uses `Lightsoft.Serilog` without declaring it (only `Infrastructure.csproj` does).
  - Fragile: if the upstream project ever drops one of these packages, downstream projects break with no direct signal why. Recommend each project explicitly declare what it directly uses.
- `tests/Framework.Tests` currently references only `Shared`, `Infrastructure`, `Persistence` — **no test coverage exists for `Identity.Api`/`Identity.Contracts`**.

## Housekeeping

- `src/Identity.Api/obj/` and `src/Identity.Contracts/obj/` still contain stale build artifacts referencing pre-refactor assembly names (`Identity.Core.AssemblyInfo.cs`, `StaterKit.Identity.EntityFrameworkCore.*` — note the "StaterKit" typo). Run `dotnet clean` (or delete `bin`/`obj`) to clear these.
- `UserSession` (renamed from `JwtToken`) still extends the plain vendor `Entity` base type — no decision made yet on whether it should carry an audit trail via `AuditableEntity`.
- `Entities/UserToken.cs` (ASP.NET Core Identity's built-in `IdentityUserToken<string>` — external-login-provider token store, wired only as an `IdentityDbContext` generic parameter/table mapping, no business logic touches it) sits in the same `Entities/` folder as `UserSession` (this module's actual JWT/session entity) — no functional coupling, but "token" means two unrelated things in this module. Low-cost mitigation if it comes up again: a one-line comment on `UserToken.cs` noting it's unrelated to JWT sessions, or grouping ASP.NET Identity's built-in entities (`UserClaim`, `UserRole`, `UserLogin`, `RoleClaim`, `UserToken`) separately from module-specific ones (`User`, `Role`, `UserSession`).

---

## Out of scope

- `src/Migrations/**` — excluded per request, except where a change elsewhere forced a mechanical edit there (the `UserSession` rename; the `AddUserCreatedIndex` migration for P19 on Sqlite/PostgreSQL; and the MSSQL `CreateIdentitySchema` baseline reset, 2026-08-01 — see resolved D5 above).
