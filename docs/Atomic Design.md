# Atomic Design Distillation For Memora

This document distills Atomic Design ideas into Memora-specific UI practice.
It is not a copy of the source text. It preserves the concepts that matter for
Memora's operator interface: build systems, name parts clearly, validate
responsive behavior early, and keep the design language maintainable as the
product grows.

Use this alongside `ui-design-system.md`. That file is the durable project
guidance for implementation; this file explains the thinking Memora should
carry forward when new UI work is planned.

## Why This Matters Here

Memora is a local-first memory and governance tool. Its UI is not a marketing
site and not a collection of isolated screens. Operators need to inspect
project state, compare artifacts, understand lifecycle status, review proposed
changes, and trust that the interface is telling the truth.

That makes the design system part of the product contract:

- repeated workflow elements should look and behave the same way
- lifecycle and approval state must be visible, not decorative
- dense records should remain scannable on both desktop and narrow screens
- new pages should reuse known parts before inventing new visual structure
- UI code must not smuggle lifecycle, storage, retrieval, or approval rules
  into rendering concerns

The page is still useful as a route and operator destination, but it is the
wrong unit for estimating or designing most UI work. For Memora, the reusable
unit is the workflow component.

## Memora Translation

Atomic Design gives Memora a vocabulary for deciding where a UI decision
belongs.

| Level | Memora Meaning | Examples |
| --- | --- | --- |
| Atom | Small primitive with stable interaction rules | button, text input, badge, timestamp, label, table cell |
| Molecule | Focused control or display group | project selector, status summary, metadata row, filter group |
| Organism | Complete workflow section | artifact browser, approval queue, review panel, context viewer |
| Template | Reusable route structure | project dashboard, artifact detail, review workflow, understanding view |
| Page | Concrete route with real data | `/`, `/projects/{projectId}`, `/context-viewer`, `/understanding` |

The hierarchy is practical, not ceremonial. If a thing only appears once and
has no reusable behavior, it can stay local. If it repeats, carries workflow
meaning, or needs responsive rules, give it a named component role.

## System-First Rules

Before adding a route, identify the parts it needs:

- What operator decision does the screen support?
- Which existing atoms, molecules, organisms, or templates already cover part
  of the job?
- Which new part is truly reusable?
- Which state must be visible for trust: lifecycle, revision, project,
  proposal, approval readiness, retrieval source, or validation result?
- How does each part behave when labels are long, records are dense, or the
  viewport is narrow?

Do not design a page as a static picture and then retrofit components. Build the
parts and the page together so the route demonstrates the system instead of
escaping it.

## Component Inventory To Grow Toward

Memora should converge on a small, explicit inventory:

- navigation atoms and molecules: links, current route state, project context
- lifecycle display: status badges, revision labels, approval state indicators
- forms: labeled inputs, selectors, validation messages, proposed-change
  controls
- data display: record rows, property lists, tables, section/value pairs,
  traceability rows
- workflow organisms: artifact browser, approval queue, diff review panel,
  context package viewer, understanding output panel
- templates: dashboard, artifact detail, review, context inspection, local
  setup and diagnostics

When a new issue touches one of these areas, the implementation should either
reuse the existing part or intentionally improve the shared part.

## Responsive Behavior Is Part Of The Component

Responsive design is not a final cleanup pass. In Memora, dense local operator
screens must work when:

- navigation wraps
- project names and artifact ids are long
- tables contain wide values
- approval and diff metadata stack
- badges and controls appear beside multi-line labels
- the viewport is phone-width during quick local checks

Each reusable component should own its narrow-width behavior. Tables can scroll
inside their own region. Headers can stack. Control groups can wrap. The page
should not need special-case patches for every repeated component.

## Pattern Library Thinking Without Premature Framework

Memora does not need to pause feature work to invent a full design-system
platform. It does need pattern-library discipline:

- name repeated UI parts consistently in docs and code
- keep examples close to the renderer or tests that exercise them
- add smoke checks when a responsive or workflow pattern becomes important
- document which route uses each major organism or template
- treat visual drift as maintenance debt, not personal preference

As the UI grows, a living component inventory can become more formal. Until
then, `ui-design-system.md`, `src/Memora.Ui/README.md`, renderer helpers, and
focused UI tests are the lightweight pattern library.

## Workflow For Future UI Issues

For any UI issue, work in this order:

1. Read the issue acceptance criteria and current UI docs.
2. Identify the affected component level: atom, molecule, organism, template,
   or page.
3. Reuse an existing part when the behavior already exists.
4. Create or adjust the smallest shared part when a pattern should repeat.
5. Keep page-specific composition thin.
6. Validate the workflow state and at least one narrow-width behavior when the
   change affects layout.
7. Update docs only when the shared pattern or operator behavior changes.

This keeps one issue reviewable while still improving the system.

## Maintenance Rules

A design system survives by being maintained during ordinary work:

- remove or consolidate duplicate patterns when they confuse future changes
- keep names aligned between docs, renderer helpers, tests, and visible UI
- prefer boring consistency over clever one-off styling
- make deprecated UI paths explicit before removing them
- avoid promising unimplemented capabilities in labels or page text
- keep provider-specific integration details out of shared UI primitives

## Memora-Specific Boundaries

Atomic Design does not override Memora architecture.

- Filesystem remains canonical truth.
- SQLite remains a derived, rebuildable index.
- Agents may propose changes only in v1.
- Canonical updates require approval.
- Retrieval remains deterministic and explainable.
- Semantic/vector retrieval does not belong in core v1.
- Memora is not an execution runtime.

The UI may reveal these constraints, but it must not bypass them.

## Carry-Forward Principle

Design Memora screens as evidence of a coherent system. A page is successful
when an operator can understand the current project state, trust the governance
boundary, and recognize familiar workflow parts without relearning the product.
