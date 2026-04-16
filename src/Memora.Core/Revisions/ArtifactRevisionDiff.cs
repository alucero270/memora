using Memora.Core.Artifacts;
using Memora.Core.Validation;

namespace Memora.Core.Revisions;

public enum ArtifactFieldChangeKind
{
    Added,
    Removed,
    Modified
}

public sealed record ArtifactFieldChange(
    string Path,
    string? BeforeValue,
    string? AfterValue,
    ArtifactFieldChangeKind Kind);

public sealed class ArtifactRevisionDiff
{
    public ArtifactRevisionDiff(
        ArtifactDocument currentApprovedArtifact,
        ArtifactDocument candidateArtifact,
        IEnumerable<ArtifactFieldChange> changes)
    {
        CurrentApprovedArtifact = currentApprovedArtifact ?? throw new ArgumentNullException(nameof(currentApprovedArtifact));
        CandidateArtifact = candidateArtifact ?? throw new ArgumentNullException(nameof(candidateArtifact));
        Changes = changes.OrderBy(change => change.Path, StringComparer.Ordinal).ToArray();
    }

    public ArtifactDocument CurrentApprovedArtifact { get; }

    public ArtifactDocument CandidateArtifact { get; }

    public IReadOnlyList<ArtifactFieldChange> Changes { get; }

    public bool HasChanges => Changes.Count > 0;
}

public sealed class ArtifactRevisionDiffResult
{
    public ArtifactRevisionDiffResult(
        ArtifactRevisionDiff? diff,
        ArtifactValidationResult validation)
    {
        Diff = diff;
        Validation = validation ?? throw new ArgumentNullException(nameof(validation));
    }

    public ArtifactRevisionDiff? Diff { get; }

    public ArtifactValidationResult Validation { get; }

    public bool IsSuccess => Validation.IsValid && Diff is not null;
}
