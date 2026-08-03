# Client App Overview: admin

## Purpose

Internal admin console for the ModularMonolith starter template. Per `src/app/layout.tsx` metadata: "Admin Dashboard" / "Admin dashboard for the ModularMonolith starter kit." This is the **first and only client app** in this template repo (`clients/web` does not exist — verified via directory listing of `clients/`).

Since the last sync: the Users feature gained edit/delete dialogs, rounding out full CRUD, and its permission-check strings were corrected to match the backend (`Users.View` → `identity.users.view`, etc.); Roles went from API-only scaffolding to a full CRUD UI at `/identity/roles` (previously 404ing despite a nav entry already pointing at it), with its permissions checklist sourced live from a new `GET /permissions` catalog endpoint rather than any hardcoded list; and the shared `DataTable` had a bug fixed where a stale error from a prior failed load could mask a subsequent in-flight loading state. The dashboard page itself **still renders static mock data** — unchanged this sync (verified: `features/dashboard/api/sample-data.ts` and its own comment "this UI shell does not call src/Identity.Api yet").

## Structure

- **Router**: App Router, now rooted at `src/app/` (was `app/` directly under `clients/admin/` before this sync) — verified via directory listing; no `pages/` directory exists.
- **Package manager**: pnpm — verified via `pnpm-lock.yaml`. A `pnpm-workspace.yaml` now exists too, but it only configures pnpm's build-script approval (`allowBuilds`/`ignoredBuiltDependencies` for `sharp`, `unrs-resolver`) — it does not define a multi-package workspace; `clients/admin` is still a single, independent app.
- **Data fetching approach**: server-only, hand-written per endpoint — every `features/<name>/api/*.ts` file wraps one backend call via `lib/server/http.ts`'s `requestJson`/`requestVoid`, normalized through `lib/server/call-guard.ts`. Whole-list reads happen in async Server Components (`user-profile-page.tsx`, `(dashboard)/layout.tsx`, `features/users/components/users-page.tsx`, `features/roles/components/roles-page.tsx`); writes happen via a Next.js Server Action — now 8 of them, all `"use server"`, same `(prevState, formData) => Promise<{error?, success?}>` shape: `features/auth/api/login-action.ts`, `features/users/api/{create,update,force-password,delete}-user-action.ts`, `features/roles/api/{create,update,delete}-role-action.ts`. Two more Server Actions serve a third purpose — an on-demand single-record *read* triggered from a Client Component when a list row's own data isn't complete enough (see [architecture.md](./architecture.md) for why): `features/users/api/get-user-detail-action.ts` and `features/roles/api/get-role-detail-action.ts`, both a plain `(id) => Promise<{data, error?}>` called directly rather than bound to a `<form>`. No client-side data-fetching library (no React Query/SWR in `package.json`). The dashboard page is the one exception — it still reads static arrays from `features/dashboard/api/sample-data.ts` instead of calling the backend.
- **State management**: local component state + React Context, no global state library:
  - `hooks/use-sidebar.tsx` (`SidebarProvider`/`useSidebar`) — sidebar hidden/expanded/mobile-open state, persisted to `localStorage`.
  - `components/theme/accent-color-provider.tsx` (`AccentColorProvider`/`useAccentColor`) — accent color selection, persisted to `localStorage`, applied via `data-accent` on `<html>`. Moved here from a former `hooks/use-accent-color.tsx`.
  - `components/theme/theme-provider.tsx` — thin wrapper around `next-themes`' `ThemeProvider`. Moved here from a former top-level `providers/` folder.
  - `features/dashboard/components/users-table.tsx` — local `useState` for pagination (`page`).
  - `features/auth/components/login-form.tsx` — `useActionState` bound to the `loginAction` server action.
  - `features/users/components/users-data-table.tsx` — drives search/pagination through URL `searchParams` via `router.push`/`router.refresh()` wrapped in `useTransition`; owns open/selected-row state for all three dialogs (create/edit/delete) plus a per-dialog remount-key counter used to force-remount each on every open.
  - `features/users/components/{create,edit}-user-dialog.tsx` — `useActionState` bound to their respective Server Actions, plus a parallel controlled `useState<FormValues>` for the form fields (deliberate — see [architecture.md](./architecture.md) for why). `edit-user-dialog.tsx` (new) additionally fetches the full user record via `getUserDetailAction` in a `useEffect` on open, since the list endpoint's DTO doesn't carry `roles`/`claims`.
  - `features/users/components/delete-user-dialog.tsx` (new) — `useTransition` wrapping a direct (non-form) call to `deleteUserAction`.
  - `features/roles/components/roles-data-table.tsx` (new) — same shape as `users-data-table.tsx`, but drives search via local `useState` + client-side `.filter()` instead of URL params, since `RoleController` has no search/pagination endpoint.
  - `features/roles/components/edit-role-dialog.tsx` (new) — same "fetch full detail on open" pattern as `edit-user-dialog.tsx`, via `getRoleDetailAction`.
  - `components/shared/data-table/data-table-toolbar.tsx` (new) — local debounced search-text state (400ms) before calling the `onSearchChange` prop; `data-table-pagination.tsx` (new) — local page-jump input state, committed on blur/Enter.
