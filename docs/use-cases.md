# Use Cases

This document captures concrete workflows Memora should support and how those
workflows map to the current repo and demo data.

## Future-Track Exploration Without Polluting Truth

Memora should help a team think ahead without turning speculative ideas into
approved project truth.

The core workflow is:

1. start from current approved artifacts and current repo rules
2. explore future ideas, risks, and possible directions
3. record the approved boundary explicitly
4. preserve deferred questions for later work
5. keep the exploratory conversation separate from canonical truth unless a
   human approves a new artifact

This is a high-value use case for AI-assisted work because product and
architecture discussions often branch into adjacent ideas that matter later but
should not silently expand the current milestone.

### Demo Project Example

The sample workspace at `samples/workspaces/demo-project` models this workflow
with a small artifact set:

- `CHR-001`: the charter that anchors the project
- `ADR-001`: an earlier approved technical decision
- `ADR-002`: an approved boundary that keeps semantic retrieval out of Memora
  core
- `QST-001`: an approved question whose status is `deferred`, capturing future
  ideas without claiming they are current scope
- `SUM-001`: a non-canonical session summary that records the discussion which
  produced the boundary and deferred question

### What The Demo Proves

The demo should make these distinctions visible:

- current approved truth lives in canonical artifacts
- future-track thinking can be preserved without becoming approved design
- rejected or excluded ideas can still leave behind decision rationale
- deferred questions are first-class artifacts, not forgotten chat residue
- session summaries support continuity but do not become canonical truth by
  themselves

## Product Packaging Without Losing The Core

Memora should also preserve product strategy decisions when those decisions are
important enough to shape roadmap boundaries.

The product-packaging workflow follows the same pattern:

1. identify what the MVP must include to remain trustworthy
2. decide which capabilities belong to the free local-first core
3. separate paid convenience and team features from canonical truth behavior
4. preserve deferred pricing or packaging questions for later review

### Demo Project Example

The sample workspace models this with:

- `ADR-003`: the local-first MVP is free
- `CNS-001`: core truth and governance stay in the free tier
- `QST-002`: which paid capabilities should come first remains deferred

This makes the demo more realistic because Memora is not only storing technical
constraints. It is also preserving the product reasoning that prevents future
drift in packaging and scope.

### Why This Matters

Without this workflow, a team often loses important context:

- why an idea was rejected
- whether a topic is closed, deferred, or still open
- which part of a discussion is canonical versus speculative
- which follow-up belongs to a future milestone instead of the current one

Memora is strongest when it preserves what the project knows while still
leaving room for thoughtful future planning.

## Remote Planning Conversations That Can Update Memora

Memora should eventually support a workflow where an operator can hold a
planning conversation from a remote client such as ChatGPT and have that
conversation create reviewable Memora artifacts.

The intended workflow is:

1. resolve the target project remotely
2. retrieve grounded Memora context for the planning topic
3. continue the planning conversation in the remote client
4. submit a reviewable proposal or update back into Memora
5. preserve approval and canonical-truth boundaries

### Why This Is Distinct

This is not just another transport variation of the local external workflow.

It adds requirements around:

- off-machine reachability
- safe remote write paths
- state freshness and revision handling
- operator review after remote conversation-driven writes

### Why This Needs Explicit Planning

This use case is important enough to shape future milestone boundaries because
it can easily pull in:

- hosted or remotely reachable service design
- authentication and client access rules
- write-path limits by artifact type
- review workflow expectations

The product value here is not merely "connect another client." The value is
"make remote planning conversations durable and governable without turning chat
output directly into truth."
