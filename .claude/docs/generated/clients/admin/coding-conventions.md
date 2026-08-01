# Coding Conventions: admin

## Build & Tooling

- **Next.js**: 16.2.12, App Router (`src/app/` — moved under `src/` this cycle, was `app/` at the package root; no `pages/` directory exists) — verified in `package.json` and directory listing.
- **Bundler for `next dev`**: `unknown` — `package.json`'s `"dev": "next dev"` has no explicit `--turbopack` flag and `next.config.ts` sets no bundler option. Unchanged/unverified from last sync.
- **Package manager**: pnpm — `pnpm-lock.yaml` present, no `package-lock.json`/`yarn.lock`. A `pnpm-workspace.yaml` now exists but only configures pnpm's build-script approval (`allowBuilds`/`ignoredBuiltDependencies` for `sharp`, `unrs-resolver`), not a multi-package workspace.
- **`tsconfig.json` strictness**: `"strict": true`, target `ES2017`, module resolution `"bundler"`, path alias `"@/*"` → `"./src/*"` — **changed** from `"./*"` to match the new `src/` root. `components.json`'s finer-grained aliases were updated to match: `utils` → `@/lib/shared/utils` (was `@/lib/utils`), `css` → `"src/app/globals.css"` (was `"app/globals.css"`); `components`/`ui`/`lib`/`hooks` aliases (`@/components`, `@/components/ui`, `@/lib`, `@/hooks`) are unchanged strings, now resolving under `src/` via the updated `tsconfig.json` mapping.
- **Styling toolchain**: Tailwind CSS v4 via `@tailwindcss/postcss`, CSS-first config (`src/app/globals.css`) — unchanged.

## Style

- **Linting**: ESLint 9 flat config (`eslint.config.mjs`), `eslint-config-next`'s `core-web-vitals` + `typescript` rule sets. Unchanged.
- **Formatting**: `prettier` + `prettier-plugin-tailwindcss` are devDependencies; still no `.prettierrc*` config file and no `format` script — unchanged, still `unknown` whether formatting is enforced anywhere.
- **Component file naming**: kebab-case filenames, one primary export per file matching a PascalCase component/function name — unchanged, now also applied consistently across `features/*/api/*.ts` (one function per file, named after the endpoint it wraps, e.g. `get-current-user.ts` exports `getCurrentUser`).
- **`"use client"` directive**: applied to files using hooks/state/browser APIs — `hooks/*`, `components/theme/*` (except none currently omit it), `components/layout/*` except `brand.tsx`, `components/shared/search-box.tsx`, `features/dashboard/components/users-table.tsx`, `features/auth/components/login-form.tsx`. Server Components without hooks (`components/layout/brand.tsx`, `features/dashboard/components/stat-card.tsx`, `features/auth/components/login-page.tsx`, `features/user-profile/components/user-profile-page.tsx`, all `app/**/page.tsx` and `app/**/layout.tsx`, all `features/*/api/*.ts` files) omit it and run server-side — the async data-fetching components (`user-profile-page.tsx`, `login-page.tsx`, `(dashboard)/layout.tsx`) are `async function`/Server Components, not client-fetched.
- **`"use server"` directive**: new this cycle — `features/auth/api/login-action.ts` is a Server Action module (`"use server"` at the top), the only one in the codebase so far.
- **shadcn-generated primitives** (`components/ui/*`, 24 files): unchanged `data-slot="<name>"` + `React.ComponentProps<...>` + `cva()` convention.

## Structural Conventions

- **Feature folder convention** (formalized this cycle — was a single unverified instance before): `features/<name>/` colocates `api/` (one file per backend endpoint, wrapping `lib/server/http.ts` + `lib/server/call-guard.ts`), `components/`, optionally `types/` (only when a type has exactly one consumer, e.g. `features/roles/types/role.ts`), and a mandatory `index.ts` barrel that re-exports the feature's public surface. **Cross-feature imports must go through the barrel** — verified in the one place it's exercised: `features/auth/api/login-action.ts` imports `getCurrentUser` from `@/features/user-profile` (the barrel), not `@/features/user-profile/api/get-current-user` directly.
- **`app/` is routing-only**: every `page.tsx`/`layout.tsx` under `src/app/` either re-exports a feature component as `default` or composes shared layout chrome (`AppShell`) plus a feature's session-resolution call — no business logic lives in `app/`.
- **API-per-endpoint-file convention**: each `features/*/api/*.ts` file wraps exactly one backend HTTP call via `requestJson`/`requestVoid` (`lib/server/http.ts`), returning a normalized result via one of `guardCall`/`guardResponseCall`/`guardRawCall` (`lib/server/call-guard.ts`) depending on whether the backend endpoint returns a `Result<T>` envelope, a bare `ApiResponse`, or a raw value/array.
- **`lib/server/` vs `lib/shared/` split** (new): `lib/server/*` is server-only (env access, fetch wrapper, cookie helpers) and must never be imported from a Client Component; `lib/shared/*` (`utils.ts`, `dedupe-claims.ts`, `user-display.ts`) is safe for both server and client and is used by 2+ features or layout chrome. `lib/server/session-cookie.ts` is deliberately kept free of `next/headers` (unlike `lib/server/session.ts`) so `proxy.ts`, which runs on the edge runtime, can import just the cookie-name constant.
- **Client component boundary**: state/effects/browser API usage pushed into small dedicated hooks/providers (`hooks/use-sidebar.tsx`, `hooks/use-scrolled.ts`, `components/theme/{accent-color-provider,use-has-mounted}`) — unchanged pattern, some files relocated.
- **Design-token-driven styling**: unchanged — semantic Tailwind utilities backed by CSS custom properties in `globals.css`'s `@theme inline` block.
- **Error handling**: still no error boundaries, no `error.tsx`/`not-found.tsx` under `app/`. `loginAction` surfaces failures via a typed `LoginFormState.error` string returned to `useActionState`, rather than throwing; `call-guard.ts` normalizes thrown errors (network failure, non-2xx status, non-JSON body) into the same `Result`/`ApiResponse` shape the backend itself returns, so UI code only ever handles one shape.

## Testing Conventions

- **Test framework(s)**: none installed — unchanged. No Jest/Vitest/Playwright/Testing Library, no `*.test.*`/`*.spec.*` files, no `test` script.
- **Naming convention / mocking library**: not applicable — no test suite exists yet, now more notable given real auth/session/API logic has landed untested.

## Deviations From Norms Elsewhere in the Repo

- Not fully evaluable — `clients/admin` is still the only client app (no sibling under `clients/` to compare against). No deviation from a documented cross-repo convention was identified; `.claude/CLAUDE.md`'s stated client-app defaults (Next.js App Router, TypeScript, React) all still hold.
- Worth noting for any future sibling app: the `src/`-rooted layout, the feature-folder + barrel convention, and the `lib/server`/`lib/shared` split introduced this cycle are conventions established here first — treat them as a candidate baseline, not yet a cross-repo norm.

## Notes

<!-- manual: content below this line is human-authored and must be preserved verbatim during sync -->

---
_Generated: 2026-08-01 — scope: client app "admin" — see .claude/CLAUDE.md for update rules._
