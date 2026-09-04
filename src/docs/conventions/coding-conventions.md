# Coding Conventions: Backend

## Build & Tooling

- Target framework: `net10.0` across all backend projects (`src/Shared`, `src/Infrastructure`, `src/Persistence`, `src/Identity.Contracts`, `src/Identity.Api`, `src/Notifications.Contracts`, `src/Notifications.Api`, `src/StarterKit.WebApi`, `tests/Framework.Tests`, `tests/Identity.Tests`).
- Central package management via `Directory.Packages.props` (`ManagePackageVersionsCentrally=true`, `CentralPackageTransitivePinningEnabled=false`), except `tests/Framework.Tests/Framework.Tests.csproj` and `tests/Identity.Tests/Identity.Tests.csproj`, which both opt out (`ManagePackageVersionsCentrally=false`) and pin their own test package versions (3 for `Framework.Tests`; 4 for `Identity.Tests`, which adds `Moq`).
- `Directory.Build.props` sets `ImplicitUsings=enable` and `Nullable=enable` repo-wide.
- Module structure convention: flat projects directly under `src/` (no `src/Modules/` nesting), plus a `<Module>.Contracts` seam project per module — see [../architecture/architecture.md § Layering](../architecture/architecture.md#layering). Two modules built so far, both single-project: `Identity` (`Identity.Api` + `Identity.Contracts`) and `Notifications` (`Notifications.Api` + `Notifications.Contracts`) — per-module detail (including deviations from this convention) now lives in [../architecture/modules/Identity.md](../architecture/modules/Identity.md) / [../architecture/modules/Notifications.md](../architecture/modules/Notifications.md).
- Vendor library family: `Lightsoft.*` (namespace `Light.*`) supplies the mediator, `Result`/`Paged` contracts, domain base types, ASP.NET Core authorization/modularity/CORS/Swagger helpers, EF Core helpers, and Serilog setup — treat as fixed external API, not renameable/refactorable project code.

## Style

- No root `.editorconfig` found — re-checked, still not present.
- Nullable reference types enabled everywhere.
- File-scoped namespaces (`namespace X;`) used consistently — no block-scoped `namespace X { }`.
- PascalCase for all constants and enum members (no `SCREAMING_SNAKE_CASE`).
- Folder names mirror the trailing namespace segment (e.g. `Authorization/` ⇒ `StarterKit.Authorization`).
- CQRS command/query types: `internal sealed record` (never `public` — the controller is in the same assembly and the type is not part of any seam), one file per feature named after the feature (`CreateUser.cs`, not `CreateUserCommand.cs`), holding the command/query and its handler. A command/query wraps the `Contracts` request DTO (`CreateUserCommand(CreateUserRequest Model)`) or takes primitives for trivial payloads (`DeleteUserCommand(string Id)`) — the controller binds the `Contracts` DTO and constructs the command/query, keeping the mediator type internal.

## Structural Conventions

- DI registration via small `static class DependencyInjection` (or similarly named) extension classes exposing `Add<Feature>`/`Use<Feature>` methods, one per feature area/folder.
- Error handling/result pattern: vendor `Light.Contracts.Result`/`Result<T>`/`PagedResult<T>` for expected-failure outcomes; `Light.Exceptions.ValidationException` (thrown by `ValidationBehaviour<,>`) for validation failures.
- Mediator pipeline behaviors (registered in `StarterKit.WebApi/ConfigureExtensions.cs`, outermost first): `LoggingBehaviour<,>` (`src/Shared` — logs request type name + elapsed time only, never request/response bodies, which can carry secrets), then `ValidationBehaviour<,>` (FluentValidation).
- Logging: `AppLogging` static Serilog logger for bootstrap/startup messages (`Information`/`Warning`); standard `ILogger<T>` DI for request/runtime logging elsewhere.
- Module layering conventions: two modules exist now (`Identity`, `Notifications`) — both single-project, both loosely conforming to the flat-project + `Contracts`-seam convention adopted 2026-07-30 (see [../architecture/architecture.md](../architecture/architecture.md)). `Notifications` additionally establishes an audience-split-controller pattern (admin vs. self-service) not present in `Identity` — see [../architecture/modules/Notifications.md](../architecture/modules/Notifications.md).
- Every module — single or split — gets a `<Module>.Contracts` project as its only externally-referenceable seam. `Notifications.Contracts` is a confirmed true leaf (only references `Shared`). `Identity.Contracts` is not a leaf — it has one `ProjectReference` (`Shared`) purely to reach a transitive package (see Deviations below) — so "seam project" no longer implies "leaf project" as a hard rule across all modules.

## Testing Conventions

- Test framework: xUnit v3 (`xunit.v3`, `xunit.runner.visualstudio`, `Microsoft.NET.Test.Sdk`).
- Naming: `<TypeUnderTest>Tests` classes; `MethodOrMember_ShouldExpectedBehavior_WhenCondition` test methods; `// Arrange`/`// Act`/`// Assert` comments.
- No mocking library in `Framework.Tests` — hand-written fakes/test doubles instead (e.g. `RecordingPublisher : IPublisher`, `TestCurrentUser : CurrentUserBase`). `Identity.Tests` is the first backend test project to add `Moq` (used for `UserManager<User>`, `IMediator`, `IPublisher`, and service-interface doubles), alongside a real Sqlite-in-memory + ASP.NET Identity stack (`TestSupport/IdentityTestHost`) for the majority of its service/Jwt tests rather than mocking EF Core itself.
- Test project layout: two test projects now exist under `tests/`. `Framework.Tests/<ProjectName>/...` mirrors `src/<ProjectName>` 1:1 (`Shared/`, `Infrastructure/`, `Persistence/` folders). `Identity.Tests/<Area>/...` mirrors `Identity.Api`'s own folder structure (`Application/` — with `Users/Commands`, `Users/Queries` — `Extensions/`, `Jwt/`, `Entities/`, `Services/`, `Controllers/`), plus a `TestSupport/` folder for shared test infrastructure (`IdentityTestHost`, `FakeCurrentUser`, `FakeDateTime`, `TestJwtOptions`). Controller tests mock `IMediator` (registered on the test `HttpContext.RequestServices`) and assert the dispatched command/query; handler tests exercise the handler directly. `InternalsVisibleTo` is set on `Shared.csproj`, `Infrastructure.csproj`, `Persistence.csproj` for `Framework.Tests`, and on `Identity.Api.csproj` for `Identity.Tests` (so the tests can reach the `internal` command/query types and JWT classes).
- **Coverage gap**: `Identity` has automated test coverage via `tests/Identity.Tests` (100 tests, `dotnet test tests/Identity.Tests/Identity.Tests.csproj`). `Notifications` has **no dedicated test project yet** — `Framework.Tests` doesn't reference it either, so the module is currently untested.

## Deviations From Norms Elsewhere in the Repo

Module-specific deviations now live in each module's own doc ([../architecture/modules/Identity.md](../architecture/modules/Identity.md) / [../architecture/modules/Notifications.md](../architecture/modules/Notifications.md) § Notable Conventions) — summarized here:

- `Identity.Api` routes all writes + user search through mediator commands/queries, but the handlers still delegate to `UserService`/`RoleService` (which hold the logic) rather than owning it — a half-migration, not the intended end state (see D1). `UserController` also exposes both an unbounded `GET user` and a paginated `GET user/search`; `Identity.Api`/`Identity.Contracts` each have one undeclared-transitive-package usage (`WhereIf` via `Persistence`, `IPermissionDefinitionProvider` via `Shared`). See [../architecture/modules/Identity.md](../architecture/modules/Identity.md).
- `Notifications.Contracts` repeats the same undeclared-transitive-package pattern (`Light.AspNetCore.Authorization` via `Shared`) — the second instance of this pattern in the repo; avoid it in future modules by declaring packages a project actually uses directly. See [../architecture/modules/Notifications.md](../architecture/modules/Notifications.md) and [../architecture/dependency-graph.md](../architecture/dependency-graph.md).

## Notes

<!-- manual: content below this line is human-authored and must be preserved verbatim during sync -->

---
_Last synced: 2026-09-04_
