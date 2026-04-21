# Controlled Automation

This document records the current controlled-automation boundary. It is
policy groundwork only; it does not grant agents direct canonical write access.

## Low-Risk Candidate Classes

Memora can discuss future direct-write behavior only for explicitly bounded
artifact classes. The current low-risk candidates are:

- `session_summary`: non-canonical summaries written under the summaries store.
  They remain lower risk because the artifact schema requires
  `canonical: false` and they do not become approved project truth.
- `repo_structure`: generated repository-shape snapshots while staged as draft
  artifacts for review. They are lower risk than planning artifacts only when
  they describe filesystem shape and do not become approved canonical truth
  without lifecycle governance.

## Not Low-Risk

The following artifact types are not direct-write candidates:

- `charter`
- `plan`
- `decision`
- `constraint`
- `question`
- `outcome`

These artifact types carry planning intent, decisions, constraints, open
questions, or execution outcomes. They must continue through proposal, review,
and approval workflows before they can influence canonical truth.

## V1 Boundary

In v1, low-risk classification does not weaken governance:

- agents still use proposal paths for canonical artifacts
- approved artifacts remain the only canonical truth
- direct canonical writes are not allowed by these definitions
- future automation must add explicit policy checks before any write can occur

## Policy Model

Controlled automation policies are explicit data models in core. A valid policy
must:

- name the policy and declare whether it is enabled
- require an explicit trigger
- list each allowed action, artifact type, storage scope, and guardrail
- refer only to low-risk candidate artifact classes
- include every guardrail required by the artifact-class definition
- reject canonical write scope when the artifact class does not allow it

The policy model is inert by itself. It does not register event triggers, start
automation, or create a write path.

## Safe Trigger Events

Controlled automation trigger handling distinguishes observed lifecycle events
from explicit operator requests:

- lifecycle and approval-adjacent events can be represented for audit and
  future workflow evaluation
- policy-governed automation requires an explicit trigger before it can become
  eligible
- trigger evaluation returns an eligibility decision and reason codes only
- trigger evaluation does not persist artifacts, approve artifacts, or mutate
  canonical truth
