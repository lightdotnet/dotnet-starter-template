# Module Overview: Approval

## Purpose

A generic, reusable multi-level approval-request engine — not tied to any specific request type. A consuming module (e.g. a future Leave module) resolves the full approver chain itself (Approval has no knowledge of org structure, employee levels, or who should approve what) and calls `IApprovalService.CreateAsync` with an already-resolved `ApproverChain`. Approval then owns the generic mechanics: one `ApprovalRequest` per source record, a `Level`-ordered chain of `ApprovalStep`s, advancing `CurrentLevel` as each step is approved, finalizing the request as `Approved`/`Rejected`/`Cancelled`, and publishing domain events so the relevant approver/requester gets notified at each transition. `RequestType`/`RequestId` is an opaque pointer back to whatever record in the calling module triggered the approval (no FK — the same opaque-cross-module-reference pattern already used by `Organization.Employee.UserId` and `Notifications.Notification.FromUserId`/`ToUserId`).

Because Approval cannot see identity or organization data, the calling side also passes the **display labels** it wants shown — `RequesterName` on the request, `ApproverName` on each `ApproverStepInput` — and the module stores them verbatim (see Notable Conventions, and `known-debt.md` D5 for the staleness trade-off). Optionally a request carries an admin-managed **document type** tag (`DocumentTypeId`) and a **deep link** (`DeepLinkUrl`) that the notification handlers use as the click target.

## Internal Layering

Approval is a **single-project module** (not split Domain/Application/Infrastructure/Api), following the same structural convention as `Identity`/`Notifications`/`Organization` — the template's 4-row project table doesn't apply as-is; adapted to the module's actual two projects:

| Project | Responsibility | Notes |
|---|---|---|
| `Approval.Contracts` | DTOs (`Approvals/ApprovalRequestDto` — includes `IList<ApprovalStepDto> Steps`, `RequesterName?`, `RequesterEmployeeId?`, `DocumentTypeId?`/`DocumentTypeName?`; `Approvals/ApprovalStepDto` — includes `ApproverName?`; `DocumentTypes/ApprovalDocumentTypeDto`), enums (`Approvals/ApprovalStatus`: `Pending`/`Approved`/`Rejected`/`Cancelled`; `Approvals/ApprovalStepStatus`: `Pending`/`Approved`/`Rejected`/`Skipped` — `Skipped` currently unreachable, see Notable Conventions; `Approvals/ApprovalRelation`: `All`/`Requested`/`AwaitingMyDecision`/`DecidedByMe`), input shapes (`Approvals/CreateApprovalRequest { RequestType, RequestId, RequesterUserId, RequesterEmployeeId?, RequesterName?, Title, Content?, DeepLinkUrl?, DocumentTypeId?, ApproverChain }`, `Approvals/ApproverStepInput(Level, ApproverUserId, ApproverEmployeeId, ApproverName?)`, `Approvals/DecideApprovalRequest(Approved, Comment?)`, `Approvals/ApprovalRequestSearchRequest : SearchQuery` with `RequestType?`/`Status?`, `Approvals/MyApprovalRequestSearchRequest : SearchQuery` with `Relation`/`RequestType?`/`Status?`, `DocumentTypes/CreateApprovalDocumentTypeRequest(Name, Code, Description?, IsActive)`), the module's own cross-module seam (`Services/IApprovalService`: `CreateAsync`/`DecideAsync`/`CancelAsync`/`GetByRequestAsync`), permission catalog (`Authorization/ApprovalPermissions` — `Group = "approval"`, `Requests.ViewAll` (**admin-only**; the self-service surface has no permission, just authentication), `DocumentTypes.{View,Create,Update,Delete}`; `Authorization/ApprovalPermissionProvider`). Declares `Lightsoft.AspNetCore.Authorization` and `Lightsoft.Result` directly. |
| `Approval.Api` | Single project organized by folder: `Domain/Approvals/` (`ApprovalRequest`, `ApprovalStep`, `ApprovalDocumentType`, all `: AuditableEntity`; `ApprovalRequestByIdSpec`; the two domain events `ApprovalStepPendingEvent`/`ApprovalFinalizedEvent`, both `internal sealed record : DomainEvent`), `Data/` (`ApprovalDbContext`, `ApprovalContextInitialiser`), `Application/Approvals/{Commands,Queries,EventHandlers}` — `Commands`: `CreateApprovalRequestCommand`/`DecideApprovalStepCommand` (thin pass-throughs to `IApprovalService`); `Queries`: `GetApprovalRequestByIdQuery`/`SearchApprovalRequestsQuery` (admin), `GetMyApprovalRequestByIdQuery`/`SearchMyApprovalsQuery` (self-service, relation-scoped) — read `ApprovalDbContext` directly, no service indirection; `EventHandlers`: `ApprovalStepPendingEventHandler`/`ApprovalFinalizedEventHandler` (both `INotificationHandler<T>`, each calling `Notifications.Contracts.Services.INotificationService.SendAsync` — see Dependencies) plus `ApprovalDeepLink` (static helper building the `/approvals/requests/{id}` notification click target). `Application/DocumentTypes/{Commands,Queries}` — full CRUD for the document-type catalog. `Services/ApprovalService.cs` (`internal`, implements `IApprovalService` — see below for why this is a deliberate exception). `Controllers/` (`ApprovalController`, `UserApprovalController`, `ApprovalDocumentTypeController`). `ApprovalModule.cs` (DI: DbContext + `IApprovalService` + permission provider). | `.Api` suffix follows the same convention as the other three modules (future-microservice-extraction candidate, not a fresh deviation). |

