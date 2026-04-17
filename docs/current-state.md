# Current State

This document describes the implemented behavior in the current checkout.
It is intentionally separate from roadmap and milestone planning docs.

## Implemented Now

### Foundation

- typed artifact models, enums, and lifecycle rules in `Memora.Core`
- artifact validation, planning intake, draft editing, approval queue building, and revision diffs
- markdown plus frontmatter parsing in `Memora.Storage`
- filesystem persistence for canonical, draft, and summary artifacts
- workspace discovery through `project.json`
- SQLite schema plus rebuild-from-files indexing in `Memora.Index`

### Context Assembly

- deterministic context bundle models
- deterministic ranking with stable ordering
- explicit inclusion reasoning for selected artifacts
- layered bundle assembly in `Memora.Context`
- context viewer UI route at `/context-viewer`

### Integration Surfaces

- local HTTP endpoints in `Memora.Api` for:
  - project lookup
  - context assembly
  - artifact proposals
  - artifact updates
  - outcome recording
- a thin MCP adapter in `Memora.Mcp` over the shared agent interaction contract

### Operator UI

- styled local operator shell in `Memora.Ui`
- project selection from discovered workspaces
- artifact browsing and draft editing
- approval queue and revision review previews
- context viewer page backed by the shared context builder

## Still Intentionally Thin

- UI review is preview-oriented and does not claim full approval or rejection persistence
- API is a minimal HTTP surface, not a fully documented production service
- MCP is currently an in-process adapter surface, not a complete hosted server transport
- context assembly is deterministic and explainable, but remains non-semantic and non-vector in v1

## Where To Look In Code

### Core Domain

- `src/Memora.Core/Artifacts/ArtifactDocuments.cs`
- `src/Memora.Core/Validation/`
- `src/Memora.Core/Approval/`
- `src/Memora.Core/Revisions/`
- `src/Memora.Core/AgentInteraction/AgentInteractionContract.cs`

### Storage

- `src/Memora.Storage/Parsing/ArtifactMarkdownParser.cs`
- `src/Memora.Storage/Persistence/ArtifactFileStore.cs`
- `src/Memora.Storage/Workspaces/WorkspaceDiscovery.cs`

### Index

- `src/Memora.Index/Schema/SqliteIndexSchema.cs`
- `src/Memora.Index/Rebuild/SqliteIndexRebuilder.cs`

### Context

- `src/Memora.Context/Models/ContextBundleModels.cs`
- `src/Memora.Context/Ranking/DeterministicContextRankingEngine.cs`
- `src/Memora.Context/Reasoning/ContextInclusionReasoner.cs`
- `src/Memora.Context/Assembly/ContextBundleBuilder.cs`

### API

- `src/Memora.Api/Program.cs`
- `src/Memora.Api/Services/FileSystemAgentInteractionService.cs`
- `src/Memora.Api/AgentInteractionHttpResults.cs`

### MCP

- `src/Memora.Mcp/Server/MemoraMcpServer.cs`

### UI

- `src/Memora.Ui/Program.cs`
- `src/Memora.Ui/Operator/LocalOperatorWorkspaceService.cs`
- `src/Memora.Ui/Rendering/OperatorShellPageRenderer.cs`
- `src/Memora.Ui/ContextViewer/FileSystemContextViewerService.cs`

## Local Run Behavior

### UI

- project: `src/Memora.Ui`
- default dev URL: `http://127.0.0.1:5080`
- if no workspace root is configured, it boots from a writable local copy of `samples/workspaces`

### API

- project: `src/Memora.Api`
- default dev URL: `http://127.0.0.1:5081`
- it uses a file-backed agent interaction service only when `MEMORA_WORKSPACES_ROOT` or `Memora:WorkspacesRootPath` is configured

## Guidance

- use `docs/milestones.md` for roadmap intent
- use this file for implemented behavior
- if docs and code disagree, the code wins and the docs should be updated
