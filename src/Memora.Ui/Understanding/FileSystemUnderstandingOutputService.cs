using System.Net;
using System.Text;
using Microsoft.Data.Sqlite;
using Memora.Context.Assembly;
using Memora.Context.Models;
using Memora.Core.Artifacts;
using Memora.Index.Rebuild;
using Memora.Index.Relationships;
using Memora.Index.Traceability;
using Memora.Storage.Parsing;
using Memora.Storage.Workspaces;

namespace Memora.Ui.Understanding;

internal sealed class FileSystemUnderstandingOutputService
{
    private readonly string _workspacesRootPath;
    private readonly WorkspaceDiscovery _workspaceDiscovery = new();
    private readonly ArtifactMarkdownParser _markdownParser = new();
    private readonly ContextBundleBuilder _contextBundleBuilder = new();
    private readonly SqliteIndexRebuilder _indexRebuilder = new();
    private readonly TraceabilityQueryService _traceabilityQueryService = new();
    private readonly ArtifactRelationshipIndex _relationshipIndex = new();

    public FileSystemUnderstandingOutputService(string workspacesRootPath)
    {
        _workspacesRootPath = Path.GetFullPath(workspacesRootPath ?? throw new ArgumentNullException(nameof(workspacesRootPath)));
    }

    public UnderstandingPageModel BuildPage(UnderstandingRequest request)
    {
        var workspace = FindWorkspace(request.ProjectId);
        if (workspace is null)
        {
            return new UnderstandingPageModel(
                request.ProjectId,
                request.TaskDescription,
                request.ArtifactId,
                request.TraceabilityKind,
                request.IncludeDraftArtifacts,
                request.IncludeLayer3History,
                $"Project '{request.ProjectId}' was not found in '{_workspacesRootPath}'.",
                null,
                null,
                null);
        }

        var contextArtifacts = LoadArtifacts(workspace, request.IncludeDraftArtifacts, request.IncludeLayer3History);
        var bundle = _contextBundleBuilder.Build(
            new ContextBundleRequest(
                request.ProjectId,
                request.TaskDescription,
                request.IncludeDraftArtifacts,
                request.IncludeLayer3History),
            contextArtifacts);

        var approvedArtifacts = LoadArtifacts(workspace, includeDrafts: false, includeSummaries: false)
            .Where(artifact => artifact.Status == ArtifactStatus.Approved)
            .GroupBy(artifact => artifact.Id, StringComparer.Ordinal)
            .ToDictionary(
                group => group.Key,
                group => group
                    .OrderByDescending(artifact => artifact.Revision)
                    .ThenByDescending(artifact => artifact.UpdatedAtUtc)
                    .First(),
                StringComparer.Ordinal);

        var resolvedArtifactId = ResolveArtifactId(request.ArtifactId, bundle, approvedArtifacts);
        var contextView = BuildContextView(bundle);

        if (resolvedArtifactId is null)
        {
            return new UnderstandingPageModel(
                request.ProjectId,
                request.TaskDescription,
                request.ArtifactId,
                request.TraceabilityKind,
                request.IncludeDraftArtifacts,
                request.IncludeLayer3History,
                "No approved canonical artifact is available for traceability and component understanding output.",
                contextView,
                null,
                null);
        }

        var isolatedRootPath = CreateIsolatedWorkspaceRoot(workspace);

        try
        {
            using var connection = new SqliteConnection("Data Source=:memory:");
            connection.Open();

            var rebuildResult = _indexRebuilder.Rebuild(connection, isolatedRootPath);
            if (!rebuildResult.Success)
            {
                return new UnderstandingPageModel(
                    request.ProjectId,
                    request.TaskDescription,
                    resolvedArtifactId,
                    request.TraceabilityKind,
                    request.IncludeDraftArtifacts,
                    request.IncludeLayer3History,
                    $"Understanding output rebuild failed: {rebuildResult.Diagnostics[0].Message}",
                    contextView,
                    null,
                    null);
            }

            var traceabilityResult = _traceabilityQueryService.Query(
                connection,
                new TraceabilityQuery(request.ProjectId, resolvedArtifactId, request.TraceabilityKind));

            if (!approvedArtifacts.TryGetValue(resolvedArtifactId, out var anchorArtifact))
            {
                return new UnderstandingPageModel(
                    request.ProjectId,
                    request.TaskDescription,
                    resolvedArtifactId,
                    request.TraceabilityKind,
                    request.IncludeDraftArtifacts,
                    request.IncludeLayer3History,
                    $"Approved artifact '{resolvedArtifactId}' was not found in project '{request.ProjectId}'.",
                    contextView,
                    BuildTraceabilityView(traceabilityResult),
                    null);
            }

            return new UnderstandingPageModel(
                request.ProjectId,
                request.TaskDescription,
                resolvedArtifactId,
                request.TraceabilityKind,
                request.IncludeDraftArtifacts,
                request.IncludeLayer3History,
                null,
                contextView,
                BuildTraceabilityView(traceabilityResult),
                BuildComponentSummary(connection, bundle, anchorArtifact));
        }
        finally
        {
            if (Directory.Exists(isolatedRootPath))
            {
                Directory.Delete(isolatedRootPath, recursive: true);
            }
        }
    }

