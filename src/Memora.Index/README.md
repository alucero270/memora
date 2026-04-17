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

## Key Areas

- `Schema/SqliteIndexSchema.cs`: schema creation and rebuildable index shape
- `Rebuild/SqliteIndexRebuilder.cs`: rebuilds the index from filesystem truth
- `Rebuild/IndexRebuildResult.cs`: diagnostics and rebuild summary models
