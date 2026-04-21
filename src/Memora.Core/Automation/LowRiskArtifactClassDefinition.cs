using System.Collections.ObjectModel;
using Memora.Core.Artifacts;

namespace Memora.Core.Automation;

public enum AutomationStorageScope
{
    Draft,
    Summary,
    Canonical
}

public sealed record LowRiskArtifactClassDefinition
{
    public LowRiskArtifactClassDefinition(
        ArtifactType artifactType,
        string className,
        AutomationStorageScope storageScope,
        bool allowsCanonicalDirectWrite,
        bool allowsApprovalBypassInV1,
        string lowerRiskReason,
        IReadOnlyList<string> requiredGuardrails)
    {
        ArtifactType = artifactType;
        ClassName = RequireValue(className, nameof(className));
        StorageScope = storageScope;
        AllowsCanonicalDirectWrite = allowsCanonicalDirectWrite;
        AllowsApprovalBypassInV1 = allowsApprovalBypassInV1;
        LowerRiskReason = RequireValue(lowerRiskReason, nameof(lowerRiskReason));
        RequiredGuardrails = new ReadOnlyCollection<string>(
            (requiredGuardrails ?? throw new ArgumentNullException(nameof(requiredGuardrails)))
                .Where(guardrail => !string.IsNullOrWhiteSpace(guardrail))
                .Select(guardrail => guardrail.Trim())
                .Distinct(StringComparer.Ordinal)
                .OrderBy(guardrail => guardrail, StringComparer.Ordinal)
                .ToList());
    }

    public ArtifactType ArtifactType { get; }

    public string ClassName { get; }

    public AutomationStorageScope StorageScope { get; }

    public bool AllowsCanonicalDirectWrite { get; }

    public bool AllowsApprovalBypassInV1 { get; }

    public string LowerRiskReason { get; }

    public IReadOnlyList<string> RequiredGuardrails { get; }

    private static string RequireValue(string value, string parameterName) =>
        string.IsNullOrWhiteSpace(value)
            ? throw new ArgumentException($"{parameterName} is required.", parameterName)
            : value.Trim();
}

