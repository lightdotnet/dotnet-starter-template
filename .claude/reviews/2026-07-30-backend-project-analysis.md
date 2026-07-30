# Backend Project Analysis & Refactor Proposals — 2026-07-30

**Scope**: all backend projects under `src/` on branch `feat-dev/identity-module`, including the new `Identity.Api` / `Identity.Contracts` projects, after the recent structural refactor. `src/Migrations/**` was explicitly excluded from analysis per request.

**Method**: read-only analysis via four specialized agents — `dependency-analyzer`, `architecture-reviewer`, `dotnet-architect`, `efcore-specialist` — each scoped to `.csproj` references, layering, naming, and EF Core design respectively. No code was changed. This document consolidates their findings into prioritized proposals. Nothing below has been applied yet.

## Current project graph (verified, acyclic)

```
Shared (leaf)
  ← Infrastructure
  ← Persistence
Identity.Contracts (leaf)
  ← Identity.Api  (also → Infrastructure, Persistence)
       ← StarterKit.WebApi (host, also → Infrastructure, Shared)
```

No circular references. Dependency direction is sound at the project level.

---

## Decisions needed (highest leverage — affects every future module)

> The module-structure decision (module = flat project(s) under `src/`, single-project or Clean-Architecture-split depending on complexity, always with a `<Module>.Contracts` seam) has been made and is now documented in `CLAUDE.md` and [ARCHITECTURE.md § Backend — Module Structure Convention](../ARCHITECTURE.md#backend--module-structure-convention) — removed from this list.

### D1. CQRS vs. service classes inside Identity — pick one pattern
`Identity.Api` currently has both: `UserService`/`RoleService` (traditional service classes, used directly by controllers) and a single CQRS command (`Application/Users/Commands/CreateUser.cs`) that just wraps `IUserService.CreateAsync` to publish `UserCreatedEvent`. Every other operation (roles, tokens, other user operations) bypasses the command pattern entirely. This reads as an unfinished migration rather than a deliberate layering. Recommend either:
- Drop the CQRS wrapper — publish `UserCreatedEvent` directly from `UserService.CreateAsync`; or
- Commit to CQRS for all Identity writes and demote services to internal/read-only helpers.

### D2. Soft-delete is wired up but never actually happens — audit-trail loss, not a login-bypass risk
**Corrected 2026-07-30**: this was originally framed as "a deleted user could still log in" — that's wrong and worth retracting explicitly. `TokenService.GetTokenAsync`/`RefreshTokenAsync` both short-circuit on `user is null` *before* calling `CheckInvalidUser` (`TokenService.cs:28,81`), so once `UserManager.DeleteAsync` hard-deletes the row, `FindByNameAsync`/`FindByIdAsync` already return `null` and login/refresh is rejected regardless of the `Deleted` field. `CheckInvalidUser`'s `user.Deleted != null` check is dead code (unreachable in practice), but that's incidental — it's not a security hole.

The actual cost of disabling soft-delete (`AppIdentityDbContext` calls `AuditEntries(..., enableSoftDelete: false)`):
- `User.Delete()` (`Entities/User.cs:55-65`) anonymizes the row first — nulls `UserName`/`Email`/`PhoneNumber`/`FirstName`/`LastName`/`PasswordHash`, locks `Status` — clearly written for a soft-delete-and-anonymize pattern (the kind used for GDPR-style "forget me" while keeping a tombstone). But `UserService.DeleteAsync` (`Services/UserService.cs:170-186`) calls `user.Delete()` and then immediately `userManager.DeleteAsync(user)`, which hard-deletes that same just-anonymized row in the same operation — so the anonymization work is wasted; the row is gone either way.
- Once the row is gone, anything elsewhere that stamped `CreatedBy`/`LastModifiedBy`/a `UserId` foreign key with this user's Id (e.g. `JwtTokens.UserId`, or audit fields on any other module's entities once they exist) becomes an orphaned reference with no way to resolve back to even an anonymized tombstone — e.g. an audit log showing "created by `<UserId>`" can no longer look up who that was.
- `User.Deleted`/`DeletedBy` end up permanently `null` for every row that still exists — dead schema. Any future admin/audit feature that queries `WHERE Deleted IS NOT NULL` to list deleted users will always get zero rows, silently.

