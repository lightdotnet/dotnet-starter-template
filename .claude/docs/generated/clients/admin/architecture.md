# Architecture: admin

## Layering

Observed folder organization (verified via directory listing) — everything now lives under `clients/admin/src/` (previously at `clients/admin/` root):

```text
src/
  app/                      routing only — layout.tsx, globals.css, error.tsx (new this sync — root error boundary,
                             "use client", a bare centered Alert + retry Button, no AppShell), login/page.tsx,
                             (dashboard)/{layout.tsx,page.tsx,error.tsx,loading.tsx,user-profile/page.tsx,
                             identity/{users,roles}/page.tsx,notifications/page.tsx} (error.tsx/loading.tsx new
                             this sync — error.tsx same shape as the root one but rendered inside AppShell, since
                             the parent layout stays mounted above an error boundary; loading.tsx a centered
                             Spinner, cascading to every nested dashboard route)
  features/
    auth/                   api/{token.api,login-action,logout-action,refresh-session-action}.ts,
                             components/{login-page,login-form}.tsx, types/token.ts, index.ts
                             (token.api.ts — new this sync, consolidates the former login.ts/refresh-token.ts into
                             one file, exporting getToken (renamed from login) and refreshToken; logout-action.ts —
                             barrel does not export it, see Dependency Direction; refresh-session-action.ts — a
                             super-admin-gated manual token-rotation Server Action, uses the persistSessionCookie()
                             helper, see Dependency Direction)
    user-profile/           api/{user-profile.api,resolve-session}.ts, components/user-profile-page.tsx, types/user-session.ts, index.ts
                             (user-profile.api.ts — new this sync, consolidates the former get-current-user.ts/
                             list-sessions.ts/revoke-session.ts into one file, exporting getCurrentUser/listSessions/
                             revokeSession; resolve-session.ts deliberately left separate — cookie-only, no backend call)
    home/                   (replaces the deleted dashboard/ — see below) components/{home-page,
                             profile-summary-card}.tsx, constants/nav-item.ts (`HOME_NAV_ITEM`), index.ts. No `api/`
                             of its own — `home-page.tsx` calls `resolveSession()` (via the user-profile barrel) and
                             `getMyNotificationsAction()` (via a direct file import into notifications)
    users/                  api/*.ts (8 files, down from 15 — new this sync: the former 7 per-function endpoint
                             files (search-users, get-all-users, get-user-by-id, create-user, update-user,
                             delete-user, force-password) plus the already-deleted get-user-by-username.ts were
                             consolidated into one users.api.ts exporting getAllUsers/getDomainUser/getUserById/
                             searchUsers/createUser/updateUser/forcePassword/deleteUser; the 7 "use server" action
                             files — create-user-action, update-user-action, force-password-action,
                             delete-user-action, get-user-detail-action, search-users-action, get-domain-user-action
                             — were left as-is, only their import path changed to users.api.ts),
                             components/{users-page,users-data-table,create-user-dialog,edit-user-dialog,delete-user-dialog}.tsx,
                             constants/{permissions,nav-item,auth-provider}.ts (`USERS_PERMISSIONS`, `USERS_NAV_ITEM`,
                             `AUTH_PROVIDER_SELECT_OPTIONS`, shared by create-user-dialog.tsx and edit-user-dialog.tsx),
                             types/user.ts, index.ts
    roles/                  api/*.ts (7 files, down from 12 — new this sync: the former 6 per-function endpoint
                             files (get-all-roles, get-role-by-id, get-permissions, create-role, update-role,
                             delete-role) were consolidated into one roles.api.ts exporting getAllRoles/
                             getPermissions/getRoleById/createRole/updateRole/deleteRole; the 6 "use server" action
                             files — create-role-action, update-role-action, delete-role-action,
                             get-role-detail-action, get-all-roles-action, get-permissions-action — were left as-is,
                             only their import path changed to roles.api.ts),
                             components/{roles-page,roles-data-table,create-role-dialog,edit-role-dialog,delete-role-dialog}.tsx,
                             constants/{permissions,nav-item}.ts (`ROLES_PERMISSIONS`, `ROLES_NAV_ITEM`), types/{role,permission-definition}.ts, index.ts
    notifications/          api/*.ts (7 files, down from 8 — new this sync: get-notifications.ts/send-notification.ts
                             were consolidated into notifications.api.ts (getNotifications/sendNotification — the
                             admin-facing `notification`-route functions; SendNotificationRequest moved out of this
                             file into types/notification.ts), and get-my-notifications.ts/get-unread-count.ts/
                             mark-notification-read.ts were consolidated into a new, separate user-notifications.api.ts
                             (getMyNotifications/getUnreadCount/markNotificationRead — the self-service
                             `user_notification`-route functions); the 5 "use server" action files —
                             get-my-notifications-action, get-unread-count-action, mark-notification-read-action,
                             send-notification-action, get-signalr-token-action — were left as-is, only their import
                             path changed), hooks/use-notifications.ts, context/notifications-provider.tsx
                             (`NotificationsProvider`/`useNotificationsContext`),
                             components/{notification-bell,notifications-page,notifications-data-table,
                             send-notification-dialog,user-select,notification-inbox,notification-list,
                             notification-detail}.tsx, constants/{permissions,nav-item}.ts, types/notification.ts, index.ts
  components/
    ui/                     shadcn-CLI-generated primitives (23 files) plus five hand-written/added-this-sync
                             additions: native-select.tsx (wraps a real `<select>`); popover.tsx (Radix
                             `Popover` wrapper); command.tsx (`cmdk`-based filterable list); combobox.tsx
                             — `Combobox<TValue>`, composing popover.tsx + command.tsx + button.tsx, the
                             shadcn-style replacement for the now-deleted components/select/* family (see
                             Key Design Patterns); button-group.tsx (new this sync — `ButtonGroup`/
                             `ButtonGroupSeparator`, a shadcn-style primitive that visually connects adjacent
                             buttons via shared borders and end-only rounding, composed with separator.tsx;
                             consumed by the new components/shared/data-table/data-table-buttons.tsx —
                             provenance (CLI-generated vs hand-written) is `unknown`, but it follows the same
                             `data-slot`/`cn` conventions as the rest of this folder). select.tsx (the former
                             Radix `Select` wrapper) and components/select/* (its Floating-UI-based successor)
                             are both gone. 28 files total. **Changed this sync**: `tabs.tsx`'s `TabsTrigger`
                             gained `cursor-pointer` (see Key Design Patterns), and `dialog.tsx`'s `DialogContent`
                             gained a `max-h`/`overflow-y-auto` scroll fix (see Key Design Patterns) — no new files.
    foundation/             use-listbox.ts, use-virtual-list.ts, floating-overlay.tsx — shrunk this sync;
                             use-floating-popover.ts, options-list.tsx, use-async-options.ts, and types.ts
                             were deleted along with components/select/*, their only consumer. The three
                             survivors now serve only components/command/* (Command Palette) plus
                             components/shared/data-table/data-table-virtual-body.tsx (use-virtual-list.ts
                             only). portal-container.ts (new) is unrelated to the rest of this folder — a
                             small React Context (`usePortalContainer`/`PortalContainerProvider`) that lets
                             components/ui/dialog.tsx hand its own DOM node to components/ui/popover.tsx,
                             fixing a Dialog/Popover nested-scroll bug (see Key Design Patterns)
    command/                command-palette.tsx, command-palette-provider.tsx, types.ts, index.ts;
                             Cmd/Ctrl+K overlay — untouched this sync, still no consumer wired into the app
    layout/                 topbar, sidebar, sidebar-nav-item, brand, breadcrumbs, user-menu, app-shell
    theme/                  theme-provider, accent-color-provider, theme-toggle, accent-color-picker, use-has-mounted, index.ts (barrel)
    shared/
      search-box.tsx
      access-denied.tsx     (new this sync) `AccessDenied({ permission })` — built on `components/ui/empty.tsx`;
                             the shared "access denied" panel returned by `lib/server/require-permission.tsx`'s
                             `requirePermission()` when the caller lacks the checked permission, replacing three
                             previously separate per-page implementations (see Key Design Patterns)
      data-table/           types.ts, data-table-toolbar.tsx, data-table-buttons.tsx (new this sync),
                             data-table-pagination.tsx, data-table.tsx, data-table-virtual-body.tsx,
                             index.ts (barrel); generic reusable list-table building block, no data-fetching
                             of its own; `types.ts`/`data-table.tsx` gained optional per-column client-side
                             sorting (`sortable`/`sortValue`) in a prior sync, and an optional `mode` prop
                             (`"paginated"` default / `"virtualized"` / `"infinite"`, the latter two
                             rendered via `data-table-virtual-body.tsx`) plus `onSortChange` for
                             server-driven sort — both additive, fully backward-compatible. New this sync:
                             `data-table-toolbar.tsx` was restructured into three stacked sections
                             (actions / search / an inserted `Separator` between rendered sections) and
                             gained `customSearch`/`onCustomSearch` props for a caller-supplied,
                             apply-on-click multi-field filter UI; the Export/Refresh/Columns cluster it used
                             to render inline was extracted into the new `data-table-buttons.tsx`
                             (`DataTableButtons`, built on the new `components/ui/button-group.tsx`), which
                             `data-table.tsx` now composes itself and renders either inline (via the
                             toolbar's `buttons` prop) or inside the table's own bordered content box when
                             `customSearch` is used
      object-viewer/         (new, additive — not yet used by any page) utils.ts, object-viewer-layout-context.tsx,
                             column-resize-handle.tsx, object-viewer-row.tsx, object-viewer.tsx, index.ts;
                             built on `components/ui/table` + `components/ui/input`, no `features/*` dependency
    toast/                  toast-theme.ts, notify.ts, toaster.tsx, index.ts (barrel); wraps the `sonner` dependency
  hooks/                    use-sidebar.tsx, use-scrolled.ts, use-guarded-action.ts, use-action-success-toast.ts
                             (last 2 new — centralize the toast/pending patterns previously duplicated per dialog,
                             see Key Design Patterns)
  lib/
    server/                 config.ts, http.ts, call-guard.ts, session-cookie.ts, session.ts, authorization.ts,
                             token-cipher.ts, jwt.ts, build-session-claims.ts, refresh-session.ts, parse-session.ts,
                             backend-api.ts, api-clients.ts, http-handlers/bearer-token-handler.ts,
                             persist-session-cookie.ts, require-permission.tsx (new this sync)
                             (session encryption/JWT/refresh chain from a prior sync; backend-api.ts/api-clients.ts/
                             http-handlers/bearer-token-handler.ts are from a prior sync — decouple auth-token
                             injection from http.ts into a handler pipeline; persist-session-cookie.ts is from a
                             prior sync — extracts the Server-Action-context cookie write (next/headers's cookies())
                             previously hand-rolled separately in refresh-session-action.ts and
                             get-signalr-token-action.ts, see Key Design Patterns; require-permission.tsx is new
                             this sync — `requirePermission(permission)`, the shared page-level permission-gate
                             helper composing `resolveSession()`, `authorization.ts`'s `hasPermission`, and
                             `components/shared/access-denied.tsx`, see Key Design Patterns; all 15 flat files
                             plus http-handlers/bearer-token-handler.ts now start with `import "server-only";` —
                             from a prior sync, a compile-time guard, see Key Design Patterns. `require-permission.tsx`
                             itself is a `.tsx` file — the one `lib/server/*` module that returns JSX)
    shared/                 utils.ts, dedupe-claims.ts, user-display.ts, menu.ts, authorization.ts
                             (menu.ts and authorization.ts are new — see Key Design Patterns)
  constants/                nav-items.ts (permissions.ts is still gone — relocated per-feature; nav item definitions
                             are now also per-feature, see `features/{home,users,roles,notifications}/constants/nav-item.ts` —
                             `home` replaced `dashboard` this sync)
  types/                    api.ts, claim.ts, nav.ts, session.ts
  proxy.ts                  Next.js 16 "proxy" convention file (successor to middleware.ts); calls into
                             `lib/server/{parse-session,refresh-session,token-cipher,session-cookie}.ts` — see
                             Known Architectural Risks / Debt for an open question about which runtime this requires
```

This is a **feature-folder** layering: `app/*` pages are pure re-exports from a feature's public API; `features/<name>/` owns its own `api/`, `components/`, and (when a type has exactly one consumer) `types/`, each exposed through an `index.ts` barrel; `components/layout/*` (app chrome) and `components/theme/*` compose `components/ui/*` plus `hooks/*`/`lib/shared/*`/`constants/*`/`types/*`; `components/ui/*` remains the leaf primitive layer. `components/shared/data-table/` and `components/toast/` are cross-feature building blocks sitting at the same layer as `components/shared/search-box.tsx`/`components/shared/access-denied.tsx` — presentational/utility, composed by features (or, for `access-denied.tsx`, by `lib/server/require-permission.tsx`) but with no feature-specific knowledge baked in.

**`features/<name>/` may also own a `constants/`** — `permissions.ts` (`users`, `roles`, `notifications`) plus a `nav-item.ts` per nav-bearing feature, each exporting one `NavItem` constant for that feature's own entry — a deliberate move away from the top-level `constants/` folder for anything feature-specific. Top-level `constants/nav-items.ts` is purely an *assembly* file: it imports each feature's `NavItem` and composes the final `NAV_ITEMS` tree, only declaring the group node (see below) and the "Settings" leaf directly since neither is owned by a single feature (Settings has no feature/page at all yet). **Changed this sync**: `dashboard` (and its `DASHBOARD_NAV_ITEM`) is gone, replaced by `home`/`HOME_NAV_ITEM` (`@/features/home/constants/nav-item`) as the tree's first entry — same shape/reasoning as before, see "Each nav-bearing feature owns its nav metadata" under Key Design Patterns for the barrel-bypass this requires. Also found via this sync's verification (not part of the reported change set): the group node itself was reworked — previously labeled "Identity" (`/identity`, children `USERS_NAV_ITEM`/`ROLES_NAV_ITEM` only, with `NOTIFICATIONS_NAV_ITEM` a separate top-level entry between it and "Settings") — it is now labeled **"Administration"** (`/administration`, icon `ShieldCog`) and its `children` array now also includes `NOTIFICATIONS_NAV_ITEM`, so Notifications moved from a top-level nav entry into this group alongside Users/Roles. `USERS_NAV_ITEM`/`ROLES_NAV_ITEM`/`NOTIFICATIONS_NAV_ITEM`'s own `href`s (`/identity/users`, `/identity/roles`, `/notifications`) are unchanged — only the assembled tree's grouping/labeling moved — and `/administration` itself has no `page.tsx` (same 404-if-followed shape as `/settings`).

## Dependency Direction

Verified via actual `import` statements:

```text
src/proxy.ts                       -> features/user-profile/api/user-profile.api (direct file, not the barrel — the
                                       barrel also re-exports UserProfilePage/other feature surface not needed here;
                                       changed this sync, was features/user-profile/api/get-current-user, now
                                       consolidated into user-profile.api.ts), lib/server/token-cipher (encrypt),
                                       lib/server/build-session-claims, lib/server/refresh-session
                                       (refreshSessionIfNearExpiry — changed this sync, was a direct refreshSession()
                                       call plus its own inline REFRESH_LEAD_MS check, now delegated to the shared
                                       helper), lib/server/parse-session,
                                       lib/server/session-cookie (SESSION_COOKIE_NAME, buildSessionCookieOptions —
                                       changed this sync, REFRESH_LEAD_MS is no longer imported here directly, it
                                       moved into refresh-session.ts alongside the check it gates), types/session
                                       — see Known Architectural Risks / Debt: uses Node's `crypto` (via token-cipher.ts)
                                       directly with no explicit `export const runtime` pin
src/app/layout.tsx                 -> components/theme (ThemeProvider, AccentColorProvider), components/toast (AppToaster),
                                       components/ui/tooltip
src/app/login/page.tsx             -> features/auth (LoginPage)
src/app/(dashboard)/layout.tsx     -> components/layout/app-shell, features/user-profile (resolveSession)
src/app/(dashboard)/page.tsx       -> features/home (HomePage) — changed this sync, was features/dashboard (DashboardPage,
                                       now deleted)
src/app/(dashboard)/user-profile/page.tsx -> features/user-profile (UserProfilePage)
src/app/(dashboard)/identity/users/page.tsx -> features/users (UsersPage)
src/app/(dashboard)/identity/roles/page.tsx -> features/roles (RolesPage)
src/app/(dashboard)/notifications/page.tsx -> features/notifications (NotificationsPage)

components/layout/app-shell.tsx    -> hooks/use-sidebar, components/layout/{sidebar,topbar}, types/session (ProfileData),
                                       features/notifications/context/notifications-provider (NotificationsProvider —
                                       new this sync)
                                       (takes `{ permissions, userName, user, children }`, passes `permissions`/`userName`
                                       through to Sidebar and `user` through to TopBar; changed this sync — now wraps
                                       its whole subtree in `<NotificationsProvider>`, nested inside `<SidebarProvider>`,
                                       so TopBar's bell and any page under `(dashboard)` share one live SignalR
                                       connection/unreadCount)
components/layout/topbar.tsx       -> lib/shared/utils, hooks/{use-scrolled,use-sidebar}, components/ui/{button,badge},
                                       components/layout/{breadcrumbs,brand,user-menu}, components/shared/search-box,
                                       components/theme (ThemeToggle, AccentColorPicker),
                                       features/notifications/components/notification-bell (direct file, not the
                                       barrel — see the barrel-bypass note below)
components/layout/sidebar.tsx      -> lib/shared/utils, hooks/use-sidebar, components/layout/sidebar-nav-item,
                                       constants/nav-items (NAV_ITEMS, direct import — see Key Design Patterns for why
                                       this is a deliberate barrel-bypass exception), lib/shared/menu (buildVisibleMenu),
                                       lib/shared/authorization (hasPermission), components/ui/sheet
                                       (takes `{ permissions, userName }`, computes the visible menu via `useMemo`)
components/layout/sidebar-nav-item.tsx -> lib/shared/utils, hooks/use-sidebar, types/nav
                                       (`expanded` is now `isExpanded(item.href) ?? branchActive` — was `||`,
                                       which forced a branch containing the active route permanently open
                                       regardless of clicks; see Key Design Patterns)
components/layout/breadcrumbs.tsx  -> components/ui/breadcrumb, constants/nav-items, types/nav
components/layout/user-menu.tsx    -> components/ui/{avatar,button,dropdown-menu}, lib/shared/user-display,
                                       features/auth/api/logout-action (direct file, not the barrel — the barrel also
                                       re-exports LoginPage, an async Server Component; see note below), types/session
                                       (ProfileData) — takes a `user: ProfileData | null` prop; "Log out" calls
                                       `logoutAction(currentPath)` where `currentPath` is `usePathname()` + `useSearchParams()`

components/theme/theme-toggle.tsx        -> next-themes, ./use-has-mounted, components/ui/{button,dropdown-menu}
components/theme/accent-color-picker.tsx -> components/ui/{button,dropdown-menu}, ./accent-color-provider

features/auth/index.ts              -> ./components/login-page, ./api/token.api, ./types/token
                                        (exports getToken/refreshToken — changed this sync, was ./api/login,
                                        ./api/refresh-token, now consolidated into one token.api.ts; does NOT export
                                        logoutAction — see components/layout/user-menu.tsx above)
features/auth/components/login-page.tsx  -> ./login-form only (async; reads `searchParams` for `redirect`; changed this
                                        sync — no longer imports components/ui/card, the Card markup moved into
                                        login-form.tsx below)
features/auth/components/login-form.tsx  -> components/ui/{card,button,input,label,alert,spinner}, features/auth/api/login-action
                                        (takes `{ redirect? }`, renders it as a hidden form field when present;
                                        changed this sync — now a Client Component owning the full Card markup
                                        (moved from login-page.tsx above) so it can render a dimmed, centered
                                        Spinner overlay + `aria-busy` on the Card, plus a `<fieldset disabled={pending}>`
                                        around the inputs, while `loginAction` is pending)
features/auth/api/login-action.ts   -> "use server"; features/auth/api/token.api (getToken — changed this sync, was
                                        features/auth/api/login), features/user-profile (getCurrentUser —
                                        via the barrel, unchanged), lib/server/jwt (extractPermissions/extractRoles),
                                        lib/server/build-session-claims, lib/server/token-cipher (encrypt),
                                        lib/server/session-cookie (SESSION_COOKIE_NAME, SESSION_TTL_MS, buildSessionCookieOptions),
                                        types/session — reads a `redirect` form field and redirects there (guarded) on success
features/auth/api/logout-action.ts  -> "use server"; lib/server/session-cookie (SESSION_COOKIE_NAME), next/navigation
                                        (redirect) — deletes the cookie, redirects to `/login?redirect=<path>` (guarded)
features/auth/api/token.api.ts      -> lib/server/http, lib/server/call-guard, types/api, features/auth/types/token
                                        — new this sync, consolidates the former login.ts + refresh-token.ts into one
                                        file: exports getToken(request, device?) (renamed from login(), POST
                                        token/token/get — the doubled `token/token` segment comment now lives here)
                                        and refreshToken(request) (POST token/token/refresh, unchanged). Still one of
                                        the exceptions calling lib/server/http directly rather than
                                        lib/server/backend-api, since it runs before a session cookie exists; see Key
                                        Design Patterns for the handler-pipeline change. refreshToken's real caller is
                                        lib/server/refresh-session.ts

features/user-profile/index.ts      -> ./components/user-profile-page, ./api/{resolve-session,user-profile.api}, ./types/user-session
                                        (exports getCurrentUser/listSessions/revokeSession from user-profile.api —
                                        changed this sync, was three separate files)
features/user-profile/components/user-profile-page.tsx -> components/ui/{card,badge,separator,avatar,alert}, qrcode,
                                        features/user-profile/api/resolve-session, lib/shared/{dedupe-claims,user-display}
features/user-profile/api/resolve-session.ts -> lib/server/session (getSession), types/session
                                        (now a thin passthrough to getSession() — no longer calls getCurrentUser itself;
                                        proxy.ts keeps the cookie's profile/claims fresh instead, see Key Design Patterns)
features/user-profile/api/user-profile.api.ts -> lib/server/http, lib/server/backend-api, lib/server/call-guard,
                                        lib/server/http-handlers/bearer-token-handler (explicitBearerTokenHandler),
                                        types/api, features/users (barrel, type-only — UserDto), ./types/user-session
                                        — new this sync, consolidates the former get-current-user.ts + list-sessions.ts
                                        + revoke-session.ts into one hybrid file: getCurrentUser(accessToken) still
                                        takes an explicit token and calls lib/server/http directly via
                                        explicitBearerTokenHandler (the 3rd pre-session-cookie exception, called from
                                        login-action.ts and proxy.ts, both before/without an ambient session);
                                        listSessions()/revokeSession(tokenId) call lib/server/backend-api like an
                                        ordinary endpoint (ambient-session auth)

features/home/index.ts              -> ./components/home-page, ./constants/nav-item (HOME_NAV_ITEM) — replaces the
                                        deleted features/dashboard/index.ts
features/home/components/home-page.tsx -> components/ui/card, features/user-profile (resolveSession, barrel),
                                        features/notifications/api/get-my-notifications-action (direct file —
                                        not exported by @/features/notifications's barrel at all, so this is
                                        forced, not a stylistic choice), features/notifications/components/
                                        notification-inbox (direct file — the barrel does export NotificationInbox,
                                        so this one is a genuine cross-feature barrel-bypass; no comment in the
                                        source calls out why, but it's consistent with this app's existing pattern
                                        of importing a single component/action file directly rather than a whole
                                        feature barrel — see the barrel-bypass exceptions list below), ./profile-summary-card
                                        — async Server Component; redirects to /login if resolveSession() returns null
features/home/components/profile-summary-card.tsx -> components/ui/{avatar,badge,card,alert},
                                        features/user-profile/components/user-status-badge, lib/shared/user-display
                                        (getDisplayName, getInitials), types/session (SessionData) — a Server
                                        Component (no "use client"); renders an Alert if session.profile is null
features/home/constants/nav-item.ts -> lucide-react (Home), types/nav — label "Home", href "/", no permission

features/users/index.ts             -> ./components/users-page, ./api/users.api (getAllUsers, getDomainUser,
                                        getUserById, searchUsers, createUser, updateUser, forcePassword, deleteUser —
                                        changed this sync, was 7 separate files; get-user-by-username stays gone),
                                        ./constants/permissions (USERS_PERMISSIONS), ./constants/nav-item (USERS_NAV_ITEM),
                                        ./types/user
features/users/constants/nav-item.ts -> lucide-react (Users), ./permissions (USERS_PERMISSIONS), types/nav
                                        (new; label "Users", href "/identity/users", permission USERS_PERMISSIONS.View)
features/users/constants/auth-provider.ts (new this sync) -> components/ui/combobox (ComboboxOption, type-only) —
                                        exports `AUTH_PROVIDER_SELECT_OPTIONS`, `[{value: "", label: "Local"},
                                        {value: "AD", label: "AD"}]`; consumed by create-user-dialog.tsx and
                                        edit-user-dialog.tsx (see Key Design Patterns)
features/users/components/users-page.tsx -> lib/server/require-permission (requirePermission — changed this sync, was
                                        a direct features/user-profile (resolveSession) call plus inline
                                        redirect/permission-check/Empty JSX, see Key Design Patterns), features/users/api/users.api
                                        (searchUsers — changed a prior sync, was features/users/api/search-users),
                                        features/users/components/users-data-table,
                                        lib/server/authorization (hasPermission — changed this sync, now called as
                                        `hasPermission(session, permission)`, no separate `userName` argument),
                                        features/users/constants/permissions
                                        (USERS_PERMISSIONS — moved from the former top-level constants/permissions)
                                        — no longer imports components/ui/empty or next/navigation directly (both
                                        moved into lib/server/require-permission.tsx); no longer fetches
                                        features/roles/api/get-all-roles this sync; the role
                                        catalog is now self-fetched by edit-user-dialog.tsx on open instead
features/users/components/users-data-table.tsx -> components/shared/data-table (DataTable + types), components/ui/{avatar,badge,button},
                                        components/ui/dropdown-menu, features/users/components/{create,edit,delete}-user-dialog,
                                        features/user-profile/components/user-status-badge, lib/shared/user-display,
                                        features/users/types/user — no longer takes a `roles`/`RoleDto` prop or imports
                                        features/roles/types/role this sync (edit-user-dialog.tsx now fetches roles itself)
features/users/components/create-user-dialog.tsx -> components/ui/{alert,button,combobox,dialog,input,label,spinner},
                                        lucide-react (SearchIcon), hooks/use-action-success-toast,
                                        features/users/api/{create-user-action,get-domain-user-action},
                                        features/users/constants/auth-provider — gained a
                                        domain-user (AD) lookup — a small icon button merged into the Username
                                        input plus a blur-triggered autosearch, both calling
                                        get-domain-user-action.ts — and a new Auth Provider Combobox field; no
                                        longer imports components/toast (notifySuccess) — the "found on domain"
                                        result now renders as an inline message under the field instead of a toast
                                        (see Key Design Patterns)
features/users/components/edit-user-dialog.tsx -> components/ui/{alert,button,checkbox,combobox,dialog,input,label,
                                        spinner,tabs} — status/authProvider now use the new components/ui/combobox
                                        (`Combobox`); components/select, whose `EntitySelect` this used before this
                                        sync, is gone,
                                        components/toast (notifySuccess), features/users/api/get-user-detail-action,
                                        features/users/api/update-user-action, features/users/api/force-password-action,
                                        features/roles/api/get-all-roles-action (direct file import,
                                        not the @/features/roles barrel; see the barrel-bypass exceptions below),
                                        features/roles/types/role (RoleDto), features/users/types/user,
                                        types/claim (ClaimDto) — now holds its own
                                        `roles: RoleDto[]` state and fetches
                                        `Promise.all([getUserDetailAction(user.id), getAllRolesAction()])` on open,
                                        since users-data-table.tsx no longer passes a `roles` prop down; also uses
                                        hooks/use-action-success-toast (2 call sites — the update form
                                        and the password-reset form). Does not define its
                                        own `AUTH_PROVIDER_OPTIONS`/`AUTH_PROVIDER_SELECT_OPTIONS` locally — imports
                                        `AUTH_PROVIDER_SELECT_OPTIONS` from
                                        features/users/constants/auth-provider.ts (shared with create-user-dialog.tsx).
                                        Its `authProvider` default state and `isLocalAccount` check are
                                        `""`/`authProvider === ""` to match
                                        the constant's blank-means-Local value, fixing a latent bug where saving an
                                        unchanged Local selection would have written the literal string `"Local"`
                                        into the backend's AuthProvider column instead of `null`
features/users/components/delete-user-dialog.tsx -> components/ui/{button,dialog}, components/toast, hooks/use-guarded-action
                                        (replacing hand-rolled useTransition), features/users/api/delete-user-action, features/users/types/user
features/users/api/create-user-action.ts -> "use server"; features/user-profile (resolveSession), features/users/api/users.api
                                        (createUser — changed this sync, was features/users/api/create-user),
                                        features/users/types/user, next/cache (revalidatePath, "/identity/users")
                                        — password is only required when
                                        `authProvider !== "AD"`, matching the dialog's own conditional `required`
                                        on the Password field (both were briefly out of sync — see Key Design Patterns)
features/users/api/get-domain-user-action.ts -> "use server"; features/user-profile (resolveSession),
                                        features/users/api/users.api (getDomainUser — changed this sync, was
                                        features/users/api/get-domain-user), features/users/types/user (DomainUserDto)
                                        — same `{data, error?}` shape as get-user-detail-action.ts; no
                                        `revalidatePath` (a read, not a list mutation)
features/users/api/{update-user-action,force-password-action,delete-user-action,get-user-detail-action}.ts
                                     -> "use server"; features/user-profile (resolveSession),
                                        features/users/api/users.api (updateUser/forcePassword/deleteUser/getUserById
                                        respectively — changed this sync, was 4 separate files), features/users/types/user
                                        (update-user-action and delete-user-action additionally import next/cache
                                        (revalidatePath, "/identity/users"); force-password-action
                                        and get-user-detail-action do not — neither one changes list-page data)
features/users/api/users.api.ts -> lib/server/backend-api, lib/server/call-guard, types/api, features/users/types/user
                                        — new this sync, consolidates the former 8 per-function files (search-users,
                                        get-all-users, get-user-by-id, create-user, update-user, delete-user,
                                        force-password, get-domain-user; get-user-by-username was already deleted)
                                        into one file exporting getAllUsers/getDomainUser/getUserById/searchUsers/
                                        createUser/updateUser/forcePassword/deleteUser, returning `Result<DomainUserDto>`
                                        for getDomainUser (calls `GET user/get_domain_user/{userName}`)

features/roles/index.ts             -> ./components/roles-page, ./api/roles.api (getAllRoles, getRoleById,
                                        getPermissions, createRole, updateRole, deleteRole — changed this sync, was
                                        6 separate files), ./constants/permissions (ROLES_PERMISSIONS),
                                        ./constants/nav-item (ROLES_NAV_ITEM), ./types/{role,permission-definition} —
                                        barrel; imported by app/(dashboard)/identity/roles/page.tsx and
                                        features/users/components/users-page.tsx
features/roles/constants/nav-item.ts -> lucide-react (KeyRound), ./permissions (ROLES_PERMISSIONS), types/nav
                                        (new; label "Roles", href "/identity/roles", permission ROLES_PERMISSIONS.View)
features/roles/components/roles-page.tsx -> lib/server/require-permission (requirePermission — changed this sync, was
                                        a direct features/user-profile (resolveSession) call plus inline
                                        permission-check/Empty JSX, see Key Design Patterns), features/roles/api/roles.api
                                        (getAllRoles — changed a prior sync, was features/roles/api/get-all-roles),
                                        features/roles/components/roles-data-table, lib/server/authorization (hasPermission
                                        — changed this sync, now called as `hasPermission(session, permission)`, no
                                        separate `userName` argument),
                                        features/roles/constants/permissions (ROLES_PERMISSIONS) — no longer imports
                                        components/ui/empty directly (moved into lib/server/require-permission.tsx)
                                        — no longer fetches getPermissions this sync; the permission catalog is now
                                        self-fetched by edit-role-dialog.tsx on open instead
features/roles/components/roles-data-table.tsx -> components/shared/data-table (DataTable + types), components/ui/{button,dropdown-menu},
                                        features/roles/components/{create,edit,delete}-role-dialog,
                                        features/roles/types/role — its `name`/`description`
                                        columns set `sortable: true`/`sortValue` on the shared DataTable (the whole
                                        role list is fetched upfront, so client-side sort is meaningful here, unlike
                                        UsersDataTable which paginates server-side). No longer takes a
                                        `permissions`/`PermissionDefinition` prop or imports
                                        features/roles/types/permission-definition this sync (edit-role-dialog.tsx
                                        now fetches permissions itself)
features/roles/components/create-role-dialog.tsx -> components/ui/{alert,button,dialog,input,label}, components/toast (notifySuccess),
                                        hooks/use-action-success-toast, features/roles/api/create-role-action
features/roles/components/edit-role-dialog.tsx -> components/ui/{alert,button,checkbox,dialog,input,label,spinner},
                                        components/toast (notifySuccess), hooks/use-action-success-toast,
                                        features/roles/api/get-role-detail-action,
                                        features/roles/api/get-permissions-action (intra-feature
                                        import, both files live under features/roles/),
                                        features/roles/api/update-role-action, features/roles/types/{role,permission-definition}
                                        — holds its own `permissions: PermissionDefinition[]` state and fetches
                                        `Promise.all([getRoleDetailAction(role.id), getPermissionsAction()])` on open,
                                        since roles-data-table.tsx no longer passes a `permissions` prop down
features/roles/components/delete-role-dialog.tsx -> components/ui/{button,dialog}, components/toast, hooks/use-guarded-action
                                        (replacing hand-rolled useTransition), features/roles/api/delete-role-action, features/roles/types/role
features/roles/api/{create-role-action,update-role-action,delete-role-action,get-role-detail-action}.ts
                                     -> "use server"; features/user-profile (resolveSession),
                                        features/roles/api/roles.api (createRole/updateRole/deleteRole/getRoleById
                                        respectively — changed this sync, was features/roles/api/{create-role,
                                        update-role,delete-role,get-role-by-id})
                                        (update-role-action additionally re-reads getRoleById first, to preserve any
                                        non-"permission"-typed claims before writing back the submitted permission set);
                                        create-role-action/update-role-action/delete-role-action additionally import
                                        next/cache (revalidatePath, "/identity/roles");
                                        get-role-detail-action does not — it's a detail read, not a list mutation
features/roles/api/roles.api.ts -> lib/server/backend-api, lib/server/call-guard, features/roles/types/{role,permission-definition}
                                        — new this sync, consolidates the former get-permissions.ts, get-all-roles.ts
                                        (fixed in a prior sync: was guardRawCall assuming a bare array; the endpoint
                                        actually wraps its response in the same envelope every other endpoint uses),
                                        and {get-role-by-id,create-role,update-role,delete-role}.ts (6 files total)
                                        into one file exporting getAllRoles/getPermissions/getRoleById/createRole/
                                        updateRole/deleteRole

features/notifications/index.ts     -> ./components/{notification-bell,notifications-page,notification-inbox}
                                        (notification-inbox added in a prior sync), ./hooks/use-notifications,
                                        ./constants/{permissions,nav-item} (NOTIFICATIONS_PERMISSIONS, NOTIFICATIONS_NAV_ITEM),
                                        ./types/notification (NotificationStatus, NotificationDto) — does NOT export
                                        get-my-notifications-action, context/notifications-provider, notification-list,
                                        or notification-detail; every cross-feature/cross-file consumer of those reaches
                                        in via a direct file import (see below)
features/notifications/constants/nav-item.ts -> lucide-react (Bell), ./permissions (NOTIFICATIONS_PERMISSIONS), types/nav
features/notifications/hooks/use-notifications.ts -> @microsoft/signalr, features/notifications/api/{get-my-notifications-action,
                                        get-unread-count-action,mark-notification-read-action,get-signalr-token-action},
                                        features/notifications/types/notification — markAsRead(id)
                                        returns Promise<boolean> (was Promise<void>); refresh(status?) gained an
                                        optional NotificationStatus param, remembered in a statusRef so a SignalR-pushed
                                        re-fetch preserves the active tab; .configureLogging(LogLevel.Critical) added to
                                        the HubConnectionBuilder chain (silences SignalR's own console.error for the
                                        expected abnormal-closure on unmount, see Known Architectural Risks / Debt);
                                        now only ever called from inside NotificationsProvider (below) — no other
                                        remaining direct caller
features/notifications/context/notifications-provider.tsx -> react (createContext/useContext),
                                        features/notifications/hooks/use-notifications — calls
                                        useNotifications() exactly once and exposes its full return value
                                        (notifications/unreadCount/loading/markAsRead/refresh) via
                                        NotificationsProvider/useNotificationsContext, same throwing-custom-hook
                                        shape as hooks/use-sidebar.tsx's SidebarProvider/useSidebar; consumed by
                                        components/layout/app-shell.tsx (mount point), notification-bell.tsx, and
                                        notification-inbox.tsx
features/notifications/components/notification-bell.tsx -> components/ui/{badge,button,dropdown-menu,empty,tabs},
                                        lib/shared/utils, features/notifications/context/notifications-provider
                                        (useNotificationsContext), features/notifications/types/notification (imported by
                                        components/layout/topbar.tsx via direct file path, not this feature's own
                                        barrel — see the exceptions note below). Tab filter
                                        (All/Unread/Archived) calls refresh(status) from context instead of
                                        computing a local filteredNotifications over one fetched page — fixes a bug
                                        where the tabs could disagree with the unread-count badge
features/notifications/components/notification-inbox.tsx -> components/ui/{badge,empty,pagination,tabs},
                                        lib/shared/utils, features/notifications/api/get-my-notifications-action,
                                        features/notifications/context/notifications-provider (useNotificationsContext),
                                        features/notifications/types/notification, ./notification-list, ./notification-detail
                                        — "use client"; two-pane grid layout (list + detail), local
                                        useState for notifications/totalPages/pageNumber/filter/selectedId, fetches via
                                        getMyNotificationsAction({pageNumber, status}) inside a useTransition; does NOT
                                        call useNotifications() itself or open its own SignalR connection — reads
                                        unreadCount/markAsRead from context so selecting an unread item updates the
                                        shared unread count (and therefore the topbar badge) immediately
features/notifications/components/notification-list.tsx -> lib/shared/utils, features/notifications/types/notification
                                        — the inbox's master-list pane (unread-dot indicator, title,
                                        fromName if present, formatted timestamp; highlights the selected row)
features/notifications/components/notification-detail.tsx -> next/link, components/ui/{badge,empty}, lucide-react,
                                        features/notifications/types/notification — the inbox's
                                        detail pane (Empty state when nothing selected, an "Archived" badge, message
                                        if present, a link to notification.url if present)
features/notifications/components/notifications-page.tsx -> lib/server/require-permission (requirePermission —
                                        changed this sync, was a direct features/user-profile (resolveSession, barrel)
                                        call plus its own next/navigation (redirect) on a missing permission; see Key
                                        Design Patterns — **real behavior change**: this page used to redirect on a
                                        missing NOTIFICATIONS_PERMISSIONS.Read, it now renders the same shared
                                        AccessDenied panel as Users/Roles instead), features/notifications/api/notifications.api
                                        (getNotifications — changed a prior sync, was features/notifications/api/get-notifications),
                                        features/notifications/components/notifications-data-table,
                                        lib/server/authorization (hasPermission — changed this sync, now called as
                                        `hasPermission(session, permission)`, no separate `userName` argument),
                                        features/notifications/constants/permissions,
                                        features/notifications/types/notification — does not import features/users
                                        (getAllUsers); the notifications -> users barrel-level cross-feature
                                        edge is gone entirely, replaced by a different-shaped edge (component-level,
                                        direct-file, cross-feature) from user-select.tsx below
features/notifications/components/notifications-data-table.tsx -> components/shared/data-table (DataTable + types),
                                        components/ui/native-select (status filter — replaced components/ui/select plus an
                                        embedded "all" pseudo-option in a prior sync; a real, always-reselectable placeholder),
                                        features/notifications/components/{send-notification-dialog,user-select},
                                        features/notifications/types/notification — status + recipient filters render
                                        via DataTable's `customSearch` slot and no longer auto-navigate per change;
                                        local "pending" state is applied to the URL only when the customSearch
                                        "Search" button (`onCustomSearch`) is clicked. Does not take a `users`
                                        prop — the "To" column renders `notification.toUserId`
                                        directly (matching the existing `fromName ?? fromUserId` fallback style
                                        already used for "From")
features/notifications/components/send-notification-dialog.tsx -> components/ui/{alert,button,dialog,input,label,textarea},
                                        components/toast (notifySuccess), hooks/use-action-success-toast,
                                        features/notifications/api/send-notification-action,
                                        features/notifications/components/user-select, lib/shared/user-display
                                        (getDisplayName) — no longer takes a `users` prop. While
                                        updating UserSelect's `onValueChange` to the `(user: UserDto) => void`
                                        signature, fixed a pre-existing latent bug: `FormValues.fromName` and
                                        `sendNotificationAction`'s `formData.get("fromName")` read both already
                                        existed, but no hidden `<input name="fromName">` was ever rendered, so an
                                        explicit "From" selection always submitted an empty fromName silently. Now
                                        selecting a "From" user sets `values.fromName = getDisplayName(user)` and a
                                        `<input type="hidden" name="fromName">` submits it. The "To" UserSelect just
                                        extracts `.id` — the backend's SendNotificationRequest (defined in
                                        features/notifications/types/notification.ts) has no `toName` field
features/notifications/components/user-select.tsx -> components/ui/{button,popover,command}, lucide-react,
                                        lib/shared/utils, features/users/api/search-users-action (direct file import,
                                        cross-feature, bypassing @/features/users's barrel; see
                                        the barrel-bypass exceptions below), lib/shared/user-display,
                                        features/users (barrel, type-only — UserDto) —
                                        a bespoke component with its own `open`/`query`/`options`/
                                        `loading`/`selectedLabel` state. No default/first-page fetch on open;
                                        a search only fires once the trimmed query reaches `MIN_SEARCH_LENGTH` (3
                                        characters — the component's own placeholder/comment describe this as "at
                                        least 3 characters," matching the guard
                                        `trimmedQuery.length < MIN_SEARCH_LENGTH`), debounced
                                        300ms, calling `searchUsersAction({searchValue, pageNumber: 1, pageSize: 10})`
                                        for page 1 of matches only. Does not take a `users` prop at all.
                                        `onValueChange` is `(user: UserDto) => void` — hands back the full selected record, not just the
                                        id, so callers needing the display name (e.g. send-notification-dialog.tsx's
                                        "From" field) don't need a second lookup; `value` itself is still the
                                        selected id (string), used for the hidden `<input>` and the checkmark compare.
                                        Feature-owned, consumed by both notifications-data-table.tsx and
                                        send-notification-dialog.tsx within this same feature; not promoted to
                                        components/shared/, consistent with "only promote once 2+ features need it"
features/notifications/api/notifications.api.ts -> lib/server/backend-api, lib/server/call-guard, types/api,
                                        features/notifications/types/notification — new this sync, consolidates the
                                        former get-notifications.ts + send-notification.ts (2 files) into one,
                                        exporting getNotifications (GET notification, admin search/list) and
                                        sendNotification (POST notification, admin send); SendNotificationRequest's
                                        type definition moved out of this file into features/notifications/types/notification.ts
features/notifications/api/user-notifications.api.ts -> lib/server/backend-api, lib/server/call-guard, types/api,
                                        features/notifications/types/notification — new this sync, consolidates the
                                        former get-my-notifications.ts + get-unread-count.ts + mark-notification-read.ts
                                        (3 files) into one, exporting getMyNotifications (GET user_notification,
                                        self-scoped), getUnreadCount (GET user_notification/count_unread), and
                                        markNotificationRead (GET user_notification/{id}, marks-as-read as a side effect)
features/notifications/api/get-my-notifications-action.ts -> "use server"; features/user-profile (resolveSession),
                                        features/notifications/api/user-notifications.api (getMyNotifications —
                                        changed this sync, was features/notifications/api/get-my-notifications):
                                        signature is `(params: Omit<NotificationLookupParams, "toUserId"> = {})`,
                                        forwarded straight through to the already-parameterized getMyNotifications();
                                        fully backward-compatible with existing no-arg callers (use-notifications.ts's
                                        refresh()). Callers: features/home/components/home-page.tsx
                                        (first page, unfiltered) and features/notifications/components/
                                        notification-inbox.tsx (paged/filtered)
features/notifications/api/get-unread-count-action.ts -> "use server"; features/user-profile (resolveSession),
                                        features/notifications/api/user-notifications.api (getUnreadCount —
                                        changed this sync, was features/notifications/api/get-unread-count) — swallows
                                        any failure to `0` rather than surfacing an error (fire-and-forget-safe shape)
features/notifications/api/mark-notification-read-action.ts -> "use server"; features/user-profile (resolveSession),
                                        features/notifications/api/user-notifications.api (markNotificationRead —
                                        changed this sync, was features/notifications/api/mark-notification-read) —
                                        returns a boolean rather than {error?, success?};
                                        deliberately did NOT gain revalidatePath — its only caller is the
                                        client-only notification bell/inbox hook, which manages its own state, so
                                        there's no Server-Component page depending on route cache here (a no-op if added)
features/notifications/api/send-notification-action.ts -> "use server"; features/user-profile (resolveSession),
                                        features/notifications/api/notifications.api (sendNotification — changed this
                                        sync, was features/notifications/api/send-notification), lib/shared/user-display
                                        (getDisplayName), next/cache (revalidatePath, "/notifications")
features/notifications/api/get-signalr-token-action.ts -> "use server"; features/user-profile (resolveSession),
                                        lib/server/refresh-session (refreshSessionIfNearExpiry),
                                        lib/server/persist-session-cookie (persistSessionCookie) —
                                        proactively refreshes a near-expiry session before handing back its
                                        access token (proxy.ts's own proactive-refresh middleware excludes `/api`
                                        paths, so a session left open on one page without navigating could
                                        otherwise hand SignalR a stale/expired token and 401), persisting the
                                        rotated session via persistSessionCookie() when a refresh happened; still
                                        the one deliberate place the token leaves the httpOnly cookie boundary

lib/server/require-permission.tsx (new this sync) -> next/navigation (redirect), components/shared/access-denied
                                        (AccessDenied), features/user-profile (resolveSession, barrel), lib/server/authorization
                                        (hasPermission), types/session (SessionData) — exports `requirePermission(permission)`:
                                        resolves the session (redirects to `/login` if none), checks `permission` via
                                        `hasPermission`, returns `{session, denied}` (`denied` is `null` on success or
                                        an `<AccessDenied>` element on failure). Consumed by users-page.tsx,
                                        roles-page.tsx, notifications-page.tsx (see their rows above)
components/shared/access-denied.tsx (new this sync) -> lucide-react (ShieldOff), components/ui/empty — `AccessDenied({permission})`,
                                        a presentational-only component (no feature/session dependency of its own);
                                        the only consumer is lib/server/require-permission.tsx

components/foundation/use-listbox.ts     -> @floating-ui/react (useListNavigation, useTypeahead) — `enableTypeahead`
                                        option lets text-input callers disable it; sole remaining consumer is
                                        components/command/command-palette.tsx (use-floating-popover.ts,
                                        options-list.tsx, use-async-options.ts, and types.ts, whose only
                                        consumer was components/select/*, were deleted this sync)
components/foundation/use-virtual-list.ts -> @tanstack/react-virtual (useVirtualizer) — consumers are now
                                        components/command/command-palette.tsx and components/shared/
                                        data-table/data-table-virtual-body.tsx only (options-list.tsx, a
                                        third former consumer, was deleted this sync)
components/foundation/floating-overlay.tsx -> @floating-ui/react (FloatingPortal, FloatingOverlay, FloatingFocusManager),
                                        lib/shared/utils — modal backdrop + focus trap with `lockScroll={false}`;
                                        used only by components/command/command-palette.tsx
components/command/command-palette.tsx    -> @floating-ui/react (useFloating, useDismiss, useRole, useInteractions),
                                        lucide-react, lib/shared/utils, components/foundation/{floating-overlay,
                                        use-listbox,use-virtual-list} — Cmd/Ctrl+K overlay; still no UI consumer
components/command/command-palette-provider.tsx -> components/command/command-palette — owns the global
                                        Cmd/Ctrl+K keydown listener + open state via React Context
components/command/index.ts               -> re-exports CommandPalette/CommandPaletteProvider/useCommandPalette + types

components/shared/data-table/data-table.tsx -> components/ui/{table,skeleton,empty,alert}, ./data-table-toolbar,
                                        ./data-table-buttons (new this sync), ./data-table-pagination,
                                        ./data-table-virtual-body, ./types — computes the Export/Refresh/Columns
                                        buttons node itself via DataTableButtons and always wraps its content
                                        (Table/Empty/Alert/virtualized body) in a `rounded-md border border-border`
                                        box; when `customSearch` is used, that buttons node renders inside this box
                                        (right-aligned, above the content) instead of being passed to the toolbar
components/shared/data-table/data-table-buttons.tsx -> components/ui/{button,button-group,dropdown-menu},
                                        lib/shared/utils, ./types — new this sync; builds the Export/Refresh/Columns
                                        cluster (wrapped in the new components/ui/button-group.tsx), extracted out
                                        of data-table-toolbar.tsx
components/shared/data-table/data-table-virtual-body.tsx -> lib/shared/utils, components/foundation/use-virtual-list,
                                        ./types — row renderer for `mode="virtualized"`/`"infinite"`; switches to an
                                        ARIA-grid div layout since a native `<table>` can't virtualize rows cleanly
components/shared/data-table/data-table-toolbar.tsx -> components/ui/{button,separator}, lucide-react (Search),
                                        lib/shared/utils, ./types — restructured this sync into three stacked
                                        sections (actions / search / an inserted Separator between rendered
                                        sections); no longer imports dropdown-menu (button-cluster building moved
                                        to data-table-buttons.tsx); gained `customSearch`/`onCustomSearch` props
                                        plus a `buttons` prop rendered by the caller (data-table.tsx)
components/shared/data-table/data-table-pagination.tsx -> components/ui/{input,pagination}
components/ui/button-group.tsx (new this sync) -> lib/shared/utils, components/ui/separator — `ButtonGroup`/
                                        `ButtonGroupSeparator`, visually connects adjacent buttons via shared
                                        borders and end-only rounding; consumed by data-table-buttons.tsx

components/toast/toaster.tsx        -> next-themes, sonner, ./toast-theme
components/toast/notify.ts          -> sonner
components/toast/toast-theme.ts     -> sonner (types only)

lib/server/session.ts               -> next/headers (cookies), lib/server/parse-session, lib/server/session-cookie, types/session
lib/server/parse-session.ts         -> lib/server/token-cipher (decrypt), types/session (new)
lib/server/refresh-session.ts       -> features/auth/api/token.api (refreshToken, direct file, not the barrel —
                                        changed this sync, was features/auth/api/refresh-token), lib/server/jwt,
                                        lib/server/session-cookie (REFRESH_LEAD_MS — moved in
                                        from proxy.ts's own inline check), types/session — exports `refreshSession()`
                                        (unconditional rotate) and `refreshSessionIfNearExpiry(session)`
                                        (gates the rotate on REFRESH_LEAD_MS, returns null both when not due and when
                                        attempted and failed); called from src/proxy.ts and
                                        features/notifications/api/get-signalr-token-action.ts
lib/server/persist-session-cookie.ts (new) -> next/headers (cookies), lib/server/token-cipher (encrypt),
                                        lib/server/session-cookie (SESSION_COOKIE_NAME, buildSessionCookieOptions),
                                        types/session — Server-Action-context-only cookie writer; not importable
                                        from proxy.ts (middleware/Edge context uses NextRequest/NextResponse cookie
                                        APIs instead, incompatible with next/headers's cookies()); consumed by
                                        features/notifications/api/get-signalr-token-action.ts and
                                        features/auth/api/refresh-session-action.ts, replacing each file's own
                                        hand-rolled cookies().set(...) call
lib/server/token-cipher.ts          -> node:crypto, lib/server/config (getTokenEncryptionKey) — AES-256-GCM (new)
lib/server/jwt.ts                   -> types/claim (ClaimDto) — decodes a JWT payload without verifying its signature;
                                        no other lib/server/* dependency (new)
lib/server/build-session-claims.ts  -> lib/server/jwt (extractAllClaims), lib/shared/dedupe-claims, types/claim (new)
lib/server/http.ts                  -> lib/server/config (getApiBaseUrl), lib/server/api-clients (ApiClientName,
                                        default ApiClients.Backend) — changed this sync: no longer accepts an
                                        `accessToken` option; `RequestOptions` now has `handlers?: HttpRequestHandler[]`
                                        (run in order against a shared `headers` object before fetch()) and
                                        `client?: ApiClientName`
lib/server/backend-api.ts (new)     -> lib/server/http (requestJson/requestVoid, re-exported), lib/server/
                                        http-handlers/bearer-token-handler (bearerTokenHandler), lib/server/
                                        api-clients (ApiClients.Backend) — the entry point nearly every
                                        features/*/api/*.ts file now uses instead of lib/server/http directly;
                                        auto-injects `handlers: [bearerTokenHandler, ...]` and `client: ApiClients.Backend`
lib/server/api-clients.ts (new)     -> no internal dependency — `ApiClients = { Backend: "Backend" }`, `ApiClientName`;
                                        a named-client registry with room for more entries later (only one exists today)
lib/server/http-handlers/bearer-token-handler.ts (new) -> lib/server/session (getSession), lib/server/http (type
                                        HttpRequestHandler) — exports `bearerTokenHandler` (reads the ambient session,
                                        sets Authorization if present) and `explicitBearerTokenHandler(accessToken)`
                                        (a factory for the pre-session-cookie call sites, all now consolidated into
                                        two files: token.api.ts's getToken/refreshToken, and user-profile.api.ts's
                                        getCurrentUser — was login.ts, refresh-token.ts, get-current-user.ts)
lib/server/config.ts                -> lib/server/api-clients (ApiClientName) — changed this sync: `getApiBaseUrl()`
                                        now takes `client: ApiClientName = ApiClients.Backend`, resolving the env var
                                        per-client via an internal map (`Backend` -> `API_BASE_URL`, the only entry today)
lib/server/authorization.ts         -> lib/shared/authorization (delegates to it), types/session (SessionData) —
                                        a thin wrapper deriving `session.profile?.userName` internally and calling
                                        through to `lib/shared/authorization.ts`. **Changed this sync**: `hasPermission`/
                                        `hasAnyPermission`/`hasAllPermissions` dropped their separate `userName`
                                        parameter — call sites are now `hasPermission(session, permission)`/
                                        `hasAnyPermission(session, permissions)`/`hasAllPermissions(session, permissions)`,
                                        was `(session, userName, permission)`
lib/shared/authorization.ts         -> no internal dependency (SUPER_ADMIN_USERNAMES, isSuperAdminUser,
                                        hasPermission/hasAnyPermission/hasAllPermissions; new — safe for both server
                                        and client, takes `permissions: string[]`/`userName` directly rather than a
                                        `SessionData`-shaped object, which is what makes it usable from Sidebar).
                                        Unchanged this sync — only its `lib/server/authorization.ts` wrapper's own
                                        call-site signature changed
lib/shared/menu.ts                  -> types/nav (NavItem) — new; pure recursive `buildVisibleMenu(items, can)`
components/ui/*                     -> lib/shared/utils, radix-ui, class-variance-authority, lucide-react
                                        (button.tsx additionally -> components/ui/spinner). Exception:
                                        native-select.tsx -> lib/shared/utils, components/ui/{label,spinner},
                                        lucide-react only — no radix-ui dependency, hand-written rather than
                                        shadcn-CLI-generated
components/ui/popover.tsx (new)     -> radix-ui (Popover), lib/shared/utils, components/foundation/
                                        portal-container (usePortalContainer) — the one components/ui/*
                                        file besides dialog.tsx that reaches into components/foundation/,
                                        a deliberate, narrow exception to "components/ui/* is a strict leaf
                                        layer" (see Key Design Patterns and Dependency Direction below)
components/ui/command.tsx (new)     -> cmdk (new dependency), lucide-react, lib/shared/utils — no
                                        components/foundation/ or components/select/ dependency
components/ui/combobox.tsx (new)    -> components/ui/{button,popover,command}, lucide-react, lib/shared/
                                        utils — `Combobox<TValue>`, the shadcn-style replacement for
                                        components/select/*'s EntitySelect/SearchSelect (see Key Design
                                        Patterns)
components/ui/dialog.tsx (changed)  -> adds components/foundation/portal-container (PortalContainerProvider)
                                        to its existing radix-ui/lib/shared/utils/components/ui/button
                                        dependencies — DialogContent now captures its own DOM node and
                                        provides it via context so a nested Popover can portal inside it
                                        (see Key Design Patterns). **Changed this sync**: `DialogContent` also
                                        gained `max-h-[calc(100vh-2rem)] overflow-y-auto` — no new dependency,
                                        pure className fix (see Key Design Patterns)
components/ui/tabs.tsx (changed)    -> no dependency change — `TabsTrigger` gained `cursor-pointer` in its
                                        className (see Key Design Patterns)
components/foundation/portal-container.ts (new) -> react (createContext/useContext) only — no
                                        components/ui/ or components/foundation/ dependency of its own;
                                        consumed by components/ui/{dialog,popover}.tsx
```

Direction is still one-way: `components/ui/*` never imports from `components/layout/*`, `components/theme/*`, or `features/*`. Cross-feature imports go through a feature's `index.ts` barrel, not its internals — e.g. `features/auth/api/login-action.ts` and `features/users/components/users-page.tsx`/`api/create-user-action.ts` all import `resolveSession`/`getCurrentUser` from `@/features/user-profile` (the barrel), not from its internals directly. `features/dashboard` is still not imported by any other feature. `features/users` and `features/roles` are each imported by their own `app/**/page.tsx` (routing, not another feature). `components/shared/data-table/*`, `components/shared/object-viewer/*`, `components/shared/access-denied.tsx`, and `components/toast/*` sit below `features/*` in the same leaf-adjacent tier as `components/ui/*` — they're imported by feature code (or, for `access-denied.tsx`, by `lib/server/require-permission.tsx`) but import nothing from `features/*` themselves (`object-viewer/*` currently has no importer at all — purely additive). `components/foundation/*` and `components/command/*` sit at that same tier too — `foundation/*` imports only `@floating-ui/react`/`@tanstack/react-virtual`/`react`, `command/*` imports only `foundation/*` plus `@floating-ui/react`/`lucide-react`, and neither imports from `features/*`. `command/*` still has no importer at all outside this library itself — purely additive, same as `object-viewer/*`. The one narrow exception to "`components/ui/*` never imports from another component tier": `components/ui/dialog.tsx` and `components/ui/popover.tsx` both import `components/foundation/portal-container.ts` — a plain React Context with no further dependency of its own, added specifically so a `Popover`/`Combobox` nested inside a `Dialog` can portal into the Dialog's own DOM node (see Key Design Patterns). `components/select/*` — the library `AsyncSelect`/`MultiSelect` lived in — is gone entirely, not just unimported.

**Changed this sync**: the `features/users -> features/roles` (role catalog) and `features/notifications -> features/users` (`getAllUsers`) cross-feature barrel edges from a previous sync are both gone. `features/users/components/users-page.tsx` no longer imports `@/features/roles` at all — the role picklist moved to `features/users/components/edit-user-dialog.tsx`, which imports `@/features/roles/api/get-all-roles-action` directly (a barrel-bypass exception, see below). `features/notifications/components/notifications-page.tsx` no longer imports `@/features/users` at all — the recipient search moved to `features/notifications/components/user-select.tsx`, which imports `@/features/users/api/search-users-action` directly (another barrel-bypass exception). Both replacements are component-level, direct-file, cross-feature imports rather than page-level barrel imports.

Six deliberate, narrow exceptions to the barrel-only rule exist, all driven by the RSC client/server boundary rather than an oversight: `constants/nav-items.ts` imports `HOME_NAV_ITEM`/`USERS_NAV_ITEM`/`ROLES_NAV_ITEM`/`NOTIFICATIONS_NAV_ITEM` by direct file path (`@/features/home/constants/nav-item`, etc. — `home` replaced `dashboard`'s `DASHBOARD_NAV_ITEM`) rather than via each feature's barrel; `components/layout/user-menu.tsx` imports `logoutAction` directly from `@/features/auth/api/logout-action` rather than from `@/features/auth`'s barrel (which, notably, does not currently re-export `logoutAction` at all); `components/layout/topbar.tsx` imports `NotificationBell` directly from `@/features/notifications/components/notification-bell` rather than from `@/features/notifications`'s barrel — here the barrel *does* also export `NotificationBell`, but it additionally re-exports `NotificationsPage`, an async Server Component (`resolveSession()`, `next/headers`), so importing the barrel from this Client Component reproduced the same "next/headers only available in Server Components" build error the `nav-items.ts` exception was already working around; `features/users/components/edit-user-dialog.tsx` imports `getAllRolesAction` directly from `@/features/roles/api/get-all-roles-action` rather than `@/features/roles`'s barrel, and `features/notifications/components/user-select.tsx` imports `searchUsersAction` directly from `@/features/users/api/search-users-action` rather than `@/features/users`'s barrel — both for the same reason as the others: the target barrel also re-exports an async Server Component (`RolesPage`/`UsersPage` respectively) that calls `resolveSession()`/reads cookies via `next/headers`, so importing the full barrel from these `"use client"` components would drag that server-only chain into the client bundle; and `features/home/components/home-page.tsx` — itself an async Server Component, so not subject to the client-bundle constraint above — imports `NotificationInbox` directly from `@/features/notifications/components/notification-inbox` even though `@/features/notifications`'s barrel does export it (no comment in the source explains why; noted here as an observed fact, not a confirmed rationale). It also imports `getMyNotificationsAction` directly from `@/features/notifications/api/get-my-notifications-action` — that one isn't a stylistic bypass, since the barrel doesn't export that action at all. See Key Design Patterns for the reasoning behind the other five.

## Key Design Patterns

- **Feature-folder + barrel-export convention**: each `features/<name>/` owns `api/` (one consolidated `<feature>.api.ts` file per feature, wrapping every backend call that feature makes — changed this sync from the previous one-file-per-endpoint convention; Server Actions remain one file per action), `components/`, optionally `types/` — either single-consumer (`features/roles/types/{role,permission-definition}.ts`, `features/user-profile/types/user-session.ts`) or feature-owned DTOs with multiple consumers inside and outside the feature, re-exported through the barrel for cross-feature use (`features/users/types/user.ts` — `UserDto`/`CreateUserRequest`/`SearchUsersParams`; `features/auth/types/token.ts` — `TokenDto`/`GetTokenRequest`/`RefreshTokenRequest`/`DeviceDto`; `features/notifications/types/notification.ts` — also now home to `SendNotificationRequest`, relocated out of the notifications api file this sync) — and optionally `constants/` (a feature-owned permission-string file, e.g. `features/users/constants/permissions.ts`, and a `nav-item.ts` per nav-bearing feature), and an `index.ts` that is the only sanctioned import surface for other features or `app/*`. **Six narrow, deliberate exceptions** to the barrel-only rule exist (see Dependency Direction): `constants/nav-items.ts`, `components/layout/user-menu.tsx`, `components/layout/topbar.tsx`, `features/users/components/edit-user-dialog.tsx` (imports `@/features/roles/api/get-all-roles-action`), `features/notifications/components/user-select.tsx` (imports `@/features/users/api/search-users-action`), and `features/home/components/home-page.tsx` (imports `@/features/notifications/components/notification-inbox` and `@/features/notifications/api/get-my-notifications-action` directly) each import one specific file directly rather than through a barrel, to avoid pulling a barrel's other, server-only exports (async Server Components, cookie-reading API functions) into a client bundle — except `get-my-notifications-action`, which isn't in the notifications barrel at all.
- **API files consolidated one-per-feature this sync, Server Actions unchanged**: every `features/<name>/api/` folder used to have one file per backend-calling function (e.g. `get-all-users.ts`, `create-user.ts`, `login.ts`). Those per-function files were deleted and merged into a single `<feature>.api.ts` file per feature — `features/auth/api/token.api.ts` (`getToken`, renamed from `login`; `refreshToken`), `features/users/api/users.api.ts` (8 functions), `features/roles/api/roles.api.ts` (6 functions), `features/notifications/api/notifications.api.ts` (the admin-facing `notification`-route functions, `getNotifications`/`sendNotification`) plus a separate new `features/notifications/api/user-notifications.api.ts` (the self-service `user_notification`-route functions, `getMyNotifications`/`getUnreadCount`/`markNotificationRead`), and `features/user-profile/api/user-profile.api.ts` (`getCurrentUser`/`listSessions`/`revokeSession`). `features/user-profile/api/resolve-session.ts` was deliberately left as its own file — it only reads the local session cookie, never calls the backend. `*-action.ts` Server Action files were **not** merged; they remain one file per action, only their import path changed to point at the new consolidated file. No HTTP method/route/query/body changed for any function, and no new features/routes/UI were added — this was a pure file reorganization plus one rename (`login` → `getToken`) and one type relocation (`SendNotificationRequest` moved from the notifications api file into `types/notification.ts`).
- **Each nav-bearing feature owns its own `NavItem` metadata**: `features/{home,users,roles,notifications}/constants/nav-item.ts` each export one `NavItem` (label, href, icon, and — where relevant — the permission that gates it), re-exported from that feature's barrel. `constants/nav-items.ts` imports these four constants **by direct file path**, not via each feature's barrel, and assembles them into `NAV_ITEMS` alongside two nodes it still declares itself (a group node, since it spans two features — see below — and "Settings", which has no owning feature or page at all). The direct-file-path import is intentional, not an oversight: `nav-items.ts` is imported by the client-side `Sidebar` component, and `features/users/index.ts`/`features/roles/index.ts`'s barrels also re-export server-only code (`UsersPage`/`RolesPage`, async Server Components calling `resolveSession()`, which reads cookies via `next/headers`) — importing the full barrel from client code would drag that server-only chain into the client bundle (this was tried and produced a real "next/headers only available in Server Components" build error before being fixed this way). The `nav-item.ts` files themselves are plain data (an icon reference plus strings) with no server/client-bound dependency, so importing them directly is safe. `components/layout/sidebar.tsx` then computes the permission-filtered menu client-side via `lib/shared/menu.ts`'s `buildVisibleMenu(NAV_ITEMS, can)`, where `can` is built from `lib/shared/authorization.ts` (`hasPermission`) using the `permissions`/`userName` props `AppShell` passes down from `resolveSession()`. `dashboard`/`DASHBOARD_NAV_ITEM` is gone, replaced by `home`/`HOME_NAV_ITEM` as the tree's first (non-grouped) entry. The group node itself — declared directly in `nav-items.ts`, not owned by a feature — is labeled "Administration" (`/administration`, icon `ShieldCog`), and nests `NOTIFICATIONS_NAV_ITEM` inside its `children` array alongside `USERS_NAV_ITEM`/`ROLES_NAV_ITEM`.
- **Fetch full detail on dialog open, when the list endpoint's DTO is incomplete — generalized to self-fetching the picklist too**: both `edit-user-dialog.tsx` and `edit-role-dialog.tsx` call a dedicated `get-*-detail-action.ts` in a `useEffect` on mount rather than trusting the row data they were opened with. This exists because the backend's list-returning service methods (`UserService.SearchAsync`/`GetAllAsync`, `RoleService.GetAllAsync` — their shared `DataMapper.cs` projection) never populate `Roles`/`Claims`; only the single-record fetch (`GetByIdAsync`) does. Relying on the row data silently produced an empty roles/claims checklist that, on save, would have wiped out anything the record actually had — worth remembering before building another list-backed feature against this backend. The same "fetch on open, don't trust what the list page preloaded" idea also covers the *picklist itself* — `users-page.tsx`/`roles-page.tsx` no longer fetch the role catalog/permission catalog and pass them down as props; instead each edit dialog runs `Promise.all([<detail fetch>, <picklist fetch>])` on open (`getAllRolesAction()`/`getPermissionsAction()` respectively), so both list pages issue one fewer API call on page load.
- **Shared `requirePermission`/`AccessDenied` pattern for page-level permission gating, replacing three previously separate implementations**: `lib/server/require-permission.tsx`'s `requirePermission(permission)` resolves the session (redirecting to `/login` if none, via `resolveSession()`/`redirect()`), checks `permission` via `lib/server/authorization.ts`'s `hasPermission`, and returns `{ session, denied }` — `denied` is `null` on success or a `components/shared/access-denied.tsx` (`AccessDenied`) element on failure. A page calls it once at the top and does `if (denied) return denied;` before its own data fetch, mirroring the shape of the inline check it replaces. Before this, `users-page.tsx` and `roles-page.tsx` each called `resolveSession()`/checked the permission/rendered their own inline `Empty` block for a missing `*.View` permission, while `notifications-page.tsx` instead called `next/navigation`'s `redirect()` on a missing `NOTIFICATIONS_PERMISSIONS.Read` — three different implementations of the same idea. **Real behavior change**: `notifications-page.tsx` no longer redirects on a missing view permission; it now renders the same shared `AccessDenied` panel as Users/Roles, so all three pages behave identically. Supporting this, `lib/server/authorization.ts`'s `hasPermission`/`hasAnyPermission`/`hasAllPermissions` dropped their separate `userName` parameter — call sites are now `hasPermission(session, permission)` rather than `hasPermission(session, userName, permission)`, deriving `userName` from `session.profile?.userName` internally; the underlying client-safe `lib/shared/authorization.ts` is unchanged. `lib/server/require-permission.tsx` is the one `lib/server/*` module that returns JSX (a `.tsx` file, unlike its flat-file siblings).
- **On-demand user search, replacing a preloaded full user list**: `features/notifications/components/user-select.tsx` no longer takes a `users` prop or does client-side filtering over a preloaded list. It debounces (300ms) a call to `searchUsersAction({searchValue, pageNumber: 1, pageSize: 10})` once the trimmed query reaches a minimum length (`MIN_SEARCH_LENGTH = 3`, checked via `trimmedQuery.length < MIN_SEARCH_LENGTH`, matching the component's own placeholder/comment), fetching only the first page of matches — never the whole user list. Its `onValueChange` hands back the full selected `UserDto`, not just an id, so a caller needing the display name too (`send-notification-dialog.tsx`'s "From" field) doesn't need a second lookup.
- **On-demand domain-user (AD) lookup with silent-miss autofill**: `create-user-dialog.tsx`'s Username field backs a lookup against `get-domain-user-action.ts` (`GET user/get_domain_user/{userName}`, wrapping the backend's `IActiveDirectoryService.GetByUserNameAsync`; the underlying call now lives in `users.api.ts`'s `getDomainUser`, consolidated from a former standalone `get-domain-user.ts`) — fired either by a small icon button merged into the input's own right edge (`SearchIcon`/`Spinner`, `size="icon-xs"`, absolutely positioned), or automatically on `onBlur` once the trimmed value is at least 3 characters and different from the last one looked up (tracked in a `useRef`, not a debounced `useEffect` — deliberately not per-keystroke, a different shape from `user-select.tsx`'s typeahead search above). A match autofills First/Last name, Email, Phone, sets a new `authProvider` field to `"AD"`, and shows an inline green message under the Username field (no toast — see the Toast notifications pattern below for the one deliberate exception); a miss is treated as "this will be a local account" rather than an error — it silently resets those same autofilled fields (including `authProvider`) back to blank, with no message shown at all. Password becomes conditionally optional as a result (`required={values.authProvider !== "AD"}` client-side), and `create-user-action.ts`'s own server-side validation was updated to match (`!password && authProvider !== "AD"`) — the client-side relaxation alone had briefly shipped ahead of the server action and reintroduced the "Username and password are required." error for AD selections until both were reconciled. The options backing the new Auth Provider field live in `features/users/constants/auth-provider.ts` (`AUTH_PROVIDER_SELECT_OPTIONS`), which represents Local as an **empty string** rather than the literal `"Local"` used before — matching the backend's actual convention (`User.ChangeAuthProvider` treats `""`/`null` as "local," and the `AuthProvider` enum itself only defines `AD`; see `src/Identity.Api/Entities/User.cs`/`src/Identity.Contracts/AuthProvider.cs`). `edit-user-dialog.tsx` was migrated onto this same shared constant (it previously defined its own identical-looking `AUTH_PROVIDER_OPTIONS`/`_SELECT_OPTIONS` locally) — its default state and `isLocalAccount` check changed from `"Local"`/`.toLowerCase() === "local"` to `""`/`authProvider === ""` to match, fixing a latent bug where saving an unchanged "Local" selection would have written the literal string `"Local"` into the backend's `AuthProvider` column instead of `null`.
- **Routing files are pure re-exports**: every `app/**/page.tsx` is a one-line `export { X as default } from "@/features/<name>";` — no logic lives in `app/`.
- **Server-only API layer, one consolidated file per feature**: `lib/server/http.ts` (`requestJson`/`requestVoid`) is the single fetch wrapper; each `features/*/api/<feature>.api.ts` file wraps every backend call for that feature and returns a normalized result via `lib/server/call-guard.ts`'s `guardCall`/`guardResponseCall`/`guardRawCall` (chosen based on whether the backend endpoint returns a `Result<T>` envelope, a bare `ApiResponse`, or a raw value/array). **Changed this sync**: this replaces the previous one-file-per-endpoint convention; `*-action.ts` Server Action files are unaffected, remaining one file per action.
- **Encrypted cookie session, refreshed proactively by `proxy.ts`, no per-request live fetch**: `lib/server/session.ts`'s `getSession()` reads and decrypts the `admin_session` cookie; `features/user-profile/api/resolve-session.ts` is a thin passthrough to it, rather than composing it with its own live `getCurrentUser()` call. All the "keep this session fresh" work lives in `src/proxy.ts` instead: on every request it decrypts/validates the cookie (`lib/server/parse-session.ts`), proactively rotates the access/refresh token when close to expiry (`lib/server/refresh-session.ts`, calling the backend's `token/token/refresh` via `features/auth/api/token.api.ts`'s `refreshToken`), and refetches profile/claims on hard navigations or right after a rotation — writing the result back as a freshly-encrypted cookie on both the request and the response. This replaces the older design (`resolveSession()` doing a live fetch on every server-rendered request that needed it) with a single, centralized refresh point.
- **Session encryption via a dedicated `lib/server/token-cipher.ts`**: `encrypt()`/`decrypt()` wrap Node's `crypto` module (AES-256-GCM, keyed by `TOKEN_ENCRYPTION_KEY`), producing an `"iv.authTag.ciphertext"` (all base64) string; `decrypt()` returns `null` rather than throwing on any malformed/tampered input, which `parse-session.ts` treats the same as "no session". `lib/server/config.ts`'s `getTokenEncryptionKey()` throws if the env var is unset.
- **Session near-expiry refresh and cookie-persist logic centralized into two shared helpers, replacing three separate hand-rolled copies**: `lib/server/refresh-session.ts`'s `refreshSessionIfNearExpiry(session)` — wraps `refreshSession()` (unconditional rotate), gated on `session.expiresAt - Date.now() <= REFRESH_LEAD_MS`, returning `null` both when a refresh wasn't due and when it was attempted and failed. `src/proxy.ts` calls this instead of its own inline `REFRESH_LEAD_MS` check + `refreshSession()` call (pure dedup — `REFRESH_LEAD_MS` moved from being imported directly by `proxy.ts` to living behind this helper); `features/notifications/api/get-signalr-token-action.ts` calls it too, proactively refreshing before handing an access token to the browser for the SignalR handshake — closing a gap where `proxy.ts`'s own middleware matcher excludes `/api` paths, so a session left open on one page (no navigation) could otherwise hand SignalR a stale token and get a 401. `lib/server/persist-session-cookie.ts` (`persistSessionCookie(session)`) similarly centralizes the Server-Action-context cookie write (`cookies()` from `next/headers`, `buildSessionCookieOptions()`), replacing hand-rolled `cookies().set(...)` calls in `get-signalr-token-action.ts` and `features/auth/api/refresh-session-action.ts` (the manual, super-admin-gated refresh action, which still calls `refreshSession()` directly/unconditionally rather than the near-expiry-gated helper, since it's an explicit user-triggered rotate, not a proactive one) — deliberately not importable from `proxy.ts`, whose middleware/Edge-adjacent context uses `NextRequest`/`NextResponse` cookie APIs instead of `next/headers`.
- **Permissions/roles decoded from the JWT, never trusted from the profile API**: `lib/server/jwt.ts` decodes the access token's payload (no signature verification — safe here since the token was just issued by this app's own backend) and extracts the `permission`/`role` claim types; `lib/server/build-session-claims.ts` unions those with the profile API's own claims for display purposes only. Both `loginAction` and `refreshSession()` independently re-derive `permissions`/`roles` this way on every token issuance.
- **Context-provider-per-concern for client state**: `SidebarProvider`/`useSidebar`, `AccentColorProvider`/`useAccentColor`, and `next-themes`' provider (wrapped in `components/theme/theme-provider.tsx`) each own one slice of persisted UI state via a throwing custom hook. `features/notifications/context/notifications-provider.tsx`'s `NotificationsProvider`/`useNotificationsContext` follows the same shape (call the underlying hook exactly once, expose its return value via Context, throw from the consumer hook if called outside the provider), but unlike the other three it isn't persisted to `localStorage` — its "state" is live server data plus an open SignalR connection, not client UI preference. Mounted once in `components/layout/app-shell.tsx` (nested inside `SidebarProvider`) so `notification-bell.tsx` (topbar) and `notification-inbox.tsx` (Home page) share one connection/`unreadCount` instead of each opening its own.
- **Owned, CLI-generated UI primitives**: `components/ui/*` (23 shadcn-CLI-generated files, style `"radix-nova"`) still follows the `data-slot="<name>"` + `cva()` convention. `button.tsx` retains its hand-modification beyond CLI output: a `loading` prop (renders `Spinner`, sets `aria-busy`/`disabled`) plus a `cursor-pointer` utility baked into `buttonVariants`. `native-select.tsx` is the one hand-written exception in this folder — see the next pattern for why it and the rest of the new select family aren't CLI/Radix-based. `tabs.tsx`'s `TabsTrigger` gained the same `cursor-pointer` treatment `button.tsx` already had — Tailwind's Preflight resets `<button>` to `cursor: default`, and `TabsTrigger` had never been given the override, so every `Tabs` usage (`edit-user-dialog.tsx`, `notification-inbox.tsx`, `notification-bell.tsx`) showed the arrow cursor on hover instead of a pointer. `dialog.tsx`'s `DialogContent` gained `max-h-[calc(100vh-2rem)] overflow-y-auto` — it previously had no height constraint at all, so a dialog taller than a short viewport (e.g. `EditUserDialog`'s tabs + roles + claims) would overflow off-screen with no way to scroll down to its own footer buttons; this scrolls the whole dialog (header + body + footer together) rather than pinning the footer, a deliberately simpler scope than a sticky-footer redesign.
- **The internal Floating-UI select library from a previous sync is gone; single-select goes through a shadcn-style `Combobox`**: `components/ui/combobox.tsx` (`Combobox<TValue>`) composes `components/ui/{button,popover,command}.tsx` — `popover.tsx` is a Radix `Popover` wrapper (`data-slot`/`cn` conventions matching the rest of `ui/`), `command.tsx` wraps the `cmdk` dependency for the filterable list. This replaces both `EntitySelect` (button-triggered, non-searchable) and `SearchSelect` (text-input-triggered, filterable) with one component — `Command`'s built-in search input covers both cases, so the two consumers (`edit-user-dialog.tsx`'s status/authProvider, `features/notifications/user-select.tsx`) needed no prop changes beyond the import. `AsyncSelect`/`MultiSelect` were deleted rather than ported — neither ever had a UI consumer, so there was nothing to preserve; a remote-search or multi-value variant can be built on `Combobox` if a future feature needs one. `components/command/*` (Command Palette) still builds directly on `components/foundation/*`/`@floating-ui/react`, unrelated to and untouched by this change — it still reuses `floating-overlay.tsx` rather than Radix `Dialog` specifically because Radix `Dialog` locks body scroll by default and the Palette's overlay explicitly must not (`FloatingOverlay`'s `lockScroll={false}`).
- **A Popover nested inside a Dialog couldn't be scrolled with the mouse wheel — fixed via a shared portal-container context**: Radix `Dialog`'s modal scroll lock (`react-remove-scroll`, active while the dialog is open) only treats content that is an actual DOM descendant of `DialogContent`'s own node as "inside" the locked region; anything portaled elsewhere — which is what `Popover.Portal` does by default (`document.body`) — has its wheel/touch scroll blocked as if it were page background, even though it renders visually on top and in the right place. The fix has two parts, both in `components/ui/dialog.tsx`: (1) `DialogContent` now centers via a `flex items-center justify-center` wrapper `div` instead of a `transform` (`-translate-x-1/2 -translate-y-1/2`) on the content node itself — a `transform` on an ancestor creates a new CSS containing block for `position: fixed` descendants, which would have broken a nested Popover's floating-position math the moment its portal target moved inside `DialogContent`; the wrapper is `pointer-events-none` with `pointer-events-auto` on the content itself, so backdrop clicks still reach `DialogOverlay` exactly as before. (2) `DialogContent` captures its own DOM node (`ref={setPortalNode}`) and provides it through `components/foundation/portal-container.ts` context (`PortalContainerProvider`/`usePortalContainer`); `components/ui/popover.tsx`'s `PopoverContent` reads that context and passes it as `Popover.Portal`'s `container` prop, falling back to Radix's own `document.body` default outside a Dialog. This is automatic for any future `Combobox`/`Popover` nested in a `Dialog` — no per-call-site wiring needed, and `send-notification-dialog.tsx`'s `Combobox`-based user picker (the bug's original repro) needed no changes itself.
- **A sidebar nav group containing the active route could never be manually collapsed — fixed by separating "default" from "explicit override"**: `sidebar-nav-item.tsx` used to compute `expanded = hasChildren && (isExpanded(item.href) || branchActive)` — since `branchActive` (true whenever a descendant route is the current page) was OR'd in, a group containing the active page stayed forced-open no matter how many times its toggle was clicked; `toggleExpanded` could only ever flip `isExpanded`, which the `||` made irrelevant. `hooks/use-sidebar.tsx` now stores explicit per-href overrides in a `Map<string, boolean>` (`expandedOverrides`) rather than a plain expanded-`Set`; `isExpanded(href)` returns `boolean | undefined` (`undefined` = no override yet, so the group still auto-expands to reveal the active route by default), and `toggleExpanded(href, current)` writes an explicit `!current`, which now wins over `branchActive` once set. **Changed this sync**: `expandedOverrides` no longer persists to `localStorage` at all — the `EXPANDED_KEY` constant and its read-on-mount/write-on-change effects were removed, so a manually collapsed/expanded group resets to the default (auto-expand-active-route) on every reload; `hidden`/`HIDDEN_KEY` persistence is unaffected. The `SidebarContext.Provider`'s `value` object and its `toggleSidebar`/`isExpanded`/`toggleExpanded` functions were also wrapped in `useMemo`/`useCallback` in the same change — a pure internal optimization to avoid unnecessary consumer re-renders, no behavior change.
- **Single-CSS-variable theming** and **runtime accent swap via DOM attribute + localStorage**: `--primary` drives themed surfaces, `AccentColorProvider` sets `data-accent` on `<html>`. A seventh preset, `black`, was added to `ACCENT_COLORS` — the one preset with no Tailwind shade scale (just the flat `--color-black`/`--color-white` tokens), so unlike every other preset (which steps one shade lighter for dark mode, e.g. `-600` light / `-500` dark) it inverts across themes instead (`--color-black` light / `--color-white` dark), per rules in `globals.css`. `accent-color-picker.tsx` has a `swatchColor(value)` helper to render its dropdown swatch accordingly — `var(--color-black)` for the `black` preset, `var(--color-${value}-600)` for every other preset.
- **Hydration-safe browser-state restoration**: `hydrated` flag + `useEffect`, `eslint-disable react-hooks/set-state-in-effect`, in `SidebarProvider` and `AccentColorProvider`; `components/theme/use-has-mounted.ts` (`useSyncExternalStore`) guards `ThemeToggle`. **Changed this sync**: `SidebarProvider`'s mount-time restoration now only covers `hidden` (`HIDDEN_KEY`) — `expandedOverrides` is no longer restored from/persisted to `localStorage` at all (see the sidebar-collapse pattern above).
- **Mobile drawer closes on route change via render-time state adjustment**: in `hooks/use-sidebar.tsx`.
- **Generic, presentational `DataTable<TData>` building block**: `components/shared/data-table/` composes a toolbar (actions + search + a caller-rendered buttons node), a table body (skeleton-loading rows, an `Empty` state, or an `Alert`-based error state that replaces the body and hides pagination), and a windowed-pagination footer (`getPageWindow()` always keeps page 1/last visible plus siblings around the current page). It takes no dependency on any feature or data-fetching library — fully controlled via props (`data`, `columns`, `isLoading`, `error`, callbacks); `UsersDataTable` (server-driven search via URL params), `RolesDataTable` (local client-side filtering — there's no backend search endpoint for roles), and `NotificationsDataTable` (a custom multi-field filter UI, see below) reuse it as-is with different search wiring. `isLoading` takes priority over a stale `error` — an in-flight refetch (e.g. clicking Refresh) always shows the table's own skeleton-row loading state rather than a leftover error from a previous failed load. Optional per-column client-side sorting (`DataTableColumn.sortable`/`sortValue`) — a sortable header renders as a `<button>` cycling asc → desc → unsorted with `ArrowUp`/`ArrowDown`/`ArrowUpDown` icons, and the table body sorts a `useMemo`-derived copy of `data`; explicitly scoped to callers holding the full result set client-side — `RolesDataTable` uses it (name/description columns), `UsersDataTable` deliberately does not, since it paginates via the backend and only ever holds one page of `data` at a time. An optional `mode` prop (`"paginated"` default / `"virtualized"` / `"infinite"`) switches the row/header markup to an ARIA-grid div layout rendered via `data-table-virtual-body.tsx` (a native `<table>` can't virtualize its rows cleanly); an optional `onSortChange` lets a caller take over sorting server-side instead of the default client-side sort, a prerequisite for `"virtualized"`/`"infinite"` modes against a large or streamed dataset — both additive, no existing caller (`UsersDataTable`, `RolesDataTable`, `NotificationsDataTable`) passes either, so all three keep today's exact `"paginated"` behavior. `data-table-toolbar.tsx` renders three stacked sections (actions / search / an inserted `Separator` between rendered sections) instead of one combined row, and has a `customSearch?: React.ReactNode` prop for a caller-supplied multi-field filter UI plus `onCustomSearch?: () => void`, which renders an explicit "Search" button — custom filters apply on click, unlike the built-in single-field text search, which stays auto-debounced (400ms). The toolbar no longer builds the Export/Refresh/Columns cluster itself; `data-table-buttons.tsx` (`DataTableButtons`) does, wrapped in `components/ui/button-group.tsx` (`ButtonGroup`). `data-table.tsx` computes this buttons node and either passes it to the toolbar as `buttons` (default layout) or, when `customSearch` is used, renders it itself inside the table's own bordered content box (right-aligned, above the content) — the content box (`rounded-md border border-border`) always wraps the table/empty/error/virtualized body, not just in the `customSearch` case. `NotificationsDataTable` is the first and only consumer of `customSearch` so far — its status + recipient filter dropdowns moved out of a standalone block above `<DataTable>` into this slot, holding local "pending" state applied to the URL only when "Search" is clicked, rather than auto-navigating on every change.
- **Controlled form state alongside `useActionState`, for Server Action forms that can fail**: dialogs bound to a mutation Server Action keep their own `useState<FormValues>` in parallel with `useActionState(...)`. This is deliberate, not redundant — React resets *uncontrolled* form fields once a Server Action settles, regardless of success or failure, which would silently wipe user input after a validation error; controlled state survives that reset.
- **Force-remount via a bumped `key` to reset `useActionState`**: `useActionState` has no imperative "clear this error/state" API, so each `*DataTable` bumps a per-dialog key counter on every open (`createDialogKey`, `editDialogKey`, ...) and passes it as that dialog's React `key`, forcing a fresh component instance (fresh action state, fresh controlled form state, and — for the edit dialogs — a fresh detail-fetch) each time it's opened.
- **Toast notifications via a themed `sonner` wrapper**: `components/toast/` never exposes `sonner`'s `toast` directly — call sites use `notifySuccess`/`notifyError` (`notify.ts`), and the visual theme (`saturatedToastOptions`, `withToastProgress()`) is centralized in `toast-theme.ts` so every toast in the app looks consistent without each call site repeating class names. **One deliberate exception**: `create-user-dialog.tsx`'s domain-lookup "found" result renders as an inline message under the Username field instead of a toast — the result is directly tied to that field's own state (and needs to clear itself the moment the username changes again), not a fire-and-forget action result like every other success path in this app.
- **Backend error messages surfaced through the shared `send()` wrapper**: `lib/server/http.ts`'s `extractErrorMessage()` centralizes turning a non-2xx response body into a human-readable string (envelope message → validation-errors map → `ProblemDetails.title` → generic fallback), so every `features/*/api/*.ts` call gets real error text without each call site parsing the body itself.
- **Real-time push via SignalR, browser-authenticated with a short-lived, action-issued access token; direct browser-to-backend connection with retry**: `use-notifications.ts` opens a `HubConnection` directly from the browser to `process.env.NEXT_PUBLIC_SIGNALR_HUB_URL` — an absolute backend URL, no longer proxied same-origin through `next.config.ts`'s (deleted) `rewrites()`; the browser depends on backend CORS being configured for the admin origin (assumed, not verified). Authenticated via `accessTokenFactory` calling `getSignalRTokenAction()` — a Server Action that hands the browser a short-lived access token specifically for this handshake, since the real session token stays in an httpOnly cookie the browser can't read otherwise (proactively refreshing it first if near expiry — see the session-refresh-helpers pattern above). On any `SystemMessage` push it calls `refresh(statusRef.current)` (refetches the list, scoped to whichever status filter is currently active, plus the unread count) rather than merging the pushed payload into state, because the payload is the raw backend `SystemMessage`, not a full `NotificationDto`. `connectSignalR` is a retryable inner function — a failed `connection.start()` logs a sanitized error via `sanitizeSignalRErrorMessage()` (strips the raw HTML negotiate-failure response body the SignalR client embeds in its own error message) and retries after 30 seconds via `setTimeout`, cleared on unmount alongside the existing connection teardown. `.configureLogging(LogLevel.Critical)` is added to the `HubConnectionBuilder` chain — this app has no `not-found.tsx` anywhere under `app/`, so navigating to an unmatched route unmounts the entire `(dashboard)/layout.tsx` subtree via Next.js's default 404, which intentionally stops any open connection mid-flight; the browser surfaces that as an abnormal WebSocket closure (code 1006), which SignalR's own client previously logged via `console.error` at its default `LogLevel.Error` even though the disconnect was expected. The hook's own explicit `console.error` inside `connection.start()`'s catch block (for genuine connect failures) is untouched.
- **Server-side notification tab filtering, shared across two consumers via one Context**: both `notification-bell.tsx` (topbar) and `notification-inbox.tsx` (Home page) offer an All/Unread/Archived `Tabs` filter, but neither filters client-side over an already-fetched batch — `notification-bell.tsx` calls the shared `refresh(status)`, `notification-inbox.tsx` calls `getMyNotificationsAction({pageNumber, status})` directly. This fixes a real, pre-existing bug: `notification-bell.tsx` used to fetch one unfiltered page (10 items) and filter All/Unread/Archived client-side over just that batch, while its unread-count badge came from the separate true-total `getUnreadCountAction()` — so the "Unread"/"Archived" tabs could show fewer items than the badge implied (or appear empty) whenever matching items existed outside the latest page. Both consumers read `unreadCount`/`markAsRead` from the shared `NotificationsContext` (see the Context-provider-per-concern pattern above) rather than each owning their own copy, so marking a notification read from either place updates both immediately.
- **Every backend API response is envelope-wrapped — never assume a bare value** (learned from a real bug): `getAllUsers` (now in `users.api.ts`, formerly its own `get-all-users.ts`, pre-existing, previously uncalled) assumed `GET user` returned a bare `UserDto[]` (`guardRawCall`) and broke (`users.map is not a function`) the first time something actually called it — the endpoint, like every other endpoint in this backend, wraps its response in the `Result`/`ApiResponse` envelope. Several of the `features/notifications/api/*.ts` files had the same wrong assumption and were fixed the same way. Treat `guardRawCall` as almost never correct for this backend; verify against a sibling endpoint's actual response shape rather than assuming.
- **Auth-token injection moved out of `http.ts` into a request-handler pipeline, decoupling the API layer from sessions**: `lib/server/http.ts`'s `RequestOptions.accessToken?: string` is gone, replaced by `handlers?: HttpRequestHandler[]` (`(context: HttpRequestContext) => Promise<void> | void`, `context.headers` a mutable `Record<string, string>`) run in order by `send()` before `fetch()` — `http.ts` no longer imports or knows about sessions at all. `lib/server/backend-api.ts` re-exports `requestJson`/`requestVoid` pre-wired with `handlers: [bearerTokenHandler, ...]` and `client: ApiClients.Backend` (`lib/server/api-clients.ts` named-client registry, room for more entries later); nearly every `features/*/api/<feature>.api.ts` file now imports from `backend-api.ts` instead of `http.ts`, and those functions no longer take an `accessToken` parameter. `lib/server/http-handlers/bearer-token-handler.ts` exports `bearerTokenHandler` (reads the ambient session via `getSession()`) and `explicitBearerTokenHandler(accessToken)`, a factory used by the pre-session-cookie call sites — now consolidated into `token.api.ts`'s `getToken`/`refreshToken` (via `login-action.ts`) and `user-profile.api.ts`'s `getCurrentUser` (called from both `login-action.ts` and `proxy.ts`'s Edge middleware, which has no `next/headers` session access) — which still call `http.ts` directly. Every mutation `*-action.ts` Server Action stopped extracting/passing `session.accessToken` to its api function but kept `resolveSession()` + the "session expired" early-exit for the friendly UX message.
- **Client-side toast/pending feedback centralized into two small hooks, replacing per-dialog duplication**: `hooks/use-guarded-action.ts`'s `useGuardedAction()` (`[pending, run]`, built on `useTransition`) wraps the "run an imperative action, toast the result, track pending" pattern for non-form calls — applied to `delete-user-dialog.tsx`/`delete-role-dialog.tsx` (their confirm-dialog UI itself was left untouched, a deliberate scope decision). `hooks/use-action-success-toast.ts`'s `useActionSuccessToast(state, successMessage, onSuccess?)` is a `useEffect` wrapper that toasts and runs `onSuccess` once a `useActionState`-bound result's `.success` turns true — applied at 6 call sites across 5 form dialogs (`create-user-dialog.tsx`, `create-role-dialog.tsx`, `send-notification-dialog.tsx`, `edit-role-dialog.tsx`, `edit-user-dialog.tsx` — 2 call sites, the update form and the password-reset form), replacing a `useEffect` + `notifySuccess(...)` pattern previously duplicated in each.
- **`server-only` compile-time guard on every `lib/server/*` module handling sessions/tokens**: all 15 files under `lib/server/` — the flat files plus `http-handlers/bearer-token-handler.ts` — start with `import "server-only";`, so a Client Component that accidentally imports one of them fails at build time with a clear error, instead of silently bundling server-only code into the client or failing confusingly at runtime. Verified empirically: `src/proxy.ts` (Next.js 16 middleware/Edge runtime) transitively imports 12 of these 15 files — `pnpm build` passed, and a live `pnpm start` exercised the full `parseSessionCookie` → `token-cipher.decrypt` path (`/login` returning 200, an unauthenticated `/` returning a 307 redirect to `/login`) with no runtime throw.
- **Server Actions that change list-page data now self-invalidate via `revalidatePath`, rather than depending solely on the caller's `router.refresh()`**: `features/roles/api/{create-role-action,update-role-action,delete-role-action}.ts` call `revalidatePath("/identity/roles")`, `features/users/api/{create-user-action,update-user-action,delete-user-action}.ts` call `revalidatePath("/identity/users")`, and `features/notifications/api/send-notification-action.ts` calls `revalidatePath("/notifications")` — each placed right after the `!result.isSuccess` early-return, right before `return { success: true }`, following the precedent already set by `features/auth/api/refresh-session-action.ts`'s `revalidatePath("/user-profile")`. Deliberately **not** applied to `features/notifications/api/mark-notification-read-action.ts` (called only from the client-only notification bell/inbox hook, which manages its own state — no Server-Component page depends on route cache there, so it would be a no-op) or to the on-demand detail/picklist reads (`force-password-action.ts`, `get-user-detail-action.ts`, `get-role-detail-action.ts`) — a deliberate scope decision, not an oversight.

## Shared Kernel / Common Building Blocks Used

- `components/ui/*` — the app's own primitive layer.
- `components/ui/combobox.tsx` (plus its `popover.tsx`/`command.tsx` primitives) — the shadcn-style single-select building block, replacing `components/select/*`'s `EntitySelect`/`SearchSelect` (see Key Design Patterns). Only one consumer — `features/users` (edit dialog, status/authProvider). `features/notifications/components/user-select.tsx` no longer uses it (a bespoke on-demand-search component; see Key Design Patterns).
- `components/foundation/` — `use-listbox.ts`/`use-virtual-list.ts`/`floating-overlay.tsx` (serving only `components/command/*` and, for `use-virtual-list.ts`, `components/shared/data-table/data-table-virtual-body.tsx`) plus `portal-container.ts` (serving `components/ui/{dialog,popover}.tsx`, see Key Design Patterns). No longer backs any select/combobox component.
- `components/command/*` — Command Palette (Cmd/Ctrl+K); still no consumer wired into the app.
- `components/shared/data-table/` — generic list-table building block (toolbar, `data-table-buttons.tsx` for the Export/Refresh/Columns cluster, built on `components/ui/button-group.tsx` — pagination, loading/empty/error states, optional per-column sort, optional virtualized/infinite modes via `data-table-virtual-body.tsx`, and an optional `customSearch`/`onCustomSearch` slot for a caller-supplied, apply-on-click multi-field filter UI); consumed by `features/users`, `features/roles`, and `features/notifications` (with different search/sort strategies — see Key Design Patterns), designed with no feature-specific knowledge baked in.
- `components/shared/access-denied.tsx` (new this sync) — the shared "access denied" panel (`AccessDenied({ permission })`), built on `components/ui/empty.tsx`; not consumed directly by feature code — it's returned by `lib/server/require-permission.tsx`'s `requirePermission()`, which `users-page.tsx`/`roles-page.tsx`/`notifications-page.tsx` all now call (see Key Design Patterns).
- `components/shared/object-viewer/` — additive; a recursive read-only object/table renderer built on `components/ui/table` + `components/ui/input`, no `features/*` dependency. Not yet consumed by any page.
- `components/toast/` — toast notification wrapper around `sonner`; mounted once at the root layout, called from feature code via `notifySuccess`/`notifyError`.
- `lib/shared/utils.ts` (`cn`), `lib/shared/dedupe-claims.ts`, `lib/shared/user-display.ts` — cross-cutting helpers consumed by 2+ features or by layout chrome. `lib/shared/menu.ts` (`buildVisibleMenu`, consumed by `Sidebar`) and `lib/shared/authorization.ts` (`hasPermission`/`hasAnyPermission`/`hasAllPermissions`/`isSuperAdminUser`, safe for both server and client — consumed directly by `Sidebar` and, via `lib/server/authorization.ts`'s thin wrapper, by `UsersPage`/`RolesPage`/`NotificationsPage`).
- `lib/server/*` — the server-only building blocks every feature's `api/` layer is built on: `http.ts`, `call-guard.ts`, `config.ts`, `session.ts`, `session-cookie.ts`, `authorization.ts` (a thin wrapper over `lib/shared/authorization.ts`, deriving `userName` from the session rather than taking it as a separate parameter), `require-permission.tsx` (new this sync — the shared page-level permission-gate helper, composing `resolveSession()`, `authorization.ts`, and `components/shared/access-denied.tsx`), plus the session-encryption/refresh chain — `token-cipher.ts`, `jwt.ts`, `build-session-claims.ts`, `parse-session.ts`, `refresh-session.ts` (exports both `refreshSession()` and `refreshSessionIfNearExpiry()`), and `persist-session-cookie.ts` — used by `src/proxy.ts`, `features/auth/api/{login,logout,refresh-session}-action.ts`, and `features/notifications/api/get-signalr-token-action.ts`.
- Permission-string constants remain **not** a shared kernel piece — per-feature `features/{users,roles,notifications}/constants/permissions.ts` (`USERS_PERMISSIONS`, `ROLES_PERMISSIONS`, `NOTIFICATIONS_PERMISSIONS`), each mirroring the backend's own permission-string constants for that module. Nav metadata follows the same per-feature-ownership move (`features/{home,users,roles,notifications}/constants/nav-item.ts`), assembled (not owned) by the top-level `constants/nav-items.ts`. `features/users/constants/auth-provider.ts` follows the same per-feature-ownership pattern, not promoted to a shared location — its two consumers (`create-user-dialog.tsx`, `edit-user-dialog.tsx`) both live inside `features/users`, consistent with "only promote once 2+ features need it".
- `features/notifications/components/user-select.tsx` is a **feature-owned** reusable component (on-demand searchable user picker, built on a bespoke `components/ui/{button,popover,command}.tsx` composition, see Key Design Patterns), not promoted to `components/shared/` — consumed by two components within the same feature, not yet by a second feature, consistent with the "only promote once 2+ features need it" pattern.
- `hooks/*`, `components/theme/*` — cross-cutting building blocks consumed by layout components.
- No package or code is shared with another client app — `clients/` still contains only `admin`.

## Module/Route Boundaries

Two route groups/areas exist:
- `(dashboard)` — wraps `/`, `/user-profile`, `/identity/users`, `/identity/roles`, and `/notifications` with `resolveSession()` + `AppShell` (top bar, sidebar).
- `login` (ungrouped) — `/login`, rendered under the root layout only, no `AppShell`/session resolution.

`constants/nav-items.ts` declares `/administration` (a group node, relabeled from "Identity"/`/identity`, nesting `/notifications` alongside `/identity/users`/`/identity/roles` as a `children` entry, rather than `/notifications` being a separate top-level item), `/identity/users`, `/identity/roles`, `/notifications`, and `/settings`. `/identity/users`, `/identity/roles`, and `/notifications` all have real routes and pages, gated on `USERS_PERMISSIONS.View`/`ROLES_PERMISSIONS.View`/`NOTIFICATIONS_PERMISSIONS.Read` respectively (the "Send" action on `/notifications` is additionally gated on `NOTIFICATIONS_PERMISSIONS.Send`); `/administration` and `/settings` both still have no corresponding route/`page.tsx`.

Feature isolation is enforced by convention (barrel-only cross-feature imports), verified in the places it's exercised so far: `features/auth/api/login-action.ts` and `features/users/components/users-page.tsx`/`api/create-user-action.ts` all import from `@/features/user-profile`'s barrel, not its internals. The two page-level cross-feature barrel edges removed in a prior sync remain gone — `features/users/components/users-page.tsx` no longer imports `@/features/roles` at all (it no longer fetches the role catalog), and `features/notifications/components/notifications-page.tsx` no longer imports `@/features/users` at all (it no longer fetches `getAllUsers`). Each was replaced by a component-level, direct-file, cross-feature edge instead: `features/users/components/edit-user-dialog.tsx` imports `@/features/roles/api/get-all-roles-action` directly, and `features/notifications/components/user-select.tsx` imports `@/features/users/api/search-users-action` directly — both are two of the six barrel-bypass exceptions below, not barrel imports. Six narrow, reasoned exceptions to the barrel-only rule exist (`constants/nav-items.ts`, `components/layout/user-menu.tsx`, `components/layout/topbar.tsx`, `edit-user-dialog.tsx` -> `get-all-roles-action`, `user-select.tsx` -> `search-users-action`, and `home-page.tsx` -> `notification-inbox.tsx`/`get-my-notifications-action.ts` — see Key Design Patterns / Dependency Direction); a seventh, lower-level one also exists at the `lib/server` tier: `lib/server/refresh-session.ts` imports `features/auth/api/token.api.ts` directly (bypassing the `@/features/auth` barrel, which does export `refreshToken`) — worth noting since it's also a reversal of the usual `features/* -> lib/server/*` dependency direction (here, `lib/server` reaches into a feature's `api/` file), unlike the other exceptions which stay within the normal direction. `lib/server/require-permission.tsx` follows the normal direction (it reaches down into `features/user-profile`'s barrel for `resolveSession`, the same way ordinary page code does), not a further exception.

## Known Architectural Risks / Debt

| Finding | Severity | Notes |
|---|---|---|
| ~~Session cookie stores the access/refresh token and claims as plaintext JSON~~ — **resolved** | — | The cookie is AES-256-GCM encrypted (`lib/server/token-cipher.ts`, keyed by `TOKEN_ENCRYPTION_KEY`, which is a hard requirement — `getTokenEncryptionKey()` throws if unset). See Auth Flow (overview.md) / Key Design Patterns. |
| ~~No token refresh flow wired up~~ — **resolved** | — | `src/proxy.ts` proactively rotates the access/refresh token via `lib/server/refresh-session.ts` when within `REFRESH_LEAD_MS` (5 min) of expiry; a failed refresh is treated as non-fatal (see Auth Flow), not a dead session. |
| ~~Risk: a Client Component could accidentally import a `lib/server/*` session/token module and leak it into the client bundle~~ — **resolved** | — | All 15 `lib/server/*` files (the flat files plus `http-handlers/bearer-token-handler.ts`) start with `import "server-only";`, turning any such accidental import into a build-time error instead of a silent leak or confusing runtime failure. Verified this doesn't break `src/proxy.ts` (Edge/middleware runtime, which transitively imports 12 of the 15): `pnpm build` passed, and a live `pnpm start` exercised the full `parseSessionCookie` → `token-cipher.decrypt` path via real requests (`/login` 200, unauthenticated `/` 307 to `/login`) with no runtime throw. |
| `proxy.ts`'s call chain uses Node's `crypto` module directly, with no explicit runtime pin | Low–Medium (verify) | `token-cipher.ts` (imported transitively via `parse-session.ts`/`refresh-session.ts`) uses `createCipheriv`/`createDecipheriv` from Node's built-in `crypto`, which the traditional Edge Runtime does not support. `proxy.ts` has no `export const runtime = "nodejs"` (or similar) declaration; the current behavior is consistent with Next.js 16's `proxy.ts` convention defaulting to the Node.js runtime (unlike the old edge-only `middleware.ts`), but this is inferred from the code, not confirmed via an explicit config — worth pinning explicitly if that assumption is ever wrong for a deployment target that still expects edge middleware. |
| SignalR connects directly to the backend from the browser, bypassing the Next.js server entirely | Medium (unverified) | `use-notifications.ts`'s `SIGNALR_HUB_URL` is `process.env.NEXT_PUBLIC_SIGNALR_HUB_URL!` (an absolute backend URL, the first `NEXT_PUBLIC_`-prefixed env var in this app), no longer proxied via a deleted `next.config.ts` `rewrites()`. This assumes backend CORS is configured for the admin app's origin — not verified, and not checked anywhere in this client's own code. If CORS isn't configured backend-side, the WebSocket handshake fails and is silently retried every 30s (`connectSignalR`) rather than surfaced to the user. |
| Nav item references a route with no `page.tsx` (`/settings`, `/administration`) | Low | `/identity/users` and `/identity/roles` both have real pages. `/settings` still 404s if followed; the assembled nav tree's group node — labeled "Administration"/`/administration` (previously "Identity"/`/identity`) — also has no `page.tsx` of its own, same shape as `/settings`. |
| Backend list endpoints never populate `Roles`/`Claims` on the DTO | Low (worked around, but worth remembering) | `UserService.SearchAsync`/`GetAllAsync` and `RoleService.GetAllAsync` all use a `DataMapper.cs` projection that only maps scalar fields; only the single-record fetch (`GetByIdAsync`) populates `Roles`/`Claims`. Both edit dialogs correctly re-fetch full detail on open to work around this (see Key Design Patterns), but any future feature reading a list endpoint for role/claim data would silently get empty arrays if it forgot to do the same. |
| ~~Dashboard still renders hardcoded mock data (`features/dashboard/api/sample-data.ts`)~~ — **resolved** | — | `features/dashboard/` was deleted outright and replaced by `features/home/`, a real Server Component calling `resolveSession()` and `getMyNotificationsAction()`. There is no longer any page in this app that skips the backend. |
| No `not-found.tsx` anywhere under `app/` | Low (cosmetic) | Navigating to an unmatched route renders Next.js's default 404 within the root layout only, unmounting the entire `(dashboard)/layout.tsx` subtree (`AppShell`, `TopBar`, `NotificationBell`, and — if the current page is `/` — `NotificationInbox`), which intentionally stops any open SignalR connection mid-flight. This surfaces as an abnormal WebSocket closure (code 1006), which SignalR's client used to log via `console.error` by default; `use-notifications.ts` adds `.configureLogging(LogLevel.Critical)` to silence that specific noise (see Key Design Patterns), but the underlying missing-`not-found.tsx` fact itself is unchanged — worth adding a real one if a friendlier 404 experience is ever needed, independent of this logging fix. |
| `prettier` + `prettier-plugin-tailwindcss` installed but no config file/`format` script | Low | Still `unknown` whether formatting is enforced anywhere. |
| No automated test suite | Low (by design at this stage) | No `*.test.*`/`*.spec.*` files, no test runner in `package.json`. Now more notable given real auth/session logic plus full Users and Roles CRUD flows all exist untested. |
| `components/ui/button.tsx` hand-modified beyond shadcn CLI output (`loading` prop, `cursor-pointer`) | Low | Re-running the shadcn CLI would silently drop these customizations unless done carefully. `tabs.tsx` (`cursor-pointer` on `TabsTrigger`) and `dialog.tsx` (`max-h`/`overflow-y-auto` on `DialogContent`, plus the pre-existing `relative`/portal-container additions) carry the same kind of hand-modification risk. |
| `DataTable`'s `onExport` prop has no caller yet | Low | `components/shared/data-table/data-table-buttons.tsx` already renders an Export button when `onExport` is passed, but no current feature (including `UsersDataTable`/`RolesDataTable`/`NotificationsDataTable`) passes one — dead capability until a consumer needs it. |
| `components/shared/object-viewer/` has no consumer yet | Low (by design) | Purely additive, ported from an external export spec and adapted to this project's design tokens/`components/ui/*` primitives, but not imported by any page or dialog. Dead code until something wires it in. |
| `components/command/*` (CommandPalette) has no consumer yet | Low (by design) | Nothing in the app wires up `CommandPaletteProvider` yet. Dead code until something adopts it. (`AsyncSelect`/`MultiSelect`, the other two "no consumer yet" components from a previous sync, were deleted rather than left dead — see Key Design Patterns.) |
| `components/ui/combobox.tsx` does not virtualize its option list | Low (by design) | The Floating-UI select library it replaced auto-virtualized past 50 options (`options-list.tsx`, now deleted); `cmdk`'s `Command` has no built-in virtualization, and the current two consumers (a small static enum, a client-filtered user list) don't need it. Deliberate scope decision made when migrating to `Combobox` — revisit if a future consumer needs a large option list. |
| `eslint.config.mjs`'s `react-hooks/refs` override glob still lists `src/components/select/**/*.tsx` | Trivial | `components/select/*` was deleted; the glob entry is now a no-op (matches nothing) rather than a functional problem, but is stale and should be removed the next time `eslint.config.mjs` is touched. |
| The SignalR handshake token intentionally narrows the "JWT never leaves the httpOnly cookie" invariant | Low (deliberate) | `getSignalRTokenAction()` hands the browser a real, short-lived access token so `use-notifications.ts` can authenticate the WebSocket handshake — the one place in the app where the access token is readable by browser JS. A deliberate trade-off (SignalR can't attach a cookie/header the way `fetch` can), not an oversight, but worth keeping in mind if the token's blast radius ever needs to shrink further. |
| ~~`notifications-page.tsx` calls `getAllUsers(session.accessToken)` unpaginated to populate the recipient filter/picker~~ — **resolved** | — | Replaced by an on-demand search pattern: `features/notifications/components/user-select.tsx` calls `searchUsersAction` (page 1 of matches only) once the typed query passes a minimum length, debounced 300ms — see Key Design Patterns. `notifications-page.tsx` no longer imports `getAllUsers`/`@/features/users` at all. |

## Notes

<!-- manual: content below this line is human-authored and must be preserved verbatim during sync -->

---
_Generated: 2026-08-10 (resynced — SignalR direct-to-backend connection via `NEXT_PUBLIC_SIGNALR_HUB_URL`, `next.config.ts`'s `rewrites()` deleted; `use-notifications.ts` retry-on-failed-handshake logic; new shared `refreshSessionIfNearExpiry()` (`lib/server/refresh-session.ts`) and `persistSessionCookie()` (`lib/server/persist-session-cookie.ts`) helpers, adopted by `proxy.ts`, `get-signalr-token-action.ts`, and the previously-undocumented `features/auth/api/refresh-session-action.ts`; `AccentColorProvider`'s new `black` preset and `accent-color-picker.tsx`'s `swatchColor()` helper; further resynced same day — `ClaimDto` split out of the former global `types/user.ts` into a new `types/claim.ts`; `UserDto`/`CreateUserRequest`/`SearchUsersParams` moved into a new `features/users/types/user.ts`; `TokenDto`/`GetTokenRequest`/`RefreshTokenRequest`/`DeviceDto` moved into a new `features/auth/types/token.ts`; both re-exported via their owning feature's barrel, with cross-feature consumers (`get-current-user.ts`, `user-select.tsx`) now importing the type through the barrel instead of a global `types/` file; further resynced same day — `features/dashboard/` deleted outright and replaced by a new `features/home/` at `/`, rendering `ProfileSummaryCard` plus a two-pane `NotificationInbox` (new `notification-list.tsx`/`notification-detail.tsx`/`notification-inbox.tsx`, sixth barrel-bypass exception `home-page.tsx` -> `notification-inbox.tsx`/`get-my-notifications-action.ts`); `getMyNotificationsAction()` gained real params; fixed a real tab-vs-unread-badge-disagreement bug in `notification-bell.tsx` by making its tab filter server-side; new shared `features/notifications/context/notifications-provider.tsx` (`NotificationsProvider`, a new instance of the context-provider-per-concern pattern) so the bell and the inbox share one live SignalR connection/`unreadCount`; `use-notifications.ts`'s `markAsRead` now returns `Promise<boolean>` and `refresh` takes an optional `status`; `.configureLogging(LogLevel.Critical)` added to silence a cosmetic SignalR console error caused by the still-missing `not-found.tsx` unmounting the dashboard layout on an unmatched route (new Known Architectural Risks / Debt row); Dashboard's mock-data row in that same table marked resolved; also corrected, found via this sync's own verification rather than reported: the assembled nav tree's group node was relabeled "Identity"/`/identity` -> "Administration"/`/administration`, and `NOTIFICATIONS_NAV_ITEM` moved from a top-level entry into that group's `children`; further resynced same day — the unused `features/users/api/get-user-by-username.ts` (zero remaining references) was deleted and its barrel re-export removed; the new `server-only` npm package (`^0.0.1`) was added and `import "server-only";` prepended to all 15 `lib/server/*` files (verified via a real `pnpm build`/`pnpm start` that this doesn't break `proxy.ts`'s Edge/middleware runtime), closing the gap `nav-items.ts`'s barrel-bypass comment already described by hand; new `src/app/error.tsx` (root error boundary), `src/app/(dashboard)/error.tsx` (same shape, inside `AppShell`), and `src/app/(dashboard)/loading.tsx` (centered `Spinner`) added as error/loading boundaries; 7 Server Actions (`create/update/delete-role-action`, `create/update/delete-user-action`, `send-notification-action`) gained a self-invalidating `revalidatePath` call on their success path, following the precedent already set by `refresh-session-action.ts`, deliberately excluding `mark-notification-read-action.ts` (client-only caller, no-op) — further resynced 2026-08-11 — login `Card` markup moved from `login-page.tsx` into `login-form.tsx`, added a pending overlay/`fieldset` while `loginAction` runs; fixed `dialog.tsx`'s `DialogContent` close-button positioning (`relative` added); fixed `create-user-action.ts` reading the wrong `FormData` keys — further resynced same day — `create-user-dialog.tsx` gained a domain-user (AD) lookup against a new `user/get_domain_user/{userName}` endpoint (icon-in-input button + blur-triggered autosearch via a new `get-domain-user.ts`/`get-domain-user-action.ts`, silent reset on no-match, inline "found" message, new Auth Provider `Combobox` field with AD auto-select and conditionally-optional Password, `create-user-action.ts`'s validation updated to match); new shared `features/users/constants/auth-provider.ts` (blank = Local, matching the backend's null-means-local convention, see `User.ChangeAuthProvider`/`AuthProvider` enum) also adopted by `edit-user-dialog.tsx`, fixing a latent bug where it submitted the literal string `"Local"` instead of blank; `components/ui/tabs.tsx`'s `TabsTrigger` gained `cursor-pointer` and `components/ui/dialog.tsx`'s `DialogContent` gained `max-h-[calc(100vh-2rem)] overflow-y-auto` so tall dialogs scroll instead of overflowing a short viewport — further resynced 2026-08-11 — every `features/<name>/api/` folder was refactored from one file per backend-calling function into one consolidated file per feature: `features/auth/api/token.api.ts` (replaces `login.ts`+`refresh-token.ts`, `login()` renamed to `getToken()`), `features/users/api/users.api.ts` (replaces 7 files, plus the already-deleted `get-user-by-username.ts`), `features/roles/api/roles.api.ts` (replaces 6 files), `features/notifications/api/notifications.api.ts` (replaces `get-notifications.ts`+`send-notification.ts`; `SendNotificationRequest` relocated into `types/notification.ts`) plus a new, separate `features/notifications/api/user-notifications.api.ts` (replaces `get-my-notifications.ts`+`get-unread-count.ts`+`mark-notification-read.ts`), and `features/user-profile/api/user-profile.api.ts` (replaces `get-current-user.ts`+`list-sessions.ts`+`revoke-session.ts`; `resolve-session.ts` deliberately left separate); every `*-action.ts` Server Action, feature barrel, `src/proxy.ts`, `lib/server/refresh-session.ts`, and `users-page.tsx`/`roles-page.tsx`/`notifications-page.tsx` had their import paths updated to match — pure file reorganization, no route/contract change; also fixed two pre-existing doc gaps in this same pass: `features/users/index.ts`'s dependency-graph rows no longer mention the already-deleted `get-user-by-username.ts` — further resynced 2026-08-12 — added the shared `lib/server/require-permission.tsx` (`requirePermission(permission)`) and `components/shared/access-denied.tsx` (`AccessDenied`) permission-gate pattern, replacing three separate implementations across `users-page.tsx`/`roles-page.tsx`/`notifications-page.tsx` (two inline-`Empty`, one `redirect()`) — real behavior change: `notifications-page.tsx` no longer redirects on a missing view permission, now renders the shared `AccessDenied` like the other two; `lib/server/authorization.ts`'s `hasPermission`/`hasAnyPermission`/`hasAllPermissions` dropped their separate `userName` parameter, now `(session, permission)`/`(session, permissions)`; `hooks/use-sidebar.tsx`'s nav-group expand/collapse overrides no longer persist to `localStorage` (`EXPANDED_KEY` and its effects removed — only `hidden` persists now), and its context value/callbacks were wrapped in `useMemo`/`useCallback` (pure optimization, no behavior change) — scope: client app "admin" — see .claude/CLAUDE.md for update rules._
