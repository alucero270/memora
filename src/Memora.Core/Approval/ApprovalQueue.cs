using System.Collections.ObjectModel;
using Memora.Core.Artifacts;

namespace Memora.Core.Approval;

public sealed record ApprovalQueueItem(
    ArtifactDocument Artifact,
    DateTimeOffset PendingSinceUtc)
{
    public string ArtifactId => Artifact.Id;

    public string ProjectId => Artifact.ProjectId;

    public ArtifactType ArtifactType => Artifact.Type;

    public ArtifactStatus PendingStatus => Artifact.Status;

    public string Title => Artifact.Title;

    public int Revision => Artifact.Revision;

    public string Provenance => Artifact.Provenance;

    public string Reason => Artifact.Reason;
}

public sealed class ProjectApprovalQueue
{
    public ProjectApprovalQueue(
        string ProjectId,
        IEnumerable<ApprovalQueueItem> Items)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(ProjectId);
        ArgumentNullException.ThrowIfNull(Items);

        this.ProjectId = ProjectId;
        this.Items = new ReadOnlyCollection<ApprovalQueueItem>(Items.ToList());
    }

    public string ProjectId { get; }

    public IReadOnlyList<ApprovalQueueItem> Items { get; }
}

public sealed class ApprovalQueueBuilder
{
    public ProjectApprovalQueue Build(
        string projectId,
        IEnumerable<ArtifactDocument> artifacts)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(projectId);
        ArgumentNullException.ThrowIfNull(artifacts);

        var items = artifacts
            .Where(artifact => artifact.ProjectId == projectId)
            .Where(IsPendingReviewStatus)
            .OrderBy(GetStatusRank)
            .ThenBy(artifact => artifact.UpdatedAtUtc)
            .ThenBy(artifact => artifact.CreatedAtUtc)
            .ThenBy(artifact => artifact.Id, StringComparer.Ordinal)
            .Select(artifact => new ApprovalQueueItem(artifact, artifact.UpdatedAtUtc))
            .ToArray();

        return new ProjectApprovalQueue(projectId, items);
    }

    private static bool IsPendingReviewStatus(ArtifactDocument artifact) =>
        artifact.Status is ArtifactStatus.Proposed or ArtifactStatus.Draft;

    private static int GetStatusRank(ArtifactDocument artifact) =>
        artifact.Status switch
        {
            ArtifactStatus.Proposed => 0,
            ArtifactStatus.Draft => 1,
            _ => 2
        };
}
