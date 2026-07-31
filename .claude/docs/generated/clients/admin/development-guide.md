# Development Guide: admin

## Prerequisites

- **Node.js**: version not pinned anywhere in `clients/admin` — no `"engines"` field in `package.json`, no `.nvmrc`/`.node-version` file found. `@types/node` is `^20`, which suggests Node 20.x is the target, but this is inferred from a type-package version, not verified via an explicit engine constraint — treat as `unknown`/unverified.
- **pnpm**: required — `pnpm-lock.yaml` is the only lockfile present; no `package-lock.json`/`yarn.lock`. Exact pnpm version required is `unknown` (no `packageManager` field in `package.json`).

## Building

Verified from `package.json` scripts:

```bash
pnpm build   # runs `next build`
```

## Running Locally

```bash
pnpm dev     # runs `next dev`
```

No `--turbopack` flag is set explicitly, and `next.config.ts` has no bundler override — which bundler `next dev` uses by default for this Next.js version is `unknown` from the repo config alone.

The dev server listens on Next.js's default port (3000) — no custom port configured in `package.json` or `next.config.ts`. There is no environment variable referencing a backend API base URL anywhere in the app (verified via grep for `process.env`/`NEXT_PUBLIC`/`fetch(` — no matches), consistent with this being a UI-shell-only build with no backend integration yet.

```bash
pnpm start   # runs `next start` (serves a production build made via `pnpm build`)
```

## Running Tests

None — no test runner is installed (`package.json` has no `test` script, no Jest/Vitest/Playwright dependency), and no `*.test.*`/`*.spec.*` files exist under `clients/admin` outside `node_modules`.

## Local Setup

- `.gitignore` ignores `.env*` files wholesale ("can opt-in for committing if needed"), but no `.env.example`/`.env.local` file exists in the repo — there is currently nothing to configure locally; the app runs with defaults out of the box (`pnpm install && pnpm dev`).
- `pnpm lint` runs ESLint (`next lint`-equivalent via the `eslint` CLI directly, per `eslint.config.mjs`).

## Common Tasks

| Task | How |
|---|---|
| Add a backend migration | Not applicable to this client app — see `.claude/docs/generated/backend/development-guide.md` (out of scope for this doc). |
| Run the API locally | Not applicable to this client app. |
| Run this client app's dev server | `cd clients/admin && pnpm install && pnpm dev` |
| Add a new shadcn UI primitive | `npx shadcn@latest add <component>` from `clients/admin/` (per `components.json`: style `radix-nova`, base color `neutral`, icon library `lucide`) — be aware `components/ui/button.tsx` has manual edits (`loading` prop, `cursor-pointer`) that a regeneration could overwrite; verify diffs before committing after any CLI regen. |
| Add a new nav item | Edit `constants/nav-items.ts` (`NAV_ITEMS`); add an icon from `lucide-react`; nested items go in a node's `children` array. |
| Lint the app | `pnpm lint` |
| Regenerate this client's typed API client (if applicable) | Not applicable yet — no API client layer exists (see [overview.md](./overview.md#backend-integration)). |

## Where to Look for X

- **App shell / global layout**: `app/layout.tsx` (fonts, `ThemeProvider`, `AccentColorProvider`, `TooltipProvider`).
- **Dashboard-area layout (top bar + sidebar)**: `app/(dashboard)/layout.tsx`.
- **The only page**: `app/(dashboard)/page.tsx` (route `/`).
- **Top bar**: `components/layout/topbar.tsx` (brand, breadcrumbs, search, accent picker, theme toggle, notifications, user menu).
- **Sidebar (nav + show/hide + mobile drawer)**: `components/layout/sidebar.tsx`, `components/layout/sidebar-nav-item.tsx`, state in `hooks/use-sidebar.tsx`.
- **Nav structure/labels**: `constants/nav-items.ts` (typed via `types/nav.ts`).
- **Theming (light/dark)**: `providers/theme-provider.tsx` (`next-themes`), toggle UI in `components/layout/theme-toggle.tsx`.
- **Accent color system**: CSS tokens in `app/globals.css` (`--primary` and `:root[data-accent="..."]`/`.dark[data-accent="..."]` blocks), runtime state in `hooks/use-accent-color.tsx`, picker UI in `components/layout/accent-color-picker.tsx`.
- **Design tokens (colors, radii, shadows, motion, z-index)**: `app/globals.css`'s `@theme inline { ... }` block and the `:root`/`.dark` variable declarations below it.
- **shadcn-generated UI primitives**: `components/ui/*` — one file per primitive (button, card, dialog, dropdown-menu, sheet, table, etc.), configured via `components.json`.
- **Dashboard feature (mock data + components)**: `features/dashboard/{sample-data.ts, stat-card.tsx, users-table.tsx}`.
- **Class-name merge helper**: `lib/utils.ts` (`cn`).

## Notes

<!-- manual: content below this line is human-authored and must be preserved verbatim during sync -->

---
_Generated: 2026-07-31 — scope: client app "admin" — see .claude/CLAUDE.md for update rules._
