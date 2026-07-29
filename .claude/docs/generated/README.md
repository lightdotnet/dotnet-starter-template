# Generated Documentation

This directory holds documentation generated from actual code, produced only by explicit [generate-docs](../../skills/generate-docs.md) / [sync-docs](../../skills/sync-docs.md) requests (see [workflows/sync-documentation.md](../../workflows/sync-documentation.md)). Nothing here is created automatically as a side effect of other work.

## Layout Convention

Docs are organized by stack (backend / clients), then by module or client app — `clients/` may hold more than one app, each getting its own subfolder:

```text
docs/generated/
├── repository-overview.md            # from templates/repository-overview.md, whole-repo scope only
├── backend/
│   ├── overview.md                   # from templates/solution-overview.md
│   ├── architecture.md               # from templates/architecture.md
│   ├── database.md                   # from templates/database.md
│   ├── api.md                        # from templates/api-documentation.md
│   ├── dependency-graph.md           # from templates/dependency-graph.md
│   ├── coding-conventions.md         # from templates/coding-conventions.md
│   ├── development-guide.md          # from templates/development-guide.md
│   └── modules/
│       └── <ModuleName>/
│           ├── overview.md           # from templates/module-overview.md
│           ├── domain-model.md       # from templates/domain-model.md
│           └── <ProjectName>/
│               └── overview.md       # from templates/project-overview.md
├── frontend-overview.md              # from templates/frontend-overview.md — index across ALL apps in clients/
└── clients/
    └── <app-name>/                   # one subfolder per app under clients/, e.g. web/
        ├── overview.md                   # from templates/client-app-overview.md
        ├── architecture.md               # from templates/architecture.md
        ├── dependency-graph.md           # from templates/dependency-graph.md
        ├── coding-conventions.md         # from templates/coding-conventions.md
        └── development-guide.md          # from templates/development-guide.md
```

Each generated file carries a `<!-- manual -->`-marked section (see the templates) that must be preserved verbatim during any sync.

This directory is currently empty — nothing has been generated yet. It is populated incrementally, one scope at a time, on request.
