using System.Text.Json;
using Memora.Core.Artifacts;

namespace Memora.Core.AgentInteraction;

public static class ProjectStateViewSerializer
{
    private static readonly JsonSerializerOptions SerializerOptions = new(JsonSerializerDefaults.Web);

    public static AgentContextBundle Normalize(AgentContextBundle bundle)
    {
        ArgumentNullException.ThrowIfNull(bundle);

        return new AgentContextBundle(
            new GetContextRequest(
                bundle.Request.ProjectId,
                bundle.Request.TaskDescription,
                bundle.Request.IncludeDraftArtifacts,
                bundle.Request.IncludeLayer3History,
                bundle.Request.FocusArtifactIds,
                bundle.Request.FocusTags,
                bundle.Request.MaxLayer2Artifacts,
                bundle.Request.MaxLayer3Artifacts),
            bundle.Layers
                .OrderBy(layer => layer.Kind)
                .Select(layer => new AgentContextLayer(
                    layer.Kind,
                    layer.Artifacts
                        .Select(artifact => new AgentContextArtifact(
                            NormalizeArtifact(artifact.Artifact),
                            artifact.InclusionReasons
                                .Select(reason => new AgentContextInclusionReason(
                                    reason.Code,
                                    reason.Description,
                                    reason.RelatedArtifactIds))
                                .ToArray()))
                        .ToArray()))
                .ToArray());
    }

    public static string Serialize(AgentContextBundle bundle) =>
        JsonSerializer.Serialize(Normalize(bundle), SerializerOptions);

    private static ArtifactDocument NormalizeArtifact(ArtifactDocument artifact)
    {
        ArgumentNullException.ThrowIfNull(artifact);

        var tags = artifact.Tags
            .OrderBy(tag => tag, StringComparer.Ordinal)
            .ToArray();
        var links = new ArtifactLinks(
            artifact.Links.Relationships
                .OrderBy(relationship => relationship.Kind)
                .ThenBy(relationship => relationship.TargetArtifactId, StringComparer.Ordinal)
                .Select(relationship => new ArtifactRelationship(
                    relationship.Kind,
                    relationship.TargetArtifactId))
                .ToArray());
        var sections = artifact.Sections
            .OrderBy(pair => pair.Key, StringComparer.Ordinal)
            .ToDictionary(pair => pair.Key, pair => pair.Value, StringComparer.Ordinal);

        return artifact switch
        {
            ProjectCharterArtifact charter => charter with
            {
                Tags = tags,
                Links = links,
                Sections = sections
            },
            PlanArtifact plan => plan with
            {
                Tags = tags,
                Links = links,
                Sections = sections
            },
            ArchitectureDecisionArtifact decision => decision with
            {
                Tags = tags,
                Links = links,
                Sections = sections
            },
            ConstraintArtifact constraint => constraint with
            {
                Tags = tags,
                Links = links,
                Sections = sections
            },
            OpenQuestionArtifact question => question with
            {
                Tags = tags,
                Links = links,
                Sections = sections
            },
            OutcomeArtifact outcome => outcome with
            {
                Tags = tags,
                Links = links,
                Sections = sections
            },
            RepoStructureArtifact repoStructure => repoStructure with
            {
                Tags = tags,
                Links = links,
                Sections = sections
            },
            SessionSummaryArtifact sessionSummary => sessionSummary with
            {
                Tags = tags,
                Links = links,
                Sections = sections
            },
            _ => throw new ArgumentOutOfRangeException(nameof(artifact), artifact.GetType(), "Unsupported artifact type.")
        };
    }
}
