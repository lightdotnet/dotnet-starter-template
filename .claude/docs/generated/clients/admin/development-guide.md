# Development Guide: admin

## Prerequisites

- **Node.js**: version not pinned anywhere — no `"engines"` field in `package.json`, no `.nvmrc`/`.node-version`. `@types/node` is `^20`, suggesting Node 20.x is the target, but this is inferred, not enforced — still `unknown`/unverified.
- **pnpm**: required — `pnpm-lock.yaml` is the only lockfile present. Exact pnpm version is `unknown` (no `packageManager` field in `package.json`). A `pnpm-workspace.yaml` now exists but only configures build-script approval (`allowBuilds`/`ignoredBuiltDependencies` for `sharp`, `unrs-resolver`) — no change to the install/run flow.
- **Backend must be reachable**: unlike the prior UI-shell-only state, this app now makes real calls to `src/Identity.Api` for login and profile data — `API_BASE_URL` must point at a running instance for auth/profile pages to work (dashboard still works standalone since it's mock data).

## Building

Verified from `package.json` scripts:

```bash
pnpm build   # runs `next build`
```

## Running Locally

```bash
pnpm dev     # runs `next dev`
```

No `--turbopack` flag is set explicitly, and `next.config.ts` has no bundler override — which bundler `next dev` uses by default for this Next.js version is `unknown` from repo config alone.

The dev server listens on Next.js's default port (3000) — no custom port configured.

```bash
pnpm start   # runs `next start` (serves a production build made via `pnpm build`)
```

## Running Tests

None — no test runner is installed (`package.json` has no `test` script, no Jest/Vitest/Playwright dependency), and no `*.test.*`/`*.spec.*` files exist under `clients/admin` outside `node_modules`.

## Local Setup

- `.gitignore` ignores `.env*` wholesale except `.env.example`, which is now committed and documents the two required server-only env vars:
  - `API_BASE_URL` — backend API base URL (e.g. `http://localhost:5000`); never expose via `NEXT_PUBLIC_`.
  - `TOKEN_ENCRYPTION_KEY` — a 32-byte base64 key (generate with `openssl rand -base64 32`, per `.env.example`'s own comment); `lib/server/config.ts`'s `getTokenEncryptionKey()` throws if it's unset, and **is now actually read on every request** — `lib/server/token-cipher.ts` uses it to AES-256-GCM encrypt/decrypt the `admin_session` cookie, exercised by `features/auth/api/login-action.ts` (encrypt, on login) and `src/proxy.ts` (decrypt on every request; re-encrypt after a token refresh or profile refetch). The app will fail at runtime without a valid key set; the previous "prep work, not wired up yet" state is resolved (see [architecture.md](./architecture.md#known-architectural-risks--debt)).
- A `.env.local` file exists locally (gitignored) — not read for this doc since it may contain real values; use `.env.example` as the template.
- `pnpm lint` runs ESLint per `eslint.config.mjs`.
- `next.config.ts` now also reads `process.env.API_BASE_URL` directly (not via `lib/server/config.ts`) to build the SignalR `rewrites()` target — a second, independent read site for the same existing env var, not a new var to add to `.env.example`.

## Common Tasks

| Task | How |
|---|---|
| Add a backend migration | Not applicable to this client app — see `.claude/docs/generated/backend/development-guide.md` (out of scope for this doc). |
| Run the API locally | Not applicable to this client app — but note the dev server now depends on it being reachable at `API_BASE_URL` for login/profile to function. |
| Run this client app's dev server | `cd clients/admin && pnpm install && pnpm dev` (copy `.env.example` to `.env.local` first and fill in `API_BASE_URL`/`TOKEN_ENCRYPTION_KEY`) |
| Add a new backend endpoint call | Create one file under the relevant `features/<name>/api/`, wrapping `requestJson`/`requestVoid` (`lib/server/http.ts`) via `guardCall`/`guardResponseCall`/`guardRawCall` (`lib/server/call-guard.ts`); export it from the feature's `index.ts` barrel. |
| Add a new feature | Create `features/<name>/` with `api/`, optionally `components/`/`types/`, and an `index.ts` barrel; add a route under `src/app/` that re-exports the feature's page component as `default`. |
| Add a new list/table page | Reuse `components/shared/data-table` (`DataTable<TData>`) — define `DataTableColumn<TData>[]`, pass `data`/`isLoading`/`error`/pagination props/callbacks. Three reference patterns exist: `features/users/components/{users-page,users-data-table}.tsx` (server-driven search via URL params + `useTransition`) for a single text-field search against a backend search endpoint; `features/roles/components/{roles-page,roles-data-table}.tsx` (client-side filtering over a full list) when there's no backend search endpoint; `features/notifications/components/notifications-data-table.tsx` for a custom multi-field filter UI (e.g. multiple selects) via `DataTable`'s `customSearch`/`onCustomSearch` props — the filters hold local "pending" state and only apply (via a "Search" button) on click, unlike the built-in single-field text search which stays auto-debounced. |
| Show a toast notification | Call `notifySuccess`/`notifyError` from `@/components/toast` — don't import `sonner` directly; `AppToaster` is already mounted once in `app/layout.tsx`. |
| Add a new shadcn UI primitive | `npx shadcn@latest add <component>` from `clients/admin/` (`components.json`: style `radix-nova`, base color `neutral`, icon library `lucide`, `css: "src/app/globals.css"`, `utils` alias `@/lib/shared/utils`) — `components/ui/button.tsx` has manual edits (`loading` prop, `cursor-pointer`) a regeneration could overwrite; diff before committing after any CLI regen. `popover.tsx`/`command.tsx` (new this sync) were instead hand-written to match this repo's conventions (`data-slot`, `cn`, the `radix-ui` unified-package import style) rather than run through the CLI — worth following that precedent for a primitive the CLI doesn't cleanly support out of the box. |
| Add a new nav item | If it belongs to an existing nav-bearing feature, add/update that feature's `constants/nav-item.ts` and re-export it from the feature's `index.ts` barrel; then reference it from `src/constants/nav-items.ts` (`NAV_ITEMS`) **by direct file path**, not via the barrel (see [architecture.md](./architecture.md#key-design-patterns) for why — the barrel also carries server-only code that must not reach the client-side `Sidebar`). For a brand-new nav-bearing feature, create `features/<name>/constants/nav-item.ts` following the same pattern. Group/no-owning-feature nodes (currently "Identity", "Settings") are declared directly in `nav-items.ts`. Note: adding a nav entry does not create its route — a matching `src/app/**/page.tsx` is still needed. |
| Lint the app | `pnpm lint` |
| Regenerate this client's typed API client (if applicable) | Not applicable — this app hand-writes one function per backend endpoint under `features/*/api/`, it doesn't generate a client from an OpenAPI spec. |

## Where to Look for X

- **App shell / global layout**: `src/app/layout.tsx` (fonts, `ThemeProvider`, `AccentColorProvider`, `TooltipProvider`).
- **Dashboard-group layout (session resolution + top bar/sidebar)**: `src/app/(dashboard)/layout.tsx` → `components/layout/app-shell.tsx`.
- **Routes**: `src/app/(dashboard)/page.tsx` (`/`), `src/app/(dashboard)/user-profile/page.tsx` (`/user-profile`), `src/app/login/page.tsx` (`/login`) — all one-line re-exports; real UI lives in the corresponding `features/*` folder.
- **Login flow**: `features/auth/` — `api/login.ts` (backend call), `api/login-action.ts` (server action: login, decode JWT permissions/roles, encrypt + set session cookie, redirect to `?redirect=<path>` or `/`), `components/{login-page,login-form}.tsx`.
- **Logout flow**: `features/auth/api/logout-action.ts` (deletes the session cookie, redirects to `/login?redirect=<path>` or `/login`; note it is **not** exported from the `features/auth` barrel — `components/layout/user-menu.tsx` imports it directly), `components/layout/user-menu.tsx` ("Log out" menu item, passes the current path so login can redirect back).
- **Session cookie handling (encryption + refresh)**: `lib/server/session-cookie.ts` (cookie name + `SESSION_TTL_MS`/`REFRESH_LEAD_MS`/`buildSessionCookieOptions`), `lib/server/token-cipher.ts` (AES-256-GCM `encrypt`/`decrypt`, keyed by `TOKEN_ENCRYPTION_KEY`), `lib/server/parse-session.ts` (decrypt + validate), `lib/server/session.ts` (`getSession()` — reads the cookie, no fetch), `lib/server/jwt.ts` (decode permissions/roles/claims from the JWT), `lib/server/build-session-claims.ts`, `lib/server/refresh-session.ts` (proactive token rotation), `features/user-profile/api/resolve-session.ts` (thin passthrough to `getSession()`), `src/proxy.ts` (the actual orchestration point — validates/redirects, proactively refreshes, refetches profile on hard navigations, re-encrypts and rewrites the cookie).
- **Profile page**: `features/user-profile/components/user-profile-page.tsx` (account details, QR code via `qrcode`, roles/claims).
- **Session management API (no UI yet)**: `features/user-profile/api/{list-sessions,revoke-session}.ts`.
- **User management UI (`/identity/users`)**: `features/users/components/users-page.tsx` (Server Component: session/permission gate via `lib/server/authorization.ts` + `features/users/constants/permissions.ts`, fetches via `search-users.ts`), `features/users/components/users-data-table.tsx` (client wrapper around the shared `DataTable`, owns the create/edit/delete dialogs' open state), `features/users/components/create-user-dialog.tsx` (create form), `edit-user-dialog.tsx` (edit + password reset, fetches full detail on open via `get-user-detail-action.ts`), `delete-user-dialog.tsx` (confirm + delete).
- **Role management UI (`/identity/roles`)**: `features/roles/components/roles-page.tsx` (Server Component: permission gate via `features/roles/constants/permissions.ts`, fetches all roles + the permission catalog), `roles-data-table.tsx` (client wrapper, client-side search filter — no backend search endpoint), `create-role-dialog.tsx` (name/description only — the backend accepts no claims on create), `edit-role-dialog.tsx` (name/description + a permissions checklist sourced from `get-permissions.ts`, fetches full role detail on open via `get-role-detail-action.ts`), `delete-role-dialog.tsx`.
- **Real-time notifications (topbar bell)**: `features/notifications/hooks/use-notifications.ts` (SignalR connection, refresh-on-push), `features/notifications/components/notification-bell.tsx` (bell icon, unread badge/ring animation, All/Unread/Archived tab filter).
- **Notification management UI (`/notifications`)**: `features/notifications/components/notifications-page.tsx` (Server Component: permission gate via `NOTIFICATIONS_PERMISSIONS`, fetches via `get-notifications.ts` + `getAllUsers` for the recipient picker), `notifications-data-table.tsx` (status + recipient filter dropdowns wired to URL params), `send-notification-dialog.tsx` (the "Send" action, gated separately on `NOTIFICATIONS_PERMISSIONS.Send`), `user-select.tsx` (searchable recipient picker shared by both).
- **Select / combobox input**: `components/ui/combobox.tsx` (`Combobox<TValue>`, a Popover+Command single-select) plus its `components/ui/{popover,command}.tsx` primitives — the shadcn-style replacement for the earlier internal Floating-UI select library (`components/select/*`, `components/foundation/{use-floating-popover,options-list,use-async-options}.ts(x)`, all deleted this sync). Consumed by `features/users/components/edit-user-dialog.tsx` (status/authProvider) and `features/notifications/components/user-select.tsx`. Nested inside a `Dialog`, its `Popover` portals into the Dialog's own DOM node via `components/foundation/portal-container.ts` rather than `document.body` — see `components/ui/dialog.tsx` if extending this pattern to another Radix overlay.
- **Reusable list/table building block**: `components/shared/data-table/` (`DataTable`, toolbar, `data-table-buttons.tsx` for the Export/Refresh/Columns cluster — new this sync, built on `components/ui/button-group.tsx` — pagination, `types.ts` — including optional per-column client-side sort via `sortable`/`sortValue`, and an optional `customSearch`/`onCustomSearch` slot for a caller-supplied, apply-on-click multi-field filter UI, used by `NotificationsDataTable`) — generic, no feature knowledge; used by `features/users`, `features/roles`, and `features/notifications`.
- **Reusable read-only object/table viewer (not yet wired into any page)**: `components/shared/object-viewer/` (`ObjectViewer`, plus `object-viewer-row.tsx`, `column-resize-handle.tsx`, `object-viewer-layout-context.tsx`, `utils.ts`) — purely additive, built on `components/ui/table`/`components/ui/input`, no `features/*` dependency.
- **Toast notifications**: `components/toast/` (`AppToaster` mounted in `app/layout.tsx`, `notifySuccess`/`notifyError`, saturated color theme in `toast-theme.ts`).
- **Permission checks**: `lib/shared/authorization.ts` (`hasPermission`/`hasAnyPermission`/`hasAllPermissions`, `isSuperAdminUser`, `SUPER_ADMIN_USERNAMES` bypass list — takes `permissions: string[]`/`userName` directly, safe for both server and client) is the actual logic; `lib/server/authorization.ts` is a thin server-only wrapper preserving a `(session, userName, permission)` signature for existing server callers (`UsersPage`, `RolesPage`); `components/layout/sidebar.tsx` calls `lib/shared/authorization.ts` directly. Permission-string constants live per-feature: `features/users/constants/permissions.ts` (`USERS_PERMISSIONS`), `features/roles/constants/permissions.ts` (`ROLES_PERMISSIONS`), `features/notifications/constants/permissions.ts` (`NOTIFICATIONS_PERMISSIONS` — `notification.read`/`notification.send`) — each mirrors its module's own backend permission-string constants.
- **Dashboard (still mock data)**: `features/dashboard/api/sample-data.ts`, `features/dashboard/components/{dashboard-page,stat-card,users-table}.tsx`.
- **Top bar**: `components/layout/topbar.tsx` (brand, breadcrumbs, search, accent picker, theme toggle, notifications, user menu).
- **Sidebar (nav + show/hide + mobile drawer)**: `components/layout/sidebar.tsx` (also computes the permission-filtered menu via `lib/shared/menu.ts`'s `buildVisibleMenu()`), `components/layout/sidebar-nav-item.tsx`, state in `hooks/use-sidebar.tsx`.
- **Nav structure/labels**: each nav-bearing feature owns its own entry — `features/{dashboard,users,roles,notifications}/constants/nav-item.ts` (`DASHBOARD_NAV_ITEM`/`USERS_NAV_ITEM`/`ROLES_NAV_ITEM`/`NOTIFICATIONS_NAV_ITEM`); `src/constants/nav-items.ts` (typed via `types/nav.ts`) assembles those into `NAV_ITEMS`, only declaring the "Identity" group and "Settings" leaf itself.
- **SignalR proxy (real-time push)**: `next.config.ts`'s `rewrites()` (first use of Next.js rewrites in this app) forwards `/api/signalr-hub/:path*` to the backend so the browser can open a same-origin WebSocket without exposing `API_BASE_URL` to client code; `features/notifications/api/get-signalr-token-action.ts` hands the browser the short-lived access token needed for the handshake.
- **Theming (light/dark, accent color)**: `components/theme/` (`theme-provider.tsx`, `accent-color-provider.tsx`, `theme-toggle.tsx`, `accent-color-picker.tsx`, `use-has-mounted.ts`); design tokens in `src/app/globals.css`'s `@theme inline` block and `:root`/`.dark[data-accent=...]` variable declarations.
- **shadcn-generated UI primitives**: `components/ui/*` — configured via `components.json`.
- **Server-only API plumbing**: `lib/server/http.ts` (fetch wrapper, `api/v1/` prefix), `lib/server/call-guard.ts` (error-envelope normalization), `lib/server/config.ts` (env access).
- **Cross-cutting helpers**: `lib/shared/utils.ts` (`cn`), `lib/shared/dedupe-claims.ts`, `lib/shared/user-display.ts`, `lib/shared/menu.ts` (`buildVisibleMenu`).
- **Backend/client shared shapes**: `types/api.ts` (`Result`/`ApiResponse`/`Paged`/`PagedResult`, mirroring the backend envelope), `types/{session,token,user,nav}.ts`.

## Notes

<!-- manual: content below this line is human-authored and must be preserved verbatim during sync -->

---
_Generated: 2026-08-04 (resynced — added a third "Add a new list/table page" reference pattern for `DataTable`'s `customSearch`/`onCustomSearch` slot; updated the "Reusable list/table building block" bullet for the new `data-table-buttons.tsx`/`components/ui/button-group.tsx`) — scope: client app "admin" — see .claude/CLAUDE.md for update rules._
