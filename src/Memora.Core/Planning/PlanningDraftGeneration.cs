using System.Collections.ObjectModel;
using System.Globalization;
using Memora.Core.Artifacts;
using Memora.Core.Validation;

namespace Memora.Core.Planning;

public sealed class PlanningDraftGenerator
{
    private readonly ArtifactFactory _artifactFactory;
    private readonly PlanningIntakeValidator _intakeValidator;

    public PlanningDraftGenerator()
        : this(new ArtifactFactory(), new PlanningIntakeValidator())
    {
    }

    public PlanningDraftGenerator(
        ArtifactFactory artifactFactory,
        PlanningIntakeValidator intakeValidator)
    {
        _artifactFactory = artifactFactory ?? throw new ArgumentNullException(nameof(artifactFactory));
        _intakeValidator = intakeValidator ?? throw new ArgumentNullException(nameof(intakeValidator));
    }

    public PlanningDraftGenerationResult Generate(PlanningIntake intake)
    {
        ArgumentNullException.ThrowIfNull(intake);

        var intakeValidation = _intakeValidator.Validate(intake);

        if (!intakeValidation.IsValid)
        {
            return PlanningDraftGenerationResult.FromIntakeValidation(intakeValidation);
        }

        var artifactIssues = new List<ArtifactValidationIssue>();
        var draftArtifacts = new List<ArtifactDocument>();

        AddGeneratedArtifacts(draftArtifacts, artifactIssues, CreatePlanArtifacts(intake), "plans");
        AddGeneratedArtifacts(draftArtifacts, artifactIssues, CreateDecisionArtifacts(intake), "decisions");
        AddGeneratedArtifacts(draftArtifacts, artifactIssues, CreateConstraintArtifacts(intake), "constraints");
        AddGeneratedArtifacts(draftArtifacts, artifactIssues, CreateQuestionArtifacts(intake), "questions");

        SessionSummaryArtifact? sessionSummary = null;

        if (artifactIssues.Count == 0)
        {
            sessionSummary = CreateSessionSummary(intake, draftArtifacts, artifactIssues);
        }

        if (artifactIssues.Count > 0 || sessionSummary is null)
        {
            return PlanningDraftGenerationResult.FromArtifactValidation(intakeValidation, artifactIssues);
        }

        return PlanningDraftGenerationResult.Success(draftArtifacts, sessionSummary, intakeValidation, artifactIssues);
    }

    private IEnumerable<GeneratedArtifactDefinition> CreatePlanArtifacts(PlanningIntake intake)
    {
        for (var index = 0; index < intake.Plans.Count; index++)
        {
            var plan = intake.Plans[index];
            var sections = new Dictionary<string, string>(StringComparer.Ordinal)
            {
                ["Goal"] = plan.Goal,
                ["Scope"] = plan.Scope,
                ["Acceptance Criteria"] = ToBulletList(plan.AcceptanceCriteria),
                ["Notes"] = plan.Notes
            };

            yield return new GeneratedArtifactDefinition(
                index,
                sections,
                BuildFrontmatter(
                    intake,
                    ArtifactType.Plan,
                    index + 1,
                    plan.Title,
                    plan.Tags,
                    plan.Reason,
                    new KeyValuePair<string, object?>("priority", plan.Priority.ToSchemaValue()),
                    new KeyValuePair<string, object?>("active", plan.Active)));
        }
    }

