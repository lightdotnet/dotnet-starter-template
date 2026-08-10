# Architecture: admin

## Layering

Observed folder organization (verified via directory listing) — everything now lives under `clients/admin/src/` (previously at `clients/admin/` root):

```text
src/
  app/                      routing only — layout.tsx, globals.css, login/page.tsx,
                             (dashboard)/{layout.tsx,page.tsx,user-profile/page.tsx,identity/{users,roles}/page.tsx,
                             notifications/page.tsx}
  features/
    auth/                   api/{login,refresh-token,login-action,logout-action,refresh-session-action}.ts,
                             components/{login-page,login-form}.tsx, index.ts
                             (logout-action.ts — barrel does not export it, see Dependency Direction;
                             refresh-session-action.ts — a super-admin-gated manual token-rotation Server Action,
                             not previously listed here; changed this sync to use the new persistSessionCookie()
                             helper, see Dependency Direction)
    user-profile/           api/{get-current-user,list-sessions,revoke-session,resolve-session}.ts, components/user-profile-page.tsx, types/user-session.ts, index.ts
    dashboard/              api/sample-data.ts (mock, not a backend call), components/{stat-card,users-table,dashboard-page}.tsx,
                             constants/nav-item.ts (`DASHBOARD_NAV_ITEM`, new), index.ts
    users/                  api/*.ts (14 endpoint files — the original 8 plus create-user-action, update-user-action,
                             force-password-action, delete-user-action, get-user-detail-action, and — new this sync —
                             search-users-action, all "use server"),
                             components/{users-page,users-data-table,create-user-dialog,edit-user-dialog,delete-user-dialog}.tsx,
                             constants/{permissions,nav-item}.ts (`USERS_PERMISSIONS`, `USERS_NAV_ITEM` — nav-item.ts new), index.ts
    roles/                  api/*.ts (12 endpoint files — the original 5 plus get-permissions, get-role-detail-action,
                             create-role-action, update-role-action, delete-role-action, and — new this sync —
                             get-all-roles-action, get-permissions-action),
                             components/{roles-page,roles-data-table,create-role-dialog,edit-role-dialog,delete-role-dialog}.tsx,
                             constants/{permissions,nav-item}.ts (`ROLES_PERMISSIONS`, `ROLES_NAV_ITEM` — nav-item.ts new), types/{role,permission-definition}.ts, index.ts
    notifications/          (new) api/*.ts (8 endpoint files: get-notifications, get-my-notifications(+action),
                             get-unread-count(+action), mark-notification-read(+action), send-notification(+action),
                             get-signalr-token-action), hooks/use-notifications.ts, components/{notification-bell,
                             notifications-page,notifications-data-table,send-notification-dialog,user-select}.tsx,
                             constants/{permissions,nav-item}.ts, types/notification.ts, index.ts
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
                             are both gone. 28 files total.
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
                             persist-session-cookie.ts (new this sync)
                             (session encryption/JWT/refresh chain from a prior sync; backend-api.ts/api-clients.ts/
                             http-handlers/bearer-token-handler.ts are from a prior sync — decouple auth-token
                             injection from http.ts into a handler pipeline; persist-session-cookie.ts is new this
                             sync — extracts the Server-Action-context cookie write (next/headers's cookies())
                             previously hand-rolled separately in refresh-session-action.ts and
                             get-signalr-token-action.ts, see Key Design Patterns)
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
                                       lib/server/refresh-session (refreshSessionIfNearExpiry — changed this sync,
                                       was a direct refreshSession() call plus its own inline REFRESH_LEAD_MS check,
                                       now delegated to the shared helper), lib/server/parse-session,
                                       lib/server/session-cookie (SESSION_COOKIE_NAME, buildSessionCookieOptions —
                                       changed this sync, REFRESH_LEAD_MS is no longer imported here directly, it
                                       moved into refresh-session.ts alongside the check it gates), types/session
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
src/app/(dashboard)/notifications/page.tsx -> features/notifications (NotificationsPage)

components/layout/app-shell.tsx    -> hooks/use-sidebar, components/layout/{sidebar,topbar}, types/session (ProfileData)
                                       (takes `{ permissions, userName, user, children }`, passes `permissions`/`userName`
                                       through to Sidebar and `user` through to TopBar)
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
features/auth/api/refresh-session-action.ts -> "use server"; lib/server/session (getSession), lib/server/refresh-session
                                        (refreshSession — unconditional, not the near-expiry-gated
                                        refreshSessionIfNearExpiry), lib/server/persist-session-cookie
                                        (persistSessionCookie — changed this sync, replaced a manual
                                        cookies().set(...) call), lib/server/authorization (isSuperAdminUser),
                                        next/cache (revalidatePath) — manually rotates the current session's
                                        tokens, gated on super-admin; called from
                                        features/user-profile/components/session-info-card.tsx (that file/caller
                                        is not otherwise documented here — out of scope for this pass)
features/auth/api/login.ts          -> lib/server/http, lib/server/call-guard, types/{api,token}
                                        (one of 3 exceptions still calling lib/server/http directly — runs before
                                        a session cookie exists, so login-action.ts's own explicit token isn't
                                        applicable here; see Key Design Patterns for the handler-pipeline change)
features/auth/api/refresh-token.ts  -> lib/server/http, lib/server/call-guard, types/{api,token}
                                        (now has a real caller: lib/server/refresh-session.ts; another of the
                                        3 lib/server/http exceptions — runs before a session cookie exists)

features/user-profile/index.ts      -> ./components/user-profile-page, ./api/{resolve-session,get-current-user,list-sessions,revoke-session}, ./types/user-session
features/user-profile/components/user-profile-page.tsx -> components/ui/{card,badge,separator,avatar,alert}, qrcode,
                                        features/user-profile/api/resolve-session, lib/shared/{dedupe-claims,user-display}
features/user-profile/api/resolve-session.ts -> lib/server/session (getSession), types/session
                                        (now a thin passthrough to getSession() — no longer calls getCurrentUser itself;
                                        proxy.ts keeps the cookie's profile/claims fresh instead, see Key Design Patterns)
features/user-profile/api/get-current-user.ts -> lib/server/http, lib/server/call-guard, types/{api,user}
                                        (the 3rd lib/server/http exception — takes an explicit accessToken param
                                        and passes it via lib/server/http-handlers/bearer-token-handler.ts's
                                        explicitBearerTokenHandler; called from login-action.ts and proxy.ts,
                                        both before/without an ambient session)
features/user-profile/api/{list-sessions,revoke-session}.ts -> lib/server/backend-api, lib/server/call-guard, ./types/user-session (list)
                                        (changed this sync — now via backend-api.ts like ordinary endpoint files)

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
                                        features/users/components/users-data-table,
                                        lib/server/authorization (hasPermission), features/users/constants/permissions
                                        (USERS_PERMISSIONS — moved from the former top-level constants/permissions), components/ui/empty
                                        — no longer fetches features/roles/api/get-all-roles this sync; the role
                                        catalog is now self-fetched by edit-user-dialog.tsx on open instead (see below)
features/users/components/users-data-table.tsx -> components/shared/data-table (DataTable + types), components/ui/{avatar,badge,button},
                                        components/ui/dropdown-menu, features/users/components/{create,edit,delete}-user-dialog,
                                        features/user-profile/components/user-status-badge, lib/shared/user-display,
                                        types/user — no longer takes a `roles`/`RoleDto` prop or imports
                                        features/roles/types/role this sync (edit-user-dialog.tsx now fetches roles itself)
features/users/components/create-user-dialog.tsx -> components/ui/{alert,button,dialog,input,label}, components/toast (notifySuccess),
                                        hooks/use-action-success-toast (new this sync, replacing an inline useEffect),
                                        features/users/api/create-user-action
features/users/components/edit-user-dialog.tsx -> components/ui/{alert,button,checkbox,combobox,dialog,input,label,
                                        spinner,tabs} — status/authProvider now use the new components/ui/combobox
                                        (`Combobox`); components/select, whose `EntitySelect` this used before this
                                        sync, is gone,
                                        components/toast (notifySuccess), features/users/api/get-user-detail-action,
                                        features/users/api/update-user-action, features/users/api/force-password-action,
                                        features/roles/api/get-all-roles-action (new this sync — direct file import,
                                        not the @/features/roles barrel; see the barrel-bypass exceptions below),
                                        features/roles/types/role (RoleDto), types/user — now holds its own
                                        `roles: RoleDto[]` state and fetches
                                        `Promise.all([getUserDetailAction(user.id), getAllRolesAction()])` on open,
                                        since users-data-table.tsx no longer passes a `roles` prop down; also uses
                                        hooks/use-action-success-toast (new this sync, 2 call sites — the update form
                                        and the password-reset form)
features/users/components/delete-user-dialog.tsx -> components/ui/{button,dialog}, components/toast, hooks/use-guarded-action
                                        (new this sync, replacing hand-rolled useTransition), features/users/api/delete-user-action, types/user
features/users/api/create-user-action.ts -> "use server"; features/user-profile (resolveSession), features/users/api/create-user, types/user
features/users/api/{update-user-action,force-password-action,delete-user-action,get-user-detail-action}.ts
                                     -> "use server"; features/user-profile (resolveSession),
                                        features/users/api/{update-user,force-password,delete-user,get-user-by-id} respectively, types/user
features/users/api/*.ts (7 remaining files) -> lib/server/backend-api, lib/server/call-guard, types/{api,user}
                                        (changed this sync — was lib/server/http; these ordinary endpoint files
                                        no longer take an accessToken parameter, see Key Design Patterns)

features/roles/index.ts             -> ./components/roles-page, ./api/{get-all-roles,get-role-by-id,get-permissions,
                                        create-role,update-role,delete-role}, ./constants/permissions (ROLES_PERMISSIONS),
                                        ./constants/nav-item (ROLES_NAV_ITEM), ./types/{role,permission-definition} —
                                        barrel; imported by app/(dashboard)/identity/roles/page.tsx and
                                        features/users/components/users-page.tsx
features/roles/constants/nav-item.ts -> lucide-react (KeyRound), ./permissions (ROLES_PERMISSIONS), types/nav
                                        (new; label "Roles", href "/identity/roles", permission ROLES_PERMISSIONS.View)
features/roles/components/roles-page.tsx -> features/user-profile (resolveSession), features/roles/api/get-all-roles,
                                        features/roles/components/roles-data-table, lib/server/authorization (hasPermission),
                                        features/roles/constants/permissions (ROLES_PERMISSIONS), components/ui/empty
                                        — no longer fetches features/roles/api/get-permissions this sync; the
                                        permission catalog is now self-fetched by edit-role-dialog.tsx on open instead
features/roles/components/roles-data-table.tsx -> components/shared/data-table (DataTable + types), components/ui/{button,dropdown-menu},
                                        features/roles/components/{create,edit,delete}-role-dialog,
                                        features/roles/types/role — its `name`/`description`
                                        columns set `sortable: true`/`sortValue` on the shared DataTable (new; the whole
                                        role list is fetched upfront, so client-side sort is meaningful here, unlike
                                        UsersDataTable which paginates server-side). No longer takes a
                                        `permissions`/`PermissionDefinition` prop or imports
                                        features/roles/types/permission-definition this sync (edit-role-dialog.tsx
                                        now fetches permissions itself)
features/roles/components/create-role-dialog.tsx -> components/ui/{alert,button,dialog,input,label}, components/toast (notifySuccess),
                                        hooks/use-action-success-toast (new this sync, replacing an inline useEffect),
                                        features/roles/api/create-role-action
features/roles/components/edit-role-dialog.tsx -> components/ui/{alert,button,checkbox,dialog,input,label,spinner},
                                        components/toast (notifySuccess), hooks/use-action-success-toast
                                        (new this sync, replacing an inline useEffect), features/roles/api/get-role-detail-action,
                                        features/roles/api/get-permissions-action (new this sync — intra-feature
                                        import, both files live under features/roles/),
                                        features/roles/api/update-role-action, features/roles/types/{role,permission-definition}
                                        — now holds its own `permissions: PermissionDefinition[]` state and fetches
                                        `Promise.all([getRoleDetailAction(role.id), getPermissionsAction()])` on open,
                                        since roles-data-table.tsx no longer passes a `permissions` prop down
features/roles/components/delete-role-dialog.tsx -> components/ui/{button,dialog}, components/toast, hooks/use-guarded-action
                                        (new this sync, replacing hand-rolled useTransition), features/roles/api/delete-role-action, features/roles/types/role
features/roles/api/{create-role-action,update-role-action,delete-role-action,get-role-detail-action}.ts
                                     -> "use server"; features/user-profile (resolveSession),
                                        features/roles/api/{create-role,update-role,delete-role,get-role-by-id} respectively
                                        (update-role-action additionally re-reads get-role-by-id first, to preserve any
                                        non-"permission"-typed claims before writing back the submitted permission set)
features/roles/api/get-permissions.ts -> lib/server/backend-api, lib/server/call-guard, features/roles/types/permission-definition
features/roles/api/get-all-roles.ts -> lib/server/backend-api, lib/server/call-guard, features/roles/types/role
                                        (fixed in a prior sync: was guardRawCall assuming a bare array; the endpoint
                                        actually wraps its response in the same envelope every other endpoint uses)
features/roles/api/{get-role-by-id,create-role,update-role,delete-role}.ts -> lib/server/backend-api, lib/server/call-guard, features/roles/types/role
                                        (all 4, plus get-permissions/get-all-roles above — changed this sync from
                                        lib/server/http to lib/server/backend-api)

features/notifications/index.ts     -> ./components/{notification-bell,notifications-page}, ./hooks/use-notifications,
                                        ./constants/{permissions,nav-item} (NOTIFICATIONS_PERMISSIONS, NOTIFICATIONS_NAV_ITEM),
                                        ./types/notification (NotificationStatus, NotificationDto)
features/notifications/constants/nav-item.ts -> lucide-react (Bell), ./permissions (NOTIFICATIONS_PERMISSIONS), types/nav
features/notifications/hooks/use-notifications.ts -> @microsoft/signalr, features/notifications/api/{get-my-notifications-action,
                                        get-unread-count-action,mark-notification-read-action,get-signalr-token-action},
                                        features/notifications/types/notification
features/notifications/components/notification-bell.tsx -> components/ui/{badge,button,dropdown-menu,empty,tabs},
                                        lib/shared/utils, features/notifications/hooks/use-notifications,
                                        features/notifications/types/notification (imported by components/layout/topbar.tsx
                                        via direct file path, not this feature's own barrel — see the exceptions note below)
features/notifications/components/notifications-page.tsx -> features/user-profile (resolveSession, barrel),
                                        features/notifications/api/get-notifications, features/notifications/components/notifications-data-table,
                                        lib/server/authorization (hasPermission), features/notifications/constants/permissions,
                                        features/notifications/types/notification — **no longer imports features/users
                                        (getAllUsers) this sync**; the notifications -> users barrel-level cross-feature
                                        edge is gone entirely, replaced by a different-shaped edge (component-level,
                                        direct-file, cross-feature) from user-select.tsx below
features/notifications/components/notifications-data-table.tsx -> components/shared/data-table (DataTable + types),
                                        components/ui/native-select (status filter — replaced components/ui/select plus an
                                        embedded "all" pseudo-option in a prior sync; a real, always-reselectable placeholder),
                                        features/notifications/components/{send-notification-dialog,user-select},
                                        features/notifications/types/notification — status + recipient filters render
                                        via DataTable's `customSearch` slot and no longer auto-navigate per change;
                                        local "pending" state is applied to the URL only when the customSearch
                                        "Search" button (`onCustomSearch`) is clicked. **No longer takes a `users`
                                        prop this sync** — the `usersById` map, `userLabel()` helper, and their
                                        `lib/shared/user-display` (getDisplayName)/`types/user` (UserDto) dependencies
                                        were removed entirely; the "To" column now renders `notification.toUserId`
                                        directly (matching the existing `fromName ?? fromUserId` fallback style
                                        already used for "From")
features/notifications/components/send-notification-dialog.tsx -> components/ui/{alert,button,dialog,input,label,textarea},
                                        components/toast (notifySuccess), hooks/use-action-success-toast
                                        (new this sync, replacing an inline useEffect), features/notifications/api/send-notification-action,
                                        features/notifications/components/user-select, lib/shared/user-display
                                        (new this sync — getDisplayName) — no longer takes a `users` prop. While
                                        updating UserSelect's `onValueChange` to the new `(user: UserDto) => void`
                                        signature, fixed a pre-existing latent bug: `FormValues.fromName` and
                                        `sendNotificationAction`'s `formData.get("fromName")` read both already
                                        existed, but no hidden `<input name="fromName">` was ever rendered, so an
                                        explicit "From" selection always submitted an empty fromName silently. Now
                                        selecting a "From" user sets `values.fromName = getDisplayName(user)` and a
                                        `<input type="hidden" name="fromName">` submits it. The "To" UserSelect just
                                        extracts `.id` — the backend's SendNotificationRequest has no `toName` field
features/notifications/components/user-select.tsx -> components/ui/{button,popover,command}, lucide-react,
                                        lib/shared/utils, features/users/api/search-users-action (new this sync —
                                        direct file import, cross-feature, bypassing @/features/users's barrel; see
                                        the barrel-bypass exceptions below), lib/shared/user-display, types/user —
                                        **completely rewritten this sync**: no longer built on components/ui/combobox
                                        (Combobox); now a bespoke component with its own `open`/`query`/`options`/
                                        `loading`/`selectedLabel` state. No default/first-page fetch on open anymore;
                                        a search only fires once the trimmed query reaches `MIN_SEARCH_LENGTH` (3
                                        characters — the component's own placeholder/comment describe this as "at
                                        least 3 characters," matching the guard
                                        `trimmedQuery.length < MIN_SEARCH_LENGTH`), debounced
                                        300ms, calling `searchUsersAction({searchValue, pageNumber: 1, pageSize: 10})`
                                        for page 1 of matches only. No longer takes a `users` prop at all.
                                        `onValueChange` changed from `(value: string) => void` to
                                        `(user: UserDto) => void` — hands back the full selected record, not just the
                                        id, so callers needing the display name (e.g. send-notification-dialog.tsx's
                                        "From" field) don't need a second lookup; `value` itself is still the
                                        selected id (string), used for the hidden `<input>` and the checkmark compare.
                                        Feature-owned, consumed by both notifications-data-table.tsx and
                                        send-notification-dialog.tsx within this same feature; not promoted to
                                        components/shared/, consistent with "only promote once 2+ features need it"
features/notifications/api/get-notifications.ts -> lib/server/backend-api, lib/server/call-guard, types/api, features/notifications/types/notification
                                        (changed this sync — was lib/server/http)
features/notifications/api/get-my-notifications.ts -> lib/server/backend-api, lib/server/call-guard, types/api, features/notifications/types/notification
                                        (changed this sync — was lib/server/http)
features/notifications/api/get-my-notifications-action.ts -> "use server"; features/user-profile (resolveSession),
                                        features/notifications/api/get-my-notifications
features/notifications/api/get-unread-count.ts -> lib/server/backend-api, lib/server/call-guard, types/api
                                        (changed this sync — was lib/server/http)
features/notifications/api/get-unread-count-action.ts -> "use server"; features/user-profile (resolveSession),
                                        features/notifications/api/get-unread-count — swallows any failure to `0`
                                        rather than surfacing an error (fire-and-forget-safe shape)
features/notifications/api/mark-notification-read.ts -> lib/server/backend-api, lib/server/call-guard, types/api, features/notifications/types/notification
                                        (changed this sync — was lib/server/http)
features/notifications/api/mark-notification-read-action.ts -> "use server"; features/user-profile (resolveSession),
                                        features/notifications/api/mark-notification-read — returns a boolean rather than {error?, success?}
features/notifications/api/send-notification.ts -> lib/server/backend-api, lib/server/call-guard, types/api
                                        (changed this sync — was lib/server/http)
features/notifications/api/send-notification-action.ts -> "use server"; features/user-profile (resolveSession),
                                        features/notifications/api/send-notification, lib/shared/user-display (getDisplayName)
features/notifications/api/get-signalr-token-action.ts -> "use server"; features/user-profile (resolveSession),
                                        lib/server/refresh-session (refreshSessionIfNearExpiry — new this sync),
                                        lib/server/persist-session-cookie (persistSessionCookie — new this sync) —
                                        now proactively refreshes a near-expiry session before handing back its
                                        access token (proxy.ts's own proactive-refresh middleware excludes `/api`
                                        paths, so a session left open on one page without navigating could
                                        otherwise hand SignalR a stale/expired token and 401), persisting the
                                        rotated session via persistSessionCookie() when a refresh happened; still
                                        the one deliberate place the token leaves the httpOnly cookie boundary

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
lib/server/refresh-session.ts       -> features/auth/api/refresh-token (direct file, not the barrel), lib/server/jwt,
                                        lib/server/session-cookie (REFRESH_LEAD_MS — changed this sync, moved in
                                        from proxy.ts's own inline check), types/session — exports `refreshSession()`
                                        (unconditional rotate) and, new this sync, `refreshSessionIfNearExpiry(session)`
                                        (gates the rotate on REFRESH_LEAD_MS, returns null both when not due and when
                                        attempted and failed); called from src/proxy.ts and, new this sync,
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
lib/server/jwt.ts                   -> types/user (ClaimDto) — decodes a JWT payload without verifying its signature;
                                        no other lib/server/* dependency (new)
lib/server/build-session-claims.ts  -> lib/server/jwt (extractAllClaims), lib/shared/dedupe-claims, types/user (new)
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
                                        (a factory for the 3 call sites that run before a session cookie exists:
                                        login.ts, refresh-token.ts, get-current-user.ts)
lib/server/config.ts                -> lib/server/api-clients (ApiClientName) — changed this sync: `getApiBaseUrl()`
                                        now takes `client: ApiClientName = ApiClients.Backend`, resolving the env var
                                        per-client via an internal map (`Backend` -> `API_BASE_URL`, the only entry today)
lib/server/authorization.ts         -> lib/shared/authorization (delegates to it), types/session (SessionData) —
                                        now a thin wrapper preserving the original `(session, userName, permission)`
                                        call-site signature for existing server callers (was previously self-contained)
lib/shared/authorization.ts         -> no internal dependency (SUPER_ADMIN_USERNAMES, isSuperAdminUser,
                                        hasPermission/hasAnyPermission/hasAllPermissions; new — safe for both server
                                        and client, takes `permissions: string[]`/`userName` directly rather than a
                                        `SessionData`-shaped object, which is what makes it usable from Sidebar)
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
                                        (see Key Design Patterns)
components/foundation/portal-container.ts (new) -> react (createContext/useContext) only — no
                                        components/ui/ or components/foundation/ dependency of its own;
                                        consumed by components/ui/{dialog,popover}.tsx
```

