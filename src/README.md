# src

## Purpose
Contains all Memora product code.

## Layout
- Memora.Core
- Memora.Storage
- Memora.Index
- Memora.Context
- Memora.Api
- Memora.Mcp
- Memora.Ui

## Rule
Keep module boundaries strict. Do not duplicate domain rules across projects.

## Suggested Reading Order

1. `Memora.Core`
2. `Memora.Storage`
3. `Memora.Index`
4. `Memora.Context`
5. `Memora.Api`
6. `Memora.Mcp`
7. `Memora.Ui`

## Entry Points

- `Memora.Core`: domain rules, lifecycle, validation diagnostics, approval queue, diffs, controlled automation policy and safety models, shared agent contracts, and the external runtime contract definition
- `Memora.Storage`: parsing, markdown writing, file persistence, and workspace discovery
- `Memora.Index`: SQLite schema plus rebuild logic and diagnostics from filesystem truth
- `Memora.Context`: ranking, inclusion reasoning, derived context package caching, bounded relationship traversal, and layered context bundle assembly
- `Memora.Api`: minimal HTTP host over the shared agent interaction service, plus the guarded file-backed session-summary write prototype and runtime-facing OpenAPI prototype coverage
- `Memora.Mcp`: thin MCP adapter surface over the same shared contract, with compatibility validation against the companion API path
- `Memora.Ui`: styled operator shell, review workflow views, context viewer, and understanding outputs
