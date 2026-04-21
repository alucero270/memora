using System.Collections.ObjectModel;
using Memora.Core.Artifacts;

namespace Memora.Core.Automation;

public sealed record PolicyGovernedWriteSafetyRequest
{
    public PolicyGovernedWriteSafetyRequest(
        string projectId,
        string artifactId,
        ArtifactType artifactType,
        AutomationStorageScope storageScope,
        ControlledAutomationPolicy policy,
        ControlledAutomationTriggerEvent triggerEvent)
    {
        ProjectId = RequireValue(projectId, nameof(projectId));
        ArtifactId = RequireValue(artifactId, nameof(artifactId));
        ArtifactType = artifactType;
        StorageScope = storageScope;
        Policy = policy ?? throw new ArgumentNullException(nameof(policy));
        TriggerEvent = triggerEvent ?? throw new ArgumentNullException(nameof(triggerEvent));
    }

    public string ProjectId { get; }

    public string ArtifactId { get; }

    public ArtifactType ArtifactType { get; }

    public AutomationStorageScope StorageScope { get; }

    public ControlledAutomationPolicy Policy { get; }

    public ControlledAutomationTriggerEvent TriggerEvent { get; }

    private static string RequireValue(string value, string parameterName) =>
        string.IsNullOrWhiteSpace(value)
            ? throw new ArgumentException($"{parameterName} is required.", parameterName)
            : value.Trim();
}

public sealed record PolicyGovernedWriteSafetyIssue(
    string Code,
    string Message,
    string? Path = null)
{
    public string DiagnosticMessage => string.IsNullOrWhiteSpace(Path)
        ? $"{Message} (code: {Code}; path: not provided)"
        : $"{Message} (code: {Code}; path: {Path.Trim()})";
}

public sealed class PolicyGovernedWriteSafetyResult
{
    public PolicyGovernedWriteSafetyResult(IEnumerable<PolicyGovernedWriteSafetyIssue> issues)
    {
        Issues = new ReadOnlyCollection<PolicyGovernedWriteSafetyIssue>(issues.ToList());
    }

    public bool IsAllowed => Issues.Count == 0;

    public IReadOnlyList<PolicyGovernedWriteSafetyIssue> Issues { get; }

    public static PolicyGovernedWriteSafetyResult Allowed { get; } = new([]);
}

public sealed class PolicyGovernedWriteSafetyValidator
{
    private readonly SafeAutomationTriggerEvaluator _triggerEvaluator;

    public PolicyGovernedWriteSafetyValidator()
        : this(new SafeAutomationTriggerEvaluator())
    {
    }

    public PolicyGovernedWriteSafetyValidator(SafeAutomationTriggerEvaluator triggerEvaluator)
    {
        _triggerEvaluator = triggerEvaluator ?? throw new ArgumentNullException(nameof(triggerEvaluator));
    }

    public PolicyGovernedWriteSafetyResult Validate(PolicyGovernedWriteSafetyRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);

        var issues = new List<PolicyGovernedWriteSafetyIssue>();

        var triggerDecision = _triggerEvaluator.Evaluate(request.Policy, request.TriggerEvent);
        if (!triggerDecision.IsEligible)
        {
            issues.AddRange(MapTriggerDecision(triggerDecision));
        }

        if (!string.Equals(request.ProjectId, request.TriggerEvent.ProjectId, StringComparison.Ordinal))
        {
            issues.Add(new PolicyGovernedWriteSafetyIssue(
                "automation.write.project_id.mismatch",
                $"Trigger project '{request.TriggerEvent.ProjectId}' does not match requested project '{request.ProjectId}'.",
                "trigger_event.project_id"));
        }

        if (request.TriggerEvent.ArtifactType != request.ArtifactType)
        {
            issues.Add(new PolicyGovernedWriteSafetyIssue(
                "automation.write.artifact_type.mismatch",
                $"Trigger artifact type '{request.TriggerEvent.ArtifactType}' does not match requested artifact type '{request.ArtifactType}'.",
                "trigger_event.artifact_type"));
        }

        if (request.TriggerEvent.ArtifactId is not null &&
            !string.Equals(request.TriggerEvent.ArtifactId, request.ArtifactId, StringComparison.Ordinal))
        {
            issues.Add(new PolicyGovernedWriteSafetyIssue(
                "automation.write.artifact_id.mismatch",
                $"Trigger artifact '{request.TriggerEvent.ArtifactId}' does not match requested artifact '{request.ArtifactId}'.",
                "trigger_event.artifact_id"));
        }

        var permission = request.Policy.Permissions.FirstOrDefault(policyPermission =>
            policyPermission.Action == ControlledAutomationAction.DirectWrite &&
            policyPermission.ArtifactType == request.ArtifactType);

        if (permission is null)
        {
            issues.Add(new PolicyGovernedWriteSafetyIssue(
                "automation.write.permission.missing",
                $"Policy '{request.Policy.PolicyId}' does not allow direct writes for artifact type '{request.ArtifactType}'.",
                "policy.permissions"));
        }
        else if (permission.StorageScope != request.StorageScope)
        {
            issues.Add(new PolicyGovernedWriteSafetyIssue(
                "automation.write.storage_scope.mismatch",
                $"Policy storage scope '{permission.StorageScope}' does not match requested scope '{request.StorageScope}'.",
                "storage_scope"));
        }

        if (request.StorageScope == AutomationStorageScope.Canonical)
        {
            issues.Add(new PolicyGovernedWriteSafetyIssue(
                "automation.write.canonical_scope.denied",
                "Policy-governed writes cannot target canonical storage in the current prototype.",
                "storage_scope"));
        }

        return issues.Count == 0
            ? PolicyGovernedWriteSafetyResult.Allowed
            : new PolicyGovernedWriteSafetyResult(issues);
    }

    private static IEnumerable<PolicyGovernedWriteSafetyIssue> MapTriggerDecision(
        ControlledAutomationTriggerDecision triggerDecision)
    {
        foreach (var issue in triggerDecision.ValidationIssues)
        {
            yield return new PolicyGovernedWriteSafetyIssue(issue.Code, issue.DiagnosticMessage, issue.Path);
        }

        foreach (var reasonCode in triggerDecision.ReasonCodes)
        {
            yield return new PolicyGovernedWriteSafetyIssue(
                reasonCode,
                $"Automation trigger was not eligible: {reasonCode}.",
                "trigger_event");
        }
    }
}

