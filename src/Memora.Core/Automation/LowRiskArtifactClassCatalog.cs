using Memora.Core.Artifacts;

namespace Memora.Core.Automation;

public static class LowRiskArtifactClassCatalog
{
    private static readonly LowRiskArtifactClassDefinition[] Definitions =
    [
        new(
            ArtifactType.SessionSummary,
            "session-summary",
            AutomationStorageScope.Summary,
            allowsCanonicalDirectWrite: false,
            allowsApprovalBypassInV1: false,
            "Session summaries are explicitly non-canonical and live outside approved artifact truth.",
            [
                "canonical must remain false",
                "write location must be the summaries store",
                "relationships must not create new canonical claims",
                "default artifact proposal and approval paths must remain unchanged"
            ]),
        new(
            ArtifactType.RepoStructure,
            "generated-repo-structure-snapshot",
            AutomationStorageScope.Draft,
            allowsCanonicalDirectWrite: false,
            allowsApprovalBypassInV1: false,
            "Generated repo-structure snapshots describe filesystem shape and are lower risk only while staged for review.",
            [
                "snapshot_source must be generated",
                "write location must be draft storage unless explicit future approval changes are made",
                "filesystem remains the source for repo shape",
                "approved repo-structure artifacts still require lifecycle governance"
            ])
    ];

    private static readonly IReadOnlyDictionary<ArtifactType, LowRiskArtifactClassDefinition> DefinitionsByType =
        Definitions.ToDictionary(definition => definition.ArtifactType);

    public static IReadOnlyList<LowRiskArtifactClassDefinition> GetDefinitions() => Definitions;

    public static bool IsLowRiskCandidate(ArtifactType artifactType) =>
        DefinitionsByType.ContainsKey(artifactType);

    public static bool TryGetDefinition(
        ArtifactType artifactType,
        out LowRiskArtifactClassDefinition definition) =>
        DefinitionsByType.TryGetValue(artifactType, out definition!);
}

