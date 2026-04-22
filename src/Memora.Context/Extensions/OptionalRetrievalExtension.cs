using System.Collections.ObjectModel;

namespace Memora.Context.Extensions;

public enum OptionalRetrievalExtensionKind
{
    SemanticCandidateDiscovery
}

public sealed record OptionalRetrievalExtensionDescriptor
{
    public OptionalRetrievalExtensionDescriptor(
        string name,
        string provider,
        OptionalRetrievalExtensionKind kind,
        bool enabledByDefault = false,
        bool canProvideCanonicalTruth = false)
    {
        Name = RequireValue(name, nameof(name));
        Provider = RequireValue(provider, nameof(provider));
        Kind = kind;
        EnabledByDefault = enabledByDefault;
        CanProvideCanonicalTruth = canProvideCanonicalTruth;

        if (EnabledByDefault)
        {
            throw new ArgumentException(
                "Optional retrieval extensions cannot be enabled by default in core v1.",
                nameof(enabledByDefault));
        }

        if (CanProvideCanonicalTruth)
        {
            throw new ArgumentException(
                "Optional retrieval extensions cannot provide canonical truth.",
                nameof(canProvideCanonicalTruth));
        }
    }

    public string Name { get; }

    public string Provider { get; }

    public OptionalRetrievalExtensionKind Kind { get; }

    public bool EnabledByDefault { get; }

    public bool CanProvideCanonicalTruth { get; }

    private static string RequireValue(string value, string parameterName) =>
        string.IsNullOrWhiteSpace(value)
            ? throw new ArgumentException($"{parameterName} is required.", parameterName)
            : value.Trim();
}

public sealed record ExternalRetrievalRequest
{
    public ExternalRetrievalRequest(
        string projectId,
        string taskDescription,
        IReadOnlyList<string>? focusArtifactIds = null,
        IReadOnlyList<string>? focusTags = null,
        int maxCandidateCount = 20)
    {
        if (maxCandidateCount <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(maxCandidateCount), "Candidate limit must be greater than zero.");
        }

        ProjectId = RequireValue(projectId, nameof(projectId));
        TaskDescription = RequireValue(taskDescription, nameof(taskDescription));
        FocusArtifactIds = NormalizeValues(focusArtifactIds);
        FocusTags = NormalizeValues(focusTags);
        MaxCandidateCount = maxCandidateCount;
    }

    public string ProjectId { get; }

    public string TaskDescription { get; }

    public IReadOnlyList<string> FocusArtifactIds { get; }

    public IReadOnlyList<string> FocusTags { get; }

    public int MaxCandidateCount { get; }

    private static string RequireValue(string value, string parameterName) =>
        string.IsNullOrWhiteSpace(value)
            ? throw new ArgumentException($"{parameterName} is required.", parameterName)
            : value.Trim();

    private static IReadOnlyList<string> NormalizeValues(IReadOnlyList<string>? values) =>
        values is null
            ? []
            : values
                .Where(value => !string.IsNullOrWhiteSpace(value))
                .Select(value => value.Trim())
                .Distinct(StringComparer.Ordinal)
                .OrderBy(value => value, StringComparer.Ordinal)
                .ToArray();
}

public sealed record ExternalRetrievalCandidate
{
    public ExternalRetrievalCandidate(
        string artifactId,
        string explanation,
        IReadOnlyDictionary<string, string>? metadata = null)
    {
        ArtifactId = RequireValue(artifactId, nameof(artifactId));
        Explanation = RequireValue(explanation, nameof(explanation));
        Metadata = new ReadOnlyDictionary<string, string>(
            (metadata ?? new Dictionary<string, string>(StringComparer.Ordinal))
                .Where(pair => !string.IsNullOrWhiteSpace(pair.Key))
                .OrderBy(pair => pair.Key, StringComparer.Ordinal)
                .ToDictionary(
                    pair => pair.Key.Trim(),
                    pair => pair.Value,
                    StringComparer.Ordinal));
    }

    public string ArtifactId { get; }

    public string Explanation { get; }

    public IReadOnlyDictionary<string, string> Metadata { get; }

    private static string RequireValue(string value, string parameterName) =>
        string.IsNullOrWhiteSpace(value)
            ? throw new ArgumentException($"{parameterName} is required.", parameterName)
            : value.Trim();
}

public interface IOptionalRetrievalExtension
{
    OptionalRetrievalExtensionDescriptor Descriptor { get; }

    IReadOnlyList<ExternalRetrievalCandidate> DiscoverCandidates(ExternalRetrievalRequest request);
}
