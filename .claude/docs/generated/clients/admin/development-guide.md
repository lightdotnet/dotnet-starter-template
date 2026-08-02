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
  - `TOKEN_ENCRYPTION_KEY` — a 32-byte base64 key; `lib/server/config.ts`'s `getTokenEncryptionKey()` throws if it's unset, but **the function itself is not called anywhere in the app yet** — the session cookie is still plaintext JSON (see [architecture.md](./architecture.md#known-architectural-risks--debt)). Setting this var is required only because the function would throw if invoked in the future, not because anything currently reads it.
- A `.env.local` file exists locally (gitignored) — not read for this doc since it may contain real values; use `.env.example` as the template.
- `pnpm lint` runs ESLint per `eslint.config.mjs`.

## Common Tasks

| Task | How |
|---|---|
| Add a backend migration | Not applicable to this client app — see `.claude/docs/generated/backend/development-guide.md` (out of scope for this doc). |
| Run the API locally | Not applicable to this client app — but note the dev server now depends on it being reachable at `API_BASE_URL` for login/profile to function. |
| Run this client app's dev server | `cd clients/admin && pnpm install && pnpm dev` (copy `.env.example` to `.env.local` first and fill in `API_BASE_URL`/`TOKEN_ENCRYPTION_KEY`) |
| Add a new backend endpoint call | Create one file under the relevant `features/<name>/api/`, wrapping `requestJson`/`requestVoid` (`lib/server/http.ts`) via `guardCall`/`guardResponseCall`/`guardRawCall` (`lib/server/call-guard.ts`); export it from the feature's `index.ts` barrel. |
| Add a new feature | Create `features/<name>/` with `api/`, optionally `components/`/`types/`, and an `index.ts` barrel; add a route under `src/app/` that re-exports the feature's page component as `default`. |
| Add a new list/table page | Reuse `components/shared/data-table` (`DataTable<TData>`) — define `DataTableColumn<TData>[]`, pass `data`/`isLoading`/`error`/pagination props/callbacks; see `features/users/components/{users-page,users-data-table}.tsx` for the reference pattern (Server Component fetch + gate on permission, Client Component wrapper drives URL-param search/pagination via `useTransition`). |
| Show a toast notification | Call `notifySuccess`/`notifyError` from `@/components/toast` — don't import `sonner` directly; `AppToaster` is already mounted once in `app/layout.tsx`. |
| Add a new shadcn UI primitive | `npx shadcn@latest add <component>` from `clients/admin/` (`components.json`: style `radix-nova`, base color `neutral`, icon library `lucide`, `css: "src/app/globals.css"`, `utils` alias `@/lib/shared/utils`) — `components/ui/button.tsx` has manual edits (`loading` prop, `cursor-pointer`) a regeneration could overwrite; diff before committing after any CLI regen. |
| Add a new nav item | Edit `src/constants/nav-items.ts` (`NAV_ITEMS`); nested items go in a node's `children` array. Note: adding a nav entry does not create its route — a matching `src/app/**/page.tsx` is still needed. |
| Lint the app | `pnpm lint` |
| Regenerate this client's typed API client (if applicable) | Not applicable — this app hand-writes one function per backend endpoint under `features/*/api/`, it doesn't generate a client from an OpenAPI spec. |

## Where to Look for X

- **App shell / global layout**: `src/app/layout.tsx` (fonts, `ThemeProvider`, `AccentColorProvider`, `TooltipProvider`).
- **Dashboard-group layout (session resolution + top bar/sidebar)**: `src/app/(dashboard)/layout.tsx` → `components/layout/app-shell.tsx`.
- **Routes**: `src/app/(dashboard)/page.tsx` (`/`), `src/app/(dashboard)/user-profile/page.tsx` (`/user-profile`), `src/app/login/page.tsx` (`/login`) — all one-line re-exports; real UI lives in the corresponding `features/*` folder.
- **Login flow**: `features/auth/` — `api/login.ts` (backend call), `api/login-action.ts` (server action: login + set session cookie + redirect), `components/{login-page,login-form}.tsx`.
- **Session cookie handling**: `lib/server/session-cookie.ts` (cookie name constant), `lib/server/session.ts` (read/parse), `features/user-profile/api/resolve-session.ts` (read + live profile fetch), `src/proxy.ts` (redirect-if-unauthenticated).
- **Profile page**: `features/user-profile/components/user-profile-page.tsx` (account details, QR code via `qrcode`, roles/claims).
- **Session management API (no UI yet)**: `features/user-profile/api/{list-sessions,revoke-session}.ts`.
- **User management UI (`/identity/users`)**: `features/users/components/users-page.tsx` (Server Component: session/permission gate via `lib/server/authorization.ts` + `constants/permissions.ts`, fetches via `search-users.ts`), `features/users/components/users-data-table.tsx` (client wrapper around the shared `DataTable`), `features/users/components/create-user-dialog.tsx` (create form, `useActionState` + `features/users/api/create-user-action.ts`).
- **Role management API (no UI yet)**: `features/roles/api/*`.
- **Reusable list/table building block**: `components/shared/data-table/` (`DataTable`, toolbar, pagination, `types.ts`) — generic, no feature knowledge; currently only used by `features/users`.
- **Toast notifications**: `components/toast/` (`AppToaster` mounted in `app/layout.tsx`, `notifySuccess`/`notifyError`, saturated color theme in `toast-theme.ts`).
- **Permission checks**: `lib/server/authorization.ts` (`hasPermission`/`hasAnyPermission`/`hasAllPermissions`, `SUPER_ADMIN_USERNAMES` bypass list), `constants/permissions.ts` (`IDENTITY_PERMISSIONS`, mirrors the backend's `IdentityPermissions` class).
- **Dashboard (still mock data)**: `features/dashboard/api/sample-data.ts`, `features/dashboard/components/{dashboard-page,stat-card,users-table}.tsx`.
- **Top bar**: `components/layout/topbar.tsx` (brand, breadcrumbs, search, accent picker, theme toggle, notifications, user menu).
- **Sidebar (nav + show/hide + mobile drawer)**: `components/layout/sidebar.tsx`, `components/layout/sidebar-nav-item.tsx`, state in `hooks/use-sidebar.tsx`.
- **Nav structure/labels**: `src/constants/nav-items.ts` (typed via `types/nav.ts`).
- **Theming (light/dark, accent color)**: `components/theme/` (`theme-provider.tsx`, `accent-color-provider.tsx`, `theme-toggle.tsx`, `accent-color-picker.tsx`, `use-has-mounted.ts`); design tokens in `src/app/globals.css`'s `@theme inline` block and `:root`/`.dark[data-accent=...]` variable declarations.
- **shadcn-generated UI primitives**: `components/ui/*` — configured via `components.json`.
- **Server-only API plumbing**: `lib/server/http.ts` (fetch wrapper, `api/v1/` prefix), `lib/server/call-guard.ts` (error-envelope normalization), `lib/server/config.ts` (env access).
- **Cross-cutting helpers**: `lib/shared/utils.ts` (`cn`), `lib/shared/dedupe-claims.ts`, `lib/shared/user-display.ts`.
- **Backend/client shared shapes**: `types/api.ts` (`Result`/`ApiResponse`/`Paged`/`PagedResult`, mirroring the backend envelope), `types/{session,token,user,nav}.ts`.

## Notes

<!-- manual: content below this line is human-authored and must be preserved verbatim during sync -->

---
_Generated: 2026-08-02 — scope: client app "admin" — see .claude/CLAUDE.md for update rules._
