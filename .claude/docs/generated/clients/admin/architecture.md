# Architecture: admin

## Layering

Observed folder organization (verified via directory listing) — everything now lives under `clients/admin/src/` (previously at `clients/admin/` root):

```text
src/
  app/                      routing only — layout.tsx, globals.css, login/page.tsx,
                             (dashboard)/{layout.tsx,page.tsx,user-profile/page.tsx,identity/{users,roles}/page.tsx}
  features/
    auth/                   api/{login,refresh-token,login-action,logout-action}.ts, components/{login-page,login-form}.tsx, index.ts
                             (logout-action.ts new — barrel does not export it, see Dependency Direction)
    user-profile/           api/{get-current-user,list-sessions,revoke-session,resolve-session}.ts, components/user-profile-page.tsx, types/user-session.ts, index.ts
    dashboard/              api/sample-data.ts (mock, not a backend call), components/{stat-card,users-table,dashboard-page}.tsx,
                             constants/nav-item.ts (`DASHBOARD_NAV_ITEM`, new), index.ts
    users/                  api/*.ts (13 endpoint files — the original 8 plus create-user-action, update-user-action,
                             force-password-action, delete-user-action, get-user-detail-action, all "use server"),
                             components/{users-page,users-data-table,create-user-dialog,edit-user-dialog,delete-user-dialog}.tsx,
                             constants/{permissions,nav-item}.ts (`USERS_PERMISSIONS`, `USERS_NAV_ITEM` — nav-item.ts new), index.ts
    roles/                  api/*.ts (10 endpoint files — the original 5 plus get-permissions, get-role-detail-action,
                             create-role-action, update-role-action, delete-role-action),
                             components/{roles-page,roles-data-table,create-role-dialog,edit-role-dialog,delete-role-dialog}.tsx,
                             constants/{permissions,nav-item}.ts (`ROLES_PERMISSIONS`, `ROLES_NAV_ITEM` — nav-item.ts new), types/{role,permission-definition}.ts, index.ts
  components/
    ui/                     shadcn-CLI-generated primitives (24 files)
    layout/                 topbar, sidebar, sidebar-nav-item, brand, breadcrumbs, user-menu, app-shell
    theme/                  theme-provider, accent-color-provider, theme-toggle, accent-color-picker, use-has-mounted, index.ts (barrel)
    shared/
      search-box.tsx
      data-table/           types.ts, data-table-toolbar.tsx, data-table-pagination.tsx, data-table.tsx, index.ts (barrel);
                             generic reusable list-table building block, no data-fetching of its own; `types.ts`/`data-table.tsx`
                             gained optional per-column client-side sorting (`sortable`/`sortValue`)
      object-viewer/         (new, additive — not yet used by any page) utils.ts, object-viewer-layout-context.tsx,
                             column-resize-handle.tsx, object-viewer-row.tsx, object-viewer.tsx, index.ts;
                             built on `components/ui/table` + `components/ui/input`, no `features/*` dependency
    toast/                  toast-theme.ts, notify.ts, toaster.tsx, index.ts (barrel); wraps the `sonner` dependency
  hooks/                    use-sidebar.tsx, use-scrolled.ts
  lib/
    server/                 config.ts, http.ts, call-guard.ts, session-cookie.ts, session.ts, authorization.ts,
                             token-cipher.ts, jwt.ts, build-session-claims.ts, refresh-session.ts, parse-session.ts
                             (the last 5 are new — session encryption, JWT claim decoding, and the token-refresh flow)
    shared/                 utils.ts, dedupe-claims.ts, user-display.ts, menu.ts, authorization.ts
                             (menu.ts and authorization.ts are new — see Key Design Patterns)
  constants/                nav-items.ts (permissions.ts is still gone — relocated per-feature; nav item definitions
                             are now also per-feature, see `features/{dashboard,users,roles}/constants/nav-item.ts`)
  types/                    api.ts, nav.ts, session.ts, token.ts, user.ts
  proxy.ts                  Next.js 16 "proxy" convention file (successor to middleware.ts); calls into
                             `lib/server/{parse-session,refresh-session,token-cipher,session-cookie}.ts` — see
                             Known Architectural Risks / Debt for an open question about which runtime this requires
```

This is a **feature-folder** layering: `app/*` pages are pure re-exports from a feature's public API; `features/<name>/` owns its own `api/`, `components/`, and (when a type has exactly one consumer) `types/`, each exposed through an `index.ts` barrel; `components/layout/*` (app chrome) and `components/theme/*` compose `components/ui/*` plus `hooks/*`/`lib/shared/*`/`constants/*`/`types/*`; `components/ui/*` remains the leaf primitive layer. `components/shared/data-table/` and `components/toast/` are cross-feature building blocks sitting at the same layer as `components/shared/search-box.tsx` — presentational/utility, composed by features but with no feature-specific knowledge baked in.

The pattern extended this sync: **`features/<name>/` may also own a `constants/`** — `permissions.ts` (`users`, `roles`) plus, new this sync, a `nav-item.ts` per nav-bearing feature (`dashboard`, `users`, `roles`), each exporting one `NavItem` constant for that feature's own entry — a deliberate move away from the top-level `constants/` folder for anything feature-specific, now extended from permission strings to nav metadata too. Top-level `constants/nav-items.ts` is now purely an *assembly* file: it imports each feature's `NavItem` and composes the final `NAV_ITEMS` tree, only declaring the "Identity" group node and the "Settings" leaf directly since neither is owned by a single feature (Identity spans two features; Settings has no feature/page at all yet). See "Each nav-bearing feature owns its nav metadata" under Key Design Patterns for the barrel-bypass this required.

## Dependency Direction

Verified via actual `import` statements:

```text
src/proxy.ts                       -> features/user-profile/api/get-current-user (direct file, not the barrel — the
                                       barrel also re-exports UserProfilePage/other feature surface not needed here),
                                       lib/server/token-cipher (encrypt), lib/server/build-session-claims,
                                       lib/server/refresh-session, lib/server/parse-session,
                                       lib/server/session-cookie (SESSION_COOKIE_NAME, REFRESH_LEAD_MS), types/session
                                       — see Known Architectural Risks / Debt: uses Node's `crypto` (via token-cipher.ts)
                                       directly with no explicit `export const runtime` pin
src/app/layout.tsx                 -> components/theme (ThemeProvider, AccentColorProvider), components/toast (AppToaster),
                                       components/ui/tooltip
src/app/login/page.tsx             -> features/auth (LoginPage)
src/app/(dashboard)/layout.tsx     -> components/layout/app-shell, features/user-profile (resolveSession)
src/app/(dashboard)/page.tsx       -> features/dashboard (DashboardPage)
src/app/(dashboard)/user-profile/page.tsx -> features/user-profile (UserProfilePage)
src/app/(dashboard)/identity/users/page.tsx -> features/users (UsersPage)
src/app/(dashboard)/identity/roles/page.tsx -> features/roles (RolesPage)

components/layout/app-shell.tsx    -> hooks/use-sidebar, components/layout/{sidebar,topbar}, types/session (ProfileData)
                                       (takes `{ permissions, userName, user, children }`, passes `permissions`/`userName`
                                       through to Sidebar and `user` through to TopBar)
components/layout/topbar.tsx       -> lib/shared/utils, hooks/{use-scrolled,use-sidebar}, components/ui/{button,badge},
                                       components/layout/{breadcrumbs,brand,user-menu}, components/shared/search-box,
                                       components/theme (ThemeToggle, AccentColorPicker)
components/layout/sidebar.tsx      -> lib/shared/utils, hooks/use-sidebar, components/layout/sidebar-nav-item,
                                       constants/nav-items (NAV_ITEMS, direct import — see Key Design Patterns for why
                                       this is a deliberate barrel-bypass exception), lib/shared/menu (buildVisibleMenu),
                                       lib/shared/authorization (hasPermission), components/ui/sheet
                                       (takes `{ permissions, userName }`, computes the visible menu via `useMemo`)
components/layout/sidebar-nav-item.tsx -> lib/shared/utils, hooks/use-sidebar, types/nav
components/layout/breadcrumbs.tsx  -> components/ui/breadcrumb, constants/nav-items, types/nav
components/layout/user-menu.tsx    -> components/ui/{avatar,button,dropdown-menu}, lib/shared/user-display,
                                       features/auth/api/logout-action (direct file, not the barrel — the barrel also
                                       re-exports LoginPage, an async Server Component; see note below), types/session
                                       (ProfileData) — takes a `user: ProfileData | null` prop; "Log out" calls
                                       `logoutAction(currentPath)` where `currentPath` is `usePathname()` + `useSearchParams()`

components/theme/theme-toggle.tsx        -> next-themes, ./use-has-mounted, components/ui/{button,dropdown-menu}
components/theme/accent-color-picker.tsx -> components/ui/{button,dropdown-menu}, ./accent-color-provider

features/auth/index.ts              -> ./components/login-page, ./api/login, ./api/refresh-token
                                        (does NOT export logoutAction — see components/layout/user-menu.tsx above)
features/auth/components/login-page.tsx  -> components/ui/card, ./login-form (async; reads `searchParams` for `redirect`)
features/auth/components/login-form.tsx  -> components/ui/{button,input,label,alert}, features/auth/api/login-action
                                        (takes `{ redirect? }`, renders it as a hidden form field when present)
features/auth/api/login-action.ts   -> "use server"; features/auth/api/login, features/user-profile (getCurrentUser —
                                        via the barrel, unchanged), lib/server/jwt (extractPermissions/extractRoles),
                                        lib/server/build-session-claims, lib/server/token-cipher (encrypt),
                                        lib/server/session-cookie (SESSION_COOKIE_NAME, SESSION_TTL_MS, buildSessionCookieOptions),
                                        types/session — reads a `redirect` form field and redirects there (guarded) on success
features/auth/api/logout-action.ts  -> "use server"; lib/server/session-cookie (SESSION_COOKIE_NAME), next/navigation
                                        (redirect) — new; deletes the cookie, redirects to `/login?redirect=<path>` (guarded)
features/auth/api/login.ts          -> lib/server/http, lib/server/call-guard, types/{api,token}
features/auth/api/refresh-token.ts  -> lib/server/http, lib/server/call-guard, types/{api,token}
                                        (now has a real caller: lib/server/refresh-session.ts)

features/user-profile/index.ts      -> ./components/user-profile-page, ./api/{resolve-session,get-current-user,list-sessions,revoke-session}, ./types/user-session
features/user-profile/components/user-profile-page.tsx -> components/ui/{card,badge,separator,avatar,alert}, qrcode,
                                        features/user-profile/api/resolve-session, lib/shared/{dedupe-claims,user-display}
features/user-profile/api/resolve-session.ts -> lib/server/session (getSession), types/session
                                        (now a thin passthrough to getSession() — no longer calls getCurrentUser itself;
                                        proxy.ts keeps the cookie's profile/claims fresh instead, see Key Design Patterns)
features/user-profile/api/get-current-user.ts -> lib/server/http, lib/server/call-guard, types/{api,user}
features/user-profile/api/{list-sessions,revoke-session}.ts -> lib/server/http, lib/server/call-guard, ./types/user-session (list)

features/dashboard/index.ts         -> ./components/dashboard-page, ./constants/nav-item (DASHBOARD_NAV_ITEM)
features/dashboard/components/dashboard-page.tsx -> components/ui/card, ./stat-card, ./users-table, features/dashboard/api/sample-data
features/dashboard/components/{stat-card,users-table}.tsx -> components/ui/*, lib/shared/utils, features/dashboard/api/sample-data (mock data, no backend call)
features/dashboard/constants/nav-item.ts -> lucide-react (LayoutDashboard), types/nav (new; label "Dashboard", href "/", no permission)

features/users/index.ts             -> ./components/users-page, ./api/{search-users,get-all-users,get-user-by-id,
                                        get-user-by-username,create-user,update-user,delete-user,force-password},
                                        ./constants/permissions (USERS_PERMISSIONS), ./constants/nav-item (USERS_NAV_ITEM)
features/users/constants/nav-item.ts -> lucide-react (Users), ./permissions (USERS_PERMISSIONS), types/nav
                                        (new; label "Users", href "/identity/users", permission USERS_PERMISSIONS.View)
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
                                        ./constants/nav-item (ROLES_NAV_ITEM), ./types/{role,permission-definition} —
                                        barrel; imported by app/(dashboard)/identity/roles/page.tsx and
                                        features/users/components/users-page.tsx
features/roles/constants/nav-item.ts -> lucide-react (KeyRound), ./permissions (ROLES_PERMISSIONS), types/nav
                                        (new; label "Roles", href "/identity/roles", permission ROLES_PERMISSIONS.View)
features/roles/components/roles-page.tsx -> features/user-profile (resolveSession), features/roles/api/{get-all-roles,get-permissions},
                                        features/roles/components/roles-data-table, lib/server/authorization (hasPermission),
                                        features/roles/constants/permissions (ROLES_PERMISSIONS), components/ui/empty
features/roles/components/roles-data-table.tsx -> components/shared/data-table (DataTable + types), components/ui/{button,dropdown-menu},
                                        features/roles/components/{create,edit,delete}-role-dialog,
                                        features/roles/types/{role,permission-definition} — its `name`/`description`
                                        columns set `sortable: true`/`sortValue` on the shared DataTable (new; the whole
                                        role list is fetched upfront, so client-side sort is meaningful here, unlike
                                        UsersDataTable which paginates server-side)
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

lib/server/session.ts               -> next/headers (cookies), lib/server/parse-session, lib/server/session-cookie, types/session
lib/server/parse-session.ts         -> lib/server/token-cipher (decrypt), types/session (new)
lib/server/refresh-session.ts       -> features/auth/api/refresh-token (direct file, not the barrel), lib/server/jwt,
                                        types/session (new — the token-refresh flow; called from src/proxy.ts)
lib/server/token-cipher.ts          -> node:crypto, lib/server/config (getTokenEncryptionKey) — AES-256-GCM (new)
lib/server/jwt.ts                   -> types/user (ClaimDto) — decodes a JWT payload without verifying its signature;
                                        no other lib/server/* dependency (new)
lib/server/build-session-claims.ts  -> lib/server/jwt (extractAllClaims), lib/shared/dedupe-claims, types/user (new)
lib/server/http.ts                  -> lib/server/config (getApiBaseUrl)
lib/server/authorization.ts         -> lib/shared/authorization (delegates to it), types/session (SessionData) —
                                        now a thin wrapper preserving the original `(session, userName, permission)`
                                        call-site signature for existing server callers (was previously self-contained)
lib/shared/authorization.ts         -> no internal dependency (SUPER_ADMIN_USERNAMES, isSuperAdminUser,
                                        hasPermission/hasAnyPermission/hasAllPermissions; new — safe for both server
                                        and client, takes `permissions: string[]`/`userName` directly rather than a
                                        `SessionData`-shaped object, which is what makes it usable from Sidebar)
lib/shared/menu.ts                  -> types/nav (NavItem) — new; pure recursive `buildVisibleMenu(items, can)`
components/ui/*                     -> lib/shared/utils, radix-ui, class-variance-authority, lucide-react
                                        (button.tsx additionally -> components/ui/spinner)
```

Direction is still one-way: `components/ui/*` never imports from `components/layout/*`, `components/theme/*`, or `features/*`. Cross-feature imports go through a feature's `index.ts` barrel, not its internals — e.g. `features/auth/api/login-action.ts` and `features/users/components/users-page.tsx`/`api/create-user-action.ts` all import `resolveSession`/`getCurrentUser` from `@/features/user-profile` (the barrel), not from its internals directly; `features/users/components/users-page.tsx` similarly imports `getAllRoles` from `@/features/roles`'s barrel, not `@/features/roles/api/get-all-roles` directly. `features/dashboard` is still not imported by any other feature. `features/roles` gained a cross-feature importer in a prior sync (`features/users/components/users-page.tsx`, for the role catalog) in addition to its own route. `features/users` and `features/roles` are each imported by their own `app/**/page.tsx` (routing, not another feature). `components/shared/data-table/*`, `components/shared/object-viewer/*`, and `components/toast/*` sit below `features/*` in the same leaf-adjacent tier as `components/ui/*` — they're imported by feature code but import nothing from `features/*` themselves (`object-viewer/*` currently has no importer at all — purely additive).

Two deliberate, narrow exceptions to the barrel-only rule exist, both driven by the RSC client/server boundary rather than an oversight: `constants/nav-items.ts` imports `DASHBOARD_NAV_ITEM`/`USERS_NAV_ITEM`/`ROLES_NAV_ITEM` by direct file path (`@/features/dashboard/constants/nav-item`, etc.) rather than via each feature's barrel, and `components/layout/user-menu.tsx` imports `logoutAction` directly from `@/features/auth/api/logout-action` rather than from `@/features/auth`'s barrel (which, notably, does not currently re-export `logoutAction` at all). See Key Design Patterns for the reasoning.

## Key Design Patterns

- **Feature-folder + barrel-export convention**: each `features/<name>/` owns `api/` (one file per backend endpoint), `components/`, optionally `types/` (only for types with exactly one consumer — `features/roles/types/{role,permission-definition}.ts`, `features/user-profile/types/user-session.ts`) and optionally `constants/` (a feature-owned permission-string file, e.g. `features/users/constants/permissions.ts`, and now — new this sync — a `nav-item.ts` per nav-bearing feature), and an `index.ts` that is the only sanctioned import surface for other features or `app/*`. **Two narrow, deliberate exceptions** to the barrel-only rule exist (see Dependency Direction): `constants/nav-items.ts` and `components/layout/user-menu.tsx` each import one specific file directly rather than through a barrel, to avoid pulling a barrel's other, server-only exports (async Server Components, cookie-reading API functions) into a client bundle.
- **Each nav-bearing feature owns its own `NavItem` metadata** (new): `features/{dashboard,users,roles}/constants/nav-item.ts` each export one `NavItem` (label, href, icon, and — where relevant — the permission that gates it), re-exported from that feature's barrel. `constants/nav-items.ts` imports these three constants **by direct file path**, not via each feature's barrel, and assembles them into `NAV_ITEMS` alongside two nodes it still declares itself (the "Identity" group, since it spans two features, and "Settings", which has no owning feature or page at all). The direct-file-path import is intentional, not an oversight: `nav-items.ts` is imported by the client-side `Sidebar` component, and `features/users/index.ts`/`features/roles/index.ts`'s barrels also re-export server-only code (`UsersPage`/`RolesPage`, async Server Components calling `resolveSession()`, which reads cookies via `next/headers`) — importing the full barrel from client code would drag that server-only chain into the client bundle (this was tried and produced a real "next/headers only available in Server Components" build error before being fixed this way). The `nav-item.ts` files themselves are plain data (an icon reference plus strings) with no server/client-bound dependency, so importing them directly is safe. `components/layout/sidebar.tsx` then computes the permission-filtered menu client-side via `lib/shared/menu.ts`'s `buildVisibleMenu(NAV_ITEMS, can)`, where `can` is built from the new `lib/shared/authorization.ts` (`hasPermission`) using the `permissions`/`userName` props `AppShell` passes down from `resolveSession()`.
- **Fetch full detail on dialog open, when the list endpoint's DTO is incomplete** (new): both `edit-user-dialog.tsx` and `edit-role-dialog.tsx` call a dedicated `get-*-detail-action.ts` in a `useEffect` on mount rather than trusting the row data they were opened with. This exists because the backend's list-returning service methods (`UserService.SearchAsync`/`GetAllAsync`, `RoleService.GetAllAsync` — their shared `DataMapper.cs` projection) never populate `Roles`/`Claims`; only the single-record fetch (`GetByIdAsync`) does. Relying on the row data silently produced an empty roles/claims checklist that, on save, would have wiped out anything the record actually had — worth remembering before building another list-backed feature against this backend.
- **Routing files are pure re-exports**: every `app/**/page.tsx` is a one-line `export { X as default } from "@/features/<name>";` — no logic lives in `app/`.
- **Server-only API layer, one function per endpoint file**: `lib/server/http.ts` (`requestJson`/`requestVoid`) is the single fetch wrapper; every `features/*/api/*.ts` file wraps exactly one backend call and returns a normalized result via `lib/server/call-guard.ts`'s `guardCall`/`guardResponseCall`/`guardRawCall` (chosen based on whether the backend endpoint returns a `Result<T>` envelope, a bare `ApiResponse`, or a raw value/array).
- **Encrypted cookie session, refreshed proactively by `proxy.ts`, no per-request live fetch** (changed this sync): `lib/server/session.ts`'s `getSession()` reads and decrypts the `admin_session` cookie; `features/user-profile/api/resolve-session.ts` is now a thin passthrough to it, rather than composing it with its own live `getCurrentUser()` call. All the "keep this session fresh" work now lives in `src/proxy.ts` instead: on every request it decrypts/validates the cookie (`lib/server/parse-session.ts`), proactively rotates the access/refresh token when close to expiry (`lib/server/refresh-session.ts`, calling the backend's `token/token/refresh`), and refetches profile/claims on hard navigations or right after a rotation — writing the result back as a freshly-encrypted cookie on both the request and the response. This replaces the previous design (`resolveSession()` doing a live fetch on every server-rendered request that needed it) with a single, centralized refresh point.
- **Session encryption via a dedicated `lib/server/token-cipher.ts`** (new): `encrypt()`/`decrypt()` wrap Node's `crypto` module (AES-256-GCM, keyed by `TOKEN_ENCRYPTION_KEY`), producing an `"iv.authTag.ciphertext"` (all base64) string; `decrypt()` returns `null` rather than throwing on any malformed/tampered input, which `parse-session.ts` treats the same as "no session". `lib/server/config.ts`'s `getTokenEncryptionKey()` throws if the env var is unset — the cookie's "plaintext for now" state from the previous sync is fully resolved.
- **Permissions/roles decoded from the JWT, never trusted from the profile API** (new): `lib/server/jwt.ts` decodes the access token's payload (no signature verification — safe here since the token was just issued by this app's own backend) and extracts the `permission`/`role` claim types; `lib/server/build-session-claims.ts` unions those with the profile API's own claims for display purposes only. Both `loginAction` and `refreshSession()` independently re-derive `permissions`/`roles` this way on every token issuance.
- **Context-provider-per-concern for client state**: `SidebarProvider`/`useSidebar`, `AccentColorProvider`/`useAccentColor`, and `next-themes`' provider (wrapped in `components/theme/theme-provider.tsx`) each own one slice of persisted UI state via a throwing custom hook — unchanged pattern, relocated files.
- **Owned, CLI-generated UI primitives**: `components/ui/*` (24 files, style `"radix-nova"`) still follows the `data-slot="<name>"` + `cva()` convention. `button.tsx` retains its hand-modification beyond CLI output: a `loading` prop (renders `Spinner`, sets `aria-busy`/`disabled`) plus a `cursor-pointer` utility baked into `buttonVariants`.
- **Single-CSS-variable theming** and **runtime accent swap via DOM attribute + localStorage**: unchanged from before — `--primary` drives themed surfaces, `AccentColorProvider` sets `data-accent` on `<html>`.
- **Hydration-safe browser-state restoration**: unchanged pattern (`hydrated` flag + `useEffect`, `eslint-disable react-hooks/set-state-in-effect`) in `SidebarProvider` and `AccentColorProvider`; `components/theme/use-has-mounted.ts` (`useSyncExternalStore`) still guards `ThemeToggle`.
- **Mobile drawer closes on route change via render-time state adjustment**: unchanged, still in `hooks/use-sidebar.tsx`.
- **Generic, presentational `DataTable<TData>` building block**: `components/shared/data-table/` composes a toolbar (actions + debounced search + export/refresh/columns-visibility), a table body (skeleton-loading rows, an `Empty` state, or an `Alert`-based error state that replaces the body and hides pagination), and a windowed-pagination footer (`getPageWindow()` always keeps page 1/last visible plus siblings around the current page). It takes no dependency on any feature or data-fetching library — fully controlled via props (`data`, `columns`, `isLoading`, `error`, callbacks); both `UsersDataTable` (server-driven search via URL params) and `RolesDataTable` (local client-side filtering — there's no backend search endpoint for roles) reuse it as-is with different search wiring. `isLoading` takes priority over a stale `error` — an in-flight refetch (e.g. clicking Refresh) always shows the table's own skeleton-row loading state rather than a leftover error from a previous failed load. **New this sync**: optional per-column client-side sorting (`DataTableColumn.sortable`/`sortValue`) — a sortable header renders as a `<button>` cycling asc → desc → unsorted with `ArrowUp`/`ArrowDown`/`ArrowUpDown` icons, and the table body sorts a `useMemo`-derived copy of `data`. Explicitly scoped to callers holding the full result set client-side — `RolesDataTable` uses it (name/description columns), `UsersDataTable` deliberately does not, since it paginates via the backend and only ever holds one page of `data` at a time.
- **Controlled form state alongside `useActionState`, for Server Action forms that can fail**: dialogs bound to a mutation Server Action keep their own `useState<FormValues>` in parallel with `useActionState(...)`. This is deliberate, not redundant — React resets *uncontrolled* form fields once a Server Action settles, regardless of success or failure, which would silently wipe user input after a validation error; controlled state survives that reset.
- **Force-remount via a bumped `key` to reset `useActionState`**: `useActionState` has no imperative "clear this error/state" API, so each `*DataTable` bumps a per-dialog key counter on every open (`createDialogKey`, `editDialogKey`, ...) and passes it as that dialog's React `key`, forcing a fresh component instance (fresh action state, fresh controlled form state, and — for the edit dialogs — a fresh detail-fetch) each time it's opened.
- **Toast notifications via a themed `sonner` wrapper**: `components/toast/` never exposes `sonner`'s `toast` directly — call sites use `notifySuccess`/`notifyError` (`notify.ts`), and the visual theme (`saturatedToastOptions`, `withToastProgress()`) is centralized in `toast-theme.ts` so every toast in the app looks consistent without each call site repeating class names.
- **Backend error messages surfaced through the shared `send()` wrapper**: `lib/server/http.ts`'s `extractErrorMessage()` centralizes turning a non-2xx response body into a human-readable string (envelope message → validation-errors map → `ProblemDetails.title` → generic fallback), so every `features/*/api/*.ts` call gets real error text without each call site parsing the body itself.

## Shared Kernel / Common Building Blocks Used

- `components/ui/*` — the app's own primitive layer.
- `components/shared/data-table/` — generic list-table building block (toolbar, pagination, loading/empty/error states, optional per-column sort); consumed by both `features/users` and `features/roles` (with different search/sort strategies — see Key Design Patterns), designed with no feature-specific knowledge baked in.
- `components/shared/object-viewer/` — new this sync, additive; a recursive read-only object/table renderer built on `components/ui/table` + `components/ui/input`, no `features/*` dependency. Not yet consumed by any page.
- `components/toast/` — toast notification wrapper around `sonner`; mounted once at the root layout, called from feature code via `notifySuccess`/`notifyError`.
- `lib/shared/utils.ts` (`cn`), `lib/shared/dedupe-claims.ts`, `lib/shared/user-display.ts` — cross-cutting helpers consumed by 2+ features or by layout chrome. New this sync: `lib/shared/menu.ts` (`buildVisibleMenu`, consumed by `Sidebar`) and `lib/shared/authorization.ts` (`hasPermission`/`hasAnyPermission`/`hasAllPermissions`/`isSuperAdminUser`, safe for both server and client — consumed directly by `Sidebar` and, via `lib/server/authorization.ts`'s thin wrapper, by `UsersPage`/`RolesPage`).
- `lib/server/*` — the server-only building blocks every feature's `api/` layer is built on: `http.ts`, `call-guard.ts`, `config.ts`, `session.ts`, `session-cookie.ts`, `authorization.ts` (now a thin wrapper over `lib/shared/authorization.ts`), plus, new this sync, the session-encryption/refresh chain — `token-cipher.ts`, `jwt.ts`, `build-session-claims.ts`, `parse-session.ts`, `refresh-session.ts` — used by `src/proxy.ts` and `features/auth/api/{login,logout}-action.ts`.
- Permission-string constants remain **not** a shared kernel piece — per-feature `features/{users,roles}/constants/permissions.ts` (`USERS_PERMISSIONS`, `ROLES_PERMISSIONS`), each mirroring the backend's `IdentityPermissions` string constants. New this sync: nav metadata followed the same per-feature-ownership move (`features/{dashboard,users,roles}/constants/nav-item.ts`), assembled (not owned) by the top-level `constants/nav-items.ts`.
- `hooks/*`, `components/theme/*` — cross-cutting building blocks consumed by layout components.
- No package or code is shared with another client app — `clients/` still contains only `admin`.

## Module/Route Boundaries

Two route groups/areas exist:
- `(dashboard)` — wraps `/`, `/user-profile`, `/identity/users`, and now `/identity/roles` with `resolveSession()` + `AppShell` (top bar, sidebar).
- `login` (ungrouped) — `/login`, rendered under the root layout only, no `AppShell`/session resolution.

`constants/nav-items.ts` declares `/identity`, `/identity/users`, `/identity/roles`, `/settings`. `/identity/users` and `/identity/roles` both now have real routes and pages (`app/(dashboard)/identity/{users,roles}/page.tsx`, gated on `USERS_PERMISSIONS.View`/`ROLES_PERMISSIONS.View` respectively); `/settings` still has no corresponding route.

Feature isolation is enforced by convention (barrel-only cross-feature imports), verified in the places it's exercised so far: `features/auth/api/login-action.ts` and `features/users/components/users-page.tsx`/`api/create-user-action.ts` all import from `@/features/user-profile`'s barrel, not its internals; `features/users/components/users-page.tsx` likewise imports `getAllRoles` from `@/features/roles`'s barrel, not its internals. Two narrow, reasoned exceptions exist (`constants/nav-items.ts`, `components/layout/user-menu.tsx` — see Key Design Patterns / Dependency Direction); a third, lower-level one also exists at the `lib/server` tier: `lib/server/refresh-session.ts` imports `features/auth/api/refresh-token.ts` directly (bypassing the `@/features/auth` barrel, which does export `refreshToken`) — worth noting since it's also a reversal of the usual `features/* -> lib/server/*` dependency direction (here, `lib/server` reaches into a feature's `api/` file), unlike the other two exceptions which stay within the normal direction.

## Known Architectural Risks / Debt

| Finding | Severity | Notes |
|---|---|---|
| ~~Session cookie stores the access/refresh token and claims as plaintext JSON~~ — **resolved this sync** | — | The cookie is now AES-256-GCM encrypted (`lib/server/token-cipher.ts`, keyed by `TOKEN_ENCRYPTION_KEY`, which is a hard requirement — `getTokenEncryptionKey()` throws if unset). See Auth Flow (overview.md) / Key Design Patterns. |
| ~~No token refresh flow wired up~~ — **resolved this sync** | — | `src/proxy.ts` now proactively rotates the access/refresh token via `lib/server/refresh-session.ts` when within `REFRESH_LEAD_MS` (5 min) of expiry; a failed refresh is treated as non-fatal (see Auth Flow), not a dead session. |
| `proxy.ts`'s call chain uses Node's `crypto` module directly, with no explicit runtime pin | Low–Medium (verify) | `token-cipher.ts` (imported transitively via `parse-session.ts`/`refresh-session.ts`) uses `createCipheriv`/`createDecipheriv` from Node's built-in `crypto`, which the traditional Edge Runtime does not support. `proxy.ts` has no `export const runtime = "nodejs"` (or similar) declaration; the current behavior is consistent with Next.js 16's `proxy.ts` convention defaulting to the Node.js runtime (unlike the old edge-only `middleware.ts`), but this is inferred from the code, not confirmed via an explicit config — worth pinning explicitly if that assumption is ever wrong for a deployment target that still expects edge middleware. |
| Nav item references a route with no `page.tsx` (`/settings`) | Low | `/identity/users` and `/identity/roles` both now have real pages. `/settings` still 404s if followed. |
| Backend list endpoints never populate `Roles`/`Claims` on the DTO | Low (worked around, but worth remembering) | `UserService.SearchAsync`/`GetAllAsync` and `RoleService.GetAllAsync` all use a `DataMapper.cs` projection that only maps scalar fields; only the single-record fetch (`GetByIdAsync`) populates `Roles`/`Claims`. Both edit dialogs correctly re-fetch full detail on open to work around this (see Key Design Patterns), but any future feature reading a list endpoint for role/claim data would silently get empty arrays if it forgot to do the same. |
| Dashboard still renders hardcoded mock data (`features/dashboard/api/sample-data.ts`) | Low (by design) | Unchanged this sync — the dashboard wasn't part of this batch of work either. |
| `prettier` + `prettier-plugin-tailwindcss` installed but no config file/`format` script | Low | Unchanged — still `unknown` whether formatting is enforced anywhere. |
| No automated test suite | Low (by design at this stage) | Unchanged — no `*.test.*`/`*.spec.*` files, no test runner in `package.json`. Now more notable given real auth/session logic plus full Users and Roles CRUD flows all exist untested. |
| `components/ui/button.tsx` hand-modified beyond shadcn CLI output (`loading` prop, `cursor-pointer`) | Low | Unchanged — re-running the shadcn CLI would silently drop these customizations unless done carefully. |
| `DataTable`'s `onExport` prop has no caller yet | Low | `components/shared/data-table/data-table-toolbar.tsx` already renders an Export button when `onExport` is passed, but no current feature (including `UsersDataTable`/`RolesDataTable`) passes one — dead capability until a consumer needs it. |
| `components/shared/object-viewer/` has no consumer yet | Low (by design) | New this sync, purely additive — ported from an external export spec and adapted to this project's design tokens/`components/ui/*` primitives, but not imported by any page or dialog. Dead code until something wires it in. |

## Notes

<!-- manual: content below this line is human-authored and must be preserved verbatim during sync -->

---
_Generated: 2026-08-03 — scope: client app "admin" — see .claude/CLAUDE.md for update rules._
