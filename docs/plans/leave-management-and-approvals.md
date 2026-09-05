# Plan — Leave Management + Generic Approval Workflow

Cross-cutting implementation plan spanning `src/` (three new/changed backend modules) and `clients/admin/` (two new UI features). Each phase below is sized to be implemented in one session, following the repo's [code-change workflow gate](../../CLAUDE.md#2-ai-operating-rules) (plan → approval → implement → review, tests/docs are separate follow-up requests). Do not start a phase's implementation without a fresh explicit go-ahead in that session.

## 0. Status

| Phase | Status | Notes |
|---|---|---|
| 1 — Shared Prerequisites (Identity + Notifications) | ✅ Implemented | `employee_id` claim wiring shipped with one deliberate deviation from § 5a below — see § 0a. |
| 2 — Approval Module (generic engine) | ✅ Implemented | Backend + a `POST approval` admin/test-creation endpoint (not in the original design, added to support manual/UI-driven testing) + the admin client's Approvals UI (test-request creation with a multi-level approver chain, and the approve/reject "decide" page) — covers the bulk of § 9 (Phase 5)'s scope ahead of schedule, generically rather than LeaveManagement-specific. |
| 3 — Leave Management Module (backend) | ⬜ Not implemented | Unstarted. |
| 4 — Admin Client: Leave Requests Feature | ⬜ Not implemented | Unstarted; depends on Phase 3. |
| 5 — Admin Client: Approvals Inbox + Notification Wiring | 🟡 Partially covered | The generic inbox/decide UI already exists (built as part of Phase 2, see above). What's still missing is specific to Leave: building the `DeepLinkUrl` that points into a leave request, and wiring `LeaveManagement`'s create handler to set it when calling `IApprovalService.CreateAsync` — both depend on Phase 3 existing first. |
| 6 — Suggested Future Enhancements | ⬜ Backlog, not scheduled | Unchanged — see § 10. |

Also delivered this session, outside this plan's original scope entirely (Organization module employee/department position management — not part of Leave/Approval): `EmployeeOrgUnitMembership` gained `AssignmentType` (`Current`/`Acting`) and `IsManager` fields, a `GET org_unit/{id}/manager` endpoint, and corresponding admin UI (assign/update dialogs, a "View managers" dialog on the department tree). Tracked in `src/docs/architecture/modules/Organization.md`, not here.

### 0a. Deviation from § 5a / § 3: `employee_id` storage mechanism

§ 3 and § 5a below describe `employee_id` as a **denormalized column on `User`**. During implementation this was changed to a **native ASP.NET Identity `UserClaim` row** instead:

- `Identity.Contracts/Services/IUserService.cs` gained a generic `Task<IResult> SetClaimAsync(string userId, string claimType, string? claimValue)` (null clears the claim) rather than a `employee_id`-specific `SetEmployeeIdAsync`.
- No `User.EmployeeId` column/migration was added — `UserManager<User>.AddClaimAsync`/`RemoveClaimAsync`/`ReplaceClaimAsync`/`GetClaimsAsync` already existed and were the idiomatic mechanism (this codebase already had `UserService.GetUsersHasClaimAsync` using the same infra).
- A real latent gap was found and fixed as part of this: `JwtTokenIssuer.GetUserClaimsAsync` never merged `UserManager.GetClaimsAsync(user)` into the issued JWT before — meaning **no** arbitrary per-user claim ever reached a token, not just `employee_id`. Fixed to `.Union(userClaims)`.

Functionally equivalent for every consumer described below (`ICurrentUser.EmployeeId` still resolves the same way via a claim on the principal) — just a schema-free implementation. If a future session revisits Phase 3+, read the actual code (`Shared/Authorization/CurrentUserBase.cs`, `Organization.Api/Application/Employees/Commands/{CreateEmployeeLogin,LinkEmployeeLogin,UnlinkEmployeeLogin}.cs`) rather than trusting § 5a's original column-based description.

## 1. Goal

