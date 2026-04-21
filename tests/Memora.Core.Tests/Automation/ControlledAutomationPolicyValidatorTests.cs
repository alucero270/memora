using Memora.Core.Artifacts;
using Memora.Core.Automation;

namespace Memora.Core.Tests.Automation;

public sealed class ControlledAutomationPolicyValidatorTests
{
    private readonly ControlledAutomationPolicyValidator _validator = new();

    [Fact]
    public void Validate_WithExplicitLowRiskPermission_Passes()
    {
        var definition = GetDefinition(ArtifactType.SessionSummary);
        var policy = new ControlledAutomationPolicy(
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

        var result = _validator.Validate(policy);

        Assert.True(result.IsValid);
    }

    [Fact]
    public void Validate_RequiresExplicitTrigger()
    {
        var definition = GetDefinition(ArtifactType.SessionSummary);
        var policy = new ControlledAutomationPolicy(
            "summary-direct-write",
            "Summary direct-write prototype",
            enabled: true,
            requiresExplicitTrigger: false,
            [
                new ControlledAutomationPermission(
                    ControlledAutomationAction.DirectWrite,
                    ArtifactType.SessionSummary,
                    definition.StorageScope,
                    definition.RequiredGuardrails)
            ]);

        var result = _validator.Validate(policy);

        Assert.False(result.IsValid);
        Assert.Contains(result.Issues, issue => issue.Code == "automation.policy.trigger.required");
    }

    [Fact]
    public void Validate_BlocksGovernanceCriticalArtifactTypes()
    {
        var policy = new ControlledAutomationPolicy(
            "plan-direct-write",
            "Plan direct-write attempt",
            enabled: true,
            requiresExplicitTrigger: true,
            [
                new ControlledAutomationPermission(
                    ControlledAutomationAction.DirectWrite,
                    ArtifactType.Plan,
                    AutomationStorageScope.Canonical,
                    ["reviewed by operator"])
            ]);

        var result = _validator.Validate(policy);

        Assert.False(result.IsValid);
        Assert.Contains(result.Issues, issue => issue.Code == "automation.policy.artifact_type.not_low_risk");
    }

    [Fact]
    public void Validate_RequiresDefinitionGuardrails()
    {
        var definition = GetDefinition(ArtifactType.SessionSummary);
        var policy = new ControlledAutomationPolicy(
            "summary-direct-write",
            "Summary direct-write prototype",
            enabled: true,
            requiresExplicitTrigger: true,
            [
                new ControlledAutomationPermission(
                    ControlledAutomationAction.DirectWrite,
                    ArtifactType.SessionSummary,
                    definition.StorageScope,
                    ["canonical must remain false"])
            ]);

        var result = _validator.Validate(policy);

        Assert.False(result.IsValid);
        Assert.Contains(result.Issues, issue => issue.Code == "automation.policy.guardrail.missing");
    }

    [Fact]
    public void Validate_BlocksCanonicalScopeWhenDefinitionDisallowsIt()
    {
        var definition = GetDefinition(ArtifactType.SessionSummary);
        var policy = new ControlledAutomationPolicy(
            "summary-direct-write",
            "Summary direct-write prototype",
            enabled: true,
            requiresExplicitTrigger: true,
            [
                new ControlledAutomationPermission(
                    ControlledAutomationAction.DirectWrite,
                    ArtifactType.SessionSummary,
                    AutomationStorageScope.Canonical,
                    definition.RequiredGuardrails)
            ]);

        var result = _validator.Validate(policy);

        Assert.False(result.IsValid);
        Assert.Contains(result.Issues, issue => issue.Code == "automation.policy.storage_scope.invalid");
        Assert.Contains(result.Issues, issue => issue.Code == "automation.policy.canonical_write.invalid");
    }

    private static LowRiskArtifactClassDefinition GetDefinition(ArtifactType artifactType)
    {
        Assert.True(LowRiskArtifactClassCatalog.TryGetDefinition(artifactType, out var definition));
        return definition;
    }
}

