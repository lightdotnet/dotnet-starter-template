<!--
Template: Dependency Graph
Used by: agents/dependency-analyzer.md
Output location: .claude/docs/generated/backend/dependency-graph.md (or .claude/docs/generated/clients/<app-name>/dependency-graph.md)
Do not populate this file itself — copy its structure into the generated output.
-->

# Dependency Graph: <Backend | Client app name>

## Project-to-Project References (backend) / Internal Module Imports (client app)

| From | To | Notes |
|---|---|---|

## Package References

| Project | Package | Version | Notes |
|---|---|---|---|

## Circular References

_List any found — these are always worth flagging, never expected._

## Version Mismatches

_Same package pinned to different versions across modules (backend) or in package.json/lockfile (client app) — or across sibling client apps, if they're meant to share versions._

## Cross-Module Boundary Violations (backend only)

_Any module referencing another module's `Domain`/`Infrastructure` project directly instead of going through its `Api`/public contract — always worth flagging._

## Notes

<!-- manual: content below this line is human-authored and must be preserved verbatim during sync -->

---
_Generated: <date> — scope: <Backend | client app "<app-name>"> — see .claude/CLAUDE.md for update rules._
