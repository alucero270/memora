# Delivery Process

This document describes the repo-level delivery expectations for milestone and
issue work.

## Core Rules

- one GitHub issue equals one reviewable change
- do not expand scope beyond the current issue or milestone slice
- keep current behavior and roadmap behavior clearly separated in docs
- run the smallest meaningful validation for each touched project

## Milestone Closeout

Every milestone must end with a docs refresh before the next milestone begins.

The closeout pass should update:

- `docs/current-state.md`
  - reflect what is actually implemented in the current checkout
- `README.md`
  - update the status section and any top-level run guidance that changed
- relevant module `README.md` files in `src/`
  - refresh entry points, local run notes, and current-scope language when module behavior changed
- `docs/README.md`
  - adjust navigation when new orientation docs are added

Update `docs/milestones.md` only when:

- the milestone definition itself changed, or
- a completion note is needed to clarify roadmap vs implemented state

## Milestone Completion Criteria

A milestone should be considered complete only when:

- its documented outcome is present in the current checkout
- validation for the touched surfaces passes
- current-state docs have been refreshed
- the repo is ready for the next milestone without relying on stale assumptions

## Next-Milestone Handoff

Before starting the next milestone:

1. confirm the previous milestone outcome is implemented in the checkout being used
2. complete the milestone closeout docs refresh
3. write a fresh kickoff prompt grounded in:
   - `AGENTS.md`
   - `docs/current-state.md`
   - `docs/milestones.md`
   - current branch and working tree state
4. keep the new prompt explicit about:
   - target milestone
   - milestone outcome
   - scope boundaries
   - validation expectations
   - stop conditions
