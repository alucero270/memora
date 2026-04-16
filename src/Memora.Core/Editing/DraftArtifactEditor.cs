using System.Globalization;
using Memora.Core.Artifacts;
using Memora.Core.Validation;

namespace Memora.Core.Editing;

public sealed record DraftArtifactEditRequest(
    string? Title = null,
    string? Reason = null,
    IReadOnlyList<string>? Tags = null,
    IReadOnlyDictionary<string, string>? Sections = null,
    IReadOnlyDictionary<string, object?>? TypeSpecificValues = null);

public sealed class DraftArtifactEditor
{
    private readonly ArtifactFactory _artifactFactory;

    public DraftArtifactEditor()
        : this(new ArtifactFactory())
    {
    }

    public DraftArtifactEditor(ArtifactFactory artifactFactory)
    {
        _artifactFactory = artifactFactory ?? throw new ArgumentNullException(nameof(artifactFactory));
    }

    public DraftArtifactEditResult Edit(
        ArtifactDocument originalArtifact,
        DraftArtifactEditRequest request,
        DateTimeOffset editedAtUtc)
    {
        ArgumentNullException.ThrowIfNull(originalArtifact);
        ArgumentNullException.ThrowIfNull(request);

        var issues = new List<ArtifactValidationIssue>();

        if (originalArtifact.Status is not ArtifactStatus.Draft and not ArtifactStatus.Proposed)
        {
            issues.Add(new ArtifactValidationIssue(
                "draft_edit.status.invalid",
                "Only draft or proposed artifacts can be edited.",
                "status"));
        }

        if (editedAtUtc == default || editedAtUtc.Offset != TimeSpan.Zero)
        {
            issues.Add(new ArtifactValidationIssue(
                "draft_edit.timestamp.invalid",
                "Edited timestamp must be a non-default UTC value.",
                "updated_at"));
        }

        foreach (var issue in ValidateTypeSpecificKeys(originalArtifact.Type, request.TypeSpecificValues))
        {
            issues.Add(issue);
        }

        if (issues.Count > 0)
        {
            return new DraftArtifactEditResult(
                originalArtifact,
                null,
                new ArtifactValidationResult(issues));
        }

        var frontmatter = BuildFrontmatter(originalArtifact);
        frontmatter["title"] = request.Title ?? originalArtifact.Title;
        frontmatter["reason"] = request.Reason ?? originalArtifact.Reason;
        frontmatter["updated_at"] = editedAtUtc.ToString("yyyy-MM-ddTHH:mm:ss'Z'", CultureInfo.InvariantCulture);
        frontmatter["revision"] = originalArtifact.Revision + 1;

        if (request.Tags is not null)
        {
            frontmatter["tags"] = request.Tags.Select(tag => (object?)tag).ToList();
        }

        if (request.TypeSpecificValues is not null)
        {
            foreach (var pair in request.TypeSpecificValues)
            {
                frontmatter[pair.Key] = pair.Value;
            }
        }

        var sections = request.Sections is null
            ? new Dictionary<string, string>(originalArtifact.Sections, StringComparer.Ordinal)
            : new Dictionary<string, string>(request.Sections, StringComparer.Ordinal);

        var body = BuildBody(sections);
        var creationResult = _artifactFactory.Create(frontmatter, body, sections);

        return new DraftArtifactEditResult(
            originalArtifact,
            creationResult.Artifact,
            creationResult.Validation);
    }

    private static IEnumerable<ArtifactValidationIssue> ValidateTypeSpecificKeys(
        ArtifactType artifactType,
        IReadOnlyDictionary<string, object?>? values)
    {
        if (values is null)
        {
            yield break;
        }

        var allowedKeys = GetEditableTypeSpecificKeys(artifactType);

        foreach (var key in values.Keys)
        {
            if (!allowedKeys.Contains(key))
            {
                yield return new ArtifactValidationIssue(
                    "draft_edit.type_specific_key.invalid",
                    $"Type-specific edit key '{key}' is not editable for artifact type '{artifactType.ToSchemaValue()}'.",
                    $"type_specific.{key}");
            }
        }
    }

    private static HashSet<string> GetEditableTypeSpecificKeys(ArtifactType artifactType) =>
        artifactType switch
        {
            ArtifactType.Charter => [],
            ArtifactType.Plan => new HashSet<string>(["priority", "active"], StringComparer.Ordinal),
            ArtifactType.Decision => new HashSet<string>(["decision_date"], StringComparer.Ordinal),
            ArtifactType.Constraint => new HashSet<string>(["constraint_kind", "severity"], StringComparer.Ordinal),
            ArtifactType.Question => new HashSet<string>(["question_status", "priority"], StringComparer.Ordinal),
            ArtifactType.Outcome => new HashSet<string>(["outcome"], StringComparer.Ordinal),
            ArtifactType.RepoStructure => new HashSet<string>(["snapshot_source"], StringComparer.Ordinal),
            ArtifactType.SessionSummary => new HashSet<string>(["session_type"], StringComparer.Ordinal),
            _ => throw new ArgumentOutOfRangeException(nameof(artifactType), artifactType, "Unsupported artifact type.")
        };

    private static Dictionary<string, object?> BuildFrontmatter(ArtifactDocument artifact)
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

    private static string BuildBody(IReadOnlyDictionary<string, string> sections) =>
        string.Join(
            "\n\n",
            sections.Select(section => $"## {section.Key}\n{section.Value}"));
}

public sealed class DraftArtifactEditResult
{
    public DraftArtifactEditResult(
        ArtifactDocument originalArtifact,
        ArtifactDocument? editedArtifact,
        ArtifactValidationResult validation)
    {
        OriginalArtifact = originalArtifact ?? throw new ArgumentNullException(nameof(originalArtifact));
        EditedArtifact = editedArtifact;
        Validation = validation ?? throw new ArgumentNullException(nameof(validation));
    }

    public ArtifactDocument OriginalArtifact { get; }

    public ArtifactDocument? EditedArtifact { get; }

    public ArtifactValidationResult Validation { get; }

    public bool IsSuccess => Validation.IsValid && EditedArtifact is not null;
}
