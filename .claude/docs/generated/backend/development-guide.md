# Development Guide: Backend

## Prerequisites

- .NET SDK `10.0.301` (or compatible `net10.0`-targeting SDK).

## Building

```
dotnet build StarterKit.slnx
```

## Running Locally

Not available yet — no composition-root host project/`Program.cs` exists under `src/`.

## Running Tests

```
dotnet test tests/Framework.Tests/Framework.Tests.csproj
```

No special setup needed — all current tests are unit tests using EF Core's InMemory/Sqlite providers directly (no external database/services required).

## Local Setup

No connection strings/secrets required yet — no module `DbContext`/host project exists.

## Common Tasks

| Task | How |
|---|---|
| Add a backend migration | Not applicable yet — no module `DbContext` exists. |
| Run the API locally | Not applicable yet — no host project exists. |
| Run the backend test suite | `dotnet test tests/Framework.Tests/Framework.Tests.csproj` |
| Build the whole solution | `dotnet build StarterKit.slnx` |

## Where to Look for X

- Shared abstractions/base types (`ICurrentUser`, `IDateTime`, `Status`, `BaseDto`, entity wrappers, authorization): `src/Shared/`.
- EF Core provider config, audit/soft-delete tracking, domain-event dispatch, CORS, health checks, module base classes: `src/Infrastructure/`.
- Tests: `tests/Framework.Tests/<ProjectName>/...`, mirroring the corresponding `src/<ProjectName>` structure.

## Notes

<!-- manual: content below this line is human-authored and must be preserved verbatim during sync -->

---
_Generated: 2026-07-29 — scope: Backend — see .claude/CLAUDE.md for update rules._
