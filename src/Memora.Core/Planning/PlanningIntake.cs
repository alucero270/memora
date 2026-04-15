using Memora.Core.Artifacts;

namespace Memora.Core.Planning;

public sealed record PlanningIntake(
    PlanningProjectScope Project,
    PlanningSession Session,
    IReadOnlyList<PlanDraftSeed> Plans,
    IReadOnlyList<DecisionDraftSeed> Decisions,
    IReadOnlyList<ConstraintDraftSeed> Constraints,
    IReadOnlyList<QuestionDraftSeed> Questions);

public sealed record PlanningProjectScope(
    string ProjectId,
    string Name);

public sealed record PlanningSession(
    string SourceReference,
    DateTimeOffset ImportedAtUtc,
    string Summary,
    IReadOnlyList<string> OpenThreads,
    IReadOnlyList<string> Tags);

public sealed record PlanDraftSeed(
    string Title,
    string Goal,
    string Scope,
    IReadOnlyList<string> AcceptanceCriteria,
    string Notes,
    string Reason,
    ArtifactPriority Priority,
    bool Active,
    IReadOnlyList<string> Tags);

public sealed record DecisionDraftSeed(
    string Title,
    string Context,
    string Decision,
    string AlternativesConsidered,
    string Consequences,
    string Reason,
    DateOnly DecisionDate,
    IReadOnlyList<string> Tags);

public sealed record ConstraintDraftSeed(
    string Title,
    string Constraint,
    string WhyItExists,
    string Implications,
    string Reason,
    ConstraintKind Kind,
    ConstraintSeverity Severity,
    IReadOnlyList<string> Tags);

public sealed record QuestionDraftSeed(
    string Title,
    string Question,
    string Context,
    string PossibleDirections,
    string? Resolution,
    string Reason,
    QuestionStatus Status,
    ArtifactPriority Priority,
    IReadOnlyList<string> Tags);
