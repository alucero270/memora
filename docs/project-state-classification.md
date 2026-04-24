# Project State Classification

## Purpose

This document adds the smallest classification language external agents need
when reading Memora project-state materials.

The point is to distinguish:

- public repo-facing documentation
- canonical approved project state
- non-canonical planning or review inputs

This is a documentation vocabulary only. It is not a permissions system.

## Classification Terms

### Public Repo Docs

Public repo docs are repository documents intended to describe the product,
architecture, current implementation, workflows, or integration model for human
readers and external collaborators.

Examples in this repository include:

- `docs/current-state.md`
- `docs/architecture.md`
- `docs/external-runtime-contract.md`
- `docs/machina-interaction-model.md`

These docs are useful orientation material, but they are not a replacement for
canonical approved project artifacts in a Memora workspace.

### Canonical Approved State

Canonical approved state is the authoritative project memory that Memora treats
as truth.

In Memora terms, that means approved artifacts stored in the filesystem-backed
canonical project state.

Examples:

- approved charters
- approved plans
- approved decisions
- approved constraints

If an agent needs the strongest truth signal, this is the classification to
prefer.

### Non-Canonical Review State

Non-canonical review state includes artifacts and materials that are useful for
reasoning or review, but are not yet approved truth.

Examples:

- `draft` artifacts
- `proposed` artifacts
- reviewable outcomes
- session summaries

These materials may appear in the projection when explicitly requested or when
supporting history is included. They remain review-only until approval changes
their lifecycle state.

### Planning Baseline Material

Planning baseline material includes roadmap, sequencing, or execution-planning
inputs that help humans and agents understand intended future work, but do not
become public truth just because they exist in the repository.

Examples referenced during M9:

- `samples/workspaces/demo-project/canonical/decisions/ADR-004.r0001.md`
- `samples/workspaces/demo-project/canonical/decisions/ADR-005.r0001.md`
- `samples/workspaces/demo-project/canonical/constraints/CNS-002.r0001.md`
- `samples/workspaces/demo-project/drafts/plan/PLN-002.r0001.md`

Those materials are useful roadmap context. They do not redefine the public
runtime contract, and draft planning content is not canonical truth.

## How Agents Should Use The Terms

Agents should interpret these classifications conservatively:

- public repo docs explain the system and its current shipped or intended
  boundaries
- canonical approved state is the authoritative grounding source
- non-canonical review state is informative but not yet approved
- planning baseline material explains roadmap intent, not current truth by
  itself

## Important Distinctions

### Public Is Not The Same As Canonical

A document can be public and still not be canonical project memory.

For example:

- a repo doc may accurately describe the current implementation
- but an approved artifact in the Memora workspace is still the stronger truth
  source for governed project memory

### Non-Canonical Is Not The Same As Secret

Non-canonical means lifecycle and authority status, not confidentiality.

A draft or proposal may be visible in a repository or workspace and still be
non-canonical. Its classification comes from approval state, not from who can
see the file.

### Planning Is Not Execution Truth

Planning material helps explain intended work and sequencing. It does not make
future behavior true before implementation and approval.

Agents should avoid treating roadmap or planning notes as already-shipped
behavior.

## What This Document Does Not Mean

This classification language does not define:

- access control
- user roles
- repository permissions
- security labels
- a new lifecycle model

It only gives agents a small vocabulary for interpreting project-state
materials correctly.

## Practical Shortcut

When in doubt:

- public docs explain
- canonical approved artifacts ground truth
- drafts, proposals, outcomes, and summaries remain non-canonical
- planning materials describe intended future work, not automatic truth
