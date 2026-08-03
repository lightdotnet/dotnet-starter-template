# Dependency Graph: Backend

## Project-to-Project References (backend)

| From | To | Notes |
|---|---|---|
| `src/Infrastructure/Infrastructure.csproj` | `src/Shared/Shared.csproj` | Only project reference in `Infrastructure`. |
| `src/Persistence/Persistence.csproj` | `src/Shared/Shared.csproj` | Only project reference in `Persistence`. |
| `src/Identity.Contracts/Identity.Contracts.csproj` | `src/Shared/Shared.csproj` | New — added to reach `Shared`'s transitive `Lightsoft.AspNetCore.Authorization` package for `IdentityPermissionProvider`; `Identity.Contracts.csproj` declares no direct `PackageReference` for it (see "Undeclared transitive dependency" below). |
| `src/Identity.Api/Identity.Api.csproj` | `src/Identity.Contracts/Identity.Contracts.csproj` | |
| `src/Identity.Api/Identity.Api.csproj` | `src/Infrastructure/Infrastructure.csproj` | |
| `src/Identity.Api/Identity.Api.csproj` | `src/Persistence/Persistence.csproj` | |
| `src/StarterKit.WebApi/StarterKit.WebApi.csproj` | `src/Identity.Api/Identity.Api.csproj` | Composition-root host. |
| `src/StarterKit.WebApi/StarterKit.WebApi.csproj` | `src/Infrastructure/Infrastructure.csproj` | |
| `src/StarterKit.WebApi/StarterKit.WebApi.csproj` | `src/Shared/Shared.csproj` | |
| `tests/Framework.Tests/Framework.Tests.csproj` | `src/Shared/Shared.csproj` | |
| `tests/Framework.Tests/Framework.Tests.csproj` | `src/Infrastructure/Infrastructure.csproj` | |
| `tests/Framework.Tests/Framework.Tests.csproj` | `src/Persistence/Persistence.csproj` | |
| `tests/Identity.Tests/Identity.Tests.csproj` | `src/Identity.Api/Identity.Api.csproj` | |
| `tests/Identity.Tests/Identity.Tests.csproj` | `src/Shared/Shared.csproj` | |

`src/Shared/Shared.csproj` has no project references (leaf). `src/Identity.Contracts/Identity.Contracts.csproj` is **no longer a leaf** — it now references `Shared` (see row above); its role as the module's cross-boundary seam is unaffected (still the only project other modules/the host may reference), but the "true leaf" claim from earlier syncs no longer holds. `Identity.Api.csproj` also declares `<InternalsVisibleTo Include="Identity.Tests" />` (mirroring the pattern `Shared`/`Infrastructure`/`Persistence` use for `Framework.Tests`), giving `Identity.Tests` access to the module's `internal` JWT orchestration classes (`AuthenticationService`, `UserSessionService`, `JwtTokenIssuer`, `JwtSigningService`).

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

**Undeclared transitive dependency**: `UserService.SearchAsync` (`src/Identity.Api/Services/UserService.cs`) uses `Light.EntityFrameworkCore.Extensions`' `WhereIf` — supplied by the `Lightsoft.EntityFrameworkCore` package, which `Identity.Api.csproj` does **not** declare as a `<PackageReference>` itself; it rides in transitively via the `ProjectReference` to `Persistence` (which does declare it). Works today because `CentralPackageTransitivePinningEnabled=false` doesn't block it, but it's an implicit coupling — if `Persistence` ever drops that package, `Identity.Api` would silently break.

**Same pattern, new instance**: `Identity.Contracts.Authorization.IdentityPermissionProvider` uses `Light.AspNetCore.Authorization`'s `IPermissionDefinitionProvider`/`PermissionDefinition` — supplied by `Lightsoft.AspNetCore.Authorization`, which `Identity.Contracts.csproj` does **not** declare as a `<PackageReference>` itself; it rides in transitively via the new `ProjectReference` to `Shared` (which does declare it). This is also why `Identity.Contracts` is no longer a leaf project — the `ProjectReference` to `Shared` appears to exist solely to reach this one package.

## Circular References

None found.

## Version Mismatches

None — all packages are centrally managed via `Directory.Packages.props` (`Framework.Tests` and `Identity.Tests` each opt out via `ManagePackageVersionsCentrally=false` and pin their own test packages independently — `Identity.Tests` adds `Moq` on top of the same 3 xunit/Test.Sdk packages `Framework.Tests` uses — no overlap with the centrally-managed set).

## Cross-Module Boundary Violations (backend only)

None found. `Identity.Api` references only `Identity.Contracts`, `Infrastructure`, and `Persistence` — no reach into another module's internals. Still unverified in the strict sense that `Identity` is the only module, so there's no second module's `Domain`/`Infrastructure`/`Api` project it could improperly reference.

## Notes

<!-- manual: content below this line is human-authored and must be preserved verbatim during sync -->

---
_Generated: 2026-08-03 (resynced — `Identity.Contracts -> Shared` project reference added, `Lightsoft.AspNetCore.Authorization` bumped to preview2) — scope: Backend — see .claude/CLAUDE.md for update rules._
