# Milestones

## Milestone 1 - Memory Core

Goal: build the durable core that defines and protects Memora's project truth.

Includes:

- repository scaffold and solution structure
- artifact base schema and enums
- Markdown plus frontmatter parsing
- artifact-specific validation
- lifecycle transition validation
- multi-project workspace model
- canonical and draft filesystem storage
- typed relationship parsing
- SQLite index schema
- rebuild-from-files indexing path
- core schema, lifecycle, parsing, and rebuild tests

Outcome: Memora can store, validate, version, and index structured project
artifacts using filesystem truth and a rebuildable SQLite index.

## Milestone 2 - Human Loop

Goal: turn planning input into structured, reviewable memory through an
approval workflow.

Includes:

- planning intake model
- draft artifact generation from planning input
- session summary generation
- approval queue model
- editable draft flow
- approve and reject workflow
- revision diff model
- minimal local operator UI
- tests and validation for planning-to-draft and approval flow

Outcome: a human can import planning material, review generated drafts, edit
them, and approve canonical memory safely.

## Milestone 3 - Agent Loop

Goal: allow agents to consume grounded context and return structured proposals
without bypassing governance.

Includes:

- deterministic context package builder
- deterministic ranking engine
- agent interaction contract
- OpenAPI artifact and context endpoints
- MCP server surface
- proposal submission path
- outcome recording path
- context viewer in the local UI
- tests for context assembly and proposal-only flow

Outcome: agents can request grounded context, submit proposals and outcomes,
and operate against Memora without direct canonical writes.

## Milestone 4 - Integration Expansion

Goal: expand Memora's provider-facing compatibility while preserving the same
core contract.

Includes:

- hardened MCP tools and resources contracts
- OpenWebUI-compatible OpenAPI usage path
- Claude-oriented integration shell and config guidance
- Codex-oriented integration shell and config guidance
- provider-facing integration strategy documentation
- integration-level validation for MCP and OpenAPI surfaces

Outcome: Memora becomes easier to connect to common agent ecosystems without
changing core truth, lifecycle, or storage rules.

## Milestone 5 - Workflow Hardening

Goal: improve day-to-day usability, trust, and operator confidence.

Includes:

- approval UX improvements
- clearer revision diff handling
- stronger validation error reporting
- rebuild and consistency diagnostics
- workflow-focused operator guidance
- end-to-end human-loop test expansion

Outcome: Memora becomes smoother to operate and easier to trust during regular
use.

## Milestone 6 - Controlled Automation

Goal: introduce carefully bounded automation without weakening governance.

Includes:

- definition of low-risk artifact classes for future direct-write
- policy model for controlled automation
- safer event handling for automation triggers
- selective direct-write prototype behind guardrails
- safety validation for policy-governed writes

Outcome: Memora begins moving from proposal-only interaction toward selective
trusted automation.

## Milestone 7 - Advanced Retrieval Evolution

Goal: improve retrieval depth and efficiency without changing the canonical
truth model.

Includes:

- deterministic retrieval optimization
- cached context package support
- expanded relationship traversal
- optional semantic retrieval extension point design
- retrieval evolution documentation

Outcome: Memora retrieval becomes faster and richer while keeping deterministic
project truth intact.

## Milestone 8 - Machina Alignment

Goal: make Memora a stable cognition and governance layer for future runtime
integration.

Includes:

- external runtime contract definition
- Machina-to-Memora interaction model
- runtime-facing context and proposal prototype
- shared contract compatibility validation across runtimes

Outcome: Memora is ready to serve as a memory and governance substrate for
Machina and other runtimes.

## Roadmap Bands

### Band 1 - Core Product Build

- Milestone 1 - Memory Core
- Milestone 2 - Human Loop
- Milestone 3 - Agent Loop

### Band 2 - Usability And Ecosystem Fit

- Milestone 4 - Integration Expansion
- Milestone 5 - Workflow Hardening

### Band 3 - Automation And Runtime Evolution

- Milestone 6 - Controlled Automation
- Milestone 7 - Advanced Retrieval Evolution
- Milestone 8 - Machina Alignment

## Guidance

Milestones 1 through 3 define the real v1 product build.

Later milestones are progression paths, not claims of current capability.

Memora must remain filesystem-first, approval-governed, and deterministic at
its core even as integrations and automation expand.
