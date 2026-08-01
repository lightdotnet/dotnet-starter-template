---
description: Load backend-only context (CLAUDE.md + PROJECT-BACKEND.md + ARCHITECTURE-BACKEND.md)
---

Load backend-scoped context for this session:

1. Read `.claude/CLAUDE.md` if not already read this session.
2. Read `.claude/PROJECT-BACKEND.md`.
3. Read `.claude/ARCHITECTURE-BACKEND.md`.

Do not read `PROJECT-CLIENTS.md`, `ARCHITECTURE-CLIENTS.md`, or anything under `clients/` as part of this command — this is a backend-only context load. If the user's next request turns out to also need client context, load it then via `/context-frontend`, or use `/context-full` next time to load both up front.

After loading, briefly confirm what was loaded and wait for the user's actual request — do not start analyzing or summarizing the backend unprompted.
