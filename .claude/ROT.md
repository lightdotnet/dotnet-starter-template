# ROT.md — Scheduled Maintenance for `.claude/`

This file defines the recurring review that keeps `.claude/` itself healthy: agents, skills, workflows, templates, commands, and the core docs (`CLAUDE.md`, `AI_CONTEXT.md`, `ARCHITECTURE.md` + `ARCHITECTURE-BACKEND.md`/`ARCHITECTURE-CLIENTS.md`, `PROJECT.md` + `PROJECT-BACKEND.md`/`PROJECT-CLIENTS.md`, `DEVELOPMENT.md`, `WORKFLOWS.md`). It is a **meta-maintenance** doc — it reviews the `.claude/` configuration, not the application code (see [workflows/sync-documentation.md](workflows/sync-documentation.md) for that).

**ROT** = the three things a review looks for:

- **R — Redundant**: two agents/skills/workflows with overlapping responsibility; the same guidance repeated in multiple files instead of cross-linked; a doc template duplicating what another already covers.
- **O — Outdated**: content that no longer matches reality — a broken cross-reference (renamed/deleted file), a described module/route that no longer exists or was renamed, an architecture decision recorded here that the team actually changed, a tech-stack claim in `CLAUDE.md` §1.1 that's since drifted.
- **T — Trivial**: an agent/skill/workflow that's never invoked, adds no value over Claude reasoning inline, or has shrunk to boilerplate that restates the obvious.

Like everything else under `.claude/`, ROT review is **explicit and pull-based** — run it when the user asks, not automatically as a side effect of other work (see `CLAUDE.md` §2.5). Findings are reported; fixes to core docs/templates require the same confirm-before-edit discipline as [sync-documentation](workflows/sync-documentation.md).

## Schedule

