using System.Text.RegularExpressions;
using Memora.Context.Models;
using Memora.Core.Artifacts;

namespace Memora.Context.Ranking;

public sealed class DeterministicContextRankingEngine
{
    private static readonly Regex TokenRegex = new("[a-z0-9]+", RegexOptions.Compiled | RegexOptions.CultureInvariant);
    private static readonly Regex MilestoneRegex = new(@"(?:milestone[\s_-]*\d+|m\d+)", RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);

    private static readonly IReadOnlyDictionary<ArtifactType, int> TypePriorityMap =
        new Dictionary<ArtifactType, int>
        {
            [ArtifactType.Charter] = 8,
            [ArtifactType.Plan] = 7,
            [ArtifactType.RepoStructure] = 6,
            [ArtifactType.Decision] = 5,
            [ArtifactType.Constraint] = 4,
            [ArtifactType.Question] = 3,
            [ArtifactType.Outcome] = 2,
            [ArtifactType.SessionSummary] = 1
        };

    public IReadOnlyList<RankedContextArtifact> Rank(
        ContextBundleRequest request,
        IReadOnlyList<ContextBundleArtifact> candidates)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentNullException.ThrowIfNull(candidates);

        var profile = BuildRequestProfile(request);

        var recencyRanks = candidates
            .Select(candidate => candidate.Artifact.UpdatedAtUtc)
            .Distinct()
            .OrderByDescending(updatedAt => updatedAt)
            .Select((updatedAt, index) => (updatedAt, rank: candidates.Count - index))
            .ToDictionary(pair => pair.updatedAt, pair => pair.rank);

