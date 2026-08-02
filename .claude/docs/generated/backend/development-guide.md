# Development Guide: Backend

## Prerequisites

- .NET SDK `10.0.301` (or compatible `net10.0`-targeting SDK).
- A reachable database matching the configured `DbProvider` (see Local Setup below) if running the host — no external service is required just to run the test suite.

## Building

```
dotnet build StarterKit.slnx
```

## Running Locally

```
dotnet run --project src/StarterKit.WebApi/StarterKit.WebApi.csproj
```

`src/StarterKit.WebApi/Program.cs` boots the host: configures Serilog, calls `ConfigureServices`, wires MVC/JSON options, then `ConfigurePipelines()` + `MapEndpoints(...)`. By default (`appsettings.json`) `DbProvider` is `MSSQL`, pointing `ConnectionStrings:DefaultConnection` at a local `(localdb)\mssqllocaldb` instance — switch to `InMemory`/`Sqlite`/`PostgreSQL` via `IConfiguration["DbProvider"]` (and matching `ConnectionStrings:DefaultConnection`, commented-out examples for PostgreSQL/Sqlite are already present in `appsettings.json`) if you don't have SQL Server LocalDB available. `AllowAnonymous: true` is set by default in `appsettings.json`.

## Running Tests

```
dotnet test tests/Framework.Tests/Framework.Tests.csproj
dotnet test tests/Identity.Tests/Identity.Tests.csproj
```

No special setup needed — all current tests are unit tests using EF Core's InMemory/Sqlite providers directly (no external database/services required). `Framework.Tests` covers `Shared`, `Infrastructure`, and `Persistence`; `Identity.Tests` covers the `Identity` module (`Identity.Api`) — 98 tests spanning extensions, JWT orchestration, entities, services, and controllers.

## Local Setup

- Default `DbProvider` is `MSSQL` (`src/StarterKit.WebApi/appsettings.json`) with a `(localdb)\mssqllocaldb` connection string checked into `appsettings.json` as a starter-template default — replace for real use, do not treat as a production secret.
- JWT signing (`Jwt:SecretKey`, `Jwt:Issuer`, token lifetimes) and Basic Auth (`BasicAuth: "super:123"`) values in `appsettings.json` are template placeholders, not production secrets — replace before any real deployment.
- `UserSecretsId` is set on `StarterKit.WebApi.csproj` for local `dotnet user-secrets` overrides if preferred over editing `appsettings.Development.json` directly.

## Common Tasks

| Task | How |
|---|---|
| Add a backend migration | `dotnet ef migrations add <Name> --project src/Migrations/<Provider>/<Provider>.csproj --context IdentityDbContext --output-dir Identity` where `<Provider>` is `MSSQL`, `PostgreSQL`, or `Sqlite` — run once per provider (design-time EF migration projects live under top-level `src/Migrations/{MSSQL,PostgreSQL,Sqlite}`). Confirmed working this session for a real change (`AddUserCreatedIndex` on Sqlite/PostgreSQL; a full-baseline `CreateIdentitySchema` reset on MSSQL after its old model snapshot went stale — see `docs/migrations.md` for the general pattern and `reviews/2026-07-30-backend-project-analysis.md` for that incident). |
| Run the API locally | `dotnet run --project src/StarterKit.WebApi/StarterKit.WebApi.csproj` |
| Run the backend test suite | `dotnet test tests/Framework.Tests/Framework.Tests.csproj` and `dotnet test tests/Identity.Tests/Identity.Tests.csproj` |
| Build the whole solution | `dotnet build StarterKit.slnx` |

## Where to Look for X

- Shared abstractions/base types (`ICurrentUser`, `IDateTime`, `Status`, `BaseDto`, entity wrappers, authorization): `src/Shared/`.
- Cross-cutting infrastructure (CORS, health checks, Serilog bootstrap logging, Mapster config, module/endpoint base classes): `src/Infrastructure/`.
- EF Core provider config, DbContext base class, audit/soft-delete tracking, domain-event dispatch: `src/Persistence/`.
- Identity module (users, roles, claims, JWT auth): `src/Identity.Api/` (entities in `Entities/`, DbContext in `Data/`, services in `Services/` + `Jwt/`, controllers in `Controllers/`); public DTOs/service interfaces in `src/Identity.Contracts/`.
- Host wiring/startup (`Program.cs`, `appsettings*.json`): `src/StarterKit.WebApi/`.
- Tests: `tests/Framework.Tests/<ProjectName>/...` (mirrors `Shared/`, `Infrastructure/`, `Persistence/`) and `tests/Identity.Tests/<Area>/...` (mirrors `Identity.Api`'s own `Extensions/`, `Jwt/`, `Entities/`, `Services/`, `Controllers/` folders, plus a `TestSupport/` folder for shared test infrastructure).

## Notes

<!-- manual: content below this line is human-authored and must be preserved verbatim during sync -->

---
_Generated: 2026-08-02 (resynced — added `tests/Identity.Tests` run command and layout) — scope: Backend — see .claude/CLAUDE.md for update rules._
