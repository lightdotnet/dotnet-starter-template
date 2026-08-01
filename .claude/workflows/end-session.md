# Workflow: End Session

Run at the end of a non-trivial session (meaningful backend and/or client-app code/doc changes were made).

## Steps

1. **Summarize completed work.** A concise account of what changed on each side of the stack touched, scoped to what actually happened this session — no restating the whole conversation.
2. **Suggest documentation updates.** If code changed in ways that would make `.claude/PROJECT.md`/`.claude/PROJECT-BACKEND.md`/`.claude/PROJECT-CLIENTS.md`, `.claude/ARCHITECTURE.md`/`.claude/ARCHITECTURE-BACKEND.md`/`.claude/ARCHITECTURE-CLIENTS.md`, `.claude/DEVELOPMENT.md`, or a generated doc under `.claude/docs/generated/` stale, name specifically which doc(s) and why.
3. **Flag contract drift risk.** If both backend and one or more client apps changed, or only the backend changed but a client consumes the affected endpoint, note whether [api-contract-reviewer](../agents/api-contract-reviewer.md) should be run before shipping.
4. **List follow-up tasks.** Anything identified but not done this session (e.g. a review finding not yet fixed, a test gap noted but not closed, a breaking-change flag that needs a decision).
5. **Do not automatically modify documentation.** Suggesting is the end of this workflow's responsibility — actually syncing requires the user to explicitly invoke [sync-documentation](sync-documentation.md) in a future step/session.

## Output

- A short summary of completed work.
- A list of suggested documentation updates (not applied).
- A list of follow-up tasks/open questions, including any contract-drift risk.
