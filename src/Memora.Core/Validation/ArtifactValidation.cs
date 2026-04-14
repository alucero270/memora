using System.Collections.ObjectModel;
using Memora.Core.Artifacts;

namespace Memora.Core.Validation;

public sealed record ArtifactValidationIssue(string Code, string Message, string? Path = null);

public sealed class ArtifactValidationResult
{
    public ArtifactValidationResult(IEnumerable<ArtifactValidationIssue> issues)
    {
        Issues = new ReadOnlyCollection<ArtifactValidationIssue>(issues.ToList());
    }

    public bool IsValid => Issues.Count == 0;

    public IReadOnlyList<ArtifactValidationIssue> Issues { get; }

    public static ArtifactValidationResult Success { get; } = new([]);
}

public sealed record ArtifactCreationResult(
    ArtifactDocument? Artifact,
    ArtifactValidationResult Validation);

internal sealed class ValidationCollector
{
    private readonly List<ArtifactValidationIssue> _issues = [];

    public bool HasIssues => _issues.Count > 0;

    public void Add(string code, string message, string? path = null)
    {
        _issues.Add(new ArtifactValidationIssue(code, message, path));
    }

    public ArtifactValidationResult ToResult() => new(_issues);
}
