# Dependency Graph: Backend

## Package References

Package versions are centrally managed via the root `Directory.Packages.props` (`ManagePackageVersionsCentrally=true`) — individual `.csproj` files reference packages by name only, with no `Version` attribute. Most `Lightsoft.*` packages share one version via a single `$(LightVersion)` MSBuild property; `Lightsoft.Mediator`, `Lightsoft.Result`, and `Lightsoft.EventBus`/`Lightsoft.EventBus.MassTransit.RabbitMQ` are pinned on their own version lines instead. Most `Microsoft.AspNetCore.*`/EF Core packages similarly share a single `$(AspnetVersion)` property. `tests/Framework.Tests`, `tests/Identity.Tests`, `tests/Organization.Tests`, `tests/Approval.Tests`, and `tests/LeaveManagement.Tests` are the only opt-outs (`ManagePackageVersionsCentrally=false`) and pin their own package versions directly in their `.csproj` files. Exact version numbers are intentionally omitted below — check `Directory.Packages.props` (and the five test `.csproj` files) for current pins rather than trusting a number here.

| Project | Packages | Notes |
|---|---|---|
| Shared | FluentValidation, Lightsoft.AspNetCore.Authorization, Lightsoft.EventBus, Lightsoft.Extensions, Lightsoft.Mediator, Lightsoft.Result, Lightsoft.SharedKernel, Mapster | `Lightsoft.EventBus` has no usage found in `Shared` — see `../known-debt.md` (dependency hygiene). |
| Infrastructure | AspNetCore.HealthChecks.UI.Client, Lightsoft.AspNetCore.Extensions, Lightsoft.AspNetCore.Modularity, Lightsoft.FileGenerator, Lightsoft.Serilog | `Lightsoft.FileGenerator` has no usage found here either — see `../known-debt.md`. |
| Persistence | Lightsoft.EntityFrameworkCore, Microsoft.EntityFrameworkCore.InMemory, Microsoft.EntityFrameworkCore.Sqlite, Microsoft.EntityFrameworkCore.SqlServer, Npgsql.EntityFrameworkCore.PostgreSQL, SQLitePCLRaw.lib.e_sqlite3 | One EF Core provider package per supported `DbProvider` value; `SQLitePCLRaw.lib.e_sqlite3` (Sqlite native bundle) is declared but not directly used in code — a transitive dependency of `Microsoft.EntityFrameworkCore.Sqlite`. |
| Identity.Contracts | Lightsoft.Result | |
| Identity.Api | Lightsoft.ActiveDirectory, Lightsoft.SharedKernel, Microsoft.AspNetCore.Identity.EntityFrameworkCore | Several other vendor types it uses (`Lightsoft.AspNetCore.Authorization`, `Lightsoft.Result`, `Lightsoft.Extensions`, `Lightsoft.Mediator`, `Light.EntityFrameworkCore.Extensions.WhereIf`) ride in transitively via `ProjectReference`s rather than being declared directly — see `../known-debt.md` (dependency hygiene). |
| Notifications.Contracts | (none — no direct `<PackageReference>`) | The `Light.AspNetCore.Authorization` types it uses (`IPermissionDefinitionProvider`, permission attributes) ride in transitively via its `ProjectReference` to `Shared`. |
| Notifications.Api | Lightsoft.SmtpMail | Its only direct package reference; everything else it uses (`Light.EntityFrameworkCore.Extensions`, `Light.Specification`, `Mapster`, `Microsoft.AspNetCore.SignalR` — the last a shared-framework reference, not a NuGet package) rides in transitively via `Infrastructure`/`Persistence`/`Shared`. |
| Organization.Contracts | Lightsoft.AspNetCore.Authorization | Declared directly (for `IPermissionDefinitionProvider`/`OrganizationPermissionProvider`) — unlike `Notifications.Contracts`, which uses the same vendor type transitively without declaring it. |
| Organization.Api | Lightsoft.AspNetCore.Authorization, Lightsoft.EntityFrameworkCore, Lightsoft.Mediator, Lightsoft.Result, Mapster | Every vendor package the project directly uses is declared directly — no undeclared-transitive-dependency instance here, unlike `Identity.Api`/`Notifications.Api` (see `../known-debt.md`). |
| Approval.Contracts | Lightsoft.AspNetCore.Authorization, Lightsoft.Result | Declared directly (for `IPermissionDefinitionProvider`/`ApprovalPermissionProvider` and `IResult`/`IResult<T>`) — same "declare what you use" discipline as `Organization.Contracts`. |
| Approval.Api | Lightsoft.AspNetCore.Authorization, Lightsoft.EntityFrameworkCore, Lightsoft.Mediator, Lightsoft.Result, Mapster | Every vendor package the project directly uses is declared directly — same positive pattern as `Organization.Api`, no undeclared-transitive-dependency instance. |
| LeaveManagement.Contracts | Lightsoft.AspNetCore.Authorization, Lightsoft.Result | Declared directly (for `IPermissionDefinitionProvider`/`LeaveManagementPermissionProvider` and `IResult`/`IResult<T>`) — same "declare what you use" discipline as `Organization.Contracts`/`Approval.Contracts`. |
| LeaveManagement.Api | Lightsoft.AspNetCore.Authorization, Lightsoft.EntityFrameworkCore, Lightsoft.Mediator, Lightsoft.Result, Mapster | Every vendor package the project directly uses is declared directly — same positive pattern as `Organization.Api`/`Approval.Api`, no undeclared-transitive-dependency instance. |
| StarterKit.WebApi | AspNetCore.HealthChecks.UI.Client, FluentValidation.DependencyInjectionExtensions, Lightsoft.AspNetCore.Swagger, Microsoft.VisualStudio.Azure.Containers.Tools.Targets, Spectre.Console | Uses `Lightsoft.Serilog` too, without declaring it directly — rides in via `Infrastructure`. |
| Framework.Tests | xunit.v3, xunit.runner.visualstudio, Microsoft.NET.Test.Sdk | Opts out of central package management and pins these directly. |
| Identity.Tests | xunit.v3, xunit.runner.visualstudio, Microsoft.NET.Test.Sdk, Moq | Same opt-out as `Framework.Tests`, plus `Moq`. |
| Organization.Tests | xunit.v3, xunit.runner.visualstudio, Microsoft.NET.Test.Sdk, Moq | Same opt-out/package set as `Identity.Tests`. Uses `Moq` to mock `Identity.Contracts.Services.IUserService` in the employee-login command tests; everything else runs against a real Sqlite in-memory `OrganizationDbContext` (`OrganizationTestHost`). |
| Approval.Tests | xunit.v3, xunit.runner.visualstudio, Microsoft.NET.Test.Sdk, Moq | Same opt-out/package set as `Identity.Tests`/`Organization.Tests`. |
| LeaveManagement.Tests | xunit.v3, xunit.runner.visualstudio, Microsoft.NET.Test.Sdk, Moq | Same opt-out/package set as `Identity.Tests`/`Organization.Tests`/`Approval.Tests`. Uses `Moq` to mock `Organization.Contracts.Services.IOrgDirectoryService` and `Approval.Contracts.Services.IApprovalService` — both real cross-module boundaries, same rationale as `Organization.Tests` mocking `IUserService`; everything else runs against a real Sqlite in-memory `LeaveManagementDbContext` (`LeaveManagementTestHost`). |

