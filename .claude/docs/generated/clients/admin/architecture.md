# Architecture: admin

## Layering

Observed folder organization (verified via directory listing) — everything now lives under `clients/admin/src/` (previously at `clients/admin/` root):

```text
src/
  app/                      routing only — layout.tsx, globals.css, login/page.tsx, (dashboard)/{layout.tsx,page.tsx,user-profile/page.tsx}
  features/
    auth/                   api/{login,refresh-token,login-action}.ts, components/{login-page,login-form}.tsx, index.ts
    user-profile/           api/{get-current-user,list-sessions,revoke-session,resolve-session}.ts, components/user-profile-page.tsx, types/user-session.ts, index.ts
    dashboard/              api/sample-data.ts (mock, not a backend call), components/{stat-card,users-table,dashboard-page}.tsx, index.ts
    users/                  api/*.ts (8 endpoint files, API-only — no components/, no UI consumer), index.ts
    roles/                  api/*.ts (5 endpoint files, API-only — no UI consumer), types/role.ts, index.ts
  components/
    ui/                     shadcn-CLI-generated primitives (24 files)
    layout/                 topbar, sidebar, sidebar-nav-item, brand, breadcrumbs, user-menu, app-shell
    theme/                  theme-provider, accent-color-provider, theme-toggle, accent-color-picker, use-has-mounted, index.ts (barrel)
    shared/                 search-box.tsx
  hooks/                    use-sidebar.tsx, use-scrolled.ts
  lib/
    server/                 config.ts, http.ts, call-guard.ts, session-cookie.ts, session.ts
    shared/                 utils.ts, dedupe-claims.ts, user-display.ts
  constants/                nav-items.ts
  types/                    api.ts, nav.ts, session.ts, token.ts, user.ts
  proxy.ts                  Next.js 16 convention file (middleware-equivalent)
```

This is now a **feature-folder** layering, replacing the earlier route → layout-chrome → feature-component → primitive-only structure: `app/*` pages are pure re-exports from a feature's public API; `features/<name>/` owns its own `api/`, `components/`, and (when a type has exactly one consumer) `types/`, each exposed through an `index.ts` barrel; `components/layout/*` (app chrome) and `components/theme/*` compose `components/ui/*` plus `hooks/*`/`lib/shared/*`/`constants/*`/`types/*`; `components/ui/*` remains the leaf primitive layer.

## Dependency Direction

Verified via actual `import` statements:

```text
src/proxy.ts                       -> lib/server/session-cookie (constant only, no next/headers — edge-safe)
src/app/layout.tsx                 -> components/theme (ThemeProvider, AccentColorProvider), components/ui/tooltip
src/app/login/page.tsx             -> features/auth (LoginPage)
src/app/(dashboard)/layout.tsx     -> components/layout/app-shell, features/user-profile (resolveSession)
src/app/(dashboard)/page.tsx       -> features/dashboard (DashboardPage)
src/app/(dashboard)/user-profile/page.tsx -> features/user-profile (UserProfilePage)

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

features/users/*, features/roles/*  -> lib/server/http, lib/server/call-guard, types/{api,user} / features/roles/types/role
                                        (no importer outside their own feature — no UI consumer yet)

lib/server/session.ts               -> next/headers (cookies), lib/server/session-cookie, types/session
lib/server/http.ts                  -> lib/server/config (getApiBaseUrl)
components/ui/*                     -> lib/shared/utils, radix-ui, class-variance-authority, lucide-react
                                        (button.tsx additionally -> components/ui/spinner)
```

Direction is still one-way: `components/ui/*` never imports from `components/layout/*`, `components/theme/*`, or `features/*`. Cross-feature imports go through a feature's `index.ts` barrel, not its internals — e.g. `features/auth/api/login-action.ts` imports `getCurrentUser` from `@/features/user-profile` (the barrel), not from `@/features/user-profile/api/get-current-user` directly. `features/dashboard`, `features/users`, and `features/roles` are not imported by any other feature.

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

