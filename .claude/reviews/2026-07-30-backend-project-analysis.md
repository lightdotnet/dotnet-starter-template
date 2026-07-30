# Backend Project Analysis & Refactor Proposals — 2026-07-30

**Scope**: all backend projects under `src/` on branch `feat-dev/identity-module`, including `Identity.Api` / `Identity.Contracts`. `src/Migrations/**` generally excluded from analysis per request (exceptions noted inline where a change elsewhere forced a mechanical edit there).

This file now tracks **pending items only**. Resolved/implemented items (module-structure convention, N1–N6 naming, P2/P3 Schemas removal, Sqlite/PostgreSQL migration fix, the `IdentityDbContext`/framework name-collision bug) have been removed — see `ARCHITECTURE.md` § Backend — Module Structure Convention and git history for that work.

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
`TokenService.GetTokenAsync`/`RefreshTokenAsync` both short-circuit on `user is null` *before* calling `CheckInvalidUser` (`TokenService.cs:28,81`), so a hard-deleted user is already rejected via the null check — `CheckInvalidUser`'s `user.Deleted != null` branch is dead code, but that's incidental, not a security hole.

The actual cost of disabling soft-delete (`IdentityDbContext` calls `AuditEntries(..., enableSoftDelete: false)`):
- `User.Delete()` (`Entities/User.cs:55-65`) anonymizes the row first (nulls `UserName`/`Email`/`PhoneNumber`/names/`PasswordHash`, locks `Status`) — a soft-delete-and-anonymize pattern. But `UserService.DeleteAsync` (`Services/UserService.cs:170-186`) then immediately hard-deletes that same just-anonymized row — the anonymization work is wasted.
- Once the row is gone, anything that stamped `CreatedBy`/`LastModifiedBy`/a `UserId` FK with this user's Id (e.g. `UserSessions.UserId`, audit fields on any future module's entities) becomes an orphaned reference — an audit log showing "created by `<UserId>`" can no longer resolve who that was.
- `User.Deleted`/`DeletedBy` end up permanently `null` — dead schema; a future admin/audit feature filtering `WHERE Deleted IS NOT NULL` would always get zero rows, silently.

