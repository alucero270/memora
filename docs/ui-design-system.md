# UI Design System Guidance

This document records the frontend design direction Memora should carry forward.
It is inspired by Brad Frost's Atomic Design methodology, but it is a
project-specific guide rather than a copy of the source material.

## Purpose

Memora UI work should be designed as a system of reusable parts, not as a
series of unrelated pages. Future operator screens should feel predictable to
work in because the same component roles, responsive behavior, and interaction
patterns show up consistently.

This guidance supports issue #122 and future UI work. It does not change
Memora's core rules, lifecycle behavior, storage model, retrieval model, or
agent proposal boundaries.

## Source Reference

- Atomic Design, Brad Frost
- Local reference copy used by the project owner:
  `C:\Users\Alex Lucero\Nextcloud\Documents\software\ai\Atomic Design.md`

Do not vendor the source text into this repository. Keep Memora docs focused on
the project-specific practices we want to preserve.

## Component Hierarchy

Use these levels as a shared vocabulary for UI work. The names are guidance,
not a mandate to create a framework before the UI needs one.

### Atoms

Small, reusable primitives with stable behavior.

Examples:

- text inputs
- select controls
- buttons and icon buttons
- labels
- status badges
- validation messages
- timestamps
- table cells

Expectations:

- keep sizing, spacing, focus, disabled, and error states consistent
- avoid page-specific styling baked into primitive controls
- make small-screen behavior part of the primitive when the primitive can
  reasonably own it

### Molecules

Combinations of atoms that perform a focused UI job.

Examples:

- project selector
- artifact status summary
- artifact metadata row
- approval decision control group
- context source summary
- navigation item
- filter control group

Expectations:

- own their layout rules across narrow and wide screens
- expose clear labels and states without relying on surrounding page copy
- stay small enough to reuse in different operator surfaces

### Organisms

Larger sections composed from molecules and atoms.

Examples:

- top navigation shell
- artifact browser table
- approval queue
- artifact detail panel
- section-value list
- context package viewer
- understanding output panel

Expectations:

- define the local information hierarchy for the section
- keep overflow contained inside the organism instead of leaking across the page
- make responsive stacking rules explicit

### Templates

Reusable page structures that arrange organisms for a workflow.

Examples:

- project selection layout
- project dashboard layout
- artifact detail layout
- review layout
- context and understanding layout

Expectations:

- describe the operator task the layout supports
- reuse existing organisms where possible
- avoid inventing one-off page structure when an existing template can adapt

### Pages

Concrete route-level screens populated with real project data.

Examples:

- `/`
- `/projects/{projectId}`
- `/projects/{projectId}/artifacts`
- `/context-viewer`
- `/understanding`

Expectations:

- should primarily compose existing templates and organisms
- should not be the first place where reusable visual rules are defined
- should remain honest about current capability and avoid promising unbuilt
  workflow behavior

## Memora UI Principles

- Build components before pages when a pattern is likely to repeat.
- Preserve operator trust by keeping labels explicit and workflow state visible.
- Design responsive behavior at the component level instead of patching each
  route after overflow appears.
- Keep navigation and project context stable across related screens.
- Do not hide governance limits; if a control is preview-only or inactive, make
  that state clear.
- Keep frontend code out of lifecycle, storage, indexing, and retrieval rules.
- Prefer shared rendering helpers or component boundaries when repetition starts
  to affect consistency.

## Responsive Expectations

Every reusable UI piece should define how it behaves at narrow widths before it
is reused broadly.

Required checks for UI work:

- top-level navigation must wrap or collapse without horizontal page overflow
- project selectors must fit small screens without pushing the viewport wider
- panel headers must stack cleanly when supporting text is long
- tables and dense records must contain their own horizontal scroll region
- badges, labels, and controls must not overlap neighboring text
- route-level pages should remain usable around 320px to 430px wide

## Future Work

Issue #122 should turn this guidance into the durable source for future UI
implementation. Useful follow-up slices may include:

- inventory current UI atoms, molecules, organisms, templates, and pages
- name reusable rendering helpers around the component hierarchy
- add focused visual or smoke checks for responsive component behavior
- document where each reusable UI pattern appears in the operator shell

