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
