# Development Conventions

> Template. Fill in per-module/per-client-app conventions only as they're actually observed in code (via `analyze-solution`/`analyze-module`/`analyze-frontend`/`analyze-client`), not from assumption. This file aggregates verified conventions; it is not a style guide invented up front.

## How to Use This File

- Each section is split Backend / Clients; note deviations per-module or per-client-app rather than assuming repo-wide uniformity, unless verified (e.g. a root `.editorconfig`, a shared `Directory.Build.props`). Once more than one client app exists, don't assume they share conventions — each app gets its own subsection if they diverge.
- Do not add entries speculatively. If a convention hasn't been observed in actual code, leave the section marked `unknown`.

## Backend (`src/`)

### Build & Tooling

- **Target framework(s)**: _unknown_
- **Shared build props/targets** (`Directory.Build.props`, `Directory.Build.targets`, `Directory.Packages.props`): _unknown_
- **Central package management**: _unknown_
- **Module structure convention**: _unknown_ (expected: `src/Modules/<ModuleName>/{Domain,Application,Infrastructure,Api}`)

### Coding Style

- **`.editorconfig` present**: _unknown_
- **Nullable reference types**: _unknown_
- **Naming conventions observed**: _unknown_
- **Formatting/analyzer rules**: _unknown_

### Module Layering Conventions

> Only document a layering convention once seen consistently across multiple modules — otherwise note it as local to one module. See [agents/architecture-reviewer.md](agents/architecture-reviewer.md).

- _unknown_

### Dependency Injection Patterns

- _unknown_

### Error Handling / Result Patterns

- _unknown_

### Logging Conventions

- _unknown_

### Testing Conventions

- **Test framework(s)**: _unknown_
- **Naming convention for test classes/methods**: _unknown_
- **Mocking library**: _unknown_
- **Test project layout**: _unknown_

### EF Core Conventions

> See also [agents/efcore-specialist.md](agents/efcore-specialist.md), [skills/efcore.md](skills/efcore.md).

- **Migration strategy**: _unknown_
- **Configuration style** (Fluent API vs. attributes): _unknown_
- **Naming conventions for tables/columns**: _unknown_
- **DbContext-per-module boundary respected**: _unknown_

### API Conventions

> See also [agents/api-designer.md](agents/api-designer.md), [skills/api.md](skills/api.md).

- **Versioning strategy**: _unknown_
- **Response/error contract shape**: _unknown_
- **Controllers are API-only (no Razor views)**: _unknown — verify, this is the intended default_

## Clients (`clients/<app-name>/`)

> There may be more than one app under `clients/` — repeat this subsection per app if their conventions diverge (e.g. `clients/web/` vs a future `clients/admin/`). Don't assume a convention observed in one app applies to another until verified.

### `clients/web/` (primary app — rename/duplicate this heading once other apps exist)

#### Build & Tooling

- **Next.js version / router (App vs Pages)**: _unknown_
- **Package manager**: _unknown_
- **`tsconfig.json` strictness**: _unknown_

#### Coding Style

- **Linting** (ESLint config): _unknown_
- **Formatting** (Prettier or other): _unknown_
- **Component naming/file organization**: _unknown_

#### Data Fetching & State

- **Server vs client components usage**: _unknown_
- **Data fetching library** (React Query/SWR/native fetch/server actions): _unknown_
- **Global state management**: _unknown_

#### Styling

- **Styling approach** (Tailwind/CSS Modules/other): _unknown_

#### Testing Conventions

- **Test framework(s)** (Jest/Vitest + Testing Library, Playwright, etc.): _unknown_
- **Naming convention**: _unknown_

## Full-Stack Integration Conventions

> See also [agents/api-contract-reviewer.md](agents/api-contract-reviewer.md), [skills/nextjs.md](skills/nextjs.md). If multiple clients exist, note per-client deviations explicitly rather than assuming they all integrate the same way.

- **API client generation** (hand-written vs generated from OpenAPI), per client app: _unknown_
- **Environment/config for API base URL**, per client app: _unknown_
- **Auth token handling between each client and the backend**: _unknown_
- **Error contract mapping** (backend error shape → client handling): _unknown_

## Versioning & Release

- **Versioning scheme**: _unknown_
- **Changelog convention**: _unknown_

---
_Last updated: never (template not yet populated)_
