# Architecture: admin

## Layering

Observed folder organization (verified via directory listing) — everything now lives under `clients/admin/src/` (previously at `clients/admin/` root):

```text
src/
  app/                      routing only — layout.tsx, globals.css, login/page.tsx,
                             (dashboard)/{layout.tsx,page.tsx,user-profile/page.tsx,identity/users/page.tsx}
  features/
    auth/                   api/{login,refresh-token,login-action}.ts, components/{login-page,login-form}.tsx, index.ts
    user-profile/           api/{get-current-user,list-sessions,revoke-session,resolve-session}.ts, components/user-profile-page.tsx, types/user-session.ts, index.ts
    dashboard/              api/sample-data.ts (mock, not a backend call), components/{stat-card,users-table,dashboard-page}.tsx, index.ts
    users/                  api/*.ts (9 endpoint files: the prior 8 plus create-user-action.ts, a "use server" Server Action),
                             components/{users-page,users-data-table,create-user-dialog}.tsx (new — first UI this feature has had), index.ts
    roles/                  api/*.ts (5 endpoint files, API-only — no UI consumer), types/role.ts, index.ts
  components/
    ui/                     shadcn-CLI-generated primitives (24 files)
    layout/                 topbar, sidebar, sidebar-nav-item, brand, breadcrumbs, user-menu, app-shell
    theme/                  theme-provider, accent-color-provider, theme-toggle, accent-color-picker, use-has-mounted, index.ts (barrel)
    shared/
      search-box.tsx
      data-table/           new — types.ts, data-table-toolbar.tsx, data-table-pagination.tsx, data-table.tsx, index.ts (barrel);
                             generic reusable list-table building block, no data-fetching of its own
    toast/                  new — toast-theme.ts, notify.ts, toaster.tsx, index.ts (barrel); wraps the `sonner` dependency
  hooks/                    use-sidebar.tsx, use-scrolled.ts
  lib/
    server/                 config.ts, http.ts, call-guard.ts, session-cookie.ts, session.ts, authorization.ts
                             (authorization.ts pre-existed but had no consumer until this sync's `features/users/components/users-page.tsx`)
    shared/                 utils.ts, dedupe-claims.ts, user-display.ts
  constants/                nav-items.ts, permissions.ts (new — `IDENTITY_PERMISSIONS`, mirrors `Identity.Contracts/Authorization/IdentityPermissions.cs`)
  types/                    api.ts, nav.ts, session.ts, token.ts, user.ts
  proxy.ts                  Next.js 16 convention file (middleware-equivalent)
```

This is now a **feature-folder** layering, replacing the earlier route → layout-chrome → feature-component → primitive-only structure: `app/*` pages are pure re-exports from a feature's public API; `features/<name>/` owns its own `api/`, `components/`, and (when a type has exactly one consumer) `types/`, each exposed through an `index.ts` barrel; `components/layout/*` (app chrome) and `components/theme/*` compose `components/ui/*` plus `hooks/*`/`lib/shared/*`/`constants/*`/`types/*`; `components/ui/*` remains the leaf primitive layer. `components/shared/data-table/` and `components/toast/` are new cross-feature building blocks sitting at the same layer as `components/shared/search-box.tsx` — presentational/utility, composed by features but with no feature-specific knowledge baked in.

## Dependency Direction

Verified via actual `import` statements:

