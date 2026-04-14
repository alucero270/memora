namespace Memora.Core.Planning;

public sealed class PlanningIntakeValidator
{
    public PlanningIntakeValidationResult Validate(PlanningIntake intake)
    {
        ArgumentNullException.ThrowIfNull(intake);

        var collector = new PlanningIntakeValidationCollector();

        ValidateProject(intake.Project, collector);
        ValidateSession(intake.Session, collector);

        var planCount = ValidatePlans(intake.Plans, collector);
        var decisionCount = ValidateDecisions(intake.Decisions, collector);
        var constraintCount = ValidateConstraints(intake.Constraints, collector);
        var questionCount = ValidateQuestions(intake.Questions, collector);

        if (planCount + decisionCount + constraintCount + questionCount == 0)
        {
            collector.Add(
                "planning_intake.items.required",
                "Planning intake must include at least one draft candidate.",
                "items");
        }

        return collector.ToResult();
    }

    private static void ValidateProject(PlanningProjectScope? project, PlanningIntakeValidationCollector collector)
    {
        if (project is null)
        {
            collector.Add(
                "planning_intake.project.required",
                "Planning intake must include project scope information.",
                "project");
            return;
        }

        ValidateRequiredString(project.ProjectId, "planning_intake.project_id.required", "Project id is required.", "project.project_id", collector);
        ValidateRequiredString(project.Name, "planning_intake.project_name.required", "Project name is required.", "project.name", collector);
    }

    private static void ValidateSession(PlanningSession? session, PlanningIntakeValidationCollector collector)
    {
        if (session is null)
        {
            collector.Add(
                "planning_intake.session.required",
                "Planning intake must include session information.",
                "session");
            return;
        }

        ValidateRequiredString(session.SourceReference, "planning_intake.session.source.required", "Session source reference is required.", "session.source_reference", collector);
        ValidateRequiredString(session.Summary, "planning_intake.session.summary.required", "Session summary is required.", "session.summary", collector);

        if (session.ImportedAtUtc == default || session.ImportedAtUtc.Offset != TimeSpan.Zero)
        {
            collector.Add(
                "planning_intake.session.imported_at.invalid",
                "Session imported timestamp must be a non-default UTC value.",
                "session.imported_at_utc");
        }

        ValidateStringList(session.OpenThreads, "session.open_threads", collector);
        ValidateStringList(session.Tags, "session.tags", collector);
    }

    private static int ValidatePlans(IReadOnlyList<PlanDraftSeed>? plans, PlanningIntakeValidationCollector collector)
    {
        if (plans is null)
        {
            collector.Add(
                "planning_intake.plans.required",
                "Plans collection is required.",
                "plans");
            return 0;
        }

        for (var index = 0; index < plans.Count; index++)
        {
            ValidatePlan(plans[index], index, collector);
        }

        return plans.Count;
    }

    private static void ValidatePlan(PlanDraftSeed? plan, int index, PlanningIntakeValidationCollector collector)
    {
        var path = $"plans[{index}]";

        if (plan is null)
        {
            collector.Add(
                "planning_intake.plan.required",
                "Plan candidate cannot be null.",
                path);
            return;
        }

        ValidateRequiredString(plan.Title, "planning_intake.plan.title.required", "Plan title is required.", $"{path}.title", collector);
        ValidateRequiredString(plan.Goal, "planning_intake.plan.goal.required", "Plan goal is required.", $"{path}.goal", collector);
        ValidateRequiredString(plan.Scope, "planning_intake.plan.scope.required", "Plan scope is required.", $"{path}.scope", collector);
        ValidateRequiredString(plan.Notes, "planning_intake.plan.notes.required", "Plan notes are required.", $"{path}.notes", collector);
        ValidateRequiredString(plan.Reason, "planning_intake.plan.reason.required", "Plan reason is required.", $"{path}.reason", collector);
        ValidateRequiredStringList(plan.AcceptanceCriteria, $"{path}.acceptance_criteria", "planning_intake.plan.acceptance_criteria.required", "Plan acceptance criteria must include at least one non-empty item.", collector);
        ValidateStringList(plan.Tags, $"{path}.tags", collector);
    }

    private static int ValidateDecisions(IReadOnlyList<DecisionDraftSeed>? decisions, PlanningIntakeValidationCollector collector)
    {
        if (decisions is null)
        {
            collector.Add(
                "planning_intake.decisions.required",
                "Decisions collection is required.",
                "decisions");
            return 0;
        }

        for (var index = 0; index < decisions.Count; index++)
        {
            ValidateDecision(decisions[index], index, collector);
        }

        return decisions.Count;
    }

    private static void ValidateDecision(DecisionDraftSeed? decision, int index, PlanningIntakeValidationCollector collector)
    {
        var path = $"decisions[{index}]";

        if (decision is null)
        {
            collector.Add(
                "planning_intake.decision.required",
                "Decision candidate cannot be null.",
                path);
            return;
        }

        ValidateRequiredString(decision.Title, "planning_intake.decision.title.required", "Decision title is required.", $"{path}.title", collector);
        ValidateRequiredString(decision.Context, "planning_intake.decision.context.required", "Decision context is required.", $"{path}.context", collector);
        ValidateRequiredString(decision.Decision, "planning_intake.decision.body.required", "Decision content is required.", $"{path}.decision", collector);
        ValidateRequiredString(decision.AlternativesConsidered, "planning_intake.decision.alternatives.required", "Decision alternatives are required.", $"{path}.alternatives_considered", collector);
        ValidateRequiredString(decision.Consequences, "planning_intake.decision.consequences.required", "Decision consequences are required.", $"{path}.consequences", collector);
        ValidateRequiredString(decision.Reason, "planning_intake.decision.reason.required", "Decision reason is required.", $"{path}.reason", collector);

        if (decision.DecisionDate == default)
        {
            collector.Add(
                "planning_intake.decision.date.required",
                "Decision date is required.",
                $"{path}.decision_date");
        }

        ValidateStringList(decision.Tags, $"{path}.tags", collector);
    }

