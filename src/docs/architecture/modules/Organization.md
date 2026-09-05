# Module Overview: Organization

## Purpose

Owns companies, a department/team hierarchy, configurable per-company employee levels, and employees — including their membership history in that hierarchy and, optionally, a link to an Identity login. A `Company` scopes everything below it (org units, levels, employees are never shared across companies); an `OrgUnit` is a single self-referencing entity that unifies what would otherwise be separate `Department`/`Team` entities via a `Type` discriminator; `EmployeeLevel` is a configurable lookup per company (not a hardcoded enum), letting each company define its own level ladder and ranking; `EmployeeOrgUnitMembership` is the Employee↔OrgUnit join, carrying the level, primary-membership flag, and a start/end date pair that models membership history (`EndDate == null` means currently active) rather than a simple many-to-many.

## Internal Layering

Organization is a **single-project module** (not split Domain/Application/Infrastructure/Api), following the same structural convention as `Identity`/`Notifications` — the template's 4-row project table doesn't apply as-is; adapted to the module's actual two projects:

| Project | Responsibility | Notes |
|---|---|---|
| `Organization.Contracts` | DTOs, requests, enums, and the permission catalog, organized into **per-feature subfolders** — `Common/` (`OrganizationStatus`: `Active`/`Inactive`), `Companies/` (`CompanyDto`, `CreateCompanyRequest`, `CompanySearchRequest : SearchQuery`), `OrgUnits/` (`OrgUnitType`: `Department`/`Team`, `OrgUnitDto`, `CreateOrgUnitRequest`, `MoveOrgUnitRequest`, `OrgUnitTreeNodeDto`), `EmployeeLevels/` (`EmployeeLevelDto`, `CreateEmployeeLevelRequest`), `Employees/` (`EmploymentStatus`: `Active`/`OnLeave`/`Terminated`, `EmployeeDto`, `EmployeeMembershipDto`, `CreateEmployeeRequest`, `EmployeeSearchRequest : SearchQuery`, `AssignEmployeeOrgUnitRequest`, `UpdateEmployeeMembershipRequest`, `CreateEmployeeLoginRequest`, `LinkEmployeeLoginRequest`), `Authorization/` (`OrganizationPermissions`, `OrganizationPermissionProvider`). Each subfolder has its own namespace suffix (e.g. `StarterKit.Organization.Contracts.Companies`) — a finer-grained split than `Identity.Contracts`/`Notifications.Contracts`'s flatter layout. Declares `Lightsoft.AspNetCore.Authorization` directly (see Notable Conventions). |
| `Organization.Api` | Single project organized by folder: `Entities/` (`Company`, `OrgUnit`, `EmployeeLevel`, `Employee`, `EmployeeOrgUnitMembership`), `Data/` (`OrganizationDbContext`, `OrganizationContextInitialiser`), `Application/{Companies,OrgUnits,EmployeeLevels,Employees}/{Commands,Queries}` (every handler owns its business logic directly against `OrganizationDbContext` — no service-class indirection, see Notable Conventions), `Controllers/` (`CompanyController`, `OrgUnitController`, `EmployeeLevelController`, `EmployeeController`), `OrganizationModule.cs` (DI: DbContext + permission provider). | `.Api` suffix follows the same convention as `Identity.Api`/`Notifications.Api` (future-microservice-extraction candidate, not a fresh deviation). |

## Public Contract

`CompanyController` (route `company`, `[MustHavePermission(OrganizationPermissions.Companies.View)]` at class level):

| Route | Verb | Permission | Request | Response |
|---|---|---|---|---|
| `api/v{version}/company` | GET | `organization.companies.view` | Query `CompanySearchRequest` (`SearchValue`, `Status?`, `PageNumber`/`PageSize`) | `PagedResult<CompanyDto>` |
| `api/v{version}/company/{id}` | GET | `organization.companies.view` | Route `id` | `Result<CompanyDto>` |
| `api/v{version}/company` | POST | `organization.companies.create` | `CreateCompanyRequest` | `Result<string>` (new id); rejects a duplicate `Code` |
| `api/v{version}/company/{id}` | PUT | `organization.companies.update` | `CompanyDto` (route/body id must match) | `Result`; rejects a `Code` collision with another company |
| `api/v{version}/company/{id}` | DELETE | `organization.companies.delete` | Route `id` | `Result`; blocked if the company still has org units or employees |

