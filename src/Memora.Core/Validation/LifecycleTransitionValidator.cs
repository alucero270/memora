using Memora.Core.Artifacts;

namespace Memora.Core.Validation;

public static class LifecycleTransitionValidator
{
    private static readonly IReadOnlySet<(ArtifactStatus From, ArtifactStatus To)> AllowedTransitions =
        new HashSet<(ArtifactStatus From, ArtifactStatus To)>
        {
            (ArtifactStatus.Proposed, ArtifactStatus.Draft),
            (ArtifactStatus.Proposed, ArtifactStatus.Deprecated),
            (ArtifactStatus.Draft, ArtifactStatus.Approved),
            (ArtifactStatus.Draft, ArtifactStatus.Deprecated),
            (ArtifactStatus.Approved, ArtifactStatus.Superseded),
            (ArtifactStatus.Approved, ArtifactStatus.Deprecated)
        };

    public static ArtifactValidationResult Validate(ArtifactStatus from, ArtifactStatus to)
    {
        var collector = new ValidationCollector();

        if (!AllowedTransitions.Contains((from, to)))
        {
            collector.Add(
                "artifact.lifecycle.transition.invalid",
                $"Lifecycle transition '{from.ToSchemaValue()} -> {to.ToSchemaValue()}' is not allowed.",
                "status");
        }

        return collector.ToResult();
    }

    public static ArtifactValidationResult Validate(ArtifactDocument current, ArtifactDocument next)
    {
        ArgumentNullException.ThrowIfNull(current);
        ArgumentNullException.ThrowIfNull(next);

        var collector = new ValidationCollector();
        var transition = Validate(current.Status, next.Status);

        foreach (var issue in transition.Issues)
        {
            collector.Add(issue.Code, issue.Message, issue.Path);
        }

        if (!string.Equals(current.Id, next.Id, StringComparison.Ordinal))
        {
            collector.Add("artifact.lifecycle.id.mismatch", "Lifecycle transitions must preserve artifact identity.", "id");
        }

        if (!string.Equals(current.ProjectId, next.ProjectId, StringComparison.Ordinal))
        {
            collector.Add("artifact.lifecycle.project_id.mismatch", "Lifecycle transitions must preserve project identity.", "project_id");
        }

        if (current.Type != next.Type)
        {
            collector.Add("artifact.lifecycle.type.mismatch", "Lifecycle transitions must preserve artifact type.", "type");
        }

        if (current.Status == ArtifactStatus.Approved && next.Revision <= current.Revision)
        {
            collector.Add(
                "artifact.lifecycle.revision.required",
                "Approved artifacts cannot be overwritten without creating a new revision.",
                "revision");
        }

        return collector.ToResult();
    }
}
