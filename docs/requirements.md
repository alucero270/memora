# Requirements

## 1. Product Requirements

- Memora must function as a local-first system
- Memora must preserve canonical project state as structured artifacts
- Memora must remain useful without cloud dependency
- Memora must support approval-governed updates to canonical memory
- Memora must expose its capabilities through MCP-compatible integrations
- Memora must support an OpenAPI-compatible companion surface

## 2. Functional Requirements

- store canonical project knowledge as structured artifacts
- provide relevant project state to agents before execution
- promote planning outputs into structured artifacts
- require approval before AI-generated updates become canonical memory
- track artifact revisions, supersession, and deprecation
- represent repository structure as part of project state
- record outcomes including success, failure, and reasoning
- support lifecycle states for artifacts
- support typed relationships between artifacts
- provide a rebuildable SQLite-backed local index
- support project-scoped workspaces
- support draft and proposal handling separate from canonical memory
- support event-triggered memory workflows for planning, execution, and approval

## 3. Non-Functional Requirements

- local-first operation
- human-readable and editable artifact files
- deterministic and explainable retrieval in v1
- filesystem as source of truth
- SQLite as a derived index only
- provider-agnostic integration posture
- solo-developer maintainability
- cross-platform by design
- no semantic or vector retrieval dependency in core v1
- high-signal retention without unbounded memory growth
- explainability through provenance and reasoning-aware artifacts

## 4. Lifecycle Requirements

Artifacts must support:

- `proposed`
- `draft`
- `approved`
- `superseded`
- `deprecated`

Rules:

- AI creates proposed artifacts only
- drafts are editable
- approval is required for canonical state
- superseded artifacts must remain traceable
- invalid lifecycle transitions must be rejected

## 5. Storage Requirements

- canonical memory must be file-backed
- artifacts must be stored as Markdown with strict frontmatter
- SQLite must be rebuildable from canonical files
- external editing may be allowed, but only validated content is accepted
- filesystem is the authoritative source of truth

## 6. Retrieval Requirements

- retrieval must use layered context

Layer 1, always included when available:

- Project Charter
- active plan
- repo structure

Layer 2:

- decisions
- constraints
- questions
- outcomes

Layer 3:

- historical artifacts
- extended relationships

Rules:

- Layer 1 must always be included
- ranking must be deterministic in v1
- retrieval must be explainable
- retrieval must not define truth; it only selects from structured memory

## 7. Integration Requirements

- MCP is the primary provider-facing integration layer
- OpenAPI is the companion integration surface
- integration layers must not bypass lifecycle or approval rules
- agents may only propose changes in v1
- canonical writes require explicit approval

## 8. Boundary Requirements

- Memora is not Machina
- Memora is not Strata
- Strata may later provide supporting retrieval, but not canonical truth
- external retrieval results are not authoritative unless promoted and approved

## 9. Artifact Model Requirements

Canonical artifact types:

- Project Charter
- plan or milestone
- architecture decision record
- constraint
- open question
- outcome or lesson
- repo structure snapshot

Supporting artifact:

- session summary, which is non-canonical

## 10. Relationship Requirements

Artifacts must support typed relationships:

- `depends_on`
- `affects`
- `derived_from`
- `supersedes`

Rules:

- relationships must be directional
- relationships must reference valid artifacts
- relationships must be parseable and indexable

## 11. Planning Fidelity Requirements

- planning conversations must be distilled into structured artifacts
- session summaries may be stored as supporting context
- full conversation transcripts must not be treated as canonical memory

## 12. Event Model Requirements

Memora must support event-triggered workflows.

Events include:

- session start
- planning promotion
- decision updates
- task completion
- outcome recording
- approval events

These events drive:

- artifact creation
- lifecycle transitions
- workflow execution

## 13. Assumptions

- planning conversations can be distilled into structured artifacts
- one-click approval is sufficient for promotion
- typed links are sufficient for v1 relationships
- file-backed memory plus index is practical
- agents improve when grounded in structured project state

## 14. Non-Goals

- storing full conversation history as canonical memory
- building a PKM or note-taking system
- using a vector database as source of truth
- a fully autonomous memory system in v1
- sprint or task management
- advanced graph traversal in v1
- a full UI product in v1

## 15. Locked Decisions

- filesystem is the source of truth
- SQLite is a derived index
- typed links are required
- layered retrieval is required
- one-click approval is required
- editable draft state is required
- planning uses structured artifacts plus summaries
- external editors are allowed but not authoritative
