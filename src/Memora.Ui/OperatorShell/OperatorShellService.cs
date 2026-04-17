using System.Net;
using System.Text;
using Memora.Storage.Parsing;
using Memora.Storage.Workspaces;

namespace Memora.Ui.OperatorShell;

internal sealed class OperatorShellService
{
    private readonly string _workspacesRootPath;
    private readonly WorkspaceDiscovery _workspaceDiscovery = new();
    private readonly ArtifactMarkdownParser _markdownParser = new();

    public OperatorShellService(string workspacesRootPath)
    {
        _workspacesRootPath = Path.GetFullPath(workspacesRootPath ?? throw new ArgumentNullException(nameof(workspacesRootPath)));
    }

    public string RenderRoot()
    {
        var workspaces = DiscoverWorkspaces();
        var html = new StringBuilder();
        html.AppendLine("<!doctype html><html><body>");
        html.AppendLine("<h1>Project Selector</h1>");
        html.AppendLine("<p>Select a project</p><ul>");
        foreach (var workspace in workspaces)
        {
            html.AppendLine($"<li><a href=\"/projects/{WebUtility.UrlEncode(workspace.ProjectId)}\">{WebUtility.HtmlEncode(workspace.Metadata.Name)}</a></li>");
        }
        html.AppendLine("</ul></body></html>");
        return html.ToString();
    }

    public string RenderProject(string projectId)
    {
        var workspace = RequireWorkspace(projectId);
        var files = EnumerateWorkspaceFiles(workspace);
        var html = new StringBuilder();
        html.AppendLine("<!doctype html><html><body>");
        html.AppendLine($"<h1>{WebUtility.HtmlEncode(workspace.Metadata.Name)}</h1>");
        html.AppendLine("<h2>Artifact Browser</h2><ul>");
        foreach (var file in files)
        {
            html.AppendLine($"<li>{WebUtility.HtmlEncode(file.RelativePath)}</li>");
        }
        html.AppendLine("</ul><h2>Approval Queue</h2><ul>");
        foreach (var file in files.Where(file => file.RelativePath.StartsWith("drafts/", StringComparison.Ordinal)))
        {
            html.AppendLine($"<li>{WebUtility.HtmlEncode(file.RelativePath)}</li>");
        }
        html.AppendLine("</ul></body></html>");
        return html.ToString();
    }

    public string RenderArtifact(string projectId, string relativePath)
    {
        var workspace = RequireWorkspace(projectId);
        var file = ResolveFile(workspace, relativePath);
        var parsed = _markdownParser.Parse(File.ReadAllText(file.FullPath));
        var title = parsed.Artifact?.Title ?? relativePath;

        return $"""
                <!doctype html>
                <html><body>
                <h1>Edit Draft</h1>
                <p>Save new draft revision</p>
                <h2>{WebUtility.HtmlEncode(title)}</h2>
                <pre>{WebUtility.HtmlEncode(File.ReadAllText(file.FullPath))}</pre>
                </body></html>
                """;
    }

    public string RenderReview(string projectId, string relativePath)
    {
        var workspace = RequireWorkspace(projectId);
        var file = ResolveFile(workspace, relativePath);

        return $"""
                <!doctype html>
                <html><body>
                <h1>Revision Review</h1>
                <p>Current UI boundary</p>
                <p>approval and rejection persistence remain outside this UI slice</p>
                <pre>{WebUtility.HtmlEncode(File.ReadAllText(file.FullPath))}</pre>
                </body></html>
                """;
    }

    private ProjectWorkspace RequireWorkspace(string projectId) =>
        DiscoverWorkspaces().Single(workspace => string.Equals(workspace.ProjectId, projectId, StringComparison.Ordinal));

    private IReadOnlyList<ProjectWorkspace> DiscoverWorkspaces() =>
        Directory.Exists(_workspacesRootPath)
            ? _workspaceDiscovery.Discover(_workspacesRootPath)
            : [];

    private static WorkspaceFile ResolveFile(ProjectWorkspace workspace, string relativePath)
    {
        var normalizedRelativePath = relativePath.Replace('\\', '/');
        var fullPath = Path.GetFullPath(Path.Combine(workspace.RootPath, normalizedRelativePath));
        if (!fullPath.StartsWith(workspace.RootPath, StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException("Requested file path escapes the workspace root.");
        }

        return new WorkspaceFile(normalizedRelativePath, fullPath);
    }

    private static IReadOnlyList<WorkspaceFile> EnumerateWorkspaceFiles(ProjectWorkspace workspace) =>
        Directory
            .EnumerateFiles(workspace.RootPath, "*.md", SearchOption.AllDirectories)
            .Select(path => new WorkspaceFile(
                Path.GetRelativePath(workspace.RootPath, path).Replace('\\', '/'),
                path))
            .OrderBy(file => file.RelativePath, StringComparer.Ordinal)
            .ToArray();

    private sealed record WorkspaceFile(string RelativePath, string FullPath);
}