| Cadence | Trigger | Scope |
|---|---|---|
| Monthly (or every ~4–6 weeks of active work) | Manual — user runs it | Full `.claude/` sweep |
| After `scaffold-project` completes | One-time, event-based | Reconcile `CLAUDE.md`/`ARCHITECTURE*.md`/`PROJECT*.md` intended-shape sections against what was actually scaffolded |
| After adding/removing a backend module or a client app under `clients/` | Event-based | `ARCHITECTURE-BACKEND.md`/`ARCHITECTURE-CLIENTS.md`, `PROJECT-BACKEND.md`/`PROJECT-CLIENTS.md`, `DEVELOPMENT.md`, any agent/skill/template/command that names specific modules/apps/routes |
| After a stack/tooling decision changes (e.g. switch data-fetching library, EF Core provider, router style) | Event-based | `CLAUDE.md` §1.1, `DEVELOPMENT.md`, `ARCHITECTURE-BACKEND.md`/`ARCHITECTURE-CLIENTS.md`, affected agents |
| Before a release/milestone tag | Event-based | Full sweep, with emphasis on Trivial (unused agents/skills accumulate over a project's life) |

Track actual runs in the [Review Log](#review-log) below so the next review knows the baseline.

## How to Run It

Ask Claude: *"Run a ROT review of `.claude/` per ROT.md"* (optionally scoped, e.g. "...just the agents" or "...since the last entry in the Review Log"). Expected behavior:

1. Read the [Review Log](#review-log) to find the last reviewed date/scope — a review is a diff since then, not necessarily a full re-read every time.
2. Walk the checklist below for the scope in play.
3. Report findings as a table: file → R/O/T → what's wrong → suggested action. No file edits during this step.
4. Only apply fixes the user explicitly approves — this is a report-then-confirm workflow, same as [review-repository](workflows/review-repository.md), not an autonomous cleanup.
5. Append a row to the Review Log once findings are reported (or once fixes are applied, if the user asked for that too).

## Checklist

### Core docs (`CLAUDE.md`, `AI_CONTEXT.md`, `ARCHITECTURE.md` + split files, `PROJECT.md` + split files, `DEVELOPMENT.md`, `WORKFLOWS.md`)

- [ ] Every agent/skill/workflow/command link in `CLAUDE.md` §4–6 and `WORKFLOWS.md` resolves to a file that still exists.
- [ ] `CLAUDE.md` §1.1 (Tech Stack) still matches what's actually in `src/`/`clients/*` (or is still correctly marked "intent, not yet verified" if nothing's scaffolded).
- [ ] `ARCHITECTURE-BACKEND.md`/`ARCHITECTURE-CLIENTS.md`/`PROJECT-BACKEND.md`/`PROJECT-CLIENTS.md`/`DEVELOPMENT.md` "verified" sections aren't silently stale — either they match current code, or they're honestly still `unknown`.
- [ ] No core doc contradicts another (e.g. `ARCHITECTURE-BACKEND.md` describing a module `DEVELOPMENT.md` doesn't mention, or vice versa) — including each root `PROJECT.md`/`ARCHITECTURE.md` not contradicting its own `-BACKEND`/`-CLIENTS` split file.
- [ ] `commands/context-backend.md`/`context-frontend.md`/`context-full.md` still list the correct files to load (they'll go stale if a core doc is ever renamed/re-split again).

### Agents (`agents/*.md`)

- [ ] Each agent is referenced by at least one skill or workflow, or by `CLAUDE.md` §4 — an orphaned agent is a Trivial candidate.
- [ ] No two agents claim overlapping primary responsibility without a clear "defer to X for Y" boundary (check each agent's "Things to Avoid" section for the handoff).
- [ ] `description` frontmatter still accurately scopes the agent (e.g. still says "backend" if that's still true, references the right sibling agents by current filename).
- [ ] Agent still reflects current architecture decisions (e.g. if the project moved away from one `DbContext` per module, `efcore-specialist.md` needs updating — Outdated).

### Skills (`skills/*.md`)

- [ ] Each skill is referenced from `CLAUDE.md` §5, a workflow, or another skill — otherwise flag as possibly Trivial.
- [ ] Skill's `Workflow` steps still name agents/skills that exist under their current filenames.
- [ ] No two skills cover the same task with diverging advice (Redundant) — if they do, decide which is canonical and fold/cross-link the other.

### Workflows (`workflows/*.md`)

- [ ] `WORKFLOWS.md` index table matches the actual files in `workflows/` (no missing rows, no rows for deleted files).
- [ ] Each workflow's steps still reference current agent/skill filenames.
- [ ] A workflow that's never matched a real request in practice (ask the user) is a Trivial candidate for trimming or merging.

### Doc templates (`docs/templates/*.md`)

- [ ] Every template is still used by at least one skill/agent/workflow reference (`docs/generated/README.md` layout should match the template set).
- [ ] Template `Output location` comments still match the layout described in `docs/generated/README.md`.
- [ ] No template duplicates another's structure (Redundant) — e.g. two templates both trying to own "module-level architecture."

## Review Log

| Date | Scope | Reviewer | R/O/T found | Actions taken |
|---|---|---|---|---|
| 2026-07-29 | Full `.claude/` (initial creation of this file, following the C#+Next.js re-sync) | Claude + user | None yet — baseline entry | N/A |
| 2026-08-01 | Full `.claude/` sweep | Claude + user | 9× Outdated: `AI_CONTEXT.md`, `DEVELOPMENT.md`, `agents/dotnet-architect.md`, `agents/architecture-reviewer.md`, `agents/api-designer.md`, `agents/api-contract-reviewer.md`, `skills/analyze-module.md`, `workflows/new-session.md`, `workflows/scaffold-project.md` all still described the pre-2026-07-30 nested `src/Modules/<Name>/{Domain,Application,Infrastructure,Api}` convention instead of the adopted flat-project + `.Contracts`-seam convention. No Redundant/Trivial found — all agents/skills/workflows still referenced, boundaries clear, `WORKFLOWS.md` matches actual files, templates all used. | All 9 Outdated findings fixed this session. Noted out-of-ROT-scope staleness in `docs/generated/backend/*.md` (predates the Identity module entirely) — left for a future `sync-documentation` run. |

---
_This file is itself subject to ROT review — if the schedule or checklist stops matching how the project actually works, update it during a review rather than letting it drift._
