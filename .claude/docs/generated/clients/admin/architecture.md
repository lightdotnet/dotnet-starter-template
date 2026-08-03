# Architecture: admin

## Layering

Observed folder organization (verified via directory listing) — everything now lives under `clients/admin/src/` (previously at `clients/admin/` root):

```text
src/
  app/                      routing only — layout.tsx, globals.css, login/page.tsx,
                             (dashboard)/{layout.tsx,page.tsx,user-profile/page.tsx,identity/{users,roles}/page.tsx}
  features/
    auth/                   api/{login,refresh-token,login-action}.ts, components/{login-page,login-form}.tsx, index.ts
    user-profile/           api/{get-current-user,list-sessions,revoke-session,resolve-session}.ts, components/user-profile-page.tsx, types/user-session.ts, index.ts
    dashboard/              api/sample-data.ts (mock, not a backend call), components/{stat-card,users-table,dashboard-page}.tsx, index.ts
    users/                  api/*.ts (13 endpoint files — the original 8 plus create-user-action, update-user-action,
                             force-password-action, delete-user-action, get-user-detail-action, all "use server"),
                             components/{users-page,users-data-table,create-user-dialog,edit-user-dialog,delete-user-dialog}.tsx,
                             constants/permissions.ts (`USERS_PERMISSIONS`, new — moved out of the former app-wide constants/permissions.ts), index.ts
    roles/                  api/*.ts (10 endpoint files — the original 5 plus get-permissions, get-role-detail-action,
                             create-role-action, update-role-action, delete-role-action),
                             components/{roles-page,roles-data-table,create-role-dialog,edit-role-dialog,delete-role-dialog}.tsx (new — first UI this feature has had),
                             constants/permissions.ts (`ROLES_PERMISSIONS`, new), types/{role,permission-definition}.ts, index.ts
  components/
    ui/                     shadcn-CLI-generated primitives (24 files)
    layout/                 topbar, sidebar, sidebar-nav-item, brand, breadcrumbs, user-menu, app-shell
    theme/                  theme-provider, accent-color-provider, theme-toggle, accent-color-picker, use-has-mounted, index.ts (barrel)
    shared/
      search-box.tsx
      data-table/           types.ts, data-table-toolbar.tsx, data-table-pagination.tsx, data-table.tsx, index.ts (barrel);
                             generic reusable list-table building block, no data-fetching of its own
    toast/                  toast-theme.ts, notify.ts, toaster.tsx, index.ts (barrel); wraps the `sonner` dependency
  hooks/                    use-sidebar.tsx, use-scrolled.ts
  lib/
    server/                 config.ts, http.ts, call-guard.ts, session-cookie.ts, session.ts, authorization.ts
    shared/                 utils.ts, dedupe-claims.ts, user-display.ts
  constants/                nav-items.ts (permissions.ts is gone — relocated per-feature, see `features/{users,roles}/constants/permissions.ts`)
  types/                    api.ts, nav.ts, session.ts, token.ts, user.ts
  proxy.ts                  Next.js 16 convention file (middleware-equivalent)
```

This is a **feature-folder** layering: `app/*` pages are pure re-exports from a feature's public API; `features/<name>/` owns its own `api/`, `components/`, and (when a type has exactly one consumer) `types/`, each exposed through an `index.ts` barrel; `components/layout/*` (app chrome) and `components/theme/*` compose `components/ui/*` plus `hooks/*`/`lib/shared/*`/`constants/*`/`types/*`; `components/ui/*` remains the leaf primitive layer. `components/shared/data-table/` and `components/toast/` are cross-feature building blocks sitting at the same layer as `components/shared/search-box.tsx` — presentational/utility, composed by features but with no feature-specific knowledge baked in.

The pattern gained one addition this sync: **`features/<name>/` may also own a `constants/`** (currently `permissions.ts` in both `users` and `roles`) for constants that are conceptually part of that feature rather than app-wide — a deliberate move away from the top-level `constants/` folder for anything feature-specific. Top-level `constants/` (`nav-items.ts` only now) is reserved for genuinely cross-cutting, no-single-feature-owns-it constants.

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
src/app/(dashboard)/identity/roles/page.tsx -> features/roles (RolesPage)

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
                                        get-user-by-username,create-user,update-user,delete-user,force-password},
                                        ./constants/permissions (USERS_PERMISSIONS)
