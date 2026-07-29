# Backend Solution Overview

## Purpose

ASP.NET Core (C#) Modular Monolith backend for the StarterKit template. Currently only the pre-module shared kernel exists (`src/Shared`, `src/Infrastructure`) — no `src/Modules/*` and no composition-root host project have been built yet.

## Modules

| Module | Path | Responsibility | Status |
|---|---|---|---|
| _none yet_ | `src/Modules/` | — | `src/Modules/` does not exist yet |

## Shared/Host Projects

| Project | Path | Responsibility |
|---|---|---|
| Shared | `src/Shared` | Shared kernel: base entity/DTO wrappers over vendor `Light.Domain` types, `ICurrentUser`/`IDateTime` abstractions, permission-based authorization building blocks, FluentValidation pipeline behavior, constants. Leaf project — no dependencies. |
| Infrastructure | `src/Infrastructure` | Cross-cutting infrastructure: EF Core provider configuration, audit/soft-delete tracking + domain-event dispatch helpers for `DbContext.SaveChanges`, CORS, health checks, Serilog bootstrap logging, Mapster config, module/endpoint base classes, API controller base classes. Depends on `Shared`. |
| Composition-root host | `src/Host` (or similar — not yet created) | Not yet built. `Infrastructure.InfrastructureModule.AddSharedInfrastructure`/`MapEndpoints` are ready to be called from one once it exists. |

## Dependency Graph

Verified from actual `.csproj` `ProjectReference` entries:

```text
Infrastructure -> Shared
Framework.Tests (tests/) -> Shared
Framework.Tests (tests/) -> Infrastructure
```

`Shared` has no project references (leaf). No cross-module boundary violations possible yet — no modules exist.

## Entry Points

None yet — no host project/`Program.cs` exists under `src/`.

## Data Access

No module `DbContext` exists yet. `src/Infrastructure/Database/BaseDbContext.cs` is the intended shared base class (applies a Sqlite `DateTimeOffset`/`DateTimeOffset?` conversion fix in `OnModelCreating`, via `SqliteDbContextExtensions.FixDateTimeOffsetSqlite`). `DbContextExtensions.GetDbProvider`/`AddConfiguredDbContext<TContext>` support `InMemory`/`PostgreSQL`/`MSSQL`/`Sqlite`, selected via `IConfiguration["DbProvider"]`.

## External Dependencies

- **`Lightsoft.*` (namespace `Light.*`)** — private vendor package family: `Lightsoft.Mediator`/`Lightsoft.Mediator.Contracts` (mediator + `INotification`/`IPublisher`), `Lightsoft.Result` (`Result`/`Result<T>`/`Paged<T>`/`PagedResult<T>` contracts), `Lightsoft.SharedKernel` (`Light.Domain` base entity/value-object types), `Lightsoft.AspNetCore.Authorization` (permission-policy authorization), `Lightsoft.AspNetCore.Modularity` (`IModuleEndpoint`, `AppModule` base), `Lightsoft.AspNetCore.Extensions` (CORS helpers), `Lightsoft.EntityFrameworkCore`, `Lightsoft.Serilog`, `Lightsoft.FileGenerator`, `Lightsoft.EventBus`.
- **EF Core providers**: `Microsoft.EntityFrameworkCore.InMemory`/`.Sqlite`/`.SqlServer`, `Npgsql.EntityFrameworkCore.PostgreSQL`, `SQLitePCLRaw.bundle_e_sqlite3` — all referenced by `Infrastructure` only.
- **`FluentValidation`** (`Shared`) — backs `ValidationBehaviour<,>`.
- **`Mapster`** (`Shared`) — object-mapping; configured in `Infrastructure/Mappings/MapsterSettings.cs`.
- **`AspNetCore.HealthChecks.UI.Client`** (`Infrastructure`) — health check endpoint response formatting.
- **`Asp.Versioning`** (`Infrastructure`) — API versioning, used by `VersionedApiController`.

## Client Integration

None yet — no `clients/` app exists.

## Notes

<!-- manual: content below this line is human-authored and must be preserved verbatim during sync -->

---
_Generated: 2026-07-29 — scope: backend solution — see .claude/CLAUDE.md for update rules._
