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

### D2. Soft-delete is wired up but disabled — likely a real bug, not just an inconsistency
`AppIdentityDbContext` calls `AuditEntries(..., enableSoftDelete: false)`, so `User.Deleted`/`DeletedBy` (from `ISoftDelete`) are never populated and `UserManager.DeleteAsync` issues a **hard delete**. But `TokenService.CheckInvalidUser` actively checks `user.Deleted != null` to reject logins — dead logic that will never trigger. Decide: enable soft-delete for `AppIdentityDbContext`, or remove `ISoftDelete`/the columns/the dead check from `User`/`TokenService`. Left as-is, this is a data-loss risk masquerading as a safety check that doesn't actually run.

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

> N1 (below) was proposed as a rename but has been decided against — see note under the table.

| # | Current | Proposed | Why |
|---|---|---|---|
| N2 | `Identity.Api/Jwt/JwtTokenMananger.cs` (class itself is `JwtTokenMananger`) | `JwtTokenManager.cs` / `JwtTokenManager` | Typo in both filename and type name |
| N3 | `Identity.Api/Extensions/IdentityResultExtension.cs` (class inside is already plural `IdentityResultExtensions`) | `IdentityResultExtensions.cs` | File name doesn't match the class it contains |
| N4 | `Persistence/Migrations/` (runtime helpers: `MigrationsExtensions.cs`, `MigratorCurrentUser.cs`) sits next to the unrelated top-level `src/Migrations/` (design-time EF projects) | `Persistence/MigrationSupport/` or `Persistence/DesignTimeSupport/` | Same word, two unrelated concepts — actively confusing when navigating the tree |
| N5 | `Identity.Api/Services/QueryExtensions.cs` vs `Persistence/Extensions/QueryableResultExtensions.cs` — not actual duplicates (different responsibilities: Identity-specific claim-union logic vs generic paging/result helpers), but the near-identical names invite confusion | `IdentityClaimQueryExtensions.cs` | Disambiguate by name since responsibilities are genuinely different |

**N1 — decided, kept as-is**: `src/Identity.Api` stays named `Identity.Api`, not renamed to `Identity`. The `.Api` suffix is intentional: `Identity` is a deliberate future candidate for extraction into an independent microservice, and keeping the `.Api` name now means the standalone service's own API project would already have the right name, avoiding a rename later. This is now captured in [ARCHITECTURE.md § Backend — Module Structure Convention](../ARCHITECTURE.md#backend--module-structure-convention) as an explicit naming option (`<Module>` by default, `<Module>.Api` for extraction candidates).

## Dependency / package hygiene

- **Likely dead package references** (no source usage found): `Lightsoft.EventBus` (`Shared.csproj`), `Lightsoft.FileGenerator` (`Infrastructure.csproj`). Verify and remove if confirmed unused.
- **Undeclared transitive dependencies** — several projects compile only because a `ProjectReference` happens to bring a package along, not because they declare it themselves:
  - `Infrastructure` uses `Mapster` and `Lightsoft.AspNetCore.Authorization`/`Lightsoft.Result` without declaring either (rides on `Shared`'s references).
  - `Identity.Api` uses `Lightsoft.AspNetCore.Authorization`, `Lightsoft.Result`, `Lightsoft.Extensions` (via `GlobalUsings.cs`) without declaring any of them.
  - `StarterKit.WebApi` uses `Lightsoft.Serilog` without declaring it (only `Infrastructure.csproj` does).
  - This is fragile: if the upstream project ever drops one of these packages, downstream projects break with no direct signal why. Recommend each project explicitly declare what it directly uses.
- `tests/Framework.Tests` currently references only `Shared`, `Infrastructure`, `Persistence` — **no test coverage exists for `Identity.Api`/`Identity.Contracts`**. Noted for awareness, not a judgment on test quality/priority.

## Housekeeping

- `src/Identity.Api/obj/` and `src/Identity.Contracts/obj/` contain stale build artifacts referencing pre-refactor assembly names (`Identity.Core`, `Identity.EntityFrameworkCore`, `StaterKit.Identity.EntityFrameworkCore` — note the "StaterKit" typo). Not source, but run `dotnet clean` (or delete `bin`/`obj`) before doing the renames above so stale artifacts don't cause confusion mid-refactor.
- `AppIdentityDbContext` cannot inherit `Persistence/Context/BaseDbContext.cs` (it must derive from `IdentityDbContext<...>`), so it re-applies the Sqlite `DateTimeOffset` fix manually via the shared extension method instead. This is a reasonable, unavoidable exception — worth one line in `ARCHITECTURE.md` documenting it as a known exception rather than leaving it as silent drift once docs are next synced.
- `JwtToken` extends the plain vendor `Entity` base type instead of the shared `AuditableEntity` convention used elsewhere — fine if tokens intentionally carry no audit trail, but worth an explicit decision rather than an accident.

---

## Explicitly out of scope here

- `src/Migrations/**` (MSSQL/Sqlite/PostgreSQL design-time projects) — excluded per request.
- No code has been changed yet. N1 has been decided (kept as-is, see note above) — everything else, especially P1–P6, is wide-reaching and should be treated as its own reviewed change, not a drive-by edit.
- `.claude/PROJECT.md` and `.claude/ARCHITECTURE.md` have since been updated (2026-07-30, same day) with the module-structure convention and current verified facts (`Persistence`, `Identity.Contracts`, `Identity.Api`, `StarterKit.WebApi`). Everything else in this file (D1, D2, P1–P6, N2–N5, dependency hygiene, housekeeping) is still pending.