```text
src/proxy.ts                       -> lib/server/session-cookie (constant only, no next/headers — edge-safe)
src/app/layout.tsx                 -> components/theme (ThemeProvider, AccentColorProvider), components/toast (AppToaster),
                                       components/ui/tooltip
src/app/login/page.tsx             -> features/auth (LoginPage)
src/app/(dashboard)/layout.tsx     -> components/layout/app-shell, features/user-profile (resolveSession)
src/app/(dashboard)/page.tsx       -> features/dashboard (DashboardPage)
src/app/(dashboard)/user-profile/page.tsx -> features/user-profile (UserProfilePage)
src/app/(dashboard)/identity/users/page.tsx -> features/users (UsersPage)

components/layout/app-shell.tsx    -> hooks/use-sidebar, components/layout/{sidebar,topbar}
components/layout/topbar.tsx       -> lib/shared/utils, hooks/{use-scrolled,use-sidebar}, components/ui/{button,badge},
                                       components/layout/{breadcrumbs,brand,user-menu}, components/shared/search-box,
                                       components/theme (ThemeToggle, AccentColorPicker)
components/layout/sidebar.tsx      -> lib/shared/utils, hooks/use-sidebar, components/layout/sidebar-nav-item,
                                       constants/nav-items, components/ui/sheet
components/layout/sidebar-nav-item.tsx -> lib/shared/utils, hooks/use-sidebar, types/nav
components/layout/breadcrumbs.tsx  -> components/ui/breadcrumb, constants/nav-items, types/nav
components/layout/user-menu.tsx    -> components/ui/{avatar,button,dropdown-menu}, lib/shared/user-display, types/user
                                       (now takes a `user: UserDto | null` prop — no more hardcoded MOCK_USER)

components/theme/theme-toggle.tsx        -> next-themes, ./use-has-mounted, components/ui/{button,dropdown-menu}
components/theme/accent-color-picker.tsx -> components/ui/{button,dropdown-menu}, ./accent-color-provider

features/auth/index.ts              -> ./components/login-page, ./api/login, ./api/refresh-token
features/auth/components/login-page.tsx  -> components/ui/card, ./login-form
features/auth/components/login-form.tsx  -> components/ui/{button,input,label,alert}, features/auth/api/login-action
features/auth/api/login-action.ts   -> "use server"; features/auth/api/login, features/user-profile (getCurrentUser),
                                        lib/shared/dedupe-claims, lib/server/session-cookie, types/session
features/auth/api/login.ts          -> lib/server/http, lib/server/call-guard, types/{api,token}

features/user-profile/index.ts      -> ./components/user-profile-page, ./api/{resolve-session,get-current-user,list-sessions,revoke-session}, ./types/user-session
features/user-profile/components/user-profile-page.tsx -> components/ui/{card,badge,separator,avatar,alert}, qrcode,
                                        features/user-profile/api/resolve-session, lib/shared/{dedupe-claims,user-display}
features/user-profile/api/resolve-session.ts -> lib/server/session (getSession), ./get-current-user, types/{api,session,user}
features/user-profile/api/get-current-user.ts -> lib/server/http, lib/server/call-guard, types/{api,user}
features/user-profile/api/{list-sessions,revoke-session}.ts -> lib/server/http, lib/server/call-guard, ./types/user-session (list)

features/dashboard/index.ts         -> ./components/dashboard-page
features/dashboard/components/dashboard-page.tsx -> components/ui/card, ./stat-card, ./users-table, features/dashboard/api/sample-data
features/dashboard/components/{stat-card,users-table}.tsx -> components/ui/*, lib/shared/utils, features/dashboard/api/sample-data (mock data, no backend call)

features/users/index.ts             -> ./components/users-page, ./api/{search-users,get-all-users,get-user-by-id,
                                        get-user-by-username,create-user,update-user,delete-user,force-password}
features/users/components/users-page.tsx -> features/user-profile (resolveSession), features/users/api/search-users,
                                        features/users/components/users-data-table, lib/server/authorization (hasPermission),
                                        constants/permissions, components/ui/empty
features/users/components/users-data-table.tsx -> components/shared/data-table (DataTable + types), components/ui/{avatar,badge},
                                        features/users/components/create-user-dialog,
                                        features/user-profile/components/user-status-badge, lib/shared/user-display, types/user
features/users/components/create-user-dialog.tsx -> components/ui/{alert,button,dialog,input,label}, components/toast (notifySuccess),
                                        features/users/api/create-user-action
features/users/api/create-user-action.ts -> "use server"; features/user-profile (resolveSession), features/users/api/create-user, types/user
features/users/api/*.ts (8 remaining files) -> lib/server/http, lib/server/call-guard, types/{api,user}

features/roles/*                    -> lib/server/http, lib/server/call-guard, features/roles/types/role
                                        (no importer outside their own feature — no UI consumer yet; unchanged this sync)

components/shared/data-table/data-table.tsx -> components/ui/{table,skeleton,empty,alert}, ./data-table-toolbar, ./data-table-pagination, ./types
components/shared/data-table/data-table-toolbar.tsx -> components/ui/{button,dropdown-menu}, lib/shared/utils, ./types
components/shared/data-table/data-table-pagination.tsx -> components/ui/{input,pagination}

components/toast/toaster.tsx        -> next-themes, sonner, ./toast-theme
components/toast/notify.ts          -> sonner
components/toast/toast-theme.ts     -> sonner (types only)

lib/server/session.ts               -> next/headers (cookies), lib/server/session-cookie, types/session
lib/server/http.ts                  -> lib/server/config (getApiBaseUrl)
lib/server/authorization.ts         -> types/session (SessionData) — no other lib/server/* dependency
components/ui/*                     -> lib/shared/utils, radix-ui, class-variance-authority, lucide-react
                                        (button.tsx additionally -> components/ui/spinner)
```

