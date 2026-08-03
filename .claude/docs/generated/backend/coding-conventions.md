# Coding Conventions: Backend

## Build & Tooling

- Target framework: `net10.0` across all backend projects (`src/Shared`, `src/Infrastructure`, `src/Persistence`, `src/Identity.Contracts`, `src/Identity.Api`, `src/StarterKit.WebApi`, `tests/Framework.Tests`, `tests/Identity.Tests`).
- Central package management via `Directory.Packages.props` (`ManagePackageVersionsCentrally=true`, `CentralPackageTransitivePinningEnabled=false`), except `tests/Framework.Tests/Framework.Tests.csproj` and `tests/Identity.Tests/Identity.Tests.csproj`, which both opt out (`ManagePackageVersionsCentrally=false`) and pin their own test package versions (3 for `Framework.Tests`; 4 for `Identity.Tests`, which adds `Moq`).
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
- Every module — single or split — gets a `<Module>.Contracts` project as its only externally-referenceable seam. `Identity.Contracts` was a confirmed true leaf through the previous sync; it now has one `ProjectReference` (`Shared`) — see the transitive-dependency deviation below — so "seam project" no longer implies "leaf project" as a hard rule.

## Testing Conventions

- Test framework: xUnit v3 (`xunit.v3`, `xunit.runner.visualstudio`, `Microsoft.NET.Test.Sdk`).
- Naming: `<TypeUnderTest>Tests` classes; `MethodOrMember_ShouldExpectedBehavior_WhenCondition` test methods; `// Arrange`/`// Act`/`// Assert` comments.
- No mocking library in `Framework.Tests` — hand-written fakes/test doubles instead (e.g. `RecordingPublisher : IPublisher`, `TestCurrentUser : CurrentUserBase`). `Identity.Tests` is the first backend test project to add `Moq` (used for `UserManager<User>` and service-interface doubles), alongside a real Sqlite-in-memory + ASP.NET Identity stack (`TestSupport/IdentityTestHost`) for the majority of its service/Jwt tests rather than mocking EF Core itself.
- Test project layout: two test projects now exist under `tests/`. `Framework.Tests/<ProjectName>/...` mirrors `src/<ProjectName>` 1:1 (`Shared/`, `Infrastructure/`, `Persistence/` folders). `Identity.Tests/<Area>/...` mirrors `Identity.Api`'s own folder structure (`Extensions/`, `Jwt/`, `Entities/`, `Services/`, `Controllers/`), plus a `TestSupport/` folder for shared test infrastructure (`IdentityTestHost`, `FakeCurrentUser`, `FakeDateTime`, `TestJwtOptions`). `InternalsVisibleTo` is set on `Shared.csproj`, `Infrastructure.csproj`, `Persistence.csproj` for `Framework.Tests`, and on `Identity.Api.csproj` for `Identity.Tests`.
- **Coverage gap (closed)**: the Identity module now has automated test coverage via `tests/Identity.Tests` (98 tests, `dotnet test tests/Identity.Tests/Identity.Tests.csproj`). `Framework.Tests` itself still doesn't reference `Identity.Api`/`Identity.Contracts` — that coverage lives in the separate `Identity.Tests` project instead.

## Deviations From Norms Elsewhere in the Repo

- `Identity.Api`'s internal layering mixes a CQRS command (`Application/Users/Commands`) with traditional service classes (`Services/UserService`, `Services/RoleService`) for what should be one consistent approach — a deviation from a to-be-established norm, not yet reconciled (finding D1 in `reviews/2026-07-30-backend-project-analysis.md`).
- `UserProfileController`'s route is explicitly overridden to `api/v{version:apiVersion}/user_profile` rather than relying on the default `[controller]`-token convention the other Identity controllers use — confirmed deliberate (for readability), not an inconsistency to fix.
- `UserController` now exposes both an unbounded `GET user` and a paginated `GET user/search` for the same read use case — organic drift, not a resolved decision (finding D4).
- `Identity.Api` uses `Light.EntityFrameworkCore.Extensions.WhereIf` (`UserService.SearchAsync`) without declaring the backing `Lightsoft.EntityFrameworkCore` package itself — it rides in transitively via the `ProjectReference` to `Persistence`. See `.claude/docs/generated/backend/dependency-graph.md` for detail; avoid repeating this pattern in future modules — declare packages a project actually uses directly.
- Same pattern repeated: `Identity.Contracts`'s new `IdentityPermissionProvider` uses `Light.AspNetCore.Authorization` types without `Identity.Contracts.csproj` declaring `Lightsoft.AspNetCore.Authorization` as a direct `PackageReference` — it rides in transitively via a new `ProjectReference` to `Shared`, which is also why `Identity.Contracts` is no longer a leaf project. See `dependency-graph.md`.

## Notes

<!-- manual: content below this line is human-authored and must be preserved verbatim during sync -->

---
_Generated: 2026-08-03 (resynced — `Identity.Contracts` no longer a confirmed leaf; new undeclared-transitive-dependency instance via `IdentityPermissionProvider`) — scope: Backend — see .claude/CLAUDE.md for update rules._