        return candidates
            .Select(CreateCandidateProfile)
            .Select(candidateProfile => new RankedContextArtifact(
                candidateProfile.Candidate,
                new ContextRankingBreakdown(
                    TypePriority: GetTypePriority(candidateProfile.Candidate.Artifact.Type),
                    CanonicalStatusPriority: GetCanonicalStatusPriority(candidateProfile.Candidate),
                    MilestoneRelevance: CalculateMilestoneRelevance(candidateProfile, profile.MilestoneTokens),
                    RelationshipProximity: CalculateRelationshipProximity(candidateProfile.Candidate.Artifact, profile.FocusArtifactIds),
                    RecencyPriority: recencyRanks[candidateProfile.Candidate.Artifact.UpdatedAtUtc],
                    DirectMatchStrength: CalculateDirectMatchStrength(candidateProfile, profile.RequestTokens))))
            .OrderByDescending(result => result.Breakdown.TypePriority)
            .ThenByDescending(result => result.Breakdown.CanonicalStatusPriority)
            .ThenByDescending(result => result.Breakdown.MilestoneRelevance)
            .ThenByDescending(result => result.Breakdown.RelationshipProximity)
            .ThenByDescending(result => result.Breakdown.RecencyPriority)
            .ThenByDescending(result => result.Breakdown.DirectMatchStrength)
            .ThenBy(result => result.Artifact.Artifact.Id, StringComparer.Ordinal)
            .ThenByDescending(result => result.Artifact.Artifact.Revision)
            .ToArray();
    }

    private static int GetTypePriority(ArtifactType artifactType) => TypePriorityMap[artifactType];

    private static int GetCanonicalStatusPriority(ContextBundleArtifact artifact) =>
        artifact.Origin switch
        {
            ContextArtifactOrigin.CanonicalApproved => 3,
            ContextArtifactOrigin.DraftProposal when artifact.Artifact.Status == ArtifactStatus.Draft => 2,
            ContextArtifactOrigin.DraftProposal when artifact.Artifact.Status == ArtifactStatus.Proposed => 1,
            ContextArtifactOrigin.SessionSummary => 0,
            _ => 0
        };

    private static int CalculateMilestoneRelevance(
        ContextRankingCandidateProfile candidateProfile,
        IReadOnlySet<string> requestMilestones)
    {
        if (requestMilestones.Count == 0)
        {
            return 0;
        }

        return requestMilestones.Count(candidateProfile.MilestoneTokens.Contains);
    }

    private static int CalculateRelationshipProximity(ArtifactDocument artifact, IReadOnlySet<string> focusArtifactIds)
    {
        if (focusArtifactIds.Count == 0)
        {
            return 0;
        }

        if (focusArtifactIds.Contains(artifact.Id))
        {
            return focusArtifactIds.Count + 1;
        }

        return artifact.Links.Relationships.Count(relationship =>
            focusArtifactIds.Contains(relationship.TargetArtifactId));
    }

    private static int CalculateDirectMatchStrength(
        ContextRankingCandidateProfile candidateProfile,
        IReadOnlyDictionary<string, int> requestTokens)
    {
        if (requestTokens.Count == 0)
        {
            return 0;
        }

        return requestTokens.Sum(pair =>
            pair.Value * candidateProfile.TextWeights.GetValueOrDefault(pair.Key, 0));
    }

    private static ContextRankingRequestProfile BuildRequestProfile(ContextBundleRequest request)
    {
        var milestoneTokens = ExtractMilestoneTokens(request.TaskDescription)
            .Union(request.FocusTags.SelectMany(ExtractMilestoneTokens), StringComparer.Ordinal)
            .ToHashSet(StringComparer.Ordinal);

        return new ContextRankingRequestProfile(
            BuildRequestTokens(request),
            milestoneTokens,
            request.FocusArtifactIds.ToHashSet(StringComparer.Ordinal));
    }

    private static IReadOnlyDictionary<string, int> BuildRequestTokens(ContextBundleRequest request)
    {
        var tokens = new Dictionary<string, int>(StringComparer.Ordinal);
        AddTokens(tokens, Tokenize(request.TaskDescription), 3);
        AddTokens(tokens, request.FocusTags.SelectMany(Tokenize), 4);
        AddTokens(tokens, request.FocusArtifactIds.SelectMany(Tokenize), 5);
        return tokens;
    }

    private static ContextRankingCandidateProfile CreateCandidateProfile(ContextBundleArtifact candidate)
    {
        var artifactTextWeights = new Dictionary<string, int>(StringComparer.Ordinal);
        AddTokens(artifactTextWeights, Tokenize(candidate.Artifact.Title), 6);
        AddTokens(artifactTextWeights, candidate.Artifact.Tags.SelectMany(Tokenize), 5);
        AddTokens(artifactTextWeights, candidate.Artifact.Sections.Keys.SelectMany(Tokenize), 3);
        AddTokens(artifactTextWeights, candidate.Artifact.Sections.Values.SelectMany(Tokenize), 2);
        AddTokens(artifactTextWeights, Tokenize(candidate.Artifact.Body), 1);

        return new ContextRankingCandidateProfile(
            candidate,
            ExtractMilestoneTokens(FlattenArtifactText(candidate.Artifact)).ToHashSet(StringComparer.Ordinal),
            artifactTextWeights);
    }

    private static void AddTokens(IDictionary<string, int> target, IEnumerable<string> tokens, int weight)
    {
        foreach (var token in tokens)
        {
            target[token] = target.TryGetValue(token, out var current)
                ? current + weight
                : weight;
        }
    }

    private static IEnumerable<string> Tokenize(string value) =>
        TokenRegex
            .Matches(value.ToLowerInvariant())
            .Select(match => match.Value);

    private static IReadOnlyList<string> ExtractMilestoneTokens(string value) =>
        MilestoneRegex
            .Matches(value)
            .Select(match => match.Value.Replace(" ", string.Empty, StringComparison.Ordinal).Replace("_", string.Empty, StringComparison.Ordinal).Replace("-", string.Empty, StringComparison.Ordinal).ToLowerInvariant())
            .Distinct(StringComparer.Ordinal)
            .OrderBy(token => token, StringComparer.Ordinal)
            .ToArray();

    private static string FlattenArtifactText(ArtifactDocument artifact)
    {
        var sectionText = string.Join(Environment.NewLine, artifact.Sections.Select(pair => $"{pair.Key}: {pair.Value}"));
        var tagText = string.Join(" ", artifact.Tags);
        return string.Join(Environment.NewLine, artifact.Title, tagText, sectionText, artifact.Body);
    }

    private sealed record ContextRankingRequestProfile(
        IReadOnlyDictionary<string, int> RequestTokens,
        IReadOnlySet<string> MilestoneTokens,
        IReadOnlySet<string> FocusArtifactIds);

    private sealed record ContextRankingCandidateProfile(
        ContextBundleArtifact Candidate,
        IReadOnlySet<string> MilestoneTokens,
        IReadOnlyDictionary<string, int> TextWeights);
}
