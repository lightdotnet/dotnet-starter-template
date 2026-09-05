# Module Overview: Approval

## Purpose

A generic, reusable multi-level approval-request engine — not tied to any specific request type. A consuming module (e.g. a future Leave module) resolves the full approver chain itself (Approval has no knowledge of org structure, employee levels, or who should approve what) and calls `IApprovalService.CreateAsync` with an already-resolved `ApproverChain`. Approval then owns the generic mechanics: one `ApprovalRequest` per source record, a `Level`-ordered chain of `ApprovalStep`s, advancing `CurrentLevel` as each step is approved, finalizing the request as `Approved`/`Rejected`/`Cancelled`, and publishing domain events so the relevant approver/requester gets notified at each transition. `RequestType`/`RequestId` is an opaque pointer back to whatever record in the calling module triggered the approval (no FK — the same opaque-cross-module-reference pattern already used by `Organization.Employee.UserId` and `Notifications.Notification.FromUserId`/`ToUserId`).

## Internal Layering

Approval is a **single-project module** (not split Domain/Application/Infrastructure/Api), following the same structural convention as `Identity`/`Notifications`/`Organization` — the template's 4-row project table doesn't apply as-is; adapted to the module's actual two projects:

| Project | Responsibility | Notes |
|---|---|---|
| `Approval.Contracts` | DTOs (`ApprovalRequestDto` — includes `IList<ApprovalStepDto> Steps`, `ApprovalStepDto`), enums (`ApprovalStatus`: `Pending`/`Approved`/`Rejected`/`Cancelled`; `ApprovalStepStatus`: `Pending`/`Approved`/`Rejected`/`Skipped` — `Skipped` currently unreachable, see Notable Conventions), input shapes (`CreateApprovalRequest { RequestType, RequestId, RequesterUserId, RequesterEmployeeId, Title, DeepLinkUrl?, ApproverChain }`, `ApproverStepInput(Level, ApproverUserId, ApproverEmployeeId)`, `DecideApprovalRequest(Approved, Comment?)`, `ApprovalRequestSearchRequest : SearchQuery` with `RequestType?`/`Status?`), the module's own cross-module seam (`Services/IApprovalService`: `CreateAsync`/`DecideAsync`/`CancelAsync`/`GetByRequestAsync`), permission catalog (`Authorization/ApprovalPermissions` — `Group = "approval"`, `Requests.View`/`Requests.ViewAll`; `Authorization/ApprovalPermissionProvider`). Declares `Lightsoft.AspNetCore.Authorization` and `Lightsoft.Result` directly. |
| `Approval.Api` | Single project organized by folder: `Entities/` (`ApprovalRequest`, `ApprovalStep`, both `: AuditableEntity`), `Data/` (`ApprovalDbContext`, `ApprovalContextInitialiser`), `Events/` (`ApprovalStepPendingEvent`, `ApprovalFinalizedEvent`, both `internal sealed record : DomainEvent`) plus `Events/EventHandlers/` (`ApprovalStepPendingEventHandler`, `ApprovalFinalizedEventHandler` — both `INotificationHandler<T>`, each calling `Notifications.Contracts.Services.INotificationService.SendAsync` to push a live notification, see Dependencies), `Services/ApprovalService.cs` (`internal`, implements `IApprovalService` — see below for why this is a deliberate exception to the "no service-class indirection" convention), `Application/Approvals/{Commands,Queries}` (`CreateApprovalRequestCommand`/`DecideApprovalStepCommand` — thin pass-throughs to `IApprovalService`; `GetApprovalRequestByIdQuery`/`SearchApprovalRequestsQuery`/`GetMyPendingApprovalsQuery` — read `ApprovalDbContext` directly, no service indirection), `Controllers/ApprovalController.cs`, `ApprovalModule.cs` (DI: DbContext + `IApprovalService` + permission provider). | `.Api` suffix follows the same convention as the other three modules (future-microservice-extraction candidate, not a fresh deviation). |

