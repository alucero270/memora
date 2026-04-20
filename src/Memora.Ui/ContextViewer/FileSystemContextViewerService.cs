using System.Net;
using System.Text;
using Memora.Context.Assembly;
using Memora.Context.Models;
using Memora.Core.Artifacts;
using Memora.Storage.Parsing;
using Memora.Storage.Workspaces;

namespace Memora.Ui.ContextViewer;

internal sealed class FileSystemContextViewerService
{
    private readonly string _workspacesRootPath;
    private readonly WorkspaceDiscovery _workspaceDiscovery = new();
    private readonly ArtifactMarkdownParser _markdownParser = new();
    private readonly ContextBundleBuilder _contextBundleBuilder = new();

    public FileSystemContextViewerService(string workspacesRootPath)
    {
        _workspacesRootPath = Path.GetFullPath(workspacesRootPath ?? throw new ArgumentNullException(nameof(workspacesRootPath)));
    }

    public ContextViewerPageModel BuildPage(ContextViewerRequest request)
    {
        var workspace = FindWorkspace(request.ProjectId);
        if (workspace is null)
        {
            return new ContextViewerPageModel(
                request.ProjectId,
                request.TaskDescription,
                request.IncludeDraftArtifacts,
                request.IncludeLayer3History,
                $"Project '{request.ProjectId}' was not found in '{_workspacesRootPath}'.",
                []);
        }

        var artifacts = LoadArtifacts(workspace, request.IncludeDraftArtifacts, request.IncludeLayer3History);
        var bundle = _contextBundleBuilder.Build(
            new ContextBundleRequest(
                request.ProjectId,
                request.TaskDescription,
                request.IncludeDraftArtifacts,
                request.IncludeLayer3History),
            artifacts);

        return new ContextViewerPageModel(
            request.ProjectId,
            request.TaskDescription,
            request.IncludeDraftArtifacts,
            request.IncludeLayer3History,
            null,
            bundle.Layers.Select(layer =>
                new ContextViewerLayer(
                    layer.Kind.ToString(),
                    layer.Artifacts.Select(artifact =>
                        new ContextViewerArtifact(
                            artifact.Artifact.Id,
                            artifact.Artifact.Title,
                            artifact.Artifact.Type.ToSchemaValue(),
                            artifact.Artifact.Status.ToSchemaValue(),
                            artifact.InclusionReasons.Select(reason => reason.Description).ToArray()))
                        .ToArray()))
                .ToArray());
    }

    public string RenderPage(ContextViewerPageModel model)
    {
        var html = new StringBuilder();
        html.AppendLine("<!doctype html>");
        html.AppendLine("<html><head><meta charset=\"utf-8\"><title>Memora Context Viewer</title>");
        html.AppendLine("<style>body{font-family:Segoe UI,Arial,sans-serif;margin:2rem;line-height:1.5;}form{display:grid;gap:.75rem;max-width:38rem;margin-bottom:2rem;}label{display:grid;gap:.25rem;font-weight:600;}input[type=text]{padding:.65rem;border:1px solid #bbb;border-radius:.4rem;}fieldset{border:1px solid #ddd;padding:1rem;border-radius:.5rem;}section{margin-top:1.5rem;}article{border:1px solid #ddd;border-radius:.5rem;padding:1rem;margin:.75rem 0;background:#fafafa;}small{color:#555;}ul{margin:.5rem 0 0 1.25rem;}button{padding:.7rem 1rem;border:none;border-radius:.4rem;background:#0f5cc0;color:#fff;font-weight:600;cursor:pointer;}</style>");
        html.AppendLine("</head><body>");
        html.AppendLine("<h1>Context Viewer</h1>");
        html.AppendLine("<p>Inspect the exact layered context bundle and inclusion reasoning the shared context builder produces.</p>");
        html.AppendLine("<form method=\"get\" action=\"/context-viewer\">");
        html.AppendLine($"<label>Project Id<input type=\"text\" name=\"projectId\" value=\"{WebUtility.HtmlEncode(model.ProjectId ?? string.Empty)}\" /></label>");
        html.AppendLine($"<label>Task Description<input type=\"text\" name=\"taskDescription\" value=\"{WebUtility.HtmlEncode(model.TaskDescription ?? string.Empty)}\" /></label>");
        html.AppendLine($"<label><input type=\"checkbox\" name=\"includeDraftArtifacts\" value=\"true\" {(model.IncludeDraftArtifacts ? "checked" : string.Empty)} /> Include draft artifacts</label>");
        html.AppendLine($"<label><input type=\"checkbox\" name=\"includeLayer3History\" value=\"true\" {(model.IncludeLayer3History ? "checked" : string.Empty)} /> Include Layer 3 history</label>");
        html.AppendLine("<button type=\"submit\">Build Context</button>");
        html.AppendLine("</form>");

        if (!string.IsNullOrWhiteSpace(model.ErrorMessage))
        {
            html.AppendLine($"<p><strong>Error:</strong> {WebUtility.HtmlEncode(model.ErrorMessage)}</p>");
        }

        foreach (var layer in model.Layers)
        {
            html.AppendLine($"<section><h2>{WebUtility.HtmlEncode(layer.Name)}</h2>");
            if (layer.Artifacts.Count == 0)
            {
                html.AppendLine("<p><small>No artifacts selected for this layer.</small></p></section>");
                continue;
            }

            foreach (var artifact in layer.Artifacts)
            {
                html.AppendLine("<article>");
                html.AppendLine($"<h3>{WebUtility.HtmlEncode(artifact.Id)} - {WebUtility.HtmlEncode(artifact.Title)}</h3>");
                html.AppendLine($"<p><small>{WebUtility.HtmlEncode(artifact.Type)} | {WebUtility.HtmlEncode(artifact.Status)}</small></p>");
                html.AppendLine("<ul>");
                foreach (var reason in artifact.InclusionReasons)
                {
                    html.AppendLine($"<li>{WebUtility.HtmlEncode(reason)}</li>");
                }

                html.AppendLine("</ul></article>");
            }

            html.AppendLine("</section>");
        }

        html.AppendLine("</body></html>");
        return html.ToString();
    }

    private ProjectWorkspace? FindWorkspace(string projectId)
    {
        if (!Directory.Exists(_workspacesRootPath))
        {
            return null;
        }

        return _workspaceDiscovery
            .Discover(_workspacesRootPath)
            .SingleOrDefault(workspace => string.Equals(workspace.ProjectId, projectId, StringComparison.Ordinal));
    }

    private IReadOnlyList<ArtifactDocument> LoadArtifacts(ProjectWorkspace workspace, bool includeDrafts, bool includeSummaries)
    {
        var files = new List<string>();
        AddMarkdownFiles(files, workspace.CanonicalRootPath);

        if (includeDrafts)
        {
            AddMarkdownFiles(files, workspace.DraftsRootPath);
        }

        if (includeSummaries)
        {
            AddMarkdownFiles(files, workspace.SummariesRootPath);
        }

        files.Sort(StringComparer.Ordinal);

        return files
            .Select(filePath => _markdownParser.Parse(File.ReadAllText(filePath)).Artifact)
            .Where(artifact => artifact is not null)
            .Cast<ArtifactDocument>()
            .ToArray();
    }

    private static void AddMarkdownFiles(ICollection<string> files, string rootPath)
    {
        if (!Directory.Exists(rootPath))
        {
            return;
        }

        foreach (var filePath in Directory.EnumerateFiles(rootPath, "*.md", SearchOption.AllDirectories))
        {
            files.Add(filePath);
        }
    }
}
