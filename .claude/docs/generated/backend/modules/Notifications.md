# Module Overview: Notifications

## Purpose

Owns storage and real-time delivery of system/user notifications: persisting notification records (sender, recipient, title, message, optional deep-link URL, read/archived status) and pushing them live to connected clients over SignalR. Exposes two surfaces: an admin-facing "browse + send" capability gated by explicit permissions, and a self-service "my notifications" surface auto-scoped to the calling user via `ICurrentUser` (no extra permission required — any authenticated user can read/mark-read their own notifications). Also carries a distinct "force logout" push message that reuses the same SignalR channel as a live session-invalidation signal, without creating a stored notification record.

## Internal Layering

Notifications is a **single-project module** (not split Domain/Application/Infrastructure/Api) — the template's 4-row project table doesn't apply as-is; adapted to the module's actual two projects:

| Project | Responsibility | Notes |
|---|---|---|
| `Notifications.Contracts` | DTOs (`NotificationDto`), enum (`NotificationStatus`: `None`/`Read`/`Archived`, serialized by name over HTTP), request shape (`NotificationLookup : PageQuery`), push-message contracts (`SystemMessage`, `ForceLogoutMessage`, both implementing marker `INotificationMessage`), service interface (`INotificationService`), permission strings + catalog (`NotificationPermissions`, `NotificationPermissionProvider : IPermissionDefinitionProvider`), push-channel name constant (`NotificationConstants.SERVER_NOTIFICATION`, currently defined but unused — see Notable Conventions). The module's only seam project. | Depends only on `Shared` (leaf project reference) — no dependency on any other module's `Contracts`. |
| `Notifications.Api` | Single project organized by folder: `Entities/Notification.cs` (`: AuditableEntity`), `Data/NotificationDbContext.cs`, `Services/NotificationService.cs` (`internal`, implements `INotificationService`), `Controllers/{NotificationController,UserNotificationController}.cs`, `SignalR/{SignalRHub,IHubService,HubService,CustomIdProvider,SignalRModule}.cs`, `NotificationModule.cs` (DI: DbContext + `INotificationService`). | `.Api` suffix kept for the same future-microservice-extraction reason already established for `Identity.Api` — not a fresh deviation. |

## Public Contract

`NotificationController` (admin surface, route base `notification`, permission-gated):