Direction is still one-way: `components/ui/*` never imports from `components/layout/*`, `components/theme/*`, or `features/*`. Cross-feature imports go through a feature's `index.ts` barrel, not its internals — e.g. `features/auth/api/login-action.ts` and `features/users/components/users-page.tsx`/`api/create-user-action.ts` all import `resolveSession`/`getCurrentUser` from `@/features/user-profile` (the barrel), not from its internals directly. `features/dashboard` is still not imported by any other feature. `features/users` and `features/roles` are each imported by their own `app/**/page.tsx` (routing, not another feature). `components/shared/data-table/*`, `components/shared/object-viewer/*`, and `components/toast/*` sit below `features/*` in the same leaf-adjacent tier as `components/ui/*` — they're imported by feature code but import nothing from `features/*` themselves (`object-viewer/*` currently has no importer at all — purely additive). `components/foundation/*` and `components/command/*` sit at that same tier too — `foundation/*` imports only `@floating-ui/react`/`@tanstack/react-virtual`/`react`, `command/*` imports only `foundation/*` plus `@floating-ui/react`/`lucide-react`, and neither imports from `features/*`. `command/*` still has no importer at all outside this library itself — purely additive, same as `object-viewer/*`. The one narrow exception to "`components/ui/*` never imports from another component tier": `components/ui/dialog.tsx` and `components/ui/popover.tsx` both import `components/foundation/portal-container.ts` — a plain React Context with no further dependency of its own, added specifically so a `Popover`/`Combobox` nested inside a `Dialog` can portal into the Dialog's own DOM node (see Key Design Patterns). `components/select/*` — the library `AsyncSelect`/`MultiSelect` lived in — is gone entirely, not just unimported.

