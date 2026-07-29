# Dependency Graph: Backend

## Project-to-Project References

| From | To | Notes |
|---|---|---|
| `src/Infrastructure/Infrastructure.csproj` | `src/Shared/Shared.csproj` | Only project reference in `Infrastructure`. |
| `tests/Framework.Tests/Framework.Tests.csproj` | `src/Shared/Shared.csproj` | |
| `tests/Framework.Tests/Framework.Tests.csproj` | `src/Infrastructure/Infrastructure.csproj` | |

`src/Shared/Shared.csproj` has no project references (leaf project).

## Package References

| Project | Package | Version | Notes |
|---|---|---|---|
| Shared | FluentValidation | 12.1.1 | Backs `ValidationBehaviour<,>`. |
| Shared | Lightsoft.AspNetCore.Authorization | 10.2.1-preview1 | Permission policy provider/handler base classes. |
| Shared | Lightsoft.EventBus | 0.2.1 | Referenced, not yet used anywhere in `Shared`. |
| Shared | Lightsoft.Extensions | 1.10.1-preview1 | `Light.Extensions.DependencyInjection` helpers. |
| Shared | Lightsoft.Mediator | 1.2.1 | `IRequest<T>`, `IPipelineBehavior<,>`, `RequestHandlerDelegate<>`. |
| Shared | Lightsoft.Result | 2.0.0 | `Result`/`Result<T>`/`Paged<T>`/`PagedResult<T>` contracts. |
| Shared | Lightsoft.SharedKernel | 1.10.2 | `Light.Domain` base entity/value-object types, `Light.Exceptions.*`. |
| Shared | Mapster | 10.0.11 | Object mapping (configured in `Infrastructure`). |
| Infrastructure | AspNetCore.HealthChecks.UI.Client | 9.0.0 | Health check endpoint response writer. |
| Infrastructure | Lightsoft.AspNetCore.Extensions | 10.1.1-preview1 | CORS helpers (`Light.AspNetCore.Cors`). |
| Infrastructure | Lightsoft.AspNetCore.Modularity | 10.2.1-preview1 | `IModuleEndpoint`, `AppModule` vendor base. |
| Infrastructure | Lightsoft.EntityFrameworkCore | 10.3.1-preview1 | `Light.EntityFrameworkCore.Extensions` (paging helpers). |
| Infrastructure | Lightsoft.FileGenerator | 1.14.1-preview1 | Referenced, not yet used anywhere in `Infrastructure`. |
| Infrastructure | Lightsoft.Serilog | 1.1.1-preview1 | Referenced; `AppLogging.cs` uses plain `Serilog` directly today. |
| Infrastructure | Microsoft.EntityFrameworkCore.InMemory | 10.0.10 | `DbProvider.InMemory`. |
| Infrastructure | Microsoft.EntityFrameworkCore.Sqlite | 10.0.10 | `DbProvider.Sqlite`; `SqliteDbContextExtensions`. |
| Infrastructure | Microsoft.EntityFrameworkCore.SqlServer | 10.0.10 | `DbProvider.MSSQL`. |
| Infrastructure | Npgsql.EntityFrameworkCore.PostgreSQL | 10.0.3 | `DbProvider.PostgreSQL`. |
| Infrastructure | SQLitePCLRaw.bundle_e_sqlite3 | 3.0.5 | Sqlite native bundle. |
| Framework.Tests | xunit.v3 | 3.2.2 | Test framework. |
| Framework.Tests | xunit.runner.visualstudio | 3.1.5 | Test runner/discovery. |
| Framework.Tests | Microsoft.NET.Test.Sdk | 18.8.1 | Test SDK. |

## Circular References

None found.

## Version Mismatches

None — all packages are centrally managed via `Directory.Packages.props` (`Framework.Tests` pins its own 3 test packages independently, no overlap with the centrally-managed set).

## Cross-Module Boundary Violations (backend only)

None possible yet — no `src/Modules/*` exist.

## Notes

<!-- manual: content below this line is human-authored and must be preserved verbatim during sync -->

---
_Generated: 2026-07-29 — scope: Backend — see .claude/CLAUDE.md for update rules._
