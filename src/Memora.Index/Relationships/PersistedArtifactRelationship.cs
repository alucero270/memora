using Memora.Core.Artifacts;

namespace Memora.Index.Relationships;

public sealed record PersistedArtifactRelationship(
    string ProjectId,
    string SourceArtifactId,
    int SourceRevision,
    ArtifactRelationshipKind Kind,
    string TargetArtifactId);