**Changed this sync**: the `features/users -> features/roles` (role catalog) and `features/notifications -> features/users` (`getAllUsers`) cross-feature barrel edges from the previous sync are both gone. `features/users/components/users-page.tsx` no longer imports `@/features/roles` at all — the role picklist moved to `features/users/components/edit-user-dialog.tsx`, which now imports `@/features/roles/api/get-all-roles-action` directly (a new barrel-bypass exception, see below). `features/notifications/components/notifications-page.tsx` no longer imports `@/features/users` at all — the recipient search moved to `features/notifications/components/user-select.tsx`, which now imports `@/features/users/api/search-users-action` directly (another new barrel-bypass exception). Both replacements are component-level, direct-file, cross-feature imports rather than page-level barrel imports — a different shape from what they replaced.

Five deliberate, narrow exceptions to the barrel-only rule exist, all driven by the RSC client/server boundary rather than an oversight: `constants/nav-items.ts` imports `DASHBOARD_NAV_ITEM`/`USERS_NAV_ITEM`/`ROLES_NAV_ITEM`/`NOTIFICATIONS_NAV_ITEM` by direct file path (`@/features/dashboard/constants/nav-item`, etc.) rather than via each feature's barrel; `components/layout/user-menu.tsx` imports `logoutAction` directly from `@/features/auth/api/logout-action` rather than from `@/features/auth`'s barrel (which, notably, does not currently re-export `logoutAction` at all); `components/layout/topbar.tsx` imports `NotificationBell` directly from `@/features/notifications/components/notification-bell` rather than from `@/features/notifications`'s barrel — here the barrel *does* also export `NotificationBell`, but it additionally re-exports `NotificationsPage`, an async Server Component (`resolveSession()`, `next/headers`), so importing the barrel from this Client Component reproduced the same "next/headers only available in Server Components" build error the `nav-items.ts` exception was already working around; and, **new this sync**: `features/users/components/edit-user-dialog.tsx` imports `getAllRolesAction` directly from `@/features/roles/api/get-all-roles-action` rather than `@/features/roles`'s barrel, and `features/notifications/components/user-select.tsx` imports `searchUsersAction` directly from `@/features/users/api/search-users-action` rather than `@/features/users`'s barrel — both for the same reason as the others: the target barrel also re-exports an async Server Component (`RolesPage`/`UsersPage` respectively) that calls `resolveSession()`/reads cookies via `next/headers`, so importing the full barrel from these `"use client"` components would drag that server-only chain into the client bundle. See Key Design Patterns for the reasoning.

