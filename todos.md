# Admin client — API services refactor: TODO

Scope: `clients/admin`. Captured from discussion, for review before any implementation starts (per CLAUDE.md workflow gate — nothing here is approved/implemented yet).

## 1. Response-envelope mismatch check (deferred — investigate separately, not yet started)

Backend response shape (bare value vs. wrapped `Result<T>`/`ApiResponse`) is per-endpoint, not universal — confirmed no global wrapping filter exists in `src/`. Found candidate mismatches while spot-checking, need full verification before deciding on fixes:

- [ ] `features/notifications/api/get-unread-count.ts` — expects `Result<number>`, but `INotificationService.CountUnreadAsync` returns bare `Task<int>`.
- [ ] `features/notifications/api/mark-notification-read.ts` — expects `Result<NotificationDto | null>`, but `INotificationService.GetByIdAsync` returns bare `Task<NotificationDto?>`.
- [ ] `features/users/api/get-all-users.ts` — expects `Result<UserDto[]>`, but `IUserService.GetAllAsync` returns bare `Task<IEnumerable<UserDto>>`.
- [ ] `features/roles/api/get-all-roles.ts` — expects `Result<RoleDto[]>` (needs re-check), `IRoleService.GetAllAsync` returns bare `Task<IEnumerable<RoleDto>>`.
- [ ] Open question: does the internal "Light" framework (`Light.AspNetCore.Mvc.VersionedApiController`, referenced via NuGet, not in `src/`) auto-wrap bare `Ok(value)` responses at the HTTP boundary? This determines whether the above are real bugs or expected. Needs a targeted way to confirm (e.g. actually calling the endpoint, or reading the Light package) — full backend read was explicitly paused, not resumed yet.

## 2. Decouple auth-token injection from the API layer

Problem: every `features/*/api/*.ts` function (24 files) takes `accessToken: string` as an explicit parameter, and every `*-action.ts` caller (~20 files) threads `session.accessToken` through. Changing how/whether a token is obtained today means touching all of them.

Design agreed: mirror the `HttpMessageHandler`/`DelegatingHandler` pattern from `dotnet-starter-template-1`'s `HttpFactory` — auth injection lives outside the request-building code, declared per call site rather than hidden as one global default.

- [ ] `src/lib/server/http.ts`: add `HttpRequestContext { headers: Record<string,string> }` and `HttpRequestHandler = (context) => Promise<void> | void`. Remove `accessToken` from `RequestOptions`, add `handlers?: HttpRequestHandler[]`. `send()` runs `options.handlers` in order (each may mutate `headers`) before `fetch()`. `http.ts` no longer imports/knows about session at all.
- [ ] New file `src/lib/server/http-handlers/bearer-token-handler.ts` — `bearerTokenHandler: HttpRequestHandler` reads the session internally (`getSession()`) and sets `Authorization: Bearer <token>` when present.
- [ ] Update 21 of the 24 `features/*/api/*.ts` files (all except `auth/login.ts`, `auth/refresh-token.ts` — no token yet — and `dashboard/api/sample-data.ts` — not a real API call): drop the `accessToken` parameter, pass `handlers: [bearerTokenHandler]` in the request options instead.
- [ ] Update the ~20 `*-action.ts` callers: stop reading/passing `session.accessToken` to the api function. **Keep** the existing `resolveSession()` + `if (!session) return { error: "Your session has expired..." }` early-exit — that's still needed for the friendly expired-session message (otherwise the call proceeds with no token, hits the backend, and surfaces a generic 401 error instead).
- [ ] Verify `features/notifications/api/get-signalr-token-action.ts` is intentionally left alone (it reads `session.accessToken` directly to hand to the browser for the SignalR handshake — not a `http.ts` call, out of scope).

### 2.1 Multi-base-URL support (named client registry)

Problem: `buildUrl()` in `http.ts` hardcodes a single base URL (`getApiBaseUrl()` → env `API_BASE_URL`). Today the backend is one Modular Monolith process, so there's only one real base URL — but the API layer should be ready for a second service (e.g. a split-out microservice, a third-party API) without another rewrite across every `api/*.ts` file.

Design agreed: full named-client registry, mirroring `HttpClientConstants` + `AddHttpClients` from the `dotnet-starter-template-1` reference (name → base URL map), rather than a lighter per-call `baseUrl?: string` override.

- [ ] `src/lib/server/api-clients.ts` (or similar) — `export const ApiClients = { Backend: "backend" } as const;` (room to add more entries later, e.g. `ThirdParty`).
- [ ] `src/lib/server/config.ts` — replace the single `getApiBaseUrl()` with a name → base URL map (e.g. keyed by `ApiClients` values), each backed by its own env var. `Backend` maps to today's `API_BASE_URL`.
- [ ] `src/lib/server/http.ts` — add `client?: keyof typeof ApiClients` to `RequestOptions`, defaulting to `ApiClients.Backend`. `buildUrl()` resolves the base URL from the registry using `options.client`.
- [ ] Each `features/*/api/*.ts` file declares `client` explicitly only when it's not the default backend (i.e. today, no file needs to — all 21 stay implicit/default). Documents the pattern for when a second service is actually added.
- [ ] Ties into §2's `handlers` design: a future non-default `client` may also need a different `handlers` set (e.g. an API-key handler instead of `bearerTokenHandler`) — both are per-call declarations on the same `RequestOptions`, consistent with the "service declares what it needs" principle already agreed for auth handlers.

## 3. Client-side call-guard equivalent for UI feedback (toast / pending state)

Problem: `src/lib/server/call-guard.ts` only normalizes server-side network/HTTP results — it has no access to toast/dialog/spinner (those are browser-side concerns). Today, every dialog component hand-rolls its own "call action → toast on result → pending state" boilerplate:

- Form-submit dialogs (`create-user-dialog.tsx`, `edit-user-dialog.tsx`, `create-role-dialog.tsx`, `edit-role-dialog.tsx`): own `useActionState` + a `useEffect` watching `state.success` to call `notifySuccess(...)` with a hardcoded message.
- Confirm-then-run dialogs (`delete-user-dialog.tsx`, `delete-role-dialog.tsx`): own `useTransition` + manual `if (!result.success) notifyError(...) else notifySuccess(...)`, plus hand-drawn confirm UI.

Reference for the *intent* (not a direct port — this is server-action architecture, not a live DI container like Blazor): `light-nuget/src/blazor/src/Blazor/CallGuarded.cs` — centralizes toast/spinner/confirm behind injected interfaces so swapping the toast/dialog library later touches one place, not every call site.

Decisions made so far:
- [x] Keep "guarded execute" (call + auto toast + pending) and "confirm before execute" as **separate** concerns/hooks — not one combined API.
- [x] Whether/how to confirm before running is decided by the calling page/component, not baked into the guarded-execute hook.

Still open / not yet designed:
- [ ] Shape of the client-side "guarded execute" hook (name, signature, where it lives — likely `src/components/toast/` or a new shared `hooks/` location — needs a decision).
- [ ] Whether it needs to cover both the `useActionState` (form action) shape and the `useTransition` (imperative call, e.g. delete) shape, or just one.
- [ ] Whether a shared confirm-dialog UI component is in scope at all, given confirm stays the calling page's decision — or whether pages keep hand-rolling their own confirm dialogs.
- [ ] Which existing dialogs get migrated once the hook exists (candidates: the 6 listed above, plus `send-notification-dialog.tsx` — not yet checked).

## Not yet covered in this discussion

- Any other `clients/admin` API-layer concerns beyond the three areas above.
