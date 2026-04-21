using Memora.Core.Artifacts;
using Memora.Core.Validation;

namespace Memora.Core.Revisions;

public enum ArtifactFieldChangeKind
{
    Added,
    Removed,
    Modified
}

public enum ArtifactFieldChangeArea
{
    Metadata,
    Sections,
    Links,
    TypeSpecific
}

public sealed record ArtifactFieldChange(
    string Path,
    string? BeforeValue,
    string? AfterValue,
    ArtifactFieldChangeKind Kind)
{
    public ArtifactFieldChangeArea Area => Path switch
    {
        _ when Path.StartsWith("sections.", StringComparison.Ordinal) => ArtifactFieldChangeArea.Sections,
        _ when Path.StartsWith("links.", StringComparison.Ordinal) => ArtifactFieldChangeArea.Links,
        _ when Path.StartsWith("type_specific.", StringComparison.Ordinal) => ArtifactFieldChangeArea.TypeSpecific,
        _ => ArtifactFieldChangeArea.Metadata
    };

    public string DisplayPath => Path switch
    {
        "title" => "Title",
        "reason" => "Reason",
        "provenance" => "Provenance",
        "tags" => "Tags",
        _ when Path.StartsWith("sections.", StringComparison.Ordinal) =>
            "Section: " + Path["sections.".Length..],
        _ when Path.StartsWith("links.", StringComparison.Ordinal) =>
            "Link: " + FormatKey(Path["links.".Length..]),
        _ when Path.StartsWith("type_specific.", StringComparison.Ordinal) =>
            "Type-specific: " + FormatKey(Path["type_specific.".Length..]),
        _ => Path
    };

    private static string FormatKey(string key) =>
        key.Replace('_', ' ');
}

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

    public int ChangeCount => Changes.Count;

    public IReadOnlyList<ArtifactFieldChangeArea> ChangedAreas =>
        Changes
            .Select(change => change.Area)
            .Distinct()
            .OrderBy(area => area)
            .ToArray();
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