**`ApprovalService`/`IApprovalService` is a deliberate exception to `Organization`'s "handlers own their logic directly, no service class" convention** — it exists specifically because it is the module's cross-module DI seam, playing the same role `IUserService` plays for `Identity` and `INotificationService` plays for `Notifications`: another module (a future Leave module, say) must be able to call `CreateAsync`/`DecideAsync`/`CancelAsync`/`GetByRequestAsync` via constructor-injected DI, not via `Mediator.Send` — the mediator commands/queries under `Application/` are `internal sealed record`s in `Approval.Api` and are not reachable cross-module at all. So within this module the split is by audience, not a blanket rule: **write-path logic that must be DI-reachable lives in the service** (`CreateAsync`/`DecideAsync`/`CancelAsync`), while **HTTP-only read-path logic and the document-type CRUD live directly in the handlers** (all reading `ApprovalDbContext` directly via `Mapster.ProjectToType<T>`/`Adapt<T>` + `AsNoTracking`) — matching `Organization`'s inlined-handler convention for anything with no cross-module caller.

## Public Contract

### `ApprovalController` — admin surface (route `approval`, `[MustHavePermission(ApprovalPermissions.Requests.ViewAll)]` at class level)

| Route | Verb | Request | Response |
|---|---|---|---|
| `api/v{version}/approval` | GET | Query `ApprovalRequestSearchRequest` (`RequestType?`, `Status?`, plus `SearchQuery` paging/`SearchValue` matched against `Title`) | `PagedResult<ApprovalRequestDto>` — every request, unrestricted |
| `api/v{version}/approval/{id}` | GET | Route `id` | `Result<ApprovalRequestDto>` |
| `api/v{version}/approval` | POST | `CreateApprovalRequest` | `Result<string>` (new id) — ad-hoc/admin escape hatch; trusts the caller-supplied requester + chain as-is (see Notable Conventions) |

### `UserApprovalController` — self-service surface (route `user_approval`, authentication only, **no permission gate**)

Every query and command is scoped server-side to requests the current user is related to (as requester or as an approver on any step); "not found" and "not related" return the same `NotFound` so a user can't probe for requests they have no relation to.

| Route | Verb | Request | Response |
|---|---|---|---|
| `api/v{version}/user_approval` | GET | Query `MyApprovalRequestSearchRequest` (`Relation` = `All`/`Requested`/`AwaitingMyDecision`/`DecidedByMe`, `RequestType?`, `Status?`, paging) | `PagedResult<ApprovalRequestDto>` |
| `api/v{version}/user_approval/{id}` | GET | Route `id` | `Result<ApprovalRequestDto>` — 404 unless the caller is the requester or an approver on some step |
| `api/v{version}/user_approval/{id}/decide` | PUT | `DecideApprovalRequest { Approved, Comment? }` | `Result`; the caller must be the assigned approver for the current step (enforced in `ApprovalService.DecideAsync`); approving advances `CurrentLevel` or finalizes as `Approved`, rejecting finalizes immediately as `Rejected` (a reason is required) |
| `api/v{version}/user_approval` | POST | `CreateApprovalRequest` | `Result<string>` (new id) — `RequesterUserId` and `RequesterEmployeeId` are overridden server-side from the caller's identity + `employee_id` claim; `RequesterName` is kept as the client sent it (the JWT carries no name claim — same pattern as Notifications' caller-supplied `fromName`) |

### `ApprovalDocumentTypeController` — document-type catalog (route `approval_document_type`)

