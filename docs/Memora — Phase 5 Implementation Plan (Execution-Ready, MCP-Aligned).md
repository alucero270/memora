## 1. Overview

This document defines the execution plan for Memora v1, updated to align with an MCP-based integration model.

Goal: Build the smallest complete end-to-end loop:

Planning → Memora → Approval → Retrieval → Agent (via MCP) → Proposal → Approval

---

## 2. System Architecture (Condensed)

Core layers:

- memora-core → artifacts, lifecycle, storage
- memora-index → SQLite indexing + retrieval
- memora-ingest → planning intake + draft generation
- memora-context → context package builder
- memora-ui → inspection + approval surface
- memora-mcp-server → MCP tool interface (external integration boundary)

---

## 3. Vertical Slice Definition

### Slice 1 (Must Work End-to-End)

1. Import planning session
2. Generate draft artifacts
3. Approve artifacts
4. Persist canonical memory
5. Retrieve context
6. Agent consumes context via MCP
7. Agent submits proposal/outcome via MCP
8. Approve proposal

---

## 4. Milestone Breakdown

### Milestone 1 — Memory Core

- Projects
- Artifacts
- Lifecycle
- Storage
- Index

### Milestone 2 — Human Loop

- Planning intake
- Draft generation
- Approval UI

### Milestone 3 — Agent Loop (MCP)

- Context retrieval
- MCP server implementation
- Proposal submission via MCP

---

## 5. Milestone 1 — Detailed Tasks

### 5.1 Project Workspace

Structure:

/projects/{project_id}/ /canonical/ /drafts/ /summaries/ project.json

Tasks:

- create project
- load project
- set active project

---

### 5.2 Artifact Schema

Base schema:

- id
- type
- title
- status
- created_at
- updated_at
- links[]
- metadata{}

Schemas:

- charter
- plan
- adr
- constraint
- question
- outcome
- repo_structure
- session_summary

---

### 5.3 Lifecycle Engine

States:

- proposed
- draft
- approved
- superseded

Functions:

- create_proposal()
- promote_to_draft()
- approve()
- reject()
- supersede()

---

### 5.4 File Storage

Canonical:

- /canonical/

Drafts:

- /drafts/

Requirements:

- versioned files
- stable naming

---

### 5.5 SQLite Index

Tables:

- artifacts
- links
- projects

Tasks:

- insert/update index
- rebuild index from filesystem
- parse typed links

---

### 5.6 Relationship Parsing

Parse:

- [[artifact-id]]

Map to:

- depends_on
- affects
- derived_from
- supersedes

---

## 6. Milestone 2 — Human Loop

### 6.1 Planning Intake API

Endpoint:

POST /intake/planning

Input:

- project_id
- raw_text

Output:

- draft artifacts
- session summary

---

### 6.2 Draft Generation

Transform into:

- Plan
- Decisions
- Constraints
- Questions

---

### 6.3 Approval Flow

- list drafts
- view artifact
- edit draft
- approve
- reject

---

### 6.4 Diff Engine

- compare draft vs canonical

---

### 6.5 Minimal UI

- project selection
- artifact browser
- approval queue
- draft editor
- context viewer

---

## 7. Milestone 3 — Agent Loop (MCP)

### 7.1 MCP Server (New Core Component)

Responsibilities:

- expose Memora capabilities as MCP tools
- handle request/response lifecycle
- route tool calls to internal services

Transport:

- HTTP / SSE (initial)

---

### 7.2 Tool Definitions (v1)

#### get_context

Input:

- project_id
- task

Output:

- Layer 1 (charter, plan, repo)
- Layer 2 (relevant artifacts)

---

#### propose_artifact

Input:

- project_id
- type
- payload

Output:

- draft artifact created

---

#### propose_update

Input:

- project_id
- artifact_id
- patch

Output:

- updated draft

---

#### record_outcome

Input:

- project_id
- payload

Output:

- outcome artifact (draft)

---

### 7.3 Context Builder

Function:

get_context(project_id, task)

Responsibilities:

- assemble Layer 1 + Layer 2
- apply deterministic ranking

---

### 7.4 Ranking Engine

Scoring factors:

- type priority
- canonical status
- milestone relevance
- link proximity
- recency

---

### 7.5 Agent Contract

Agents must:

- call get_context before execution
- submit proposals instead of direct writes
- record outcomes after execution

---

### 7.6 Outcome Recording

Store:

- action performed
- result
- implications

---

## 8. Initial Repo Structure

/memora /core /index /ingest /context /ui /mcp

---

## 9. Interfaces (MCP-Aligned)

All external interaction occurs through MCP tools.

Internal functions:

- get_context()
- propose_artifact()
- propose_update()
- record_outcome()

These are exposed via MCP, not direct API integrations.

---

## 10. MCP Tool Schemas (v1)

This section defines the initial tool contracts exposed by `memora-mcp-server`.

Design goals:

- stable and explicit contracts
- small payloads
- deterministic behavior
- approval-safe write model
- provider-agnostic naming