- **Styling**: Tailwind CSS v4, CSS-first configuration — `src/app/globals.css` uses `@import "tailwindcss"` plus an inline `@theme inline { ... }` block. No `tailwind.config.ts`/`.js` file. `postcss.config.mjs` wires in `@tailwindcss/postcss`. Unchanged since the last sync.

## Key Routes/Areas

| Route/Area | Path | Responsibility | Notes |
|---|---|---|---|
| Dashboard | `src/app/(dashboard)/page.tsx` (route `/`) | Re-exports `DashboardPage` from `@/features/dashboard` | Still mock data only — see Backend Integration |
| Profile | `src/app/(dashboard)/user-profile/page.tsx` (route `/user-profile`) | Re-exports `UserProfilePage` from `@/features/user-profile` | Account details, QR code of the user ID, roles/claims, all fetched live |
| Login | `src/app/login/page.tsx` (route `/login`) | Re-exports `LoginPage` from `@/features/auth` | Outside the `(dashboard)` group, no `AppShell`/session resolution |
| Dashboard-group layout | `src/app/(dashboard)/layout.tsx` | Calls `resolveSession()`, then renders `AppShell` with the resolved user (or `null`) | Wraps `/`, `/user-profile`, `/identity/users`, `/identity/roles`; unauthenticated users never actually reach it in practice because `proxy.ts` redirects first |
| Root layout | `src/app/layout.tsx` | Loads `Inter` font, wraps app in `ThemeProvider` → `AccentColorProvider` → `TooltipProvider`, mounts `<AppToaster />` (`components/toast`) as a sibling of `TooltipProvider` | `<html suppressHydrationWarning>` for `next-themes` |
| Users | `src/app/(dashboard)/identity/users/page.tsx` (route `/identity/users`) | Re-exports `UsersPage` from `@/features/users` | Full CRUD: list, create, edit, delete, force-password-reset |
| Roles | `src/app/(dashboard)/identity/roles/page.tsx` (route `/identity/roles`) | Re-exports `RolesPage` from `@/features/roles` | New this sync — this route previously had no `page.tsx` and 404'd despite the nav entry existing. Full CRUD: list (client-filtered, no server search), create (name/description only — the backend's `POST /role` accepts no claims), edit (name/description + a permissions checklist), delete |

Every `page.tsx` under `src/app/` is now a one-line re-export from a feature's public barrel (`export { X as default } from "@/features/<name>";`) — routing files contain no logic.

`src/constants/nav-items.ts` still declares nav entries for `/identity` (children `/identity/users`, `/identity/roles`) and `/settings`. `/identity/users` and `/identity/roles` both now have real pages (see above); `/settings` still has **no corresponding `page.tsx`** (verified via directory listing) and still 404s if followed.

## Backend Integration