    public string RenderPage(UnderstandingPageModel model)
    {
        var html = new StringBuilder();
        html.AppendLine("<!doctype html>");
        html.AppendLine("<html><head><meta charset=\"utf-8\"><title>Memora Understanding Outputs</title>");
        html.AppendLine("<style>body{font-family:Segoe UI,Arial,sans-serif;margin:2rem;line-height:1.5;background:#f7f4ec;color:#1e1a14;}form{display:grid;gap:.75rem;max-width:42rem;margin-bottom:2rem;padding:1.25rem;border:1px solid #d8cdbb;border-radius:.75rem;background:#fffaf1;}label{display:grid;gap:.25rem;font-weight:600;}input[type=text],select{padding:.65rem;border:1px solid #b9ab95;border-radius:.4rem;background:#fffdfa;}section{margin-top:1.5rem;padding:1rem 1.25rem;border:1px solid #d8cdbb;border-radius:.75rem;background:#fffdf8;}article{border:1px solid #eadfcb;border-radius:.6rem;padding:1rem;margin:.75rem 0;background:#fff;}small{color:#655949;}ul{margin:.5rem 0 0 1.25rem;}button{padding:.7rem 1rem;border:none;border-radius:.4rem;background:#8b4f2a;color:#fff;font-weight:600;cursor:pointer;}code{background:#f2e9dc;padding:.1rem .3rem;border-radius:.25rem;}h1,h2,h3{font-family:Georgia,\"Times New Roman\",serif;} .summary-grid{display:grid;grid-template-columns:repeat(auto-fit,minmax(14rem,1fr));gap:.75rem;} .fact{padding:.75rem;border:1px solid #eadfcb;border-radius:.5rem;background:#fff;} .empty{color:#655949;font-style:italic;}</style>");
        html.AppendLine("</head><body>");
        html.AppendLine("<h1>Understanding Outputs</h1>");
        html.AppendLine("<p>Generate a grounded context view, traceability view, and component summary from approved artifacts and the shared deterministic retrieval path.</p>");
        html.AppendLine("<form method=\"get\" action=\"/understanding\">");
        html.AppendLine($"<label>Project Id<input type=\"text\" name=\"projectId\" value=\"{WebUtility.HtmlEncode(model.ProjectId ?? string.Empty)}\" /></label>");
        html.AppendLine($"<label>Task Description<input type=\"text\" name=\"taskDescription\" value=\"{WebUtility.HtmlEncode(model.TaskDescription ?? string.Empty)}\" /></label>");
        html.AppendLine($"<label>Anchor Artifact Id<input type=\"text\" name=\"artifactId\" value=\"{WebUtility.HtmlEncode(model.ArtifactId ?? string.Empty)}\" /></label>");
        html.AppendLine("<label>Traceability View<select name=\"traceabilityKind\">");

        foreach (var kind in Enum.GetValues<TraceabilityQueryKind>())
        {
            var selected = kind == model.TraceabilityKind ? "selected" : string.Empty;
            html.AppendLine($"<option value=\"{kind}\" {selected}>{WebUtility.HtmlEncode(kind.ToString())}</option>");
        }

        html.AppendLine("</select></label>");
        html.AppendLine($"<label><input type=\"checkbox\" name=\"includeDraftArtifacts\" value=\"true\" {(model.IncludeDraftArtifacts ? "checked" : string.Empty)} /> Include draft artifacts in context view</label>");
        html.AppendLine($"<label><input type=\"checkbox\" name=\"includeLayer3History\" value=\"true\" {(model.IncludeLayer3History ? "checked" : string.Empty)} /> Include Layer 3 history in context view</label>");
        html.AppendLine("<button type=\"submit\">Build Understanding Output</button>");
        html.AppendLine("</form>");

        if (!string.IsNullOrWhiteSpace(model.ErrorMessage))
        {
            html.AppendLine($"<p><strong>Error:</strong> {WebUtility.HtmlEncode(model.ErrorMessage)}</p>");
        }

        if (model.ContextView is not null)
        {
            html.AppendLine("<section><h2>Context View</h2>");
            html.AppendLine($"<p><small>{model.ContextView.ArtifactCount} artifacts selected by the shared context builder.</small></p>");

            foreach (var layer in model.ContextView.Layers)
            {
                html.AppendLine($"<article><h3>{WebUtility.HtmlEncode(layer.Name)}</h3>");
                if (layer.Artifacts.Count == 0)
                {
                    html.AppendLine("<p class=\"empty\">No artifacts selected for this layer.</p></article>");
                    continue;
                }

                foreach (var artifact in layer.Artifacts)
                {
                    html.AppendLine("<div class=\"fact\">");
                    html.AppendLine($"<strong>{WebUtility.HtmlEncode(artifact.Id)} - {WebUtility.HtmlEncode(artifact.Title)}</strong>");
                    html.AppendLine($"<p><small>{WebUtility.HtmlEncode(artifact.Type)} | {WebUtility.HtmlEncode(artifact.Status)} | {WebUtility.HtmlEncode(artifact.Origin)}</small></p>");
                    html.AppendLine("<ul>");
                    foreach (var reason in artifact.InclusionReasons)
                    {
                        html.AppendLine($"<li>{WebUtility.HtmlEncode(reason)}</li>");
                    }

                    html.AppendLine("</ul></div>");
                }

                html.AppendLine("</article>");
            }

            html.AppendLine("</section>");
        }

        if (model.TraceabilityView is not null)
        {
            html.AppendLine("<section><h2>Traceability View</h2>");
            html.AppendLine($"<p><small>Anchor artifact: <code>{WebUtility.HtmlEncode(model.TraceabilityView.ArtifactId)}</code> | query: {WebUtility.HtmlEncode(model.TraceabilityView.QueryKind)}</small></p>");

            if (model.TraceabilityView.Errors.Count > 0)
            {
                html.AppendLine("<ul>");
                foreach (var error in model.TraceabilityView.Errors)
                {
                    html.AppendLine($"<li>{WebUtility.HtmlEncode(error.Message)}</li>");
                }

                html.AppendLine("</ul>");
            }
            else if (model.TraceabilityView.Paths.Count == 0)
            {
                html.AppendLine("<p class=\"empty\">No traceability paths were found for this query.</p>");
            }
            else
            {
                foreach (var path in model.TraceabilityView.Paths)
                {
                    html.AppendLine($"<article><h3>Path {path.Index}</h3>");
                    html.AppendLine($"<p><small>Artifacts: {WebUtility.HtmlEncode(string.Join(" -> ", path.ArtifactIds))}</small></p>");
                    html.AppendLine("<ul>");
                    foreach (var segment in path.Segments)
                    {
                        html.AppendLine($"<li>{WebUtility.HtmlEncode(segment.SourceArtifactId)} {WebUtility.HtmlEncode(segment.Relationship)} {WebUtility.HtmlEncode(segment.TargetArtifactId)} <small>({WebUtility.HtmlEncode(segment.Direction)})</small></li>");
                    }

                    html.AppendLine("</ul></article>");
                }
            }

            html.AppendLine("</section>");
        }

        if (model.ComponentSummary is not null)
        {
            html.AppendLine("<section><h2>Component Summary</h2>");
            html.AppendLine($"<p><strong>{WebUtility.HtmlEncode(model.ComponentSummary.ArtifactId)} - {WebUtility.HtmlEncode(model.ComponentSummary.Title)}</strong></p>");
            html.AppendLine("<div class=\"summary-grid\">");
            html.AppendLine($"<div class=\"fact\"><strong>Type</strong><div>{WebUtility.HtmlEncode(model.ComponentSummary.Type)}</div></div>");
            html.AppendLine($"<div class=\"fact\"><strong>Status</strong><div>{WebUtility.HtmlEncode(model.ComponentSummary.Status)}</div></div>");
            html.AppendLine($"<div class=\"fact\"><strong>Revision</strong><div>{model.ComponentSummary.Revision}</div></div>");
            html.AppendLine($"<div class=\"fact\"><strong>Context placement</strong><div>{WebUtility.HtmlEncode(model.ComponentSummary.ContextLayer ?? "Not selected")}</div></div>");
            html.AppendLine("</div>");
            html.AppendLine($"<p><small>Provenance: {WebUtility.HtmlEncode(model.ComponentSummary.Provenance)} | Reason: {WebUtility.HtmlEncode(model.ComponentSummary.Reason)}</small></p>");

            html.AppendLine("<article><h3>Context placement</h3>");
            if (model.ComponentSummary.ContextInclusionReasons.Count == 0)
            {
                html.AppendLine("<p class=\"empty\">This artifact was not selected in the current context bundle.</p>");
            }
            else
            {
                html.AppendLine("<ul>");
                foreach (var reason in model.ComponentSummary.ContextInclusionReasons)
                {
                    html.AppendLine($"<li>{WebUtility.HtmlEncode(reason)}</li>");
                }

                html.AppendLine("</ul>");
            }

            html.AppendLine("</article>");

            html.AppendLine("<article><h3>Outgoing relationships</h3>");
            AppendRelationshipList(html, model.ComponentSummary.OutgoingRelationships, "No outgoing approved relationships.");
            html.AppendLine("</article>");

            html.AppendLine("<article><h3>Incoming relationships</h3>");
            AppendRelationshipList(html, model.ComponentSummary.IncomingRelationships, "No incoming approved relationships.");
            html.AppendLine("</article>");

            html.AppendLine("<article><h3>Key sections</h3>");
            if (model.ComponentSummary.KeySections.Count == 0)
            {
                html.AppendLine("<p class=\"empty\">No structured sections were parsed from this artifact.</p>");
            }
            else
            {
                foreach (var section in model.ComponentSummary.KeySections)
                {
                    html.AppendLine($"<p><strong>{WebUtility.HtmlEncode(section.Name)}</strong><br />{WebUtility.HtmlEncode(section.Content)}</p>");
                }
            }

            html.AppendLine("</article>");

            html.AppendLine("<article><h3>Traceability highlights</h3><ul>");
            foreach (var highlight in model.ComponentSummary.TraceabilityHighlights)
            {
                html.AppendLine($"<li>{WebUtility.HtmlEncode(highlight)}</li>");
            }

            html.AppendLine("</ul></article>");
            html.AppendLine("</section>");
        }

        html.AppendLine("</body></html>");
        return html.ToString();
    }