A small admin-managed reference list a request can optionally be tagged with. **The list read is available to any authenticated user** (it feeds the create-request document-type picker); the single-record read and every write are permission-gated.

| Route | Verb | Permission | Request | Response |
|---|---|---|---|---|
| `api/v{version}/approval_document_type` | GET | *(auth only)* | Query `activeOnly?` | `Result<IList<ApprovalDocumentTypeDto>>`, ordered by `Name` |
| `api/v{version}/approval_document_type/{id}` | GET | `approval.document_types.view` | Route `id` | `Result<ApprovalDocumentTypeDto>` |
| `api/v{version}/approval_document_type` | POST | `approval.document_types.create` | `CreateApprovalDocumentTypeRequest` | `Result<string>` (new id); rejects a duplicate `Code` |
| `api/v{version}/approval_document_type/{id}` | PUT | `approval.document_types.update` | `ApprovalDocumentTypeDto` (route/body id must match) | `Result` |
| `api/v{version}/approval_document_type/{id}` | DELETE | `approval.document_types.delete` | Route `id` | `Result`; blocked (pre-check **and** an FK-violation catch) if any `ApprovalRequest` still references the type |

## Data Access

`ApprovalDbContext : BaseDbContext`, schema `"approval"`, registered via `Persistence.DbContextExtensions.AddConfiguredDbContext<ApprovalDbContext>(configuration, DbConnectionNames.Approval)`. `DbConnectionNames.Approval` aliases `DbConnectionNames.Default` ("DefaultConnection") — same physical database/connection string as `Identity`/`Notifications`/`Organization`, separated only by schema (`approval`) + table name.

Three tables:

- **`ApprovalRequests`** — composite (non-unique) index on `(RequestType, RequestId)` (the lookup key `GetByRequestAsync` uses); indexes on `RequesterUserId` and `DocumentTypeId`. `RequesterEmployeeId` and `RequesterName` are nullable `MaxLength` string columns (opaque bookkeeping / display label). `DocumentTypeId` is a nullable FK to `ApprovalDocumentTypes` with `DeleteBehavior.Restrict`.
- **`ApprovalSteps`** — index on `ApprovalRequestId`; index on `ApproverUserId`; `ApproverName` is a nullable `MaxLength(256)` display-label column; FK to `ApprovalRequest` is `DeleteBehavior.Cascade`.
- **`ApprovalDocumentTypes`** — unique index on `Code`; `Name`/`Code`/`Description` length-capped; `IsActive` defaults true.

All three are audited via `ApprovalDbContext.SaveChanges[Async]` → `AuditEntries(currentUser.UserId, clock.AuditTime, enableSoftDelete: false)` — no entity implements `ISoftDelete`, so this is no soft-delete support at all, not a comparable gap to `Identity`'s `User`.

`GetByRequestAsync` orders `(RequestType, RequestId)` matches by `Created` desc and takes the first rather than assuming uniqueness (a caller can legitimately re-raise a request for the same source record after a rejection — see `known-debt.md`).

Migrations exist for all three supported providers: `src/Migrations/{MSSQL,PostgreSQL,Sqlite}/Approval/` each hold **one baseline `CreateApprovalSchema` migration** (the incremental history was squashed 2026-09-06 — this is a template repo with disposable dev databases, so the schema is expressed as a single `CreateTable` set rather than an `AlterColumn`/table-rebuild chain). Each Migrations project's `.csproj` references `Approval.Api` and registers/invokes `ApprovalContextInitialiser` (`MigrateDatabaseAsync` only — no `TrySeedAsync`/seed data, unlike `Organization`'s initialiser) alongside the other three module initialisers in each provider's `Program.cs`. The module starts genuinely empty until a consuming feature creates requests through `IApprovalService`.

## Dependencies