    private static int ValidateConstraints(IReadOnlyList<ConstraintDraftSeed>? constraints, PlanningIntakeValidationCollector collector)
    {
        if (constraints is null)
        {
            collector.Add(
                "planning_intake.constraints.required",
                "Constraints collection is required.",
                "constraints");
            return 0;
        }

        for (var index = 0; index < constraints.Count; index++)
        {
            ValidateConstraint(constraints[index], index, collector);
        }

        return constraints.Count;
    }

    private static void ValidateConstraint(ConstraintDraftSeed? constraint, int index, PlanningIntakeValidationCollector collector)
    {
        var path = $"constraints[{index}]";

        if (constraint is null)
        {
            collector.Add(
                "planning_intake.constraint.required",
                "Constraint candidate cannot be null.",
                path);
            return;
        }

        ValidateRequiredString(constraint.Title, "planning_intake.constraint.title.required", "Constraint title is required.", $"{path}.title", collector);
        ValidateRequiredString(constraint.Constraint, "planning_intake.constraint.body.required", "Constraint body is required.", $"{path}.constraint", collector);
        ValidateRequiredString(constraint.WhyItExists, "planning_intake.constraint.reasoning.required", "Constraint justification is required.", $"{path}.why_it_exists", collector);
        ValidateRequiredString(constraint.Implications, "planning_intake.constraint.implications.required", "Constraint implications are required.", $"{path}.implications", collector);
        ValidateRequiredString(constraint.Reason, "planning_intake.constraint.reason.required", "Constraint reason is required.", $"{path}.reason", collector);
        ValidateStringList(constraint.Tags, $"{path}.tags", collector);
    }

    private static int ValidateQuestions(IReadOnlyList<QuestionDraftSeed>? questions, PlanningIntakeValidationCollector collector)
    {
        if (questions is null)
        {
            collector.Add(
                "planning_intake.questions.required",
                "Questions collection is required.",
                "questions");
            return 0;
        }

        for (var index = 0; index < questions.Count; index++)
        {
            ValidateQuestion(questions[index], index, collector);
        }

        return questions.Count;
    }

    private static void ValidateQuestion(QuestionDraftSeed? question, int index, PlanningIntakeValidationCollector collector)
    {
        var path = $"questions[{index}]";

        if (question is null)
        {
            collector.Add(
                "planning_intake.question.required",
                "Question candidate cannot be null.",
                path);
            return;
        }

        ValidateRequiredString(question.Title, "planning_intake.question.title.required", "Question title is required.", $"{path}.title", collector);
        ValidateRequiredString(question.Question, "planning_intake.question.body.required", "Question content is required.", $"{path}.question", collector);
        ValidateRequiredString(question.Context, "planning_intake.question.context.required", "Question context is required.", $"{path}.context", collector);
        ValidateRequiredString(question.PossibleDirections, "planning_intake.question.directions.required", "Question possible directions are required.", $"{path}.possible_directions", collector);
        ValidateRequiredString(question.Reason, "planning_intake.question.reason.required", "Question reason is required.", $"{path}.reason", collector);

        if (question.Status is not Memora.Core.Artifacts.QuestionStatus.Open &&
            string.IsNullOrWhiteSpace(question.Resolution))
        {
            collector.Add(
                "planning_intake.question.resolution.required",
                "Resolved or deferred questions must include a resolution.",
                $"{path}.resolution");
        }

        ValidateStringList(question.Tags, $"{path}.tags", collector);
    }

    private static void ValidateRequiredString(
        string? value,
        string code,
        string message,
        string path,
        PlanningIntakeValidationCollector collector)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            collector.Add(code, message, path);
        }
    }

    private static void ValidateRequiredStringList(
        IReadOnlyList<string>? values,
        string path,
        string code,
        string message,
        PlanningIntakeValidationCollector collector)
    {
        if (values is null || values.Count == 0)
        {
            collector.Add(code, message, path);
            return;
        }

        var hasNonEmptyValue = false;

        for (var index = 0; index < values.Count; index++)
        {
            if (string.IsNullOrWhiteSpace(values[index]))
            {
                collector.Add(
                    "planning_intake.list_item.invalid",
                    "List values must be non-empty strings.",
                    $"{path}[{index}]");
                continue;
            }

            hasNonEmptyValue = true;
        }

        if (!hasNonEmptyValue)
        {
            collector.Add(code, message, path);
        }
    }

    private static void ValidateStringList(
        IReadOnlyList<string>? values,
        string path,
        PlanningIntakeValidationCollector collector)
    {
        if (values is null)
        {
            collector.Add(
                "planning_intake.list.required",
                "List values are required.",
                path);
            return;
        }

        for (var index = 0; index < values.Count; index++)
        {
            if (string.IsNullOrWhiteSpace(values[index]))
            {
                collector.Add(
                    "planning_intake.list_item.invalid",
                    "List values must be non-empty strings.",
                    $"{path}[{index}]");
            }
        }
    }
}
