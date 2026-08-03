# Coding Conventions: Backend

## Build & Tooling

- Target framework: `net10.0` across all backend projects (`src/Shared`, `src/Infrastructure`, `src/Persistence`, `src/Identity.Contracts`, `src/Identity.Api`, `src/Notifications.Contracts`, `src/Notifications.Api`, `src/StarterKit.WebApi`, `tests/Framework.Tests`, `tests/Identity.Tests`).
- Central package management via `Directory.Packages.props` (`ManagePackageVersionsCentrally=true`, `CentralPackageTransitivePinningEnabled=false`), except `tests/Framework.Tests/Framework.Tests.csproj` and `tests/Identity.Tests/Identity.Tests.csproj`, which both opt out (`ManagePackageVersionsCentrally=false`) and pin their own test package versions (3 for `Framework.Tests`; 4 for `Identity.Tests`, which adds `Moq`).
- `Directory.Build.props` sets `ImplicitUsings=enable` and `Nullable=enable` repo-wide.
- Module structure convention: flat projects directly under `src/` (no `src/Modules/` nesting), plus a `<Module>.Contracts` seam project per module — see `.claude/ARCHITECTURE-BACKEND.md § Module Structure Convention`. Two modules built so far, both single-project: `Identity` (`Identity.Api` + `Identity.Contracts`) and `Notifications` (`Notifications.Api` + `Notifications.Contracts`) — per-module detail (including deviations from this convention) now lives in `modules/Identity.md` / `modules/Notifications.md`.
- Vendor library family: `Lightsoft.*` (namespace `Light.*`) supplies the mediator, `Result`/`Paged` contracts, domain base types, ASP.NET Core authorization/modularity/CORS/Swagger helpers, EF Core helpers, and Serilog setup — treat as fixed external API, not renameable/refactorable project code.

## Style

- No root `.editorconfig` found — re-checked, still not present.
- Nullable reference types enabled everywhere.
- File-scoped namespaces (`namespace X;`) used consistently — no block-scoped `namespace X { }`.
- PascalCase for all constants and enum members (no `SCREAMING_SNAKE_CASE`).
- Folder names mirror the trailing namespace segment (e.g. `Authorization/` ⇒ `StarterKit.Authorization`).

## Structural Conventions

- DI registration via small `static class DependencyInjection` (or similarly named) extension classes exposing `Add<Feature>`/`Use<Feature>` methods, one per feature area/folder.
- Error handling/result pattern: vendor `Light.Contracts.Result`/`Result<T>`/`PagedResult<T>` for expected-failure outcomes; `Light.Exceptions.ValidationException` (thrown by `ValidationBehaviour<,>`) for validation failures.
- Logging: `AppLogging` static Serilog logger for bootstrap/startup messages (`Information`/`Warning`); standard `ILogger<T>` DI for request/runtime logging elsewhere.
- Module layering conventions: two modules exist now (`Identity`, `Notifications`) — both single-project, both loosely conforming to the flat-project + `Contracts`-seam convention adopted 2026-07-30 (see `.claude/ARCHITECTURE-BACKEND.md`). `Notifications` additionally establishes an audience-split-controller pattern (admin vs. self-service) not present in `Identity` — see `modules/Notifications.md`.
- Every module — single or split — gets a `<Module>.Contracts` project as its only externally-referenceable seam. `Notifications.Contracts` is a confirmed true leaf (only references `Shared`). `Identity.Contracts` is not a leaf — it has one `ProjectReference` (`Shared`) purely to reach a transitive package (see Deviations below) — so "seam project" no longer implies "leaf project" as a hard rule across all modules.

## Testing Conventions

- Test framework: xUnit v3 (`xunit.v3`, `xunit.runner.visualstudio`, `Microsoft.NET.Test.Sdk`).
- Naming: `<TypeUnderTest>Tests` classes; `MethodOrMember_ShouldExpectedBehavior_WhenCondition` test methods; `// Arrange`/`// Act`/`// Assert` comments.
- No mocking library in `Framework.Tests` — hand-written fakes/test doubles instead (e.g. `RecordingPublisher : IPublisher`, `TestCurrentUser : CurrentUserBase`). `Identity.Tests` is the first backend test project to add `Moq` (used for `UserManager<User>` and service-interface doubles), alongside a real Sqlite-in-memory + ASP.NET Identity stack (`TestSupport/IdentityTestHost`) for the majority of its service/Jwt tests rather than mocking EF Core itself.
- Test project layout: two test projects now exist under `tests/`. `Framework.Tests/<ProjectName>/...` mirrors `src/<ProjectName>` 1:1 (`Shared/`, `Infrastructure/`, `Persistence/` folders). `Identity.Tests/<Area>/...` mirrors `Identity.Api`'s own folder structure (`Extensions/`, `Jwt/`, `Entities/`, `Services/`, `Controllers/`), plus a `TestSupport/` folder for shared test infrastructure (`IdentityTestHost`, `FakeCurrentUser`, `FakeDateTime`, `TestJwtOptions`). `InternalsVisibleTo` is set on `Shared.csproj`, `Infrastructure.csproj`, `Persistence.csproj` for `Framework.Tests`, and on `Identity.Api.csproj` for `Identity.Tests`.
- **Coverage gap**: `Identity` has automated test coverage via `tests/Identity.Tests` (98 tests, `dotnet test tests/Identity.Tests/Identity.Tests.csproj`). `Notifications` has **no dedicated test project yet** — `Framework.Tests` doesn't reference it either, so the module is currently untested.

## Deviations From Norms Elsewhere in the Repo

Module-specific deviations now live in each module's own doc (`modules/Identity.md` / `modules/Notifications.md` § Notable Conventions) — summarized here:

- `Identity.Api` mixes a CQRS command with traditional service classes for what should be one consistent approach; `UserController` exposes both an unbounded `GET user` and a paginated `GET user/search`; `Identity.Api`/`Identity.Contracts` each have one undeclared-transitive-package usage (`WhereIf` via `Persistence`, `IPermissionDefinitionProvider` via `Shared`). See `modules/Identity.md`.
- `Notifications.Contracts` repeats the same undeclared-transitive-package pattern (`Light.AspNetCore.Authorization` via `Shared`) — the second instance of this pattern in the repo; avoid it in future modules by declaring packages a project actually uses directly. See `modules/Notifications.md` and `dependency-graph.md`.

## Notes

<!-- manual: content below this line is human-authored and must be preserved verbatim during sync -->

---
_Generated: 2026-08-03 (resynced — added `Notifications` module; module-specific deviations split out to `modules/Identity.md` and `modules/Notifications.md`) — scope: Backend — see .claude/CLAUDE.md for update rules._