Decide: flip `enableSoftDelete: true` in `AppIdentityDbContext.SaveChanges[Async]` (keeps the anonymized tombstone row; `Deleted`/`DeletedBy` become meaningful; `TokenService`'s check then becomes the actual enforcement point, since the row still exists for an ID-based lookup) — or, if hard delete is genuinely intended, remove `ISoftDelete`, the `Deleted`/`DeletedBy` columns, and the now-pointless check from `User`/`TokenService` so the code stops implying a mechanism that isn't really there.

If soft-delete is enabled: `TrackingExtensions.AuditEntries` (`Persistence/Extensions/TrackingExtensions.cs:19-31`) already does the "who and when" stamping automatically — for any tracked `ISoftDelete` entity in `EntityState.Deleted`, it sets `Entity.Deleted = auditTime` and `Entity.DeletedBy = userId` and flips the entry to `EntityState.Modified` so EF Core issues an `UPDATE` instead of a `DELETE`. Since `AppIdentityDbContext` already passes `currentUser.UserId`/`clock.AuditTime` into this call today (just with the flag off), turning this on is a **one-line change** (`false` → `true`), not new logic to write.

---

## Correctness / performance (fix regardless of D1–D2)

| # | Finding | File(s) | Recommendation |
|---|---|---|---|
| P1 | `JwtTokens` has no index on `Token`, and no composite index on `(UserId, RefreshToken)` | `Identity.Api/Data/AppIdentityDbContext.cs` (only `UserId` indexed); hot-path lookups in `Jwt/JwtTokenMananger.cs` (`IsTokenValidAsync` filters by `Token` alone — full table scan on every authenticated request) | Add an index on `Token`, a composite index on `(UserId, RefreshToken)`, and consider uniqueness constraints on both |
| P2 | `Schemas.cs` duplicated verbatim in two projects; the module's copy shadows the shared one | `Persistence/Schemas.cs` vs `Identity.Api/Data/Schemas.cs` (identical `Audit`/`Identity`/`System` consts) | Delete `Identity.Api/Data/Schemas.cs`; reference `StarterKit.Persistence.Schemas` directly |
| P3 | `Persistence/Schemas.cs` and `DbConnectionNames.cs` bake an `Identity`-specific constant into a project meant to be generic/shared | `Persistence/Schemas.cs`, `Persistence/DbConnectionNames.cs` | Remove the module-specific entry from `Persistence`; keep only `Default`/`Audit`/`System` there (module-specific names belong in the module) |
| P4 | `IServiceClaimService` has no registered implementation — commented out in DI | `Identity.Api/DependencyInjection.cs`, `Identity.Contracts/Services/IServiceClaimService.cs` | Either implement and register it, or remove the unused contract until it's needed |
| P5 | `QueryExtensions.CheckUserHasClaimAsync` is dead code (never called) | `Identity.Api/Services/QueryExtensions.cs` | Wire it into a permission check or remove it |
| P6 | `DispatchDomainEvents` convention is never called from `AppIdentityDbContext.SaveChanges[Async]`; `UserCreatedEvent` is instead published manually via MediatR from the command handler, bypassing it | `Persistence/Extensions/DispatchDomainEventsExtensions.cs`, `Identity.Api/Data/AppIdentityDbContext.cs`, `Application/Users/Commands/CreateUser.cs` | Related to D1 — once a pattern is picked, make domain-event dispatch consistent across Identity |

## Naming / structure cleanup

> N1 was proposed as a rename but has been decided against. N2–N5 have all been applied and verified in code — nothing open in this section.

**N1 — decided, kept as-is**: `src/Identity.Api` stays named `Identity.Api`, not renamed to `Identity`. The `.Api` suffix is intentional: `Identity` is a deliberate future candidate for extraction into an independent microservice, and keeping the `.Api` name now means the standalone service's own API project would already have the right name, avoiding a rename later. This is now captured in [ARCHITECTURE.md § Backend — Module Structure Convention](../ARCHITECTURE.md#backend--module-structure-convention) as an explicit naming option (`<Module>` by default, `<Module>.Api` for extraction candidates).

**N3 — done**: `Identity.Api/Extensions/IdentityResultExtension.cs` → `IdentityResultExtensions.cs`; class inside already matched (`IdentityResultExtensions`). Verified in code.

**N4 — done**: `Persistence/Migrations/` → `Persistence/MigrationSupport/`; namespace updated to `StarterKit.Persistence.MigrationSupport` consistently everywhere it's consumed, including `src/Migrations/MSSQL` and `src/Migrations/Sqlite` (outside this analysis's scope, but needed updating since they referenced the old namespace). No remaining references to the old namespace. Verified in code.

