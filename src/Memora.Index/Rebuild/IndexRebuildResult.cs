namespace Memora.Index.Rebuild;

public sealed record IndexRebuildResult(
    int ProjectCount,
    int ArtifactCount,
    int RevisionCount,
    int RelationshipCount,
    IReadOnlyList<IndexRebuildDiagnostic> Diagnostics,
    int FilesystemProjectCount,
    int FilesystemArtifactFileCount)
{
    public bool Success => Diagnostics.Count == 0;

    public string Summary =>
        Success
            ? $"Rebuilt derived SQLite index from filesystem truth: {ProjectCount} project(s), {ArtifactCount} artifact(s), {RevisionCount} revision(s), {RelationshipCount} relationship(s)."
            : $"Rebuild failed with {Diagnostics.Count} diagnostic(s). Filesystem truth was scanned ({FilesystemProjectCount} project(s), {FilesystemArtifactFileCount} artifact file(s)); derived SQLite index rows were cleared and not repopulated.";
}