**`ApprovalService`/`IApprovalService` is a deliberate exception to `Organization`'s "handlers own their logic directly, no service class" convention** — it exists specifically because it is the module's cross-module DI seam, playing the same role `IUserService` plays for `Identity` and `INotificationService` plays for `Notifications`: another module (a future Leave module, say) must be able to call `CreateAsync`/`DecideAsync`/`CancelAsync`/`GetByRequestAsync` via constructor-injected DI, not via `Mediator.Send` — the mediator commands/queries under `Application/Approvals/` are `internal sealed record`s in `Approval.Api` and are not reachable cross-module at all. So within this module the split is by audience, not a blanket rule: **write-path logic that must be DI-reachable lives in the service** (`CreateAsync`/`DecideAsync`/`CancelAsync`), while **HTTP-only read-path logic lives directly in the query handlers** (`GetApprovalRequestByIdQuery`/`SearchApprovalRequestsQuery`/`GetMyPendingApprovalsQuery`, all reading `ApprovalDbContext` directly via `Mapster.ProjectToType<T>` + `AsNoTracking`) — matching `Organization`'s inlined-handler convention for anything with no cross-module caller.

## Public Contract

`ApprovalController` (route `approval`; `[MustHavePermission(ApprovalPermissions.Requests.View)]` at class level):

| Route | Verb | Permission | Request | Response |
|---|---|---|---|---|
| `api/v{version}/approval/mine` | GET | `approval.requests.view` | Query `PageQuery` | `PagedResult<ApprovalRequestDto>` — pending requests where the caller (`ICurrentUser.UserId`) is the assigned approver for the request's current level |
| `api/v{version}/approval/{id}` | GET | `approval.requests.view` | Route `id` | `Result<ApprovalRequestDto>` |
| `api/v{version}/approval/{id}/decide` | PUT | `approval.requests.view` | `DecideApprovalRequest { Approved, Comment? }` | `Result`; the caller must be the assigned approver for the current step (enforced in `ApprovalService.DecideAsync` by comparing `ICurrentUser.UserId`); approving advances `CurrentLevel` to the next step or finalizes as `Approved` if none remain, rejecting finalizes immediately as `Rejected` |
| `api/v{version}/approval` | GET | `approval.requests.view_all` | Query `ApprovalRequestSearchRequest` (`RequestType?`, `Status?`, plus `SearchQuery`'s `SearchValue`/paging, matched against `Title`) | `PagedResult<ApprovalRequestDto>` — admin "all requests" view |
| `api/v{version}/approval` | POST | `approval.requests.view_all` | `CreateApprovalRequest` | `Result<string>` (new id) — see Notable Conventions: this is an explicitly-documented ad-hoc/admin escape hatch, not the intended integration path for real request types |

## Data Access

`ApprovalDbContext : BaseDbContext`, schema `"approval"`, registered via `Persistence.DbContextExtensions.AddConfiguredDbContext<ApprovalDbContext>(configuration, DbConnectionNames.Approval)`. `DbConnectionNames.Approval` aliases `DbConnectionNames.Default` ("DefaultConnection") — same physical database/connection string as `Identity`/`Notifications`/`Organization`, separated only by schema (`approval`) + table name.

Two tables:

- **`ApprovalRequests`** — composite (non-unique) index on `(RequestType, RequestId)` (the lookup key `GetByRequestAsync` uses to find the approval tied to a source record); separate index on `RequesterUserId`.
- **`ApprovalSteps`** — index on `ApprovalRequestId`; index on `ApproverUserId`; FK to `ApprovalRequest` is `DeleteBehavior.Cascade`.

Both tables are audited via `ApprovalDbContext.SaveChanges[Async]` → `AuditEntries(currentUser.UserId, clock.AuditTime, enableSoftDelete: false)` — neither entity implements `ISoftDelete`, so this is no soft-delete support at all, not a comparable gap to `Identity`'s `User`.

Migrations exist for all three supported providers: `src/Migrations/{MSSQL,PostgreSQL,Sqlite}/Approval/` each hold one baseline `CreateApprovalSchema` migration; each Migrations project's `.csproj` references `Approval.Api` and registers/invokes `ApprovalContextInitialiser` (`MigrateDatabaseAsync` only — no `TrySeedAsync`/seed data, unlike `Organization`'s initialiser) alongside the other three module initialisers in each provider's `Program.cs`. No seed data — the module starts genuinely empty until a consuming feature creates requests through `IApprovalService`.

## Dependencies