    private IEnumerable<GeneratedArtifactDefinition> CreateDecisionArtifacts(PlanningIntake intake)
    {
        for (var index = 0; index < intake.Decisions.Count; index++)
        {
            var decision = intake.Decisions[index];
            var sections = new Dictionary<string, string>(StringComparer.Ordinal)
            {
                ["Context"] = decision.Context,
                ["Decision"] = decision.Decision,
                ["Alternatives Considered"] = decision.AlternativesConsidered,
                ["Consequences"] = decision.Consequences
            };

            yield return new GeneratedArtifactDefinition(
                index,
                sections,
                BuildFrontmatter(
                    intake,
                    ArtifactType.Decision,
                    index + 1,
                    decision.Title,
                    decision.Tags,
                    decision.Reason,
                    new KeyValuePair<string, object?>("decision_date", decision.DecisionDate.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture))));
        }
    }

    private IEnumerable<GeneratedArtifactDefinition> CreateConstraintArtifacts(PlanningIntake intake)
    {
        for (var index = 0; index < intake.Constraints.Count; index++)
        {
            var constraint = intake.Constraints[index];
            var sections = new Dictionary<string, string>(StringComparer.Ordinal)
            {
                ["Constraint"] = constraint.Constraint,
                ["Why It Exists"] = constraint.WhyItExists,
                ["Implications"] = constraint.Implications
            };

            yield return new GeneratedArtifactDefinition(
                index,
                sections,
                BuildFrontmatter(
                    intake,
                    ArtifactType.Constraint,
                    index + 1,
                    constraint.Title,
                    constraint.Tags,
                    constraint.Reason,
                    new KeyValuePair<string, object?>("constraint_kind", constraint.Kind.ToSchemaValue()),
                    new KeyValuePair<string, object?>("severity", constraint.Severity.ToSchemaValue())));
        }
    }

    private IEnumerable<GeneratedArtifactDefinition> CreateQuestionArtifacts(PlanningIntake intake)
    {
        for (var index = 0; index < intake.Questions.Count; index++)
        {
            var question = intake.Questions[index];
            var sections = new Dictionary<string, string>(StringComparer.Ordinal)
            {
                ["Question"] = question.Question,
                ["Context"] = question.Context,
                ["Possible Directions"] = question.PossibleDirections,
                ["Resolution"] = question.Resolution ?? string.Empty
            };

            yield return new GeneratedArtifactDefinition(
                index,
                sections,
                BuildFrontmatter(
                    intake,
                    ArtifactType.Question,
                    index + 1,
                    question.Title,
                    question.Tags,
                    question.Reason,
                    new KeyValuePair<string, object?>("question_status", question.Status.ToSchemaValue()),
                    new KeyValuePair<string, object?>("priority", question.Priority.ToSchemaValue())));
        }
    }

    private SessionSummaryArtifact? CreateSessionSummary(
        PlanningIntake intake,
        IReadOnlyList<ArtifactDocument> draftArtifacts,
        ICollection<ArtifactValidationIssue> artifactIssues)
    {
        var sections = new Dictionary<string, string>(StringComparer.Ordinal)
        {
            ["Summary"] = intake.Session.Summary,
            ["Artifacts Created"] = ToBulletList(draftArtifacts.Select(artifact => $"{artifact.Id}: {artifact.Title}")),
            ["Artifacts Updated"] = "None.",
            ["Open Threads"] = intake.Session.OpenThreads.Count == 0
                ? "None."
                : ToBulletList(intake.Session.OpenThreads)
        };

        var result = _artifactFactory.Create(
            BuildFrontmatter(
                intake,
                ArtifactType.SessionSummary,
                1,
                $"Planning intake summary - {intake.Project.Name}",
                intake.Session.Tags,
                "Supporting planning summary generated from validated planning intake.",
                new KeyValuePair<string, object?>("session_type", SessionType.Planning.ToSchemaValue()),
                new KeyValuePair<string, object?>("canonical", false)),
            BuildBody(sections),
            sections);

        if (result.Validation.IsValid && result.Artifact is SessionSummaryArtifact summaryArtifact)
        {
            return summaryArtifact;
        }

        foreach (var issue in result.Validation.Issues)
        {
            artifactIssues.Add(PrefixIssue(issue, "session_summary"));
        }

        return null;
    }

    private void AddGeneratedArtifacts(
        ICollection<ArtifactDocument> draftArtifacts,
        ICollection<ArtifactValidationIssue> artifactIssues,
        IEnumerable<GeneratedArtifactDefinition> definitions,
        string categoryPath)
    {
        foreach (var definition in definitions)
        {
            var result = _artifactFactory.Create(
                definition.Frontmatter,
                BuildBody(definition.Sections),
                definition.Sections);

            if (!result.Validation.IsValid || result.Artifact is null)
            {
                foreach (var issue in result.Validation.Issues)
                {
                    artifactIssues.Add(PrefixIssue(issue, $"{categoryPath}[{definition.Index}]"));
                }

                continue;
            }

            draftArtifacts.Add(result.Artifact);
        }
    }

    private static ArtifactValidationIssue PrefixIssue(ArtifactValidationIssue issue, string prefix)
    {
        var path = string.IsNullOrWhiteSpace(issue.Path)
            ? prefix
            : $"{prefix}.{issue.Path}";

        return issue with { Path = path };
    }

    private static Dictionary<string, object?> BuildFrontmatter(
        PlanningIntake intake,
        ArtifactType type,
        int sequence,
        string title,
        IReadOnlyList<string> tags,
        string reason,
        params KeyValuePair<string, object?>[] typeSpecificValues)
    {
        var frontmatter = new Dictionary<string, object?>(StringComparer.Ordinal)
        {
            ["id"] = CreateArtifactId(type, intake.Session.ImportedAtUtc, sequence),
            ["project_id"] = intake.Project.ProjectId,
            ["type"] = type.ToSchemaValue(),
            ["status"] = ArtifactStatus.Draft.ToSchemaValue(),
            ["title"] = title,
            ["created_at"] = intake.Session.ImportedAtUtc.ToString("yyyy-MM-ddTHH:mm:ss'Z'", CultureInfo.InvariantCulture),
            ["updated_at"] = intake.Session.ImportedAtUtc.ToString("yyyy-MM-ddTHH:mm:ss'Z'", CultureInfo.InvariantCulture),
            ["revision"] = 1,
            ["tags"] = tags.Select(tag => (object?)tag).ToList(),
            ["provenance"] = $"planning_import:{intake.Session.SourceReference}",
            ["reason"] = reason,
            ["links"] = CreateEmptyLinks()
        };

        foreach (var pair in typeSpecificValues)
        {
            frontmatter[pair.Key] = pair.Value;
        }

        return frontmatter;
    }

    private static Dictionary<string, object?> CreateEmptyLinks() =>
        new(StringComparer.Ordinal)
        {
            ["depends_on"] = new List<object?>(),
            ["affects"] = new List<object?>(),
            ["derived_from"] = new List<object?>(),
            ["supersedes"] = new List<object?>()
        };

    private static string CreateArtifactId(ArtifactType type, DateTimeOffset importedAtUtc, int sequence) =>
        $"{ArtifactIdValidator.GetPrefix(type)}-{importedAtUtc:yyyyMMddHHmmss}{sequence:D2}";

    private static string BuildBody(IReadOnlyDictionary<string, string> sections) =>
        string.Join(
            "\n\n",
            sections.Select(section => $"## {section.Key}\n{section.Value}"));

    private static string ToBulletList(IEnumerable<string> values) =>
        string.Join(
            "\n",
            values.Select(value => $"- {value}"));

    private sealed record GeneratedArtifactDefinition(
        int Index,
        IReadOnlyDictionary<string, string> Sections,
        IReadOnlyDictionary<string, object?> Frontmatter);
}

