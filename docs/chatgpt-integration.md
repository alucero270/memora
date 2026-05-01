# ChatGPT-Oriented Read-Only Integration Guidance

## Purpose

This guide explains the current honest Memora path for ChatGPT-oriented
workflows in Milestone 10.

It is intentionally read-only. The goal is to validate that ChatGPT-oriented
tooling can consume the same deterministic project state through the shared
Memora contract without adding mutation-specific behavior yet.

## Current Fit

Memora's current repo state already provides the shared read boundary that a
ChatGPT-oriented workflow needs:

- deterministic context retrieval through the shared runtime-facing contract
- a local companion OpenAPI surface at `http://127.0.0.1:5081`
- a published OpenAPI document at `http://127.0.0.1:5081/openapi.json`

What the repo does not yet provide:

- a ChatGPT-specific adapter inside Memora
- a documented ChatGPT write workflow for proposals or outcomes
- a hosted Memora MCP transport for direct registration

That means the current M10 validation target is read-only:

- project lookup
- deterministic context retrieval
- interpretation of the returned state view

## Shared Contract Boundary

ChatGPT-oriented workflows should stay within the same shared boundary used by
the rest of M10:

- `GET /api/projects/{projectId}`
- `POST /api/context`

Those routes reuse the same shared Memora-facing contract that also grounds the
MCP surface.

For read-only validation, use:

- `ProjectLookupResponse`
- `GetContextRequest`
- `GetContextResponse`

Do not document proposal or outcome submission as part of the ChatGPT path in
M10. That write behavior remains outside this milestone's ChatGPT slice.

## Recommended Current Workflow

### 1. Run Memora Locally

Set a workspace root and start the companion API:

```powershell
$env:MEMORA_WORKSPACES_ROOT = "C:\\path\\to\\memora-workspaces"
dotnet run --project src/Memora.Api
```

### 2. Verify The Published OpenAPI Document

```powershell
curl http://127.0.0.1:5081/openapi.json
```

### 3. Resolve The Project And Request Context

The current ChatGPT-oriented validation path is:

1. resolve the target project
2. request deterministic context for the task
3. interpret the returned state view as read-only grounded context

That keeps the workflow aligned with:

- filesystem-backed canonical truth
- deterministic state-view retrieval
- no direct canonical writes
- future Claude compatibility through the same shared read shape

## Example Read-Only Validation Flow

Run the included sample script:

```powershell
./samples/workflows/chatgpt-read-only-context.ps1 `
  -ProjectId "demo-project" `
  -TaskDescription "Validate the shared read-only state view for ChatGPT-oriented workflows."
```

That workflow performs:

- project lookup
- deterministic context retrieval
- artifact id summary output

without proposal or outcome mutation behavior.

## Current Limits To Keep Explicit

- this is a read-only validation path in M10
- the current repo does not document ChatGPT proposal submission or outcome recording
- the companion OpenAPI host is the practical current path because the repo does not yet ship a hosted Memora MCP transport
- the returned state view must still be interpreted using `docs/agent-project-state-interpretation.md`

## Validation

Use the existing runtime-facing compatibility coverage alongside the read-only
sample run when this guidance changes:

```powershell
dotnet test tests/Memora.Api.Tests/Memora.Api.Tests.csproj --no-restore --filter "FullyQualifiedName~RuntimeContractCompatibilityTests"
```