| Depends on | Type | Why |
|---|---|---|
| `Shared` | project (`Approval.Contracts → Shared`) | Base `SearchQuery`/`PageQuery` for `ApprovalRequestSearchRequest`/`PageQuery`; `ICurrentUser`/`IDateTime`. |
| `Infrastructure` | project (`Approval.Api → Infrastructure`) | `VersionedApiController`, `AppModule` base class. |
| `Persistence` | project (`Approval.Api → Persistence`) | `BaseDbContext`, `AddConfiguredDbContext`, `AuditEntries`/`ConfigureAuditableEntity`, paging extensions. |
| `Approval.Contracts` | project (`Approval.Api → Approval.Contracts`) | The module's own seam. |
| `Notifications.Contracts` | project (`Approval.Api → Notifications.Contracts`) | The module's **one cross-module business dependency**: `ApprovalStepPendingEventHandler`/`ApprovalFinalizedEventHandler` take an injected `INotificationService` and call `SendAsync(...)` to persist + push a live notification (over SignalR) to the relevant approver when a step becomes pending, or to the requester when the request is finalized. This is a **third instance** of the compliant business-module-to-business-module dependency pattern already established by `Identity.Api → Notifications.Contracts` and `Organization.Api → Identity.Contracts` (see `Identity.md`, `Organization.md`, and `../dependency-graph.md`) — reaches only the `Contracts` seam, never `Notifications.Api`'s internals. |
| Vendor `Lightsoft.AspNetCore.Authorization` (both projects), `Lightsoft.EntityFrameworkCore`, `Lightsoft.Mediator`, `Lightsoft.Result`, `Mapster` (`Approval.Api`) | package, **all declared directly** | Same positive pattern already established by `Organization` — every vendor package the module directly uses is declared directly, no undeclared-transitive-dependency instance here. |

## Depended On By

`StarterKit.WebApi` (composition-root host, wired into `ConfigureExtensions.cs`'s `assemblies` array alongside `IdentityModule`/`NotificationModule`/`OrganizationModule`) and the three Migrations tooling projects (`src/Migrations/{MSSQL,PostgreSQL,Sqlite}`, each referencing `Approval.Api` directly for `ApprovalDbContext`/`ApprovalContextInitialiser`). No other business module references `Approval.Api`/`Approval.Contracts` yet — confirmed via `ProjectReference` search across all `.csproj` files under `src/`. There is **no `tests/Approval.Tests` project** — the module currently has zero automated test coverage (see `../../known-debt.md`). Client-side: `clients/admin/src/modules/approvals/` is the only verified consumer of the module's HTTP surface (an `/approvals` page); not re-inspected in depth as part of this backend-only sync.

## Notable Conventions

- **Approval never resolves approvers itself.** `CreateApprovalRequest.ApproverChain` must already be fully resolved (level, approver user id, approver employee id) by the calling module before it calls `IApprovalService.CreateAsync` — Approval has no knowledge of org structure, employee levels, or any other domain concept a chain might be derived from. This keeps the engine generic enough to back any future request type (leave, expense, etc.), not leave-management-specific, at the cost of pushing all chain-resolution logic onto each caller.
- **`ApprovalStepStatus.Skipped` is defined but no code path ever sets it** — flagged as a currently-unreachable enum value, the same way `Notifications.md` flags `NotificationStatus.Archived`; don't assume a "skip a level" feature exists yet.
- **`ApprovalController`'s `POST approval` endpoint is an explicitly-documented escape hatch, not an oversight.** Its XML-doc comment states it exists for "ad-hoc/admin-triggered requests and exercising the engine directly" — real request types are expected to create their approval request via `IApprovalService` in-process instead, the same way `Identity`'s `UserCreatedEventHandler` and `Organization`'s login commands consume another module's `Contracts` seam in-process rather than over HTTP.
- **No delegate/backup-approver concept.** `ApprovalService.DecideAsync` enforces single-approver-per-step authorization by comparing `ApprovalStep.ApproverUserId` to the caller (`decidedByUserId`, sourced from `ICurrentUser.UserId` in the controller) — if the assigned approver is unavailable, nothing in this module lets anyone else act on their behalf.
- Read-path handlers (`GetApprovalRequestByIdQueryHandler`/`SearchApprovalRequestsQueryHandler`/`GetMyPendingApprovalsQueryHandler`) query `ApprovalDbContext` directly (`Mapster.ProjectToType<T>`, `AsNoTracking`) — matching `Organization`'s "handlers own their logic directly" convention for anything with no cross-module caller; write-path logic instead lives behind `IApprovalService` (see Internal Layering) because it must be DI-reachable from other modules.

## Notes

<!-- manual: content below this line is human-authored and must be preserved verbatim during sync -->

---
_Last synced: 2026-09-05_
