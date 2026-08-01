# Coding Conventions: Backend

## Build & Tooling

- Target framework: `net10.0` across all backend projects (`src/Shared`, `src/Infrastructure`, `src/Persistence`, `src/Identity.Contracts`, `src/Identity.Api`, `src/StarterKit.WebApi`, `tests/Framework.Tests`).
- Central package management via `Directory.Packages.props` (`ManagePackageVersionsCentrally=true`, `CentralPackageTransitivePinningEnabled=false`), except `tests/Framework.Tests/Framework.Tests.csproj` which opts out (`ManagePackageVersionsCentrally=false`) and pins its own 3 test package versions.
- `Directory.Build.props` sets `ImplicitUsings=enable` and `Nullable=enable` repo-wide.
- Module structure convention: flat projects directly under `src/` (no `src/Modules/` nesting), plus a `<Module>.Contracts` seam project per module — see `.claude/ARCHITECTURE-BACKEND.md § Module Structure Convention`. `Identity` is the first module built (single project, `Identity.Api` + `Identity.Contracts`); its internal layering is informal and not yet fully conformant (CQRS command + traditional service classes coexist).
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
- Module layering conventions: only one module (`Identity`) exists so far, and it doesn't yet fully conform to the flat-project + `Contracts`-seam convention adopted 2026-07-30 — see `.claude/ARCHITECTURE-BACKEND.md`. Not yet verified as a repo-wide convention across multiple modules.
- Every module — single or split — gets a `<Module>.Contracts` project as its only externally-referenceable seam; confirmed as a true leaf (no `ProjectReference`s) for `Identity.Contracts`.

## Testing Conventions

- Test framework: xUnit v3 (`xunit.v3`, `xunit.runner.visualstudio`, `Microsoft.NET.Test.Sdk`).
- Naming: `<TypeUnderTest>Tests` classes; `MethodOrMember_ShouldExpectedBehavior_WhenCondition` test methods; `// Arrange`/`// Act`/`// Assert` comments.
- No mocking library — hand-written fakes/test doubles instead (e.g. `RecordingPublisher : IPublisher`, `TestCurrentUser : CurrentUserBase`).
- Test project layout: `tests/Framework.Tests/<ProjectName>/...` mirrors `src/<ProjectName>` 1:1 — currently has `Shared/`, `Infrastructure/`, `Persistence/` folders only. `InternalsVisibleTo` is set on `Shared.csproj`, `Infrastructure.csproj`, and `Persistence.csproj` for `Framework.Tests`.
- **Coverage gap (re-confirmed)**: `Framework.Tests` still has no `Identity`/`Identity.Api`/`Identity.Contracts` folder or `ProjectReference` — the Identity module has no automated test coverage.

## Deviations From Norms Elsewhere in the Repo

- `Identity.Api`'s internal layering mixes a CQRS command (`Application/Users/Commands`) with traditional service classes (`Services/UserService`, `Services/RoleService`) for what should be one consistent approach — a deviation from a to-be-established norm, not yet reconciled (finding D1 in `reviews/2026-07-30-backend-project-analysis.md`).
- `UserProfileController`'s route is explicitly overridden to `api/v{version:apiVersion}/user_profile` rather than relying on the default `[controller]`-token convention the other Identity controllers use — confirmed deliberate (for readability), not an inconsistency to fix.

## Notes

<!-- manual: content below this line is human-authored and must be preserved verbatim during sync -->

---
_Generated: 2026-08-01 — scope: Backend — see .claude/CLAUDE.md for update rules._