Decide: flip `enableSoftDelete: true` in `IdentityDbContext.SaveChanges[Async]` (keeps the anonymized tombstone; `TokenService`'s check becomes the real enforcement point) — or, if hard delete is genuinely intended, remove `ISoftDelete`/the `Deleted`/`DeletedBy` columns/the dead check.

This is a **one-line change**, not new logic: `TrackingExtensions.AuditEntries` (`Persistence/Extensions/TrackingExtensions.cs:19-31`) already does the "who/when" stamping automatically when `enableSoftDelete: true` is passed, and `IdentityDbContext` already passes the right `currentUser.UserId`/`clock.AuditTime` arguments today — just the boolean flag needs flipping (or the cleanup path taken instead).

---

## Correctness / performance

| # | Finding | File(s) | Recommendation |
|---|---|---|---|
| P4 | `IServiceClaimService` has no registered implementation — commented out in DI | `Identity.Api/DependencyInjection.cs`, `Identity.Contracts/Services/IServiceClaimService.cs` | Either implement and register it, or remove the unused contract until it's needed. *(User: later.)* |
| P5 | `IdentityClaimQueryExtensions.CheckUserHasClaimAsync` is dead code (never called) | `Identity.Api/Services/IdentityClaimQueryExtensions.cs` | Wire it into a permission check or remove it. *(User: later.)* |
| P6 | `DispatchDomainEvents` convention is never called from `IdentityDbContext.SaveChanges[Async]`; `UserCreatedEvent` is instead published manually via MediatR from the command handler, bypassing it | `Persistence/Extensions/DispatchDomainEventsExtensions.cs`, `Identity.Api/Data/IdentityDbContext.cs`, `Application/Users/Commands/CreateUser.cs` | Related to D1 — once a pattern is picked, make domain-event dispatch consistent across Identity. |

### P1 — lookup by PK, not by indexing the token string (decided, not yet implemented)

`Token` (on `UserSession`) stores the full JWT (`JwtTokenManager.cs:91`), an unbounded string that grows with the number of role/permission claims a user has — realistically over 1500 characters for users with several roles. With no `HasMaxLength` configured, EF Core maps this to `nvarchar(max)` on SQL Server, and **SQL Server cannot index an `nvarchar(max)` column at all**; PostgreSQL's btree index has a similar ~2704-byte per-entry cap that fails at insert time instead. `RefreshToken`, by contrast, is a fixed ~44-char Base64 string and is safe to index directly.

Better design (no schema/index changes needed at all):
- Every issued token already embeds its own `UserSession.Id` as a `tid` claim (`ClaimTypeConstants.TokenId`, set in `JwtTokenManager.cs:81,123`), and `Id` is already the primary key (indexed for free).
- Parse the `tid` claim out of the presented token, fetch the single row by PK (`context.UserSessions.FindAsync(tokenId)` — no new index required), then compare `entity.Token == accessToken` (and `entity.RefreshToken == refreshToken` for the refresh flow) **in memory**, plus check `Revoked`/expiry.
- Safe even though `tid` can be read from an unverified JWT (e.g. in `IsTokenValidAsync`/`CheckToken`): the trust boundary is the final full-string equality check against the securely-stored original — forging a `tid` doesn't help an attacker, since they'd still need the exact original signed token content, which requires the signing key.
- In the refresh flow, `tid` is available for free from the already-signature-validated `userPrincipal` (`JwtHelper.GetPrincipalFromExpiredToken`, `TokenService.cs:67`) — just extract one more claim.
- Minor optional hardening: once comparison moves from SQL `=` to in-memory C# `==`, consider `CryptographicOperations.FixedTimeEquals` to avoid a timing side-channel on the secret token value.

---

## Dependency / package hygiene

- **Likely dead package references** (no source usage found): `Lightsoft.EventBus` (`Shared.csproj`), `Lightsoft.FileGenerator` (`Infrastructure.csproj`). Verify and remove if confirmed unused.
- **Undeclared transitive dependencies** — several projects compile only because a `ProjectReference` happens to bring a package along, not because they declare it themselves:
  - `Infrastructure` uses `Mapster` and `Lightsoft.AspNetCore.Authorization`/`Lightsoft.Result` without declaring either (rides on `Shared`'s references).
  - `Identity.Api` uses `Lightsoft.AspNetCore.Authorization`, `Lightsoft.Result`, `Lightsoft.Extensions` (via `GlobalUsings.cs`) without declaring any of them.
  - `StarterKit.WebApi` uses `Lightsoft.Serilog` without declaring it (only `Infrastructure.csproj` does).
  - Fragile: if the upstream project ever drops one of these packages, downstream projects break with no direct signal why. Recommend each project explicitly declare what it directly uses.
- `tests/Framework.Tests` currently references only `Shared`, `Infrastructure`, `Persistence` — **no test coverage exists for `Identity.Api`/`Identity.Contracts`**.

## Housekeeping

- `src/Identity.Api/obj/` and `src/Identity.Contracts/obj/` still contain stale build artifacts referencing pre-refactor assembly names (`Identity.Core.AssemblyInfo.cs`, `StaterKit.Identity.EntityFrameworkCore.*` — note the "StaterKit" typo). Run `dotnet clean` (or delete `bin`/`obj`) to clear these.
- `UserSession` (renamed from `JwtToken`) still extends the plain vendor `Entity` base type — no decision made yet on whether it should carry an audit trail via `AuditableEntity`.

---

## Out of scope

- `src/Migrations/**` — excluded per request, except where a change elsewhere (e.g. the `UserSession` rename) forced a mechanical edit there.
