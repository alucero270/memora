using Memora.Core.Planning;

namespace Memora.Core.Tests.Planning;

public sealed class PlanningIntakeValidatorTests
{
    private readonly PlanningIntakeValidator _validator = new();

    [Fact]
    public void ValidIntake_PassesValidation()
    {
        var intake = PlanningIntakeTestBuilder.CreateValidIntake();

        var result = _validator.Validate(intake);

        Assert.True(result.IsValid);
        Assert.Empty(result.Issues);
    }

    [Fact]
    public void MissingProjectScope_FailsValidation()
    {
        var intake = PlanningIntakeTestBuilder.CreateValidIntake() with
        {
            Project = new PlanningProjectScope("", "")
        };

        var result = _validator.Validate(intake);

        Assert.False(result.IsValid);
        Assert.Contains(result.Issues, issue => issue.Path == "project.project_id" && issue.Code == "planning_intake.project_id.required");
        Assert.Contains(result.Issues, issue => issue.Path == "project.name" && issue.Code == "planning_intake.project_name.required");
    }

    [Fact]
    public void NonUtcImportedTimestamp_FailsValidation()
    {
        var intake = PlanningIntakeTestBuilder.CreateValidIntake();
        intake = intake with
        {
            Session = intake.Session with
            {
                ImportedAtUtc = new DateTimeOffset(2026, 04, 15, 12, 30, 00, TimeSpan.FromHours(2))
            }
        };

        var result = _validator.Validate(intake);

        Assert.False(result.IsValid);
        Assert.Contains(result.Issues, issue => issue.Path == "session.imported_at_utc" && issue.Code == "planning_intake.session.imported_at.invalid");
    }

    [Fact]
    public void MissingPlanAcceptanceCriteria_FailsValidation()
    {
        var intake = PlanningIntakeTestBuilder.CreateValidIntake();
        intake = intake with
        {
            Plans =
            [
                intake.Plans[0] with
                {
                    AcceptanceCriteria = []
                }
            ]
        };

        var result = _validator.Validate(intake);

        Assert.False(result.IsValid);
        Assert.Contains(result.Issues, issue => issue.Path == "plans[0].acceptance_criteria" && issue.Code == "planning_intake.plan.acceptance_criteria.required");
    }

    [Fact]
    public void ResolvedQuestionWithoutResolution_FailsValidation()
    {
        var intake = PlanningIntakeTestBuilder.CreateValidIntake();
        intake = intake with
        {
            Questions =
            [
                intake.Questions[0] with
                {
                    Status = Memora.Core.Artifacts.QuestionStatus.Resolved,
                    Resolution = ""
                }
            ]
        };

        var result = _validator.Validate(intake);

        Assert.False(result.IsValid);
        Assert.Contains(result.Issues, issue => issue.Path == "questions[0].resolution" && issue.Code == "planning_intake.question.resolution.required");
    }

    [Fact]
    public void IntakeWithoutDraftCandidates_FailsValidation()
    {
        var intake = PlanningIntakeTestBuilder.CreateValidIntake() with
        {
            Plans = [],
            Decisions = [],
            Constraints = [],
            Questions = []
        };

        var result = _validator.Validate(intake);

        Assert.False(result.IsValid);
        Assert.Contains(result.Issues, issue => issue.Path == "items" && issue.Code == "planning_intake.items.required");
    }
}
