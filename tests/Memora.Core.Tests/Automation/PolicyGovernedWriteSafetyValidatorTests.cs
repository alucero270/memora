using Memora.Core.Artifacts;
using Memora.Core.Automation;

namespace Memora.Core.Tests.Automation;

public sealed class PolicyGovernedWriteSafetyValidatorTests
{
    private readonly PolicyGovernedWriteSafetyValidator _validator = new();

    [Fact]
    public void Validate_AllowsExplicitSessionSummaryWriteWithinPolicyScope()
    {
        var request = new PolicyGovernedWriteSafetyRequest(
            "memora",
            "SUM-001",
            ArtifactType.SessionSummary,
            AutomationStorageScope.Summary,
            CreateSessionSummaryPolicy(),
            CreateExplicitTrigger("memora", ArtifactType.SessionSummary, "SUM-001"));

        var result = _validator.Validate(request);

        Assert.True(result.IsAllowed);
    }

    [Fact]
    public void Validate_BlocksProjectMismatch()
    {
        var request = new PolicyGovernedWriteSafetyRequest(
            "memora",
            "SUM-001",
            ArtifactType.SessionSummary,
            AutomationStorageScope.Summary,
            CreateSessionSummaryPolicy(),
            CreateExplicitTrigger("other", ArtifactType.SessionSummary, "SUM-001"));

        var result = _validator.Validate(request);

        Assert.False(result.IsAllowed);
        Assert.Contains(result.Issues, issue => issue.Code == "automation.write.project_id.mismatch");
    }

    [Fact]
    public void Validate_BlocksArtifactIdMismatch()
    {
        var request = new PolicyGovernedWriteSafetyRequest(
            "memora",
            "SUM-001",
            ArtifactType.SessionSummary,
            AutomationStorageScope.Summary,
            CreateSessionSummaryPolicy(),
            CreateExplicitTrigger("memora", ArtifactType.SessionSummary, "SUM-999"));

        var result = _validator.Validate(request);

        Assert.False(result.IsAllowed);
        Assert.Contains(result.Issues, issue => issue.Code == "automation.write.artifact_id.mismatch");
    }

    [Fact]
    public void Validate_BlocksLifecycleTrigger()
    {
        var request = new PolicyGovernedWriteSafetyRequest(
            "memora",
            "SUM-001",
            ArtifactType.SessionSummary,
            AutomationStorageScope.Summary,
            CreateSessionSummaryPolicy(),
            new ControlledAutomationTriggerEvent(
                "event-001",
                ControlledAutomationEventKind.ArtifactLifecycleChanged,
                "memora",
                ArtifactType.SessionSummary,
                "SUM-001",
                ArtifactStatus.Proposed,
                ArtifactStatus.Draft,
                new DateTimeOffset(2026, 4, 21, 12, 0, 0, TimeSpan.Zero)));

        var result = _validator.Validate(request);

        Assert.False(result.IsAllowed);
        Assert.Contains(result.Issues, issue => issue.Code == "automation.trigger.explicit_required");
    }

    [Fact]
    public void Validate_BlocksCanonicalScopeEvenWithMatchingPolicyAttempt()
    {
        var request = new PolicyGovernedWriteSafetyRequest(
            "memora",
            "SUM-001",
            ArtifactType.SessionSummary,
            AutomationStorageScope.Canonical,
            CreateSessionSummaryPolicy(),
            CreateExplicitTrigger("memora", ArtifactType.SessionSummary, "SUM-001"));

        var result = _validator.Validate(request);

        Assert.False(result.IsAllowed);
        Assert.Contains(result.Issues, issue => issue.Code == "automation.write.storage_scope.mismatch");
        Assert.Contains(result.Issues, issue => issue.Code == "automation.write.canonical_scope.denied");
    }

    private static ControlledAutomationPolicy CreateSessionSummaryPolicy()
    {
        Assert.True(LowRiskArtifactClassCatalog.TryGetDefinition(ArtifactType.SessionSummary, out var definition));

        return new ControlledAutomationPolicy(
            "summary-direct-write",
            "Summary direct-write prototype",
            enabled: true,
            requiresExplicitTrigger: true,
            [
                new ControlledAutomationPermission(
                    ControlledAutomationAction.DirectWrite,
                    ArtifactType.SessionSummary,
                    definition.StorageScope,
                    definition.RequiredGuardrails)
            ]);
    }

    private static ControlledAutomationTriggerEvent CreateExplicitTrigger(
        string projectId,
        ArtifactType artifactType,
        string artifactId) =>
        ControlledAutomationTriggerEvent.ExplicitOperatorRequest(
            "event-001",
            projectId,
            artifactType,
            artifactId,
            new DateTimeOffset(2026, 4, 21, 12, 0, 0, TimeSpan.Zero));
}

