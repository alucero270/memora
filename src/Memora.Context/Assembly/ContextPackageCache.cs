using System.Security.Cryptography;
using System.Text;
using Memora.Context.Models;
using Memora.Core.Artifacts;

namespace Memora.Context.Assembly;

public sealed class ContextPackageCache
{
    private readonly object _sync = new();
    private readonly Dictionary<ContextPackageCacheKey, ContextBundle> _packages = new();

    public ContextPackageCacheResult GetOrBuild(
        ContextBundleRequest request,
        IReadOnlyList<ArtifactDocument> artifacts,
        Func<ContextBundleRequest, IReadOnlyList<ArtifactDocument>, ContextBundle> build)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentNullException.ThrowIfNull(artifacts);
        ArgumentNullException.ThrowIfNull(build);

        var key = ContextPackageCacheKey.Create(request, artifacts);

        lock (_sync)
        {
            if (_packages.TryGetValue(key, out var cachedBundle))
            {
                return new ContextPackageCacheResult(cachedBundle, WasCacheHit: true);
            }
        }

        var bundle = build(request, artifacts);

        lock (_sync)
        {
            if (_packages.TryGetValue(key, out var cachedBundle))
            {
                return new ContextPackageCacheResult(cachedBundle, WasCacheHit: true);
            }

            _packages[key] = bundle;
        }

        return new ContextPackageCacheResult(bundle, WasCacheHit: false);
    }

    public void Clear()
    {
        lock (_sync)
        {
            _packages.Clear();
        }
    }

    public int Count
    {
        get
        {
            lock (_sync)
            {
                return _packages.Count;
            }
        }
    }
}

public sealed record ContextPackageCacheResult(ContextBundle Bundle, bool WasCacheHit);

internal sealed record ContextPackageCacheKey(string RequestFingerprint, string ArtifactSetFingerprint)
{
    public static ContextPackageCacheKey Create(
        ContextBundleRequest request,
        IReadOnlyList<ArtifactDocument> artifacts) =>
        new(FingerprintRequest(request), FingerprintArtifacts(artifacts));

    private static string FingerprintRequest(ContextBundleRequest request) =>
        Hash(builder =>
        {
            Append(builder, request.ProjectId);
            Append(builder, request.TaskDescription);
            Append(builder, request.IncludeDraftArtifacts);
            Append(builder, request.IncludeLayer3History);
            Append(builder, request.MaxLayer2Artifacts);
            Append(builder, request.MaxLayer3Artifacts);
            AppendValues(builder, request.FocusArtifactIds);
            AppendValues(builder, request.FocusTags);
        });

    private static string FingerprintArtifacts(IReadOnlyList<ArtifactDocument> artifacts) =>
        Hash(builder =>
        {
            foreach (var artifact in artifacts
                         .OrderBy(artifact => artifact.ProjectId, StringComparer.Ordinal)
                         .ThenBy(artifact => artifact.Id, StringComparer.Ordinal)
                         .ThenByDescending(artifact => artifact.Revision)
                         .ThenBy(artifact => artifact.Type))
            {
                Append(builder, artifact.ProjectId);
                Append(builder, artifact.Id);
                Append(builder, artifact.Type);
                Append(builder, artifact.Status);
                Append(builder, artifact.Title);
                Append(builder, artifact.CreatedAtUtc.UtcTicks);
                Append(builder, artifact.UpdatedAtUtc.UtcTicks);
                Append(builder, artifact.Revision);
                AppendValues(builder, artifact.Tags);
                Append(builder, artifact.Provenance);
                Append(builder, artifact.Reason);
                AppendRelationships(builder, artifact.Links.Relationships);
                Append(builder, artifact.Body);
                AppendSections(builder, artifact.Sections);
                AppendTypeSpecificValues(builder, artifact);
            }
        });

    private static void AppendTypeSpecificValues(StringBuilder builder, ArtifactDocument artifact)
    {
        switch (artifact)
        {
            case PlanArtifact plan:
                Append(builder, plan.Priority);
                Append(builder, plan.Active);
                break;
            case ArchitectureDecisionArtifact decision:
                Append(builder, decision.DecisionDate);
                break;
            case ConstraintArtifact constraint:
                Append(builder, constraint.ConstraintKind);
                Append(builder, constraint.Severity);
                break;
            case OpenQuestionArtifact question:
                Append(builder, question.QuestionStatus);
                Append(builder, question.Priority);
                break;
            case OutcomeArtifact outcome:
                Append(builder, outcome.Outcome);
                break;
            case RepoStructureArtifact repoStructure:
                Append(builder, repoStructure.SnapshotSource);
                break;
            case SessionSummaryArtifact summary:
                Append(builder, summary.SessionType);
                Append(builder, summary.Canonical);
                break;
        }
    }

    private static void AppendRelationships(StringBuilder builder, IReadOnlyList<ArtifactRelationship> relationships)
    {
        foreach (var relationship in relationships
                     .OrderBy(relationship => relationship.Kind)
                     .ThenBy(relationship => relationship.TargetArtifactId, StringComparer.Ordinal))
        {
            Append(builder, relationship.Kind);
            Append(builder, relationship.TargetArtifactId);
        }
    }

    private static void AppendSections(StringBuilder builder, IReadOnlyDictionary<string, string> sections)
    {
        foreach (var pair in sections.OrderBy(pair => pair.Key, StringComparer.Ordinal))
        {
            Append(builder, pair.Key);
            Append(builder, pair.Value);
        }
    }

    private static void AppendValues(StringBuilder builder, IReadOnlyList<string> values)
    {
        foreach (var value in values)
        {
            Append(builder, value);
        }
    }

    private static void Append(StringBuilder builder, object? value)
    {
        var text = value?.ToString() ?? string.Empty;
        builder.Append(text.Length);
        builder.Append(':');
        builder.Append(text);
        builder.Append('|');
    }

    private static string Hash(Action<StringBuilder> append)
    {
        var builder = new StringBuilder();
        append(builder);
        return Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(builder.ToString())));
    }
}
