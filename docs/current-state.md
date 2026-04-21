# Current State

This document describes the implemented behavior in the current checkout.
It is intentionally separate from roadmap and milestone planning docs.

## Implemented Now

### Foundation

- typed artifact models, enums, and lifecycle rules in `Memora.Core`
- artifact validation, diagnostic formatting, planning intake, draft editing, approval queue building, and revision diffs
- revision diffs include deterministic change areas, display labels, and raw paths
- markdown plus frontmatter parsing in `Memora.Storage`
- filesystem persistence for canonical, draft, and summary artifacts
- workspace discovery through `project.json`
- SQLite schema plus rebuild-from-files indexing in `Memora.Index`
- rebuild diagnostics distinguish filesystem truth from derived SQLite index state

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
- approval queue navigation, revision review previews, and decision-readiness context
- context viewer page backed by the shared context builder
- understanding output page with context, traceability, and component views

### Operator Guidance

- workflow-focused operator guide in `docs/operator-workflows.md`
- operations doc that points operators to current review and recovery workflows

## Still Intentionally Thin

- UI review is preview-oriented and does not persist approval or rejection decisions
- API is a minimal HTTP surface, not a fully documented production service
- MCP is currently an in-process adapter surface, not a complete hosted server transport
- context assembly is deterministic and explainable, but remains non-semantic and non-vector in v1
- rebuild diagnostics identify filesystem issues, but they do not auto-repair artifacts or indexes

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
- `src/Memora.Index/Rebuild/IndexRebuildResult.cs`

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
- `src/Memora.Ui/Understanding/FileSystemUnderstandingOutputService.cs`

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
