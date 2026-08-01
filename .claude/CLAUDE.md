# CLAUDE.md — Primary Source of Truth

This file is the entry point for every Claude Code session in this repository. Read it first, every session, before touching anything else.

## 0. Language Convention

- The user gives instructions in Vietnamese. Understand and respond to Vietnamese requests normally — do not ask the user to switch to English.
- **Everything written into the repository — documentation (`.claude/**`, generated docs, README-style files) and code comments — must be written in English**, regardless of what language the request came in. This keeps the codebase and its docs consistent for any future contributor, and matches the language already used across every file in `.claude/`.
- This applies to new content and edits alike: don't leave a Vietnamese comment or doc section next to English ones.

## 1. Repository Purpose

This repository is a **starter template monorepo for a full-stack application**: a C#/.NET backend and one or more frontend clients, meant to be cloned/forked as the starting point for new projects.

- **Backend** — `src/` — ASP.NET Core Web API, **C#**, organized as a **Modular Monolith**. One solution (`.sln`), one deployable process. Business modules live as flat projects directly under `src/` (no `src/Modules/` nesting) — a simple module is a single `<Module>` project internally organized by folder (Entities/Application/Data/Controllers/etc.); a module complex enough to justify it is split Clean-Architecture-style into `<Module>.Domain` / `<Module>.Application` / `<Module>.Infrastructure` / `<Module>.Api` projects. Every module also has a `<Module>.Contracts` project — the only project other modules or the host may reference — exposing its public DTOs and service interfaces. See [ARCHITECTURE-BACKEND.md § Module Structure Convention](ARCHITECTURE-BACKEND.md#backend--module-structure-convention) for the decision criteria and naming rules.
- **Clients** — `clients/` — one or more frontend apps, each in its own subfolder (e.g. `clients/web/` for the primary **Next.js** (TypeScript/React) app; additional apps such as `clients/admin/` or a future mobile client may be added later). Each client consumes the backend exclusively over HTTP as a JSON API. The backend's MVC controllers are **API-only** (no server-rendered Razor views) — all UI rendering happens in the client apps.
- **Integration** — the only contract between backend and any client is the HTTP API surface (routes, DTOs, status/error shapes). No side reaches into another's internals; there is no shared DB access or shared source between `src/` and `clients/*`.

Consequences of this:

- There is one backend solution but potentially **multiple client apps** — don't assume `clients/` has only one subfolder, mirroring the backend's module plurality. A request about "the frontend" without naming an app is ambiguous once more than one exists — ask.
- Module boundaries are the primary structural concern on the backend — each module's `<Module>.Contracts` project is the only allowed seam. Treat cross-module reach-through (Module A's internals — or, for a split module, its `Domain`/`Application`/`Infrastructure` project — referenced directly by Module B or by another module's `Contracts`) as an architecture violation, not a shortcut.
- Changes to the API contract ripple into every client that consumes that endpoint (and vice versa for any typed client a frontend generates/consumes) — treat the HTTP contract as the seam to protect.
- This repo is a **template**: expect it to start mostly empty/skeletal and be filled in incrementally. Do not assume scaffolding exists until verified — check before describing structure.

### 1.1 Tech Stack

