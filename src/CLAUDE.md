# Backend — StarterKit.WebApi

Project-specific guidance for the backend solution under `src/`. See the root [CLAUDE.md](../CLAUDE.md) for repository-wide rules (language convention, code-change workflow gate, agent/skill/workflow usage) — this file only covers what's specific to the backend.

## Purpose

ASP.NET Core (C#) **Modular Monolith** — one solution (`StarterKit.slnx`), one deployable process (`src/StarterKit.WebApi`). Built on the private `Lightsoft.*` (`Light.*`) vendor framework family (mediator, `Result`/`Paged` contracts, domain base types, ASP.NET Core authorization/modularity/CORS helpers, EF Core helpers, Serilog).

## Stack

`net10.0`, EF Core (provider-configurable: `InMemory`/`PostgreSQL`/`MSSQL`/`Sqlite` via `DbProvider`), xUnit v3 for tests, central package management via `Directory.Packages.props`.

## Modules

| Module | Path | Responsibility |
|---|---|---|
| Identity | `src/Identity.Api` + `src/Identity.Contracts` | Users, roles, claims, JWT auth/token issuance, permission catalog. |
| Notifications | `src/Notifications.Api` + `src/Notifications.Contracts` | Notification storage + real-time push over SignalR (admin + self-service surfaces). |
| Organization | `src/Organization.Api` + `src/Organization.Contracts` | Companies, department/team hierarchy (`OrgUnit`), company-scoped employee levels, employees, and optional employee-to-Identity-login linking. |
| Approval | `src/Approval.Api` + `src/Approval.Contracts` | Generic, reusable multi-level approval-request engine — the calling module resolves the approver chain and drives the workflow via `IApprovalService`; not tied to any specific request type. |

Plus shared/host projects: `src/Shared` (shared kernel, leaf), `src/Infrastructure` (cross-cutting infra), `src/Persistence` (EF Core concerns), `src/StarterKit.WebApi` (composition-root host).

## Architectural Constraints ("do not" rules)

- **Every module's `<Module>.Contracts` project is the only seam other modules or the host may reference.** Never reference another module's internals (its single-project folders, or a split module's `Domain`/`Application`/`Infrastructure`/`Api`) directly.
- **One `DbContext` per module is the default**, even when modules share one physical database (current state: `Identity`/`Notifications`/`Organization`/`Approval` share one DB, separated by schema/table).
- Full module structure convention (single-project vs. Clean-Architecture split, `.Api`-suffix naming) — see [docs/architecture/architecture.md § Layering](docs/architecture/architecture.md#layering).

## Architecture

- [docs/architecture/overview.md](docs/architecture/overview.md) — solution overview, dependency graph summary, entry points.
- [docs/architecture/architecture.md](docs/architecture/architecture.md) — layering, dependency direction, key design patterns, shared kernel.
- [docs/architecture/dependency-graph.md](docs/architecture/dependency-graph.md) — package references, circular-reference/boundary-violation check.
- [docs/architecture/modules/Identity.md](docs/architecture/modules/Identity.md) / [docs/architecture/modules/Notifications.md](docs/architecture/modules/Notifications.md) / [docs/architecture/modules/Organization.md](docs/architecture/modules/Organization.md) / [docs/architecture/modules/Approval.md](docs/architecture/modules/Approval.md) — per-module deep dive.

## Conventions

- [docs/conventions/coding-conventions.md](docs/conventions/coding-conventions.md) — build/tooling, style, structural/testing conventions.
- [docs/conventions/development-guide.md](docs/conventions/development-guide.md) — local setup, common tasks, where to look for X.
- [docs/conventions/docker-cli.md](docs/conventions/docker-cli.md) — local Postgres/Redis/pgAdmin via Docker.
- [docs/conventions/migrations.md](docs/conventions/migrations.md) — EF Core migration CLI cheat sheet.

## Testing

```bash
dotnet test tests/Framework.Tests/Framework.Tests.csproj
dotnet test tests/Identity.Tests/Identity.Tests.csproj
dotnet test tests/Organization.Tests/Organization.Tests.csproj
```

`Notifications` and `Approval` have no dedicated test project yet (see Known Debt).

## Known Debt

See [docs/known-debt.md](docs/known-debt.md) — the single, current-state-only list of open backend technical debt/pending architecture decisions. Update it directly when debt is found or resolved; don't re-scatter items back into the architecture docs above.

---
_Last synced: 2026-09-05_
