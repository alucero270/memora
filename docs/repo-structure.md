Memora Repo Structure

memora/
├─ artifacts/
│  └─ README.md
├─ build/
│  └─ README.md
├─ docs/
│  ├─ adr/
│  │  └─ README.md
│  ├─ architecture.md
│  ├─ charter.md
│  ├─ data-model.md
│  ├─ delivery-process.md
│  ├─ feasibility.md
│  ├─ integration-strategy.md
│  ├─ interface-spec.md
│  ├─ memora-vs-strata.md
│  ├─ milestones.md
│  ├─ operations.md
│  ├─ progression-model.md
│  ├─ quality-attributes.md
│  ├─ repo-structure.md
│  ├─ requirements.md
│  ├─ scope.md
│  ├─ test-strategy.md
│  ├─ traceability.md
│  ├─ use-cases.md
│  └─ README.md
├─ samples/
│  ├─ workspaces/
│  │  └─ demo-project/
│  │     ├─ canonical/
│  │     ├─ drafts/
│  │     ├─ summaries/
│  │     └─ project.json
│  └─ README.md
├─ src/
│  ├─ Memora.Core/
│  │  └─ README.md
│  ├─ Memora.Storage/
│  │  └─ README.md
│  ├─ Memora.Index/
│  │  └─ README.md
│  ├─ Memora.Context/
│  │  └─ README.md
│  ├─ Memora.Api/
│  │  └─ README.md
│  ├─ Memora.Mcp/
│  │  └─ README.md
│  ├─ Memora.Ui/
│  │  └─ README.md
│  └─ README.md
├─ tests/
│  ├─ Memora.Core.Tests/
│  ├─ Memora.Storage.Tests/
│  ├─ Memora.Index.Tests/
│  ├─ Memora.Context.Tests/
│  ├─ Memora.Api.Tests/
│  ├─ Memora.Mcp.Tests/
│  └─ README.md
├─ .github/
│  ├─ ISSUE_TEMPLATE/
│  │  └─ issue-template.md
│  └─ workflows/
│     └─ validate-platform-readiness.yml
├─ .editorconfig
├─ .gitattributes
├─ .gitignore
├─ .env.example
├─ AGENTS.md
├─ build.cmd
├─ build.sh
├─ CONTRIBUTING.md
├─ LICENSE
├─ NuGet.Config
├─ README.md
└─ Memora.sln


---

Top-level folder intent

artifacts/

Generated build outputs, packaged deliverables, and temporary validation outputs.

Do not store canonical Memora project memory here.

build/

Repository-level build helpers, validation scripts, bootstrap scripts, and future build customizations.

docs/

Project truth for planning, architecture, scope, requirements, and operational guidance.

samples/

Example workspaces, fixture artifacts, demo data, and parser/index/rebuild test material.

src/

Product code, split by responsibility.

tests/

Automated tests grouped by module.

.github/

Issue templates and CI workflows.


---

Module boundaries

src/Memora.Core

Purpose

domain types

artifact schemas

lifecycle rules

validation primitives


Must not contain

file I/O

SQLite logic

API controllers

MCP logic

UI logic



---

src/Memora.Storage

Purpose

markdown + frontmatter parsing

filesystem persistence

revision file handling

workspace layout helpers


Must not contain

ranking

API logic

MCP logic



---

src/Memora.Index

Purpose

SQLite schema

artifact metadata indexing

link indexing

rebuild-from-files support


Must not contain

canonical truth rules

provider-specific logic



---

src/Memora.Context

Purpose

layered retrieval

deterministic ranking

context package assembly


Must not contain

persistence

API/controller logic



---

src/Memora.Api

Purpose

OpenAPI surface

project/artifact endpoints

approval endpoints

local service host


Must not contain

duplicated lifecycle logic

duplicated ranking logic



---

src/Memora.Mcp

Purpose

MCP server exposure

tools/resources/prompts mapping

provider-facing protocol layer


Must not contain

business logic separate from core services



---

src/Memora.Ui

Purpose

local inspection and control UI

project selector

artifact browser

approval queue

context viewer


Must not contain

canonical rules duplicated from core



---

Folder README stub templates

src/README.md

# src

## Purpose
Contains all Memora product code.

## Layout
- Memora.Core
- Memora.Storage
- Memora.Index
- Memora.Context
- Memora.Api
- Memora.Mcp
- Memora.Ui

## Rule
Keep module boundaries strict. Do not duplicate domain rules across projects.

src/Memora.Core/README.md

# Memora.Core

## Purpose
Defines Memora's core domain model and rules.

## Responsibilities
- artifact schemas
- lifecycle rules
- validation primitives

## Does NOT contain
- storage logic
- API logic
- MCP logic
- indexing
- UI logic

src/Memora.Storage/README.md

# Memora.Storage

## Purpose
Handles artifact file parsing and persistence.

## Responsibilities
- markdown + frontmatter parsing
- workspace file layout
- canonical and draft persistence
- revision file handling

## Does NOT contain
- lifecycle rules
- API logic
- ranking logic

src/Memora.Index/README.md

# Memora.Index

## Purpose
Maintains the derived SQLite index for Memora artifacts.

## Responsibilities
- SQLite schema
- indexing metadata
- indexing links
- rebuild from files

## Does NOT contain
- canonical truth decisions
- provider-specific integrations

src/Memora.Context/README.md

# Memora.Context

## Purpose
Builds deterministic context packages for agents and workflows.

## Responsibilities
- layered retrieval
- deterministic ranking
- context assembly

## Does NOT contain
- storage or file parsing
- API controllers

src/Memora.Api/README.md

# Memora.Api

## Purpose
Exposes Memora capabilities through a local OpenAPI-compatible service.

## Responsibilities
- project endpoints
- artifact endpoints
- approval endpoints
- context endpoints

## Does NOT contain
- duplicated core rules
- duplicated ranking logic

src/Memora.Mcp/README.md

# Memora.Mcp

## Purpose
Exposes Memora through MCP as the primary provider-facing protocol.

## Responsibilities
- MCP tools
- MCP resources
- MCP prompts
- mapping protocol calls to Memora services

## Does NOT contain
- business logic beyond protocol adaptation

src/Memora.Ui/README.md

# Memora.Ui

## Purpose
Provides the local operator interface for Memora.

## Responsibilities
- project selection
- artifact browsing
- draft editing
- approval queue
- context inspection

## Does NOT contain
- lifecycle rules
- indexing logic

tests/README.md

# tests

## Purpose
Contains automated tests for Memora modules.

## Testing stance
- core domain rules should be strongly covered
- integration layers must include the smallest meaningful validation
- no feature is complete without appropriate tests or validation

docs/README.md

# docs

## Purpose
Contains the planning, design, scope, and operational documentation for Memora.

## Rule
Docs must describe current implementation honestly and keep roadmap work clearly separate from shipped behavior.

samples/README.md

# samples

## Purpose
Contains sample workspaces, fixture artifacts, and demo data for Memora.

## Rule
Use samples to validate parsing, indexing, rebuild behavior, and example workflows.


---

Workspace note

Actual Memora-managed project workspaces should live outside the product source repo by default.

Use samples/workspaces/ only for:

example data

fixture artifacts

parser/index/rebuild testing

demos



---

Recommended immediate scaffold order

1. top-level repo skeleton


2. src/ module folders + READMEs


3. tests/ folders


4. docs/ starter files


5. samples/workspaces/demo-project/


6. solution and project files


7. CI/workflow stubs


