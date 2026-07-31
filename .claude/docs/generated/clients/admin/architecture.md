# Architecture: admin

## Layering

Observed folder organization (verified via directory listing):

```text
app/                      routes (App Router) — layout.tsx, globals.css, (dashboard)/{layout.tsx,page.tsx}
components/
  ui/                     shadcn-CLI-generated primitives (owned source, not a runtime UI-kit dependency)
  layout/                 app chrome: topbar, sidebar, sidebar-nav-item, brand, breadcrumbs, accent-color-picker, theme-toggle, user-menu
  shared/                 cross-feature reusable pieces (currently just search-box.tsx)
features/
  dashboard/              feature-scoped: stat-card.tsx, users-table.tsx, sample-data.ts
hooks/                    use-sidebar, use-accent-color, use-scrolled, use-has-mounted
providers/                theme-provider.tsx (next-themes wrapper)
constants/                nav-items.ts
types/                    nav.ts
lib/                      utils.ts (cn helper)
```

This is a route → layout-chrome → feature-component → primitive layering: `app/*` pages compose `features/*` and `components/ui/*`; `components/layout/*` compose `components/ui/*` plus `hooks/*`/`constants/*`/`types/*`; `components/ui/*` are leaf primitives depending only on `lib/utils` and Radix/lucide.

## Dependency Direction

Verified via actual `import` statements in the files read:

```text
app/layout.tsx        -> providers/theme-provider, hooks/use-accent-color, components/ui/tooltip
app/(dashboard)/layout.tsx -> hooks/use-sidebar, components/layout/{sidebar,topbar}
app/(dashboard)/page.tsx   -> components/ui/card, features/dashboard/{stat-card,users-table,sample-data}

components/layout/topbar.tsx      -> lib/utils, hooks/{use-scrolled,use-sidebar}, components/ui/{button,badge},
                                      components/layout/{breadcrumbs,brand}, components/shared/search-box,
                                      components/layout/{theme-toggle,accent-color-picker,user-menu}
components/layout/sidebar.tsx     -> lib/utils, hooks/use-sidebar, components/layout/sidebar-nav-item,
                                      constants/nav-items, components/ui/sheet
components/layout/sidebar-nav-item.tsx -> lib/utils, hooks/use-sidebar, types/nav
components/layout/breadcrumbs.tsx -> components/ui/breadcrumb, constants/nav-items, types/nav
components/layout/user-menu.tsx   -> components/ui/{avatar,button,dropdown-menu} (no external data — MOCK_USER inline)
components/layout/accent-color-picker.tsx -> components/ui/{button,dropdown-menu}, hooks/use-accent-color
components/layout/theme-toggle.tsx        -> next-themes, hooks/use-has-mounted, components/ui/{button,dropdown-menu}

features/dashboard/stat-card.tsx   -> components/ui/card, lib/utils, features/dashboard/sample-data (types)
features/dashboard/users-table.tsx -> components/ui/{badge,table,pagination,avatar,empty}, features/dashboard/sample-data

components/ui/*  -> lib/utils, radix-ui (unified package), class-variance-authority, lucide-react
                     (button.tsx additionally -> components/ui/spinner)
```

Direction is one-way: `components/ui/*` never imports from `components/layout/*` or `features/*`; `features/*` and `components/layout/*` only import from `components/ui/*`, `hooks/*`, `lib/*`, `constants/*`, `types/*` — never from each other's sibling feature folders (there is currently only one feature, `dashboard`, so cross-feature isolation is unverified in practice).

## Key Design Patterns

