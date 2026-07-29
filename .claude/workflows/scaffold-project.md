# Workflow: Scaffold Project

Triggered by requests to set up the initial backend and/or frontend structure in this starter template (e.g. "scaffold the project," "set up the initial backend," "create the Next.js app").

This repo starts empty/skeletal by design (see `.claude/CLAUDE.md` §1). This workflow is how that skeleton gets created — it's a one-time (or per-module) structural setup, not a routine task, so it goes through a plan first.

## Steps

1. **Confirm what's being scaffolded.** Backend only, one or more client apps, or both? If backend: which module(s) first? If clients: which app name(s) and how many — don't assume a single `clients/web/` if the user hasn't said so. Don't assume "everything" from a vague request.
2. **Confirm/record key decisions before generating files**, if not already settled in `.claude/PROJECT.md`/`.claude/ARCHITECTURE.md`:
   - Backend: .NET target version, module list (or just the first module + shared building-blocks + host project), EF Core provider, test framework.
   - Each client app: name/folder under `clients/` (e.g. `web`), Next.js version/router (App Router is the template default — confirm if unsure), package manager, styling approach, test framework. If multiple apps are being scaffolded, confirm whether they share tooling (e.g. a workspace/monorepo setup at the `clients/` root) or are fully independent.
3. **Delegate design validation**:
   - [dotnet-architect](../agents/dotnet-architect.md) for the backend project/module layout before creating `.csproj` files.
   - [nextjs-architect](../agents/nextjs-architect.md) for each client app's initial structure before running `create-next-app`-equivalent setup.
4. **Produce a concrete plan**: the exact folder/file layout to be created on each side in scope (e.g. `src/ModularMonolith.sln`, `src/Modules/<Name>/{Domain,Application,Infrastructure,Api}/*.csproj`, `src/Shared/`, `src/Host/`, `clients/web/app/`, `clients/web/package.json`). Use plan mode — this touches many files at once and is exactly the kind of wide-blast-radius change `.claude/CLAUDE.md` §2.7 asks to confirm first.
5. **Wait for approval** before creating files.
6. **Scaffold incrementally**: create the solution/project skeleton (or `create-next-app` equivalent, per client app) first, verify it builds/runs with no business logic, then add the first module/feature as a working example if requested — don't generate a large amount of untested skeleton in one shot without a build check in between.
7. **Verify it runs**: build the backend solution and start each scaffolded client's dev server (or at minimum confirm the build/lint passes) — this is basic build sanity, not the project's eventual automated test suite.
8. **Present the created structure back to the user for review** before considering the task done — don't chain into running a test suite (there's usually nothing to test yet at this stage) or updating docs.
9. **Update docs only if requested**: `.claude/PROJECT.md` (Backend Modules / Client Apps tables) can be updated to reflect the new skeleton, but only if the user asks — otherwise report the created structure conversationally.

## Output

- An approved scaffolding plan, followed by the actual created files.
- A working, buildable skeleton (backend solution and/or one or more client apps) with no unrequested business logic.
- A summary of what was created and any decisions made that should be recorded (offer to update `.claude/PROJECT.md`).
