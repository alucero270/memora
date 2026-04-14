namespace Memora.Core.Artifacts;

public sealed record ArtifactLinks(
    IReadOnlyList<string> DependsOn,
    IReadOnlyList<string> Affects,
    IReadOnlyList<string> DerivedFrom,
    IReadOnlyList<string> Supersedes)
{
    public static ArtifactLinks Empty { get; } = new([], [], [], []);
}
