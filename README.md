# Memora

Memora is a **local-first structured memory and governance system**
for AI-assisted software development.

It ensures that planning, decisions, constraints, and outcomes are:

- structured
- versioned
- linked
- approval-controlled
- durable across sessions

---

## Why Memora Exists

AI tools improve execution, but project context is often lost between sessions.

This leads to:

- repeated explanations
- inconsistent behavior
- hallucinated continuity
- drift between planning and execution

Memora solves this by making project state:

> explicit, structured, and persistent.

See the project charter:
- `docs/charter.md`

---

## Core Principles

- Filesystem is the source of truth
- SQLite is a derived index
- Retrieval is deterministic and explainable
- Agents may propose, not write canonical state
- All changes require approval

---

## System Overview

Memora is composed of modular components:

- **Core** → schemas, lifecycle, validation
- **Storage** → markdown + frontmatter parsing
- **Index** → SQLite indexing (rebuildable)
- **Context** → deterministic retrieval + ranking
- **API** → OpenAPI surface
- **MCP** → primary integration layer
- **UI** → inspection and approval interface

See:
- `docs/architecture.md`

---

## Repository Structure

See full structure:
- `docs/repo-structure.md`

---

## How It Works (v1 Flow)

1. Planning input is ingested
2. Draft artifacts are generated
3. Human reviews and approves artifacts
4. Canonical memory is stored (filesystem)
5. Context is retrieved deterministically
6. Agents consume context via MCP
7. Agents propose updates or outcomes
8. Human approves changes

---

## Milestones

Memora is built in stages:

- Milestone 1 — Memory Core
- Milestone 2 — Human Loop
- Milestone 3 — Agent Loop

See:
- `docs/milestones.md`

---

## Getting Started

### 1. Scaffold the repository

Run:

- `dotnet build Memora.sln`
- `build.cmd`
- `build.sh`

---

## Development Rules

All contributors must follow:

- `AGENTS.md`
- `CONTRIBUTING.md`

---

## Scope (v1)

Memora focuses on:

- structured project memory
- deterministic retrieval
- approval-governed updates

See:
- `docs/scope.md`

---

## Non-Goals

Memora is not:

- a chat system
- a vector database
- a general knowledge base
- an execution runtime

---

## Status

Early implementation phase.

Milestone 1 — Memory Core.

---

## License

TBD
