using Memora.Index.Traceability;

namespace Memora.Ui.Understanding;

public sealed record UnderstandingRequest(
    string ProjectId,
    string TaskDescription,
    string? ArtifactId,
    TraceabilityQueryKind TraceabilityKind,
    bool IncludeDraftArtifacts,
    bool IncludeLayer3History);

public sealed record UnderstandingPageModel(
    string? ProjectId,
    string? TaskDescription,
    string? ArtifactId,
    TraceabilityQueryKind TraceabilityKind,
    bool IncludeDraftArtifacts,
    bool IncludeLayer3History,
    string? ErrorMessage,
    ContextUnderstandingView? ContextView,
    TraceabilityUnderstandingView? TraceabilityView,
    ComponentUnderstandingSummary? ComponentSummary);

public sealed record ContextUnderstandingView(
    int ArtifactCount,
    IReadOnlyList<ContextUnderstandingLayer> Layers);

public sealed record ContextUnderstandingLayer(
    string Name,
    IReadOnlyList<ContextUnderstandingArtifact> Artifacts);

public sealed record ContextUnderstandingArtifact(
    string Id,
    string Title,
    string Type,
    string Status,
    string Origin,
    IReadOnlyList<string> InclusionReasons);

public sealed record TraceabilityUnderstandingView(
    string ArtifactId,
    string QueryKind,
    IReadOnlyList<TraceabilityUnderstandingPath> Paths,
    IReadOnlyList<TraceabilityQueryError> Errors);

public sealed record TraceabilityUnderstandingPath(
    int Index,
    IReadOnlyList<string> ArtifactIds,
    IReadOnlyList<TraceabilityUnderstandingSegment> Segments);

public sealed record TraceabilityUnderstandingSegment(
    string SourceArtifactId,
    string Relationship,
    string TargetArtifactId,
    string Direction);

public sealed record ComponentUnderstandingSummary(
    string ArtifactId,
    string Title,
    string Type,
    string Status,
    int Revision,
    IReadOnlyList<string> Tags,
    string Provenance,
    string Reason,
    string? ContextLayer,
    IReadOnlyList<string> ContextInclusionReasons,
    IReadOnlyList<ComponentRelationshipView> OutgoingRelationships,
    IReadOnlyList<ComponentRelationshipView> IncomingRelationships,
    IReadOnlyList<ComponentSectionView> KeySections,
    IReadOnlyList<string> TraceabilityHighlights);

public sealed record ComponentRelationshipView(
    string ArtifactId,
    string Relationship,
    string Direction);

public sealed record ComponentSectionView(
    string Name,
    string Content);
