# Client App Overview: admin

## Purpose

Internal admin console for the ModularMonolith starter template. Per `app/layout.tsx` metadata: "Admin Dashboard" / "Admin dashboard for the ModularMonolith starter kit." This is the **first client app** built in this template repo (`clients/web` does not exist — verified via glob).

As of this doc, the app is a **UI-shell-only** build: layout chrome (top bar, sidebar, theming) and one dashboard page rendering static mock data. No calls into `src/Identity.Api` or any other backend module exist yet — verified by grepping the app for `fetch(`, `process.env`, and `NEXT_PUBLIC` (no matches), and there is no `lib/api/` directory.

## Structure

- **Router**: App Router — `app/layout.tsx` (root layout) → `app/(dashboard)/layout.tsx` (route group layout) → `app/(dashboard)/page.tsx` (only page). Verified via `app/` directory listing; no `pages/` directory exists.
- **Package manager**: pnpm — verified via `pnpm-lock.yaml` at `clients/admin/pnpm-lock.yaml`.
- **Data fetching approach**: None yet. The only page (`app/(dashboard)/page.tsx`) reads static arrays from `features/dashboard/sample-data.ts` (`STAT_SUMMARIES`, `SAMPLE_USERS`). No server components fetch data, no server actions, no React Query/SWR dependency in `package.json`.
- **State management**: Local component state + React Context, no global state library:
  - `hooks/use-sidebar.tsx` (`SidebarProvider`/`useSidebar`) — sidebar hidden/expanded/mobile-open state, persisted to `localStorage`.
  - `hooks/use-accent-color.tsx` (`AccentColorProvider`/`useAccentColor`) — accent color selection, persisted to `localStorage`, applied via `data-accent` attribute on `<html>`.
  - `providers/theme-provider.tsx` — thin wrapper around `next-themes`' `ThemeProvider` (`attribute="class"`, `defaultTheme="system"`, `enableSystem`).
  - `features/dashboard/users-table.tsx` — local `useState` for pagination (`page`).
- **Styling**: Tailwind CSS v4, CSS-first configuration — `app/globals.css` uses `@import "tailwindcss"` plus an inline `@theme inline { ... }` block for design tokens (colors, radii, shadows, motion durations, z-index scale). No `tailwind.config.ts`/`.js` file exists (verified via glob) — consistent with Tailwind v4's CSS-first config model. `postcss.config.mjs` wires in `@tailwindcss/postcss`.

## Key Routes/Areas

| Route/Area | Path | Responsibility | Notes |
|---|---|---|---|
| Dashboard | `app/(dashboard)/page.tsx` (route `/`) | Overview page: 4 stat cards (`STAT_SUMMARIES`) + a paginated "Recent users" table (`SAMPLE_USERS`) | Only implemented page; all data is mock, sourced from `features/dashboard/sample-data.ts` |
| Dashboard layout | `app/(dashboard)/layout.tsx` | Wraps all dashboard-group routes with `SidebarProvider`, `TopBar`, `Sidebar` | Route group `(dashboard)` — currently the only route group |
| Root layout | `app/layout.tsx` | Loads `Inter` font, wraps app in `ThemeProvider` → `AccentColorProvider` → `TooltipProvider` | Sets `<html suppressHydrationWarning>` for `next-themes` compatibility |

`constants/nav-items.ts` declares nav entries for `/identity` (with children `/identity/users`, `/identity/roles`) and `/settings`, but **no corresponding `page.tsx` files exist for any of these paths** — verified via `app/` directory listing. These are sidebar-navigation placeholders only; following them today would 404.

## Backend Integration

None yet. There is no `lib/api/` directory, no typed API client, and no code that references `src/Identity.Api` or constructs a base API URL. All data on the dashboard page is hardcoded in `features/dashboard/sample-data.ts` (explicitly commented there: "this UI shell does not call src/Identity.Api yet"). See `.claude/ARCHITECTURE.md` (Integration section) for the intended contract once wired up; there is nothing in `.claude/docs/generated/backend/` yet to link to for a concrete API surface (out of scope for this doc — not modified here).

## Auth Flow

Not implemented. `components/layout/user-menu.tsx` renders a hardcoded `MOCK_USER` object (name/username/email/initials) with no session/token logic, no cookie/JWT handling, and no login route. Sign-out ("Log out") is a menu item with no handler wired up.

## External Dependencies

From `package.json` `dependencies`:

- **`next` 16.2.12, `react`/`react-dom` 19.2.4** — framework/runtime.
- **`radix-ui` ^1.6.7** — unified Radix UI primitives package (single package covering what used to be many `@radix-ui/react-*` packages), used as the headless base for `components/ui/*` (avatar, dropdown-menu, dialog, sheet, tabs, tooltip, etc.) and for `Slot` in `button.tsx`.
- **`class-variance-authority` ^0.7.1** — variant-driven class composition, used in `components/ui/*` (`button.tsx`, `badge.tsx`, `alert.tsx`, `empty.tsx`, `tabs.tsx`).
- **`clsx` ^2.1.1 + `tailwind-merge` ^3.6.0** — combined in `lib/utils.ts`'s `cn()` helper, used throughout for conditional/merged class names.
- **`lucide-react` ^1.28.0** — icon set used across nav items, top bar, theme/accent pickers, dashboard stat trend icons.
- **`next-themes` ^0.4.6** — dark/light/system theme switching (`providers/theme-provider.tsx`, `class` attribute strategy).
- **`shadcn` ^4.16.0** — listed as a runtime `dependency` (not just a dev CLI tool): `app/globals.css` imports `shadcn/tailwind.css` directly. Also used as the CLI that generated `components/ui/*` (per `components.json`, style `"radix-nova"`, base color `"neutral"`).
- **`tw-animate-css` ^1.4.0** — animation utility classes, imported in `app/globals.css`.

From `devDependencies`: `@tailwindcss/postcss`/`tailwindcss` (Tailwind v4 build pipeline), `eslint`/`eslint-config-next` (linting), `prettier`/`prettier-plugin-tailwindcss` (installed but **no `.prettierrc*` file and no `format` script exist** — unknown whether/how these are actually invoked; likely editor-integration-only, unverified), `typescript`, `@types/*`.

## Relationship to Other Client Apps

`clients/admin` is the only subfolder under `clients/` (verified: `clients/web/**` glob returned no results). It is fully independent — there is no shared package, no monorepo tooling (no workspace root `package.json`, no Turborepo/Nx config found), and nothing to compare against yet.

## Notes

<!-- manual: content below this line is human-authored and must be preserved verbatim during sync -->

---
_Generated: 2026-07-31 — scope: client app "admin" — see .claude/CLAUDE.md for update rules._
