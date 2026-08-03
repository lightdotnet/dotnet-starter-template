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

`src/Shared/Shared.csproj` and `src/Notifications.Contracts/Notifications.Contracts.csproj` have no project references beyond `Shared` itself (both leaves in the "only depends on `Shared`" sense — `Shared` has none at all). `src/Identity.Contracts/Identity.Contracts.csproj` is **not a leaf** — it references `Shared` purely to reach one transitive package (see row above; full detail in `modules/Identity.md`). `Identity.Api.csproj` declares `<InternalsVisibleTo Include="Identity.Tests" />` (mirroring the pattern `Shared`/`Infrastructure`/`Persistence` use for `Framework.Tests`), giving `Identity.Tests` access to the module's `internal` JWT orchestration classes. `Notifications.Api` has no equivalent test project or `InternalsVisibleTo` entry yet.

## Package References

| Project | Package | Version | Notes |
|---|---|---|---|
| Shared | FluentValidation | 12.1.1 | Backs `ValidationBehaviour<,>`. |
| Shared | Lightsoft.AspNetCore.Authorization | 10.2.1-preview2 | Permission policy provider/handler base classes; preview2 added `IPermissionDefinitionProvider`/`PermissionDefinition`, now used by `Identity.Contracts.Authorization.IdentityPermissionProvider`. |
| Shared | Lightsoft.EventBus | 0.2.1 | Referenced, not yet used anywhere in `Shared`. |
| Shared | Lightsoft.Extensions | 1.10.1-preview1 | `Light.Extensions.DependencyInjection` helpers. |
| Shared | Lightsoft.Mediator | 1.2.1 | `IRequest<T>`, `IPipelineBehavior<,>`, `RequestHandlerDelegate<>`. |
| Shared | Lightsoft.Result | 2.0.0 | `Result`/`Result<T>`/`Paged<T>`/`PagedResult<T>` contracts. |
| Shared | Lightsoft.SharedKernel | 1.10.2 | `Light.Domain` base entity/value-object types, `Light.Exceptions.*`. |
| Shared | Mapster | 10.0.11 | Object mapping (configured in `Infrastructure`). |
| Infrastructure | AspNetCore.HealthChecks.UI.Client | 9.0.0 | Health check endpoint response writer. |
| Infrastructure | Lightsoft.AspNetCore.Extensions | 10.1.1-preview2 | CORS helpers (`Light.AspNetCore.Cors`). |
| Infrastructure | Lightsoft.AspNetCore.Modularity | 10.2.1-preview1 | `IModuleEndpoint`, `AppModule` vendor base. |
| Infrastructure | Lightsoft.FileGenerator | 1.14.1-preview1 | Referenced, not yet used anywhere in `Infrastructure`. |
| Infrastructure | Lightsoft.Serilog | 1.1.1-preview1 | Referenced; `AppLogging.cs` uses plain `Serilog` directly today. |
| Persistence | Lightsoft.EntityFrameworkCore | 10.3.1-preview1 | `Light.EntityFrameworkCore.Extensions` (paging helpers). |
| Persistence | Microsoft.EntityFrameworkCore.InMemory | 10.0.10 | `DbProvider.InMemory`. |
| Persistence | Microsoft.EntityFrameworkCore.Sqlite | 10.0.10 | `DbProvider.Sqlite`; Sqlite `DateTimeOffset` fix. |
| Persistence | Microsoft.EntityFrameworkCore.SqlServer | 10.0.10 | `DbProvider.MSSQL` (the default configured provider). |
| Persistence | Npgsql.EntityFrameworkCore.PostgreSQL | 10.0.3 | `DbProvider.PostgreSQL`. |
| Identity.Contracts | Lightsoft.Result | 2.0.0 | `Result`/`Result<T>` return types for `IUserService`/`IRoleService`/`IServiceClaimService`. |
| Identity.Api | Lightsoft.ActiveDirectory | 1.10.2-preview1 | Referenced; usage not verified in this pass. |
| Identity.Api | Lightsoft.SharedKernel | 1.10.2 | `Light.Domain` base types for entities. |
| Identity.Api | Microsoft.AspNetCore.Identity.EntityFrameworkCore | 10.0.10 | `IdentityUser`/`IdentityRole`/`IdentityDbContext<...>` base types. |
| StarterKit.WebApi | AspNetCore.HealthChecks.UI.Client | 9.0.0 | Health check endpoint wiring at the host. |
| StarterKit.WebApi | FluentValidation.DependencyInjectionExtensions | 12.1.1 | Registers `IValidator<T>` implementations with DI. |
| StarterKit.WebApi | Lightsoft.AspNetCore.Swagger | 10.1.1-preview1 | Swagger/OpenAPI setup (host only). |
| StarterKit.WebApi | Microsoft.VisualStudio.Azure.Containers.Tools.Targets | 1.23.0 | Container tooling (build-time only). |
| StarterKit.WebApi | Spectre.Console | 0.57.2 | Startup console banner in `Program.cs`. |
| Framework.Tests | xunit.v3 | 3.2.2 | Test framework. |
| Framework.Tests | xunit.runner.visualstudio | 3.1.5 | Test runner/discovery. |
| Framework.Tests | Microsoft.NET.Test.Sdk | 18.8.1 | Test SDK. |
| Identity.Tests | xunit.v3 | 3.2.2 | Test framework. |
| Identity.Tests | xunit.runner.visualstudio | 3.1.5 | Test runner/discovery. |
| Identity.Tests | Microsoft.NET.Test.Sdk | 18.8.1 | Test SDK. |
| Identity.Tests | Moq | 4.20.72 | Mocking library — used for `UserManager<User>`/service-interface test doubles; `Framework.Tests` uses hand-written fakes instead, so this is the first mocking-library dependency in the backend test suite. |

