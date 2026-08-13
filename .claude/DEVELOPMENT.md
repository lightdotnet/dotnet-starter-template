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
- **Module structure convention**: flat projects directly under `src/` (no `src/Modules/` nesting), plus a `<Module>.Contracts` seam project per module — see `.claude/ARCHITECTURE-BACKEND.md § Module Structure Convention`. Two modules built so far: `Identity` (first, single project, `Identity.Api` + `Identity.Contracts`; internal layering informal, not yet fully conformant) and `Notifications` (second, single project, `Notifications.Api` + `Notifications.Contracts`; controllers split by audience rather than resource) — see `.claude/ARCHITECTURE-BACKEND.md` for details.
- **Vendor library family**: a private NuGet package family `Lightsoft.*` (namespace `Light.*`) supplies the mediator (`Lightsoft.Mediator`), `Result`/`Paged` contracts (`Lightsoft.Result`), domain base types (`Lightsoft.SharedKernel`, namespace `Light.Domain`), ASP.NET Core authorization/modularity/CORS helpers, EF Core helpers, and Serilog setup. Treat these as fixed external API, not renameable/refactorable project code.

### Coding Style

- **`.editorconfig` present**: not found at repo root as of this scope.
- **Nullable reference types**: enabled repo-wide via `Directory.Build.props`.
- **Naming conventions observed**: file-scoped namespaces everywhere; PascalCase for all constants/enum members (no `SCREAMING_SNAKE_CASE`); folder name mirrors the last namespace segment (e.g. `Authorization/` → `StarterKit.Authorization`); one file-scoped namespace per file — no block-scoped `namespace X { }`.
- **Formatting/analyzer rules**: none beyond the SDK defaults observed so far.

### Module Layering Conventions

> Only document a layering convention once seen consistently across multiple modules — otherwise note it as local to one module. See [agents/architecture-reviewer.md](agents/architecture-reviewer.md).

- Both modules built so far (`Identity`, `Notifications`) are single-project, folder-organized (no Domain/Application/Infrastructure/Api split yet in this repo) — controllers call only into services/mediator, never directly into entities/DbContext. This discipline holds in both but isn't compiler-enforced (single assembly per module).
- **Not consistent across modules**: `Identity`'s controllers split by *resource* (`UserController`, `RoleController`, `TokenController`); `Notifications`' split by *audience* (`NotificationController` admin vs. `UserNotificationController` self-service) over the same table. Treat as two valid patterns depending on the module's shape, not a convention violation.
- `Identity` mixes CQRS commands and traditional service classes for what should be one approach (open finding, not yet reconciled); `Notifications` uses one service class throughout, no CQRS.
- See `.claude/ARCHITECTURE-BACKEND.md § Backend — Layering (per module)` for the full per-module table.

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

- **Migration strategy**: `MigrationsExtensions.MigrateDatabaseAsync` (`src/Persistence/MigrationSupport/`, moved from `src/Infrastructure/Database/` in the 2026-07 refactor) is the generic apply-pending-migrations helper called from the host's startup path. Design-time EF migration projects live separately under top-level `src/Migrations/{Sqlite,PostgreSQL,MSSQL}`, one per provider, not colocated with each module.
- **Configuration style** (Fluent API vs. attributes): Fluent API, via each `DbContext`'s model configuration (e.g. `EntityBuilderExtensions.BuildEntities` for `IdentityDbContext`) — no attribute-based (`[Column]`/`[Table]`) configuration observed.
- **Naming conventions for tables/columns**: not yet documented as a repo-wide rule — verify per module (`Notifications` uses schema `"system"`, table `Notifications`; `Identity` uses ASP.NET Identity's own default table names).
- **DbContext-per-module boundary respected**: yes, verified with two modules. `IdentityDbContext` (`src/Identity.Api/Data/`) extends ASP.NET Identity's own `IdentityDbContext<...>` (can't also extend `BaseDbContext` — single inheritance; re-applies the Sqlite `DateTimeOffset` fix manually). `NotificationDbContext` (`src/Notifications.Api/Data/`) does extend `Persistence/Context/BaseDbContext.cs` (`src/Persistence/`, moved from `Infrastructure` during the 2026-07 refactor). Both currently share the same physical database/connection string (`DbConnectionNames.Default`), separated by schema/table only — "one DbContext per module, shared physical DB" is the confirmed default, not full physical isolation.
- **Provider selection**: `DbContextExtensions.GetDbProvider`/`AddConfiguredDbContext<TContext>` (`src/Persistence/`) read an `IConfiguration["DbProvider"]` enum value (`InMemory`/`PostgreSQL`/`MSSQL`/`Sqlite`) and configure the matching EF Core provider — used by both `IdentityDbContext` and `NotificationDbContext`.
- **Audit/soft-delete**: `TrackingExtensions.AuditEntries` (`src/Persistence/Extensions/`), called from each module's own `SaveChanges[Async]`, stamps `Created`/`LastModified`/`*By` and applies soft-delete via `ISoftDelete`. `Identity`'s `User` implements `ISoftDelete` but passes `enableSoftDelete: false` (flagged as a likely bug, currently dead code); `Notifications`' `Notification` doesn't implement `ISoftDelete` at all (no soft-delete support, not a bug).
- See `.claude/ARCHITECTURE-BACKEND.md § Backend — Data Access` for full per-module detail.

