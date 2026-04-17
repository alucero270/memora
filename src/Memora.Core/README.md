# Memora.Core

## Purpose
Defines Memora's core domain model and rules.

## Responsibilities
- artifact schemas
- lifecycle rules
- validation primitives

## Does NOT contain
- storage logic
- API logic
- MCP logic
- indexing
- UI logic

## Key Areas

- `Artifacts/`: typed artifact models and enums
- `Validation/`: frontmatter, body, id, timestamp, and lifecycle validation
- `Approval/`: approval queue and workflow rules
- `Editing/`: draft-edit behavior
- `Revisions/`: field-level revision diffs
- `AgentInteraction/`: shared contracts used by API and MCP
