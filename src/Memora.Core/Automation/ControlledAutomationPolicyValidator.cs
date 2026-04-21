using System.Collections.ObjectModel;

namespace Memora.Core.Automation;

public sealed record ControlledAutomationPolicyValidationIssue(
    string Code,
    string Message,
    string? Path = null)
{
    public string DiagnosticMessage => string.IsNullOrWhiteSpace(Path)
        ? $"{Message} (code: {Code}; path: not provided)"
        : $"{Message} (code: {Code}; path: {Path.Trim()})";
}
public sealed class ControlledAutomationPolicyValidationResult
{
    public ControlledAutomationPolicyValidationResult(IEnumerable<ControlledAutomationPolicyValidationIssue> issues)
    {
        Issues = new ReadOnlyCollection<ControlledAutomationPolicyValidationIssue>(issues.ToList());
    }

    public bool IsValid => Issues.Count == 0;

    public IReadOnlyList<ControlledAutomationPolicyValidationIssue> Issues { get; }

    public static ControlledAutomationPolicyValidationResult Success { get; } = new([]);
}
public sealed class ControlledAutomationPolicyValidator
{
    public ControlledAutomationPolicyValidationResult Validate(ControlledAutomationPolicy policy)
    {
        ArgumentNullException.ThrowIfNull(policy);

        var issues = new List<ControlledAutomationPolicyValidationIssue>();

        if (policy.Permissions.Count == 0)
        {
            issues.Add(new ControlledAutomationPolicyValidationIssue(
                "automation.policy.permissions.missing",
                "Controlled automation policies must declare at least one explicit permission.",
                "permissions"));
        }

        if (!policy.RequiresExplicitTrigger)
        {
            issues.Add(new ControlledAutomationPolicyValidationIssue(
                "automation.policy.trigger.required",
                "Controlled automation policies must require an explicit trigger.",
                "requires_explicit_trigger"));
        }

        for (var index = 0; index < policy.Permissions.Count; index++)
        {
            ValidatePermission(policy.Permissions[index], index, issues);
        }

        return issues.Count == 0
            ? ControlledAutomationPolicyValidationResult.Success
            : new ControlledAutomationPolicyValidationResult(issues);
    }

    private static void ValidatePermission(
        ControlledAutomationPermission permission,
        int index,
        ICollection<ControlledAutomationPolicyValidationIssue> issues)
    {
        var pathPrefix = $"permissions[{index}]";

        if (!LowRiskArtifactClassCatalog.TryGetDefinition(permission.ArtifactType, out var definition))
        {
            issues.Add(new ControlledAutomationPolicyValidationIssue(
                "automation.policy.artifact_type.not_low_risk",
                $"Artifact type '{permission.ArtifactType}' is not a low-risk automation candidate.",
                $"{pathPrefix}.artifact_type"));
            return;
        }

        if (permission.StorageScope != definition.StorageScope)
        {
            issues.Add(new ControlledAutomationPolicyValidationIssue(
                "automation.policy.storage_scope.invalid",
                $"Artifact type '{permission.ArtifactType}' is only eligible for '{definition.StorageScope}' automation scope.",
                $"{pathPrefix}.storage_scope"));
        }

        if (permission.StorageScope == AutomationStorageScope.Canonical && !definition.AllowsCanonicalDirectWrite)
        {
            issues.Add(new ControlledAutomationPolicyValidationIssue(
                "automation.policy.canonical_write.invalid",
                $"Artifact type '{permission.ArtifactType}' is not eligible for direct canonical writes.",
                $"{pathPrefix}.storage_scope"));
        }

        foreach (var guardrail in definition.RequiredGuardrails)
        {
            if (!permission.RequiredGuardrails.Contains(guardrail, StringComparer.Ordinal))
            {
                issues.Add(new ControlledAutomationPolicyValidationIssue(
                    "automation.policy.guardrail.missing",
                    $"Policy permission is missing required guardrail '{guardrail}'.",
                    $"{pathPrefix}.required_guardrails"));
            }
        }
    }
}
