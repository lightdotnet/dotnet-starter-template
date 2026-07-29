<!--
Template: Module Overview (Backend, Modular Monolith)
Used by: skills/analyze-module.md, agents/architecture-reviewer.md
Output location: .claude/docs/generated/backend/modules/<ModuleName>/overview.md
Do not populate this file itself — copy its structure into the generated output.
-->

# Module Overview: <ModuleName>

## Purpose

_What business capability this module owns, in business terms._

## Internal Layering

| Project | Responsibility | Notes |
|---|---|---|
| `<ModuleName>.Domain` | | |
| `<ModuleName>.Application` | | |
| `<ModuleName>.Infrastructure` | | |
| `<ModuleName>.Api` | | |

## Public Contract

_Routes/DTOs exposed by `<ModuleName>.Api` — this is what other modules and any client app may rely on. Link to [../../api.md](../../api.md) section if generated._

## Data Access

_This module's `DbContext`, key entities, and whether it shares the physical database with other modules (expected) vs. truly isolated storage._

## Dependencies

| Depends on | Type | Why |
|---|---|---|
| Shared/building-blocks | project | |

## Depended On By

_Other modules or client app(s) that consume this module's public contract. A dependency on this module's `Domain`/`Infrastructure` directly (not through `Api`) is a boundary violation — flag it._

## Notable Conventions

_Anything distinct about this module's conventions vs. other modules._

## Notes

<!-- manual: content below this line is human-authored and must be preserved verbatim during sync -->

---
_Generated: <date> — scope: module "<ModuleName>" — see .claude/CLAUDE.md for update rules._