| Depends on | Type | Why |
|---|---|---|
| `Shared` | project (`Approval.Contracts → Shared`) | Base `SearchQuery`/`PageQuery`; `ICurrentUser`/`IDateTime`; `ClaimsPrincipalExtensions` (`GetEmployeeId`) used by `UserApprovalController`. |
| `Infrastructure` | project (`Approval.Api → Infrastructure`) | `VersionedApiController`, `AppModule` base class. |
| `Persistence` | project (`Approval.Api → Persistence`) | `BaseDbContext`, `AddConfiguredDbContext`, `AuditEntries`/`ConfigureAuditableEntity`, paging extensions, `DbUpdateExceptionExtensions` (FK-violation catch in `DeleteApprovalDocumentType`). |
| `Approval.Contracts` | project (`Approval.Api → Approval.Contracts`) | The module's own seam. |
| `Notifications.Contracts` | project (`Approval.Api → Notifications.Contracts`) | The module's **one cross-module business dependency**: `ApprovalStepPendingEventHandler`/`ApprovalFinalizedEventHandler` take an injected `INotificationService` and call `SendAsync(...)` to persist + push a live notification to the relevant approver / requester on each transition. The notification's `Url` is `DeepLinkUrl ?? ApprovalDeepLink.RequestDetail(id)` — a client deep link to the request detail page. This is a **third instance** of the compliant business-module-to-business-module dependency pattern established by `Identity.Api → Notifications.Contracts` and `Organization.Api → Identity.Contracts` (see `../dependency-graph.md`) — reaches only the `Contracts` seam, never `Notifications.Api`'s internals. |
| Vendor `Lightsoft.AspNetCore.Authorization` (both projects), `Lightsoft.EntityFrameworkCore`, `Lightsoft.Mediator`, `Lightsoft.Result`, `Mapster` (`Approval.Api`) | package, **all declared directly** | Same positive pattern already established by `Organization` — no undeclared-transitive-dependency instance here. |

## Depended On By

`StarterKit.WebApi` (composition-root host, wired into `ConfigureExtensions.cs`'s `assemblies` array alongside `IdentityModule`/`NotificationModule`/`OrganizationModule`), the three Migrations tooling projects (`src/Migrations/{MSSQL,PostgreSQL,Sqlite}`, each referencing `Approval.Api` directly for `ApprovalDbContext`/`ApprovalContextInitialiser`), and `tests/Approval.Tests` (`Approval.Api.csproj` grants `InternalsVisibleTo` so the test project can reach the `internal` command/query records, handlers, `ApprovalService`, and domain types directly). No other business module references `Approval.Api`/`Approval.Contracts` yet — confirmed via `ProjectReference` search across all `.csproj` files under `src/`. Client-side: `clients/admin/src/modules/approvals/` is the verified consumer of the module's HTTP surface (self-service + admin request lists, a `/approvals/requests/{id}` detail page with an inline decide action, and the document-type catalog under `/approvals/document-types`).

## Notable Conventions

- **Approval never resolves approvers — or their display names — itself.** `CreateApprovalRequest.ApproverChain` must already be fully resolved by the calling module: level, approver user id, approver employee id, **and the display label** (`ApproverName`). Likewise `RequesterName` on the request. Approval has no knowledge of org structure, employee levels, or identity data, so it stores these verbatim. The client's linked-employee approver picker supplies `${firstName} ${lastName}` (falling back to the employee code); `UserApprovalController` keeps the client-supplied `RequesterName` because the JWT has no name claim. Trade-off: labels do not update if the underlying user/employee is renamed (`known-debt.md` D5).
- **`ApprovalStepStatus.Skipped` is defined but no code path ever sets it** — flagged as a currently-unreachable enum value, the same way `Notifications.md` flags `NotificationStatus.Archived`.
- **`ApprovalController`'s `POST approval` endpoint is an explicitly-documented escape hatch, not an oversight.** Real request types are expected to create their approval request via `IApprovalService` in-process instead. Unlike `UserApprovalController.PostAsync`, it trusts the caller's chosen requester/approver chain as-is.
- **The self-service surface (`UserApprovalController`) has no permission, only authentication** — mirroring `UserNotificationController`. Authorization is entirely relation-based (requester or approver on some step), enforced server-side in every query/command. `ApprovalPermissions.Requests` therefore only carries `ViewAll` (the admin surface); there is no `Requests.View`.
- **No delegate/backup-approver concept.** `ApprovalService.DecideAsync` enforces single-approver-per-step authorization by comparing `ApprovalStep.ApproverUserId` to the caller — if the assigned approver is unavailable, nothing in this module lets anyone else act on their behalf.
- **`GET approval_document_type` (list) is intentionally ungated** so the create-request document-type picker can populate for any authenticated user; the by-id read and every write stay permission-gated. Same "a small reference list feeds a picker" reasoning as `GET employee/search` in `Organization`.
- Read-path handlers query `ApprovalDbContext` directly (`Mapster.ProjectToType<T>` for the `ProjectTo` queries, `.Adapt<T>()` after an `.Include` for `GetMyApprovalRequestByIdQueryHandler`, always `AsNoTracking`) — matching `Organization`'s "handlers own their logic directly" convention; write-path logic instead lives behind `IApprovalService` because it must be DI-reachable from other modules.

## Notes

<!-- manual: content below this line is human-authored and must be preserved verbatim during sync -->

---
_Last synced: 2026-09-06_