Direction is still one-way: `components/ui/*` never imports from `components/layout/*`, `components/theme/*`, or `features/*`. Cross-feature imports go through a feature's `index.ts` barrel, not its internals — e.g. `features/auth/api/login-action.ts` and `features/users/components/users-page.tsx`/`api/create-user-action.ts` all import `resolveSession`/`getCurrentUser` from `@/features/user-profile` (the barrel), not from its internals directly. `features/dashboard` and `features/roles` are still not imported by any other feature; `features/users` is now imported by `app/(dashboard)/identity/users/page.tsx` (routing, not another feature) but not by any sibling feature. `components/shared/data-table/*` and `components/toast/*` sit below `features/*` in the same leaf-adjacent tier as `components/ui/*` — they're imported by feature code but import nothing from `features/*` themselves.

## Key Design Patterns

- **Feature-folder + barrel-export convention**: each `features/<name>/` owns `api/` (one file per backend endpoint), `components/`, optionally `types/` (only for types with exactly one consumer — `features/roles/types/role.ts`, `features/user-profile/types/user-session.ts`), and an `index.ts` that is the only sanctioned import surface for other features or `app/*`.
- **Routing files are pure re-exports**: every `app/**/page.tsx` is a one-line `export { X as default } from "@/features/<name>";` — no logic lives in `app/`.
- **Server-only API layer, one function per endpoint file**: `lib/server/http.ts` (`requestJson`/`requestVoid`) is the single fetch wrapper; every `features/*/api/*.ts` file wraps exactly one backend call and returns a normalized result via `lib/server/call-guard.ts`'s `guardCall`/`guardResponseCall`/`guardRawCall` (chosen based on whether the backend endpoint returns a `Result<T>` envelope, a bare `ApiResponse`, or a raw value/array).
- **Cookie session + server-side session resolution, no client cache**: `lib/server/session.ts` reads the `admin_session` cookie; `features/user-profile/api/resolve-session.ts` composes that with a live `getCurrentUser()` call. Kept in two layers deliberately — `getSession()` has no feature dependency, while `resolveSession()` needs `user-profile`'s API, so it lives in the feature instead of `lib/server`.
- **Edge-safe cookie name constant**: `lib/server/session-cookie.ts` exports only the `SESSION_COOKIE_NAME` string with no `next/headers` import, specifically so `proxy.ts` (which runs on the edge runtime) can import it without pulling in the Node-only cookie-reading API.
- **Context-provider-per-concern for client state**: `SidebarProvider`/`useSidebar`, `AccentColorProvider`/`useAccentColor`, and `next-themes`' provider (wrapped in `components/theme/theme-provider.tsx`) each own one slice of persisted UI state via a throwing custom hook — unchanged pattern, relocated files.
- **Owned, CLI-generated UI primitives**: `components/ui/*` (24 files, style `"radix-nova"`) still follows the `data-slot="<name>"` + `cva()` convention. `button.tsx` retains its hand-modification beyond CLI output: a `loading` prop (renders `Spinner`, sets `aria-busy`/`disabled`) plus a `cursor-pointer` utility baked into `buttonVariants`.
- **Single-CSS-variable theming** and **runtime accent swap via DOM attribute + localStorage**: unchanged from before — `--primary` drives themed surfaces, `AccentColorProvider` sets `data-accent` on `<html>`.
- **Hydration-safe browser-state restoration**: unchanged pattern (`hydrated` flag + `useEffect`, `eslint-disable react-hooks/set-state-in-effect`) in `SidebarProvider` and `AccentColorProvider`; `components/theme/use-has-mounted.ts` (`useSyncExternalStore`) still guards `ThemeToggle`.
- **Mobile drawer closes on route change via render-time state adjustment**: unchanged, still in `hooks/use-sidebar.tsx`.
- **Generic, presentational `DataTable<TData>` building block** (new): `components/shared/data-table/` composes a toolbar (actions + debounced search + export/refresh/columns-visibility), a table body (skeleton-loading rows, an `Empty` state, or an `Alert`-based error state that replaces the body and hides pagination), and a windowed-pagination footer (`getPageWindow()` always keeps page 1/last visible plus siblings around the current page). It takes no dependency on any feature or data-fetching library — fully controlled via props (`data`, `columns`, `isLoading`, `error`, callbacks) — so any future list page (e.g. a Roles page) can reuse it as-is.
- **Controlled form state alongside `useActionState`, for Server Action forms that can fail** (new): `CreateUserDialog` keeps its own `useState<FormValues>` in parallel with `useActionState(createUserAction, {})`. This is deliberate, not redundant — React resets *uncontrolled* form fields once a Server Action settles, regardless of success or failure, which would silently wipe user input after a validation error; controlled state survives that reset.
- **Force-remount via a bumped `key` to reset `useActionState`** (new): `useActionState` has no imperative "clear this error/state" API, so `UsersDataTable` bumps a `createDialogKey` counter on every dialog open and passes it as `CreateUserDialog`'s React `key`, forcing a fresh component instance (fresh action state, fresh controlled form state) each time the dialog is opened.
- **Toast notifications via a themed `sonner` wrapper** (new): `components/toast/` never exposes `sonner`'s `toast` directly — call sites use `notifySuccess`/`notifyError` (`notify.ts`), and the visual theme (`saturatedToastOptions`, `withToastProgress()`) is centralized in `toast-theme.ts` so every toast in the app looks consistent without each call site repeating class names.
- **Backend error messages surfaced through the shared `send()` wrapper** (new): `lib/server/http.ts`'s `extractErrorMessage()` centralizes turning a non-2xx response body into a human-readable string (envelope message → validation-errors map → `ProblemDetails.title` → generic fallback), so every `features/*/api/*.ts` call gets real error text without each call site parsing the body itself.

