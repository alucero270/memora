# tests

## Purpose
Contains automated tests for Memora modules.

## Testing stance
- core domain rules should be strongly covered
- integration layers must include the smallest meaningful validation
- no feature is complete without appropriate tests or validation

## Test Projects

- `Memora.Core.Tests`: lifecycle, validation, planning, approval queue, and diff behavior
- `Memora.Storage.Tests`: parsing, persistence, and workspace layout behavior
- `Memora.Index.Tests`: SQLite schema and rebuild behavior
- `Memora.Context.Tests`: ranking, inclusion reasoning, and context assembly behavior
- `Memora.Api.Tests`: HTTP contract and file-backed agent interaction behavior
- `Memora.Mcp.Tests`: MCP adapter contract behavior
- `Memora.Ui.Tests`: operator shell and context viewer behavior
