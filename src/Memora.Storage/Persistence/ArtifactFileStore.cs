using Memora.Core.Artifacts;
using Memora.Storage.Workspaces;

namespace Memora.Storage.Persistence;

public sealed class ArtifactFileStore
{
    private readonly ArtifactMarkdownWriter _markdownWriter = new();

    public string Save(ProjectWorkspace workspace, ArtifactDocument artifact)
    {
        ArgumentNullException.ThrowIfNull(workspace);
        ArgumentNullException.ThrowIfNull(artifact);

        if (!string.Equals(workspace.ProjectId, artifact.ProjectId, StringComparison.Ordinal))
        {
            throw new InvalidOperationException(
                $"Artifact project '{artifact.ProjectId}' does not match workspace project '{workspace.ProjectId}'.");
        }

        var path = ResolvePath(workspace, artifact);
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);

        if (File.Exists(path))
        {
            throw new IOException($"Artifact revision file '{path}' already exists.");
        }

        var markdown = _markdownWriter.Write(artifact);
        File.WriteAllText(path, markdown);
        return path;
    }

    private static string ResolvePath(ProjectWorkspace workspace, ArtifactDocument artifact)
    {
        var fileName = $"{artifact.Id}.r{artifact.Revision:D4}.md";
        var directory = ResolveDirectory(workspace, artifact);
        return Path.Combine(directory, fileName);
    }

    private static string ResolveDirectory(ProjectWorkspace workspace, ArtifactDocument artifact) =>
        artifact switch
        {
            SessionSummaryArtifact => workspace.SummariesRootPath,
            _ when artifact.Status == ArtifactStatus.Approved => ResolveCanonicalDirectory(workspace, artifact.Type),
            _ => Path.Combine(workspace.DraftsRootPath, artifact.Type.ToSchemaValue())
        };

    private static string ResolveCanonicalDirectory(ProjectWorkspace workspace, ArtifactType artifactType) =>
        artifactType switch
        {
            ArtifactType.Charter => workspace.CanonicalChartersPath,
            ArtifactType.Plan => workspace.CanonicalPlansPath,
            ArtifactType.Decision => workspace.CanonicalDecisionsPath,
            ArtifactType.Constraint => workspace.CanonicalConstraintsPath,
            ArtifactType.Question => workspace.CanonicalQuestionsPath,
            ArtifactType.Outcome => workspace.CanonicalOutcomesPath,
            ArtifactType.RepoStructure => workspace.CanonicalRepoPath,
            ArtifactType.SessionSummary => workspace.SummariesRootPath,
            _ => throw new ArgumentOutOfRangeException(nameof(artifactType), artifactType, "Unsupported artifact type.")
        };
}
