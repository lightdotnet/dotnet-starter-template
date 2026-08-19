# Backend Project Analysis — 2026-07-30 (Archival)

> **Archival.** This file was originally a full point-in-time refactor analysis of the `Identity` module. Every finding it raised has since been either resolved (see git history) or is tracked live in [docs/KNOWN_DEBT.md](../docs/KNOWN_DEBT.md) — that file is the canonical, current-state list; nothing here should be treated as still-open. This file is kept only for the two lessons below, which have ongoing diagnostic value.

## Lessons learned

- **A short claim name can silently collide with `JwtSecurityTokenHandler`'s legacy inbound claim-type map.** The session-id claim was originally named `"tid"`; `JwtSecurityTokenHandler`'s built-in `DefaultInboundClaimTypeMap` (a legacy WS-Federation/Azure-AD-interop table) silently remaps `"tid"` to a different, unrelated claim URI on every token validation, breaking any code that looks up `"tid"` literally. Fixed by renaming to `"jti"` (the standard JWT-ID claim, not present in that map). If a new short claim name is ever introduced, check it against `JwtSecurityTokenHandler.DefaultInboundClaimTypeMap` first — `"email"` was checked at the time and was safe only because no `Email` claim was being issued yet.
- **A broken EF Core migration diff doesn't always mean the migration itself is wrong.** `dotnet ef migrations add` produced a bloated, broken diff for one provider; the instinctive read ("the migration history is stale, tables are missing") was wrong. The actual cause was a stale `*ModelSnapshot.cs` — the EF bookkeeping file used only to compute the *next* diff — out of sync with the model despite the migration's own `.Designer.cs` being correct. Hand-patching the snapshot made it worse. The fix was to delete the stale migration + designer + snapshot files and rescaffold fresh from the current model (see [docs/migrations.md](../../docs/migrations.md)). Worth remembering as the first thing to check the next time a migrations-add diff looks wrong for one provider but not the others.

---
_Trimmed to lessons-learned only: 2026-08-19 (see [ROT.md](../ROT.md) Review Log). Full original analysis available in git history at this file's prior revision._
