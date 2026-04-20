using Memora.Context.Models;
using Memora.Context.Ranking;
using Memora.Context.Reasoning;
using Memora.Core.Artifacts;

namespace Memora.Context.Assembly;

public sealed class ContextBundleBuilder
{
    private readonly DeterministicContextRankingEngine _rankingEngine;
    private readonly ContextInclusionReasoner _reasoner;

    public ContextBundleBuilder()
        : this(new DeterministicContextRankingEngine(), new ContextInclusionReasoner())
    {
    }

    public ContextBundleBuilder(
        DeterministicContextRankingEngine rankingEngine,
        ContextInclusionReasoner reasoner)
    {
        _rankingEngine = rankingEngine ?? throw new ArgumentNullException(nameof(rankingEngine));
        _reasoner = reasoner ?? throw new ArgumentNullException(nameof(reasoner));
    }

    public ContextBundle Build(
        ContextBundleRequest request,
        IReadOnlyList<ArtifactDocument> artifacts)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentNullException.ThrowIfNull(artifacts);

        var candidates = CreateCandidates(request, artifacts);
        var selectedKeys = new HashSet<(string Id, int Revision)>();

        var layer1 = BuildLayer(
            request,
            ContextLayerKind.Layer1,
            SelectLayer1Candidates(candidates),
            selectedKeys,
            limit: int.MaxValue,
            anchorSelection: true);

        var layer2 = BuildLayer(
            request,
            ContextLayerKind.Layer2,
            candidates.Where(candidate =>
                !selectedKeys.Contains((candidate.Artifact.Id, candidate.Artifact.Revision)) &&
                candidate.Artifact.Type is ArtifactType.Decision or ArtifactType.Constraint or ArtifactType.Question or ArtifactType.Outcome),
            selectedKeys,
            request.MaxLayer2Artifacts);

        var layer3 = request.IncludeLayer3History
            ? BuildLayer(
                request,
                ContextLayerKind.Layer3,
                candidates.Where(candidate =>
                    !selectedKeys.Contains((candidate.Artifact.Id, candidate.Artifact.Revision)) &&
                    (candidate.Origin == ContextArtifactOrigin.SessionSummary ||
                     candidate.Artifact is PlanArtifact { Active: false })),
                selectedKeys,
                request.MaxLayer3Artifacts)
            : new ContextBundleLayer(ContextLayerKind.Layer3);

        return new ContextBundle(request, [layer1, layer2, layer3]);
    }

    private ContextBundleLayer BuildLayer(
        ContextBundleRequest request,
        ContextLayerKind layer,
        IEnumerable<ContextBundleArtifact> candidates,
        ISet<(string Id, int Revision)> selectedKeys,
        int limit,
        bool anchorSelection = false)
    {
        var rankedCandidates = _rankingEngine
            .Rank(request, candidates.ToArray())
            .ToArray();

        var selected = anchorSelection
            ? SelectLayer1Anchors(request, rankedCandidates, selectedKeys)
            : SelectTopRanked(request, layer, rankedCandidates, selectedKeys, limit);

        return new ContextBundleLayer(layer, selected);
    }

    private IReadOnlyList<ContextBundleArtifact> SelectLayer1Anchors(
        ContextBundleRequest request,
        IReadOnlyList<RankedContextArtifact> rankedCandidates,
        ISet<(string Id, int Revision)> selectedKeys)
    {
        var selected = new List<ContextBundleArtifact>();

        foreach (var artifactType in new[] { ArtifactType.Charter, ArtifactType.Plan, ArtifactType.RepoStructure })
        {
            var rankedArtifact = rankedCandidates.FirstOrDefault(candidate =>
                candidate.Artifact.Artifact.Type == artifactType &&
                (artifactType != ArtifactType.Plan || candidate.Artifact.Artifact is PlanArtifact { Active: true }) &&
                selectedKeys.Add((candidate.Artifact.Artifact.Id, candidate.Artifact.Artifact.Revision)));

            if (rankedArtifact is null)
            {
                continue;
            }

            selected.Add(WithReasons(request, ContextLayerKind.Layer1, rankedArtifact));
        }

        return selected;
    }

    private IReadOnlyList<ContextBundleArtifact> SelectTopRanked(
        ContextBundleRequest request,
        ContextLayerKind layer,
        IReadOnlyList<RankedContextArtifact> rankedCandidates,
        ISet<(string Id, int Revision)> selectedKeys,
        int limit)
    {
        var selected = new List<ContextBundleArtifact>();

        foreach (var rankedArtifact in rankedCandidates)
        {
            if (!selectedKeys.Add((rankedArtifact.Artifact.Artifact.Id, rankedArtifact.Artifact.Artifact.Revision)))
            {
                continue;
            }

            selected.Add(WithReasons(request, layer, rankedArtifact));

            if (selected.Count == limit)
            {
                break;
            }
        }

        return selected;
    }

    private ContextBundleArtifact WithReasons(
        ContextBundleRequest request,
        ContextLayerKind layer,
        RankedContextArtifact rankedArtifact) =>
        new(
            rankedArtifact.Artifact.Artifact,
            rankedArtifact.Artifact.Origin,
            _reasoner.ExplainInclusion(request, layer, rankedArtifact));

    private static IReadOnlyList<ContextBundleArtifact> SelectLayer1Candidates(IReadOnlyList<ContextBundleArtifact> candidates) =>
        candidates
            .Where(candidate =>
                candidate.Artifact.Type == ArtifactType.Charter ||
                candidate.Artifact.Type == ArtifactType.RepoStructure ||
                candidate.Artifact is PlanArtifact { Active: true })
            .ToArray();

    private static IReadOnlyList<ContextBundleArtifact> CreateCandidates(
        ContextBundleRequest request,
        IReadOnlyList<ArtifactDocument> artifacts) =>
        artifacts
            .Select(artifact => CreateCandidate(request, artifact))
            .Where(candidate => candidate is not null)
            .Cast<ContextBundleArtifact>()
            .ToArray();

    private static ContextBundleArtifact? CreateCandidate(ContextBundleRequest request, ArtifactDocument artifact) =>
        artifact switch
        {
            SessionSummaryArtifact summary when request.IncludeLayer3History =>
                new ContextBundleArtifact(summary, ContextArtifactOrigin.SessionSummary),
            _ when artifact.Status == ArtifactStatus.Approved =>
                new ContextBundleArtifact(artifact, ContextArtifactOrigin.CanonicalApproved),
            _ when request.IncludeDraftArtifacts && artifact.Status is ArtifactStatus.Draft or ArtifactStatus.Proposed =>
                new ContextBundleArtifact(artifact, ContextArtifactOrigin.DraftProposal),
            _ => null
        };
}