| Route | Verb | Permission | Request | Response |
|---|---|---|---|---|
| `api/v{version}/notification` | GET | `notification.read` | Query `NotificationLookup` (`ToUserId?`, `Status?` — `NotificationStatus` by name, plus `PageQuery`'s `PageNumber`/`PageSize`, default 1/20) | `PagedResult<NotificationDto>` |
| `api/v{version}/notification` | POST | `notification.send` | Query string `fromUserId`, `fromName?`, `toUserId`; body `SystemMessage { Title, Message?, Url?, ByMessage }` | `Ok()` (no body) — persists a `Notification` row, then pushes it live to `toUserId` over SignalR as event `SystemMessage` |
| `api/v{version}/notification/force_logout` | POST | `notification.send` | Body `ForceLogoutMessage { UserId }` | `Ok()` (no body) — pushes a `ForceLogoutMessage` event to that user's live connection(s) only; **no DB write, no stored notification** |

`UserNotificationController` (self-service surface, route base `user_notification`, **no `[MustHavePermission]`** — any authenticated user, force-scoped server-side to `ICurrentUser.UserId`):

| Route | Verb | Request | Response |
|---|---|---|---|
| `api/v{version}/user_notification` | GET | Query `NotificationLookup` — `ToUserId` in the query is **ignored/overwritten** with the caller's own id | `PagedResult<NotificationDto>` |
| `api/v{version}/user_notification/{entryId}` | GET | Route param `entryId` | `NotificationDto?` — **side effect**: marks the entry `Read` before returning it (`MarkAsReadAsync` then `GetByIdAsync`) |
| `api/v{version}/user_notification/count_unread` | GET | none | `int` — count of the caller's rows with `Status == None` |

*(Route prefix `api/v{version}/` and the exact base-route strings `notification`/`user_notification` are confirmed via the admin client's actual API call sites, not just inferred from the vendor base controller's naming convention.)*

**SignalR**: `SignalREndpoint` (an `AppModuleEndpoint`) maps `/signalr-hub` → `SignalRHub : Hub` (`[Authorize]`), with `Transports = WebSockets` and `CloseOnAuthenticationExpiration = true`. `CustomIdProvider : IUserIdProvider` maps a hub connection to a user id via the `ClaimTypeConstants.UserId` claim, so `IHubService`'s `Clients.User(userId)` targeting works. The hub defines **no server-callable client→server methods** — it's push-only; `OnConnectedAsync`/`OnDisconnectedAsync` just join/leave a broadcast group ("SignalR Users") that nothing currently broadcasts to (`IHubService.NotifyAsync(...)`'s `Clients.All`/broadcast overloads have no controller caller today — dead capability).

**Two verified gaps**: `INotificationService.ReadAllAsync(userId)` exists on the interface/implementation but is **not exposed by any controller** — no "mark all as read" endpoint. There is also **no server-side path that ever sets `Status = Archived`** despite the enum value existing and the admin frontend's "Archived" tab filtering for it — this status value appears currently unreachable from any wired endpoint (flagged as a gap to follow up on, not assumed intentional).

## Data Access

`NotificationDbContext : BaseDbContext`, default schema `"system"`, one `DbSet<Notification>` → table `Notifications`. Registered via `AddConfiguredDbContext<NotificationDbContext>(configuration, DbConnectionNames.Identity)` — `DbConnectionNames.Identity` is itself an alias for `DbConnectionNames.Default` ("DefaultConnection"), so this module **shares the same physical database/connection string as `Identity`** (and any other module using `Default`), separated only by DB schema + table name, not physical isolation — consistent with "one DbContext per module, shared physical DB" being the intended default. Only index: `HasIndex(x => x.ToUserId)` — no index on `Status`, even though both list queries (`GetAsync`) and `CountUnreadAsync` filter by `Status` (often combined with `ToUserId`); worth a performance-reviewer follow-up at scale, not a proven problem today. Column lengths: `FromUserId`/`ToUserId` `MaxLength(450)` (matches ASP.NET Identity's default id length), `FromName` `MaxLength(200)`, `Title` `MaxLength(250)`; `Message`/`Url` unconstrained. `Notification : AuditableEntity`, audited via `NotificationDbContext.SaveChanges[Async]` → `AuditEntries(currentUser.UserId, clock.AuditTime, enableSoftDelete: false)` — `Notification` doesn't implement `ISoftDelete` at all, so (unlike Identity's `User`) this is not a comparable bug, just no soft-delete support.

## Dependencies

| Depends on | Type | Why |
|---|---|---|
| `Shared` | project (`Notifications.Contracts → Shared`) | Base `PageQuery`, etc. |
| `Infrastructure` | project (`Notifications.Api → Infrastructure`) | `VersionedApiController`, `AppModule`/`AppModuleEndpoint` base classes. |
| `Persistence` | project (`Notifications.Api → Persistence`) | `BaseDbContext`, `AddConfiguredDbContext`, `AuditEntries`/`ConfigureAuditableEntity`, `WhereIf`/`ToPagedResultAsync` extensions. |
| `Notifications.Contracts` | project (`Notifications.Api → Notifications.Contracts`) | The module's own seam. |
| Vendor `Light.AspNetCore.Authorization` | package, transitive (no direct `PackageReference` in `Notifications.Contracts.csproj` — rides in via `Shared`) | `[MustHavePermission]`, `IPermissionDefinitionProvider`/`PermissionDefinition` — same undeclared-transitive-dependency pattern already flagged for `Identity.Contracts` (see `modules/Identity.md`). |
| Vendor `Light.EntityFrameworkCore.Extensions`, `Light.Specification`, `Mapster` | package | `WhereIf`, `ProjectToType<T>`, `ToPagedResultAsync` in `NotificationService`. |
| `Microsoft.AspNetCore.SignalR` | ASP.NET Core shared framework | `Hub`, `IHubContext<T>`, `IUserIdProvider`. |

No business-module-to-business-module dependency exists: **Notifications never references `Identity.Api`/`Identity.Contracts`** — `fromUserId`/`toUserId` are opaque strings with no FK or cross-module service call to validate them against real users. Intentional decoupling with a real consequence: nothing stops `POST notification` from writing/pushing to a `toUserId` that doesn't exist.

## Depended On By

No other business module references `Notifications.Api`/`Notifications.Contracts` (verified via `ProjectReference` search across all `.csproj` files under `src/`) — only `StarterKit.WebApi` (composition-root host) and `src/Migrations/MSSQL/MSSQL.csproj` (migrations tooling) reference `Notifications.Api`. Client-side: `clients/admin/src/features/notifications/` is the only verified consumer, calling `notification`/`user_notification` over HTTP and connecting directly to `/signalr-hub` (proxied same-origin via the admin app's `next.config.ts` `rewrites()`).

## Notable Conventions

- Unlike `Identity`, this module's controllers split cleanly by **audience** rather than by resource — `NotificationController` (admin, explicit permissions) vs. `UserNotificationController` (self-service, permission-less but hard-scoped via `ICurrentUser.UserId`) — a pattern worth reusing for any future module needing both an admin and a self-service view over the same table.
- `NotificationConstants.SERVER_NOTIFICATION` is defined but never referenced by `HubService`'s actual `SendAsync<T>` calls (which use `typeof(T).Name` — e.g. `"SystemMessage"`, `"ForceLogoutMessage"` — as the event name instead) — likely dead/leftover from an earlier design; don't assume it's the real event name when integrating a new client.
- `HubService.NotifyAsync(...)` (broadcast-to-all/many, no payload) is fully implemented but has zero callers anywhere in the module or host — dead capability today.
- Two verified functional gaps (see Public Contract): no "mark all read" endpoint despite `ReadAllAsync` existing on the service interface, and no code path ever sets `NotificationStatus.Archived`.

## Notes

<!-- manual: content below this line is human-authored and must be preserved verbatim during sync -->

---
_Generated: 2026-08-03 — scope: module "Notifications" (first-time generation) — see .claude/CLAUDE.md for update rules._
