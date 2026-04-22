# Retrieval Evolution

This document describes the retrieval state in the Milestone 7 stack and the
bounded path for future retrieval work.

Memora retrieval remains deterministic, explainable, filesystem-first, and
approval-governed. Cached packages, relationship traversal, and optional future
extensions are all subordinate to approved artifact truth.

## Core V1 Behavior

Core v1 retrieval includes:

- deterministic context ranking with explicit priority factors and stable
  tie-breakers
- inclusion reasoning attached to selected context artifacts
- layered context bundle assembly
- derived in-memory context package reuse when the request shape and loaded
  artifact fingerprints are unchanged
- bounded typed relationship traversal for focus proximity

The retrieval path does not use semantic search, vector search, probabilistic
ranking, or external broad-search responsibilities.

## Cache Boundary

Context package caching is derived convenience only:

- filesystem artifacts are loaded and parsed before cache lookup
- cache keys include normalized request values and loaded artifact fingerprints
- changed artifact content, metadata, lifecycle status, links, or request shape
  produces a cache miss
- cached bundles preserve the same output shape and inclusion reasoning as an
  uncached build

Cached packages are never canonical truth and do not replace the filesystem or
the rebuildable SQLite index.

## Relationship Traversal Boundary

Relationship traversal remains explicit and lifecycle-aware:

- traversal operates on candidates that have already passed context visibility
  rules
- stored relationship kinds and traversal direction are preserved on ranked
  artifacts
- focus proximity uses bounded traversal instead of fuzzy graph exploration
- inclusion reasons explain direct and traversed focus connections

Traversal improves retrieval depth while keeping results reproducible for
identical inputs.

## Optional Future Extension Boundary

The optional retrieval extension contract is a boundary for future candidate
discovery providers. It is not active core behavior.

Extension descriptors must remain:

- disabled by default
- unable to provide canonical truth
- advisory rather than authoritative
- separate from the deterministic core ranking and lifecycle model

An external provider may suggest candidate artifact ids with explanations in a
future milestone. Memora core must still validate any candidate against
filesystem truth, lifecycle visibility, and deterministic ranking rules before
including it in context.

## Deferred Work

The following remain outside core v1:

- semantic retrieval implementation
- vector database storage
- probabilistic ranking
- Strata-style broad search
- treating retrieval output as canonical truth

Future work can explore these areas only through explicit optional boundaries
that preserve Memora's governance model.
