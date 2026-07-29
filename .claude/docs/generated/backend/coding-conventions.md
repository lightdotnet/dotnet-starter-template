# Coding Conventions: Backend

## Build & Tooling

- Target framework: `net10.0` across `src/Shared`, `src/Infrastructure`, `tests/Framework.Tests`.
- Central package management via `Directory.Packages.props` (`ManagePackageVersionsCentrally=true`), except `tests/Framework.Tests/Framework.Tests.csproj` which opts out and pins its own test package versions.
- `Directory.Build.props` sets `ImplicitUsings=enable` and `Nullable=enable` repo-wide.
- Module structure convention (`src/Modules/<ModuleName>/{Domain,Application,Infrastructure,Api}`): unverified — no modules exist yet.

## Style

- No root `.editorconfig` found as of this scope.
- Nullable reference types enabled everywhere.
- File-scoped namespaces (`namespace X;`) used consistently — no block-scoped `namespace X { }`.
- PascalCase for all constants and enum members (no `SCREAMING_SNAKE_CASE`).
- Folder names mirror the trailing namespace segment (e.g. `Authorization/` ⇒ `StarterKit.Authorization`, `Database/` ⇒ `StarterKit.Database`).

## Structural Conventions

- DI registration via small `static class DependencyInjection` (or similarly named) extension classes exposing `Add<Feature>`/`Use<Feature>` methods, one per feature area/folder.
- Error handling/result pattern: vendor `Light.Contracts.Result`/`Result<T>`/`PagedResult<T>` for expected-failure outcomes; `Light.Exceptions.ValidationException` (thrown by `ValidationBehaviour<,>`) for validation failures.
- Logging: `AppLogging` static Serilog logger for bootstrap/startup messages (`Information`/`Warning`); standard `ILogger<T>` DI for request/runtime logging elsewhere (e.g. `MigrationsExtensions`).
- Module layering conventions: unverified — no modules exist yet.

## Testing Conventions

- Test framework: xUnit v3 (`xunit.v3`, `xunit.runner.visualstudio`, `Microsoft.NET.Test.Sdk`).
- Naming: `<TypeUnderTest>Tests` classes; `MethodOrMember_ShouldExpectedBehavior_WhenCondition` test methods; `// Arrange`/`// Act`/`// Assert` comments.
- No mocking library — hand-written fakes/test doubles instead (e.g. `RecordingPublisher : IPublisher`, `TestCurrentUser : CurrentUserBase`).
- Test project layout: `tests/Framework.Tests/<ProjectName>/...` mirrors `src/<ProjectName>` 1:1; EF Core-backed tests get a per-target-project `TestSupport/` folder with minimal InMemory fixtures. `InternalsVisibleTo` is set on `Shared.csproj`/`Infrastructure.csproj` for `Framework.Tests`.

## Deviations From Norms Elsewhere in the Repo

None observed — this is the first backend scope documented.

## Notes

<!-- manual: content below this line is human-authored and must be preserved verbatim during sync -->

---
_Generated: 2026-07-29 — scope: Backend — see .claude/CLAUDE.md for update rules._