1. When an `Employee` gets an Identity login (create or link), the login's JWT automatically carries an `employee_id` claim, so any client/API caller can resolve "which employee is this logged-in user" without an extra round trip.
2. A new **generic Approval module** that any future module can plug into: it stores who requested what, a chain of approval levels, and the decision history — without knowing anything about "leave", "expense", etc.
3. A new **Leave Management module** as the first (and for now, only) consumer of Approval: employees create/edit/delete their own leave requests (edit/delete blocked once approved unless the caller has a management permission), and every new request is routed to an approver and triggers a notification.
4. Approver selection rule for leave: the highest-ranked active employee in the requester's department (see § 4 for the exact algorithm and its edge cases).
5. Creating a request raises a domain event that results in an in-app notification to the approver (real-time via the existing SignalR channel).

## 2. Module Map

| Module | Path | New/Changed |
|---|---|---|
| Identity | `src/Identity.Api` + `src/Identity.Contracts` | Changed — `employee_id` claim plumbing |
| Notifications | `src/Notifications.Api` + `src/Notifications.Contracts` | Changed — new cross-module "notify" seam method |
| Organization | `src/Organization.Api` + `src/Organization.Contracts` | Changed — new approver-resolution query |
| **Approval** | `src/Approval.Api` + `src/Approval.Contracts` | **New** — generic approval engine |
| **LeaveManagement** | `src/LeaveManagement.Api` + `src/LeaveManagement.Contracts` | **New** — leave requests |
| Admin client | `clients/admin/src/features/leave-requests`, `.../features/approvals` | **New** |

Dependency direction (all through `.Contracts` seams only, matching the existing `Organization.Api → Identity.Contracts` / `Identity.Api → Notifications.Contracts` pattern — no new boundary-violation shape introduced):

```
LeaveManagement.Api → Approval.Contracts
LeaveManagement.Api → Organization.Contracts   (approver resolution)
LeaveManagement.Api → Identity.Contracts       (resolve current user → employee, if not read from claim)
Approval.Api        → Notifications.Contracts  (notify approver/requester on state change)
Organization.Api    → Identity.Contracts       (already exists, unchanged)
Identity.Api         → Notifications.Contracts (already exists, unchanged)
```

`Approval.Api` deliberately gets **no** dependency on `Organization.Contracts` or `LeaveManagement.Contracts` — it only ever receives an already-resolved list of `(level, approverUserId, approverEmployeeId)` from its caller. That's what keeps it reusable for a future "Expense" or "Overtime" module without any change to Approval itself. *(Confirmed as implemented — `Approval.Api` has no such reference.)*

## 3. Assumptions / Open Questions

