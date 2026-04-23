# Retrieval Evolution

This document explains how retrieval evolves in Memora without changing the
core truth model.

It is intentionally narrower than the general roadmap. Use
`current-state.md` for implemented behavior in the current checkout and
`milestones.md` for sequencing. This file exists to make the retrieval
boundary explicit so future work does not blur shipped deterministic behavior
with deferred exploration.

## Retrieval Rules That Do Not Change

Memora retrieval remains governed by the same core rules:

- filesystem-backed approved artifacts are canonical truth
- SQLite is a derived index and can be rebuilt from files
- retrieval in core v1 is deterministic and explainable
- agents do not write canonical truth directly
- Memora is not a broad search system or execution runtime

These rules apply even when retrieval becomes faster or can inspect more of the
approved relationship graph.

## Shipped Retrieval Behavior

The current retrieval path in `Memora.Context` includes:

- deterministic ranking with stable ordering
- explicit inclusion reasoning for selected artifacts
- layered context bundle assembly
- derived context package caching keyed by request shape and loaded artifact
  fingerprints
- bounded typed relationship traversal for focus proximity

These improvements deepen retrieval and reduce repeated work, but they do not
change what counts as truth or how inclusion is justified.

## What Caching Changes

Cached context packages are a derived convenience only.

- cache keys are fingerprinted from normalized request values and the loaded
  artifact set
- changes to artifact content, metadata, lifecycle state, relationships,
  sections, or relevant request inputs produce a different cache key
- cached bundles preserve the same context shape and reasoning as an uncached
  build

Caching improves efficiency. It does not create a second source of truth.

## What Relationship Traversal Changes

Relationship traversal makes deterministic retrieval richer without becoming
fuzzy.

- traversal follows explicit stored relationships
- traversal stays bounded and lifecycle-aware
- direct and traversed relationship paths are available for inclusion reasoning
- focus proximity is still reproducible for the same inputs

Traversal adds grounded depth. It is not graph exploration for its own sake,
and it does not replace deterministic ranking rules.

## What Stays Out Of Core V1

The following remain out of scope for core Memora retrieval:

- semantic retrieval
- vector storage or vector search
- probabilistic ranking
- treating retrieval results as canonical truth
- Strata-style broad search inside Memora core

This boundary matters more than the specific retrieval technique. Memora must
preserve what the system knows, not what the system guesses.

## Future Exploration Boundary

Future retrieval exploration must stay outside core v1 unless it preserves the
same governance model.

That means any later advisory discovery layer would need to remain:

- non-canonical
- clearly separate from deterministic core retrieval
- unable to bypass lifecycle or approval rules
- validated back through approved artifacts and normal context assembly before
  it affects grounded Memora output

If semantic or broad retrieval is explored later, it should be treated as an
external advisory concern rather than as a replacement for Memora's core
deterministic retrieval path.

## Practical Reading

For the current implementation and adjacent boundaries, read these together:

- `docs/current-state.md`
- `docs/architecture.md`
- `docs/integration-strategy.md`
- `src/Memora.Context/README.md`

## Summary

Retrieval evolution in Memora means making deterministic retrieval faster,
richer, and easier to explain. It does not mean shifting Memora core toward
semantic search, vector indexing, or probabilistic project memory.
