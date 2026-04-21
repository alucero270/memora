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

## Stacked PR Review And Merge

Unattended stacked milestone work creates one branch and one draft PR per issue.
Those PRs are review units first; they do not make issue work complete until the
changes land on the default branch.

When merging a stack after review:

1. merge the first issue PR into `main`
2. confirm `main` contains that issue's completed behavior and validation result
3. retarget the next PR in the stack to `main` before merging it
4. repeat from the bottom of the stack until the final issue PR lands on `main`

Do not treat a PR merged into an intermediate feature branch as mainline
completion. If a stacked PR is merged into another feature branch, the issue
work is still pending until that branch content reaches `main`.

After the final mainline merge:

- confirm every issue in the stack closed or was manually closed for the correct
  mainline merge
- confirm no remaining stack PR targets a deleted or stale intermediate branch
- delete completed local feature branches only after they are merged into
  `origin/main`
- delete the matching remote feature branches after the final mainline merge
- fetch with pruning so local branch and remote-tracking state reflects GitHub

If the stack merge order goes wrong, open a narrow consolidation PR that brings
the missing branch content into `main`, validate the combined result, and close
issues only after the consolidation reaches the default branch.

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