features/users/components/users-page.tsx -> features/user-profile (resolveSession), features/users/api/search-users,
                                        features/roles/api/get-all-roles (role catalog for the edit dialog, fetched only when
                                        the viewer can update users), features/users/components/users-data-table,
                                        lib/server/authorization (hasPermission), features/users/constants/permissions
                                        (USERS_PERMISSIONS — moved from the former top-level constants/permissions), components/ui/empty
features/users/components/users-data-table.tsx -> components/shared/data-table (DataTable + types), components/ui/{avatar,badge,button},
                                        components/ui/dropdown-menu, features/users/components/{create,edit,delete}-user-dialog,
                                        features/user-profile/components/user-status-badge, lib/shared/user-display,
                                        features/roles/types/role (RoleDto), types/user
features/users/components/create-user-dialog.tsx -> components/ui/{alert,button,dialog,input,label}, components/toast (notifySuccess),
                                        features/users/api/create-user-action
features/users/components/edit-user-dialog.tsx -> components/ui/{alert,button,checkbox,dialog,input,label,select,spinner,tabs},
                                        components/toast (notifySuccess), features/users/api/get-user-detail-action,
                                        features/users/api/update-user-action, features/users/api/force-password-action,
                                        features/roles/types/role (RoleDto), types/user
features/users/components/delete-user-dialog.tsx -> components/ui/{button,dialog}, components/toast, features/users/api/delete-user-action, types/user
features/users/api/create-user-action.ts -> "use server"; features/user-profile (resolveSession), features/users/api/create-user, types/user
features/users/api/{update-user-action,force-password-action,delete-user-action,get-user-detail-action}.ts
                                     -> "use server"; features/user-profile (resolveSession),
                                        features/users/api/{update-user,force-password,delete-user,get-user-by-id} respectively, types/user
