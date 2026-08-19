<!--
Template: Repository Overview
Used by: skills/generate-docs.md, skills/sync-docs.md
Output location: .claude/docs/generated/repository-overview.md
Do not populate this file itself — copy its structure into the generated output.
-->

# Repository Overview

## Purpose

_What this repository is for, in one or two sentences (starter template monorepo: C#/.NET Modular Monolith backend + one or more frontend clients)._

## Repository Shape

- **Type**: Full-stack starter template monorepo
- **Backend**: `src/` — ASP.NET Core, C#, Modular Monolith, API-only
- **Clients**: `clients/<app-name>/` — one or more apps; list actual count/names once verified (don't assume exactly one)
- **Integration**: REST/JSON over HTTP only

## Backend Modules Index

| Module | Path | Purpose | Docs |
|---|---|---|---|
| | | | [overview](backend/modules/<module>/overview.md) |

## Client Apps Index

| App | Path | Purpose | Docs |
|---|---|---|---|
| | `clients/<app-name>/` | | [overview](clients/<app-name>/overview.md) |

## Shared/Common Building Blocks

_Backend shared/building-blocks project, any shared API client/UI kit reused across client apps — only if verified to be actually shared._

## How to Navigate This Repo

_Practical guidance for a newcomer: where to start reading (e.g. a specific module for backend logic, a specific client app/route for frontend UI), and where the API contract between them lives._

---
_Last synced: <date>_
