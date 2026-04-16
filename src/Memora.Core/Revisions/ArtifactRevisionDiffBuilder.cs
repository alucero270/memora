using Memora.Core.Artifacts;
using Memora.Core.Validation;

namespace Memora.Core.Revisions;

public sealed class ArtifactRevisionDiffBuilder
{
    public ArtifactRevisionDiffResult Build(
        ArtifactDocument currentApprovedArtifact,
        ArtifactDocument candidateArtifact)
    {
        ArgumentNullException.ThrowIfNull(currentApprovedArtifact);
        ArgumentNullException.ThrowIfNull(candidateArtifact);

        var collector = new ValidationCollector();

        if (currentApprovedArtifact.Status != ArtifactStatus.Approved)
        {
            collector.Add(
                "revision_diff.current.status.invalid",
                "Current artifact must be approved to build a revision diff.",
                "current.status");
        }

        if (!string.Equals(currentApprovedArtifact.Id, candidateArtifact.Id, StringComparison.Ordinal))
        {
            collector.Add(
                "revision_diff.id.mismatch",
                "Revision diffs require both artifacts to have the same id.",
                "id");
        }

        if (!string.Equals(currentApprovedArtifact.ProjectId, candidateArtifact.ProjectId, StringComparison.Ordinal))
        {
            collector.Add(
                "revision_diff.project_id.mismatch",
                "Revision diffs require both artifacts to have the same project id.",
                "project_id");
        }

        if (currentApprovedArtifact.Type != candidateArtifact.Type)
        {
            collector.Add(
                "revision_diff.type.mismatch",
                "Revision diffs require both artifacts to have the same type.",
                "type");
        }

        if (candidateArtifact.Revision <= currentApprovedArtifact.Revision)
        {
            collector.Add(
                "revision_diff.revision.invalid",
                "Candidate artifact revision must be greater than the current approved revision.",
                "revision");
        }

        if (collector.HasIssues)
        {
            return new ArtifactRevisionDiffResult(null, collector.ToResult());
        }

        var changes = BuildValueMap(currentApprovedArtifact)
            .Union(BuildValueMap(candidateArtifact), StringComparer.Ordinal)
            .Distinct(StringComparer.Ordinal)
            .Select(path => CreateChange(
                path,
                GetValue(currentApprovedArtifact, path),
                GetValue(candidateArtifact, path)))
            .Where(change => change is not null)
            .Cast<ArtifactFieldChange>()
            .OrderBy(change => change.Path, StringComparer.Ordinal)
            .ToArray();

        return new ArtifactRevisionDiffResult(
            new ArtifactRevisionDiff(currentApprovedArtifact, candidateArtifact, changes),
            ArtifactValidationResult.Success);
    }

    private static IEnumerable<string> BuildValueMap(ArtifactDocument artifact)
    {
        yield return "title";
        yield return "reason";
        yield return "provenance";
        yield return "tags";

        foreach (var section in artifact.Sections.Keys.OrderBy(key => key, StringComparer.Ordinal))
        {
            yield return $"sections.{section}";
        }

        foreach (var key in ArtifactLinks.FrontmatterKeys.OrderBy(key => key, StringComparer.Ordinal))
        {
            yield return $"links.{key}";
        }

        foreach (var key in GetTypeSpecificValues(artifact).Keys.OrderBy(key => key, StringComparer.Ordinal))
        {
            yield return $"type_specific.{key}";
        }
    }

    private static ArtifactFieldChange? CreateChange(
        string path,
        string? beforeValue,
        string? afterValue)
    {
        if (string.Equals(beforeValue, afterValue, StringComparison.Ordinal))
        {
            return null;
        }

        if (beforeValue is null)
        {
            return new ArtifactFieldChange(path, null, afterValue, ArtifactFieldChangeKind.Added);
        }

        if (afterValue is null)
        {
            return new ArtifactFieldChange(path, beforeValue, null, ArtifactFieldChangeKind.Removed);
        }

        return new ArtifactFieldChange(path, beforeValue, afterValue, ArtifactFieldChangeKind.Modified);
    }

    private static string? GetValue(ArtifactDocument artifact, string path)
    {
        return path switch
        {
            "title" => artifact.Title,
            "reason" => artifact.Reason,
            "provenance" => artifact.Provenance,
            "tags" => FormatList(artifact.Tags),
            _ when path.StartsWith("sections.", StringComparison.Ordinal) => GetSectionValue(artifact, path),
            _ when path.StartsWith("links.", StringComparison.Ordinal) => GetLinkValue(artifact, path),
            _ when path.StartsWith("type_specific.", StringComparison.Ordinal) => GetTypeSpecificValue(artifact, path),
            _ => null
        };
    }

    private static string? GetSectionValue(ArtifactDocument artifact, string path)
    {
        var key = path["sections.".Length..];
        return artifact.Sections.TryGetValue(key, out var value) ? value : null;
    }

    private static string? GetLinkValue(ArtifactDocument artifact, string path)
    {
        var key = path["links.".Length..];

        if (!ArtifactLinks.TryParseKind(key, out var kind))
        {
            return null;
        }

        var values = artifact.Links
            .GetTargetArtifactIds(kind)
            .OrderBy(value => value, StringComparer.Ordinal)
            .ToArray();

        return values.Length == 0 ? null : FormatList(values);
    }

    private static string? GetTypeSpecificValue(ArtifactDocument artifact, string path)
    {
        var key = path["type_specific.".Length..];
        return GetTypeSpecificValues(artifact).TryGetValue(key, out var value) ? value : null;
    }

    private static IReadOnlyDictionary<string, string> GetTypeSpecificValues(ArtifactDocument artifact)
    {
        return artifact switch
        {
            PlanArtifact plan => new Dictionary<string, string>(StringComparer.Ordinal)
            {
                ["priority"] = plan.Priority.ToSchemaValue(),
                ["active"] = plan.Active ? "true" : "false"
            },
            ArchitectureDecisionArtifact decision => new Dictionary<string, string>(StringComparer.Ordinal)
            {
                ["decision_date"] = decision.DecisionDate
            },
            ConstraintArtifact constraint => new Dictionary<string, string>(StringComparer.Ordinal)
            {
                ["constraint_kind"] = constraint.ConstraintKind.ToSchemaValue(),
                ["severity"] = constraint.Severity.ToSchemaValue()
            },
            OpenQuestionArtifact question => new Dictionary<string, string>(StringComparer.Ordinal)
            {
                ["question_status"] = question.QuestionStatus.ToSchemaValue(),
                ["priority"] = question.Priority.ToSchemaValue()
            },
            OutcomeArtifact outcome => new Dictionary<string, string>(StringComparer.Ordinal)
            {
                ["outcome"] = outcome.Outcome.ToSchemaValue()
            },
            RepoStructureArtifact repoStructure => new Dictionary<string, string>(StringComparer.Ordinal)
            {
                ["snapshot_source"] = repoStructure.SnapshotSource.ToSchemaValue()
            },
            SessionSummaryArtifact sessionSummary => new Dictionary<string, string>(StringComparer.Ordinal)
            {
                ["session_type"] = sessionSummary.SessionType.ToSchemaValue(),
                ["canonical"] = sessionSummary.Canonical ? "true" : "false"
            },
            _ => new Dictionary<string, string>(StringComparer.Ordinal)
        };
    }

    private static string FormatList(IEnumerable<string> values) =>
        string.Join(", ", values.OrderBy(value => value, StringComparer.Ordinal));
}
