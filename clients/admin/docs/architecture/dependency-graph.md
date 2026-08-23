# Dependency Graph: admin

## Package References

The full dependency/devDependency list with exact versions lives in `clients/admin/package.json` — short enough to read directly rather than duplicating a version table here that would just drift out of sync. Grouped by purpose:

- **Framework/runtime**: `next`, `react`, `react-dom`.
- **UI primitives**: `radix-ui` (unified Radix primitives, base for `components/ui/*`), `class-variance-authority` (`cva`), `clsx` + `tailwind-merge` (combined in `lib/shared/utils.ts`'s `cn()`), `lucide-react` (icon set), `cmdk` (filterable-list primitive behind `components/ui/command.tsx`, the base of `components/ui/combobox.tsx`).
- **Theming/toast**: `next-themes` (light/dark/system switching; also drives `components/toast/toaster.tsx`'s toast theme), `sonner` (toast notifications, wrapped by `components/toast/`, never imported directly by feature code), `tw-animate-css` (animation utility classes).
- **Feature-specific**: `qrcode` (QR code on `/user-profile`), `@microsoft/signalr` (real-time push client, used only by `features/notifications/hooks/use-notifications.ts`).
- **Positioning/virtualization**: `@floating-ui/react` (Command Palette + the surviving `components/foundation/{use-listbox,floating-overlay}`), `@tanstack/react-virtual` (headless list virtualization behind `components/foundation/use-virtual-list.ts`, used by the DataTable's `"virtualized"`/`"infinite"` modes and the Command Palette's result list).
- **Server-only guard**: `server-only` — compile-time marker forcing a build error if a `lib/server/*` module is ever imported into a Client Component; added to all 15 files under `lib/server/`.
- **Styling toolchain** (devDependencies): `tailwindcss` + `@tailwindcss/postcss` (Tailwind v4), `shadcn` (CLI that generated `components/ui/*`; also imported at runtime for `shadcn/tailwind.css`), `prettier` + `prettier-plugin-tailwindcss` (no config file/`format` script found — see [coding-conventions.md](../conventions/coding-conventions.md)).
- **Tooling** (devDependencies): `eslint` + `eslint-config-next` (pinned to match `next`'s version), `typescript`, `@types/*`.

Package manager: pnpm (`pnpm-lock.yaml`). `pnpm-workspace.yaml` exists but only configures build-script approval (`sharp`, `unrs-resolver`) — not a multi-package workspace.

## Circular References

None found among internal module imports. `components/ui/*` is not an absolutely strict leaf layer: `components/ui/dialog.tsx` and `components/ui/popover.tsx` both import `components/foundation/portal-container.ts`, a deliberate, narrow exception (a dependency-free React Context) that lets a Popover portal into an open Dialog's own DOM node rather than `document.body` (see [architecture.md](./architecture.md#key-design-patterns)). `components/ui/*` never imports from `components/layout/*`, `components/theme/*`, or `features/*`, and no cycle results — `components/foundation/*` doesn't import back from `components/ui/*`.

Seven narrow, reasoned exceptions to the barrel-only cross-feature-import rule exist — `constants/nav-items.ts`, `components/layout/user-menu.tsx`, `components/layout/topbar.tsx`, `components/layout/session-gate.tsx` (→ `features/auth/api/ensure-fresh-session-action`), `features/users/components/edit-user-dialog.tsx` (→ `features/roles/api/get-all-roles-action`), `features/notifications/components/user-select.tsx` (→ `features/users/api/search-users-action`), and `features/home/components/home-page.tsx` (→ `features/notifications/components/notification-inbox`, `features/notifications/api/get-my-notifications-action`) — each importing one specific file directly instead of through a barrel. All seven are driven by the RSC client/server boundary: the target barrel also re-exports an async Server Component (`UsersPage`/`RolesPage`/`NotificationsPage`/`LoginPage`) that calls `resolveSession()`/reads cookies via `next/headers`, so importing the full barrel from a `"use client"` component (or, for `nav-items.ts`, from client-side `Sidebar`) would drag that server-only chain into the client bundle.

Two more, lower-level exceptions exist at the `lib/server` tier, both a reversal of the usual `features/* -> lib/server/*` direction: `lib/server/refresh-session.ts` imports `features/auth/api/token.api.ts` directly (bypassing the `@/features/auth` barrel, which does export `refreshToken`), and `lib/server/refetch-profile.ts` imports `features/user-profile/api/user-profile.api.ts` directly (bypassing the `@/features/user-profile` barrel, which does export `getCurrentUser`) — both are called from `features/auth/api/ensure-fresh-session-action.ts`, the Server Action `components/layout/session-gate.tsx` drives (see [architecture.md](./architecture.md#key-design-patterns)). This is a change from the previous design, where `src/proxy.ts` itself held the one direct feature-api-file import at the routing layer (`getCurrentUser`) — that import moved into `refetch-profile.ts` once the refresh/profile-freshness logic moved out of middleware. `lib/server/require-permission.tsx` does **not** share this reversal — it imports `features/user-profile`'s barrel the same way ordinary page code does.

`components/shared/data-table/*`, `components/shared/object-viewer/*`, `components/shared/access-denied.tsx`, and `components/toast/*` are consumed by feature code (or, for `access-denied.tsx`, by `lib/server/require-permission.tsx`) but import nothing from `features/*` themselves, so no cycle there either. `components/foundation/*` and `components/command/*` import only `@floating-ui/react`/`@tanstack/react-virtual`/`lucide-react`/`react`/each other — no `features/*` dependency, no cycle.

## Version Mismatches

Not applicable — `clients/admin` is still the only client app in the repo.

## Cross-Module Boundary Violations (backend only)

Not applicable — this is a client-app dependency graph, not backend.

## Notes

<!-- manual: content below this line is human-authored and must be preserved verbatim during sync -->

---
_Last synced: 2026-08-22_
