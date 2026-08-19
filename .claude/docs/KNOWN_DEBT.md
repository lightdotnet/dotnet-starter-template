# Known Debt — Backend

> Canonical, **current-state-only** list of open backend technical debt and pending architecture decisions. Resolved items are removed, not kept as history — `git log` covers that. Client-side debt has its own single source: [docs/generated/clients/admin/architecture.md § Known Architectural Risks / Debt](generated/clients/admin/architecture.md#known-architectural-risks--debt) — not duplicated here.
>
> Referenced from [ARCHITECTURE-BACKEND.md § Architectural Risks / Debt](../ARCHITECTURE-BACKEND.md#backend--architectural-risks--debt) and [PROJECT-BACKEND.md § Backend Open Questions / Gaps](../PROJECT-BACKEND.md#backend-open-questions--gaps). When an item here is resolved, delete it (don't strike it through); when a new one is found, add it here rather than back into those docs. Lessons learned from the original analysis pass (trimmed to durable diagnostic value, resolved items removed): [reviews/2026-07-30-backend-project-analysis.md](../reviews/2026-07-30-backend-project-analysis.md) (archival).

## Architecture decisions pending

- **D1 — CQRS vs. service classes inside `Identity`.** `Identity.Api` has both: `UserService`/`RoleService` (traditional service classes, used directly by controllers) and a single CQRS command (`Application/Users/Commands/CreateUser.cs`) that wraps `IUserService.CreateAsync` to publish `UserCreatedEvent`. Every other operation bypasses the command pattern. Pick one: drop the CQRS wrapper (publish `UserCreatedEvent` directly from `UserService.CreateAsync`), or commit to CQRS for all `Identity` writes and demote services to internal/read-only helpers. *(User: to be implemented separately.)*
- **D2 — Soft-delete is wired up but disabled.** `IdentityDbContext` calls `AuditEntries(..., enableSoftDelete: false)` even though `User` implements `ISoftDelete` and `AuthenticationService.CheckInvalidUser` checks `user.Deleted != null` (currently dead code — `AuthenticationService` short-circuits on `user is null` first, so this is an audit-trail/anonymization gap, not a login-bypass risk). `UserService.DeleteAsync` (`Services/UserService.cs`) hard-deletes immediately after `User.Delete()` anonymizes the row, wasting that work; a hard-deleted user's `Id` also orphans any FK/audit field that referenced it (e.g. `UserSessions.UserId`). Decide: flip `enableSoftDelete: true` in `IdentityDbContext.SaveChanges[Async]` (one-line change — `TrackingExtensions.AuditEntries` already does the stamping), or remove `ISoftDelete`/`Deleted`/`DeletedBy` if hard delete is genuinely intended.
- **D4 — `GET user` vs. `GET user/search` coexist.** `UserController` exposes both an unbounded `GET user` (`IEnumerable<UserDto>`) and a paginated `GET user/search` (`PagedResult<UserDto>`, via `Identity.Contracts/SearchUserQuery.cs`) for the same read use case. Decide whether `GET user` is an intentional "admin/all" escape hatch or should be deprecated now that paginated search exists — currently organic drift, not a documented decision, and a future module has no precedent to follow.

## Correctness / performance

- **P4 — `IServiceClaimService` has no registered implementation** (commented out in `Identity.Api/DependencyInjection.cs`; interface in `Identity.Contracts/Services/IServiceClaimService.cs`). Either implement and register it, or remove the unused contract. *(User: later.)*
- **P5 — `IdentityClaimQueryExtensions.CheckUserHasClaimAsync` is dead code** (`Identity.Api/Services/IdentityClaimQueryExtensions.cs`, never called). Wire it into a permission check or remove it. *(User: later.)*
- **P6 — Domain-event dispatch convention bypassed in `Identity`.** `DispatchDomainEventsExtensions.DispatchDomainEvents` (`src/Persistence/Extensions/`) is never called from `IdentityDbContext.SaveChanges[Async]`; `UserCreatedEvent` is instead published manually via the mediator from the `CreateUser` command handler. Related to D1 — once a CQRS-vs-service pattern is picked, make dispatch consistent.
- **`Notifications`: no index on `Status`.** `NotificationDbContext`'s only index is `HasIndex(x => x.ToUserId)`, but both list queries and `CountUnreadAsync` filter on `Status` (often combined with `ToUserId`). Performance follow-up candidate at scale, not proven today.
- **`NotificationConstants.SERVER_NOTIFICATION` is unused.** Defined but the actual `SendAsync<T>` calls use `typeof(T).Name` for the SignalR event name instead — don't assume the constant is the real event name.
- **`HubService.NotifyAsync` (broadcast) has zero callers** — dead capability.
- **Migration-time domain-event dispatch may silently miss module handlers.** `MigrationsExtensions.AddMigrationsServices` (`src/Persistence/MigrationSupport/`) registers mediator handlers via `Assembly.GetExecutingAssembly()`, which resolves to the `Persistence` assembly, not `Identity.Api`/`Notifications.Api`. Revisit once a module actually relies on migration-time dispatch.

## Structural / code quality

- **`Identity` predates the module structure convention** (adopted 2026-07-30) and doesn't fully conform: internal layering is informal (folder-based, not compiler-enforced) and CQRS/service-class patterns coexist (see D1). Treat as pending `Identity` cleanup, not a second convention. `Notifications` (built after the convention) does conform.
- **`ApiControllerBase`/`VersionedApiController` duplicate a `_mediator` backing-field + lazy `Mediator` property** (`src/Infrastructure/Endpoints/`). Likely unavoidable since they derive from two different vendor base classes; revisit if the vendor library ever offers a shared base to consolidate into.
- **`UserSession` extends the plain vendor `Entity` base type, not `AuditableEntity`** — no decision made on whether it should carry an audit trail.
- **`Entities/UserToken.cs`** (ASP.NET Core Identity's built-in `IdentityUserToken<string>`, external-login-provider token store, no business logic touches it) sits in the same `Entities/` folder as `UserSession` (this module's actual JWT/session entity) — "token" means two unrelated things in this module. Low-cost mitigation if it comes up again: a one-line comment on `UserToken.cs`, or grouping ASP.NET Identity's built-in entities separately from module-specific ones.

## Test coverage

- **No `tests/Notifications.Tests` project** — `Identity` has `tests/Identity.Tests` (98 tests), `Notifications` has no automated coverage yet.

## Dependency hygiene

- **Likely dead package references** (no source usage found): `Lightsoft.EventBus` (`Shared.csproj`), `Lightsoft.FileGenerator` (`Infrastructure.csproj`). Verify and remove if confirmed unused.
- **Undeclared transitive dependencies** — compile only because a `ProjectReference` happens to bring the package along:
  - `Infrastructure` uses `Mapster` and `Lightsoft.AspNetCore.Authorization`/`Lightsoft.Result` without declaring either (rides on `Shared`).
  - `Identity.Api` uses `Lightsoft.AspNetCore.Authorization`, `Lightsoft.Result`, `Lightsoft.Extensions` (via `GlobalUsings.cs`) without declaring any of them.
  - `Identity.Api`'s `UserService.SearchAsync` uses `Light.Specification` (`WhereIf`), provided by `Lightsoft.EntityFrameworkCore` — declared by `Persistence.csproj`, not `Identity.Api.csproj`, riding in transitively.
  - `StarterKit.WebApi` uses `Lightsoft.Serilog` without declaring it (only `Infrastructure.csproj` does).
  - Fragile: if an upstream project ever drops one of these packages, downstream projects break with no direct signal why. Recommend each project explicitly declare what it directly uses.

## Housekeeping

- **`src/Identity.Api/obj/` and `src/Identity.Contracts/obj/` contain stale build artifacts** referencing pre-refactor assembly names (`Identity.Core.AssemblyInfo.cs`, `StaterKit.Identity.EntityFrameworkCore.*`). Run `dotnet clean` (or delete `bin`/`obj`) to clear.

---
_Last synced: 2026-08-13_