This undeclared-transitive-dependency pattern (a project using a vendor type without declaring the package itself, riding in via a `ProjectReference`) recurs three times in the repo — `Identity.Api`, `Identity.Contracts`, and `Notifications.Contracts` each have one instance. `Organization.Contracts`/`Organization.Api`, `Approval.Contracts`/`Approval.Api`, and `LeaveManagement.Contracts`/`LeaveManagement.Api` do not repeat it — all six declare every vendor package they directly use. See `../known-debt.md` for the full list and the recommendation to declare packages a project actually uses directly.

## Circular References

None found. `Shared` is the only true leaf (no `ProjectReference`s of its own); every module's `Contracts` project — `Identity.Contracts`, `Notifications.Contracts`, `Organization.Contracts`, `Approval.Contracts`, `LeaveManagement.Contracts` — references `Shared`, so none of them are true leaves either; `Identity.Contracts` references it solely to reach a transitive package (see Package References above). Dependency direction is one-way throughout: `Api`/`Contracts` projects → `Infrastructure`/`Persistence` → `Shared`, and `StarterKit.WebApi` (composition-root host) → all five business modules. No project reference cycle exists anywhere in the graph.

```text
Infrastructure -> Shared
Persistence -> Shared
Identity.Contracts -> Shared
Identity.Api -> Identity.Contracts
Identity.Api -> Infrastructure
Identity.Api -> Persistence
Notifications.Contracts -> Shared
Notifications.Api -> Notifications.Contracts
Notifications.Api -> Infrastructure
Notifications.Api -> Persistence
Identity.Api -> Notifications.Contracts
Organization.Contracts -> Shared
Organization.Api -> Organization.Contracts
Organization.Api -> Infrastructure
Organization.Api -> Persistence
Organization.Api -> Identity.Contracts
Approval.Contracts -> Shared
Approval.Api -> Approval.Contracts
Approval.Api -> Infrastructure
Approval.Api -> Persistence
Approval.Api -> Notifications.Contracts
LeaveManagement.Contracts -> Shared
LeaveManagement.Api -> LeaveManagement.Contracts
LeaveManagement.Api -> Infrastructure
LeaveManagement.Api -> Persistence
LeaveManagement.Api -> Approval.Contracts
LeaveManagement.Api -> Organization.Contracts
StarterKit.WebApi -> Identity.Api
StarterKit.WebApi -> Notifications.Api
StarterKit.WebApi -> Organization.Api
StarterKit.WebApi -> Approval.Api
StarterKit.WebApi -> LeaveManagement.Api
StarterKit.WebApi -> Infrastructure
StarterKit.WebApi -> Shared
Framework.Tests -> Shared
Framework.Tests -> Infrastructure
Framework.Tests -> Persistence
Identity.Tests -> Identity.Api
Identity.Tests -> Shared
Organization.Tests -> Organization.Api
Organization.Tests -> Identity.Contracts
Organization.Tests -> Shared
Approval.Tests -> Approval.Api
Approval.Tests -> Approval.Contracts
Approval.Tests -> Shared
LeaveManagement.Tests -> LeaveManagement.Api
LeaveManagement.Tests -> LeaveManagement.Contracts
LeaveManagement.Tests -> Approval.Contracts
LeaveManagement.Tests -> Organization.Contracts
LeaveManagement.Tests -> Shared
```