public sealed class PlanningDraftGenerationResult
{
    private PlanningDraftGenerationResult(
        IEnumerable<ArtifactDocument> draftArtifacts,
        SessionSummaryArtifact? sessionSummary,
        PlanningIntakeValidationResult intakeValidation,
        IEnumerable<ArtifactValidationIssue> artifactValidationIssues)
    {
        DraftArtifacts = new ReadOnlyCollection<ArtifactDocument>(draftArtifacts.ToList());
        SessionSummary = sessionSummary;
        IntakeValidation = intakeValidation;
        ArtifactValidationIssues = new ReadOnlyCollection<ArtifactValidationIssue>(artifactValidationIssues.ToList());
    }

    public bool IsSuccess => IntakeValidation.IsValid && ArtifactValidationIssues.Count == 0 && SessionSummary is not null;

    public IReadOnlyList<ArtifactDocument> DraftArtifacts { get; }

    public SessionSummaryArtifact? SessionSummary { get; }

    public PlanningIntakeValidationResult IntakeValidation { get; }

    public IReadOnlyList<ArtifactValidationIssue> ArtifactValidationIssues { get; }

    public static PlanningDraftGenerationResult FromIntakeValidation(PlanningIntakeValidationResult intakeValidation) =>
        new([], null, intakeValidation, []);

    public static PlanningDraftGenerationResult FromArtifactValidation(
        PlanningIntakeValidationResult intakeValidation,
        IEnumerable<ArtifactValidationIssue> issues) =>
        new([], null, intakeValidation, issues);

    public static PlanningDraftGenerationResult Success(
        IEnumerable<ArtifactDocument> draftArtifacts,
        SessionSummaryArtifact sessionSummary,
        PlanningIntakeValidationResult intakeValidation,
        IEnumerable<ArtifactValidationIssue> issues) =>
        new(draftArtifacts, sessionSummary, intakeValidation, issues);
}
