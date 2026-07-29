<!--
Template: Solution Overview (Backend)
Used by: skills/analyze-solution.md, workflows/analyze-solution.md
Output location: .claude/docs/generated/backend/overview.md
Do not populate this file itself — copy its structure into the generated output.
-->

# Backend Solution Overview

## Purpose

_What this backend is for, in one or two sentences._

## Modules

| Module | Path | Responsibility | Status |
|---|---|---|---|

## Shared/Host Projects

| Project | Path | Responsibility |
|---|---|---|
| Shared/building-blocks | `src/Shared` (verify actual path) | |
| Composition-root host | `src/Host` (verify actual path) | |

## Dependency Graph

_Module-to-module and key package dependencies, verified from actual `.csproj`/`.sln` content. Flag any cross-module boundary violation found._

## Entry Points

_The hosted API and how it's started (`Program.cs` in the host project)._

## Data Access

_DbContext(s) per module, if any. Link to [database.md](database.md) doc if generated._

## External Dependencies

_Notable third-party NuGet packages and why they're used._

## Client Integration

_Which client app(s) under `clients/` consume this backend and how — see [../frontend-overview.md](../frontend-overview.md) (clients index) and `.claude/ARCHITECTURE.md` (Integration section)._

## Notes

<!-- manual: content below this line is human-authored and must be preserved verbatim during sync -->

---
_Generated: <date> — scope: backend solution — see .claude/CLAUDE.md for update rules._