## Key Design Patterns

- **Feature-folder + barrel-export convention**: each `features/<name>/` owns `api/` (one file per backend endpoint), `components/`, optionally `types/` (only for types with exactly one consumer — `features/roles/types/{role,permission-definition}.ts`, `features/user-profile/types/user-session.ts`) and optionally `constants/` (a feature-owned permission-string file, e.g. `features/users/constants/permissions.ts`, and now — new this sync — a `nav-item.ts` per nav-bearing feature), and an `index.ts` that is the only sanctioned import surface for other features or `app/*`. **Five narrow, deliberate exceptions** to the barrel-only rule exist (see Dependency Direction): `constants/nav-items.ts`, `components/layout/user-menu.tsx`, `components/layout/topbar.tsx`, and — new this sync — `features/users/components/edit-user-dialog.tsx` (imports `@/features/roles/api/get-all-roles-action`) and `features/notifications/components/user-select.tsx` (imports `@/features/users/api/search-users-action`) each import one specific file directly rather than through a barrel, to avoid pulling a barrel's other, server-only exports (async Server Components, cookie-reading API functions) into a client bundle.
- **Each nav-bearing feature owns its own `NavItem` metadata** (extended this sync): `features/{dashboard,users,roles,notifications}/constants/nav-item.ts` each export one `NavItem` (label, href, icon, and — where relevant — the permission that gates it), re-exported from that feature's barrel. `constants/nav-items.ts` imports these four constants **by direct file path**, not via each feature's barrel, and assembles them into `NAV_ITEMS` alongside two nodes it still declares itself (the "Identity" group, since it spans two features, and "Settings", which has no owning feature or page at all). The direct-file-path import is intentional, not an oversight: `nav-items.ts` is imported by the client-side `Sidebar` component, and `features/users/index.ts`/`features/roles/index.ts`'s barrels also re-export server-only code (`UsersPage`/`RolesPage`, async Server Components calling `resolveSession()`, which reads cookies via `next/headers`) — importing the full barrel from client code would drag that server-only chain into the client bundle (this was tried and produced a real "next/headers only available in Server Components" build error before being fixed this way). The `nav-item.ts` files themselves are plain data (an icon reference plus strings) with no server/client-bound dependency, so importing them directly is safe. `components/layout/sidebar.tsx` then computes the permission-filtered menu client-side via `lib/shared/menu.ts`'s `buildVisibleMenu(NAV_ITEMS, can)`, where `can` is built from the new `lib/shared/authorization.ts` (`hasPermission`) using the `permissions`/`userName` props `AppShell` passes down from `resolveSession()`.
- **Fetch full detail on dialog open, when the list endpoint's DTO is incomplete — now generalized to self-fetching the picklist too** (changed this sync): both `edit-user-dialog.tsx` and `edit-role-dialog.tsx` call a dedicated `get-*-detail-action.ts` in a `useEffect` on mount rather than trusting the row data they were opened with. This exists because the backend's list-returning service methods (`UserService.SearchAsync`/`GetAllAsync`, `RoleService.GetAllAsync` — their shared `DataMapper.cs` projection) never populate `Roles`/`Claims`; only the single-record fetch (`GetByIdAsync`) does. Relying on the row data silently produced an empty roles/claims checklist that, on save, would have wiped out anything the record actually had — worth remembering before building another list-backed feature against this backend. **New this sync**: the same "fetch on open, don't trust what the list page preloaded" idea now also covers the *picklist itself* — `users-page.tsx`/`roles-page.tsx` no longer fetch the role catalog/permission catalog and pass them down as props; instead each edit dialog runs `Promise.all([<detail fetch>, <picklist fetch>])` on open (`getAllRolesAction()`/`getPermissionsAction()` respectively), so both list pages now issue one fewer API call on page load.
- **On-demand user search, replacing a preloaded full user list** (new this sync): `features/notifications/components/user-select.tsx` no longer takes a `users` prop or does client-side filtering over a preloaded list. It debounces (300ms) a call to `searchUsersAction({searchValue, pageNumber: 1, pageSize: 10})` once the trimmed query reaches a minimum length (`MIN_SEARCH_LENGTH = 3`, checked via `trimmedQuery.length < MIN_SEARCH_LENGTH`, matching the component's own placeholder/comment), fetching only the first page of matches — never the whole user list. Its `onValueChange` hands back the full selected `UserDto`, not just an id, so a caller needing the display name too (`send-notification-dialog.tsx`'s "From" field) doesn't need a second lookup.
- **Routing files are pure re-exports**: every `app/**/page.tsx` is a one-line `export { X as default } from "@/features/<name>";` — no logic lives in `app/`.
- **Server-only API layer, one function per endpoint file**: `lib/server/http.ts` (`requestJson`/`requestVoid`) is the single fetch wrapper; every `features/*/api/*.ts` file wraps exactly one backend call and returns a normalized result via `lib/server/call-guard.ts`'s `guardCall`/`guardResponseCall`/`guardRawCall` (chosen based on whether the backend endpoint returns a `Result<T>` envelope, a bare `ApiResponse`, or a raw value/array).
- **Encrypted cookie session, refreshed proactively by `proxy.ts`, no per-request live fetch** (changed this sync): `lib/server/session.ts`'s `getSession()` reads and decrypts the `admin_session` cookie; `features/user-profile/api/resolve-session.ts` is now a thin passthrough to it, rather than composing it with its own live `getCurrentUser()` call. All the "keep this session fresh" work now lives in `src/proxy.ts` instead: on every request it decrypts/validates the cookie (`lib/server/parse-session.ts`), proactively rotates the access/refresh token when close to expiry (`lib/server/refresh-session.ts`, calling the backend's `token/token/refresh`), and refetches profile/claims on hard navigations or right after a rotation — writing the result back as a freshly-encrypted cookie on both the request and the response. This replaces the previous design (`resolveSession()` doing a live fetch on every server-rendered request that needed it) with a single, centralized refresh point.
- **Session encryption via a dedicated `lib/server/token-cipher.ts`** (new): `encrypt()`/`decrypt()` wrap Node's `crypto` module (AES-256-GCM, keyed by `TOKEN_ENCRYPTION_KEY`), producing an `"iv.authTag.ciphertext"` (all base64) string; `decrypt()` returns `null` rather than throwing on any malformed/tampered input, which `parse-session.ts` treats the same as "no session". `lib/server/config.ts`'s `getTokenEncryptionKey()` throws if the env var is unset — the cookie's "plaintext for now" state from the previous sync is fully resolved.
- **Session near-expiry refresh and cookie-persist logic centralized into two shared helpers, replacing three separate hand-rolled copies** (new this sync): `lib/server/refresh-session.ts` gained `refreshSessionIfNearExpiry(session)` — wraps the existing `refreshSession()` (unconditional rotate), gated on `session.expiresAt - Date.now() <= REFRESH_LEAD_MS`, returning `null` both when a refresh wasn't due and when it was attempted and failed. `src/proxy.ts` now calls this instead of its own inline `REFRESH_LEAD_MS` check + `refreshSession()` call (pure dedup — `REFRESH_LEAD_MS` moved from being imported directly by `proxy.ts` to living behind this helper); `features/notifications/api/get-signalr-token-action.ts` now calls it too, proactively refreshing before handing an access token to the browser for the SignalR handshake — closing a gap where `proxy.ts`'s own middleware matcher excludes `/api` paths, so a session left open on one page (no navigation) could otherwise hand SignalR a stale token and get a 401. A new `lib/server/persist-session-cookie.ts` (`persistSessionCookie(session)`) similarly centralizes the Server-Action-context cookie write (`cookies()` from `next/headers`, `buildSessionCookieOptions()`), replacing hand-rolled `cookies().set(...)` calls in `get-signalr-token-action.ts` and `features/auth/api/refresh-session-action.ts` (the manual, super-admin-gated refresh action, which still calls `refreshSession()` directly/unconditionally rather than the near-expiry-gated helper, since it's an explicit user-triggered rotate, not a proactive one) — deliberately not importable from `proxy.ts`, whose middleware/Edge-adjacent context uses `NextRequest`/`NextResponse` cookie APIs instead of `next/headers`.
- **Permissions/roles decoded from the JWT, never trusted from the profile API** (new): `lib/server/jwt.ts` decodes the access token's payload (no signature verification — safe here since the token was just issued by this app's own backend) and extracts the `permission`/`role` claim types; `lib/server/build-session-claims.ts` unions those with the profile API's own claims for display purposes only. Both `loginAction` and `refreshSession()` independently re-derive `permissions`/`roles` this way on every token issuance.
- **Context-provider-per-concern for client state**: `SidebarProvider`/`useSidebar`, `AccentColorProvider`/`useAccentColor`, and `next-themes`' provider (wrapped in `components/theme/theme-provider.tsx`) each own one slice of persisted UI state via a throwing custom hook — unchanged pattern, relocated files.
- **Owned, CLI-generated UI primitives**: `components/ui/*` (23 shadcn-CLI-generated files, style `"radix-nova"`) still follows the `data-slot="<name>"` + `cva()` convention. `button.tsx` retains its hand-modification beyond CLI output: a `loading` prop (renders `Spinner`, sets `aria-busy`/`disabled`) plus a `cursor-pointer` utility baked into `buttonVariants`. `native-select.tsx` is the one hand-written exception in this folder — see the next pattern for why it and the rest of the new select family aren't CLI/Radix-based.
- **The internal Floating-UI select library from the previous sync is gone; single-select now goes through a shadcn-style `Combobox`** (changed this sync): `components/ui/combobox.tsx` (`Combobox<TValue>`) composes `components/ui/{button,popover,command}.tsx` — `popover.tsx` is a new, otherwise-ordinary Radix `Popover` wrapper (`data-slot`/`cn` conventions matching the rest of `ui/`), `command.tsx` wraps the new `cmdk` dependency for the filterable list. This replaces both `EntitySelect` (button-triggered, non-searchable) and `SearchSelect` (text-input-triggered, filterable) with one component — `Command`'s built-in search input covers both cases, so the two consumers (`edit-user-dialog.tsx`'s status/authProvider, `features/notifications/user-select.tsx`) needed no prop changes beyond the import. `AsyncSelect`/`MultiSelect` were deleted rather than ported — neither ever had a UI consumer, so there was nothing to preserve; a remote-search or multi-value variant can be built on `Combobox` if a future feature needs one. `components/command/*` (Command Palette) still builds directly on `components/foundation/*`/`@floating-ui/react`, unrelated to and untouched by this change — it still reuses `floating-overlay.tsx` rather than Radix `Dialog` specifically because Radix `Dialog` locks body scroll by default and the Palette's overlay explicitly must not (`FloatingOverlay`'s `lockScroll={false}`).
- **A Popover nested inside a Dialog couldn't be scrolled with the mouse wheel — fixed via a shared portal-container context** (new this sync, found while building `Combobox`): Radix `Dialog`'s modal scroll lock (`react-remove-scroll`, active while the dialog is open) only treats content that is an actual DOM descendant of `DialogContent`'s own node as "inside" the locked region; anything portaled elsewhere — which is what `Popover.Portal` does by default (`document.body`) — has its wheel/touch scroll blocked as if it were page background, even though it renders visually on top and in the right place. The fix has two parts, both in `components/ui/dialog.tsx`: (1) `DialogContent` now centers via a `flex items-center justify-center` wrapper `div` instead of a `transform` (`-translate-x-1/2 -translate-y-1/2`) on the content node itself — a `transform` on an ancestor creates a new CSS containing block for `position: fixed` descendants, which would have broken a nested Popover's floating-position math the moment its portal target moved inside `DialogContent`; the wrapper is `pointer-events-none` with `pointer-events-auto` on the content itself, so backdrop clicks still reach `DialogOverlay` exactly as before. (2) `DialogContent` captures its own DOM node (`ref={setPortalNode}`) and provides it through a new `components/foundation/portal-container.ts` context (`PortalContainerProvider`/`usePortalContainer`); `components/ui/popover.tsx`'s `PopoverContent` reads that context and passes it as `Popover.Portal`'s `container` prop, falling back to Radix's own `document.body` default outside a Dialog. This is automatic for any future `Combobox`/`Popover` nested in a `Dialog` — no per-call-site wiring needed, and `send-notification-dialog.tsx`'s `Combobox`-based user picker (the bug's original repro) needed no changes itself.
- **A sidebar nav group containing the active route could never be manually collapsed — fixed by separating "default" from "explicit override"** (new this sync): `sidebar-nav-item.tsx` used to compute `expanded = hasChildren && (isExpanded(item.href) || branchActive)` — since `branchActive` (true whenever a descendant route is the current page) was OR'd in, a group containing the active page stayed forced-open no matter how many times its toggle was clicked; `toggleExpanded` could only ever flip `isExpanded`, which the `||` made irrelevant. `hooks/use-sidebar.tsx` now stores explicit per-href overrides in a `Map<string, boolean>` (`expandedOverrides`) rather than a plain expanded-`Set`; `isExpanded(href)` returns `boolean | undefined` (`undefined` = no override yet, so the group still auto-expands to reveal the active route by default), and `toggleExpanded(href, current)` writes an explicit `!current`, which now wins over `branchActive` once set. Persisted to `localStorage` the same way as before, just serialized as `[...map.entries()]` instead of a plain string array.
- **Single-CSS-variable theming** and **runtime accent swap via DOM attribute + localStorage**: unchanged from before — `--primary` drives themed surfaces, `AccentColorProvider` sets `data-accent` on `<html>`. **New this sync**: a seventh preset, `black`, was added to `ACCENT_COLORS` — the one preset with no Tailwind shade scale (just the flat `--color-black`/`--color-white` tokens), so unlike every other preset (which steps one shade lighter for dark mode, e.g. `-600` light / `-500` dark) it inverts across themes instead (`--color-black` light / `--color-white` dark), per new rules in `globals.css`. `accent-color-picker.tsx` gained a `swatchColor(value)` helper to render its dropdown swatch accordingly — `var(--color-black)` for the `black` preset, `var(--color-${value}-600)` for every other preset (previously the swatch always assumed a `-600` shade existed).
- **Hydration-safe browser-state restoration**: unchanged pattern (`hydrated` flag + `useEffect`, `eslint-disable react-hooks/set-state-in-effect`) in `SidebarProvider` and `AccentColorProvider`; `components/theme/use-has-mounted.ts` (`useSyncExternalStore`) still guards `ThemeToggle`.
- **Mobile drawer closes on route change via render-time state adjustment**: unchanged, still in `hooks/use-sidebar.tsx`.
- **Generic, presentational `DataTable<TData>` building block**: `components/shared/data-table/` composes a toolbar (actions + search + a caller-rendered buttons node), a table body (skeleton-loading rows, an `Empty` state, or an `Alert`-based error state that replaces the body and hides pagination), and a windowed-pagination footer (`getPageWindow()` always keeps page 1/last visible plus siblings around the current page). It takes no dependency on any feature or data-fetching library — fully controlled via props (`data`, `columns`, `isLoading`, `error`, callbacks); `UsersDataTable` (server-driven search via URL params), `RolesDataTable` (local client-side filtering — there's no backend search endpoint for roles), and `NotificationsDataTable` (a custom multi-field filter UI, see below) reuse it as-is with different search wiring. `isLoading` takes priority over a stale `error` — an in-flight refetch (e.g. clicking Refresh) always shows the table's own skeleton-row loading state rather than a leftover error from a previous failed load. Optional per-column client-side sorting (`DataTableColumn.sortable`/`sortValue`) — a sortable header renders as a `<button>` cycling asc → desc → unsorted with `ArrowUp`/`ArrowDown`/`ArrowUpDown` icons, and the table body sorts a `useMemo`-derived copy of `data`; explicitly scoped to callers holding the full result set client-side — `RolesDataTable` uses it (name/description columns), `UsersDataTable` deliberately does not, since it paginates via the backend and only ever holds one page of `data` at a time. An optional `mode` prop (`"paginated"` default / `"virtualized"` / `"infinite"`) switches the row/header markup to an ARIA-grid div layout rendered via `data-table-virtual-body.tsx` (a native `<table>` can't virtualize its rows cleanly); an optional `onSortChange` lets a caller take over sorting server-side instead of the default client-side sort, a prerequisite for `"virtualized"`/`"infinite"` modes against a large or streamed dataset — both additive, no existing caller (`UsersDataTable`, `RolesDataTable`, `NotificationsDataTable`) passes either, so all three keep today's exact `"paginated"` behavior. **New this sync — restructured toolbar and extracted buttons**: `data-table-toolbar.tsx` now renders three stacked sections (actions / search / an inserted `Separator` between rendered sections) instead of one combined row, and gained a `customSearch?: React.ReactNode` prop for a caller-supplied multi-field filter UI plus `onCustomSearch?: () => void`, which renders an explicit "Search" button — custom filters apply on click, unlike the built-in single-field text search, which stays auto-debounced (400ms). The toolbar no longer builds the Export/Refresh/Columns cluster itself; the new `data-table-buttons.tsx` (`DataTableButtons`) does, wrapped in the new `components/ui/button-group.tsx` (`ButtonGroup`). `data-table.tsx` computes this buttons node and either passes it to the toolbar as `buttons` (default layout) or, when `customSearch` is used, renders it itself inside the table's own bordered content box (right-aligned, above the content) — the content box (`rounded-md border border-border`) now always wraps the table/empty/error/virtualized body, not just in the `customSearch` case. `NotificationsDataTable` is the first and only consumer of `customSearch` so far — its status + recipient filter dropdowns moved out of a standalone block above `<DataTable>` into this slot, holding local "pending" state applied to the URL only when "Search" is clicked, rather than auto-navigating on every change.
- **Controlled form state alongside `useActionState`, for Server Action forms that can fail**: dialogs bound to a mutation Server Action keep their own `useState<FormValues>` in parallel with `useActionState(...)`. This is deliberate, not redundant — React resets *uncontrolled* form fields once a Server Action settles, regardless of success or failure, which would silently wipe user input after a validation error; controlled state survives that reset.
- **Force-remount via a bumped `key` to reset `useActionState`**: `useActionState` has no imperative "clear this error/state" API, so each `*DataTable` bumps a per-dialog key counter on every open (`createDialogKey`, `editDialogKey`, ...) and passes it as that dialog's React `key`, forcing a fresh component instance (fresh action state, fresh controlled form state, and — for the edit dialogs — a fresh detail-fetch) each time it's opened.
- **Toast notifications via a themed `sonner` wrapper**: `components/toast/` never exposes `sonner`'s `toast` directly — call sites use `notifySuccess`/`notifyError` (`notify.ts`), and the visual theme (`saturatedToastOptions`, `withToastProgress()`) is centralized in `toast-theme.ts` so every toast in the app looks consistent without each call site repeating class names.
- **Backend error messages surfaced through the shared `send()` wrapper**: `lib/server/http.ts`'s `extractErrorMessage()` centralizes turning a non-2xx response body into a human-readable string (envelope message → validation-errors map → `ProblemDetails.title` → generic fallback), so every `features/*/api/*.ts` call gets real error text without each call site parsing the body itself.
- **Real-time push via SignalR, browser-authenticated with a short-lived, action-issued access token; now a direct browser-to-backend connection with retry** (changed this sync): `use-notifications.ts` opens a `HubConnection` directly from the browser to `process.env.NEXT_PUBLIC_SIGNALR_HUB_URL` — an absolute backend URL. **Changed this sync**: previously a same-origin relative path (`/api/signalr-hub`) proxied to the backend via `next.config.ts`'s `rewrites()`, which is now deleted entirely (`next.config.ts` is back to an empty `NextConfig`); the browser now depends on backend CORS being configured for the admin origin (assumed, not verified this sync). Authenticated via `accessTokenFactory` calling `getSignalRTokenAction()` — a Server Action that hands the browser a short-lived access token specifically for this handshake, since the real session token stays in an httpOnly cookie the browser can't read otherwise (and, new this sync, proactively refreshes it first if near expiry — see the session-refresh-helpers pattern above). On any `SystemMessage` push it calls `refresh()` (refetches the list + unread count) rather than merging the pushed payload into state, because the payload is the raw backend `SystemMessage`, not a full `NotificationDto`. **New this sync**: `connectSignalR` is now a retryable inner function — a failed `connection.start()` logs a sanitized error via a new `sanitizeSignalRErrorMessage()` helper (strips the raw HTML negotiate-failure response body the SignalR client embeds in its own error message, since that body isn't JSON/XML) and retries after 30 seconds via `setTimeout`, cleared on unmount alongside the existing connection teardown.
- **Every backend API response is envelope-wrapped — never assume a bare value** (new, learned from a real bug): `get-all-users.ts` (pre-existing, previously uncalled) assumed `GET user` returned a bare `UserDto[]` (`guardRawCall`) and broke (`users.map is not a function`) the first time something actually called it — the endpoint, like every other endpoint in this backend, wraps its response in the `Result`/`ApiResponse` envelope. Several of the new `features/notifications/api/*.ts` files had the same wrong assumption and were fixed the same way. Treat `guardRawCall` as almost never correct for this backend; verify against a sibling endpoint's actual response shape rather than assuming.
- **Auth-token injection moved out of `http.ts` into a request-handler pipeline, decoupling the API layer from sessions** (new this sync): `lib/server/http.ts`'s `RequestOptions.accessToken?: string` is gone, replaced by `handlers?: HttpRequestHandler[]` (`(context: HttpRequestContext) => Promise<void> | void`, `context.headers` a mutable `Record<string, string>`) run in order by `send()` before `fetch()` — `http.ts` no longer imports or knows about sessions at all. A new `lib/server/backend-api.ts` re-exports `requestJson`/`requestVoid` pre-wired with `handlers: [bearerTokenHandler, ...]` and `client: ApiClients.Backend` (a new `lib/server/api-clients.ts` named-client registry, room for more entries later); nearly every `features/*/api/*.ts` file now imports from `backend-api.ts` instead of `http.ts`, and those functions no longer take an `accessToken` parameter. `lib/server/http-handlers/bearer-token-handler.ts` exports `bearerTokenHandler` (reads the ambient session via `getSession()`) and `explicitBearerTokenHandler(accessToken)`, a factory used by the 3 call sites that run before a session cookie exists — `login.ts` (via `login-action.ts`), `refresh-token.ts`, and `get-current-user.ts` (called from both `login-action.ts` and `proxy.ts`'s Edge middleware, which has no `next/headers` session access) — which still call `http.ts` directly. Every mutation `*-action.ts` Server Action stopped extracting/passing `session.accessToken` to its api function but kept `resolveSession()` + the "session expired" early-exit for the friendly UX message.
- **Client-side toast/pending feedback centralized into two small hooks, replacing per-dialog duplication** (new this sync): `hooks/use-guarded-action.ts`'s `useGuardedAction()` (`[pending, run]`, built on `useTransition`) wraps the "run an imperative action, toast the result, track pending" pattern for non-form calls — applied to `delete-user-dialog.tsx`/`delete-role-dialog.tsx` (their confirm-dialog UI itself was left untouched, a deliberate scope decision). `hooks/use-action-success-toast.ts`'s `useActionSuccessToast(state, successMessage, onSuccess?)` is a `useEffect` wrapper that toasts and runs `onSuccess` once a `useActionState`-bound result's `.success` turns true — applied at 6 call sites across 5 form dialogs (`create-user-dialog.tsx`, `create-role-dialog.tsx`, `send-notification-dialog.tsx`, `edit-role-dialog.tsx`, `edit-user-dialog.tsx` — 2 call sites, the update form and the password-reset form), replacing a `useEffect` + `notifySuccess(...)` pattern previously duplicated in each.

## Shared Kernel / Common Building Blocks Used

- `components/ui/*` — the app's own primitive layer.
- `components/ui/combobox.tsx` (plus its `popover.tsx`/`command.tsx` primitives) — the shadcn-style single-select building block, replacing `components/select/*`'s `EntitySelect`/`SearchSelect` (see Key Design Patterns). **Narrower this sync**: only one consumer left — `features/users` (edit dialog, status/authProvider). `features/notifications/components/user-select.tsx` no longer uses it (rewritten this sync into a bespoke on-demand-search component; see Key Design Patterns).
- `components/foundation/` — shrunk this sync to `use-listbox.ts`/`use-virtual-list.ts`/`floating-overlay.tsx` (serving only `components/command/*` and, for `use-virtual-list.ts`, `components/shared/data-table/data-table-virtual-body.tsx`) plus the new `portal-container.ts` (serving `components/ui/{dialog,popover}.tsx`, see Key Design Patterns). No longer backs any select/combobox component.
- `components/command/*` — Command Palette (Cmd/Ctrl+K), untouched this sync; still no consumer wired into the app.
- `components/shared/data-table/` — generic list-table building block (toolbar, `data-table-buttons.tsx` for the Export/Refresh/Columns cluster — new this sync, built on `components/ui/button-group.tsx` — pagination, loading/empty/error states, optional per-column sort, optional virtualized/infinite modes via `data-table-virtual-body.tsx`, and — new this sync — an optional `customSearch`/`onCustomSearch` slot for a caller-supplied, apply-on-click multi-field filter UI); consumed by `features/users`, `features/roles`, and `features/notifications` (with different search/sort strategies — see Key Design Patterns), designed with no feature-specific knowledge baked in.
- `components/shared/object-viewer/` — new this sync, additive; a recursive read-only object/table renderer built on `components/ui/table` + `components/ui/input`, no `features/*` dependency. Not yet consumed by any page.
- `components/toast/` — toast notification wrapper around `sonner`; mounted once at the root layout, called from feature code via `notifySuccess`/`notifyError`.
- `lib/shared/utils.ts` (`cn`), `lib/shared/dedupe-claims.ts`, `lib/shared/user-display.ts` — cross-cutting helpers consumed by 2+ features or by layout chrome. New this sync: `lib/shared/menu.ts` (`buildVisibleMenu`, consumed by `Sidebar`) and `lib/shared/authorization.ts` (`hasPermission`/`hasAnyPermission`/`hasAllPermissions`/`isSuperAdminUser`, safe for both server and client — consumed directly by `Sidebar` and, via `lib/server/authorization.ts`'s thin wrapper, by `UsersPage`/`RolesPage`/`NotificationsPage`).
- `lib/server/*` — the server-only building blocks every feature's `api/` layer is built on: `http.ts`, `call-guard.ts`, `config.ts`, `session.ts`, `session-cookie.ts`, `authorization.ts` (now a thin wrapper over `lib/shared/authorization.ts`), plus the session-encryption/refresh chain — `token-cipher.ts`, `jwt.ts`, `build-session-claims.ts`, `parse-session.ts`, `refresh-session.ts` (exports both `refreshSession()` and, new this sync, `refreshSessionIfNearExpiry()`), and, new this sync, `persist-session-cookie.ts` — used by `src/proxy.ts`, `features/auth/api/{login,logout,refresh-session}-action.ts`, and `features/notifications/api/get-signalr-token-action.ts`.
- Permission-string constants remain **not** a shared kernel piece — per-feature `features/{users,roles,notifications}/constants/permissions.ts` (`USERS_PERMISSIONS`, `ROLES_PERMISSIONS`, `NOTIFICATIONS_PERMISSIONS`), each mirroring the backend's own permission-string constants for that module. Nav metadata follows the same per-feature-ownership move (`features/{dashboard,users,roles,notifications}/constants/nav-item.ts`), assembled (not owned) by the top-level `constants/nav-items.ts`.
- `features/notifications/components/user-select.tsx` is a **feature-owned** reusable component (on-demand searchable user picker — rewritten this sync off `components/ui/combobox.tsx` onto a bespoke `components/ui/{button,popover,command}.tsx` composition, see Key Design Patterns), not promoted to `components/shared/` — consumed by two components within the same feature, not yet by a second feature, consistent with the "only promote once 2+ features need it" pattern.
- `hooks/*`, `components/theme/*` — cross-cutting building blocks consumed by layout components.
- No package or code is shared with another client app — `clients/` still contains only `admin`.

## Module/Route Boundaries

Two route groups/areas exist:
- `(dashboard)` — wraps `/`, `/user-profile`, `/identity/users`, `/identity/roles`, and now `/notifications` with `resolveSession()` + `AppShell` (top bar, sidebar).
- `login` (ungrouped) — `/login`, rendered under the root layout only, no `AppShell`/session resolution.

`constants/nav-items.ts` declares `/identity`, `/identity/users`, `/identity/roles`, `/settings`, and now `/notifications` (a top-level item between "Identity" and "Settings"). `/identity/users`, `/identity/roles`, and `/notifications` all have real routes and pages, gated on `USERS_PERMISSIONS.View`/`ROLES_PERMISSIONS.View`/`NOTIFICATIONS_PERMISSIONS.Read` respectively (the "Send" action on `/notifications` is additionally gated on `NOTIFICATIONS_PERMISSIONS.Send`); `/settings` still has no corresponding route.

Feature isolation is enforced by convention (barrel-only cross-feature imports), verified in the places it's exercised so far: `features/auth/api/login-action.ts` and `features/users/components/users-page.tsx`/`api/create-user-action.ts` all import from `@/features/user-profile`'s barrel, not its internals. **Changed this sync**: the two page-level cross-feature barrel edges from the previous sync are both gone — `features/users/components/users-page.tsx` no longer imports `@/features/roles` at all (it no longer fetches the role catalog), and `features/notifications/components/notifications-page.tsx` no longer imports `@/features/users` at all (it no longer fetches `getAllUsers`). Each is replaced by a component-level, direct-file, cross-feature edge instead: `features/users/components/edit-user-dialog.tsx` imports `@/features/roles/api/get-all-roles-action` directly, and `features/notifications/components/user-select.tsx` imports `@/features/users/api/search-users-action` directly — both are two of the five barrel-bypass exceptions below, not barrel imports. Five narrow, reasoned exceptions to the barrel-only rule exist (`constants/nav-items.ts`, `components/layout/user-menu.tsx`, `components/layout/topbar.tsx`, `edit-user-dialog.tsx` -> `get-all-roles-action`, `user-select.tsx` -> `search-users-action` — see Key Design Patterns / Dependency Direction); a sixth, lower-level one also exists at the `lib/server` tier: `lib/server/refresh-session.ts` imports `features/auth/api/refresh-token.ts` directly (bypassing the `@/features/auth` barrel, which does export `refreshToken`) — worth noting since it's also a reversal of the usual `features/* -> lib/server/*` dependency direction (here, `lib/server` reaches into a feature's `api/` file), unlike the other exceptions which stay within the normal direction.

## Known Architectural Risks / Debt

| Finding | Severity | Notes |
|---|---|---|
| ~~Session cookie stores the access/refresh token and claims as plaintext JSON~~ — **resolved this sync** | — | The cookie is now AES-256-GCM encrypted (`lib/server/token-cipher.ts`, keyed by `TOKEN_ENCRYPTION_KEY`, which is a hard requirement — `getTokenEncryptionKey()` throws if unset). See Auth Flow (overview.md) / Key Design Patterns. |
| ~~No token refresh flow wired up~~ — **resolved this sync** | — | `src/proxy.ts` now proactively rotates the access/refresh token via `lib/server/refresh-session.ts` when within `REFRESH_LEAD_MS` (5 min) of expiry; a failed refresh is treated as non-fatal (see Auth Flow), not a dead session. |
| `proxy.ts`'s call chain uses Node's `crypto` module directly, with no explicit runtime pin | Low–Medium (verify) | `token-cipher.ts` (imported transitively via `parse-session.ts`/`refresh-session.ts`) uses `createCipheriv`/`createDecipheriv` from Node's built-in `crypto`, which the traditional Edge Runtime does not support. `proxy.ts` has no `export const runtime = "nodejs"` (or similar) declaration; the current behavior is consistent with Next.js 16's `proxy.ts` convention defaulting to the Node.js runtime (unlike the old edge-only `middleware.ts`), but this is inferred from the code, not confirmed via an explicit config — worth pinning explicitly if that assumption is ever wrong for a deployment target that still expects edge middleware. |
| SignalR now connects directly to the backend from the browser, bypassing the Next.js server entirely | Medium (unverified) | **New this sync**: `use-notifications.ts`'s `SIGNALR_HUB_URL` changed from a same-origin path proxied by `next.config.ts`'s `rewrites()` (now deleted) to `process.env.NEXT_PUBLIC_SIGNALR_HUB_URL!` (an absolute backend URL, the first `NEXT_PUBLIC_`-prefixed env var in this app). This assumes backend CORS is configured for the admin app's origin — not verified as part of this sync, and not checked anywhere in this client's own code. If CORS isn't configured backend-side, the WebSocket handshake fails and is now silently retried every 30s (see the new `connectSignalR` retry logic) rather than surfaced to the user. |
| Nav item references a route with no `page.tsx` (`/settings`) | Low | `/identity/users` and `/identity/roles` both now have real pages. `/settings` still 404s if followed. |
| Backend list endpoints never populate `Roles`/`Claims` on the DTO | Low (worked around, but worth remembering) | `UserService.SearchAsync`/`GetAllAsync` and `RoleService.GetAllAsync` all use a `DataMapper.cs` projection that only maps scalar fields; only the single-record fetch (`GetByIdAsync`) populates `Roles`/`Claims`. Both edit dialogs correctly re-fetch full detail on open to work around this (see Key Design Patterns), but any future feature reading a list endpoint for role/claim data would silently get empty arrays if it forgot to do the same. |
| Dashboard still renders hardcoded mock data (`features/dashboard/api/sample-data.ts`) | Low (by design) | Unchanged this sync — the dashboard wasn't part of this batch of work either. |
| `prettier` + `prettier-plugin-tailwindcss` installed but no config file/`format` script | Low | Unchanged — still `unknown` whether formatting is enforced anywhere. |
| No automated test suite | Low (by design at this stage) | Unchanged — no `*.test.*`/`*.spec.*` files, no test runner in `package.json`. Now more notable given real auth/session logic plus full Users and Roles CRUD flows all exist untested. |
| `components/ui/button.tsx` hand-modified beyond shadcn CLI output (`loading` prop, `cursor-pointer`) | Low | Unchanged — re-running the shadcn CLI would silently drop these customizations unless done carefully. |
| `DataTable`'s `onExport` prop has no caller yet | Low | `components/shared/data-table/data-table-buttons.tsx` (moved from `data-table-toolbar.tsx` this sync) already renders an Export button when `onExport` is passed, but no current feature (including `UsersDataTable`/`RolesDataTable`/`NotificationsDataTable`) passes one — dead capability until a consumer needs it. |
| `components/shared/object-viewer/` has no consumer yet | Low (by design) | New this sync, purely additive — ported from an external export spec and adapted to this project's design tokens/`components/ui/*` primitives, but not imported by any page or dialog. Dead code until something wires it in. |
| `components/command/*` (CommandPalette) has no consumer yet | Low (by design) | Unchanged — nothing in the app wires up `CommandPaletteProvider` yet. Dead code until something adopts it. (`AsyncSelect`/`MultiSelect`, the other two "no consumer yet" components from the previous sync, were deleted this sync rather than left dead — see Key Design Patterns.) |
| `components/ui/combobox.tsx` does not virtualize its option list | Low (by design) | The Floating-UI select library it replaced auto-virtualized past 50 options (`options-list.tsx`, now deleted); `cmdk`'s `Command` has no built-in virtualization, and the current two consumers (a small static enum, a client-filtered user list) don't need it. Deliberate scope decision made when migrating to `Combobox` — revisit if a future consumer needs a large option list. |
| `eslint.config.mjs`'s `react-hooks/refs` override glob still lists `src/components/select/**/*.tsx` | Trivial | `components/select/*` was deleted this sync; the glob entry is now a no-op (matches nothing) rather than a functional problem, but is stale and should be removed the next time `eslint.config.mjs` is touched. |
| The SignalR handshake token intentionally narrows the "JWT never leaves the httpOnly cookie" invariant | Low (deliberate) | `getSignalRTokenAction()` hands the browser a real, short-lived access token so `use-notifications.ts` can authenticate the WebSocket handshake — the one place in the app where the access token is readable by browser JS. A deliberate trade-off (SignalR can't attach a cookie/header the way `fetch` can), not an oversight, but worth keeping in mind if the token's blast radius ever needs to shrink further. |
| ~~`notifications-page.tsx` calls `getAllUsers(session.accessToken)` unpaginated to populate the recipient filter/picker~~ — **resolved this sync** | — | Replaced by an on-demand search pattern: `features/notifications/components/user-select.tsx` was rewritten to call `searchUsersAction` (page 1 of matches only) once the typed query passes a minimum length, debounced 300ms — see Key Design Patterns. `notifications-page.tsx` no longer imports `getAllUsers`/`@/features/users` at all. |

## Notes

<!-- manual: content below this line is human-authored and must be preserved verbatim during sync -->

---
_Generated: 2026-08-10 (resynced — SignalR direct-to-backend connection via `NEXT_PUBLIC_SIGNALR_HUB_URL`, `next.config.ts`'s `rewrites()` deleted; `use-notifications.ts` retry-on-failed-handshake logic; new shared `refreshSessionIfNearExpiry()` (`lib/server/refresh-session.ts`) and `persistSessionCookie()` (`lib/server/persist-session-cookie.ts`) helpers, adopted by `proxy.ts`, `get-signalr-token-action.ts`, and the previously-undocumented `features/auth/api/refresh-session-action.ts`; `AccentColorProvider`'s new `black` preset and `accent-color-picker.tsx`'s `swatchColor()` helper) — scope: client app "admin" — see .claude/CLAUDE.md for update rules._
