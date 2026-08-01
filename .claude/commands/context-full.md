---
description: Load full context, backend and clients (CLAUDE.md + all PROJECT-*.md + ARCHITECTURE-*.md)
---

Load full-stack context for this session (both backend and clients):

1. Read `.claude/CLAUDE.md` if not already read this session.
2. Read `.claude/PROJECT-BACKEND.md` and `.claude/PROJECT-CLIENTS.md`.
3. Read `.claude/ARCHITECTURE-BACKEND.md` and `.claude/ARCHITECTURE-CLIENTS.md`.

Use this instead of `/context-backend`/`/context-frontend` when the task genuinely spans both stacks (e.g. a full-stack feature, an API contract change, integration/auth wiring between a client and the backend) — see [skills/create-feature.md](../skills/create-feature.md) and [AI_CONTEXT.md § Full-Stack Safety Rules](../AI_CONTEXT.md#full-stack-safety-rules).

After loading, briefly confirm what was loaded and wait for the user's actual request — do not start analyzing or summarizing the repository unprompted.
