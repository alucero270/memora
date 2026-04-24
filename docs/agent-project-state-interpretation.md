# Agent Project-State Interpretation

## Purpose

This guide explains how an external agent should interpret the deterministic
project-state projection returned by Memora.

Use this guide with:

- `docs/project-state-projection.md`
- `docs/external-runtime-contract.md`

The goal is to make the runtime-facing output usable without reading the
implementation first.

## First Rule

Treat the projection as grounded context, not permission to mutate truth.

Memora returns a deterministic read model of current project state through the
existing `get_context` contract. That projection helps an agent understand the
project. It does not let the agent bypass approval, lifecycle, or canonical
storage rules.

## What Counts As Canonical

An artifact is canonical project truth only when it is both:

- stored in the approved filesystem-backed project state
- represented with `artifact.status = approved`

Agents may rely on approved artifacts as the authoritative project memory that
Memora is grounding from.

Examples:

- approved charter artifacts define durable project framing
- approved active plans define the current intended path
- approved decisions and constraints define real project commitments

## What Does Not Count As Canonical

The projection may also include non-canonical state when the request allows it
or when supporting history is explicitly requested.

Agents must not treat these as approved truth:

- `draft` artifacts
- `proposed` artifacts
- session summaries and similar supporting history

These artifacts are useful for context, but they remain review-only or
supporting state until a human approval flow produces an approved artifact.

## Lifecycle Meaning

The artifact lifecycle is still the meaning boundary:

- `proposed`: suggested state awaiting review
- `draft`: editable non-canonical working state
- `approved`: canonical truth eligible for grounding default context
- `superseded` or `deprecated`: historical state that is no longer current

Agents should interpret lifecycle conservatively:

- use `approved` as the default truth signal
- treat `draft` and `proposed` as candidate or review-only inputs
- avoid assuming superseded or deprecated artifacts define current behavior

## How To Read Inclusion

If an artifact appears in the projection, that means Memora selected it through
the deterministic retrieval path for the current request.

The `inclusionReasons` list explains why it appeared. Common examples:

- approved default grounding
- explicit draft allowance
- Layer 1 anchor selection
- explicit focus artifact match
- direct relationship to a focused artifact
- bounded traversal from a focused artifact
- milestone relevance
- direct task-term match

Agents should treat these reasons as explanation, not as proof that every
included artifact has equal authority.

Authority still comes from lifecycle and canonical status.

## How To Read Layers

- `Layer1` is the highest-priority grounding layer. It contains the core anchor
  artifacts when present, such as the charter, active plan, and repo snapshot.
- `Layer2` contains supporting canonical or explicitly allowed artifacts that
  add decisions, constraints, questions, and outcomes relevant to the task.
- `Layer3` is optional supporting history. It is useful background, not default
  truth.

Agents should read top-down:

1. establish current truth from `Layer1`
2. refine task understanding with `Layer2`
3. use `Layer3` only as supporting historical context when explicitly included

## Deterministic Guarantees

For identical approved inputs and identical request inputs, Memora aims to
return the same serialized projection.

Agents may rely on these properties:

- artifact selection is deterministic
- ordering is deterministic
- inclusion reasoning is deterministic
- the projection is explainable from stored artifacts and the request

Agents should not infer more than that:

- the projection is not semantic search
- the projection is not probabilistic ranking
- the projection is not a claim that every relevant artifact in existence was
  returned

## Safe Agent Behavior

An external agent should use the projection like this:

1. treat approved artifacts as the authoritative baseline
2. use non-canonical artifacts only as review-only or supporting context
3. preserve uncertainty when artifacts conflict or remain non-canonical
4. submit proposals or outcomes back through the shared contract instead of
   treating generated output as canonical truth

## Unsafe Agent Behavior

Do not:

- treat draft or proposed artifacts as already approved
- treat session summaries as canonical decisions
- infer approval from presence in the projection alone
- assume Memora has granted write permission because context was returned
- overwrite project understanding with runtime guesses that are not backed by
  approved artifacts

## Practical Shortcut

When in doubt:

- `approved` means grounded truth
- `draft` and `proposed` mean review-only candidate state
- supporting history informs context but does not overrule approved artifacts

That rule keeps agent interpretation aligned with Memora's governance model.
