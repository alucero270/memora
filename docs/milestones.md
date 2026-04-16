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

## Milestone 2 - Relationship + Traceability Layer

Goal: persist typed relationships and expose deterministic traceability,
dependency, and impact queries across approved artifacts.

Includes:

- relationship persistence
- relationship query model
- incoming and outgoing relationship queries
- traceability query model
- dependency and impact traceability queries
- tests and validation for relationship and traceability behavior

Outcome: Memora can store and query explicit artifact relationships as durable,
explainable understanding data without taking on broad retrieval or graph
system responsibilities.

## Milestone 3 - Context Assembly Core

Goal: assemble deterministic context bundles from approved artifacts with
explicit inclusion reasoning and stable ordering.

Includes:

- deterministic context bundle builder
- inclusion reasoning and deterministic ranking rules
- context bundle models
- OpenAPI context bundle endpoints
- MCP context assembly surface
- tests and validation for context assembly behavior

Outcome: Memora can build grounded, task-oriented context bundles that explain
why each artifact was included and preserve deterministic understanding-first
behavior.

## Milestone 4 - Understanding Outputs

Goal: produce basic human-readable understanding outputs from context and
traceability data without changing Memora's core responsibilities.

Includes:

- understanding output models
- context views
- traceability views
- component understanding outputs
- output strategy documentation
- validation for understanding outputs

Outcome: Memora can turn approved artifacts, relationships, and context bundles
into clear understanding outputs that remain grounded in canonical project
memory.

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
- Milestone 2 - Relationship + Traceability Layer
- Milestone 3 - Context Assembly Core

### Band 2 - Usability And Ecosystem Fit

- Milestone 4 - Understanding Outputs
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
