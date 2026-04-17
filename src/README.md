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

- `Memora.Core`: domain rules, lifecycle, validation, approval queue, diffs, and shared agent contracts
- `Memora.Storage`: parsing, markdown writing, file persistence, and workspace discovery
- `Memora.Index`: SQLite schema plus rebuild logic from filesystem truth
- `Memora.Context`: ranking, inclusion reasoning, and layered context bundle assembly
- `Memora.Api`: minimal HTTP host over the shared agent interaction service
- `Memora.Mcp`: thin MCP adapter surface over the same shared contract
- `Memora.Ui`: styled operator shell plus context viewer
