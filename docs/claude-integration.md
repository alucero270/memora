# Claude-Oriented Integration Guidance

## Purpose

This guide explains the current Memora path for Claude-oriented workflows
without claiming a Claude-specific adapter layer that the repo does not ship.

It is intentionally grounded in the current implementation, not the roadmap.

## Current Fit

Anthropic's current Claude integration docs center MCP-based server
configuration.

Memora's current repo state is:

- `Memora.Mcp` defines a thin MCP adapter over the shared agent interaction contract
- the repo does not yet ship a hosted MCP server transport you can register directly with Claude Desktop or Claude Code
- `Memora.Api` does publish a companion OpenAPI surface at `http://127.0.0.1:5081/openapi.json`

That means the current Claude-oriented path is:

- use Memora's local files, docs, and UI as the canonical operator surface
- run the companion API when a Claude-adjacent workflow can consume HTTP tools through your existing environment
- do not describe the current repo as if it already exposes a ready-to-register Claude MCP server

## Recommended Current Workflow

### 1. Run Memora Locally

Set a workspace root and run the local services you need:

```powershell
$env:MEMORA_WORKSPACES_ROOT = "C:\\path\\to\\memora-workspaces"
dotnet run --project src/Memora.Api
```

Optional UI shell:

```powershell
dotnet run --project src/Memora.Ui
```

### 2. Verify The Companion API

Confirm that the API host and published document are reachable:

```powershell
curl http://127.0.0.1:5081/openapi.json
```

### 3. Keep Claude Grounded In Memora Truth

When a Claude-oriented workflow has access to the local companion API or local
repo files, keep the interaction bounded by current Memora rules:

- use approved artifacts and deterministic context as the grounding source
- treat artifact creation and updates as proposals, not canonical writes
- keep filesystem truth and approval gating above any Claude-facing workflow

## Configuration Guidance

### What To Configure Now

- `MEMORA_WORKSPACES_ROOT` or `Memora:WorkspacesRootPath` for file-backed API behavior
- the local API base URL: `http://127.0.0.1:5081`
- the published OpenAPI document: `http://127.0.0.1:5081/openapi.json`

### What Not To Configure Yet

Do not document or rely on a direct Claude Desktop or Claude Code MCP server
registration for Memora itself yet.

That hosted transport is not part of the current repo implementation, so a doc
that presents it as available would be overstating current capability.

## Suggested Claude Usage Pattern

Use Claude-oriented workflows for read-heavy tasks that fit Memora's current
boundaries:

- inspect project context assembled from approved artifacts
- review proposed changes against stored constraints and decisions
- prepare draft proposals that a human can review and approve

Avoid describing the current setup as if Claude can:

- write canonical artifacts directly
- bypass approval workflows
- rely on semantic or vector retrieval inside core v1

## Current Limitations

- direct Claude MCP registration is not yet a documented repo capability
- the current companion API is local and intentionally thin
- provider-specific ergonomics remain secondary to Memora's shared contract and governance rules

## Validation

Use the current API validation suite after changes:

```powershell
dotnet test tests/Memora.Api.Tests/Memora.Api.Tests.csproj -p:UseSharedCompilation=false
```

## References

- Anthropic MCP overview: https://docs.anthropic.com/en/docs/mcp
- Anthropic Claude Code MCP guide: https://docs.anthropic.com/en/docs/claude-code/mcp
