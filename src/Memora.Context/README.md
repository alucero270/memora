# Memora.Context

## Purpose
Builds deterministic context packages for agents and workflows.

## Responsibilities
- layered retrieval
- deterministic ranking
- bounded typed relationship traversal for focus proximity
- context assembly

## Does NOT contain
- storage or file parsing
- API controllers

## Key Areas

- `Models/ContextBundleModels.cs`: bundle, layer, and selection models
- `Ranking/DeterministicContextRankingEngine.cs`: stable ranking behavior
- `Reasoning/ContextInclusionReasoner.cs`: explicit inclusion reasons
- `Assembly/ContextBundleBuilder.cs`: layered bundle assembly
- `Assembly/ContextPackageCache.cs`: derived in-memory context package reuse keyed by request and artifact fingerprints
- `Extensions/OptionalRetrievalExtension.cs`: disabled-by-default boundary contracts for future external retrieval candidate providers

## Current Scope

- deterministic context assembly is implemented
- repeated package assembly can reuse cached derived bundles when loaded artifact state is unchanged
- relationship proximity can traverse explicit stored relationship paths up to a bounded depth
- optional external retrieval extension contracts exist for future candidate discovery, but they are not wired into core v1 behavior
- ranking remains explainable and non-semantic
- this project can be used by API, MCP, and UI without duplicating selection logic
