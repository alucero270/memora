# Memora.Context

## Purpose
Builds deterministic context packages for agents and workflows.

## Responsibilities
- layered retrieval
- deterministic ranking
- context assembly

## Does NOT contain
- storage or file parsing
- API controllers

## Key Areas

- `Models/ContextBundleModels.cs`: bundle, layer, and selection models
- `Ranking/DeterministicContextRankingEngine.cs`: stable ranking behavior
- `Reasoning/ContextInclusionReasoner.cs`: explicit inclusion reasons
- `Assembly/ContextBundleBuilder.cs`: layered bundle assembly

## Current Scope

- deterministic context assembly is implemented
- ranking remains explainable and non-semantic
- this project can be used by API, MCP, and UI without duplicating selection logic