**N5 — done**: `Identity.Api/Services/QueryExtensions.cs` → `IdentityClaimQueryExtensions.cs`, and the class itself renamed from `QueryExtensions` to `IdentityClaimQueryExtensions` to match (this second step was initially missed, then fixed). No remaining references to the old class name. Verified in code.

**N2 — done**: `Identity.Api/Jwt/JwtTokenMananger.cs` → `JwtTokenManager.cs`, class renamed to `JwtTokenManager`. All three consumers (`Controllers/TokenController.cs`, `Jwt/JwtServiceCollectionExtensions.cs`, `Jwt/TokenService.cs`) updated — no remaining references to the old typo'd name anywhere in `src/`. Verified in code.

## Dependency / package hygiene

- **Likely dead package references** (no source usage found): `Lightsoft.EventBus` (`Shared.csproj`), `Lightsoft.FileGenerator` (`Infrastructure.csproj`). Verify and remove if confirmed unused.
- **Undeclared transitive dependencies** — several projects compile only because a `ProjectReference` happens to bring a package along, not because they declare it themselves:
  - `Infrastructure` uses `Mapster` and `Lightsoft.AspNetCore.Authorization`/`Lightsoft.Result` without declaring either (rides on `Shared`'s references).
  - `Identity.Api` uses `Lightsoft.AspNetCore.Authorization`, `Lightsoft.Result`, `Lightsoft.Extensions` (via `GlobalUsings.cs`) without declaring any of them.
  - `StarterKit.WebApi` uses `Lightsoft.Serilog` without declaring it (only `Infrastructure.csproj` does).
  - This is fragile: if the upstream project ever drops one of these packages, downstream projects break with no direct signal why. Recommend each project explicitly declare what it directly uses.
- `tests/Framework.Tests` currently references only `Shared`, `Infrastructure`, `Persistence` — **no test coverage exists for `Identity.Api`/`Identity.Contracts`**. Noted for awareness, not a judgment on test quality/priority.

## Housekeeping

- **Still open, verified 2026-07-30**: `src/Identity.Api/obj/` and `src/Identity.Contracts/obj/` still contain stale build artifacts referencing pre-refactor assembly names (`Identity.Core.AssemblyInfo.cs`, `StaterKit.Identity.EntityFrameworkCore.*` — note the "StaterKit" typo). Run `dotnet clean` (or delete `bin`/`obj`) to clear these.
- **Done**: `AppIdentityDbContext`'s inability to inherit `Persistence/Context/BaseDbContext.cs` (must derive from `IdentityDbContext<...>` instead) is now documented as a known/accepted exception in `ARCHITECTURE.md`'s Data Access table.
- **Still open, verified 2026-07-30**: `JwtToken` still extends the plain vendor `Entity` base type, unchanged from the original finding — no decision made yet on whether it should carry an audit trail via `AuditableEntity`.

---

## Explicitly out of scope here

- `src/Migrations/**` (MSSQL/Sqlite/PostgreSQL design-time projects) — excluded per request.
- All of N1–N5 are resolved (see notes above, verified in code 2026-07-30). Still pending: **D1**, **D2** (decisions), **P1–P6** (correctness/performance — wide-reaching, treat as their own reviewed change, not a drive-by edit), dependency/package hygiene, and 2 of 3 housekeeping items (stale `bin`/`obj`, `JwtToken`/`AuditableEntity` decision).
- `.claude/PROJECT.md` and `.claude/ARCHITECTURE.md` have since been updated (2026-07-30, same day) with the module-structure convention and current verified facts (`Persistence`, `Identity.Contracts`, `Identity.Api`, `StarterKit.WebApi`).
