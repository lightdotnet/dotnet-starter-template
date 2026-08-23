# Admin Dashboard

Admin dashboard client app for the ModularMonolith starter kit — Next.js (App Router), TypeScript, Tailwind CSS v4.

Real backend integration against both `src/Identity.Api` and `src/Notifications.Api` (each wired as its own named backend client): encrypted-cookie auth with proactive token refresh, full Users/Roles CRUD (incl. a custom-claims editor on role edit), real-time Notifications (SignalR) with a topbar bell and a two-pane inbox, and permission-gated navigation. The Home page (`/`) shows a session-backed profile summary plus the live notification inbox — no mock data.

## Getting Started

```bash
pnpm install
pnpm dev
```

Open [http://localhost:3000](http://localhost:3000).

## Scripts

- `pnpm dev` — start the dev server
- `pnpm build` — production build
- `pnpm start` — serve a production build
- `pnpm lint` — run ESLint

## Docs

See [CLAUDE.md](CLAUDE.md) for project-specific rules, and `docs/` for architecture and convention docs.
