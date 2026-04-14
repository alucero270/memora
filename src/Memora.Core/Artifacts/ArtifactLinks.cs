namespace Memora.Core.Artifacts;

public enum ArtifactRelationshipKind
{
    DependsOn,
    Affects,
    DerivedFrom,
    Supersedes
}

public sealed record ArtifactRelationship(
    ArtifactRelationshipKind Kind,
    string TargetArtifactId);

public sealed record ArtifactLinks
{
    public ArtifactLinks(IReadOnlyList<ArtifactRelationship> relationships)
    {
        ArgumentNullException.ThrowIfNull(relationships);
        Relationships = relationships;
    }

    public ArtifactLinks(
        IReadOnlyList<string> dependsOn,
        IReadOnlyList<string> affects,
        IReadOnlyList<string> derivedFrom,
        IReadOnlyList<string> supersedes)
        : this(CreateRelationships(dependsOn, affects, derivedFrom, supersedes))
    {
    }

    public IReadOnlyList<ArtifactRelationship> Relationships { get; }

    public IReadOnlyList<ArtifactRelationship> DependsOn => GetRelationships(ArtifactRelationshipKind.DependsOn);

    public IReadOnlyList<ArtifactRelationship> Affects => GetRelationships(ArtifactRelationshipKind.Affects);

    public IReadOnlyList<ArtifactRelationship> DerivedFrom => GetRelationships(ArtifactRelationshipKind.DerivedFrom);

    public IReadOnlyList<ArtifactRelationship> Supersedes => GetRelationships(ArtifactRelationshipKind.Supersedes);

    public IReadOnlyList<string> GetTargetArtifactIds(ArtifactRelationshipKind kind) =>
        Relationships
            .Where(relationship => relationship.Kind == kind)
            .Select(relationship => relationship.TargetArtifactId)
            .ToArray();

    public static IReadOnlyList<string> FrontmatterKeys { get; } =
    [
        "depends_on",
        "affects",
        "derived_from",
        "supersedes"
    ];

    public static bool TryParseKind(string key, out ArtifactRelationshipKind kind)
    {
        switch (key)
        {
            case "depends_on":
                kind = ArtifactRelationshipKind.DependsOn;
                return true;
            case "affects":
                kind = ArtifactRelationshipKind.Affects;
                return true;
            case "derived_from":
                kind = ArtifactRelationshipKind.DerivedFrom;
                return true;
            case "supersedes":
                kind = ArtifactRelationshipKind.Supersedes;
                return true;
            default:
                kind = default;
                return false;
        }
    }

    public static string ToFrontmatterKey(ArtifactRelationshipKind kind) =>
        kind switch
        {
            ArtifactRelationshipKind.DependsOn => "depends_on",
            ArtifactRelationshipKind.Affects => "affects",
            ArtifactRelationshipKind.DerivedFrom => "derived_from",
            ArtifactRelationshipKind.Supersedes => "supersedes",
            _ => throw new ArgumentOutOfRangeException(nameof(kind), kind, "Unsupported relationship kind.")
        };

    public static ArtifactLinks Empty { get; } = new([]);

    private static IReadOnlyList<ArtifactRelationship> CreateRelationships(
        IReadOnlyList<string> dependsOn,
        IReadOnlyList<string> affects,
        IReadOnlyList<string> derivedFrom,
        IReadOnlyList<string> supersedes)
    {
        ArgumentNullException.ThrowIfNull(dependsOn);
        ArgumentNullException.ThrowIfNull(affects);
        ArgumentNullException.ThrowIfNull(derivedFrom);
        ArgumentNullException.ThrowIfNull(supersedes);

        var relationships = new List<ArtifactRelationship>(
            dependsOn.Count + affects.Count + derivedFrom.Count + supersedes.Count);

        relationships.AddRange(dependsOn.Select(targetArtifactId => new ArtifactRelationship(ArtifactRelationshipKind.DependsOn, targetArtifactId)));
        relationships.AddRange(affects.Select(targetArtifactId => new ArtifactRelationship(ArtifactRelationshipKind.Affects, targetArtifactId)));
        relationships.AddRange(derivedFrom.Select(targetArtifactId => new ArtifactRelationship(ArtifactRelationshipKind.DerivedFrom, targetArtifactId)));
        relationships.AddRange(supersedes.Select(targetArtifactId => new ArtifactRelationship(ArtifactRelationshipKind.Supersedes, targetArtifactId)));

        return relationships;
    }

    private IReadOnlyList<ArtifactRelationship> GetRelationships(ArtifactRelationshipKind kind) =>
        Relationships
            .Where(relationship => relationship.Kind == kind)
            .ToArray();
}
