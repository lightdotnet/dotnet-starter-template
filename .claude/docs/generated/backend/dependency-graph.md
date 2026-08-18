# Dependency Graph: Backend

## Project-to-Project References (backend)

| From | To | Notes |
|---|---|---|
| `src/Infrastructure/Infrastructure.csproj` | `src/Shared/Shared.csproj` | Only project reference in `Infrastructure`. |
| `src/Persistence/Persistence.csproj` | `src/Shared/Shared.csproj` | Only project reference in `Persistence`. |
| `src/Identity.Contracts/Identity.Contracts.csproj` | `src/Shared/Shared.csproj` | Added to reach `Shared`'s transitive `Lightsoft.AspNetCore.Authorization` package for `IdentityPermissionProvider`; `Identity.Contracts.csproj` declares no direct `PackageReference` for it (see "Undeclared transitive dependency" below; full detail in `modules/Identity.md`). |
| `src/Identity.Api/Identity.Api.csproj` | `src/Identity.Contracts/Identity.Contracts.csproj` | |
| `src/Identity.Api/Identity.Api.csproj` | `src/Infrastructure/Infrastructure.csproj` | |
| `src/Identity.Api/Identity.Api.csproj` | `src/Persistence/Persistence.csproj` | |
| `src/Identity.Api/Identity.Api.csproj` | `src/Notifications.Contracts/Notifications.Contracts.csproj` | **New this sync** — `Application/Users/EventHandlers/UserCreatedEventHandler.cs` takes a DI-injected `IMailService` (from `Notifications.Contracts.Services`) and sends a welcome email on user creation. First-ever cross-module reference between `Identity` and `Notifications` in either direction; compliant with the module-boundary rule since it targets only `Notifications.Contracts` (the seam), never `Notifications.Api`'s internals. See `modules/Identity.md` and `modules/Notifications.md`. |
| `src/Notifications.Contracts/Notifications.Contracts.csproj` | `src/Shared/Shared.csproj` | Only project reference in `Notifications.Contracts` — a true leaf, unlike `Identity.Contracts`. |
| `src/Notifications.Api/Notifications.Api.csproj` | `src/Notifications.Contracts/Notifications.Contracts.csproj` | |
| `src/Notifications.Api/Notifications.Api.csproj` | `src/Infrastructure/Infrastructure.csproj` | |
| `src/Notifications.Api/Notifications.Api.csproj` | `src/Persistence/Persistence.csproj` | |
| `src/StarterKit.WebApi/StarterKit.WebApi.csproj` | `src/Identity.Api/Identity.Api.csproj` | Composition-root host. |
| `src/StarterKit.WebApi/StarterKit.WebApi.csproj` | `src/Notifications.Api/Notifications.Api.csproj` | Composition-root host. |
| `src/StarterKit.WebApi/StarterKit.WebApi.csproj` | `src/Infrastructure/Infrastructure.csproj` | |
| `src/StarterKit.WebApi/StarterKit.WebApi.csproj` | `src/Shared/Shared.csproj` | |
| `tests/Framework.Tests/Framework.Tests.csproj` | `src/Shared/Shared.csproj` | |
| `tests/Framework.Tests/Framework.Tests.csproj` | `src/Infrastructure/Infrastructure.csproj` | |
| `tests/Framework.Tests/Framework.Tests.csproj` | `src/Persistence/Persistence.csproj` | |
| `tests/Identity.Tests/Identity.Tests.csproj` | `src/Identity.Api/Identity.Api.csproj` | |
| `tests/Identity.Tests/Identity.Tests.csproj` | `src/Shared/Shared.csproj` | |

`src/Shared/Shared.csproj` has no project references beyond itself (a true leaf — `Shared` has none at all). `src/Notifications.Contracts/Notifications.Contracts.csproj` is also a true leaf (only references `Shared`). `src/Identity.Contracts/Identity.Contracts.csproj` is **not a leaf** — it references `Shared` purely to reach one transitive package (see row above; full detail in `modules/Identity.md`). `Identity.Api.csproj` **is also no longer a "only its own module's seam + shared building blocks" project** as of this sync — alongside `Identity.Contracts`/`Infrastructure`/`Persistence`, it now also references `Notifications.Contracts` (see row above). `Identity.Api.csproj` declares `<InternalsVisibleTo Include="Identity.Tests" />` (mirroring the pattern `Shared`/`Infrastructure`/`Persistence` use for `Framework.Tests`), giving `Identity.Tests` access to the module's `internal` JWT orchestration classes. `Notifications.Api` has no equivalent test project or `InternalsVisibleTo` entry yet.

## Package References

Most `Lightsoft.*` packages now share one version via the `$(LightVersion)` MSBuild property in `Directory.Packages.props` (currently `2.0.0`) rather than being pinned individually; `Lightsoft.Mediator`, `Lightsoft.Result`, and `Lightsoft.EventBus`/`Lightsoft.EventBus.MassTransit.RabbitMQ` sit on their own version lines and are pinned separately.

