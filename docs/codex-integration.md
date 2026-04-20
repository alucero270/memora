# Codex-Oriented Integration Guidance

## Purpose

This guide explains how to use Memora effectively from Codex-oriented
environments without changing Memora's architecture or overstating current
capabilities.

It is grounded in the current repo and local workflow, not future runtime work.

## Current Fit

Codex-oriented workflows already operate directly in the repository and local
shell environment.

That makes the current Memora path straightforward:

- use repo docs and approved artifacts as the first grounding layer
- use `Memora.Ui` for local inspection when a human-oriented view is helpful
- use `Memora.Api` as the current structured companion surface
- do not claim a hosted Memora MCP transport until one is actually shipped

## Recommended Current Workflow

### 1. Start From Repo Truth

In Codex-oriented work, build context from:

- `AGENTS.md`
- `docs/current-state.md`
- relevant module README files
- current branch and working tree state
- approved workspace artifacts when a configured workspace root is available

This keeps Codex grounded in the same deterministic Memora truth model the repo
enforces elsewhere.

### 2. Run The Companion Services When Needed

Set a workspace root and run the local API when structured requests are useful:

```powershell
$env:MEMORA_WORKSPACES_ROOT = "C:\\path\\to\\memora-workspaces"
dotnet run --project src/Memora.Api
```

Optional UI shell for human inspection:

```powershell
dotnet run --project src/Memora.Ui
```

### 3. Use The Current Structured Surface Deliberately

The companion API currently exposes:

- `GET /api/projects/{projectId}`
- `POST /api/context`
- `POST /api/artifacts/proposals`
- `POST /api/artifacts/updates`
- `POST /api/outcomes`

The current OpenAPI document is published at:

- `http://127.0.0.1:5081/openapi.json`

From a Codex-oriented shell, that means structured Memora calls can be made
with local HTTP tooling when helpful, while the repo and workspace files remain
the higher-priority source of truth.

## Suggested Codex Usage Pattern

Codex-oriented workflows fit best with Memora when they:

- read project rules and approved artifacts first
- request deterministic context when the current task needs structured grounding
- generate proposals or outcomes through reviewable non-canonical flows
- keep human approval in the loop for canonical changes

This keeps Codex aligned with Memora's current governance model instead of
treating Memora like a direct-write execution runtime.

## Configuration Guidance

### Useful Local Configuration

- `MEMORA_WORKSPACES_ROOT` or `Memora:WorkspacesRootPath`
- API base URL: `http://127.0.0.1:5081`
- OpenAPI document: `http://127.0.0.1:5081/openapi.json`
- UI base URL: `http://127.0.0.1:5080`

### Current Limits To Keep Explicit

- the repo does not yet ship a hosted Memora MCP server transport
- the API surface is companion-only and intentionally thin
- Codex-facing workflows must not be described as direct canonical write paths
- retrieval remains deterministic and non-semantic in core v1

## Example Local Shell Flow

Start the API:

```powershell
dotnet run --project src/Memora.Api
```

Inspect the published API document:

```powershell
curl http://127.0.0.1:5081/openapi.json
```

Request deterministic context:

```powershell
$body = @{
  projectId = "memora"
  taskDescription = "Review the current integration contracts."
} | ConvertTo-Json

Invoke-RestMethod `
  -Method Post `
  -Uri "http://127.0.0.1:5081/api/context" `
  -ContentType "application/json" `
  -Body $body
```

## Validation

Use the current API validation suite after changes:

```powershell
dotnet test tests/Memora.Api.Tests/Memora.Api.Tests.csproj --no-restore -p:UseSharedCompilation=false
```
