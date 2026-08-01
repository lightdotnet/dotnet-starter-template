---
description: Load frontend/clients-only context (CLAUDE.md + PROJECT-CLIENTS.md + ARCHITECTURE-CLIENTS.md)
---

Load frontend/clients-scoped context for this session:

1. Read `.claude/CLAUDE.md` if not already read this session.
2. Read `.claude/PROJECT-CLIENTS.md`.
3. Read `.claude/ARCHITECTURE-CLIENTS.md`.

Do not read `PROJECT-BACKEND.md`, `ARCHITECTURE-BACKEND.md`, or anything under `src/` as part of this command — this is a clients-only context load. If the request names a specific client app (once `clients/` has more than one), scope further to that app rather than treating "the frontend" as singular — see [skills/analyze-client.md](../skills/analyze-client.md) vs [skills/analyze-frontend.md](../skills/analyze-frontend.md). If the user's next request turns out to also need backend context, load it then via `/context-backend`, or use `/context-full` next time to load both up front.

After loading, briefly confirm what was loaded and wait for the user's actual request — do not start analyzing or summarizing the frontend unprompted.