| Project | Package | Version | Notes |
|---|---|---|---|
| Shared | FluentValidation | 12.1.1 | Backs `ValidationBehaviour<,>`. |
| Shared | Lightsoft.AspNetCore.Authorization | 2.0.0 | Permission policy provider/handler base classes, including `IPermissionDefinitionProvider`/`PermissionDefinition` used by `Identity.Contracts.Authorization.IdentityPermissionProvider`. |
| Shared | Lightsoft.EventBus | 0.2.1 | Referenced, not yet used anywhere in `Shared`. |
| Shared | Lightsoft.Extensions | 2.0.0 | `Light.Extensions.DependencyInjection` helpers. |
| Shared | Lightsoft.Mediator | 1.3.0 | `IRequest<T>`, `IPipelineBehavior<,>`, `RequestHandlerDelegate<>`. |
| Shared | Lightsoft.Result | 2.1.1 | `Result`/`Result<T>`/`Paged<T>`/`PagedResult<T>` contracts. |
| Shared | Lightsoft.SharedKernel | 2.0.0 | `Light.Domain` base entity/value-object types, `Light.Exceptions.*`. |
| Shared | Mapster | 10.0.11 | Object mapping (configured in `Infrastructure`). |
| Infrastructure | AspNetCore.HealthChecks.UI.Client | 9.0.0 | Health check endpoint response writer. |
| Infrastructure | Lightsoft.AspNetCore.Extensions | 2.0.0 | CORS helpers (`Light.AspNetCore.Cors`). |
| Infrastructure | Lightsoft.AspNetCore.Modularity | 2.0.0 | `IModuleEndpoint`, `AppModule` vendor base. |
| Infrastructure | Lightsoft.FileGenerator | 2.0.0 | Referenced, not yet used anywhere in `Infrastructure`. |
| Infrastructure | Lightsoft.Serilog | 2.0.0 | Referenced; `AppLogging.cs` uses plain `Serilog` directly today. |
| Persistence | Lightsoft.EntityFrameworkCore | 2.0.0 | `Light.EntityFrameworkCore.Extensions` (paging helpers). |
| Persistence | Microsoft.EntityFrameworkCore.InMemory | 10.0.10 | `DbProvider.InMemory`. |
| Persistence | Microsoft.EntityFrameworkCore.Sqlite | 10.0.10 | `DbProvider.Sqlite`; Sqlite `DateTimeOffset` fix. |
| Persistence | Microsoft.EntityFrameworkCore.SqlServer | 10.0.10 | `DbProvider.MSSQL` (the default configured provider). |
| Persistence | Npgsql.EntityFrameworkCore.PostgreSQL | 10.0.3 | `DbProvider.PostgreSQL`. |
| Identity.Contracts | Lightsoft.Result | 2.1.1 | `Result`/`Result<T>` return types for `IUserService`/`IRoleService`/`IServiceClaimService`. |
| Identity.Api | Lightsoft.ActiveDirectory | 2.0.0 | Referenced; usage not verified in this pass. |
| Identity.Api | Lightsoft.SharedKernel | 2.0.0 | `Light.Domain` base types for entities. |
| Identity.Api | Microsoft.AspNetCore.Identity.EntityFrameworkCore | 10.0.10 | `IdentityUser`/`IdentityRole`/`IdentityDbContext<...>` base types. |
| Notifications.Api | Lightsoft.SmtpMail | 2.0.0 | `ISmtpMailSender`/`SmtpMailKitOptions` backing `MailService`. `Notifications.Api`'s first-ever **direct** `<PackageReference>` — every other vendor type it uses still rides in transitively (see below). |
| StarterKit.WebApi | AspNetCore.HealthChecks.UI.Client | 9.0.0 | Health check endpoint wiring at the host. |
| StarterKit.WebApi | FluentValidation.DependencyInjectionExtensions | 12.1.1 | Registers `IValidator<T>` implementations with DI. |
| StarterKit.WebApi | Lightsoft.AspNetCore.Swagger | 2.0.0 | Swagger/OpenAPI setup (host only). |
| StarterKit.WebApi | Microsoft.VisualStudio.Azure.Containers.Tools.Targets | 1.23.0 | Container tooling (build-time only). |
| StarterKit.WebApi | Spectre.Console | 0.57.2 | Startup console banner in `Program.cs`. |
| Framework.Tests | xunit.v3 | 4.0.0 | Test framework. |
| Framework.Tests | xunit.runner.visualstudio | 4.0.0 | Test runner/discovery. |
| Framework.Tests | Microsoft.NET.Test.Sdk | 18.9.0 | Test SDK. |
| Identity.Tests | xunit.v3 | 4.0.0 | Test framework. |
| Identity.Tests | xunit.runner.visualstudio | 4.0.0 | Test runner/discovery. |
| Identity.Tests | Microsoft.NET.Test.Sdk | 18.9.0 | Test SDK. |
| Identity.Tests | Moq | 4.20.72 | Mocking library — used for `UserManager<User>`/service-interface test doubles; `Framework.Tests` uses hand-written fakes instead, so this is the first mocking-library dependency in the backend test suite. |

