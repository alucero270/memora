namespace Memora.Index.Rebuild;

public sealed record IndexRebuildResult(
    int ProjectCount,
    int ArtifactCount,
    int RevisionCount,
    int RelationshipCount,
    IReadOnlyList<IndexRebuildDiagnostic> Diagnostics)
{
    public bool Success => Diagnostics.Count == 0;
}
