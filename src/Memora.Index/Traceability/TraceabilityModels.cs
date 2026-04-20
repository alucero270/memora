using Memora.Core.Artifacts;

namespace Memora.Index.Traceability;

public enum TraceabilityQueryKind
{
    Direct,
    Dependency,
    Impact
}

public enum TraceabilityTraversalDirection
{
    Outgoing,
    Incoming
}

public sealed record TraceabilityQuery(
    string ProjectId,
    string ArtifactId,
    TraceabilityQueryKind Kind);

public sealed record TraceabilityQueryError(
    string Code,
    string Message,
    string Path);

public sealed record TraceabilityPathSegment(
    string SourceArtifactId,
    ArtifactRelationshipKind RelationshipKind,
    string TargetArtifactId,
    TraceabilityTraversalDirection TraversalDirection);

public sealed record TraceabilityPath(
    IReadOnlyList<string> ArtifactIds,
    IReadOnlyList<TraceabilityPathSegment> Segments);

public sealed record TraceabilityQueryResult(
    TraceabilityQuery Query,
    IReadOnlyList<TraceabilityPath> Paths,
    IReadOnlyList<TraceabilityQueryError> Errors)
{
    public bool Success => Errors.Count == 0;
}