## Shared Kernel / Common Building Blocks Used

- `components/ui/*` — the app's own primitive layer.
- `components/shared/data-table/` (new) — generic list-table building block (toolbar, pagination, loading/empty/error states); not yet consumed outside `features/users`, but designed with no `users`-specific knowledge, so it's cataloged here rather than under `features/users`.
- `components/toast/` (new) — toast notification wrapper around `sonner`; mounted once at the root layout, called from feature code via `notifySuccess`/`notifyError`.
- `lib/shared/utils.ts` (`cn`), `lib/shared/dedupe-claims.ts`, `lib/shared/user-display.ts` — cross-cutting helpers consumed by 2+ features or by layout chrome (moved out of a flat `lib/utils.ts` into `lib/shared/` alongside these new helpers).
- `lib/server/*` — the server-only building blocks (`http.ts`, `call-guard.ts`, `config.ts`, `session.ts`, `session-cookie.ts`, `authorization.ts`) every feature's `api/` layer is built on. `authorization.ts` (`hasPermission`/`hasAnyPermission`/`hasAllPermissions`, plus a `SUPER_ADMIN_USERNAMES` bypass list) predates this sync but only gained a real consumer now (`features/users/components/users-page.tsx`).
- `constants/permissions.ts` (new) — `IDENTITY_PERMISSIONS`, the client-side mirror of the backend's `IdentityPermissions` string constants; currently only `Users.*` is populated.
- `hooks/*`, `components/theme/*` — cross-cutting building blocks consumed by layout components.
- No package or code is shared with another client app — `clients/` still contains only `admin`.