### API Conventions

> See also [agents/api-designer.md](agents/api-designer.md), [skills/api.md](skills/api.md).

- **Versioning strategy**: `Asp.Versioning` via `VersionedApiController` (`src/Infrastructure/Endpoints/`, `[ApiVersion("1.0")]`) as the versioned controller base. Both modules' controllers use the `api/v{version:apiVersion}/...` URL-segment convention (confirmed via the admin client's actual call sites) — header vs. URL negotiation and default-version behavior beyond that are still unverified.
- **Response/error contract shape**: vendor `Light.Contracts.Result`/`Result<T>`/`PagedResult<T>` (`Lightsoft.Result`) is the standard controller return type, auto-wrapped into the response envelope by `ApiControllerBase`/`VersionedApiController`'s `Ok<T>()` helpers — a bare-looking service return type is not a bug, it's wrapped before reaching the client. Confirmed consumed as such by the admin client's `types/api.ts` (`Result`/`ApiResponse`/`Paged`/`PagedResult`).
- **Controllers are API-only (no Razor views)**: confirmed — `UserController`/`RoleController`/`TokenController`/`UserProfileController` (`Identity`) and `NotificationController`/`UserNotificationController` (`Notifications`) are all JSON-only MVC controllers, no views.
- **Route-splitting pattern**: two controllers can front the same table for different audiences, gated differently — `Notifications`' `NotificationController` (admin, `[MustHavePermission]`) vs. `UserNotificationController` (self-service, permission-less, hard-scoped server-side via `ICurrentUser.UserId`). Worth reusing for a future module needing both an admin and a self-service view.

## Clients (`clients/<app-name>/`)

> There may be more than one app under `clients/` — repeat this subsection per app if their conventions diverge. Don't assume a convention observed in one app applies to another until verified. Full detail lives in `docs/generated/clients/<app>/*.md`; this section keeps only a summary.

### `clients/admin/` (first and, so far, only client app)

#### Build & Tooling

- **Next.js version / router**: 16.2.12, App Router, rooted at `src/app/` (no `pages/` directory).
- **Package manager**: pnpm (`pnpm-lock.yaml`). A `pnpm-workspace.yaml` exists but only configures build-script approval, not a multi-package workspace.
- **`tsconfig.json` strictness**: `"strict": true`, target `ES2017`, module resolution `"bundler"`, path alias `"@/*"` → `"./src/*"`.

#### Coding Style

- **Linting**: ESLint 9 flat config (`eslint.config.mjs`), `eslint-config-next`'s `core-web-vitals` + `typescript` rule sets.
- **Formatting**: `prettier` + `prettier-plugin-tailwindcss` are devDependencies, but no `.prettierrc*` and no `format` script exist — unverified whether formatting is actually enforced anywhere.
- **Component naming/file organization**: kebab-case filenames, one primary export per file matching a PascalCase component/function name; feature-folder layout (`features/<name>/{api,components,types,constants,hooks}` + a mandatory `index.ts` barrel), see `docs/generated/clients/admin/coding-conventions.md § Structural Conventions`.