## Cross-Module Boundary Violations (backend only)

None found. Five business-module-to-business-module dependencies exist, all compliant (each reaches only the other module's `Contracts` seam, never its `.Api` internals):

- `Identity.Api` references `Notifications.Contracts`, consumed by `UserCreatedEventHandler` for a welcome-email side effect via `IMailService`.
- `Organization.Api` references `Identity.Contracts`, consumed by `CreateEmployeeLoginCommandHandler`/`LinkEmployeeLoginCommandHandler`/`UnlinkEmployeeLoginCommandHandler` via `IUserService`, to create or link an Identity login for an employee and store the resulting `User.Id` as an opaque string on `Employee.UserId` (no FK — the same opaque-reference pattern `Notifications.Notification.FromUserId`/`ToUserId` already uses against `Identity`), plus stamping/clearing that user's `employee_id` claim via `IUserService.SetClaimAsync`.
- `Approval.Api` references `Notifications.Contracts`, consumed by `ApprovalStepPendingEventHandler`/`ApprovalFinalizedEventHandler` via `INotificationService.SendAsync`, to notify the relevant approver/requester when a step becomes pending or a request is finalized.
- `LeaveManagement.Api` references `Approval.Contracts`, consumed by every `Application/LeaveRequests/{Commands,Queries}` handler that touches the approval workflow via `IApprovalService` (`CreateAsync`/`CancelAsync`/`GetByRequestAsync`) — LeaveManagement never runs its own approval mechanics.
- `LeaveManagement.Api` references `Organization.Contracts`, consumed by the same handlers via `IOrgDirectoryService` (`GetApproverCandidatesAsync`/`GetEmployeeNameAsync`) to resolve eligible approvers and display names from the org hierarchy without reaching into `Organization.Api`'s internals.

None of the reverse directions exist: `Notifications` references nothing belonging to `Identity`, `Organization`, `Approval`, or `LeaveManagement`; `Identity`/`Notifications`/`Approval` reference nothing belonging to `Organization`; `Identity`/`Notifications`/`Organization` reference nothing belonging to `Approval`; and no module besides `LeaveManagement.Api` references `Approval.Contracts`/`Organization.Contracts` for this pairing, nor does any module reference `LeaveManagement.Api`/`LeaveManagement.Contracts` at all yet. See `modules/Identity.md`, `modules/Notifications.md`, `modules/Organization.md`, `modules/Approval.md`, and `modules/LeaveManagement.md` for full detail.

## Notes

<!-- manual: content below this line is human-authored and must be preserved verbatim during sync -->

---
_Last synced: 2026-09-07_
