# Admin client — API services refactor: TODO

Scope: `clients/admin`. Captured from discussion, for review before any implementation starts (per CLAUDE.md workflow gate — nothing here is approved/implemented yet).

## 1. Response-envelope mismatch check (deferred — investigate separately, not yet started)

Backend response shape (bare value vs. wrapped `Result<T>`/`ApiResponse`) is per-endpoint, not universal — confirmed no global wrapping filter exists in `src/`. Found candidate mismatches while spot-checking, need full verification before deciding on fixes:

- [ ] `features/notifications/api/get-unread-count.ts` — expects `Result<number>`, but `INotificationService.CountUnreadAsync` returns bare `Task<int>`.
- [ ] `features/notifications/api/mark-notification-read.ts` — expects `Result<NotificationDto | null>`, but `INotificationService.GetByIdAsync` returns bare `Task<NotificationDto?>`.
- [ ] `features/users/api/get-all-users.ts` — expects `Result<UserDto[]>`, but `IUserService.GetAllAsync` returns bare `Task<IEnumerable<UserDto>>`.
- [ ] `features/roles/api/get-all-roles.ts` — expects `Result<RoleDto[]>` (needs re-check), `IRoleService.GetAllAsync` returns bare `Task<IEnumerable<RoleDto>>`.
- [ ] Open question: does the internal "Light" framework (`Light.AspNetCore.Mvc.VersionedApiController`, referenced via NuGet, not in `src/`) auto-wrap bare `Ok(value)` responses at the HTTP boundary? This determines whether the above are real bugs or expected. Needs a targeted way to confirm (e.g. actually calling the endpoint, or reading the Light package) — full backend read was explicitly paused, not resumed yet.

## 2. Decouple auth-token injection from the API layer — ✅ done (commit `da7f0e9`)

Problem: every `features/*/api/*.ts` function (24 files) took `accessToken: string` as an explicit parameter, and every `*-action.ts` caller (~20 files) threaded `session.accessToken` through. Changing how/whether a token is obtained meant touching all of them.

Implemented: `http.ts` now exposes `HttpRequestContext`/`HttpRequestHandler` and a `handlers?: HttpRequestHandler[]` option instead of `accessToken` — it no longer knows about sessions at all. `http-handlers/bearer-token-handler.ts` provides `bearerTokenHandler` (reads the ambient session) and `explicitBearerTokenHandler(token)` (for the two call sites that run before a session cookie exists: `login-action.ts`, and `get-current-user.ts`'s use from `proxy.ts` Edge middleware). A new `backend-api.ts` wraps `http.ts`'s `requestJson`/`requestVoid` to default-inject `bearerTokenHandler` + `ApiClients.Backend`, so the 20 ordinary api files just import from `backend-api.ts` with no `accessToken` param and no per-call boilerplate; only the explicit-token exception imports `http.ts` directly. All `*-action.ts` callers stopped passing `session.accessToken` while keeping the `resolveSession()` expired-session check. `get-signalr-token-action.ts` was left alone as planned (reads `session.accessToken` directly for the SignalR handshake, not a `http.ts` call).

### 2.1 Multi-base-URL support (named client registry) — ✅ done (commit `da7f0e9`)

Problem: `buildUrl()` in `http.ts` hardcoded a single base URL. Implemented the full named-client registry: `src/lib/server/api-clients.ts` exports `ApiClients = { Backend: "Backend" } as const` (room for more entries), `config.ts`'s `getApiBaseUrl(client)` resolves per-client env vars via an internal map (`Backend` → `API_BASE_URL`), and `RequestOptions.client?: ApiClientName` (default `ApiClients.Backend`) drives `buildUrl()`. No file needs a non-default `client` yet — registered for when a second service is added.

## 3. Client-side call-guard equivalent for UI feedback (toast / pending state) — ✅ done

Problem: `src/lib/server/call-guard.ts` only normalizes server-side network/HTTP results — it has no access to toast/dialog/spinner (those are browser-side concerns). Every dialog component hand-rolled its own "call action → toast on result → pending state" boilerplate.

Reference for the *intent* (not a direct port — this is server-action architecture, not a live DI container like Blazor): `light-nuget/src/blazor/src/Blazor/CallGuarded.cs`.

Decisions made:
- [x] Kept "guarded execute" (call + auto toast + pending) and "confirm before execute" as **separate** concerns — confirm stays the calling page's decision, not baked into the hook.
- [x] Covers both shapes: a `useTransition`-based hook for imperative calls, and a `useActionState`-companion hook for form actions.

Implemented (`src/hooks/`):
- `use-guarded-action.ts` — `useGuardedAction()` → `[pending, run]`. `run(action, successMessage?, onSuccess?)` mirrors `CallGuarded.ExecuteAsync(call, successMessage, runIfSuccess)`. Applied to `delete-user-dialog.tsx`, `delete-role-dialog.tsx` (confirm UI itself untouched, still hand-drawn per the page's-own-decision principle).
- `use-action-success-toast.ts` — `useActionSuccessToast(state, successMessage, onSuccess?)`, an effect wrapping `useActionState` results. Applied to `create-user-dialog.tsx`, `create-role-dialog.tsx`, `send-notification-dialog.tsx`, `edit-role-dialog.tsx`, `edit-user-dialog.tsx` (2 call sites: update + password reset).
- No shared confirm-dialog component was built — out of scope per the "page decides" principle; each delete dialog still draws its own confirm UI.
- `tsc --noEmit` and `eslint` clean on all touched files.

## Not yet covered in this discussion

- Any other `clients/admin` API-layer concerns beyond the three areas above.
