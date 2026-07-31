# Coding Conventions: admin

## Build & Tooling

- **Next.js**: 16.2.12, App Router (`app/` directory; no `pages/` directory exists) — verified in `package.json` and directory listing.
- **Bundler for `next dev`**: `unknown` — `package.json`'s `"dev": "next dev"` script has no explicit `--turbopack` flag and `next.config.ts` sets no bundler option, so whether Turbopack or webpack is active depends on this Next.js version's undocumented (from this repo's perspective) default. Not asserted without checking the installed Next.js source, which was out of scope for this pass.
- **Package manager**: pnpm — `pnpm-lock.yaml` present, no `package-lock.json`/`yarn.lock`.
- **`tsconfig.json` strictness**: `"strict": true`, target `ES2017`, module resolution `"bundler"`, path alias `"@/*"` → `./*` (single alias, not per-folder — `components.json` declares finer-grained aliases like `@/components`, `@/hooks`, `@/lib`, `@/ui` for the shadcn CLI, but `tsconfig.json` itself only defines the one root `@/*` mapping).
- **Styling toolchain**: Tailwind CSS v4 via `@tailwindcss/postcss` (in `postcss.config.mjs`) — CSS-first config, no `tailwind.config.ts`/`.js`. `tw-animate-css` supplies animation utilities; `shadcn` package supplies a base stylesheet imported directly (`@import "shadcn/tailwind.css"` in `app/globals.css`).

## Style

- **Linting**: ESLint 9 flat config (`eslint.config.mjs`) using `eslint-config-next`'s `core-web-vitals` and `typescript` rule sets via `defineConfig`/`globalIgnores`. No custom rule overrides beyond re-declaring the default ignore list (`.next/**`, `out/**`, `build/**`, `next-env.d.ts`).
- **Formatting**: `prettier` + `prettier-plugin-tailwindcss` are devDependencies, but no `.prettierrc*` config file exists and no `format` script is defined in `package.json` — `unknown` whether/how formatting is actually enforced (likely editor-integration-only with Prettier defaults + the Tailwind class-sorting plugin, unverified).
- **Component file naming**: kebab-case filenames (`sidebar-nav-item.tsx`, `accent-color-picker.tsx`, `use-accent-color.tsx`), one primary export per file, matching PascalCase component/function name (e.g. `sidebar-nav-item.tsx` exports `SidebarNavItem`).
- **`"use client"` directive**: applied at the top of files that use hooks/state/browser APIs (`hooks/*`, `providers/theme-provider.tsx`, `components/layout/*` except `brand.tsx`, `components/shared/search-box.tsx`, `features/dashboard/users-table.tsx`). Presentational/prop-driven files without hooks (e.g. `components/layout/brand.tsx`, `features/dashboard/stat-card.tsx`) omit it and can run as Server Components — verified by absence of the directive and absence of hooks in those files.
- **shadcn-generated primitives** (`components/ui/*`) consistently use a `data-slot="<component-name>"` attribute on the root element of each part, `React.ComponentProps<...>` for prop typing (rather than manually listing HTML attributes), and `cva()` for variant/size composition where more than one visual variant exists.

## Structural Conventions

- **Client component boundary**: state/effects/browser API usage (`localStorage`, `usePathname`, `useTheme`, scroll listeners) is pushed into small dedicated hooks (`hooks/use-sidebar.tsx`, `hooks/use-accent-color.tsx`, `hooks/use-scrolled.ts`, `hooks/use-has-mounted.ts`) or providers rather than scattered inline — components consume these via a single custom hook (`useSidebar()`, `useAccentColor()`, `useScrolled()`, `useHasMounted()`) that throws if called outside its provider (for the two Context-based ones).
- **Data-fetching pattern**: none yet — the sole page reads directly from a static, colocated `sample-data.ts` module. No pattern for real fetching (server components vs. client-side fetch vs. server actions) has been established in code yet.
- **Feature folder convention**: `features/<feature-name>/` colocates a feature's components and its data/types (`features/dashboard/{stat-card.tsx,users-table.tsx,sample-data.ts}`) — only one feature exists so far (`dashboard`), so this is a convention observed once, not cross-validated against a second feature.
- **Design-token-driven styling**: components reference semantic Tailwind utility classes backed by CSS custom properties (`bg-primary`, `text-sidebar-foreground`, `border-sidebar-border`, `duration-fast`, `ease-standard`, `z-topbar`) defined in `app/globals.css`'s `@theme inline` block, rather than hardcoded colors/durations/z-indices in component files — verified across `topbar.tsx`, `sidebar.tsx`, `button.tsx`.
- **Error handling**: no error boundaries, no `error.tsx`/`not-found.tsx` files under `app/` (verified via directory listing) — none needed yet given the single static page, but this is unverified/unbuilt territory for future routes.

## Testing Conventions

- **Test framework(s)**: none installed — no Jest/Vitest/Playwright/Testing Library in `package.json`, no `*.test.*`/`*.spec.*` files anywhere under `clients/admin` outside `node_modules`. No `test` script in `package.json`.
- **Naming convention / mocking library**: not applicable — no test suite exists yet.

## Deviations From Norms Elsewhere in the Repo

- Not evaluable in full — `clients/admin` is the only client app (no sibling under `clients/` to compare against), and this pass is explicitly out of scope for auditing `.claude/docs/generated/backend/` conventions. No deviation from a documented cross-repo convention was identified during this pass; `.claude/CLAUDE.md`'s stated client-app defaults (Next.js App Router, TypeScript, React) all match what's observed here.

## Notes

<!-- manual: content below this line is human-authored and must be preserved verbatim during sync -->

---
_Generated: 2026-07-31 — scope: client app "admin" — see .claude/CLAUDE.md for update rules._