`OrgUnitController` (explicit route override `api/v{version}/org_unit`, same "explicit override for readability" convention as Identity's `UserProfileController`; `[MustHavePermission(OrganizationPermissions.OrgUnits.View)]` at class level):

| Route | Verb | Permission | Request | Response |
|---|---|---|---|---|
| `org_unit/company/{companyId}/tree` | GET | `organization.org_units.view` | Route `companyId` | `IList<OrgUnitTreeNodeDto>` — flat table loaded once, assembled into a tree in memory by grouping on `ParentId` |
| `org_unit/{id}` | GET | `organization.org_units.view` | Route `id` | `Result<OrgUnitDto>` |
| `org_unit/{id}/employee` | GET | `organization.org_units.view` | Route `id` | `IList<EmployeeDto>` — only employees with an **active** membership (`EndDate == null`) in that unit |
| `org_unit` | POST | `organization.org_units.create` | `CreateOrgUnitRequest` | `Result<string>`; validates the company exists, a given parent exists and belongs to the same company, and `Code` is unique within the company |
| `org_unit/{id}` | PUT | `organization.org_units.update` | `OrgUnitDto` (route/body id must match) | `Result`; rejects a `Code` collision within the same company |
| `org_unit/{id}/move` | PUT | `organization.org_units.update` | `MoveOrgUnitRequest { NewParentId }` | `Result`; guards: can't parent to itself, new parent must exist in the same company, and can't move under one of its own descendants (walked iteratively via `ParentId`, one round-trip per ancestor level) |
| `org_unit/{id}` | DELETE | `organization.org_units.delete` | Route `id` | `Result`; blocked if the unit has child units or any **active** membership — see Data Access for what happens to already-ended memberships |

`EmployeeLevelController` (explicit route override `api/v{version}/employee_level`; `[MustHavePermission(OrganizationPermissions.EmployeeLevels.View)]` at class level):

| Route | Verb | Permission | Request | Response |
|---|---|---|---|---|
| `employee_level/company/{companyId}` | GET | `organization.employee_levels.view` | Route `companyId` | `IList<EmployeeLevelDto>`, ordered by `Rank` |
| `employee_level` | POST | `organization.employee_levels.create` | `CreateEmployeeLevelRequest` | `Result<string>`; validates the company exists and `Code` is unique within it |
| `employee_level/{id}` | PUT | `organization.employee_levels.update` | `EmployeeLevelDto` (route/body id must match) | `Result`; rejects a `Code` collision within the same company |
| `employee_level/{id}` | DELETE | `organization.employee_levels.delete` | Route `id` | `Result` — **no dependent-check guard**, unlike Company/OrgUnit delete (see Notable Conventions) |

`EmployeeController` (route `employee`; `[MustHavePermission(OrganizationPermissions.Employees.View)]` at class level):

| Route | Verb | Permission | Request | Response |
|---|---|---|---|---|
| `employee/search` | GET | `organization.employees.view` | Query `EmployeeSearchRequest` (`CompanyId?`, `OrgUnitId?`, `EmploymentStatus?`, `SearchValue` over `FirstName`/`LastName`/`EmployeeCode`, paging) | `PagedResult<EmployeeDto>`; `OrgUnitId` filters via an `Any` subquery against active memberships |
| `employee/{id}` | GET | `organization.employees.view` | Route `id` | `Result<EmployeeDto>` — `Memberships` is populated from active memberships only, with `OrgUnitName`/`LevelName` resolved live via navigation (see Data Access) |
| `employee` | POST | `organization.employees.create` | `CreateEmployeeRequest` | `Result<string>`; validates the company exists and `EmployeeCode` is unique within it |
| `employee/{id}` | PUT | `organization.employees.update` | `EmployeeDto` (route/body id must match) | `Result`; rejects an `EmployeeCode` collision within the same company |
| `employee/{id}` | DELETE | `organization.employees.delete` | Route `id` | `Result` — **no dependent-check guard**; membership rows cascade-delete along with the employee (see Notable Conventions) |
| `employee/{id}/org_unit` | POST | `organization.employees.update` | `AssignEmployeeOrgUnitRequest { OrgUnitId, LevelId?, IsPrimary }` | `Result`; validates the org unit belongs to the employee's company, an optional level belongs to that company, and the employee isn't already actively assigned to that unit; setting `IsPrimary` clears any other active primary membership first |
| `employee/{id}/org_unit/{orgUnitId}` | PUT | `organization.employees.update` | `UpdateEmployeeMembershipRequest { LevelId?, IsPrimary }` | `Result`; requires an active membership to exist; re-`IsPrimary` clears other active primaries the same way |
| `employee/{id}/org_unit/{orgUnitId}` | DELETE | `organization.employees.update` | Route params | `Result` — **ends** the active membership (`EndDate = now`, `IsPrimary = false`) rather than deleting the row, preserving history |
| `employee/{id}/login` | POST | `organization.employees.manage_login` | `CreateEmployeeLoginRequest { UserName, Password?, Email?, PhoneNumber? }` | `Result<string>` (new Identity user id); creates a new Identity login via `IUserService.CreateAsync` and stores its id on `Employee.UserId`; rejects if the employee already has a login |
| `employee/{id}/login` | PUT | `organization.employees.manage_login` | `LinkEmployeeLoginRequest { UserId }` | `Result`; links an existing Identity user (verified via `IUserService.GetByIdAsync`) to the employee; rejects if the employee already has a login or the target user is already linked to a different employee |
| `employee/{id}/login` | DELETE | `organization.employees.manage_login` | Route `id` | `Result`; clears `Employee.UserId` — does **not** delete or deactivate the underlying Identity user |

`Employees.ManageLogin` is a separate, more sensitive permission from `Employees.Update`, gating only the three login-management actions above.

Every action across all four controllers dispatches a mediator command/query under `Application/{Companies,OrgUnits,EmployeeLevels,Employees}/{Commands,Queries}` — see Notable Conventions for how this differs from `Identity`/`Notifications`.

## Data Access

`OrganizationDbContext : BaseDbContext`, schema `"organization"`, registered via `Persistence.DbContextExtensions.AddConfiguredDbContext<OrganizationDbContext>(configuration, DbConnectionNames.Organization)`. `DbConnectionNames.Organization` aliases `DbConnectionNames.Default` ("DefaultConnection") — same physical database/connection string as `Identity` and `Notifications`, separated only by schema (`organization`) + table name.

Five tables:

- **`Companies`** — unique index on `Code`.
- **`OrgUnits`** — composite (non-unique) index on `(CompanyId, ParentId)`; self-referencing `Parent`/`Children` FK (`ParentId`) is `DeleteBehavior.Restrict`; FK to `Company` (`CompanyId`, no navigation property) is `Restrict`. `ManagerEmployeeId` is a plain `MaxLength(450)` string column with **no FK constraint** — an unenforced, opaque reference to `Employee.Id` (nothing stops it pointing at a deleted or cross-company employee).
- **`EmployeeLevels`** — index on `CompanyId`; FK to `Company` is `Restrict`.
- **`Employees`** — unique composite index on `(CompanyId, EmployeeCode)`; separate index on `UserId`; FK to `Company` is `Restrict`.
- **`EmployeeOrgUnitMemberships`** — separate (non-composite) indexes on `EmployeeId` and `OrgUnitId`; FK to `Employee` is `Cascade`; FK to `OrgUnit` is `Cascade`; FK to `EmployeeLevel` (`LevelId`, nullable) is `SetNull`.

**A real bug found and fixed via the test suite**: the FK from `EmployeeOrgUnitMembership.OrgUnitId` to `OrgUnit` was originally `DeleteBehavior.Restrict`. `DeleteOrgUnitCommandHandler`'s own guard only blocks deletion when an **active** membership exists (`EndDate == null`) — but `Restrict` blocks at the DB level on *any* referencing row regardless of `EndDate`, so deleting a unit that had only already-ended memberships still failed. Fixed to `Cascade`: a deleted unit's historical membership rows are meaningless anyway, since `GetEmployeeByIdQueryHandler` resolves `EmployeeMembershipDto.OrgUnitName`/`OrgUnitType` live via the `OrgUnit` navigation property rather than storing a snapshot at assignment time. `tests/Organization.Tests/.../DeleteOrgUnitCommandHandlerTests.Handle_ShouldAllow_WhenOnlyEndedMembershipsExist` covers this directly.

`SearchCompaniesQueryHandler`/`SearchEmployeesQueryHandler` (`Light.EntityFrameworkCore.Extensions.WhereIf` + `ToPagedResultAsync`) and `GetOrgUnitTreeQueryHandler`/`GetOrgUnitEmployeesQueryHandler`/`GetEmployeeByIdQueryHandler` (`Mapster.ProjectToType<T>`) all read `AsNoTracking`. `SaveChanges[Async]` calls `TrackingExtensions.AuditEntries(currentUser.UserId, clock.AuditTime, enableSoftDelete: false)` — none of the five entities implement `ISoftDelete`, so (unlike `Identity`'s `User`) this is not a comparable soft-delete gap, just no soft-delete support at all in this module.

Migrations exist for all three supported providers: `src/Migrations/{MSSQL,PostgreSQL,Sqlite}/Organization/` each hold one baseline `CreateOrganizationSchema` migration; each Migrations project's `.csproj` references `Organization.Api` and registers/invokes `OrganizationContextInitialiser` (mirrors `IdentityContextInitialiser`'s `InitialiseAsync`-then-`TrySeedAsync` shape) right after the Identity initialiser call in each provider's `Program.cs`. Seed data: two companies — `ACME` ("Acme Corporation"), fully fleshed out with an `Engineering` department containing `Backend`/`Frontend` teams plus a standalone `HR` department, five employee levels (`Intern`→`Manager`, ranked 1–5), and six employees each with one primary membership — and `GLBX` ("Globex Corporation"), deliberately left bare with no org units/levels/employees, to demonstrate multi-company scoping. Seeding is idempotent, checked by `Code`/`EmployeeCode` before inserting (`GetOrCreate*Async` helpers).

## Dependencies

| Depends on | Type | Why |
|---|---|---|
| `Shared` | project (`Organization.Contracts → Shared`) | Base `SearchQuery`/`PageQuery` for `CompanySearchRequest`/`EmployeeSearchRequest`. |
| `Infrastructure` | project (`Organization.Api → Infrastructure`) | `VersionedApiController`, `AppModule` base class. |
| `Persistence` | project (`Organization.Api → Persistence`) | `BaseDbContext`, `AddConfiguredDbContext`, `AuditEntries`/`ConfigureAuditableEntity`, paging extensions. |
| `Organization.Contracts` | project (`Organization.Api → Organization.Contracts`) | The module's own seam. |
| `Identity.Contracts` | project (`Organization.Api → Identity.Contracts`) | The module's **one cross-module business dependency**: `CreateEmployeeLoginCommandHandler`/`LinkEmployeeLoginCommandHandler`/`UnlinkEmployeeLoginCommandHandler` take an injected `IUserService` to create or link an Identity login for an employee, storing the resulting Identity `User.Id` as an opaque string on `Employee.UserId` — no FK, same "opaque cross-module reference" pattern `Notifications.Notification.FromUserId`/`ToUserId` already uses against `Identity`. This mirrors `Identity.Api → Notifications.Contracts` (see `Identity.md`): both reach only the other module's `Contracts` seam, never its `.Api` internals — a second compliant instance of a business-module-to-business-module dependency, not a boundary violation. |
| Vendor `Lightsoft.AspNetCore.Authorization` (both projects), `Lightsoft.EntityFrameworkCore`, `Lightsoft.Mediator`, `Lightsoft.Result`, `Mapster` (`Organization.Api`) | package, **all declared directly** | Unlike `Identity`/`Notifications` — which each have at least one undeclared-transitive-dependency instance (see `../dependency-graph.md`) — both `Organization.Contracts.csproj` and `Organization.Api.csproj` declare every vendor package they directly use. A positive contrast, not itself a convention change for the other two modules. |

## Depended On By

`StarterKit.WebApi` (composition-root host, wired into `ConfigureExtensions.cs`'s `assemblies` array alongside `IdentityModule`/`NotificationModule`), the three Migrations tooling projects (`src/Migrations/{MSSQL,PostgreSQL,Sqlite}`, each referencing `Organization.Api` directly for `OrganizationDbContext`/`OrganizationContextInitialiser`), and `Organization.Tests` (`Organization.Api.csproj` grants `InternalsVisibleTo` so the test project can reach the `internal` command/query records and handlers directly). No other business module references `Organization.Api`/`Organization.Contracts` — confirmed via `ProjectReference` search across all `.csproj` files under `src/` and `tests/`. Client-side integration is not re-inspected as part of backend-only syncs.

## Notable Conventions

- **CQRS handlers own their logic directly — a deliberate deviation from `Identity`/`Notifications`.** Every controller action dispatches an `internal sealed record` command/query under `Application/{Companies,OrgUnits,EmployeeLevels,Employees}/{Commands,Queries}`, same as the other two modules — but unlike them, the handlers here hold the actual `OrganizationDbContext` query/mutation logic directly, with **no separate service-class layer** to delegate to. This is the resolution direction `known-debt.md`'s D1 poses as an open question for `Identity`/`Notifications` (inline the service logic into the handlers), applied from the start in this module — a real, intentional structural difference from its two siblings, not an oversight.
- **Delete guards are asymmetric by design, and it's test-documented, not accidental.** `DeleteCompanyCommandHandler`/`DeleteOrgUnitCommandHandler` explicitly check for dependents (org units/employees; child units/active memberships) before allowing a delete. `DeleteEmployeeLevelCommandHandler`/`DeleteEmployeeCommandHandler` have **no such guard** — they rely entirely on the FK behavior configured in `OrganizationDbContext` (`SetNull` for a level still referenced by memberships, `Cascade` for an employee's membership history) to do the right thing instead of blocking. Both `tests/Organization.Tests/Application/EmployeeLevels/Commands/DeleteEmployeeLevelCommandHandlerTests.cs` and `.../Employees/Commands/DeleteEmployeeCommandHandlerTests.cs` carry an explicit XML-doc comment stating this is intentional and documenting the resulting behavior, not flagging a gap.
- **`OrgUnit` unifies Department and Team into one self-referencing entity** via the `Type` discriminator (`OrgUnitType.Department`/`Team`) rather than two separate entities/tables — `MoveOrgUnitCommandHandler`'s cycle check walks `ParentId` one row at a time (`IsDescendantAsync`, one query per ancestor level, no recursive CTE) to prevent an org unit from being moved under its own descendant.
- **`EmployeeLevel` is a company-scoped configurable lookup, not a hardcoded enum** — each company defines its own set of levels and a `Rank` for ordering (seed data ranks `Intern`(1)→`Manager`(5) for `ACME`); `Code` uniqueness is enforced per-company, not globally, matching the same per-company scoping already used for `OrgUnit.Code` and `Employee.EmployeeCode`.
- **`EmployeeOrgUnitMembership` models history, not a snapshot join.** Removing an employee from an org unit (`RemoveEmployeeFromOrgUnitCommandHandler`) sets `EndDate`/`IsPrimary = false` rather than deleting the row, so past assignments remain queryable; `EndDate == null` is the "currently active" predicate used consistently across every handler/query that touches memberships (assignment, update, tree/employee listing, delete guards).
- **`Organization.Contracts`' per-feature subfolder split** (`Common/Companies/OrgUnits/EmployeeLevels/Employees/Authorization`, each its own namespace) is a finer-grained organization than `Identity.Contracts`/`Notifications.Contracts`'s flatter, single-namespace layout — worth following for a future module expected to grow this many DTOs, but not (yet) proposed as a retrofit for the other two.
- `Organization.Api.csproj` declares `<InternalsVisibleTo Include="Organization.Tests" />`, same pattern as `Identity.Api.csproj` → `Identity.Tests`. `tests/Organization.Tests` uses a Sqlite in-memory `OrganizationTestHost` (mirroring `Identity.Tests`'s `IdentityTestHost`) so handler tests exercise real EF Core behavior (FK constraints, `IQueryable` translation) rather than hand-mocked stand-ins; the two login-command test files mock `IUserService` instead, since that dependency is a real cross-module boundary. 46 test methods across 16 files, weighted toward business-rule edge cases (org-unit move/cycle/cross-company checks, delete-blocked-by-dependents checks, membership assignment/primary-exclusivity rules, the three login commands) over trivial CRUD happy paths.

## Notes

<!-- manual: content below this line is human-authored and must be preserved verbatim during sync -->

---
_Last synced: 2026-09-05_
