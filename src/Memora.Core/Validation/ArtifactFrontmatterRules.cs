using Memora.Core.Artifacts;

namespace Memora.Core.Validation;

public static class ArtifactFrontmatterRules
{
    private static readonly IReadOnlySet<string> BaseKeys = new HashSet<string>(StringComparer.Ordinal)
    {
        "id",
        "project_id",
        "type",
        "status",
        "title",
        "created_at",
        "updated_at",
        "revision",
        "tags",
        "provenance",
        "reason",
        "links"
    };

    private static readonly IReadOnlySet<string> RelationshipKeys = new HashSet<string>(StringComparer.Ordinal)
    {
        "depends_on",
        "affects",
        "derived_from",
        "supersedes"
    };

    public static IReadOnlySet<string> GetAllowedFrontmatterKeys(ArtifactType artifactType)
    {
        var keys = new HashSet<string>(BaseKeys, StringComparer.Ordinal);

        switch (artifactType)
        {
            case ArtifactType.Plan:
                keys.Add("priority");
                keys.Add("active");
                break;
            case ArtifactType.Decision:
                keys.Add("decision_date");
                break;
            case ArtifactType.Constraint:
                keys.Add("constraint_kind");
                keys.Add("severity");
                break;
            case ArtifactType.Question:
                keys.Add("question_status");
                keys.Add("priority");
                break;
            case ArtifactType.Outcome:
                keys.Add("outcome");
                break;
            case ArtifactType.RepoStructure:
                keys.Add("snapshot_source");
                break;
            case ArtifactType.SessionSummary:
                keys.Add("session_type");
                keys.Add("canonical");
                break;
        }

        return keys;
    }

    public static IReadOnlySet<string> AllowedRelationshipKeys => RelationshipKeys;
}
