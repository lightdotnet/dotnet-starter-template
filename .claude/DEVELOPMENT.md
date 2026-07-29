# Development Conventions

> Template. Fill in per-module/per-client-app conventions only as they're actually observed in code (via `analyze-solution`/`analyze-module`/`analyze-frontend`/`analyze-client`), not from assumption. This file aggregates verified conventions; it is not a style guide invented up front.

## How to Use This File

- Each section is split Backend / Clients; note deviations per-module or per-client-app rather than assuming repo-wide uniformity, unless verified (e.g. a root `.editorconfig`, a shared `Directory.Build.props`). Once more than one client app exists, don't assume they share conventions — each app gets its own subsection if they diverge.
- Do not add entries speculatively. If a convention hasn't been observed in actual code, leave the section marked `unknown`.

## Backend (`src/`)

### Build & Tooling

- **Target framework(s)**: `net10.0` (all of `src/Shared`, `src/Infrastructure`, `tests/Framework.Tests`).
- **Shared build props/targets**: `Directory.Build.props` sets `ImplicitUsings=enable` and `Nullable=enable` repo-wide. `Directory.Packages.props` centralizes all package versions (`ManagePackageVersionsCentrally=true`, `CentralPackageTransitivePinningEnabled=false`); `Framework.Tests.csproj` opts out (`ManagePackageVersionsCentrally=false`) and pins its own test package versions directly.
- **Central package management**: yes, repo-wide via `Directory.Packages.props` (except `Framework.Tests`, see above).
- **Module structure convention**: unverified — no `src/Modules/*` exists yet (expected: `src/Modules/<ModuleName>/{Domain,Application,Infrastructure,Api}` per `.claude/CLAUDE.md`).
- **Vendor library family**: a private NuGet package family `Lightsoft.*` (namespace `Light.*`) supplies the mediator (`Lightsoft.Mediator`), `Result`/`Paged` contracts (`Lightsoft.Result`), domain base types (`Lightsoft.SharedKernel`, namespace `Light.Domain`), ASP.NET Core authorization/modularity/CORS helpers, EF Core helpers, and Serilog setup. Treat these as fixed external API, not renameable/refactorable project code.

### Coding Style

- **`.editorconfig` present**: not found at repo root as of this scope.
- **Nullable reference types**: enabled repo-wide via `Directory.Build.props`.
- **Naming conventions observed**: file-scoped namespaces everywhere; PascalCase for all constants/enum members (no `SCREAMING_SNAKE_CASE`); folder name mirrors the last namespace segment (e.g. `Authorization/` → `StarterKit.Authorization`); one file-scoped namespace per file — no block-scoped `namespace X { }`.
- **Formatting/analyzer rules**: none beyond the SDK defaults observed so far.

### Module Layering Conventions

> Only document a layering convention once seen consistently across multiple modules — otherwise note it as local to one module. See [agents/architecture-reviewer.md](agents/architecture-reviewer.md).

- Unverified — no modules exist yet.

### Dependency Injection Patterns

- Registration is done via small `static class DependencyInjection` (or similarly-named static extension classes, e.g. `Cors/DependencyInjection.cs`, `Authorization/DependencyInjection.cs`) exposing `Add<Feature>`/`Use<Feature>` extension methods on `IServiceCollection`/`IApplicationBuilder`, following the standard ASP.NET Core convention. `InfrastructureModule.AddSharedInfrastructure`/`MapEndpoints` (`src/Infrastructure/InfrastructureModule.cs`) is the intended single entry point a future host project calls.

### Error Handling / Result Patterns

- Vendor `Light.Contracts.Result`/`Result<T>`/`PagedResult<T>` (from `Lightsoft.Result`) is the standard return-value pattern for operation outcomes (`Success`/`NotFound`/`BadRequest`/`Unauthorized`/`Forbidden`/`Conflict`/`Error` factory methods) — see `src/Infrastructure/Extensions/QueryableResultExtensions.cs`.
- Validation failures use FluentValidation via `ValidationBehaviour<TRequest,TResponse>` (`src/Shared/ValidationBehaviour.cs`), a mediator pipeline behavior that throws `Light.Exceptions.ValidationException` (property-name-keyed error dictionary) when any registered `IValidator<TRequest>` fails — each validator gets its own `ValidationContext<TRequest>` (not shared across concurrent validator runs).

### Logging Conventions

- Per-request/DI logging: standard `Microsoft.Extensions.Logging.ILogger<T>` (e.g. `MigrationsExtensions.MigrateDatabaseAsync`).
- Bootstrap/startup logging: `AppLogging` (`src/Infrastructure/AppLogging.cs`) — a static Serilog logger (console + rolling file at `logs\application-startup.txt`) with `Information`/`Warning` helper methods, used before/outside the DI-built host (e.g. `AppModule.ShowModuleInfo`/`ShowEndpointInfo`, `DbContextExtensions.AddConfiguredDbContext`).

### Testing Conventions