    private static void AppendRelationshipList(StringBuilder html, IReadOnlyList<ComponentRelationshipView> relationships, string emptyMessage)
    {
        if (relationships.Count == 0)
        {
            html.AppendLine($"<p class=\"empty\">{WebUtility.HtmlEncode(emptyMessage)}</p>");
            return;
        }

        html.AppendLine("<ul>");
        foreach (var relationship in relationships)
        {
            html.AppendLine($"<li>{WebUtility.HtmlEncode(relationship.ArtifactId)} <small>{WebUtility.HtmlEncode(relationship.Direction)}</small> {WebUtility.HtmlEncode(relationship.Relationship)}</li>");
        }

        html.AppendLine("</ul>");
    }

    private ContextUnderstandingView BuildContextView(ContextBundle bundle) =>
        new(
            bundle.ArtifactCount,
            bundle.Layers.Select(layer =>
                new ContextUnderstandingLayer(
                    FormatLayer(layer.Kind),
                    layer.Artifacts.Select(artifact =>
                        new ContextUnderstandingArtifact(
                            artifact.Artifact.Id,
                            artifact.Artifact.Title,
                            artifact.Artifact.Type.ToSchemaValue(),
                            artifact.Artifact.Status.ToSchemaValue(),
                            FormatOrigin(artifact.Origin),
                            artifact.InclusionReasons.Select(reason => reason.Description).ToArray()))
                        .ToArray()))
                .ToArray());