## Shared Kernel / Common Building Blocks Used

- `components/ui/*` — the app's own primitive layer.
- `lib/shared/utils.ts` (`cn`), `lib/shared/dedupe-claims.ts`, `lib/shared/user-display.ts` — cross-cutting helpers consumed by 2+ features or by layout chrome (moved out of a flat `lib/utils.ts` into `lib/shared/` alongside these new helpers).
- `lib/server/*` — the server-only building blocks (`http.ts`, `call-guard.ts`, `config.ts`, `session.ts`, `session-cookie.ts`) every feature's `api/` layer is built on.
- `hooks/*`, `components/theme/*` — cross-cutting building blocks consumed by layout components.
- No package or code is shared with another client app — `clients/` still contains only `admin`.

## Module/Route Boundaries

Two route groups/areas now exist:
- `(dashboard)` — wraps `/` and `/user-profile` with `resolveSession()` + `AppShell` (top bar, sidebar).
- `login` (ungrouped) — `/login`, rendered under the root layout only, no `AppShell`/session resolution.

`constants/nav-items.ts` still declares `/identity`, `/identity/users`, `/identity/roles`, `/settings` with no corresponding routes — unchanged planned/future boundaries, now backed by API-only `features/users`/`features/roles` scaffolding but still no pages.

Feature isolation is enforced by convention (barrel-only cross-feature imports), verified in the one place it's exercised so far: `features/auth/api/login-action.ts` imports `getCurrentUser` via `@/features/user-profile`'s barrel, not its internals.

## Known Architectural Risks / Debt

| Finding | Severity | Notes |
|---|---|---|
| Session cookie stores the access/refresh token and claims as **plaintext JSON** | Medium | `login-action.ts` sets the cookie `httpOnly` but unencrypted, with its own comment: "Plaintext for now — encryption is a separate follow-up step." A `TOKEN_ENCRYPTION_KEY` env var and `getTokenEncryptionKey()` function already exist (`lib/server/config.ts`, `.env.example`) but are **not called anywhere** — prep work for a follow-up that hasn't landed. Worth tracking as real risk now that auth is live, not hypothetical. |
| No token refresh flow wired up | Low–Medium | `features/auth/api/refresh-token.ts` (`refreshToken()`) exists and is exported from the feature barrel but has no caller. The session cookie's `maxAge` is tied to the access token's `expiresIn`, so once it expires the cookie is simply gone and `proxy.ts` redirects to `/login` — there's no silent renewal. |
| Nav items reference routes with no `page.tsx` (`/identity`, `/identity/users`, `/identity/roles`, `/settings`) | Low | Unchanged from before. `features/users`/`features/roles` now provide the API surface these pages would need, but no UI has been built yet — clicking these links still 404s. |
| `features/users` and `features/roles` are fully API-only with zero UI consumers | Low (by design at this stage) | 13 endpoint files total across the two features, none imported outside their own feature. Expected mid-build state, but worth revisiting once user/role management UI is scoped. |
| Dashboard still renders hardcoded mock data (`features/dashboard/api/sample-data.ts`) | Low (by design) | Despite the rest of the app gaining real backend integration this cycle, the dashboard itself wasn't part of that work — flagged so it isn't mistaken for "already done." |
| `prettier` + `prettier-plugin-tailwindcss` installed but no config file/`format` script | Low | Unchanged — still `unknown` whether formatting is enforced anywhere. |
| No automated test suite | Low (by design at this stage) | Unchanged — no `*.test.*`/`*.spec.*` files, no test runner in `package.json`. Now more notable given real auth/session logic exists to test. |
| `components/ui/button.tsx` hand-modified beyond shadcn CLI output (`loading` prop, `cursor-pointer`) | Low | Unchanged — re-running the shadcn CLI would silently drop these customizations unless done carefully. |

## Notes

<!-- manual: content below this line is human-authored and must be preserved verbatim during sync -->

---
_Generated: 2026-08-01 — scope: client app "admin" — see .claude/CLAUDE.md for update rules._
