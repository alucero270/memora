using Memora.Core.Artifacts;

namespace Memora.Context.Models;

public sealed record ContextBundleRequest
{
    public ContextBundleRequest(
        string projectId,
        string taskDescription,
        bool includeDraftArtifacts = false,
        bool includeLayer3History = false,
        IReadOnlyList<string>? focusArtifactIds = null,
        IReadOnlyList<string>? focusTags = null,
        int maxLayer2Artifacts = 10,
        int maxLayer3Artifacts = 10)
    {
        if (string.IsNullOrWhiteSpace(projectId))
        {
            throw new ArgumentException("Project id is required.", nameof(projectId));
        }

        if (string.IsNullOrWhiteSpace(taskDescription))
        {
            throw new ArgumentException("Task description is required.", nameof(taskDescription));
        }

        if (maxLayer2Artifacts <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(maxLayer2Artifacts), "Layer 2 artifact limit must be greater than zero.");
        }

        if (maxLayer3Artifacts <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(maxLayer3Artifacts), "Layer 3 artifact limit must be greater than zero.");
        }

        ProjectId = projectId.Trim();
        TaskDescription = taskDescription.Trim();
        IncludeDraftArtifacts = includeDraftArtifacts;
        IncludeLayer3History = includeLayer3History;
        FocusArtifactIds = NormalizeValues(focusArtifactIds);
        FocusTags = NormalizeValues(focusTags);
        MaxLayer2Artifacts = maxLayer2Artifacts;
        MaxLayer3Artifacts = maxLayer3Artifacts;
    }

    public string ProjectId { get; }

    public string TaskDescription { get; }

    public bool IncludeDraftArtifacts { get; }

    public bool IncludeLayer3History { get; }

    public IReadOnlyList<string> FocusArtifactIds { get; }

    public IReadOnlyList<string> FocusTags { get; }

    public int MaxLayer2Artifacts { get; }

    public int MaxLayer3Artifacts { get; }

    internal static IReadOnlyList<string> NormalizeValues(IReadOnlyList<string>? values) =>
        values is null
            ? []
            : values
                .Where(value => !string.IsNullOrWhiteSpace(value))
                .Select(value => value.Trim())
                .Distinct(StringComparer.Ordinal)
                .OrderBy(value => value, StringComparer.Ordinal)
                .ToArray();
}

public enum ContextLayerKind
{
    Layer1,
    Layer2,
    Layer3
}

public enum ContextArtifactOrigin
{
    CanonicalApproved,
    DraftProposal,
    SessionSummary
}

public sealed record ContextInclusionReason
{
    public ContextInclusionReason(
        string code,
        string description,
        IReadOnlyList<string>? relatedArtifactIds = null)
    {
        if (string.IsNullOrWhiteSpace(code))
        {
            throw new ArgumentException("Inclusion reason code is required.", nameof(code));
        }

        if (string.IsNullOrWhiteSpace(description))
        {
            throw new ArgumentException("Inclusion reason description is required.", nameof(description));
        }

        Code = code.Trim();
        Description = description.Trim();
        RelatedArtifactIds = ContextBundleRequest.NormalizeValues(relatedArtifactIds);
    }

    public string Code { get; }

    public string Description { get; }

    public IReadOnlyList<string> RelatedArtifactIds { get; }
}

public sealed record ContextBundleArtifact
{
    public ContextBundleArtifact(
        ArtifactDocument artifact,
        ContextArtifactOrigin origin,
        IReadOnlyList<ContextInclusionReason>? inclusionReasons = null)
    {
        Artifact = artifact ?? throw new ArgumentNullException(nameof(artifact));
        Origin = origin;
        InclusionReasons = inclusionReasons?.ToArray() ?? [];

        ValidateOrigin(artifact, origin);
    }

    public ArtifactDocument Artifact { get; }

    public ContextArtifactOrigin Origin { get; }

    public IReadOnlyList<ContextInclusionReason> InclusionReasons { get; }

    public bool IsCanonicalTruth => Origin == ContextArtifactOrigin.CanonicalApproved;

    private static void ValidateOrigin(ArtifactDocument artifact, ContextArtifactOrigin origin)
    {
        switch (origin)
        {
            case ContextArtifactOrigin.CanonicalApproved when artifact.Status != ArtifactStatus.Approved:
                throw new ArgumentException(
                    "Canonical context artifacts must come from approved artifacts.",
                    nameof(origin));
            case ContextArtifactOrigin.DraftProposal when artifact.Status == ArtifactStatus.Approved:
                throw new ArgumentException(
                    "Draft proposal context artifacts cannot use approved artifact status.",
                    nameof(origin));
            case ContextArtifactOrigin.SessionSummary when artifact is not SessionSummaryArtifact:
                throw new ArgumentException(
                    "Session summary context artifacts must use a session summary artifact.",
                    nameof(origin));
        }
    }
}

public sealed record ContextBundleLayer
{
    public ContextBundleLayer(ContextLayerKind kind, IReadOnlyList<ContextBundleArtifact>? artifacts = null)
    {
        Kind = kind;
        Artifacts = artifacts?.ToArray() ?? [];
    }

    public ContextLayerKind Kind { get; }

    public IReadOnlyList<ContextBundleArtifact> Artifacts { get; }
}

public sealed record ContextBundle
{
    public ContextBundle(ContextBundleRequest request, IReadOnlyList<ContextBundleLayer> layers)
    {
        Request = request ?? throw new ArgumentNullException(nameof(request));
        Layers = layers?.ToArray() ?? throw new ArgumentNullException(nameof(layers));

        var entries = Layers
            .SelectMany(layer => layer.Artifacts)
            .ToArray();

        foreach (var entry in entries)
        {
            if (!string.Equals(entry.Artifact.ProjectId, request.ProjectId, StringComparison.Ordinal))
            {
                throw new ArgumentException(
                    $"Artifact '{entry.Artifact.Id}' belongs to project '{entry.Artifact.ProjectId}' instead of '{request.ProjectId}'.",
                    nameof(layers));
            }
        }

        var duplicateRevisions = entries
            .GroupBy(entry => (entry.Artifact.Id, entry.Artifact.Revision))
            .Where(group => group.Count() > 1)
            .Select(group => $"{group.Key.Id}.r{group.Key.Revision:D4}")
            .ToArray();

        if (duplicateRevisions.Length > 0)
        {
            throw new ArgumentException(
                $"Context bundles cannot contain duplicate artifact revisions: {string.Join(", ", duplicateRevisions)}.",
                nameof(layers));
        }
    }

    public ContextBundleRequest Request { get; }

    public IReadOnlyList<ContextBundleLayer> Layers { get; }

    public int ArtifactCount => Layers.Sum(layer => layer.Artifacts.Count);

    public IReadOnlyList<ContextBundleArtifact> Artifacts =>
        Layers.SelectMany(layer => layer.Artifacts).ToArray();
}