#### Data Fetching & State

- **Server vs client components usage**: mostly Server Components (whole-list reads, session resolution); `"use client"` applied to files using hooks/state/browser APIs (most of `hooks/*`, `components/theme/*`, `components/layout/*` except `brand.tsx`, most feature components with interactivity).
- **Data fetching library**: none (no React Query/SWR) — hand-written per-endpoint functions under `features/<name>/api/*.ts`, one file per backend call, via `lib/server/backend-api.ts`/`lib/server/http.ts`. Writes go through Next.js Server Actions (`"use server"`). Real-time notifications use a direct `@microsoft/signalr` WebSocket connection from the browser, not a fetch-based library.
- **Global state management**: none — local component state + React Context per concern (`SidebarProvider`, `AccentColorProvider`, `ThemeProvider`, `NotificationsProvider`), no Redux/Zustand/etc.

#### Styling

- **Styling approach**: Tailwind CSS v4, CSS-first config (`src/app/globals.css`'s `@import "tailwindcss"` + `@theme inline`), no `tailwind.config.ts`. shadcn-CLI-generated primitives under `components/ui/*` on top of `radix-ui` + `class-variance-authority`.

#### Testing Conventions

- **Test framework(s)**: none installed — no Jest/Vitest/Playwright/Testing Library, no `*.test.*`/`*.spec.*` files, no `test` script. Notable gap given real auth/session/CRUD logic is in place untested.
- **Naming convention**: not applicable (no test suite exists yet).

## Full-Stack Integration Conventions

> See also [agents/api-contract-reviewer.md](agents/api-contract-reviewer.md), [skills/nextjs.md](skills/nextjs.md). If multiple clients exist, note per-client deviations explicitly rather than assuming they all integrate the same way.

- **API client generation** (`admin`): hand-written, one file per feature under `features/<name>/api/<feature>.api.ts` (e.g. `users.api.ts`) — no OpenAPI-generated client.
- **Environment/config for API base URL** (`admin`): two named backend clients via `lib/server/api-clients.ts`'s `ApiClients` registry (`Identity`, `Notifications`), each with its own server-only base-URL env var (`IDENTITY_API_BASE_URL`/`NOTIFICATIONS_API_BASE_URL`, never `NEXT_PUBLIC_`) resolved via `lib/server/config.ts`; the base URL itself now owns the version prefix (e.g. `api/v1/`) rather than `http.ts` hardcoding it. Real-time notifications additionally need `NEXT_PUBLIC_SIGNALR_HUB_URL` (client-exposed, absolute URL — the browser connects directly to the backend's `/signalr-hub`, requiring backend CORS for the admin origin).
- **Auth token handling** (`admin` ↔ backend): cookie-based session (`admin_session`, httpOnly, AES-256-GCM encrypted via `lib/server/token-cipher.ts`), decoded permissions/roles straight from the access-token JWT. A request-handler pipeline (`lib/server/backend-api.ts` + `lib/server/http-handlers/bearer-token-handler.ts`) auto-attaches `Authorization: Bearer <accessToken>`; `src/proxy.ts` proactively refreshes a near-expiry token on every request. SignalR's handshake gets a short-lived access token handed to the browser via a dedicated Server Action (`getSignalRTokenAction`) — the one deliberate point the token leaves the httpOnly-cookie boundary.
- **Error contract mapping** (`admin`): `lib/server/call-guard.ts`'s `guardCall`/`guardResponseCall`/`guardRawCall` normalize network failures, non-2xx responses, and non-JSON bodies into the same `Result`/`ApiResponse`-shaped envelope the backend itself returns (`types/api.ts`), preferring a real backend-authored message (`ApiResponse.message` → `ValidationProblemDetails.errors` → `ProblemDetails.title`) over a generic fallback.

## Versioning & Release

- **Versioning scheme**: _unknown_
- **Changelog convention**: _unknown_

---
_Last synced: 2026-08-13. Versioning & Release section remains unpopulated — no evidence observed yet._