`SQLitePCLRaw.bundle_e_sqlite3` (3.0.5, Sqlite native bundle) is centrally pinned in `Directory.Packages.props` but not directly referenced by any `<PackageReference>` observed in this pass (likely a transitive dependency of `Microsoft.EntityFrameworkCore.Sqlite`).

`Notifications.Contracts.csproj` declares **no direct `<PackageReference>` of its own** — every vendor type it uses (`Light.AspNetCore.Authorization`) rides in transitively via its `ProjectReference` to `Shared`. `Notifications.Api.csproj` is not in the same position — it declares one direct `<PackageReference>` (`Lightsoft.SmtpMail`, see Package References above), its first ever. Every other vendor type `Notifications.Api` uses (`Light.EntityFrameworkCore.Extensions`, `Light.Specification`, `Mapster`, `Microsoft.AspNetCore.SignalR` — the last a shared-framework reference, not a package) still rides in transitively via `ProjectReference`s to `Shared`/`Infrastructure`/`Persistence`. See below.

**Undeclared transitive dependency**: `UserService.SearchAsync` (`src/Identity.Api/Services/UserService.cs`) uses `Light.EntityFrameworkCore.Extensions`' `WhereIf` — supplied by the `Lightsoft.EntityFrameworkCore` package, which `Identity.Api.csproj` does **not** declare as a `<PackageReference>` itself; it rides in transitively via the `ProjectReference` to `Persistence` (which does declare it). Works today because `CentralPackageTransitivePinningEnabled=false` doesn't block it, but it's an implicit coupling — if `Persistence` ever drops that package, `Identity.Api` would silently break. `Notifications.Api`'s `NotificationService` uses the same `WhereIf` helper the same way, transitively via its own `ProjectReference` to `Persistence`.

**Same pattern, instance 2**: `Identity.Contracts.Authorization.IdentityPermissionProvider` uses `Light.AspNetCore.Authorization`'s `IPermissionDefinitionProvider`/`PermissionDefinition` — supplied by `Lightsoft.AspNetCore.Authorization`, which `Identity.Contracts.csproj` does **not** declare as a `<PackageReference>` itself; it rides in transitively via the `ProjectReference` to `Shared` (which does declare it). This is also why `Identity.Contracts` is not a leaf project — the `ProjectReference` to `Shared` appears to exist solely to reach this one package.

**Same pattern, instance 3**: `Notifications.Contracts.Authorization.NotificationPermissionProvider` uses the same `Light.AspNetCore.Authorization` types the same way — transitively via `Notifications.Contracts`'s `ProjectReference` to `Shared`, which is `Notifications.Contracts`'s *only* project reference (so unlike `Identity.Contracts`, it doesn't cost `Notifications.Contracts` its leaf status relative to other modules — it was always going to reference `Shared`).

## Circular References

None found.

## Version Mismatches

None — all packages are centrally managed via `Directory.Packages.props` (`Framework.Tests` and `Identity.Tests` each opt out via `ManagePackageVersionsCentrally=false` and pin their own test packages independently — `Identity.Tests` adds `Moq` on top of the same 3 xunit/Test.Sdk packages `Framework.Tests` uses — no overlap with the centrally-managed set).

## Cross-Module Boundary Violations (backend only)

None found. `Identity.Api` references `Identity.Contracts`, `Infrastructure`, `Persistence`, and — **new this sync** — `Notifications.Contracts`; `Notifications.Api` references only `Notifications.Contracts`, `Infrastructure`, and `Persistence`. The new `Identity.Api → Notifications.Contracts` edge is the first real cross-module dependency between the two business modules, and it is compliant with the module-boundary rule: it reaches only `Notifications`'s `Contracts` seam (the `IMailService` interface), never `Notifications.Api`'s internals (`MailService`, `NotificationDbContext`, controllers, etc.). The reverse direction still holds exactly as before — `Notifications.Api`/`Notifications.Contracts` reference nothing belonging to `Identity`. So: no longer "neither module references the other at all," but still zero boundary violations — the seam worked as designed the first time it was actually used cross-module.

## Notes

<!-- manual: content below this line is human-authored and must be preserved verbatim during sync -->

---
_Generated: 2026-08-03 (resynced — added `Notifications`/`Notifications.Contracts` project and package references; cross-module boundary check now verified against a second module). Resynced 2026-08-11 — added the new `Identity.Api → Notifications.Contracts` project reference (welcome-email `IMailService` dependency) and the new `Notifications.Api → Lightsoft.SmtpMail` direct package reference; updated the "no direct PackageReference" note and the Cross-Module Boundary Violations section accordingly. Resynced 2026-08-18 — all `Lightsoft.*` packages moved from preview versions to stable releases (most now pinned via the shared `$(LightVersion)=2.0.0` property in `Directory.Packages.props`; `Lightsoft.Mediator` and `Lightsoft.Result` bumped independently to `1.3.0`/`2.1.1`); test packages (`xunit.v3`, `xunit.runner.visualstudio`, `Microsoft.NET.Test.Sdk`) bumped in `Framework.Tests`/`Identity.Tests` — scope: Backend — see .claude/CLAUDE.md for update rules._