- **Context-provider-per-concern for client state**: `SidebarProvider`/`useSidebar` (`hooks/use-sidebar.tsx`), `AccentColorProvider`/`useAccentColor` (`hooks/use-accent-color.tsx`), and `next-themes`' own provider (wrapped in `providers/theme-provider.tsx`) each own one slice of persisted UI state, each exposing a single custom hook and throwing if used outside its provider.
- **Owned, CLI-generated UI primitives** (not a runtime component-library dependency): `components/ui/*` was generated via the `shadcn` CLI (`components.json`, style `"radix-nova"`) on top of the unified `radix-ui` package and `class-variance-authority`. Components follow a consistent `data-slot="<name>"` attribute convention plus `cva()`-driven `variant`/`size` props (seen in `button.tsx`, `badge.tsx`, `card.tsx`, `avatar.tsx`, `empty.tsx`). `components/ui/button.tsx` has been hand-modified past what the CLI would generate: it adds a `loading` prop (renders `Spinner` + swaps `aria-busy`/`disabled`) and a `cursor-pointer` utility class baked into `buttonVariants` — a deliberate deviation to track if the component is ever regenerated via the CLI.
- **Single-CSS-variable theming**: `--primary` (defined in `app/globals.css`) is the one variable that drives button backgrounds, focus rings, and active-nav-item text color. Accent color presets (`:root[data-accent="blue"]`, `"violet"`, `"rose"`, `"orange"`, `"amber"`; green is the unattributed default) each override only `--primary` for light and (via `.dark[data-accent="..."]`) dark mode — every other themed token derives from it rather than being duplicated per accent.
- **Runtime accent swap via DOM attribute + localStorage**: `AccentColorProvider` sets `data-accent` on `document.documentElement` and persists the choice to `localStorage` (`admin.accent-color`); `SidebarProvider` does the same for sidebar hidden/expanded state (`admin.sidebar.hidden`, `admin.sidebar.expanded`).
- **Hydration-safe browser-state restoration**: because `localStorage` can't be read during SSR/first paint, both `SidebarProvider` and `AccentColorProvider` initialize with a default value, then restore the persisted value inside a `useEffect` gated by a `hydrated` flag (explicitly commented as intentional, with `eslint-disable react-hooks/set-state-in-effect`). `hooks/use-has-mounted.ts` (`useSyncExternalStore`) provides the same guard for `ThemeToggle`'s icon, which otherwise depends on `next-themes`' client-only `theme` value.
- **Mobile drawer closes on route change via render-time state adjustment** (not an effect): `SidebarProvider` compares `usePathname()` against a `prevPathname` state value during render and closes the mobile `Sheet` if it changed — the code cites the React docs' "adjusting state when a prop changes" pattern explicitly in a comment.
- **Dark mode via `next-themes`**, `attribute="class"`, `defaultTheme="system"`, `enableSystem`, `disableTransitionOnChange` — `.dark` class toggled on `<html>`; `app/layout.tsx` sets `suppressHydrationWarning` on `<html>` to avoid a hydration warning from the attribute being set client-side.

## Shared Kernel / Common Building Blocks Used

- `components/ui/*` — the app's own primitive layer (not shared with any sibling app; `clients/admin` is the only client app in the repo).
- `lib/utils.ts` (`cn`) — used by nearly every component in `components/ui/*`, `components/layout/*`, and `features/dashboard/*` for conditional/merged Tailwind classes.
- `hooks/*` — `use-sidebar`, `use-accent-color`, `use-scrolled`, `use-has-mounted` are cross-cutting building blocks consumed by layout components.
- No package or code is shared with another client app — `clients/` currently has only `admin`.

## Module/Route Boundaries

Only one route group exists: `(dashboard)`, containing the single route `/`. There are no other route groups or route-level boundaries to evaluate yet. `constants/nav-items.ts` declares additional nav targets (`/identity`, `/identity/users`, `/identity/roles`, `/settings`) that have no corresponding routes under `app/` — these represent planned/future route boundaries, not currently enforced ones.

## Known Architectural Risks / Debt

| Finding | Severity | Notes |
|---|---|---|
| Nav items reference routes with no `page.tsx` (`/identity`, `/identity/users`, `/identity/roles`, `/settings`) | Low | Currently a UI-shell-only build; clicking these links would 404. Expected at this stage, but worth tracking as these get built out. |
| No API client layer / no backend integration | Low (by design) | Explicitly scoped out for this pass — dashboard renders only `features/dashboard/sample-data.ts`. Not a defect, but the next feature phase needs to establish the `lib/api/`-equivalent pattern. |
| `prettier` + `prettier-plugin-tailwindcss` are installed but have no config file (`.prettierrc*`) and no `format`/related npm script | Low | `unknown` whether formatting is enforced anywhere (editor-only vs. CI) — flagged rather than assumed. |
| No automated test suite (no `*.test.*`/`*.spec.*` files, no test runner in `package.json`) | Low (by design at this stage) | Matches an early UI-shell build; will need addressing once business logic accrues. |
| `components/ui/button.tsx` hand-modified beyond shadcn CLI output (`loading` prop, `cursor-pointer`) | Low | Not a risk per se, but re-running the shadcn CLI to update `button.tsx` would silently drop these customizations unless done carefully. |

## Notes

<!-- manual: content below this line is human-authored and must be preserved verbatim during sync -->

---
_Generated: 2026-07-31 — scope: client app "admin" — see .claude/CLAUDE.md for update rules._
