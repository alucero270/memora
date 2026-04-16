using System.Globalization;

namespace Memora.Core.Artifacts;

internal static class ArtifactDocumentRehydrator
{
    public static Dictionary<string, object?> BuildFrontmatter(ArtifactDocument artifact)
    {
        var frontmatter = new Dictionary<string, object?>(StringComparer.Ordinal)
        {
            ["id"] = artifact.Id,
            ["project_id"] = artifact.ProjectId,
            ["type"] = artifact.Type.ToSchemaValue(),
            ["status"] = artifact.Status.ToSchemaValue(),
            ["title"] = artifact.Title,
            ["created_at"] = artifact.CreatedAtUtc.ToString("yyyy-MM-ddTHH:mm:ss'Z'", CultureInfo.InvariantCulture),
            ["updated_at"] = artifact.UpdatedAtUtc.ToString("yyyy-MM-ddTHH:mm:ss'Z'", CultureInfo.InvariantCulture),
            ["revision"] = artifact.Revision,
            ["tags"] = artifact.Tags.Select(tag => (object?)tag).ToList(),
            ["provenance"] = artifact.Provenance,
            ["reason"] = artifact.Reason,
            ["links"] = BuildLinksMap(artifact.Links)
        };

        AddTypeSpecificValues(frontmatter, artifact);

        return frontmatter;
    }

    public static string BuildBody(IReadOnlyDictionary<string, string> sections) =>
        string.Join(
            "\n\n",
            sections.Select(section => $"## {section.Key}\n{section.Value}"));

    private static Dictionary<string, object?> BuildLinksMap(ArtifactLinks links)
    {
        var map = new Dictionary<string, object?>(StringComparer.Ordinal);

        foreach (var key in ArtifactLinks.FrontmatterKeys)
        {
            ArtifactLinks.TryParseKind(key, out var kind);
            map[key] = links.GetTargetArtifactIds(kind).Select(target => (object?)target).ToList();
        }

        return map;
    }

    private static void AddTypeSpecificValues(
        IDictionary<string, object?> frontmatter,
        ArtifactDocument artifact)
    {
        switch (artifact)
        {
            case PlanArtifact plan:
                frontmatter["priority"] = plan.Priority.ToSchemaValue();
                frontmatter["active"] = plan.Active;
                break;
            case ArchitectureDecisionArtifact decision:
                frontmatter["decision_date"] = decision.DecisionDate;
                break;
            case ConstraintArtifact constraint:
                frontmatter["constraint_kind"] = constraint.ConstraintKind.ToSchemaValue();
                frontmatter["severity"] = constraint.Severity.ToSchemaValue();
                break;
            case OpenQuestionArtifact question:
                frontmatter["question_status"] = question.QuestionStatus.ToSchemaValue();
                frontmatter["priority"] = question.Priority.ToSchemaValue();
                break;
            case OutcomeArtifact outcome:
                frontmatter["outcome"] = outcome.Outcome.ToSchemaValue();
                break;
            case RepoStructureArtifact repoStructure:
                frontmatter["snapshot_source"] = repoStructure.SnapshotSource.ToSchemaValue();
                break;
            case SessionSummaryArtifact sessionSummary:
                frontmatter["session_type"] = sessionSummary.SessionType.ToSchemaValue();
                frontmatter["canonical"] = sessionSummary.Canonical;
                break;
        }
    }
}