| Layer | Stack |
|---|---|
| Backend runtime | ASP.NET Core (C#), API-only MVC controllers |
| Backend architecture | Modular Monolith — flat projects directly under `src/` (no `src/Modules/` nesting); each module is either a single `<Module>` project or split Clean-Architecture-style into `<Module>.{Domain,Application,Infrastructure,Api}` depending on complexity, plus always a `<Module>.Contracts` seam project |
| Backend data access | EF Core (one `DbContext` per module is the intended default — verify per module, don't assume a single shared context) |
| Client apps | `clients/<app-name>/` — one per frontend; the primary app (`clients/web/`) is Next.js (App Router), TypeScript, React |
| Integration | REST/JSON over HTTP; each client keeps a typed API client generated from or hand-kept in sync with backend contracts |

Treat this table as intent for a template, not yet-verified fact — cross-check against `.claude/PROJECT-BACKEND.md`/`.claude/PROJECT-CLIENTS.md`/`.claude/ARCHITECTURE-BACKEND.md`/`.claude/ARCHITECTURE-CLIENTS.md` (or the root `.claude/PROJECT.md`/`.claude/ARCHITECTURE.md` for the cross-cutting summary) and actual code before asserting specifics to the user, including how many client apps actually exist under `clients/`.

## 2. AI Operating Rules

1. **Read only what the current task needs.** Do not open files, folders, or modules unrelated to the current request. Prefer `Glob`/`Grep` targeted lookups over broad tree walks.
2. **Prefer specialized agents over doing everything inline.** See [Agent Usage](#4-agent-usage) — backend architecture, EF Core, API design, frontend architecture, security, performance, testing, docs, and dependency questions each have a dedicated agent. Delegate to them rather than reasoning about all domains yourself in the main context.
3. **Minimize token usage.** Summarize instead of pasting large file contents back to the user. Avoid re-reading files already read this session. Avoid speculative exploration "just in case."
4. **Repository analysis is incremental, never automatic.** Do not proactively scan or document the whole repo, the whole backend, or all of `clients/` unless the user explicitly asks. A request about one module or one client app is a request about that scope only.
5. **Documentation synchronization only happens on request.** Never regenerate or rewrite files under `.claude/docs/generated/` (or any `AI_CONTEXT.md`/`PROJECT*.md`/`ARCHITECTURE*.md` content) unless the user explicitly asks to generate or sync docs.
6. **Ask before assuming structure.** If it's unclear which module or which client app (once `clients/` has more than one) a request applies to, ask rather than guessing.
7. **No destructive or wide-blast-radius edits without confirmation.** Changes to the shared kernel/building blocks, cross-module refactors, changes to an API contract that one or more clients depend on, or dependency bumps touching both `src/` and `clients/*` require explicit user confirmation first.
8. **Language**: see [§0 Language Convention](#0-language-convention) — Vietnamese input is normal; everything written to the repo is English.
9. **Code-change workflow gate.** For any request to add code, modify existing code, or add a feature: always produce a plan first and present it for the user's review — do not write any code until the user explicitly approves the plan. Once approved, implement it, delegating to the relevant agents/skills/workflows (§4–§6) wherever they apply. When implementation is complete, present the changed code back to the user for review before doing anything further — don't chain straight into testing or docs. **Running the automated test suite and updating documentation each require a separate, explicit follow-up instruction** from the user; never trigger either automatically right after implementing, even if the approved plan mentioned adding tests or docs. See [implement-feature](workflows/implement-feature.md) and [create-feature](skills/create-feature.md).

## 3. Documentation Rules

- `.claude/PROJECT.md`/`.claude/ARCHITECTURE.md` hold only the cross-cutting summary. Backend-scoped detail lives in `.claude/PROJECT-BACKEND.md`/`.claude/ARCHITECTURE-BACKEND.md`; client-scoped detail lives in `.claude/PROJECT-CLIENTS.md`/`.claude/ARCHITECTURE-CLIENTS.md`. All of these are **living templates** — they start mostly empty and are filled in incrementally as modules/projects/client apps are analyzed, and only on request. Use the `/context-backend`, `/context-frontend`, or `/context-full` commands ([commands/](commands/)) to load the right scope in one step instead of reading each file individually.
- `.claude/docs/templates/` holds **unpopulated templates** for generated docs (backend/module overview, per-client-app overview, API docs, DB docs, domain model, conventions, dependency graph, dev guide). Never delete or repurpose these.
- `.claude/docs/generated/` holds **actual generated documentation**, split into `backend/` and `clients/<app-name>/` (e.g. `docs/generated/backend/modules/<ModuleName>/overview.md`, `docs/generated/clients/web/overview.md`). Only written to when a sync/generate workflow is explicitly invoked.
- Manually-authored documentation (anything a human wrote and didn't come from a generate/sync workflow) must be preserved during sync — never silently overwritten. See [sync-documentation workflow](workflows/sync-documentation.md).
- Outdated generated content should be removed during an explicit sync, not left to drift silently.
- The `.claude/` configuration itself (agents/skills/workflows/templates/core docs) is reviewed on its own recurring schedule for redundant/outdated/trivial content — see [ROT.md](ROT.md). This is separate from syncing generated docs against application code.

## 4. Agent Usage

Specialized agents live in [agents/](agents/). Prefer delegating to them over reasoning inline:

| Agent | Use for |
|---|---|
| [architecture-reviewer](agents/architecture-reviewer.md) | Backend layering, module boundaries, dependency direction within/between modules under `src/` |
| [dotnet-architect](agents/dotnet-architect.md) | Backend project/module structure, framework choices, new module shape |
| [efcore-specialist](agents/efcore-specialist.md) | EF Core models, migrations, query performance, DbContext design per module |
| [api-designer](agents/api-designer.md) | Backend REST API contract design, versioning, DTO shape |
| [code-reviewer](agents/code-reviewer.md) | General C#/.NET backend code quality review |
| [nextjs-architect](agents/nextjs-architect.md) | Next.js/React/TypeScript structure for a given client app — routing, data fetching, state management, component architecture |
| [frontend-code-reviewer](agents/frontend-code-reviewer.md) | React/TypeScript code quality review for a given client app |
| [api-contract-reviewer](agents/api-contract-reviewer.md) | Consistency between backend API contracts and how a given client consumes them; drift detection |
| [security-reviewer](agents/security-reviewer.md) | Vulnerabilities, secrets, auth/authz gaps — backend and any client app |
| [performance-reviewer](agents/performance-reviewer.md) | Hot paths, allocations, async misuse, backend/client performance |
| [testing-reviewer](agents/testing-reviewer.md) | Test coverage/quality — backend (xUnit-style) and client apps (component/unit) |
| [documentation-writer](agents/documentation-writer.md) | Generating/updating docs from code |
| [dependency-analyzer](agents/dependency-analyzer.md) | Backend project/package references and each client's npm dependencies, coupling |

Rule of thumb: if a task maps cleanly to one row above, delegate to that agent instead of doing the analysis in the main thread.

## 5. Skill Usage

Reusable playbooks live in [skills/](skills/) — see each file for purpose, inputs, workflow, and best practices:

- [create-feature](skills/create-feature.md) — full-stack feature (backend module change + client UI + contract), [refactor](skills/refactor.md)
- [review-code](skills/review-code.md), [review-architecture](skills/review-architecture.md)
- [generate-docs](skills/generate-docs.md), [sync-docs](skills/sync-docs.md)
- [analyze-solution](skills/analyze-solution.md) (backend), [analyze-module](skills/analyze-module.md), [analyze-project](skills/analyze-project.md)
- [analyze-frontend](skills/analyze-frontend.md) (all of `clients/`), [analyze-client](skills/analyze-client.md) (one client app)
- [efcore](skills/efcore.md), [api](skills/api.md), [nextjs](skills/nextjs.md), [testing](skills/testing.md), [performance](skills/performance.md)

## 6. Workflow Usage

Session- and task-level workflows live in [workflows/](workflows/) — see [WORKFLOWS.md](WORKFLOWS.md) for the index:

- [new-session](workflows/new-session.md) — start of every session
- [scaffold-project](workflows/scaffold-project.md) — initial backend/client scaffolding for this template
- [analyze-folder](workflows/analyze-folder.md), [analyze-solution](workflows/analyze-solution.md)
- [implement-feature](workflows/implement-feature.md)
- [review-repository](workflows/review-repository.md)
- [sync-documentation](workflows/sync-documentation.md)
- [end-session](workflows/end-session.md) — end of every non-trivial session

## 7. Context Management

- Load context in this order: `CLAUDE.md` → the specific doc/template relevant to the task → the specific code files needed. Never load all of `docs/generated/` at once.
- Prefer `Grep`/`Glob` to locate the relevant module/project/client app before reading files.
- When a task is scoped to one module or one client app, do not read sibling modules/apps "for context" unless a real dependency exists (verified via project references or actual API calls, not assumption).
- Summarize large findings; don't paste entire files into the conversation when a targeted excerpt will do.
- See [AI_CONTEXT.md](AI_CONTEXT.md) for detailed working rules.

## 8. Documentation Synchronization Rules

- Sync is **explicit and pull-based**: it happens only when the user runs a sync/generate request (see [sync-documentation](workflows/sync-documentation.md), [sync-docs skill](skills/sync-docs.md)).
- Sync must diff current generated docs against current code, update what changed, remove what's stale, and leave manually-authored sections untouched.
- Never sync as a side effect of an unrelated task (e.g., don't "helpfully" update docs while implementing a feature unless asked).

## 9. Quick Reference for Common Commands

| User says | Do this |
|---|---|
| "Scaffold the project" / "set up the initial structure" | [workflows/scaffold-project.md](workflows/scaffold-project.md) |
| "Analyze the backend/solution" | [workflows/analyze-solution.md](workflows/analyze-solution.md) |
| "Analyze this module" | [skills/analyze-module.md](skills/analyze-module.md) |
| "Analyze this project" | [skills/analyze-project.md](skills/analyze-project.md) |
| "Analyze all the clients/frontends" | [skills/analyze-frontend.md](skills/analyze-frontend.md) |
| "Analyze the web client / this client app" | [skills/analyze-client.md](skills/analyze-client.md) |
| "Analyze this folder" | [workflows/analyze-folder.md](workflows/analyze-folder.md) |
| "Generate documentation" | [skills/generate-docs.md](skills/generate-docs.md) |
| "Sync documentation" | [workflows/sync-documentation.md](workflows/sync-documentation.md) |
| "Review architecture" | [skills/review-architecture.md](skills/review-architecture.md) |
| "Review code" | [skills/review-code.md](skills/review-code.md) |
| "Implement a feature" | [workflows/implement-feature.md](workflows/implement-feature.md) |
| "Update CLAUDE documentation" | [skills/sync-docs.md](skills/sync-docs.md), scoped to `.claude/` docs only |
| "Run a ROT review of .claude" / "check for stale agents/skills" | [ROT.md](ROT.md) |
| "Load backend/frontend/full context" | `/context-backend`, `/context-frontend`, or `/context-full` ([commands/](commands/)) |
