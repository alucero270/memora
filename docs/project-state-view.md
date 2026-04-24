# Project State View

## Purpose

This document defines the canonical serialized project-state view that
Memora already produces for external runtimes today.

It does not introduce a new project-state engine or a parallel domain model.
The state view is the serialized `bundle` returned by the existing shared
`GetContextResponse` contract.

`Projection` is older technical shorthand for this concept. In the docs, the
preferred term is `deterministic state view` because it more clearly describes
what the runtime actually receives.

Primary code paths:

- `src/Memora.Api/Services/FileSystemAgentInteractionService.cs`
- `src/Memora.Context/Assembly/ContextBundleBuilder.cs`
- `src/Memora.Context/Ranking/DeterministicContextRankingEngine.cs`
- `src/Memora.Context/Reasoning/ContextInclusionReasoner.cs`
- `src/Memora.Core/AgentInteraction/AgentInteractionContract.cs`

## Canonical State View Boundary

The runtime-facing serialized state view is:

- `GetContextResponse.bundle`

It is assembled in two stages:

1. `ContextBundleBuilder` produces the internal deterministic bundle in
   `Memora.Context`.
2. `FileSystemAgentInteractionService.MapBundle(...)` maps that internal bundle
   into the shared runtime-facing `AgentContextBundle`.

The public state view is therefore the existing shared contract shape, not the
internal `Memora.Context` model.

## Serialized Shape

```text
GetContextResponse
  bundle
    request
    layers[]
      kind
      artifacts[]
        artifact
        inclusionReasons[]
```

### `GetContextResponse`

| Field | Source | Notes |
| --- | --- | --- |
| `bundle` | `GetContextResponse.Bundle` | Present on success. This is the serialized project-state view. |
| `errors` | `AgentInteractionResponse.Errors` | Validation or lookup failures. Not part of successful project-state content. |

### `bundle.request`

`bundle.request` is the normalized `GetContextRequest` echoed in the response.

| Field | Source | Notes |
| --- | --- | --- |
| `projectId` | `GetContextRequest.ProjectId` | Required project identity. |
| `taskDescription` | `GetContextRequest.TaskDescription` | Required retrieval intent. |
| `includeDraftArtifacts` | `GetContextRequest.IncludeDraftArtifacts` | Enables draft and proposed artifacts in the state view. |
| `includeLayer3History` | `GetContextRequest.IncludeLayer3History` | Enables Layer 3 supporting history. |
| `focusArtifactIds` | `GetContextRequest.FocusArtifactIds` | Normalized, distinct, ordinal-sorted. |
| `focusTags` | `GetContextRequest.FocusTags` | Normalized, distinct, ordinal-sorted. |
| `maxLayer2Artifacts` | `GetContextRequest.MaxLayer2Artifacts` | Layer 2 cap. |
| `maxLayer3Artifacts` | `GetContextRequest.MaxLayer3Artifacts` | Layer 3 cap. |

Normalization is implemented by:

- `AgentInteractionContractHelpers.NormalizeValues(...)`
- `ContextBundleRequest.NormalizeValues(...)`

### `bundle.layers[]`

Each layer is an `AgentContextLayer` mapped from a `ContextBundleLayer`.

| Field | Source | Notes |
| --- | --- | --- |
| `kind` | `ContextLayerKind -> AgentContextLayerKind` | Serialized as `Layer1`, `Layer2`, or `Layer3`. |
| `artifacts` | `ContextBundleLayer.Artifacts` | Ordered state-view entries for that layer. |

Layer meaning comes from the current deterministic builder:

- `Layer1`: charter, active plan, and repo snapshot anchors when present
- `Layer2`: approved or explicitly allowed supporting artifacts
- `Layer3`: optional supporting history such as session summaries and inactive
  plans

Selection behavior is defined in:

- `ContextBundleBuilder.Build(...)`
- `ContextBundleBuilder.SelectLayer1Anchors(...)`
- `ContextBundleBuilder.SelectTopRanked(...)`

### `bundle.layers[].artifacts[]`

Each entry is an `AgentContextArtifact`.

| Field | Source | Notes |
| --- | --- | --- |
| `artifact` | `ContextBundleArtifact.Artifact` | The artifact document payload. |
| `inclusionReasons` | `ContextBundleArtifact.InclusionReasons` | Explainable inclusion justification. |

The serialized state view does not expose a separate `origin` field.
Current external callers derive canonical versus non-canonical meaning from the
artifact itself and the inclusion reasons:

- approved artifacts are canonical truth candidates in context
- draft and proposed artifacts are included only when explicitly requested
- session summaries are supporting non-canonical history

### `artifact`

`artifact` is the serialized `ArtifactDocument` contract instance, including
its type-specific fields.

Common fields:

| Field | Source |
| --- | --- |
| `id` | `ArtifactDocument.Id` |
| `projectId` | `ArtifactDocument.ProjectId` |
| `type` | `ArtifactDocument.Type` |
| `status` | `ArtifactDocument.Status` |
| `title` | `ArtifactDocument.Title` |
| `createdAtUtc` | `ArtifactDocument.CreatedAtUtc` |
| `updatedAtUtc` | `ArtifactDocument.UpdatedAtUtc` |
| `revision` | `ArtifactDocument.Revision` |
| `tags` | `ArtifactDocument.Tags` |
| `provenance` | `ArtifactDocument.Provenance` |
| `reason` | `ArtifactDocument.Reason` |
| `links` | `ArtifactDocument.Links` |
| `body` | `ArtifactDocument.Body` |
| `sections` | `ArtifactDocument.Sections` |

Relationship payload:

| Field | Source | Notes |
| --- | --- | --- |
| `links.relationships[]` | `ArtifactLinks.Relationships` | Explicit stored relationships only. |
| `links.relationships[].kind` | `ArtifactRelationship.Kind` | One of the typed relationship kinds. |
| `links.relationships[].targetArtifactId` | `ArtifactRelationship.TargetArtifactId` | Related artifact id. |

Type-specific serialized fields already carried by the current contract:

| Artifact type | Additional fields |
| --- | --- |
| `plan` | `priority`, `active` |
| `decision` | `decisionDate` |
| `constraint` | `constraintKind`, `severity` |
| `question` | `questionStatus`, `priority` |
| `outcome` | `outcome` |
| `repo_structure` | `snapshotSource` |
| `session_summary` | `sessionType`, `canonical` |

### `inclusionReasons[]`

Each reason is an `AgentContextInclusionReason` mapped from
`ContextInclusionReason`.

| Field | Source | Notes |
| --- | --- | --- |
| `code` | `ContextInclusionReason.Code` | Stable reason identifier. |
| `description` | `ContextInclusionReason.Description` | Human-readable explanation. |
| `relatedArtifactIds` | `ContextInclusionReason.RelatedArtifactIds` | Normalized, distinct, ordinal-sorted ids. |

Reason generation is defined in
`ContextInclusionReasoner.ExplainInclusion(...)`.

Current reason families include:

- default approved grounding
- explicit draft allowance
- non-canonical supporting history
- Layer 1 and Layer 3 anchor inclusion
- explicit focus artifact selection
- direct relationship to focus artifacts
- bounded traversal connection to focus artifacts
- milestone relevance
- direct task-term matching

## What The State View Includes

The current serialized state view already includes these project-state facts
when they are derivable from loaded artifacts and the deterministic retrieval
path:

- selected artifact documents
- lifecycle state through `artifact.status`
- explicit typed relationships through `artifact.links.relationships`
- revision identity through `artifact.id` and `artifact.revision`
- request-scoped retrieval intent through `bundle.request`
- explainable inclusion reasoning through `inclusionReasons`

## What The State View Does Not Include

The current serialized state view intentionally does not publish separate
internal-only fields such as:

- cache keys or cache-hit status from `ContextPackageCache`
- ranking breakdown scores from `DeterministicContextRankingEngine`
- raw traversal path segments from relationship traversal
- an additional top-level `project_state` or `get_project_state` contract
- a second canonical/projected truth model outside the existing context bundle

Those details may influence selection, but they are not part of the current
runtime-facing serialized state view.

## Contract Fit

The current state view fits the existing shared runtime contract shapes.

- read operation: `get_context`
- request contract: `GetContextRequest`
- response contract: `GetContextResponse`
- serialized state-view payload: `GetContextResponse.bundle`

No additional top-level project-state operation is needed for M9 because the
existing contract already carries the stabilized deterministic state view.

## Mapping Summary

The project-state view for external runtimes is the existing shared
context contract:

- internal deterministic source: `ContextBundle`
- runtime-facing serialized state view: `AgentContextBundle`
- transport wrapper: `GetContextResponse`

That keeps Memora on a single deterministic retrieval path and avoids a
parallel project-state model.