### 10.1 Tool: get_context

Purpose:

Return grounded project context for an execution task.

Input schema:

```json
{
  "project_id": "string",
  "task": "string",
  "max_artifacts": 12,
  "include_layers": ["core", "relevant"]
}
```

Field notes:

- `project_id` — target project
- `task` — plain-language task description used for deterministic retrieval
- `max_artifacts` — upper bound for Layer 2 retrieval
- `include_layers` — allowed values: `core`, `relevant`, `historical`

Output schema:

```json
{
  "project_id": "string",
  "task": "string",
  "core": {
    "charter": {},
    "active_plan": {},
    "repo_structure": {}
  },
  "relevant": [
    {
      "artifact_id": "string",
      "type": "adr",
      "title": "string",
      "status": "approved",
      "score": 0.0,
      "reason": "matched active milestone and related constraints",
      "content": {}
    }
  ],
  "generated_at": "ISO-8601"
}
```

Behavior rules:

- always return Layer 1 core artifacts when available
- never return drafts unless explicitly enabled in a future version
- include retrieval reason for every Layer 2 artifact
- return deterministic ordering for identical input

---

### 10.2 Tool: propose_artifact

Purpose:

Create a new proposed artifact from agent output.

Input schema:

```json
{
  "project_id": "string",
  "type": "plan|adr|constraint|question|outcome|repo_structure",
  "title": "string",
  "content": {},
  "links": [
    {
      "type": "depends_on",
      "target_id": "artifact-123"
    }
  ],
  "rationale": "string",
  "source": {
    "actor": "agent",
    "name": "string",
    "session_id": "string"
  }
}
```

Output schema:

```json
{
  "proposal_id": "string",
  "artifact_id": "string",
  "status": "proposed",
  "stored_in": "drafts",
  "created_at": "ISO-8601"
}
```

Behavior rules:

- created artifacts enter `proposed` state only
- canonical writes are forbidden
- schema validation must occur before persistence
- invalid typed links must fail with explicit error

---

### 10.3 Tool: propose_update

Purpose:

Submit a patch against an existing artifact.

Input schema:

```json
{
  "project_id": "string",
  "artifact_id": "string",
  "patch": {
    "title": "optional string",
    "content": {},
    "links": []
  },
  "rationale": "string",
  "source": {
    "actor": "agent",
    "name": "string",
    "session_id": "string"
  }
}
```

Output schema:

```json
{
  "proposal_id": "string",
  "artifact_id": "string",
  "based_on_version": "string",
  "status": "proposed",
  "diff_available": true,
  "created_at": "ISO-8601"
}
```

Behavior rules:

- updates must preserve the original artifact until approval
- diffs must be computable against the current approved version
- rejected proposals must not mutate approved state

---

### 10.4 Tool: record_outcome

Purpose:

Capture the result of execution as a structured outcome artifact.

Input schema:

```json
{
  "project_id": "string",
  "task": "string",
  "result": "success|partial|failure",
  "summary": "string",
  "implications": ["string"],
  "related_artifact_ids": ["string"],
  "rationale": "string",
  "source": {
    "actor": "agent",
    "name": "string",
    "session_id": "string"
  }
}
```

Output schema:

```json
{
  "proposal_id": "string",
  "artifact_id": "string",
  "type": "outcome",
  "status": "proposed",
  "created_at": "ISO-8601"
}
```

Behavior rules:

- outcomes must be stored as structured artifacts, not freeform logs
- result classification is required
- related artifacts should be linked when known

---

### 10.5 Shared Error Model

All MCP tools should return consistent machine-readable errors.

Error schema:

```json
{
  "error": {
    "code": "string",
    "message": "string",
    "details": {}
  }
}
```

Initial error codes:

- `project_not_found`
- `artifact_not_found`
- `schema_validation_failed`
- `invalid_link_type`
- `invalid_state_transition`
- `index_unavailable`
- `context_not_available`

---

### 10.6 Contract Rules

- Tools must be idempotent where practical for retried requests
- Read and write tools must remain separate
- Every write request must capture provenance
- Every write result must be reviewable in the approval workflow
- No MCP tool may bypass lifecycle rules
- Tool names must remain stable once published

---

### 10.7 Future MCP Extensions (Not v1)

Deferred:

- approve_artifact
- reject_artifact
- list_artifacts
- search_artifacts
- get_artifact_history
- refresh_repo_structure

These remain outside v1 to keep human approval centralized in the UI.

---

## 11. Execution Notes

- Build Milestone 1 first
- Do NOT build multiple integrations
- MCP is the only external interface
- Keep schemas strict but simple
- Ensure index rebuild works
- Focus on working loop, not completeness

---

## 12. Key Architectural Constraint (New)

Memora must not implement platform-specific adapters.

All external systems (ChatGPT, Claude, Codex, etc.) interact via MCP only.

This preserves:

- provider agnosticism
- maintainability
- system boundaries

---

## Status

Phase 5 updated and aligned with MCP integration strategy.

Ready for implementation.