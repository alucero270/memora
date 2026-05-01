using System.Text.Json;
using Memora.Core.AgentInteraction;
using Memora.Core.Artifacts;

namespace Memora.Core.Tests.AgentInteraction;

public sealed class ProjectStateViewSerializerTests
{
    [Fact]
    public void Normalize_SortsArtifactCollectionsForStableStateViewOutput()
    {
        var bundle = CreateBundle();

        var normalized = ProjectStateViewSerializer.Normalize(bundle);
        var artifact = Assert.IsType<ProjectCharterArtifact>(normalized.Layers[0].Artifacts[0].Artifact);

        Assert.Equal(["alpha", "zeta"], artifact.Tags);
        Assert.Equal(
            [ArtifactRelationshipKind.Affects, ArtifactRelationshipKind.DerivedFrom],
            artifact.Links.Relationships.Select(relationship => relationship.Kind).ToArray());
        Assert.Equal(
            ["ADR-002", "CHR-000"],
            artifact.Links.Relationships.Select(relationship => relationship.TargetArtifactId).ToArray());
        Assert.Equal(["Decision", "Problem Statement"], artifact.Sections.Keys.ToArray());
    }

    [Fact]
    public void Serialize_UsesNormalizedBundleShape()
    {
        var json = ProjectStateViewSerializer.Serialize(CreateBundle());
        using var document = JsonDocument.Parse(json);

        var artifact = document.RootElement
            .GetProperty("layers")[0]
            .GetProperty("artifacts")[0]
            .GetProperty("artifact");

        Assert.Equal(
            ["alpha", "zeta"],
            artifact.GetProperty("tags").EnumerateArray().Select(item => item.GetString() ?? string.Empty).ToArray());
        Assert.Equal(
            ["Decision", "Problem Statement"],
            artifact.GetProperty("sections").EnumerateObject().Select(property => property.Name).ToArray());
        Assert.False(document.RootElement.TryGetProperty("errors", out _));
    }

    private static AgentContextBundle CreateBundle() =>
        new(
            new GetContextRequest(
                "memora",
                "Prepare deterministic project state.",
                focusArtifactIds: ["ADR-002"],
                focusTags: ["m9"]),
            [
                new AgentContextLayer(
                    AgentContextLayerKind.Layer1,
                    [
                        new AgentContextArtifact(
                            new ProjectCharterArtifact(
                                "CHR-001",
                                "memora",
                                ArtifactStatus.Approved,
                                "Memora charter",
                                new DateTimeOffset(2026, 4, 24, 8, 0, 0, TimeSpan.Zero),
                                new DateTimeOffset(2026, 4, 24, 8, 5, 0, TimeSpan.Zero),
                                1,
                                ["zeta", "alpha"],
                                "user",
                                "seed",
                                new ArtifactLinks(
                                    [
                                        new ArtifactRelationship(ArtifactRelationshipKind.DerivedFrom, "CHR-000"),
                                        new ArtifactRelationship(ArtifactRelationshipKind.Affects, "ADR-002")
                                    ]),
                                """
                                ## Problem Statement
                                Need deterministic context.

                                ## Decision
                                Reuse the existing state view.
                                """,
                                new Dictionary<string, string>(StringComparer.Ordinal)
                                {
                                    ["Problem Statement"] = "Need deterministic context.",
                                    ["Decision"] = "Reuse the existing state view."
                                }),
                            [
                                new AgentContextInclusionReason(
                                    "approved-default",
                                    "Included because approved artifacts are the default context grounding in v1.",
                                    [])
                            ])
                    ])
            ]);
}