features/users/api/*.ts (7 remaining files) -> lib/server/http, lib/server/call-guard, types/{api,user}

features/roles/index.ts             -> ./components/roles-page, ./api/{get-all-roles,get-role-by-id,get-permissions,
                                        create-role,update-role,delete-role}, ./constants/permissions (ROLES_PERMISSIONS),
                                        ./types/{role,permission-definition} — barrel; now imported by
                                        app/(dashboard)/identity/roles/page.tsx, first UI consumer
features/roles/components/roles-page.tsx -> features/user-profile (resolveSession), features/roles/api/{get-all-roles,get-permissions},
                                        features/roles/components/roles-data-table, lib/server/authorization (hasPermission),
                                        features/roles/constants/permissions (ROLES_PERMISSIONS), components/ui/empty
features/roles/components/roles-data-table.tsx -> components/shared/data-table (DataTable + types), components/ui/{button,dropdown-menu},
                                        features/roles/components/{create,edit,delete}-role-dialog,
                                        features/roles/types/{role,permission-definition}
features/roles/components/create-role-dialog.tsx -> components/ui/{alert,button,dialog,input,label}, components/toast (notifySuccess),
                                        features/roles/api/create-role-action
features/roles/components/edit-role-dialog.tsx -> components/ui/{alert,button,checkbox,dialog,input,label,spinner},
                                        components/toast (notifySuccess), features/roles/api/get-role-detail-action,
                                        features/roles/api/update-role-action, features/roles/types/{role,permission-definition}
features/roles/components/delete-role-dialog.tsx -> components/ui/{button,dialog}, components/toast, features/roles/api/delete-role-action, features/roles/types/role
features/roles/api/{create-role-action,update-role-action,delete-role-action,get-role-detail-action}.ts
                                     -> "use server"; features/user-profile (resolveSession),
                                        features/roles/api/{create-role,update-role,delete-role,get-role-by-id} respectively
                                        (update-role-action additionally re-reads get-role-by-id first, to preserve any
                                        non-"permission"-typed claims before writing back the submitted permission set)
features/roles/api/get-permissions.ts -> lib/server/http, lib/server/call-guard, features/roles/types/permission-definition
features/roles/api/get-all-roles.ts -> lib/server/http, lib/server/call-guard, features/roles/types/role
                                        (fixed this sync: was guardRawCall assuming a bare array; the endpoint actually
                                        wraps its response in the same envelope every other endpoint uses)
features/roles/api/{get-role-by-id,create-role,update-role,delete-role}.ts -> lib/server/http, lib/server/call-guard, features/roles/types/role

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

Direction is still one-way: `components/ui/*` never imports from `components/layout/*`, `components/theme/*`, or `features/*`. Cross-feature imports go through a feature's `index.ts` barrel, not its internals — e.g. `features/auth/api/login-action.ts` and `features/users/components/users-page.tsx`/`api/create-user-action.ts` all import `resolveSession`/`getCurrentUser` from `@/features/user-profile` (the barrel), not from its internals directly; `features/users/components/users-page.tsx` similarly imports `getAllRoles` from `@/features/roles`'s barrel, not `@/features/roles/api/get-all-roles` directly. `features/dashboard` is still not imported by any other feature. `features/roles` gained its first cross-feature importer this sync (`features/users/components/users-page.tsx`, for the role catalog) in addition to its own route. `features/users` and `features/roles` are each imported by their own `app/**/page.tsx` (routing, not another feature). `components/shared/data-table/*` and `components/toast/*` sit below `features/*` in the same leaf-adjacent tier as `components/ui/*` — they're imported by feature code but import nothing from `features/*` themselves.

## Key Design Patterns

- **Feature-folder + barrel-export convention**: each `features/<name>/` owns `api/` (one file per backend endpoint), `components/`, optionally `types/` (only for types with exactly one consumer — `features/roles/types/{role,permission-definition}.ts`, `features/user-profile/types/user-session.ts`) and, new this sync, optionally `constants/` (a feature-owned permission-string file, e.g. `features/users/constants/permissions.ts`), and an `index.ts` that is the only sanctioned import surface for other features or `app/*`.
- **Fetch full detail on dialog open, when the list endpoint's DTO is incomplete** (new): both `edit-user-dialog.tsx` and `edit-role-dialog.tsx` call a dedicated `get-*-detail-action.ts` in a `useEffect` on mount rather than trusting the row data they were opened with. This exists because the backend's list-returning service methods (`UserService.SearchAsync`/`GetAllAsync`, `RoleService.GetAllAsync` — their shared `DataMapper.cs` projection) never populate `Roles`/`Claims`; only the single-record fetch (`GetByIdAsync`) does. Relying on the row data silently produced an empty roles/claims checklist that, on save, would have wiped out anything the record actually had — worth remembering before building another list-backed feature against this backend.
- **Routing files are pure re-exports**: every `app/**/page.tsx` is a one-line `export { X as default } from "@/features/<name>";` — no logic lives in `app/`.
- **Server-only API layer, one function per endpoint file**: `lib/server/http.ts` (`requestJson`/`requestVoid`) is the single fetch wrapper; every `features/*/api/*.ts` file wraps exactly one backend call and returns a normalized result via `lib/server/call-guard.ts`'s `guardCall`/`guardResponseCall`/`guardRawCall` (chosen based on whether the backend endpoint returns a `Result<T>` envelope, a bare `ApiResponse`, or a raw value/array).
- **Cookie session + server-side session resolution, no client cache**: `lib/server/session.ts` reads the `admin_session` cookie; `features/user-profile/api/resolve-session.ts` composes that with a live `getCurrentUser()` call. Kept in two layers deliberately — `getSession()` has no feature dependency, while `resolveSession()` needs `user-profile`'s API, so it lives in the feature instead of `lib/server`.
- **Edge-safe cookie name constant**: `lib/server/session-cookie.ts` exports only the `SESSION_COOKIE_NAME` string with no `next/headers` import, specifically so `proxy.ts` (which runs on the edge runtime) can import it without pulling in the Node-only cookie-reading API.
- **Context-provider-per-concern for client state**: `SidebarProvider`/`useSidebar`, `AccentColorProvider`/`useAccentColor`, and `next-themes`' provider (wrapped in `components/theme/theme-provider.tsx`) each own one slice of persisted UI state via a throwing custom hook — unchanged pattern, relocated files.
- **Owned, CLI-generated UI primitives**: `components/ui/*` (24 files, style `"radix-nova"`) still follows the `data-slot="<name>"` + `cva()` convention. `button.tsx` retains its hand-modification beyond CLI output: a `loading` prop (renders `Spinner`, sets `aria-busy`/`disabled`) plus a `cursor-pointer` utility baked into `buttonVariants`.
- **Single-CSS-variable theming** and **runtime accent swap via DOM attribute + localStorage**: unchanged from before — `--primary` drives themed surfaces, `AccentColorProvider` sets `data-accent` on `<html>`.
- **Hydration-safe browser-state restoration**: unchanged pattern (`hydrated` flag + `useEffect`, `eslint-disable react-hooks/set-state-in-effect`) in `SidebarProvider` and `AccentColorProvider`; `components/theme/use-has-mounted.ts` (`useSyncExternalStore`) still guards `ThemeToggle`.
- **Mobile drawer closes on route change via render-time state adjustment**: unchanged, still in `hooks/use-sidebar.tsx`.
- **Generic, presentational `DataTable<TData>` building block**: `components/shared/data-table/` composes a toolbar (actions + debounced search + export/refresh/columns-visibility), a table body (skeleton-loading rows, an `Empty` state, or an `Alert`-based error state that replaces the body and hides pagination), and a windowed-pagination footer (`getPageWindow()` always keeps page 1/last visible plus siblings around the current page). It takes no dependency on any feature or data-fetching library — fully controlled via props (`data`, `columns`, `isLoading`, `error`, callbacks); both `UsersDataTable` (server-driven search via URL params) and `RolesDataTable` (local client-side filtering — there's no backend search endpoint for roles) reuse it as-is with different search wiring. **Fixed this sync**: `isLoading` now takes priority over a stale `error` — previously, once an error had been shown, a subsequent in-flight refetch (e.g. clicking Refresh) kept showing that stale error instead of the table's own skeleton-row loading state.
- **Controlled form state alongside `useActionState`, for Server Action forms that can fail**: dialogs bound to a mutation Server Action keep their own `useState<FormValues>` in parallel with `useActionState(...)`. This is deliberate, not redundant — React resets *uncontrolled* form fields once a Server Action settles, regardless of success or failure, which would silently wipe user input after a validation error; controlled state survives that reset.
- **Force-remount via a bumped `key` to reset `useActionState`**: `useActionState` has no imperative "clear this error/state" API, so each `*DataTable` bumps a per-dialog key counter on every open (`createDialogKey`, `editDialogKey`, ...) and passes it as that dialog's React `key`, forcing a fresh component instance (fresh action state, fresh controlled form state, and — for the edit dialogs — a fresh detail-fetch) each time it's opened.
- **Toast notifications via a themed `sonner` wrapper**: `components/toast/` never exposes `sonner`'s `toast` directly — call sites use `notifySuccess`/`notifyError` (`notify.ts`), and the visual theme (`saturatedToastOptions`, `withToastProgress()`) is centralized in `toast-theme.ts` so every toast in the app looks consistent without each call site repeating class names.
- **Backend error messages surfaced through the shared `send()` wrapper**: `lib/server/http.ts`'s `extractErrorMessage()` centralizes turning a non-2xx response body into a human-readable string (envelope message → validation-errors map → `ProblemDetails.title` → generic fallback), so every `features/*/api/*.ts` call gets real error text without each call site parsing the body itself.

## Shared Kernel / Common Building Blocks Used

- `components/ui/*` — the app's own primitive layer.
- `components/shared/data-table/` — generic list-table building block (toolbar, pagination, loading/empty/error states); consumed by both `features/users` and `features/roles` (with different search strategies — see Key Design Patterns), designed with no feature-specific knowledge baked in.
- `components/toast/` — toast notification wrapper around `sonner`; mounted once at the root layout, called from feature code via `notifySuccess`/`notifyError`.
- `lib/shared/utils.ts` (`cn`), `lib/shared/dedupe-claims.ts`, `lib/shared/user-display.ts` — cross-cutting helpers consumed by 2+ features or by layout chrome.
- `lib/server/*` — the server-only building blocks (`http.ts`, `call-guard.ts`, `config.ts`, `session.ts`, `session-cookie.ts`, `authorization.ts`) every feature's `api/` layer is built on. `authorization.ts` (`hasPermission`/`hasAnyPermission`/`hasAllPermissions`, plus a `SUPER_ADMIN_USERNAMES` bypass list) is now consumed by both `features/users/components/users-page.tsx` and `features/roles/components/roles-page.tsx`.
- Permission-string constants are **no longer a shared kernel piece** — the former top-level `constants/permissions.ts` (`IDENTITY_PERMISSIONS`) is gone this sync, replaced by per-feature `features/{users,roles}/constants/permissions.ts` (`USERS_PERMISSIONS`, `ROLES_PERMISSIONS`). Each still mirrors the backend's `IdentityPermissions` string constants (now with corrected values — see Coding Conventions), but ownership moved into the feature that uses it.
- `hooks/*`, `components/theme/*` — cross-cutting building blocks consumed by layout components.
- No package or code is shared with another client app — `clients/` still contains only `admin`.

## Module/Route Boundaries

Two route groups/areas exist:
- `(dashboard)` — wraps `/`, `/user-profile`, `/identity/users`, and now `/identity/roles` with `resolveSession()` + `AppShell` (top bar, sidebar).
- `login` (ungrouped) — `/login`, rendered under the root layout only, no `AppShell`/session resolution.

`constants/nav-items.ts` declares `/identity`, `/identity/users`, `/identity/roles`, `/settings`. `/identity/users` and `/identity/roles` both now have real routes and pages (`app/(dashboard)/identity/{users,roles}/page.tsx`, gated on `USERS_PERMISSIONS.View`/`ROLES_PERMISSIONS.View` respectively); `/settings` still has no corresponding route.

Feature isolation is enforced by convention (barrel-only cross-feature imports), verified in the places it's exercised so far: `features/auth/api/login-action.ts` and `features/users/components/users-page.tsx`/`api/create-user-action.ts` all import from `@/features/user-profile`'s barrel, not its internals; `features/users/components/users-page.tsx` likewise imports `getAllRoles` from `@/features/roles`'s barrel, not its internals.

## Known Architectural Risks / Debt

| Finding | Severity | Notes |
|---|---|---|
| Session cookie stores the access/refresh token and claims as **plaintext JSON** | Medium | `login-action.ts` sets the cookie `httpOnly` but unencrypted, with its own comment: "Plaintext for now — encryption is a separate follow-up step." A `TOKEN_ENCRYPTION_KEY` env var and `getTokenEncryptionKey()` function already exist (`lib/server/config.ts`, `.env.example`) but are **not called anywhere** — prep work for a follow-up that hasn't landed. Worth tracking as real risk now that auth is live, not hypothetical. |
| No token refresh flow wired up | Low–Medium | `features/auth/api/refresh-token.ts` (`refreshToken()`) exists and is exported from the feature barrel but has no caller. The session cookie's `maxAge` is tied to the access token's `expiresIn`, so once it expires the cookie is simply gone and `proxy.ts` redirects to `/login` — there's no silent renewal. |
| Nav item references a route with no `page.tsx` (`/settings`) | Low | `/identity/users` and `/identity/roles` both now have real pages. `/settings` still 404s if followed. |
| Backend list endpoints never populate `Roles`/`Claims` on the DTO | Low (worked around, but worth remembering) | `UserService.SearchAsync`/`GetAllAsync` and `RoleService.GetAllAsync` all use a `DataMapper.cs` projection that only maps scalar fields; only the single-record fetch (`GetByIdAsync`) populates `Roles`/`Claims`. Both edit dialogs correctly re-fetch full detail on open to work around this (see Key Design Patterns), but any future feature reading a list endpoint for role/claim data would silently get empty arrays if it forgot to do the same. |
| Dashboard still renders hardcoded mock data (`features/dashboard/api/sample-data.ts`) | Low (by design) | Unchanged this sync — the dashboard wasn't part of this batch of work either. |
| `prettier` + `prettier-plugin-tailwindcss` installed but no config file/`format` script | Low | Unchanged — still `unknown` whether formatting is enforced anywhere. |
| No automated test suite | Low (by design at this stage) | Unchanged — no `*.test.*`/`*.spec.*` files, no test runner in `package.json`. Now more notable given real auth/session logic plus full Users and Roles CRUD flows all exist untested. |
| `components/ui/button.tsx` hand-modified beyond shadcn CLI output (`loading` prop, `cursor-pointer`) | Low | Unchanged — re-running the shadcn CLI would silently drop these customizations unless done carefully. |
| `DataTable`'s `onExport` prop has no caller yet | Low | `components/shared/data-table/data-table-toolbar.tsx` already renders an Export button when `onExport` is passed, but no current feature (including `UsersDataTable`/`RolesDataTable`) passes one — dead capability until a consumer needs it. |

## Notes

<!-- manual: content below this line is human-authored and must be preserved verbatim during sync -->

---
_Generated: 2026-08-03 — scope: client app "admin" — see .claude/CLAUDE.md for update rules._
