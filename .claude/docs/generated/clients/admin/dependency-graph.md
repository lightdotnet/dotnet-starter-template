# Dependency Graph: admin

## Internal Module Imports

Verified via actual `import` statements (see [architecture.md](./architecture.md#dependency-direction) for the full trace):

| From | To | Notes |
|---|---|---|
| `app/layout.tsx` | `providers/theme-provider.tsx`, `hooks/use-accent-color.tsx`, `components/ui/tooltip.tsx` | Root layout composes the three top-level providers |
| `app/(dashboard)/layout.tsx` | `hooks/use-sidebar.tsx`, `components/layout/sidebar.tsx`, `components/layout/topbar.tsx` | Route-group layout |
| `app/(dashboard)/page.tsx` | `components/ui/card.tsx`, `features/dashboard/stat-card.tsx`, `features/dashboard/users-table.tsx`, `features/dashboard/sample-data.ts` | Only page |
| `components/layout/topbar.tsx` | `lib/utils.ts`, `hooks/use-scrolled.ts`, `hooks/use-sidebar.tsx`, `components/ui/{button,badge}.tsx`, `components/layout/{breadcrumbs,brand,theme-toggle,accent-color-picker,user-menu}.tsx`, `components/shared/search-box.tsx` | Composes nearly all layout components |
| `components/layout/sidebar.tsx` | `lib/utils.ts`, `hooks/use-sidebar.tsx`, `components/layout/sidebar-nav-item.tsx`, `constants/nav-items.ts`, `components/ui/sheet.tsx` | Desktop `<aside>` + mobile `Sheet` |
| `components/layout/sidebar-nav-item.tsx` | `lib/utils.ts`, `hooks/use-sidebar.tsx`, `types/nav.ts` | Recursive (renders itself for `item.children`) |
| `components/layout/breadcrumbs.tsx` | `components/ui/breadcrumb.tsx`, `constants/nav-items.ts`, `types/nav.ts` | Derives labels from `NAV_ITEMS` |
| `components/layout/accent-color-picker.tsx` | `components/ui/{button,dropdown-menu}.tsx`, `hooks/use-accent-color.tsx` | |
| `components/layout/theme-toggle.tsx` | `next-themes`, `hooks/use-has-mounted.ts`, `components/ui/{button,dropdown-menu}.tsx` | |
| `components/layout/user-menu.tsx` | `components/ui/{avatar,button,dropdown-menu}.tsx` | Data is an inline `MOCK_USER` constant, no external import |
| `components/layout/brand.tsx` | `next/link` only | No hooks — server-renderable |
| `features/dashboard/stat-card.tsx` | `components/ui/card.tsx`, `lib/utils.ts`, `features/dashboard/sample-data.ts` (type only) | |
| `features/dashboard/users-table.tsx` | `components/ui/{badge,table,pagination,avatar,empty}.tsx`, `features/dashboard/sample-data.ts` | |
| `components/ui/*` | `lib/utils.ts`, `radix-ui`, `class-variance-authority`, `lucide-react` | `button.tsx` additionally imports `components/ui/spinner.tsx` |
| `hooks/use-sidebar.tsx` | `next/navigation` (`usePathname`) | For the mobile-drawer-closes-on-navigate behavior |
| `providers/theme-provider.tsx` | `next-themes` | Thin re-export wrapper |

## Package References

From `clients/admin/package.json` (`dependencies`):

| Package | Version | Notes |
|---|---|---|
| `next` | `16.2.12` | Framework |
| `react` | `19.2.4` | |
| `react-dom` | `19.2.4` | |
| `radix-ui` | `^1.6.7` | Unified Radix primitives package; base for `components/ui/*` |
| `class-variance-authority` | `^0.7.1` | Variant class composition (`cva`) |
| `clsx` | `^2.1.1` | Used inside `lib/utils.ts`'s `cn()` |
| `tailwind-merge` | `^3.6.0` | Used inside `lib/utils.ts`'s `cn()` |
| `lucide-react` | `^1.28.0` | Icon set |
| `next-themes` | `^0.4.6` | Theme (light/dark/system) switching |
| `shadcn` | `^4.16.0` | CLI that generated `components/ui/*`; also imported at runtime (`shadcn/tailwind.css` in `app/globals.css`) |
| `tailwind-merge` | (see above) | |
| `tw-animate-css` | `^1.4.0` | Animation utility classes, imported in `app/globals.css` |

`devDependencies`:

| Package | Version | Notes |
|---|---|---|
| `@tailwindcss/postcss` | `^4` | Tailwind v4 PostCSS plugin |
| `tailwindcss` | `^4` | |
| `@types/node` | `^20` | |
| `@types/react` | `^19` | |
| `@types/react-dom` | `^19` | |
| `eslint` | `^9` | |
| `eslint-config-next` | `16.2.12` | Pinned to match `next`'s exact version |
| `prettier` | `^3.9.6` | No config file/`format` script found — see [coding-conventions.md](./coding-conventions.md) |
| `prettier-plugin-tailwindcss` | `^0.8.1` | |
| `typescript` | `^5` | |

Package manager: pnpm (`pnpm-lock.yaml` present).

## Circular References

None found among internal module imports (see table above) — `components/ui/*` is a strict leaf layer with no imports from `components/layout/*` or `features/*`.

## Version Mismatches

Not applicable — `clients/admin` is the only client app in the repo (`clients/web` does not exist), so there is no sibling app to compare package versions against.

## Cross-Module Boundary Violations (backend only)

Not applicable — this is a client-app dependency graph, not backend.

## Notes

<!-- manual: content below this line is human-authored and must be preserved verbatim during sync -->

---
_Generated: 2026-07-31 — scope: client app "admin" — see .claude/CLAUDE.md for update rules._