## Module/Route Boundaries

Two route groups/areas now exist:
- `(dashboard)` — wraps `/`, `/user-profile`, and now `/identity/users` with `resolveSession()` + `AppShell` (top bar, sidebar).
- `login` (ungrouped) — `/login`, rendered under the root layout only, no `AppShell`/session resolution.

`constants/nav-items.ts` still declares `/identity`, `/identity/users`, `/identity/roles`, `/settings`. `/identity/users` now has a real route and page (`app/(dashboard)/identity/users/page.tsx`, gated on the `Users.View` permission); `/identity/roles` and `/settings` still have no corresponding routes — `features/roles` remains API-only scaffolding with no page.

Feature isolation is enforced by convention (barrel-only cross-feature imports), verified in the places it's exercised so far: `features/auth/api/login-action.ts` and `features/users/components/users-page.tsx`/`api/create-user-action.ts` all import from `@/features/user-profile`'s barrel, not its internals.

## Known Architectural Risks / Debt

| Finding | Severity | Notes |
|---|---|---|
| Session cookie stores the access/refresh token and claims as **plaintext JSON** | Medium | `login-action.ts` sets the cookie `httpOnly` but unencrypted, with its own comment: "Plaintext for now — encryption is a separate follow-up step." A `TOKEN_ENCRYPTION_KEY` env var and `getTokenEncryptionKey()` function already exist (`lib/server/config.ts`, `.env.example`) but are **not called anywhere** — prep work for a follow-up that hasn't landed. Worth tracking as real risk now that auth is live, not hypothetical. |
| No token refresh flow wired up | Low–Medium | `features/auth/api/refresh-token.ts` (`refreshToken()`) exists and is exported from the feature barrel but has no caller. The session cookie's `maxAge` is tied to the access token's `expiresIn`, so once it expires the cookie is simply gone and `proxy.ts` redirects to `/login` — there's no silent renewal. |
| Nav items reference routes with no `page.tsx` (`/identity/roles`, `/settings`) | Low | `/identity/users` gained a real page this sync (see Module/Route Boundaries). `/identity/roles` and `/settings` still 404 if followed — `features/roles` has no UI consumer yet. |
| `features/roles` is fully API-only with zero UI consumers | Low (by design at this stage) | 5 endpoint files, none imported outside the feature. `features/users` moved out of this state this sync (now has `UsersPage`/`UsersDataTable`/`CreateUserDialog`); `features/roles` has not — worth revisiting once role management UI is scoped. |
| Dashboard still renders hardcoded mock data (`features/dashboard/api/sample-data.ts`) | Low (by design) | Unchanged this sync — the dashboard wasn't part of this batch of work either. |
| `prettier` + `prettier-plugin-tailwindcss` installed but no config file/`format` script | Low | Unchanged — still `unknown` whether formatting is enforced anywhere. |
| No automated test suite | Low (by design at this stage) | Unchanged — no `*.test.*`/`*.spec.*` files, no test runner in `package.json`. Now more notable given real auth/session logic plus a create-user flow exist to test. |
| `components/ui/button.tsx` hand-modified beyond shadcn CLI output (`loading` prop, `cursor-pointer`) | Low | Unchanged — re-running the shadcn CLI would silently drop these customizations unless done carefully. |
| `DataTable`'s `onExport` prop has no caller yet | Low | `components/shared/data-table/data-table-toolbar.tsx` already renders an Export button when `onExport` is passed, but no current feature (including `UsersDataTable`) passes one — dead capability until a consumer needs it. |

## Notes

<!-- manual: content below this line is human-authored and must be preserved verbatim during sync -->

---
_Generated: 2026-08-02 — scope: client app "admin" — see .claude/CLAUDE.md for update rules._
