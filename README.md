# Memora

Memora is a local-first structured memory and governance system for
AI-assisted software development.

It is built around a few non-negotiable rules:

- filesystem is the canonical source of truth
- SQLite is derived and rebuildable
- retrieval is deterministic and explainable
- agents may propose changes in v1, but they do not directly write canonical truth
- lifecycle and approval rules are enforced in core

## What This Checkout Contains

This checkout includes working slices across:

- core artifact schemas, lifecycle rules, validation, editing, approval queue, and diffs
- validation diagnostics that surface code and path context for operators and integrations
- filesystem parsing and persistence for canonical, draft, and summary artifacts
- SQLite rebuild-from-files indexing with filesystem-first rebuild diagnostics
- deterministic context ranking, inclusion reasoning, and layered context assembly
- a provider-agnostic external runtime contract reused by MCP and OpenAPI
- a minimal local HTTP API for project lookup, context assembly, proposals, updates, and outcomes
- a thin MCP surface over the shared agent interaction contract
- Machina-to-Memora interaction guidance that keeps runtime execution outside Memora
- runtime-facing prototype and compatibility validation for context and proposal flows across MCP and OpenAPI
- controlled automation policy models, safe trigger evaluation, and a guarded session-summary direct-write prototype
- a styled local operator UI with approval review navigation, clearer revision diffs, a context viewer route, and an understanding-output route
- operator workflow guidance for review, draft editing, diff inspection, and rebuild recovery

Important limits still apply:

- canonical truth remains filesystem-first and approval-governed
- controlled automation is limited to explicit policy checks and non-canonical session-summary writes
- no semantic or vector retrieval exists in core v1
- the UI shows review previews and inactive approval decision controls, but it does not persist approval or rejection decisions
- the MCP layer is currently a thin in-process adapter surface, not a production transport host
- provider-facing runtime alignment is shared-contract based; provider-specific attachments still remain separate follow-up work

## Start Here

If you are orienting yourself in the repo, this order works well:

1. [docs/current-state.md](docs/current-state.md)
2. [docs/architecture.md](docs/architecture.md)
3. [docs/milestones.md](docs/milestones.md)
4. [docs/operator-workflows.md](docs/operator-workflows.md)
5. [src/README.md](src/README.md)
6. [tests/README.md](tests/README.md)

## Local Run

Build everything:

- `dotnet build Memora.sln`

Run the UI:

- startup project: `src/Memora.Ui`
- default dev URL: `http://127.0.0.1:5080`
- when no workspace root is configured, the UI uses a writable local copy of `samples/workspaces`

Run the API:

- startup project: `src/Memora.Api`
- default dev URL: `http://127.0.0.1:5081`
- set `MEMORA_WORKSPACES_ROOT` or `Memora:WorkspacesRootPath` to use the file-backed service

Smallest useful validation:

- `dotnet test tests/Memora.Core.Tests/Memora.Core.Tests.csproj`
- `dotnet test tests/Memora.Storage.Tests/Memora.Storage.Tests.csproj`
- `dotnet test tests/Memora.Index.Tests/Memora.Index.Tests.csproj`
- `dotnet test tests/Memora.Context.Tests/Memora.Context.Tests.csproj`
- `dotnet test tests/Memora.Api.Tests/Memora.Api.Tests.csproj`
- `dotnet test tests/Memora.Mcp.Tests/Memora.Mcp.Tests.csproj`
- `dotnet test tests/Memora.Ui.Tests/Memora.Ui.Tests.csproj`

## Repo Map

- [docs/](docs/README.md): architecture, scope, roadmap, and current-state docs
- [samples/](samples/README.md): demo workspaces and fixtures
- [src/](src/README.md): product code by module boundary
- [tests/](tests/README.md): automated validation by module

## Status

Memora is no longer at scaffold-only status. This checkout contains real
foundational, integration, and UI slices, but the product is still in an
early, honesty-first phase where some surfaces are intentionally thin.

Use [docs/current-state.md](docs/current-state.md) for the most accurate
summary of implemented behavior in this checkout.