Real, but partial. `lib/server/http.ts` calls `API_BASE_URL` (required server-only env var, see `.env.example`), prefixing every path with `api/v1/`. Requests attach `Authorization: Bearer <accessToken>` when a token is passed. Non-2xx responses now surface a real message where available: `send()`'s new `extractErrorMessage` helper reads the response body and prefers, in order, the app's own `ApiResponse.message`, an ASP.NET `ValidationProblemDetails.errors` map, or a `ProblemDetails.title`, falling back to the old generic "request failed with status N" message only if none parse — this is shared-kernel and applies to every `features/*/api/*.ts` call, not just `users`.

Endpoints currently wired, by feature:
- **`auth`**: `token/token/get` (login, POST — path has a doubled `token/token` segment because `TokenController`'s own route plus its action route both contribute `token`, per an explicit comment in `login.ts`), `token/token/refresh` (`refreshToken()` is defined but **not called anywhere** — no refresh flow is wired up yet).
- **`user-profile`**: `user_profile` (`getCurrentUser`, GET), `user_profile/token/list` (`listSessions`, GET) and `user_profile/token/revoke` (`revokeSession`, PUT) — both exist and are exported from the feature barrel but have **no UI consumer** yet.
- **`users`**: `user/search` (GET, `searchUsers`), `user` (GET all, `getAllUsers`; PUT-by-id, `updateUser`; DELETE-by-id, `deleteUser`), plus get-by-id, get-by-username, create, force-password — the full `Identity.Api` `UserController` surface, one function per file. Every write path now has a real UI consumer at `/identity/users` (search, create, edit, delete, force-password-reset); `get-user-by-id` gained a second caller this sync via `get-user-detail-action.ts`, the edit dialog's on-open detail fetch (see [architecture.md](./architecture.md)'s "fetch full detail on open" pattern). `getAllUsers`/`getUserByUsername` remain unconsumed.
- **`roles`**: `role` (GET all, `getAllRoles`; POST, `createRole`; PUT, `updateRole`; DELETE-by-id, `deleteRole`), plus get-by-id — the full `Identity.Api` `RoleController` surface. **Went from zero UI consumers to full CRUD this sync** at `/identity/roles`; `get-role-by-id` is used via `get-role-detail-action.ts` for the edit dialog's on-open fetch. `getAllRoles` was also fixed this sync — it previously assumed the endpoint returned a bare array (`guardRawCall`) when the backend actually wraps it in the same envelope every other endpoint uses; this went unnoticed because nothing called it until now.
- **`permissions`** (new this sync): `permissions` (GET, `getPermissions`) — the full catalog of definable permissions (`PermissionDefinition { name, displayName, parent }`), backing the Roles edit dialog's permissions checklist. Fetched server-side in `roles-page.tsx` only when the viewer can manage roles.
- **`dashboard`**: none — `DashboardPage` still reads `STAT_SUMMARIES`/`SAMPLE_USERS` from `features/dashboard/api/sample-data.ts`, which is hardcoded and explicitly commented as not calling the backend yet.

Each API function returns a normalized `Result`/`ApiResponse`-shaped envelope via `guardCall`/`guardResponseCall`/`guardRawCall` in `lib/server/call-guard.ts`, matching the backend's own `Result<T>` envelope (`types/api.ts`).

Both `/identity/users` and `/identity/roles` gate on permissions via `lib/server/authorization.ts`'s `hasPermission(session, userName, permission)`. Permission-string constants now live per-feature rather than in one shared file: `features/users/constants/permissions.ts` (`USERS_PERMISSIONS`) and `features/roles/constants/permissions.ts` (`ROLES_PERMISSIONS`) — the former `constants/permissions.ts` is gone. The string values were also corrected this sync to match the backend's actual format (`Identity.Contracts/Authorization/IdentityPermissions.cs`): lowercase, dotted, `identity`-prefixed (e.g. `identity.users.view`, `identity.roles.manage`), not the previous `Users.View`-style strings, which never matched anything the backend issues. Missing the `*.View` permission renders an inline "Access denied" `Empty` block instead of redirecting; missing `*.Create`/`*.Update`/`*.Delete`/`*.Manage` just hides the corresponding action.

