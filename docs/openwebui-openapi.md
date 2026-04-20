# Open WebUI OpenAPI Path

## Purpose

This guide shows the smallest honest way to connect Open WebUI to Memora's
current companion OpenAPI surface.

It is intentionally limited to the current repo behavior. Memora remains
MCP-first, approval-governed, and proposal-only for agent writes.

## What Exists Now

- `Memora.Api` listens on `http://127.0.0.1:5081` in the default development profile
- the API publishes an OpenAPI document at `http://127.0.0.1:5081/openapi.json`
- the current API surface includes:
  - project lookup
  - context assembly
  - artifact proposal submission
  - artifact update proposal submission
  - outcome recording

## Recommended Open WebUI Mode

Open WebUI's current OpenAPI tool-server docs describe both user-scoped and
global tool servers.

Memora's current path aligns best with a global tool server:

- Memora is a local companion service, not a browser-first public API
- the current API does not add browser-focused CORS handling
- backend-to-backend registration keeps the flow aligned with the current thin
  local service model

If your Open WebUI deployment only supports a user-scoped browser path, treat
that as outside the current documented Memora flow.

## Memora Setup

1. Set a workspace root so file-backed endpoints can operate:

```powershell
$env:MEMORA_WORKSPACES_ROOT = "C:\\path\\to\\memora-workspaces"
```

2. Start the API host from the repo root:

```powershell
dotnet run --project src/Memora.Api
```

3. Verify the OpenAPI document is reachable:

```powershell
curl http://127.0.0.1:5081/openapi.json
```

The document should list paths such as `/api/context`,
`/api/artifacts/proposals`, and `/api/outcomes`.

## Open WebUI Setup

In Open WebUI, use the OpenAPI tool-server flow documented by the Open WebUI
project and register the Memora API base URL:

- tool server URL: `http://127.0.0.1:5081`
- published OpenAPI document: `http://127.0.0.1:5081/openapi.json`

Use the global/admin-managed tool-server path when available so Open WebUI can
reach the local Memora service from its backend.

## What To Expect

Open WebUI should discover Memora's current companion API operations as tools
that map to the shared agent interaction contract.

Those operations are still bounded by Memora's governance rules:

- context retrieval is deterministic and explainable
- artifact writes remain proposals
- outcome recording creates reviewable non-canonical artifacts
- no Open WebUI flow should be described as a direct canonical write path

## Current Limitations

- this path depends on the current thin local API host, not a hardened remote deployment model
- the API surface is companion-only and does not replace MCP as Memora's primary provider-facing interface
- Open WebUI-specific ergonomics beyond basic registration remain out of scope for this issue

## Validation

Use these repo-local checks after changes:

```powershell
dotnet test tests/Memora.Api.Tests/Memora.Api.Tests.csproj
```
