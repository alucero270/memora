# Understanding Outputs

## Purpose
Describe the initial human-readable understanding output strategy for Memora's
read-oriented surfaces.

## Current Slice

Memora currently exposes a local UI understanding page at `/understanding`
through `Memora.Ui`.

That page produces three read-only outputs from current project files:

- a context view built from the shared `ContextBundleBuilder`
- a traceability view built from a rebuildable SQLite index plus
  `TraceabilityQueryService`
- a component summary grounded in approved artifacts, current context
  placement, and approved relationships

## Grounding Rules

- filesystem artifacts remain the canonical source of truth
- SQLite remains a derived, rebuildable read model
- context selection still comes from the shared deterministic context assembly
- traceability output still comes from the current approved relationship index
- the UI does not introduce provider-specific reporting or new retrieval logic

## Non-Goals In This Milestone

- no new canonical artifact types
- no semantic or vector summarization
- no write automation
- no provider-specific output formats
- no separate understanding selection engine

## Validation

`tests/Memora.Ui.Tests/UnderstandingOutputTests.cs` exercises the UI route with
representative approved artifacts and validates that context, traceability, and
component output sections render from grounded project data.
