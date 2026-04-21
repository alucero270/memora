using System.Collections.ObjectModel;
using Memora.Core.Artifacts;

namespace Memora.Core.Automation;

public enum ControlledAutomationAction
{
    DirectWrite
}

public sealed record ControlledAutomationPermission
{
    public ControlledAutomationPermission(
        ControlledAutomationAction action,
        ArtifactType artifactType,
        AutomationStorageScope storageScope,
        IReadOnlyList<string> requiredGuardrails)
    {
        Action = action;
        ArtifactType = artifactType;
        StorageScope = storageScope;
        RequiredGuardrails = NormalizeValues(requiredGuardrails, nameof(requiredGuardrails));
    }

    public ControlledAutomationAction Action { get; }

    public ArtifactType ArtifactType { get; }

    public AutomationStorageScope StorageScope { get; }

    public IReadOnlyList<string> RequiredGuardrails { get; }

    private static IReadOnlyList<string> NormalizeValues(
        IReadOnlyList<string>? values,
        string parameterName)
    {
        ArgumentNullException.ThrowIfNull(values, parameterName);

        return new ReadOnlyCollection<string>(
            values
                .Where(value => !string.IsNullOrWhiteSpace(value))
                .Select(value => value.Trim())
                .Distinct(StringComparer.Ordinal)
                .OrderBy(value => value, StringComparer.Ordinal)
                .ToList());
    }
}

public sealed record ControlledAutomationPolicy
{
    public ControlledAutomationPolicy(
        string policyId,
        string name,
        bool enabled,
        bool requiresExplicitTrigger,
        IReadOnlyList<ControlledAutomationPermission> permissions)
    {
        PolicyId = RequireValue(policyId, nameof(policyId));
        Name = RequireValue(name, nameof(name));
        Enabled = enabled;
        RequiresExplicitTrigger = requiresExplicitTrigger;
        Permissions = new ReadOnlyCollection<ControlledAutomationPermission>(
            (permissions ?? throw new ArgumentNullException(nameof(permissions)))
                .Where(permission => permission is not null)
                .ToList());
    }

    public string PolicyId { get; }

    public string Name { get; }

    public bool Enabled { get; }

    public bool RequiresExplicitTrigger { get; }

    public IReadOnlyList<ControlledAutomationPermission> Permissions { get; }

    private static string RequireValue(string value, string parameterName) =>
        string.IsNullOrWhiteSpace(value)
            ? throw new ArgumentException($"{parameterName} is required.", parameterName)
            : value.Trim();
}

