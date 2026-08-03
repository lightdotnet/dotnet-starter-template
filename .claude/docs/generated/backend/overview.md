# Backend Solution Overview

## Purpose

ASP.NET Core (C#) Modular Monolith backend for the StarterKit template. The pre-module shared kernel (`src/Shared`, `src/Infrastructure`) has since gained `src/Persistence` (EF Core concerns split out of `Infrastructure`), the first real business module — `Identity` (`src/Identity.Api` + `src/Identity.Contracts`) — and a working composition-root host, `src/StarterKit.WebApi` (`Program.cs`).

## Modules

| Module | Path | Responsibility | Status |
|---|---|---|---|
| Identity | `src/Identity.Api/Identity.Api.csproj` + `src/Identity.Contracts/Identity.Contracts.csproj` | Users, roles, claims, JWT auth/token issuance, plus a permission catalog (`PermissionsController`, `GET permissions`). Single-project module (`Entities/`, `Data/`, `Application/`, `Services/`, `Jwt/`, `Controllers/` folders in `Identity.Api`) plus a `Contracts` seam project (DTOs, `IUserService`/`IRoleService`/`IServiceClaimService`, and now `Authorization/IdentityPermissionProvider` — an `IPermissionDefinitionProvider` implementation listing the module's definable permissions). `.Api` suffix kept deliberately — anticipated candidate for future extraction into an independent identity service. | Built, but internal layering is still informal — CQRS commands and traditional service classes coexist for what should be one pattern. Now has automated test coverage via `tests/Identity.Tests` (98 tests: `Extensions/`, `Jwt/`, `Entities/`, `Services/`, `Controllers/`) — `Identity.Api.csproj` grants it `InternalsVisibleTo` to reach the module's `internal` JWT orchestration classes. |

## Shared/Host Projects

| Project | Path | Responsibility |
|---|---|---|
| Shared | `src/Shared` | Shared kernel: base entity/DTO wrappers over vendor `Light.Domain` types, `ICurrentUser`/`IDateTime` abstractions, permission-based authorization building blocks (`SuperUserPolicy`, `AccessControl`, `CurrentUserBase`, `AuthorizationHandler`), FluentValidation pipeline behavior, constants. Leaf project — no dependencies. |
| Infrastructure | `src/Infrastructure` | Cross-cutting infrastructure: CORS, health checks, Serilog bootstrap logging (`AppLogging`), Mapster config, module/endpoint base classes (`AppModule`, `AppModuleEndpoint`), API controller base classes, Basic Auth attribute. Depends on `Shared`. EF Core/DbContext concerns moved out to `Persistence` during the 2026-07 refactor — no longer here. |
| Persistence | `src/Persistence` | EF Core provider configuration (`DbContextExtensions`/`DbProvider`, Sqlite `DateTimeOffset` workaround), DbContext base class (`Context/BaseDbContext`), audit/soft-delete tracking + domain-event dispatch (meant to run inside each module's `SaveChangesAsync`), generic paging/result helpers, migration-time runtime support. Depends on `Shared`. |
| StarterKit.WebApi | `src/StarterKit.WebApi` | Composition-root host — the only executable/deployable project. Depends on `Identity.Api`, `Infrastructure`, `Shared`. |

## Dependency Graph

Verified from actual `.csproj` `ProjectReference` entries:

```text
Infrastructure -> Shared
Persistence -> Shared
Identity.Contracts -> Shared
Identity.Api -> Identity.Contracts
Identity.Api -> Infrastructure
Identity.Api -> Persistence
StarterKit.WebApi -> Identity.Api
StarterKit.WebApi -> Infrastructure
StarterKit.WebApi -> Shared
Framework.Tests (tests/) -> Shared
Framework.Tests (tests/) -> Infrastructure
Framework.Tests (tests/) -> Persistence
Identity.Tests (tests/) -> Identity.Api
Identity.Tests (tests/) -> Shared
```

`Shared` remains a leaf. `Identity.Contracts` — the first real example of the per-module `Contracts` seam — is **no longer a leaf**: it gained a `ProjectReference` to `Shared` alongside `IdentityPermissionProvider` (see Data Access/External Dependencies below), and this reference exists purely to reach `Shared`'s transitive `Lightsoft.AspNetCore.Authorization` package — `Identity.Contracts.csproj` declares no `PackageReference` of its own for it (see `dependency-graph.md`'s "Undeclared transitive dependency" note). No cross-module boundary violations found — `Identity` is still the only module, so the "modules reference only each other's `Contracts`" rule is unverified in practice (no second module exists to test it against). `Identity.Tests` is a second, separate test project (alongside `Framework.Tests`) that now covers the Identity module.

## Entry Points

`src/StarterKit.WebApi/Program.cs` — the composition-root host. Builds a `WebApplication`, configures Serilog bootstrap logging, calls `builder.Services.ConfigureServices(builder.Configuration)`, wires `AddLowercaseControllers`/`AddDefaultJsonOptions`/`AddInvalidModelStateHandler`, then `app.ConfigurePipelines()`, `app.UseWebSockets()`, and `app.MapEndpoints(builder.Configuration.GetValue<bool>("AllowAnonymous"))` before `app.Run()`.

## Data Access

One `DbContext` per module is the intended default.

| Module | DbContext | Provider | Notes |
|---|---|---|---|
| Identity | `IdentityDbContext` (`src/Identity.Api/Data/IdentityDbContext.cs`, renamed from `AppIdentityDbContext`) | Configured via `Persistence.DbContextExtensions.AddConfiguredDbContext`/`GetDbProvider` (`InMemory`/`PostgreSQL`/`MSSQL`/`Sqlite`, selected via `IConfiguration["DbProvider"]`; default in `appsettings.json` is `MSSQL`, pointing at a local `(localdb)\mssqllocaldb` instance) | Extends ASP.NET Identity's `IdentityDbContext<...>` directly (can't also extend `Persistence/Context/BaseDbContext.cs` — single inheritance), so it re-applies the Sqlite `DateTimeOffset` fix manually. Soft-delete is still passed as `enableSoftDelete: false` despite `User` implementing `ISoftDelete` — flagged as a likely bug (D2) in `reviews/2026-07-30-backend-project-analysis.md`. `User` now has an index on `Created` and `UserSessions` an index on `UserId` (`EntityBuilderExtensions.BuildEntities`) — MSSQL migrations were reset to a fresh baseline this session (`src/Migrations/MSSQL/Identity/20260801104131_CreateIdentitySchema.cs`); Sqlite/PostgreSQL got an incremental `AddUserCreatedIndex` migration on top of their existing baseline. |

## External Dependencies

- **`Lightsoft.*` (namespace `Light.*`)** — private vendor package family: `Lightsoft.Mediator`/`.Contracts` (mediator + `INotification`/`IPublisher`), `Lightsoft.Result` (`Result`/`Result<T>`/`Paged<T>`/`PagedResult<T>`), `Lightsoft.SharedKernel` (`Light.Domain` base types), `Lightsoft.AspNetCore.Authorization` (permission-policy authorization), `Lightsoft.AspNetCore.Modularity` (`IModuleEndpoint`, `AppModule`), `Lightsoft.AspNetCore.Extensions` (CORS helpers), `Lightsoft.AspNetCore.Swagger` (host only), `Lightsoft.EntityFrameworkCore`, `Lightsoft.Serilog`, `Lightsoft.FileGenerator`, `Lightsoft.EventBus`, `Lightsoft.ActiveDirectory` (Identity module only).
- **EF Core providers** (`Persistence`): `Microsoft.EntityFrameworkCore.InMemory`/`.Sqlite`/`.SqlServer`, `Npgsql.EntityFrameworkCore.PostgreSQL`, `SQLitePCLRaw.bundle_e_sqlite3`.
- **`Microsoft.AspNetCore.Identity.EntityFrameworkCore`** (`Identity.Api`) — ASP.NET Identity base types (`IdentityUser`, `IdentityRole`, `IdentityDbContext<...>`) that the module's entities/`AppIdentityDbContext` extend.
- **`FluentValidation`** (`Shared`) — backs `ValidationBehaviour<,>`; **`FluentValidation.DependencyInjectionExtensions`** (host) registers validators.
- **`Mapster`** (`Shared`) — object mapping; configured in `Infrastructure/Mappings/MapsterSettings.cs`.
- **`AspNetCore.HealthChecks.UI.Client`** (`Infrastructure`, host) — health check endpoint response formatting.
- **`Spectre.Console`** (host) — startup console banner (`Program.cs`).

## Client Integration

`clients/admin/` (a Next.js admin dashboard app) has since been scaffolded, but its actual API integration was not inspected as part of this backend-only sync — see `.claude/docs/generated/clients/admin/` (once generated) for that side.

## Notes

<!-- manual: content below this line is human-authored and must be preserved verbatim during sync -->

---
_Generated: 2026-08-03 (resynced — added `PermissionsController`/`IdentityPermissionProvider` permission catalog; `Identity.Contracts` is no longer a leaf project) — scope: backend solution — see .claude/CLAUDE.md for update rules._