## Auth Flow

Cookie-based session, implemented this cycle (previously "not implemented"):

1. `/login` → `LoginForm` (`features/auth/components/login-form.tsx`) submits to `loginAction`, a `"use server"` Server Action (`features/auth/api/login-action.ts`).
2. `loginAction` calls `login()` (POST `token/token/get`). On success it also calls `getCurrentUser()` to fetch claims (a failure here doesn't block login — claims default to `[]`), dedupes claims via `dedupeClaims()`, and writes an `admin_session` cookie (`httpOnly`, `sameSite: "lax"`, `path: "/"`, `maxAge` = the access token's `expiresIn`) containing the access token, refresh token, expiry, and claims **as plaintext JSON** — the code has its own comment flagging this: "Plaintext for now — encryption is a separate follow-up step."
3. `src/proxy.ts` (a Next.js 16 convention file, functionally equivalent to middleware) checks for the `admin_session` cookie on every non-static/non-API request: redirects to `/login?from=<path>` if absent, and redirects `/login` → `/` if present.
4. `lib/server/session.ts`'s `getSession()` reads and JSON-parses the cookie (no fetch). `features/user-profile/api/resolve-session.ts`'s `resolveSession()` composes `getSession()` with a live `getCurrentUser()` call — used by the dashboard-group layout and the profile page. There is no client-side session cache; the profile is refetched from the backend on every request that needs it.
5. `.env.example` also declares `TOKEN_ENCRYPTION_KEY` and `lib/server/config.ts` exposes a `getTokenEncryptionKey()` that throws if unset — but **this function is never called anywhere in the codebase**. It appears to be prep work for the "separate follow-up step" mentioned above, not yet wired to actual encryption.

Sign-out has no implementation: `components/layout/user-menu.tsx`'s "Log out" menu item has no `onClick`/handler.

## External Dependencies

From `package.json` `dependencies` (changes since last sync noted):

- **`next` 16.2.12, `react`/`react-dom` 19.2.4** — framework/runtime, unchanged.
- **`radix-ui` ^1.6.7** — unified Radix primitives, base for `components/ui/*`.
- **`class-variance-authority` ^0.7.1** — variant class composition.
- **`clsx` ^2.1.1 + `tailwind-merge` ^3.6.0** — combined in `lib/shared/utils.ts`'s `cn()` (moved from a former root `lib/utils.ts`).
- **`lucide-react` ^1.28.0** — icon set.
- **`next-themes` ^0.4.6** — theme switching; also now drives `components/toast/toaster.tsx`'s light/dark toast theme.
- **`qrcode` ^1.5.4** — generates the data-URI QR code of the user ID on `/user-profile` (`features/user-profile/components/user-profile-page.tsx`).
- **`sonner` ^2.0.7 — NEW** — toast notifications, wrapped by `components/toast/` (`AppToaster`, `notifySuccess`/`notifyError`); currently only called from `CreateUserDialog`'s success path.
- **`shadcn` ^4.16.0** — CLI + runtime stylesheet import (`app/globals.css` imports `shadcn/tailwind.css`).
- **`tw-animate-css` ^1.4.0** — animation utility classes.

From `devDependencies`: unchanged this sync (`@tailwindcss/postcss`/`tailwindcss`, `eslint`/`eslint-config-next`, `prettier`/`prettier-plugin-tailwindcss` — still no `.prettierrc*`/`format` script, `typescript`, `@types/node`, `@types/react`, `@types/react-dom`, `@types/qrcode`).

## Relationship to Other Client Apps

`clients/admin` is still the only subfolder under `clients/` (verified). It remains fully independent — the new `pnpm-workspace.yaml` only configures pnpm build-script approval, it is not a multi-app monorepo workspace root. Nothing is shared with a sibling app because none exists yet.

## Notes

<!-- manual: content below this line is human-authored and must be preserved verbatim during sync -->

---
_Generated: 2026-08-03 — scope: client app "admin" — see .claude/CLAUDE.md for update rules._
