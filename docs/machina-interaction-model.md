# Machina To Memora Interaction Model

## Purpose

This document explains how Machina should interact with Memora without
changing Memora's role as structured project memory and governance.

It is an interaction model, not a runtime-hosting design. Machina executes
runtime work. Memora provides grounded context, accepts reviewable proposals,
and preserves approval-governed project truth.

## Responsibility Split

### Memora Owns

- project memory and artifact truth
- deterministic context assembly
- lifecycle and approval rules
- proposal and outcome intake through shared contracts
- filesystem-backed canonical storage

### Machina Owns

- task execution and runtime orchestration
- tool selection and runtime-specific control flow
- interpreting retrieved Memora context for the current task
- deciding when to submit a proposal or outcome back to Memora

### Neither Side Should Do

- treat runtime output as canonical truth before approval
- move provider-specific business logic into Memora core
- turn Memora into a runtime host
- collapse Strata-style broad retrieval into Memora core

## Preferred Integration Path

Machina should prefer Memora's primary MCP surface when the runtime can consume
provider-facing tools and resources directly.

Machina may use the OpenAPI companion surface when HTTP is the practical local
transport boundary.

In both cases, the interaction model stays the same because both surfaces reuse
the shared agent interaction contract.

## Shared Contract Alignment

Machina should align to the contract defined in:

- `docs/external-runtime-contract.md`
- `src/Memora.Core/AgentInteraction/ExternalRuntimeContract.cs`

That means Machina should only rely on the published shared operations:

- project lookup
- deterministic context retrieval
- artifact proposal submission
- artifact update proposal submission
- outcome recording

Anything beyond that belongs to future scoped work, not to the current
interaction model.

## Standard Interaction Loop

### 1. Resolve Project

Machina first resolves the target Memora project so later requests are scoped
to a known workspace.

- MCP path: `memora://projects/{projectId}`
- OpenAPI path: `GET /api/projects/{projectId}`

If the project is unavailable, Machina should stop rather than guess.

### 2. Request Deterministic Context

Machina requests context for the current task through the shared
`GetContextRequest` contract.

Expected behavior:

- context comes from Memora's deterministic retrieval path
- inclusion reasoning remains explainable
- retrieval does not become semantic or probabilistic
- the returned bundle is grounding input, not canonical truth mutation

### 3. Execute Outside Memora

Machina performs its runtime work outside Memora.

Examples:

- planning or implementation steps
- analysis over the returned context bundle
- tool execution using its own runtime environment

Memora is not responsible for orchestration, scheduling, or runtime hosting in
this phase.

### 4. Submit A Reviewable Result

When runtime work should be captured, Machina submits a reviewable artifact
through the shared proposal-facing contract.

Use:

- `ProposeArtifactRequest` for a new proposed artifact
- `ProposeUpdateRequest` for a proposed revision to an existing artifact
- `RecordOutcomeRequest` for a reviewable outcome artifact

All of these remain non-canonical on submission.

### 5. Preserve Human Approval

After proposal submission:

- Memora stores the reviewable result in non-canonical state
- a human approval flow decides whether any change becomes canonical
- Machina does not promote artifacts directly to approved state

This step is the governance boundary that must not be bypassed.

## Failure And Retry Expectations

Machina should treat Memora contract errors as explicit integration feedback.

- validation errors mean the request shape or content must be corrected
- project lookup failures mean the runtime should stop or ask for a valid
  project
- revision mismatch errors mean the runtime should refresh its working context
  instead of overwriting existing memory assumptions

Retries should reuse the shared contract. They should not introduce side
channels around Memora's published boundary.

## Operator Guidance

When an operator is using Machina with Memora, the safe flow is:

1. confirm the intended Memora project
2. request deterministic context for the active task
3. let Machina perform runtime work outside Memora
4. submit a proposal or outcome back through the shared contract
5. review and approve separately before any canonical update happens

## Explicit Non-Goals

This interaction model does not claim:

- a hosted Machina adapter inside Memora
- direct canonical writes by agents or runtimes
- provider-specific core services for Machina
- semantic or vector retrieval in Memora core
- that Memora replaces Machina's runtime responsibilities

## Summary

Machina should treat Memora as a governed memory boundary: read deterministic
context, execute externally, and submit reviewable results back through the
shared contract. That keeps Memora stable as structured memory and governance
while leaving runtime behavior where it belongs.