`SQLitePCLRaw.bundle_e_sqlite3` (3.0.5, Sqlite native bundle) is centrally pinned in `Directory.Packages.props` but not directly referenced by any `<PackageReference>` observed in this pass (likely a transitive dependency of `Microsoft.EntityFrameworkCore.Sqlite`).

`Notifications.Api.csproj` and `Notifications.Contracts.csproj` declare **no direct `<PackageReference>` of their own** — every vendor type they use (`Light.AspNetCore.Authorization`, `Light.EntityFrameworkCore.Extensions`, `Light.Specification`, `Mapster`, `Microsoft.AspNetCore.SignalR` — the last a shared-framework reference, not a package) rides in transitively via `ProjectReference`s to `Shared`/`Infrastructure`/`Persistence`. See below.

**Undeclared transitive dependency**: `UserService.SearchAsync` (`src/Identity.Api/Services/UserService.cs`) uses `Light.EntityFrameworkCore.Extensions`' `WhereIf` — supplied by the `Lightsoft.EntityFrameworkCore` package, which `Identity.Api.csproj` does **not** declare as a `<PackageReference>` itself; it rides in transitively via the `ProjectReference` to `Persistence` (which does declare it). Works today because `CentralPackageTransitivePinningEnabled=false` doesn't block it, but it's an implicit coupling — if `Persistence` ever drops that package, `Identity.Api` would silently break. `Notifications.Api`'s `NotificationService` uses the same `WhereIf` helper the same way, transitively via its own `ProjectReference` to `Persistence`.

**Same pattern, instance 2**: `Identity.Contracts.Authorization.IdentityPermissionProvider` uses `Light.AspNetCore.Authorization`'s `IPermissionDefinitionProvider`/`PermissionDefinition` — supplied by `Lightsoft.AspNetCore.Authorization`, which `Identity.Contracts.csproj` does **not** declare as a `<PackageReference>` itself; it rides in transitively via the `ProjectReference` to `Shared` (which does declare it). This is also why `Identity.Contracts` is not a leaf project — the `ProjectReference` to `Shared` appears to exist solely to reach this one package.

**Same pattern, instance 3**: `Notifications.Contracts.Authorization.NotificationPermissionProvider` uses the same `Light.AspNetCore.Authorization` types the same way — transitively via `Notifications.Contracts`'s `ProjectReference` to `Shared`, which is `Notifications.Contracts`'s *only* project reference (so unlike `Identity.Contracts`, it doesn't cost `Notifications.Contracts` its leaf status relative to other modules — it was always going to reference `Shared`).

## Circular References

None found.

## Version Mismatches

None — all packages are centrally managed via `Directory.Packages.props` (`Framework.Tests` and `Identity.Tests` each opt out via `ManagePackageVersionsCentrally=false` and pin their own test packages independently — `Identity.Tests` adds `Moq` on top of the same 3 xunit/Test.Sdk packages `Framework.Tests` uses — no overlap with the centrally-managed set).

## Cross-Module Boundary Violations (backend only)

None found. `Identity.Api` references only `Identity.Contracts`, `Infrastructure`, and `Persistence`; `Notifications.Api` references only `Notifications.Contracts`, `Infrastructure`, and `Persistence` — neither reaches into the other's internals, and neither references the other's `Contracts` either (no coupling at all between the two business modules). **Now verified with a second module**, not just structurally assumed.

## Notes

<!-- manual: content below this line is human-authored and must be preserved verbatim during sync -->

---
_Generated: 2026-08-03 (resynced — added `Notifications`/`Notifications.Contracts` project and package references; cross-module boundary check now verified against a second module) — scope: Backend — see .claude/CLAUDE.md for update rules._
