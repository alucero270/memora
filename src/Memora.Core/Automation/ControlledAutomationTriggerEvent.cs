using System.Collections.ObjectModel;
using Memora.Core.Artifacts;

namespace Memora.Core.Automation;

public enum ControlledAutomationEventKind
{
    ArtifactLifecycleChanged,
    ApprovalDecisionRecorded,
    ExplicitOperatorRequested
}

public sealed record ControlledAutomationTriggerEvent
{
    public ControlledAutomationTriggerEvent(
        string eventId,
        ControlledAutomationEventKind kind,
        string projectId,
        ArtifactType artifactType,
        string? artifactId,
        ArtifactStatus? previousStatus,
        ArtifactStatus? currentStatus,
        DateTimeOffset occurredAtUtc)
    {
        if (occurredAtUtc == default || occurredAtUtc.Offset != TimeSpan.Zero)
        {
            throw new ArgumentException("Automation trigger timestamps must be non-default UTC values.", nameof(occurredAtUtc));
        }

        EventId = RequireValue(eventId, nameof(eventId));
        Kind = kind;
        ProjectId = RequireValue(projectId, nameof(projectId));
        ArtifactType = artifactType;
        ArtifactId = string.IsNullOrWhiteSpace(artifactId) ? null : artifactId.Trim();
        PreviousStatus = previousStatus;
        CurrentStatus = currentStatus;
        OccurredAtUtc = occurredAtUtc;
    }

    public string EventId { get; }

    public ControlledAutomationEventKind Kind { get; }

    public string ProjectId { get; }

    public ArtifactType ArtifactType { get; }

    public string? ArtifactId { get; }

    public ArtifactStatus? PreviousStatus { get; }

    public ArtifactStatus? CurrentStatus { get; }

    public DateTimeOffset OccurredAtUtc { get; }

    public static ControlledAutomationTriggerEvent FromLifecycleTransition(
        string eventId,
        ArtifactDocument before,
        ArtifactDocument after,
        DateTimeOffset occurredAtUtc)
    {
        ArgumentNullException.ThrowIfNull(before);
        ArgumentNullException.ThrowIfNull(after);

        if (!string.Equals(before.Id, after.Id, StringComparison.Ordinal) ||
            !string.Equals(before.ProjectId, after.ProjectId, StringComparison.Ordinal) ||
            before.Type != after.Type)
        {
            throw new ArgumentException("Lifecycle transition events must describe one artifact identity.");
        }

        return new ControlledAutomationTriggerEvent(
            eventId,
            ControlledAutomationEventKind.ArtifactLifecycleChanged,
            after.ProjectId,
            after.Type,
            after.Id,
            before.Status,
            after.Status,
            occurredAtUtc);
    }

    public static ControlledAutomationTriggerEvent ExplicitOperatorRequest(
        string eventId,
        string projectId,
        ArtifactType artifactType,
        string? artifactId,
        DateTimeOffset occurredAtUtc) =>
        new(
            eventId,
            ControlledAutomationEventKind.ExplicitOperatorRequested,
            projectId,
            artifactType,
            artifactId,
            previousStatus: null,
            currentStatus: null,
            occurredAtUtc);

    private static string RequireValue(string value, string parameterName) =>
        string.IsNullOrWhiteSpace(value)
            ? throw new ArgumentException($"{parameterName} is required.", parameterName)
            : value.Trim();
}

public sealed record ControlledAutomationTriggerDecision
{
    public ControlledAutomationTriggerDecision(
        bool isEligible,
        IReadOnlyList<string> reasonCodes,
        IReadOnlyList<ControlledAutomationPolicyValidationIssue> validationIssues)
    {
        IsEligible = isEligible;
        ReasonCodes = new ReadOnlyCollection<string>(
            (reasonCodes ?? throw new ArgumentNullException(nameof(reasonCodes)))
                .Where(reason => !string.IsNullOrWhiteSpace(reason))
                .Select(reason => reason.Trim())
                .Distinct(StringComparer.Ordinal)
                .OrderBy(reason => reason, StringComparer.Ordinal)
                .ToList());
        ValidationIssues = new ReadOnlyCollection<ControlledAutomationPolicyValidationIssue>(
            (validationIssues ?? throw new ArgumentNullException(nameof(validationIssues))).ToList());
    }

    public bool IsEligible { get; }

    public IReadOnlyList<string> ReasonCodes { get; }

    public IReadOnlyList<ControlledAutomationPolicyValidationIssue> ValidationIssues { get; }
}

public sealed class SafeAutomationTriggerEvaluator
{
    private readonly ControlledAutomationPolicyValidator _policyValidator;

    public SafeAutomationTriggerEvaluator()
        : this(new ControlledAutomationPolicyValidator())
    {
    }

    public SafeAutomationTriggerEvaluator(ControlledAutomationPolicyValidator policyValidator)
    {
        _policyValidator = policyValidator ?? throw new ArgumentNullException(nameof(policyValidator));
    }

    public ControlledAutomationTriggerDecision Evaluate(
        ControlledAutomationPolicy policy,
        ControlledAutomationTriggerEvent triggerEvent)
    {
        ArgumentNullException.ThrowIfNull(policy);
        ArgumentNullException.ThrowIfNull(triggerEvent);

        var policyValidation = _policyValidator.Validate(policy);
        if (!policyValidation.IsValid)
        {
            return new ControlledAutomationTriggerDecision(
                isEligible: false,
                ["automation.trigger.policy_invalid"],
                policyValidation.Issues);
        }

        if (!policy.Enabled)
        {
            return new ControlledAutomationTriggerDecision(
                isEligible: false,
                ["automation.trigger.policy_disabled"],
                []);
        }

        if (policy.RequiresExplicitTrigger &&
            triggerEvent.Kind != ControlledAutomationEventKind.ExplicitOperatorRequested)
        {
            return new ControlledAutomationTriggerDecision(
                isEligible: false,
                ["automation.trigger.explicit_required"],
                []);
        }

        var hasMatchingPermission = policy.Permissions.Any(permission =>
            permission.ArtifactType == triggerEvent.ArtifactType);

        return hasMatchingPermission
            ? new ControlledAutomationTriggerDecision(
                isEligible: true,
                ["automation.trigger.policy_permission_matched"],
                [])
            : new ControlledAutomationTriggerDecision(
                isEligible: false,
                ["automation.trigger.permission_missing"],
                []);
    }
}

