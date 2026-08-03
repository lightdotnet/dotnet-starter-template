# Dependency Graph: admin

## Internal Module Imports

Verified via actual `import` statements (see [architecture.md](./architecture.md#dependency-direction) for the full trace). All paths are relative to `clients/admin/src/`.

| From | To | Notes |
|---|---|---|
| `proxy.ts` | `lib/server/session-cookie.ts` | Constant-only import — keeps the edge-runtime `proxy.ts` free of `next/headers` |
| `app/layout.tsx` | `components/theme` (barrel), `components/toast` (barrel — `AppToaster`), `components/ui/tooltip.tsx` | Root layout |
| `app/login/page.tsx` | `features/auth` (barrel) | Re-export only |
| `app/(dashboard)/layout.tsx` | `components/layout/app-shell.tsx`, `features/user-profile` (barrel) | Calls `resolveSession()` before rendering `AppShell` |
| `app/(dashboard)/page.tsx` | `features/dashboard` (barrel) | Re-export only |
| `app/(dashboard)/user-profile/page.tsx` | `features/user-profile` (barrel) | Re-export only |
| `app/(dashboard)/identity/users/page.tsx` | `features/users` (barrel) | Re-export only |
| `app/(dashboard)/identity/roles/page.tsx` | `features/roles` (barrel) | Re-export only — new route this sync |
| `components/layout/app-shell.tsx` | `hooks/use-sidebar.tsx`, `components/layout/{sidebar,topbar}.tsx` | Extracted out of the former `app/(dashboard)/layout.tsx` |
| `components/layout/topbar.tsx` | `lib/shared/utils.ts`, `hooks/{use-scrolled,use-sidebar}.ts(x)`, `components/ui/{button,badge}.tsx`, `components/layout/{breadcrumbs,brand,user-menu}.tsx`, `components/shared/search-box.tsx`, `components/theme` (barrel: `ThemeToggle`, `AccentColorPicker`) | |
| `components/layout/sidebar.tsx` | `lib/shared/utils.ts`, `hooks/use-sidebar.tsx`, `components/layout/sidebar-nav-item.tsx`, `constants/nav-items.ts`, `components/ui/sheet.tsx` | |
| `components/layout/sidebar-nav-item.tsx` | `lib/shared/utils.ts`, `hooks/use-sidebar.tsx`, `types/nav.ts` | Recursive |
| `components/layout/breadcrumbs.tsx` | `components/ui/breadcrumb.tsx`, `constants/nav-items.ts`, `types/nav.ts` | |
| `components/layout/user-menu.tsx` | `components/ui/{avatar,button,dropdown-menu}.tsx`, `lib/shared/user-display.ts`, `types/user.ts` | Now takes a `user: UserDto \| null` prop — no more hardcoded `MOCK_USER` |
| `components/layout/brand.tsx` | `next/link` only | No hooks — server-renderable |
| `components/theme/theme-toggle.tsx` | `next-themes`, `./use-has-mounted.ts`, `components/ui/{button,dropdown-menu}.tsx` | |
| `components/theme/accent-color-picker.tsx` | `components/ui/{button,dropdown-menu}.tsx`, `./accent-color-provider.tsx` | |
| `features/auth/index.ts` | `./components/login-page.tsx`, `./api/login.ts`, `./api/refresh-token.ts` | Barrel |
| `features/auth/components/login-page.tsx` | `components/ui/card.tsx`, `./login-form.tsx` | |
| `features/auth/components/login-form.tsx` | `components/ui/{button,input,label,alert}.tsx`, `features/auth/api/login-action.ts` | |
| `features/auth/api/login-action.ts` | `features/auth/api/login.ts`, `features/user-profile` (barrel — `getCurrentUser`), `lib/shared/dedupe-claims.ts`, `lib/server/session-cookie.ts`, `types/session.ts` | `"use server"` |
| `features/auth/api/{login,refresh-token}.ts` | `lib/server/http.ts`, `lib/server/call-guard.ts`, `types/{api,token}.ts` | |
| `features/user-profile/index.ts` | `./components/user-profile-page.tsx`, `./api/{resolve-session,get-current-user,list-sessions,revoke-session}.ts`, `./types/user-session.ts` | Barrel |
| `features/user-profile/components/user-profile-page.tsx` | `components/ui/{card,badge,separator,avatar,alert}.tsx`, `qrcode`, `features/user-profile/api/resolve-session.ts`, `lib/shared/{dedupe-claims,user-display}.ts` | |
| `features/user-profile/api/resolve-session.ts` | `lib/server/session.ts` (`getSession`), `features/user-profile/api/get-current-user.ts`, `types/{api,session,user}.ts` | |
| `features/user-profile/api/get-current-user.ts` | `lib/server/http.ts`, `lib/server/call-guard.ts`, `types/{api,user}.ts` | |
| `features/user-profile/api/list-sessions.ts` | `lib/server/http.ts`, `lib/server/call-guard.ts`, `features/user-profile/types/user-session.ts` | |
| `features/user-profile/api/revoke-session.ts` | `lib/server/http.ts`, `lib/server/call-guard.ts` | |
| `features/dashboard/index.ts` | `./components/dashboard-page.tsx` | Barrel |
| `features/dashboard/components/dashboard-page.tsx` | `components/ui/card.tsx`, `./stat-card.tsx`, `./users-table.tsx`, `features/dashboard/api/sample-data.ts` | |
| `features/dashboard/components/{stat-card,users-table}.tsx` | `components/ui/*`, `lib/shared/utils.ts`, `features/dashboard/api/sample-data.ts` | Mock data — no backend call |
| `features/users/index.ts` | `./components/users-page.tsx`, `./api/{search-users,get-all-users,get-user-by-id,get-user-by-username,create-user,update-user,delete-user,force-password}.ts`, `./constants/permissions.ts` | Barrel; imported by `app/(dashboard)/identity/users/page.tsx` |
| `features/users/components/users-page.tsx` | `features/user-profile` (barrel — `resolveSession`), `features/users/api/search-users.ts`, `features/roles` (barrel — `getAllRoles`, fetched only when the viewer can update users), `features/users/components/users-data-table.tsx`, `lib/server/authorization.ts` (`hasPermission`), `features/users/constants/permissions.ts`, `components/ui/empty.tsx`, `next/navigation` (`redirect`) | Async Server Component; gates on `USERS_PERMISSIONS.View`/`.Create`/`.Update`/`.Delete` |
| `features/users/components/users-data-table.tsx` | `components/shared/data-table` (barrel — `DataTable` + types), `components/ui/{avatar,badge,button,dropdown-menu}.tsx`, `features/users/components/{create,edit,delete}-user-dialog.tsx`, `features/user-profile/components/user-status-badge.tsx`, `lib/shared/user-display.ts`, `features/roles/types/role.ts` (`RoleDto`), `types/user.ts`, `next/navigation` (`useRouter`/`usePathname`/`useSearchParams`) | Thin `DataTable` wrapper; drives URL-param search/pagination; owns row-actions dropdown + all 3 dialogs' open/key state |
| `features/users/components/create-user-dialog.tsx` | `components/ui/{alert,button,dialog,input,label}.tsx`, `components/toast` (barrel — `notifySuccess`), `features/users/api/create-user-action.ts` | |
| `features/users/components/edit-user-dialog.tsx` | `components/ui/{alert,button,checkbox,dialog,input,label,select,spinner,tabs}.tsx`, `components/toast` (barrel — `notifySuccess`), `features/users/api/{get-user-detail-action,update-user-action,force-password-action}.ts`, `features/roles/types/role.ts` (`RoleDto`), `types/user.ts` | New — fetches full user detail on open (see [architecture.md](./architecture.md#key-design-patterns)) |
| `features/users/components/delete-user-dialog.tsx` | `components/ui/{button,dialog}.tsx`, `components/toast` (barrel), `features/users/api/delete-user-action.ts`, `types/user.ts` | New |
| `features/users/api/create-user-action.ts` | `features/user-profile` (barrel — `resolveSession`), `features/users/api/create-user.ts`, `types/user.ts` | `"use server"` |
| `features/users/api/{update-user-action,force-password-action,delete-user-action}.ts` | `features/user-profile` (barrel — `resolveSession`), `features/users/api/{update-user,force-password,delete-user}.ts` respectively, `types/user.ts` | New — `"use server"` |
| `features/users/api/get-user-detail-action.ts` | `features/user-profile` (barrel — `resolveSession`), `features/users/api/get-user-by-id.ts`, `types/user.ts` | New — `"use server"`; on-demand detail read, not a mutation |
| `features/users/api/*.ts` (7 remaining files) | `lib/server/http.ts`, `lib/server/call-guard.ts`, `types/{api,user}.ts` | |
| `features/roles/index.ts` | `./components/roles-page.tsx`, `./api/{get-all-roles,get-role-by-id,get-permissions,create-role,update-role,delete-role}.ts`, `./constants/permissions.ts`, `./types/{role,permission-definition}.ts` | Barrel; now imported by `app/(dashboard)/identity/roles/page.tsx` and `features/users/components/users-page.tsx` — first cross-feature/UI consumers |
| `features/roles/components/roles-page.tsx` | `features/user-profile` (barrel — `resolveSession`), `features/roles/api/{get-all-roles,get-permissions}.ts`, `features/roles/components/roles-data-table.tsx`, `lib/server/authorization.ts` (`hasPermission`), `features/roles/constants/permissions.ts`, `components/ui/empty.tsx` | New — async Server Component; gates on `ROLES_PERMISSIONS.View`/`.Manage`; no `searchParams` (no backend search endpoint) |
| `features/roles/components/roles-data-table.tsx` | `components/shared/data-table` (barrel), `components/ui/{button,dropdown-menu}.tsx`, `features/roles/components/{create,edit,delete}-role-dialog.tsx`, `features/roles/types/{role,permission-definition}.ts` | New — search via local `useState` + `.filter()`, not URL params |
| `features/roles/components/create-role-dialog.tsx` | `components/ui/{alert,button,dialog,input,label}.tsx`, `components/toast` (barrel), `features/roles/api/create-role-action.ts` | New — name/description only, no claims step |
| `features/roles/components/edit-role-dialog.tsx` | `components/ui/{alert,button,checkbox,dialog,input,label,spinner}.tsx`, `components/toast` (barrel), `features/roles/api/{get-role-detail-action,update-role-action}.ts`, `features/roles/types/{role,permission-definition}.ts` | New — fetches full role detail on open |
| `features/roles/components/delete-role-dialog.tsx` | `components/ui/{button,dialog}.tsx`, `components/toast` (barrel), `features/roles/api/delete-role-action.ts`, `features/roles/types/role.ts` | New |
| `features/roles/api/{create-role-action,update-role-action,delete-role-action}.ts` | `features/user-profile` (barrel — `resolveSession`), `features/roles/api/{create-role,update-role,delete-role}.ts` respectively | New — `"use server"`; `update-role-action` additionally reads `get-role-by-id.ts` first to preserve non-`"permission"`-typed claims |
| `features/roles/api/get-role-detail-action.ts` | `features/user-profile` (barrel — `resolveSession`), `features/roles/api/get-role-by-id.ts` | New — `"use server"`; on-demand detail read |
| `features/roles/api/get-permissions.ts` | `lib/server/http.ts`, `lib/server/call-guard.ts`, `features/roles/types/permission-definition.ts` | New |
| `features/roles/api/get-all-roles.ts` | `lib/server/http.ts`, `lib/server/call-guard.ts`, `features/roles/types/role.ts` | **Fixed this sync**: was `guardRawCall` assuming a bare array; the endpoint wraps its response in the app's envelope like every other endpoint |
| `features/roles/api/{get-role-by-id,create-role,update-role,delete-role}.ts` | `lib/server/http.ts`, `lib/server/call-guard.ts`, `features/roles/types/role.ts` | |
| `components/shared/data-table/data-table.tsx` | `components/ui/{table,skeleton,empty,alert}.tsx`, `./data-table-toolbar.tsx`, `./data-table-pagination.tsx`, `./types.ts` | New; exported via `./index.ts` barrel |
| `components/shared/data-table/data-table-toolbar.tsx` | `components/ui/{button,dropdown-menu}.tsx`, `lib/shared/utils.ts`, `./types.ts` | New — debounced (400ms) search input |
| `components/shared/data-table/data-table-pagination.tsx` | `components/ui/{input,pagination}.tsx` | New — exports `getPageWindow()` helper |
| `components/toast/toaster.tsx` | `next-themes`, `sonner`, `./toast-theme.ts` | New |
| `components/toast/notify.ts` | `sonner` | New |
| `components/toast/toast-theme.ts` | `sonner` (types only) | New |
| `lib/server/session.ts` | `next/headers` (`cookies`), `lib/server/session-cookie.ts`, `types/session.ts` | |
| `lib/server/http.ts` | `lib/server/config.ts` (`getApiBaseUrl`) | `send()` gained `extractErrorMessage()` — reads non-2xx response bodies for a real error message |
| `lib/server/authorization.ts` | `types/session.ts` (`SessionData`) | Pre-existing file, gained its first real consumer this sync (`features/users/components/users-page.tsx`) |
| `components/ui/*` | `lib/shared/utils.ts`, `radix-ui`, `class-variance-authority`, `lucide-react` | `button.tsx` additionally imports `components/ui/spinner.tsx` |
| `hooks/use-sidebar.tsx` | `next/navigation` (`usePathname`) | |

## Package References

From `clients/admin/package.json` (`dependencies`):

| Package | Version | Notes |
|---|---|---|
| `next` | `16.2.12` | Framework |
| `react` | `19.2.4` | |
| `react-dom` | `19.2.4` | |
| `radix-ui` | `^1.6.7` | Unified Radix primitives; base for `components/ui/*` |
| `class-variance-authority` | `^0.7.1` | Variant class composition (`cva`) |
| `clsx` | `^2.1.1` | Used inside `lib/shared/utils.ts`'s `cn()` |
| `tailwind-merge` | `^3.6.0` | Used inside `lib/shared/utils.ts`'s `cn()` |
| `lucide-react` | `^1.28.0` | Icon set |
| `next-themes` | `^0.4.6` | Theme (light/dark/system) switching; also drives `components/toast/toaster.tsx`'s light/dark toast theme |
| `qrcode` | `^1.5.4` | QR code generation for `/user-profile`'s user-ID code |
| `sonner` | `^2.0.7` | **New** — toast notifications, wrapped by `components/toast/` (not imported directly by feature code) |
| `shadcn` | `^4.16.0` | CLI that generated `components/ui/*`; also imported at runtime (`shadcn/tailwind.css`) |
| `tw-animate-css` | `^1.4.0` | Animation utility classes |

`devDependencies`:

| Package | Version | Notes |
|---|---|---|
| `@tailwindcss/postcss` | `^4` | Tailwind v4 PostCSS plugin |
| `tailwindcss` | `^4` | |
| `@types/node` | `^20` | |
| `@types/qrcode` | `^1.5.6` | Types for the `qrcode` runtime dependency |
| `@types/react` | `^19` | |
| `@types/react-dom` | `^19` | |
| `eslint` | `^9` | |
| `eslint-config-next` | `16.2.12` | Pinned to match `next`'s exact version |
| `prettier` | `^3.9.6` | No config file/`format` script found — see [coding-conventions.md](./coding-conventions.md) |
| `prettier-plugin-tailwindcss` | `^0.8.1` | |
| `typescript` | `^5` | |

Package manager: pnpm (`pnpm-lock.yaml`). A `pnpm-workspace.yaml` exists but only configures build-script approval (`sharp`, `unrs-resolver`) — not a multi-package workspace.

## Circular References

None found among internal module imports — `components/ui/*` remains a strict leaf layer, and cross-feature imports observed so far (`features/auth` → `features/user-profile`, `features/users` → `features/user-profile`, `features/users` → `features/roles` — new this sync) go one direction only, through the target feature's barrel. `components/shared/data-table/*` and `components/toast/*` are consumed by both `features/users` and `features/roles` but import nothing from `features/*` themselves, so no cycle there either.

## Version Mismatches

Not applicable — `clients/admin` is still the only client app in the repo.

## Cross-Module Boundary Violations (backend only)

Not applicable — this is a client-app dependency graph, not backend.

## Notes

<!-- manual: content below this line is human-authored and must be preserved verbatim during sync -->

---
_Generated: 2026-08-03 — scope: client app "admin" — see .claude/CLAUDE.md for update rules._
