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

- `Memora.Core`: domain rules, lifecycle, validation diagnostics, approval queue, diffs, controlled automation policy and safety models, and shared agent contracts
- `Memora.Storage`: parsing, markdown writing, file persistence, and workspace discovery
- `Memora.Index`: SQLite schema plus rebuild logic and diagnostics from filesystem truth
- `Memora.Context`: deterministic ranking, cached context packages, bounded relationship traversal, inclusion reasoning, layered context bundle assembly, and optional future retrieval extension boundaries
- `Memora.Api`: minimal HTTP host over the shared agent interaction service, plus the guarded file-backed session-summary write prototype
- `Memora.Mcp`: thin MCP adapter surface over the same shared contract
- `Memora.Ui`: styled operator shell, review workflow views, context viewer, and understanding outputs