    private TraceabilityUnderstandingView BuildTraceabilityView(TraceabilityQueryResult result) =>
        new(
            result.Query.ArtifactId,
            result.Query.Kind.ToString(),
            result.Paths.Select((path, index) =>
                new TraceabilityUnderstandingPath(
                    index + 1,
                    path.ArtifactIds,
                    path.Segments.Select(segment =>
                        new TraceabilityUnderstandingSegment(
                            segment.SourceArtifactId,
                            FormatRelationship(segment.RelationshipKind),
                            segment.TargetArtifactId,
                            segment.TraversalDirection == TraceabilityTraversalDirection.Outgoing ? "outgoing" : "incoming"))
                        .ToArray()))
                .ToArray(),
            result.Errors);

    private ComponentUnderstandingSummary BuildComponentSummary(
        SqliteConnection connection,
        ContextBundle bundle,
        ArtifactDocument artifact)
    {
        var contextEntry = bundle.Layers
            .SelectMany(layer => layer.Artifacts.Select(entry => (Layer: layer.Kind, Entry: entry)))
            .FirstOrDefault(candidate => string.Equals(candidate.Entry.Artifact.Id, artifact.Id, StringComparison.Ordinal));

        var outgoingRelationships = _relationshipIndex
            .GetOutgoingRelationships(connection, artifact.ProjectId, artifact.Id)
            .Select(relationship => new ComponentRelationshipView(
                relationship.TargetArtifactId,
                FormatRelationship(relationship.Kind),
                "outgoing"))
            .ToArray();

        var incomingRelationships = _relationshipIndex
            .GetIncomingRelationships(connection, artifact.ProjectId, artifact.Id)
            .Select(relationship => new ComponentRelationshipView(
                relationship.SourceArtifactId,
                FormatRelationship(relationship.Kind),
                "incoming"))
            .ToArray();

        var traceabilityHighlights = Enum
            .GetValues<TraceabilityQueryKind>()
            .Select(kind => _traceabilityQueryService.Query(connection, new TraceabilityQuery(artifact.ProjectId, artifact.Id, kind)))
            .Select(result => $"{result.Query.Kind} paths: {result.Paths.Count}")
            .ToArray();

        return new ComponentUnderstandingSummary(
            artifact.Id,
            artifact.Title,
            artifact.Type.ToSchemaValue(),
            artifact.Status.ToSchemaValue(),
            artifact.Revision,
            artifact.Tags.OrderBy(tag => tag, StringComparer.Ordinal).ToArray(),
            artifact.Provenance,
            artifact.Reason,
            contextEntry == default ? null : FormatLayer(contextEntry.Layer),
            contextEntry == default
                ? []
                : contextEntry.Entry.InclusionReasons.Select(reason => reason.Description).ToArray(),
            outgoingRelationships,
            incomingRelationships,
            artifact.Sections
                .OrderBy(section => section.Key, StringComparer.Ordinal)
                .Take(4)
                .Select(section => new ComponentSectionView(section.Key, section.Value))
                .ToArray(),
            traceabilityHighlights);
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

    private static string? ResolveArtifactId(
        string? requestedArtifactId,
        ContextBundle bundle,
        IReadOnlyDictionary<string, ArtifactDocument> approvedArtifacts)
    {
        if (!string.IsNullOrWhiteSpace(requestedArtifactId))
        {
            return requestedArtifactId.Trim();
        }

        var contextArtifactId = bundle.Artifacts
            .Select(entry => entry.Artifact.Id)
            .FirstOrDefault(approvedArtifacts.ContainsKey);

        if (!string.IsNullOrWhiteSpace(contextArtifactId))
        {
            return contextArtifactId;
        }

        return approvedArtifacts.Keys
            .OrderBy(id => id, StringComparer.Ordinal)
            .FirstOrDefault();
    }

    private static string CreateIsolatedWorkspaceRoot(ProjectWorkspace workspace)
    {
        var isolatedRootPath = Path.Combine(
            Path.GetTempPath(),
            "memora-understanding-output",
            Guid.NewGuid().ToString("N"));

        var targetWorkspacePath = Path.Combine(isolatedRootPath, workspace.ProjectId);
        CopyDirectory(workspace.RootPath, targetWorkspacePath);
        return isolatedRootPath;
    }

    private static void CopyDirectory(string sourceDirectory, string targetDirectory)
    {
        Directory.CreateDirectory(targetDirectory);

        foreach (var filePath in Directory.EnumerateFiles(sourceDirectory))
        {
            File.Copy(filePath, Path.Combine(targetDirectory, Path.GetFileName(filePath)), overwrite: true);
        }

        foreach (var directoryPath in Directory.EnumerateDirectories(sourceDirectory))
        {
            CopyDirectory(
                directoryPath,
                Path.Combine(targetDirectory, Path.GetFileName(directoryPath)));
        }
    }

    private static string FormatLayer(ContextLayerKind kind) =>
        kind switch
        {
            ContextLayerKind.Layer1 => "Layer1",
            ContextLayerKind.Layer2 => "Layer2",
            ContextLayerKind.Layer3 => "Layer3",
            _ => kind.ToString()
        };

    private static string FormatOrigin(ContextArtifactOrigin origin) =>
        origin switch
        {
            ContextArtifactOrigin.CanonicalApproved => "canonical approved",
            ContextArtifactOrigin.DraftProposal => "draft proposal",
            ContextArtifactOrigin.SessionSummary => "session summary",
            _ => origin.ToString()
        };

    private static string FormatRelationship(ArtifactRelationshipKind kind) =>
        kind switch
        {
            ArtifactRelationshipKind.DependsOn => "depends on",
            ArtifactRelationshipKind.Affects => "affects",
            ArtifactRelationshipKind.DerivedFrom => "derived from",
            ArtifactRelationshipKind.Supersedes => "supersedes",
            _ => kind.ToString()
        };
}