These are the judgment calls this plan bakes in. Flag any disagreement before Phase 3 starts (Phases 1–2 don't depend on the answers):

- **Single approval level for Leave, for now.** Approval's data model supports an ordered chain of N levels (so a future 2nd/3rd level — e.g. dept head → HR — is just "populate more rows", no schema change), but Leave itself will populate exactly one level: the department's highest-ranked person. If you actually want multi-level from day one for Leave, say so before Phase 3.
- **Self-approval edge case.** If the requester *is* the highest-ranked active employee in their own primary department (e.g. a department head), the resolver climbs to the parent `OrgUnit` and repeats. If it reaches the top of the hierarchy with no other candidate, request **creation fails** with a clear error rather than silently auto-approving or leaving the request approver-less. Alternative (not implemented unless requested): fall back to `OrgUnit.ManagerEmployeeId` if set.
- **Rejected requests can be resubmitted** — a `Rejected` leave request may still be edited by its owner (treated like `Pending` for the ownership-edit rule); only `Approved` is locked. Confirm if `Rejected` should also be locked.
- ~~**`employee_id` is stored as a denormalized column on `User`**~~ — **superseded, see § 0a.** Implemented as a native ASP.NET Identity `UserClaim` instead.
- **Leave type stays a fixed enum for v1** (`Annual`/`Sick`/`Unpaid`/`Other`), not a per-company configurable lookup like `EmployeeLevel`. Listed under Phase 6 as a natural follow-up if needed.

## 4. Approver Resolution Algorithm (Leave → Organization)

New read-only query exposed from `Organization.Contracts` (consumed by `LeaveManagement.Api`):

```
IOrgDirectoryService.GetTopRankedApproverAsync(employeeId, excludeEmployeeId: employeeId)
```

1. Look up the requester's **primary** active `EmployeeOrgUnitMembership` (`IsPrimary == true`, `EndDate == null`) → its `OrgUnitId`.
2. Among all employees with an active membership in that `OrgUnit`, pick the one with the highest `EmployeeLevel.Rank`, excluding the requester.
3. If none found, walk up via `OrgUnit.ParentId` and repeat step 2 against the parent unit's active members (still excluding the requester).
4. If the top of the hierarchy is reached with no candidate, return "not found" — `LeaveManagement` surfaces this as a creation error (`Result.Failure("No approver could be determined for this employee's department.")`).

This lives in `Organization.Contracts`/`Organization.Api` (it only needs Organization's own data), not in `Approval` or `LeaveManagement` — keeps the org-hierarchy-walking logic with the module that owns the hierarchy.

**Not implemented yet** — this query doesn't exist in `Organization.Api` today. The `IsManager` flag added to `EmployeeOrgUnitMembership` this session (see § 0) could factor into a revised version of this algorithm (e.g. prefer an `IsManager == true` membership over pure `EmployeeLevel.Rank`) — worth revisiting this section's exact algorithm before Phase 3 implementation starts, not just implementing it as originally written.

## 5. Phase 1 — Shared Prerequisites (Identity + Notifications) ✅ Implemented

**Objective:** the two small, independent infra additions everything else depends on.

### 5a. `employee_id` claim end-to-end (Identity)

**Implemented with the deviation noted in § 0a** — read that section first. Original design (kept below for history/context only):

- `Shared/Constants/ClaimTypeConstants.cs` — add `public const string EmployeeId = "employee_id";`. *(Done, as designed.)*
- `Shared/Authorization/CurrentUserBase.cs` / `ICurrentUser` — add `string? EmployeeId => User?.FindFirstValue(ClaimTypeConstants.EmployeeId);` (new extension in `ClaimsPrincipalExtensions`, same pattern as `GetUserId`). *(Done, as designed.)*
- ~~`Identity.Api/Entities/User.cs` — add nullable `EmployeeId` column~~ — **not done this way**, see § 0a.
- `Identity.Api/Jwt/JwtTokenIssuer.GetUserClaimsAsync` — include the claim when non-null. *(Done, but via merging `UserManager.GetClaimsAsync(user)` generically rather than reading a dedicated column — see § 0a.)*
- ~~`Identity.Contracts/Services/IUserService.cs` — add `Task<IResult> SetEmployeeIdAsync(string userId, string? employeeId)`~~ — **implemented as the generic `SetClaimAsync(userId, claimType, claimValue)` instead**, see § 0a.
- ~~New MSSQL/PostgreSQL/Sqlite migration adding the column~~ — **not needed**, no schema change for this part.

### 5b. Wire it into Organization's employee-login commands

- `CreateEmployeeLoginCommandHandler` — after `IUserService.CreateAsync`, call `SetClaimAsync(newUserId, ClaimTypeConstants.EmployeeId, employee.Id)`. *(Done — method name differs from the original `SetEmployeeIdAsync`, see § 0a.)*
- `LinkEmployeeLoginCommandHandler` — same, for the linked user. *(Done.)*
- `UnlinkEmployeeLoginCommandHandler` — same call with `null` to clear. *(Done.)*

### 5c. Notifications cross-module "notify" seam

Today `Notifications.Contracts.INotificationService` only persists; the SignalR push (`IHubService`) is internal to `Notifications.Api`, so no other module can trigger "persist + push" in one call the way `SendNotificationCommandHandler` does internally. Add:

- `Notifications.Contracts/Services/INotificationService.cs` — new `SendAsync` method. *(Done — final signature is `SendAsync(fromUserId, fromName, toUserId, SystemMessage, cancellationToken)`, slightly different shape from the originally-sketched `(fromUserId, toUserId, title, message, url, ct)` but equivalent in effect.)*
- `Notifications.Api/Services/NotificationService.cs` — implement it as save-then-push. *(Done.)*

This is the method `Approval.Api` calls (Phase 2) — it's the only new capability Notifications needed for this whole plan, and it's already been used by a second consumer within Phase 2 itself (the two `Approval` domain-event handlers).

**Definition of done:** ✅ met — creating or linking an employee login produces a JWT containing `employee_id`; unlinking clears it on next login. `IUserService`/`INotificationService` changes compile against existing callers.

## 6. Phase 2 — Approval Module (generic engine) ✅ Implemented

**Objective:** a standalone, testable-via-API module with no leave-specific knowledge.

Implemented per the design below, plus one addition: a `POST approval` endpoint (`approval.requests.view_all` permission) for ad-hoc/admin-triggered request creation and exercising the engine directly outside of a real consumer module — added specifically to support building and testing the admin client's Approvals UI before `LeaveManagement` (the intended real caller of `IApprovalService.CreateAsync` in-process) exists.

### Entities (`Approval.Api/Entities`)

- `ApprovalRequest : AuditableEntity` — `Id`, `RequestType` (string, e.g. `"Leave"`), `RequestId` (opaque id of the source record), `RequesterUserId`, `RequesterEmployeeId`, `Title`, `DeepLinkUrl` (nullable — caller-supplied, so Approval never needs to know a frontend route), `CurrentLevel` (int, 1-based), `Status` (`Pending`/`Approved`/`Rejected`/`Cancelled`), `FinalizedAt`.
- `ApprovalStep : AuditableEntity` — `Id`, `ApprovalRequestId` (FK, cascade), `Level`, `ApproverUserId`, `ApproverEmployeeId`, `Status` (`Pending`/`Approved`/`Rejected`/`Skipped`), `Comment`, `DecidedAt`. Rows are append-only per level — this *is* the history, no separate audit table needed for v1.

### `Approval.Contracts`

- `IApprovalService`:
  - `CreateAsync(CreateApprovalRequest { RequestType, RequestId, RequesterUserId, RequesterEmployeeId, Title, DeepLinkUrl, ApproverChain: IList<ApproverStepInput { Level, ApproverUserId, ApproverEmployeeId }> })` → `Result<string>` (new `ApprovalRequest.Id`).
  - `DecideAsync(approvalRequestId, decidedByUserId, approved: bool, comment)` → `Result` — validates the caller is the assigned approver for the current level and it's still `Pending`; on `approved` at the last level, sets request `Status = Approved`; on any `approved` at a non-last level, advances `CurrentLevel` and fires the "next approver" notification; on rejected, sets `Status = Rejected` immediately.
  - `CancelAsync(approvalRequestId)` → `Result` — used when the source record (e.g. a leave request) is deleted while still pending.
  - `GetByRequestAsync(requestType, requestId)` → `ApprovalRequestDto?` — current status/level/steps, for the owning module to render "pending with X" or block edits.
  - Permission catalog: `approval.requests.view` (view own/assigned), `approval.requests.view_all` (HR/admin — view every request regardless of requester/approver, and the extra `POST approval` test-creation endpoint above).
- Domain events (published by `Approval.Api`, consumed inside `Approval.Api` itself — no other module needs to subscribe): `ApprovalStepPendingEvent` (fired on create and on level-advance) → handler calls `Notifications.Contracts.INotificationService.SendAsync(...)` to notify the current level's approver. `ApprovalFinalizedEvent` (Approved/Rejected/Cancelled) → handler notifies the original requester, using whoever made the final decision as the notification's `fromUserId`.

### Controllers

- `ApprovalController` (route `approval`) — `GET approval/{id}`, `GET approval/mine` (self-service: requests awaiting *my* decision, scoped via `ICurrentUser`, no extra permission — mirrors Notifications' audience-split convention), `PUT approval/{id}/decide`, `GET approval` (admin, `approval.requests.view_all`, paginated search), `POST approval` (admin/test-creation, see note above, same permission).

### Data access

- `ApprovalDbContext : BaseDbContext`, own schema (`"approval"`), same shared-physical-DB convention as the other three modules.
- Migrations for MSSQL/PostgreSQL/Sqlite (baseline only) — generated and applied.

### Internal structure

Follows `Organization`'s resolved pattern for its CQRS handlers (own their `ApprovalDbContext` logic directly) — **with one deliberate exception**: `ApprovalService : IApprovalService` exists as a real service class, because `IApprovalService` is the cross-module seam other modules (future `LeaveManagement`) must call via DI, not via mediator (mediator commands are `internal` and module-local). The CQRS command/query handlers that back the HTTP endpoints are thin wrappers delegating to this service, not the other way around.

**Definition of done:** ✅ met — can create an approval request via API with a hand-built approver chain, decide it, and see a real notification land for the approver via the existing SignalR hub. Also exercised end-to-end through the admin client's Approvals UI (multi-level test-request creation + approve/reject), not just curl/Postman.

## 7. Phase 3 — Leave Management Module (backend) ⬜ Not implemented

**Objective:** the first real consumer of Approval; the actual CRUD + business rules the user described.

### Entities (`LeaveManagement.Api/Entities`)

- `LeaveRequest : AuditableEntity` — `Id`, `UserId`, `EmployeeId`, `LeaveType` (enum: `Annual`/`Sick`/`Unpaid`/`Other`), `StartDate`, `EndDate`, `Reason`, `Status` (`Pending`/`Approved`/`Rejected`/`Cancelled` — kept in sync with the linked `ApprovalRequest`'s status via the `ApprovalFinalizedEvent` handler from Phase 2, or by re-querying `IApprovalService.GetByRequestAsync` on read), `ApprovalRequestId` (opaque ref into Approval).

### `LeaveManagement.Contracts`

- DTOs/requests: `LeaveRequestDto`, `CreateLeaveRequest`, `UpdateLeaveRequest`, `LeaveRequestSearchRequest : SearchQuery`.
- Permission catalog: `leave.requests.view`, `.create`, `.update`, `.delete` (own, pending-only — see rule below), `.manage` (bypasses the ownership+status restriction entirely — HR/admin).

### Business rules (`Application/LeaveRequests/Commands`)

- **Create**: `UserId`/`EmployeeId` taken from `ICurrentUser` (`EmployeeId` claim from Phase 1 — fail fast with a clear error if the caller has no linked employee). Resolve approver via `Organization.Contracts.IOrgDirectoryService.GetTopRankedApproverAsync` (§ 4 — **revisit this algorithm's exact shape before implementing, given the new `IsManager` flag, see § 4's note**); on failure, reject creation. Call `Approval.Contracts.IApprovalService.CreateAsync` with a single-level chain, store the returned id on `ApprovalRequestId`, set `Status = Pending`.
- **Update/Delete**: `leave.requests.manage` → always allowed. Otherwise: allowed only when `LeaveRequest.UserId == currentUser.UserId` **and** `Status` is `Pending` or `Rejected` (see § 3 assumption on resubmission) — `Approved` is always locked for non-management callers. Delete of a still-`Pending` request also calls `IApprovalService.CancelAsync`.
- **Read**: `leave.requests.view` for one's own; a search/list endpoint additionally supports `leave.requests.manage` (or a `.view_all`, naming TBD at implementation time) to see everyone's.

### Controllers

`LeaveRequestController` (route `leave_request`) — standard CRUD + `GET leave_request/search` (paginated, own vs. all gated by permission as above), following the Organization module's route/permission-table documentation style.

### Data access

`LeaveManagementDbContext`, own schema, same shared-DB convention. Index on `(EmployeeId, Status)` for the common "my pending requests" query. Migrations for all three providers.

### Internal structure

Same as Approval — handlers own `LeaveManagementDbContext` logic directly, no service-class layer (unless a future cross-module consumer of `LeaveManagement.Contracts` needs one, mirroring Approval's own exception in § 6).

**Definition of done:** an employee (with `employee_id` on their JWT) can create a leave request via API, it lands as a pending approval for the correct department head, that person can approve/reject it via Phase 2's endpoints, and the leave request's own status reflects the outcome. Non-management users get `403`/`Forbidden`-equivalent `Result.Failure` attempting to edit someone else's request or their own already-approved one.

## 8. Phase 4 — Admin Client: Leave Requests Feature ⬜ Not implemented

**Objective:** UI for what Phase 3 exposes, following `clients/admin`'s established conventions (one `<feature>.api.ts` per feature, actions as separate `*-action.ts` files, cross-feature imports through `index.ts` barrels, and — per the codebase's current structure — placed under `src/modules/<domain>/leave-requests/` rather than a top-level `features/` folder; confirm the right domain grouping, e.g. a new `leave-management` domain vs. nesting under `organization`, before starting).

- `<domain>/leave-requests/api/leave-requests.api.ts` — get/create/update/delete, ordered per the admin API-file convention.
- List page: table with status badge, filters (own vs. all if permitted), create/edit form (date range, type, reason).
- Edit/delete actions disabled client-side (and still enforced server-side) once `Status === "Approved"` for non-management users.
- Permission-gated nav entry, same pattern as existing nav items.

**Definition of done:** a logged-in employee can see, create, and edit their own pending leave requests through the UI; an HR/admin role sees everyone's and can override.

## 9. Phase 5 — Admin Client: Approvals Inbox + Notification Wiring 🟡 Partially covered

**Objective:** the approver-facing side, plus closing the loop on the notification's deep link.

- ~~`features/approvals/api/approvals.api.ts` — "my pending approvals" list, decide (approve/reject with comment) action.~~ **Already implemented** (ahead of schedule, as part of Phase 2) — see `src/modules/approvals/` in the admin client: a `/approvals` page with a "Waiting on your decision" list + approve/reject dialog, and (for `view_all`) an admin view with a multi-level test-request creation dialog. Built generically, not Leave-specific — good foundation to reuse once Leave exists, but nothing here builds a Leave-specific deep link yet.
- Still missing: wiring `LeaveManagement`'s create handler (Phase 3) to build a real `DeepLinkUrl` (e.g. `/leave-requests/{id}`) when calling `IApprovalService.CreateAsync`, so a notification's click-through actually lands somewhere Leave-specific instead of nowhere/generic.

**Definition of done:** creating a leave request produces a real-time notification (bell + toast, matching existing Notifications UX) for the resolved approver, and clicking it navigates to a page where they can approve/reject. *(The approve/reject page itself is done; the leave-request-specific notification deep link is not, since Phase 3 doesn't exist yet.)*

## 10. Phase 6 — Suggested Future Enhancements (backlog, not scheduled)

Raised by the user's "propose related features" ask — listed for later prioritization, not part of the phases above:

- **Multi-level chains for Leave itself** (e.g. dept head → division head → HR) — the Approval engine already supports this; only `LeaveManagement`'s chain-building logic would need to grow.
- **Leave balance / quota tracking** per employee per year/leave-type, with creation blocked or flagged when a request would exceed the remaining balance.
- **Delegate approver** when the resolved approver is themselves on leave/OOO — needs a "delegation" concept on top of Approval's `ApprovalStep.ApproverUserId`.
- **Escalation / timeout** — auto-escalate to the next level up if a step sits `Pending` past N days.
- **Holiday-aware day counting** — `EndDate - StartDate` currently counts calendar days; a company holiday calendar would let it count business days.
- **Attachments** on a leave request (e.g. medical certificate for sick leave).
- **Email notification in addition to in-app** — reuse the existing `Notifications.Contracts.IMailService` (already used by Identity's welcome-email flow) alongside `SendAsync` for users who are offline when the push happens.
- **Generalize Approval to a second request type** (e.g. Expense or Overtime) once one actually exists — validates the "generic module" design decision made in Phase 2 was worth it, rather than assuming it in advance.
- **Approval history / audit view** in the admin UI — a read-only timeline of every `ApprovalStep` decision for a given request, useful for HR review.
- **Reporting dashboard** — leave taken per employee/department/period, pending-approval aging report.

---

_Plan created 2026-09-05. Phases 1–2 implemented 2026-09-05/06 (see § 0 for status and § 0a for the one deviation). Phases 3–6 not yet implemented — confirm § 3's remaining open assumptions and § 4's algorithm revision note before Phase 3 begins._
