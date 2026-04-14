# Scope

## In Scope (v1)

- local-first structured memory for software projects
- file-backed canonical artifacts
- SQLite-derived local index
- multi-project workspace support
- Markdown plus frontmatter artifact storage
- artifact lifecycle management
- approval-governed canonical updates
- deterministic layered retrieval
- typed artifact relationships
- planning intake and draft generation
- human approval workflow
- MCP-first integration surface
- OpenAPI companion surface
- minimal local operator UI
- agent proposal and outcome submission paths

## Out Of Scope (v1)

- semantic or vector retrieval in core
- graph database adoption
- full autonomous agent control
- a full chat client
- multi-user collaboration
- Git integration
- Strata integration
- tight coupling to provider SDKs
- platform-specific adapters beyond the MCP and OpenAPI strategy
- polished enterprise UI or UX

## Assumptions

- planning conversations can be distilled into structured artifacts
- human approval remains acceptable in v1
- typed links are sufficient for v1 relationships
- filesystem plus SQLite is practical for a local-first architecture
- agents perform better when grounded in structured project state

## Constraints

- filesystem is the source of truth
- SQLite is derived and rebuildable
- agents may only propose in v1
- canonical changes require approval
- retrieval must remain deterministic in v1
- Memora must stay distinct from Strata and Machina
- Memora must remain cross-platform by design

## Non-Goals

- storing full conversation history as canonical memory
- becoming a PKM or note-taking platform
- replacing Strata
- replacing an execution runtime
- making AI mandatory for baseline product value
- building advanced orchestration before the core memory model is stable

## Scope Rule

If a feature blurs:

- truth vs retrieval
- proposal vs approval
- memory vs runtime
- Memora vs Strata

then it is likely outside current v1 scope or must be deferred.
