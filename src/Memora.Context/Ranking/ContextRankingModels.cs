using Memora.Context.Models;
using Memora.Core.Artifacts;

namespace Memora.Context.Ranking;

public sealed record ContextRankingBreakdown(
    int TypePriority,
    int CanonicalStatusPriority,
    int MilestoneRelevance,
    int RelationshipProximity,
    int RecencyPriority,
    int DirectMatchStrength);

public sealed record RankedContextArtifact(
    ContextBundleArtifact Artifact,
    ContextRankingBreakdown Breakdown,
    IReadOnlyList<ContextRelationshipTraversalPath>? RelationshipPaths = null)
{
    public IReadOnlyList<ContextRelationshipTraversalPath> RelationshipPaths { get; } =
        RelationshipPaths?.ToArray() ?? [];
}

public enum ContextRelationshipTraversalDirection
{
    Outgoing,
    Incoming
}

public sealed record ContextRelationshipTraversalSegment(
    string SourceArtifactId,
    ArtifactRelationshipKind RelationshipKind,
    string TargetArtifactId,
    ContextRelationshipTraversalDirection Direction);

public sealed record ContextRelationshipTraversalPath(
    string FocusArtifactId,
    IReadOnlyList<string> ArtifactIds,
    IReadOnlyList<ContextRelationshipTraversalSegment> Segments)
{
    public string FocusArtifactId { get; } = FocusArtifactId;
    public IReadOnlyList<string> ArtifactIds { get; } = ArtifactIds?.ToArray() ?? throw new ArgumentNullException(nameof(ArtifactIds));
    public IReadOnlyList<ContextRelationshipTraversalSegment> Segments { get; } = Segments?.ToArray() ?? throw new ArgumentNullException(nameof(Segments));
    public int Depth => Segments.Count;
}