- **Test framework(s)**: xUnit v3 (`xunit.v3`, `xunit.runner.visualstudio`, `Microsoft.NET.Test.Sdk`) in `tests/Framework.Tests/Framework.Tests.csproj`.
- **Naming convention for test classes/methods**: `<TypeUnderTest>Tests` class names; `MethodOrMember_ShouldExpectedBehavior_WhenCondition` method names; `// Arrange`/`// Act`/`// Assert` comments inside each test.
- **Mocking library**: none — hand-written fakes/test doubles (e.g. a `RecordingPublisher : IPublisher`, a minimal `TestCurrentUser : CurrentUserBase`) instead of a mocking framework.
- **Test project layout**: `tests/Framework.Tests/<ProjectName>/...` mirrors the corresponding `src/<ProjectName>` folder structure 1:1 (e.g. `Shared/Authorization/`, `Infrastructure/Database/`), so a future framework/shared project just gets its own top-level folder named after it. EF Core-backed tests get a per-target-project `TestSupport/` folder (e.g. `Infrastructure/TestSupport/TestDbContext.cs`, `TestEntities.cs`) with minimal InMemory-provider fixtures. `InternalsVisibleTo` is set on both `Shared.csproj` and `Infrastructure.csproj` for `Framework.Tests` so `internal` types are directly testable.

### EF Core Conventions

> See also [agents/efcore-specialist.md](agents/efcore-specialist.md), [skills/efcore.md](skills/efcore.md).

- **Migration strategy**: unverified — no module `DbContext`/migrations exist yet. `MigrationsExtensions.MigrateDatabaseAsync` (`src/Infrastructure/Database/`) is the generic apply-pending-migrations helper intended for a host's startup path.
- **Configuration style** (Fluent API vs. attributes): unverified — no entity configurations exist yet outside the base wrappers.
- **Naming conventions for tables/columns**: unverified.
- **DbContext-per-module boundary respected**: unverified — no module `DbContext` exists yet; `BaseDbContext` (`src/Infrastructure/Database/BaseDbContext.cs`) is the intended shared base (applies the Sqlite `DateTimeOffset` conversion fix).
- **Provider selection**: `DbContextExtensions.GetDbProvider`/`AddConfiguredDbContext<TContext>` read an `IConfiguration["DbProvider"]` enum value (`InMemory`/`PostgreSQL`/`MSSQL`/`Sqlite`) and configure the matching EF Core provider.

### API Conventions

> See also [agents/api-designer.md](agents/api-designer.md), [skills/api.md](skills/api.md).

- **Versioning strategy**: `Asp.Versioning` is referenced; `VersionedApiController` (`src/Infrastructure/Endpoints/`) is decorated `[ApiVersion("1.0")]` as the versioned controller base — no actual versioned routes exist yet (no controllers/modules built).
- **Response/error contract shape**: unverified at the HTTP level — no controllers exist yet; the `Result`/`PagedResult<T>` pattern above is what controllers are expected to return.
- **Controllers are API-only (no Razor views)**: unverified — no controllers exist yet, only the base classes (`ApiControllerBase`, `VersionedApiController`) and a `BasicAuthAttribute` authorization filter.

## Clients (`clients/<app-name>/`)

> There may be more than one app under `clients/` — repeat this subsection per app if their conventions diverge (e.g. `clients/web/` vs a future `clients/admin/`). Don't assume a convention observed in one app applies to another until verified.

### `clients/web/` (primary app — rename/duplicate this heading once other apps exist)

#### Build & Tooling

- **Next.js version / router (App vs Pages)**: _unknown_
- **Package manager**: _unknown_
- **`tsconfig.json` strictness**: _unknown_

#### Coding Style

- **Linting** (ESLint config): _unknown_
- **Formatting** (Prettier or other): _unknown_
- **Component naming/file organization**: _unknown_

#### Data Fetching & State

- **Server vs client components usage**: _unknown_
- **Data fetching library** (React Query/SWR/native fetch/server actions): _unknown_
- **Global state management**: _unknown_

#### Styling

- **Styling approach** (Tailwind/CSS Modules/other): _unknown_

#### Testing Conventions

- **Test framework(s)** (Jest/Vitest + Testing Library, Playwright, etc.): _unknown_
- **Naming convention**: _unknown_

## Full-Stack Integration Conventions

> See also [agents/api-contract-reviewer.md](agents/api-contract-reviewer.md), [skills/nextjs.md](skills/nextjs.md). If multiple clients exist, note per-client deviations explicitly rather than assuming they all integrate the same way.

- **API client generation** (hand-written vs generated from OpenAPI), per client app: _unknown_
- **Environment/config for API base URL**, per client app: _unknown_
- **Auth token handling between each client and the backend**: _unknown_
- **Error contract mapping** (backend error shape → client handling): _unknown_

## Versioning & Release

- **Versioning scheme**: _unknown_
- **Changelog convention**: _unknown_

---
_Last updated: 2026-07-29 — backend section covers `src/Shared`, `src/Infrastructure`, `tests/Framework.Tests`; Clients/Full-Stack Integration/Versioning sections still unpopulated (no `clients/` app exists yet)._
