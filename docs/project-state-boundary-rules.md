# Project State View Boundary And Size Rules

## Purpose

This document defines the current state-view boundary for deterministic
project-state output and the size rules that external agents should assume.

It is a design and interpretation document for M9. It does not add new
enforcement beyond the limits already present in the current contract and
builder.

## Boundary Principle

The project-state view is intentionally bounded context, not a
dump-everything export of the workspace.

The state view should:

- ground an external agent in current project state
- stay explainable from the request and stored artifacts
- stay small enough to reason about as an agent-facing payload

The state view should not:

- attempt to return every artifact in a workspace by default
- include unlimited historical material
- behave like broad search or an unrestricted archive export

## Current Inclusion Boundary

The current deterministic boundary is request-scoped and layer-scoped.

### Layer 1

Layer 1 is a bounded anchor layer.

Current behavior:

- at most one charter
- at most one active plan
- at most one repo structure artifact

If one of those anchors is absent, it is omitted rather than replaced with a
different artifact type.

### Layer 2

Layer 2 is a bounded supporting layer.

Current behavior:

- artifact types are limited to decisions, constraints, questions, and outcomes
- default limit is `maxLayer2Artifacts = 10`
- the request must provide a positive limit

Layer 2 should be read as "most relevant supporting context within the
configured cap," not as an exhaustive artifact listing.

### Layer 3

Layer 3 is an optional bounded history layer.

Current behavior:

- it is excluded unless `includeLayer3History = true`
- it is limited to supporting history such as session summaries and inactive
  plans
- default limit is `maxLayer3Artifacts = 10`

Layer 3 is intentionally opt-in because it is supporting history, not default
grounding truth.

## Status Boundary

The current state-view boundary is also lifecycle-aware.

Current behavior:

- approved artifacts are included by default
- draft and proposed artifacts are excluded unless
  `includeDraftArtifacts = true`
- session summaries appear only through the optional history path

That means the default state view is bounded not only by count, but also by
canonical status.

## Relationship Boundary

The current relationship boundary is explicit and bounded.

Current behavior:

- only stored explicit relationships are used
- traversal is bounded to depth 2
- traversal is used to justify inclusion, not to create an unbounded graph walk

This prevents the state view from expanding into uncontrolled graph exploration.

## Body Versus Summary Guidance

The current serialized state view carries full selected artifact bodies and
structured sections for the artifacts that make it into the bundle.

Agents should interpret that as:

- full body for selected artifacts
- not full body for every artifact in the workspace

Design guidance for future enforcement:

- preserve full body for the smallest set of grounding artifacts that agents
  actually need
- prefer bounded artifact selection before body truncation
- if later truncation becomes necessary, truncate in a way that is explicit and
  machine-visible rather than silently dropping meaning

## Ordering Rules

The state view is intended to have stable ordering for identical inputs.

Current ordering rules:

- layers are ordered `Layer1`, `Layer2`, `Layer3`
- Layer 1 anchor order is charter, active plan, repo structure
- Layer 2 and Layer 3 artifact order follows deterministic ranking and stable
  tie-breaks
- normalized state-view serialization sorts order-sensitive collections such as
  tags, relationship entries, and section keys

Ordering is part of determinism, not just presentation.

## Future Truncation Policy

Future size enforcement should start from these rules:

1. preserve Layer 1 anchors first
2. keep canonical approved artifacts ahead of non-canonical or historical
   material
3. prefer reducing lower-priority or optional layers before truncating anchor
   content
4. make any truncation explicit in the state-view shape rather than invisible
5. do not invent semantic or probabilistic fallback behavior to hide size
   limits

## What This Means For External Agents

External agents should assume:

- the state view is intentionally bounded
- absence from the state view does not mean an artifact does not exist
- Layer 1 plus relevant Layer 2 is the primary grounding path
- Layer 3 is optional supporting history
- future enforcement may tighten size, but it should preserve the same explicit
  governance and determinism model

## Summary

The deterministic project-state view is a bounded, explainable working
set. It is not an unbounded workspace export, and future size controls should
preserve the same canonical-first, deterministic retrieval behavior.
