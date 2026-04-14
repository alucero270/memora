Memora Execution Rules


---

1. What Memora Is

Memora is a:

local-first

structured memory system

governance layer for AI-assisted software development


Memora provides:

durable project state

structured artifacts

lifecycle-controlled updates

deterministic context retrieval



---

2. What Memora Is NOT

Memora is NOT:

a chat history store

a vector database

a generic knowledge base

an execution runtime

a replacement for Strata



---

3. Locked Architecture

Language: C#

Filesystem = canonical source of truth

SQLite = derived local index (rebuildable)

MCP = primary integration layer

OpenAPI = companion integration layer

Strata = external retrieval system (separate concern)



---

4. Core System Rules

4.1 Truth Model

Only approved artifacts are canonical truth

Drafts and proposals are not authoritative

Filesystem is always the final source of truth


4.2 Lifecycle Enforcement

All artifacts must follow lifecycle:

proposed → draft → approved → superseded/deprecated


No system component may bypass lifecycle rules.

4.3 Write Constraints

Agents may only propose changes in v1

No direct writes to canonical artifacts

All canonical updates require explicit approval


4.4 Retrieval Constraints

Retrieval must be deterministic and explainable

No semantic/vector retrieval in core v1

No probabilistic ranking in v1


4.5 Boundary Rules

Memora = structured project memory

Strata = broad retrieval/search

Do not mix these responsibilities



---

5. Artifact Rules

Artifacts must:

be strongly typed

be validated before persistence

include required metadata

support versioning/revision tracking


Do not:

store unstructured or ambiguous state

rely on implicit meaning



---

6. Coding Rules

Keep modules small and focused

Avoid premature abstraction

Prefer explicit over implicit behavior

Do not duplicate logic across layers

Keep provider-specific logic out of domain models



---

7. Integration Rules

MCP exposes tools/resources — not business logic

OpenAPI mirrors core capabilities where needed

All integration flows must respect lifecycle + approval



---

8. Delivery Rules

One issue = one reviewable change

Each change must have clear acceptance criteria

Do not implement beyond current milestone scope

Do not assume future features exist



---

9. Validation Expectations

Code must compile successfully

API changes must not break contract shape

Storage must remain filesystem-first

SQLite index must be rebuildable from files



---

10. Anti-Patterns (DO NOT DO)

Do not bypass approval for speed

Do not introduce vector DB logic into core

Do not treat retrieval results as truth

Do not mix planning logic into storage logic

Do not invent missing requirements


---

11. Guiding Principle

Memora preserves what the system knows, not what the system guesses.

---

12. Testing Policy
- core domain rule changes should include tests that define and protect those rules
- integration and UI changes must include the smallest meaningful validation needed to prove behavior before merge
- every feature should leave behind appropriate tests or validation before it is considered done
- strict test-first development is not required for every feature
- test-first is preferred for core logic such as lifecycle, validation, parsing, ranking, rebuild behavior, and project isolation
