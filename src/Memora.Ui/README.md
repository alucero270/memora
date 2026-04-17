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

## Current Shell
- runs as a minimal local ASP.NET Core shell
- reads workspace files through `Memora.Storage`
- uses a writable local copy of `samples/workspaces` unless `MemoraUi__WorkspacesRoot` is set
- supports project selection, artifact browsing, draft editing, and approval review previews

## Honest Scope
- draft edits write new draft revisions to the selected workspace root
- approval review surfaces are wired to current core diff and queue behavior
- approval and rejection persistence are intentionally not claimed in this slice

## Local Development
- the default development launch profile listens on `http://127.0.0.1:5080`
- run it alongside `Memora.Api` on `http://127.0.0.1:5081` to avoid local port collisions
- point `MemoraUi__WorkspacesRoot` or `MEMORA_WORKSPACES_ROOT` at the same workspace root when you want both hosts to inspect the same files
