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

## Key Areas

- `Program.cs`: route registration and service wiring
- `Services/FileSystemAgentInteractionService.cs`: file-backed project, context, proposal, outcome, and guarded session-summary write flow
- `Services/UnavailableAgentInteractionService.cs`: guarded fallback when no workspace root is configured
- `AgentInteractionHttpResults.cs`: maps shared contract results to HTTP responses

## Current Scope

- endpoints are minimal and focused on the shared agent interaction contract
- the host publishes a companion OpenAPI document at `/openapi.json`
- the file-backed path is only active when a workspace root is configured
- validation errors preserve structured code/path fields and use diagnostic messages from core validation
- the guarded direct-write prototype is limited to non-canonical session summaries in summary storage
- this host is intentionally thin and does not claim a full production API surface

## Local Development
- the default development launch profile listens on `http://127.0.0.1:5081`
- verify the current OpenAPI document at `http://127.0.0.1:5081/openapi.json`
- set `MEMORA_WORKSPACES_ROOT` or `Memora:WorkspacesRootPath` to a workspace root before exercising file-backed endpoints
- this host is intended to run alongside `Memora.Ui`, which uses its own development port
