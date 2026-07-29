# Workflow: Analyze Solution

Triggered by requests like "analyze the backend/solution" or "analyze the solution."

## Steps

1. **Confirm this means the backend.** This repo has one backend solution (`src/`) — if the user might mean a client app instead, confirm (`clients/` → [analyze-frontend skill](../skills/analyze-frontend.md) for the whole folder, [analyze-client skill](../skills/analyze-client.md) for one named app).
2. **Analyze the whole backend solution.** Use [analyze-solution skill](../skills/analyze-solution.md) to enumerate its modules/projects and map dependencies. If the user actually means one module, redirect to [analyze-module](../skills/analyze-module.md) instead.
3. **Understand dependencies**: delegate to [dependency-analyzer](../agents/dependency-analyzer.md) for the project/package reference graph, flagging any cross-module boundary violations.
4. **Optionally assess structure**: if the user also wants a structural/architecture read, delegate to [architecture-reviewer](../agents/architecture-reviewer.md).
5. **Update documentation only if requested**:
   - Update the Backend Modules table in `.claude/PROJECT.md`.
   - Write/update `.claude/docs/generated/backend/overview.md` using [solution-overview template](../docs/templates/solution-overview.md).
   - If not asked, report findings conversationally without writing files.

## Output

- A description of the backend's modules, responsibilities, and dependency graph.
- Optionally, updated `.claude/PROJECT.md` entries and/or a generated backend overview doc.
