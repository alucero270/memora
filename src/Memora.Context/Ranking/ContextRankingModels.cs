using Memora.Context.Models;

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
    ContextRankingBreakdown Breakdown);
