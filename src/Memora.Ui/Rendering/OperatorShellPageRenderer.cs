using System.Globalization;
using System.Net;
using System.Text;
using Memora.Core.Artifacts;
using Memora.Ui.Operator;

namespace Memora.Ui.Rendering;

internal static class OperatorShellPageRenderer
{
    public static string RenderHome(
        OperatorShellOptions options,
        IReadOnlyList<OperatorProjectSummary> projects)
    {
        var body = new StringBuilder();
        body.AppendLine("<section class=\"hero\">");
        body.AppendLine("<p class=\"eyebrow\">Memora Human Loop</p>");
        body.AppendLine("<h1>Minimal local operator shell</h1>");
        body.AppendLine("<p class=\"lede\">Browse workspace artifacts, inspect draft revisions, and review the current approval queue without claiming more workflow completeness than Milestone 2 actually provides.</p>");
        body.AppendLine("</section>");

        body.AppendLine("<section class=\"panel\">");
        body.AppendLine("<div class=\"panel-header\">");
        body.AppendLine("<h2>Select a project</h2>");
        body.AppendLine($"<p class=\"muted\">Workspace root: {Encode(options.NormalizedWorkspacesRootPath)}</p>");
        body.AppendLine("</div>");
        body.AppendLine("<div class=\"project-grid\">");

        foreach (var project in projects)
        {
            body.AppendLine("<article class=\"project-card\">");
            body.AppendLine($"<h3><a href=\"/projects/{Encode(project.ProjectId)}\">{Encode(project.Name)}</a></h3>");
            body.AppendLine($"<p class=\"muted\">{Encode(project.ProjectId)}</p>");
            body.AppendLine($"<p>{Encode(project.ArtifactCount.ToString(CultureInfo.InvariantCulture))} artifacts, {Encode(project.PendingCount.ToString(CultureInfo.InvariantCulture))} pending review.</p>");
            body.AppendLine($"<p class=\"muted\">Status: {Encode(project.Status ?? "unspecified")}</p>");
            body.AppendLine("</article>");
        }

        body.AppendLine("</div>");
        body.AppendLine("</section>");
        body.AppendLine(RenderScopeNote(options));

        return RenderLayout("Memora.Ui", options, projects, null, body.ToString());
    }

    public static string RenderProject(
        OperatorShellOptions options,
        IReadOnlyList<OperatorProjectSummary> projects,
        OperatorProjectSnapshot snapshot)
    {
        var body = new StringBuilder();
        body.AppendLine("<section class=\"hero compact\">");
        body.AppendLine($"<p class=\"eyebrow\">Project</p><h1>{Encode(snapshot.Workspace.Metadata.Name)}</h1>");
        body.AppendLine($"<p class=\"lede\">Project id <code>{Encode(snapshot.Workspace.ProjectId)}</code> from <code>{Encode(snapshot.Workspace.RootPath)}</code>.</p>");
        body.AppendLine("</section>");

        body.AppendLine("<section class=\"two-up\">");
        body.AppendLine("<article class=\"panel\">");
        body.AppendLine("<div class=\"panel-header\">");
        body.AppendLine("<h2>Artifact Browser</h2>");
        body.AppendLine("<p class=\"muted\">Every artifact file discovered from canonical, drafts, and summaries.</p>");
        body.AppendLine("</div>");
        body.AppendLine("<table><thead><tr><th>Title</th><th>Type</th><th>Status</th><th>Revision</th><th>File</th></tr></thead><tbody>");

        foreach (var record in snapshot.Artifacts)
        {
            var artifactLink = BuildArtifactLink(snapshot.Workspace.ProjectId, record.RelativePath);
            body.AppendLine("<tr>");
            body.AppendLine($"<td><a href=\"{artifactLink}\">{Encode(record.Artifact.Title)}</a></td>");
            body.AppendLine($"<td>{Encode(record.Artifact.Type.ToSchemaValue())}</td>");
            body.AppendLine($"<td>{RenderStatusBadge(record.Artifact.Status)}</td>");
            body.AppendLine($"<td>{Encode(record.Artifact.Revision.ToString(CultureInfo.InvariantCulture))}</td>");
            body.AppendLine($"<td><code>{Encode(record.RelativePath)}</code></td>");
            body.AppendLine("</tr>");
        }

        body.AppendLine("</tbody></table>");
        body.AppendLine("</article>");

        body.AppendLine("<article class=\"panel\">");
        body.AppendLine("<div class=\"panel-header\">");
        body.AppendLine("<h2>Approval Queue</h2>");
        body.AppendLine("<p class=\"muted\">Preview the current pending artifacts in core queue order.</p>");
        body.AppendLine("</div>");

        if (snapshot.PendingItems.Count == 0)
        {
            body.AppendLine("<p>No draft or proposed artifacts are waiting for review.</p>");
        }
        else
        {
            body.AppendLine("<ul class=\"list\">");
            foreach (var item in snapshot.PendingItems)
            {
                var reviewLink = BuildReviewLink(snapshot.Workspace.ProjectId, item.Record.RelativePath);
                body.AppendLine("<li>");
                body.AppendLine($"<a href=\"{reviewLink}\">{Encode(item.QueueItem.Title)}</a> ");
                body.AppendLine($"<span class=\"muted\">{Encode(item.QueueItem.ArtifactType.ToSchemaValue())} · rev {Encode(item.QueueItem.Revision.ToString(CultureInfo.InvariantCulture))}</span>");
                body.AppendLine("</li>");
            }

            body.AppendLine("</ul>");
            body.AppendLine($"<p><a class=\"button ghost\" href=\"/projects/{Encode(snapshot.Workspace.ProjectId)}/queue\">Open review queue</a></p>");
        }

        body.AppendLine("</article>");
        body.AppendLine("</section>");
        body.AppendLine(RenderScopeNote(options));

        return RenderLayout(snapshot.Workspace.Metadata.Name, options, projects, snapshot.Workspace.ProjectId, body.ToString());
    }

    public static string RenderArtifact(
        OperatorShellOptions options,
        IReadOnlyList<OperatorProjectSummary> projects,
        OperatorArtifactView view,
        IReadOnlyList<string> validationErrors)
    {
        var body = new StringBuilder();
        var artifact = view.SelectedArtifact.Artifact;

        body.AppendLine("<section class=\"hero compact\">");
        body.AppendLine($"<p class=\"eyebrow\">Artifact</p><h1>{Encode(artifact.Title)}</h1>");
        body.AppendLine($"<p class=\"lede\">{Encode(artifact.Type.ToSchemaValue())} · {Encode(artifact.Status.ToSchemaValue())} · revision {Encode(artifact.Revision.ToString(CultureInfo.InvariantCulture))}</p>");
        body.AppendLine("</section>");

        if (validationErrors.Count > 0)
        {
            body.AppendLine("<section class=\"panel alert\">");
            body.AppendLine("<h2>Draft edit validation</h2>");
            body.AppendLine("<ul class=\"list\">");
            foreach (var error in validationErrors)
            {
                body.AppendLine($"<li>{Encode(error)}</li>");
            }

            body.AppendLine("</ul>");
            body.AppendLine("</section>");
        }

        body.AppendLine("<section class=\"two-up\">");
        body.AppendLine("<article class=\"panel\">");
        body.AppendLine("<div class=\"panel-header\"><h2>Artifact Detail</h2><p class=\"muted\">Filesystem-backed record view.</p></div>");
        body.AppendLine(RenderArtifactSummary(view));
        body.AppendLine("</article>");

        body.AppendLine("<article class=\"panel\">");
        body.AppendLine("<div class=\"panel-header\"><h2>Sections</h2><p class=\"muted\">Structured section values from the markdown body.</p></div>");
        body.AppendLine(RenderSections(artifact.Sections));
        body.AppendLine("</article>");
        body.AppendLine("</section>");

        if (view.SelectedArtifact.IsPendingReview)
        {
            body.AppendLine("<section class=\"panel\">");
            body.AppendLine("<div class=\"panel-header\"><h2>Edit Draft</h2><p class=\"muted\">Edits create a new draft revision through the core editing flow.</p></div>");
            body.AppendLine(RenderEditForm(view));
            body.AppendLine("</section>");
        }

        body.AppendLine(RenderScopeNote(options));

        return RenderLayout(artifact.Title, options, projects, view.Project.Workspace.ProjectId, body.ToString());
    }

    public static string RenderQueue(
        OperatorShellOptions options,
        IReadOnlyList<OperatorProjectSummary> projects,
        OperatorProjectSnapshot snapshot)
    {
        var body = new StringBuilder();
        body.AppendLine("<section class=\"hero compact\">");
        body.AppendLine("<p class=\"eyebrow\">Approval Queue</p>");
        body.AppendLine($"<h1>{Encode(snapshot.Workspace.Metadata.Name)}</h1>");
        body.AppendLine($"<p class=\"lede\">{Encode(snapshot.PendingItems.Count.ToString(CultureInfo.InvariantCulture))} pending review item(s) from the core queue model, ready for operator inspection.</p>");
        body.AppendLine("</section>");

        body.AppendLine("<section class=\"panel\">");
        body.AppendLine("<div class=\"panel-header\"><h2>Pending Items</h2><p class=\"muted\">Queue ordering comes from <code>ApprovalQueueBuilder</code>.</p></div>");

        if (snapshot.PendingItems.Count == 0)
        {
            body.AppendLine("<p>No draft or proposed artifacts are queued.</p>");
        }
        else
        {
            body.AppendLine("<table><thead><tr><th>Position</th><th>Title</th><th>Status</th><th>Type</th><th>Pending Since</th><th>Review</th></tr></thead><tbody>");
            for (var index = 0; index < snapshot.PendingItems.Count; index++)
            {
                var item = snapshot.PendingItems[index];
                var reviewLink = BuildReviewLink(snapshot.Workspace.ProjectId, item.Record.RelativePath);
                body.AppendLine("<tr>");
                body.AppendLine($"<td>{Encode((index + 1).ToString(CultureInfo.InvariantCulture))} of {Encode(snapshot.PendingItems.Count.ToString(CultureInfo.InvariantCulture))}</td>");
                body.AppendLine($"<td>{Encode(item.QueueItem.Title)}</td>");
                body.AppendLine($"<td>{RenderStatusBadge(item.QueueItem.PendingStatus)}</td>");
                body.AppendLine($"<td>{Encode(item.QueueItem.ArtifactType.ToSchemaValue())}</td>");
                body.AppendLine($"<td>{Encode(item.QueueItem.PendingSinceUtc.ToString("yyyy-MM-dd HH:mm 'UTC'", CultureInfo.InvariantCulture))}</td>");
                body.AppendLine($"<td><a href=\"{reviewLink}\">Review revision</a></td>");
                body.AppendLine("</tr>");
            }

            body.AppendLine("</tbody></table>");
        }

        body.AppendLine("</section>");
        body.AppendLine(RenderScopeNote(options));

        return RenderLayout($"{snapshot.Workspace.Metadata.Name} queue", options, projects, snapshot.Workspace.ProjectId, body.ToString());
    }

    public static string RenderReview(
        OperatorShellOptions options,
        IReadOnlyList<OperatorProjectSummary> projects,
        OperatorArtifactView view)
    {
        var artifact = view.SelectedArtifact.Artifact;
        var body = new StringBuilder();

        body.AppendLine("<section class=\"hero compact\">");
        body.AppendLine("<p class=\"eyebrow\">Revision Review</p>");
        body.AppendLine($"<h1>{Encode(artifact.Title)}</h1>");
        body.AppendLine($"<p class=\"lede\">Queue review preview for <code>{Encode(view.SelectedArtifact.RelativePath)}</code>.</p>");
        if (view.ReviewQueueContext is not null)
        {
            body.AppendLine($"<p class=\"queue-position\">Review item {Encode(view.ReviewQueueContext.Position.ToString(CultureInfo.InvariantCulture))} of {Encode(view.ReviewQueueContext.TotalItems.ToString(CultureInfo.InvariantCulture))}</p>");
        }
        body.AppendLine("</section>");

        body.AppendLine(RenderReviewNavigation(view));

        body.AppendLine("<section class=\"two-up\">");
        body.AppendLine("<article class=\"panel\">");
        body.AppendLine("<div class=\"panel-header\"><h2>Pending Revision</h2><p class=\"muted\">Current draft or proposed artifact under review.</p></div>");
        body.AppendLine(RenderArtifactSummary(view));
        body.AppendLine($"<p><a class=\"button ghost\" href=\"{BuildArtifactLink(view.Project.Workspace.ProjectId, view.SelectedArtifact.RelativePath)}\">Open artifact detail</a></p>");
        body.AppendLine("</article>");

        body.AppendLine("<article class=\"panel\">");
        body.AppendLine("<div class=\"panel-header\"><h2>Current Approved Revision</h2><p class=\"muted\">Used for diff previews when one exists.</p></div>");
        if (view.CurrentApprovedArtifact is null)
        {
            body.AppendLine("<p>No approved artifact exists for this id yet, so this review is for a net-new artifact.</p>");
        }
        else
        {
            body.AppendLine(RenderApprovedSummary(view.CurrentApprovedArtifact));
        }

        body.AppendLine("</article>");
        body.AppendLine("</section>");

        body.AppendLine("<section class=\"panel\">");
        body.AppendLine("<div class=\"panel-header\"><h2>Revision Diff</h2><p class=\"muted\">Field-level changes from the core diff model.</p></div>");

        if (view.DiffIssues.Count > 0)
        {
            body.AppendLine("<ul class=\"list\">");
            foreach (var issue in view.DiffIssues)
            {
                body.AppendLine($"<li>{Encode(issue)}</li>");
            }

            body.AppendLine("</ul>");
        }
        else if (view.RevisionDiff is null || !view.RevisionDiff.HasChanges)
        {
            body.AppendLine("<p>No field-level diff is available for this review.</p>");
        }
        else
        {
            body.AppendLine("<table><thead><tr><th>Field</th><th>Change</th><th>Before</th><th>After</th></tr></thead><tbody>");
            foreach (var change in view.RevisionDiff.Changes)
            {
                body.AppendLine("<tr>");
                body.AppendLine($"<td><code>{Encode(change.Path)}</code></td>");
                body.AppendLine($"<td>{Encode(change.Kind.ToString().ToLowerInvariant())}</td>");
                body.AppendLine($"<td>{Encode(change.BeforeValue ?? "n/a")}</td>");
                body.AppendLine($"<td>{Encode(change.AfterValue ?? "n/a")}</td>");
                body.AppendLine("</tr>");
            }

            body.AppendLine("</tbody></table>");
        }

        body.AppendLine("</section>");
        body.AppendLine(RenderDecisionPanel(view));
        body.AppendLine("<section class=\"panel note\">");
        body.AppendLine("<h2>Current UI boundary</h2>");
        body.AppendLine("<p>Approval and rejection behavior exists in core, but this shell intentionally stops at review preview for now. That keeps the UI honest until filesystem persistence for those decisions is defined end to end.</p>");
        body.AppendLine("</section>");
        body.AppendLine(RenderScopeNote(options));

        return RenderLayout($"{artifact.Title} review", options, projects, view.Project.Workspace.ProjectId, body.ToString());
    }

    private static string RenderLayout(
        string title,
        OperatorShellOptions options,
        IReadOnlyList<OperatorProjectSummary> projects,
        string? selectedProjectId,
        string body)
    {
        var html = new StringBuilder();
        html.AppendLine("<!DOCTYPE html>");
        html.AppendLine("<html lang=\"en\">");
        html.AppendLine("<head>");
        html.AppendLine("<meta charset=\"utf-8\" />");
        html.AppendLine("<meta name=\"viewport\" content=\"width=device-width, initial-scale=1\" />");
        html.AppendLine($"<title>{Encode(title)}</title>");
        html.AppendLine("<style>");
        html.AppendLine(Styles);
        html.AppendLine("</style>");
        html.AppendLine("</head>");
        html.AppendLine("<body>");
        html.AppendLine("<div class=\"shell\">");
        html.AppendLine("<header class=\"topbar\">");
        html.AppendLine("<div>");
        html.AppendLine("<a class=\"brand\" href=\"/\">Memora.Ui</a>");
        html.AppendLine("<p class=\"topbar-copy\">Human-loop operator shell for local workspace files.</p>");
        html.AppendLine("</div>");
        html.AppendLine(RenderProjectSelector(projects, selectedProjectId));
        html.AppendLine("</header>");
        html.AppendLine("<main>");
        html.AppendLine(body);
        html.AppendLine("</main>");
        html.AppendLine("<footer class=\"footer\">");
        html.AppendLine($"<span>Workspace root: <code>{Encode(options.NormalizedWorkspacesRootPath)}</code></span>");
        html.AppendLine("</footer>");
        html.AppendLine("</div>");
        html.AppendLine("</body>");
        html.AppendLine("</html>");
        return html.ToString();
    }

    private static string RenderProjectSelector(
        IReadOnlyList<OperatorProjectSummary> projects,
        string? selectedProjectId)
    {
        var html = new StringBuilder();
        html.AppendLine("<label class=\"selector\">");
        html.AppendLine("<span>Project Selector</span>");
        html.AppendLine("<select onchange=\"if (this.value) window.location.href = this.value;\">");
        html.AppendLine("<option value=\"/\">Choose a project</option>");

        foreach (var project in projects)
        {
            var isSelected = string.Equals(project.ProjectId, selectedProjectId, StringComparison.Ordinal)
                ? " selected"
                : string.Empty;

            html.AppendLine($"<option value=\"/projects/{Encode(project.ProjectId)}\"{isSelected}>{Encode(project.Name)} ({Encode(project.ProjectId)})</option>");
        }

        html.AppendLine("</select>");
        html.AppendLine("</label>");
        return html.ToString();
    }

    private static string RenderArtifactSummary(OperatorArtifactView view)
    {
        var artifact = view.SelectedArtifact.Artifact;
        var html = new StringBuilder();
        html.AppendLine("<dl class=\"meta-grid\">");
        html.AppendLine($"<div><dt>Id</dt><dd><code>{Encode(artifact.Id)}</code></dd></div>");
        html.AppendLine($"<div><dt>Status</dt><dd>{RenderStatusBadge(artifact.Status)}</dd></div>");
        html.AppendLine($"<div><dt>Revision</dt><dd>{Encode(artifact.Revision.ToString(CultureInfo.InvariantCulture))}</dd></div>");
        html.AppendLine($"<div><dt>Updated</dt><dd>{Encode(artifact.UpdatedAtUtc.ToString("yyyy-MM-dd HH:mm 'UTC'", CultureInfo.InvariantCulture))}</dd></div>");
        html.AppendLine($"<div><dt>Provenance</dt><dd>{Encode(artifact.Provenance)}</dd></div>");
        html.AppendLine($"<div><dt>Reason</dt><dd>{Encode(artifact.Reason)}</dd></div>");
        html.AppendLine($"<div><dt>Tags</dt><dd>{Encode(string.Join(", ", artifact.Tags))}</dd></div>");
        html.AppendLine($"<div><dt>File</dt><dd><code>{Encode(view.SelectedArtifact.RelativePath)}</code></dd></div>");
        html.AppendLine("</dl>");
        html.AppendLine($"<div class=\"body-card\"><h3>Body</h3><pre>{Encode(artifact.Body)}</pre></div>");
        return html.ToString();
    }

    private static string RenderApprovedSummary(ArtifactDocument artifact)
    {
        var html = new StringBuilder();
        html.AppendLine("<dl class=\"meta-grid\">");
        html.AppendLine($"<div><dt>Id</dt><dd><code>{Encode(artifact.Id)}</code></dd></div>");
        html.AppendLine($"<div><dt>Status</dt><dd>{RenderStatusBadge(artifact.Status)}</dd></div>");
        html.AppendLine($"<div><dt>Revision</dt><dd>{Encode(artifact.Revision.ToString(CultureInfo.InvariantCulture))}</dd></div>");
        html.AppendLine($"<div><dt>Updated</dt><dd>{Encode(artifact.UpdatedAtUtc.ToString("yyyy-MM-dd HH:mm 'UTC'", CultureInfo.InvariantCulture))}</dd></div>");
        html.AppendLine("</dl>");
        return html.ToString();
    }

    private static string RenderSections(IReadOnlyDictionary<string, string> sections)
    {
        if (sections.Count == 0)
        {
            return "<p>No structured sections were found.</p>";
        }

        var html = new StringBuilder();
        html.AppendLine("<div class=\"section-stack\">");
        foreach (var pair in sections)
        {
            html.AppendLine("<article class=\"section-card\">");
            html.AppendLine($"<h3>{Encode(pair.Key)}</h3>");
            html.AppendLine($"<pre>{Encode(pair.Value)}</pre>");
            html.AppendLine("</article>");
        }

        html.AppendLine("</div>");
        return html.ToString();
    }

    private static string RenderEditForm(OperatorArtifactView view)
    {
        var artifact = view.SelectedArtifact.Artifact;
        var html = new StringBuilder();
        html.AppendLine($"<form method=\"post\" action=\"/projects/{Encode(view.Project.Workspace.ProjectId)}/artifacts/edit\" class=\"edit-form\">");
        html.AppendLine($"<input type=\"hidden\" name=\"path\" value=\"{Encode(view.SelectedArtifact.RelativePath)}\" />");
        html.AppendLine("<label><span>Title</span>");
        html.AppendLine($"<input type=\"text\" name=\"title\" value=\"{Encode(artifact.Title)}\" /></label>");
        html.AppendLine("<label><span>Reason</span>");
        html.AppendLine($"<input type=\"text\" name=\"reason\" value=\"{Encode(artifact.Reason)}\" /></label>");
        html.AppendLine("<label><span>Tags (comma separated)</span>");
        html.AppendLine($"<input type=\"text\" name=\"tags\" value=\"{Encode(string.Join(", ", artifact.Tags))}\" /></label>");

        foreach (var pair in artifact.Sections)
        {
            html.AppendLine($"<label><span>{Encode(pair.Key)}</span>");
            html.AppendLine($"<textarea name=\"section:{Encode(pair.Key)}\" rows=\"6\">{Encode(pair.Value)}</textarea></label>");
        }

        html.AppendLine("<button class=\"button\" type=\"submit\">Save new draft revision</button>");
        html.AppendLine("</form>");
        return html.ToString();
    }

    private static string RenderReviewNavigation(OperatorArtifactView view)
    {
        var context = view.ReviewQueueContext;
        if (context is null)
        {
            return string.Empty;
        }

        var html = new StringBuilder();
        html.AppendLine("<section class=\"review-nav panel\">");
        html.AppendLine($"<a class=\"button ghost\" href=\"/projects/{Encode(view.Project.Workspace.ProjectId)}/queue\">Back to queue</a>");

        if (context.PreviousItem is null)
        {
            html.AppendLine("<span class=\"button disabled\">Previous item</span>");
        }
        else
        {
            html.AppendLine($"<a class=\"button ghost\" href=\"{BuildReviewLink(view.Project.Workspace.ProjectId, context.PreviousItem.Record.RelativePath)}\">Previous item</a>");
        }

        if (context.NextItem is null)
        {
            html.AppendLine("<span class=\"button disabled\">Next item</span>");
        }
        else
        {
            html.AppendLine($"<a class=\"button ghost\" href=\"{BuildReviewLink(view.Project.Workspace.ProjectId, context.NextItem.Record.RelativePath)}\">Next item</a>");
        }

        html.AppendLine("</section>");
        return html.ToString();
    }

    private static string RenderDecisionPanel(OperatorArtifactView view)
    {
        var artifact = view.SelectedArtifact.Artifact;
        var html = new StringBuilder();
        html.AppendLine("<section class=\"panel decision-panel\">");
        html.AppendLine("<div class=\"panel-header\"><h2>Decision Readiness</h2><p class=\"muted\">Core workflow alignment for this pending artifact.</p></div>");
        html.AppendLine("<dl class=\"meta-grid\">");
        html.AppendLine($"<div><dt>Pending status</dt><dd>{RenderStatusBadge(artifact.Status)}</dd></div>");
        html.AppendLine($"<div><dt>Candidate revision</dt><dd>{Encode(artifact.Revision.ToString(CultureInfo.InvariantCulture))}</dd></div>");
        html.AppendLine($"<div><dt>Approved baseline</dt><dd>{Encode(view.CurrentApprovedArtifact is null ? "none" : "revision " + view.CurrentApprovedArtifact.Revision.ToString(CultureInfo.InvariantCulture))}</dd></div>");
        html.AppendLine($"<div><dt>Diff status</dt><dd>{Encode(view.DiffIssues.Count > 0 ? "needs attention" : view.RevisionDiff is null ? "net new or unavailable" : "ready to inspect")}</dd></div>");
        html.AppendLine("</dl>");
        html.AppendLine("<div class=\"decision-actions\">");
        html.AppendLine("<span class=\"button disabled\">Approve</span>");
        html.AppendLine("<span class=\"button disabled danger\">Reject</span>");
        html.AppendLine("<a class=\"button ghost\" href=\"/projects/" + Encode(view.Project.Workspace.ProjectId) + "/queue\">Return to queue</a>");
        html.AppendLine("</div>");
        html.AppendLine("<p class=\"muted\">The visible decision controls are intentionally inactive until UI persistence can apply the existing core approval workflow without bypassing filesystem-first governance.</p>");
        html.AppendLine("</section>");
        return html.ToString();
    }

    private static string RenderScopeNote(OperatorShellOptions options)
    {
        var rootMode = options.UsesSeededSampleRoot
            ? "The shell is using a writable local copy of the sample workspaces so you can explore without touching the repo fixtures."
            : "The shell is using the configured workspace root directly.";

        return $"<section class=\"panel note\"><h2>Current workflow scope</h2><p>{Encode(rootMode)}</p><p>Draft inspection and editing are wired through current core and storage behavior. Approval review is visible here, while approval and rejection persistence remain outside this UI slice.</p></section>";
    }

    private static string RenderStatusBadge(ArtifactStatus status) =>
        $"<span class=\"badge badge-{status.ToSchemaValue()}\">{Encode(status.ToSchemaValue())}</span>";

    private static string BuildArtifactLink(string projectId, string relativePath) =>
        $"/projects/{Uri.EscapeDataString(projectId)}/artifacts?path={Uri.EscapeDataString(relativePath)}";

    private static string BuildReviewLink(string projectId, string relativePath) =>
        $"/projects/{Uri.EscapeDataString(projectId)}/review?path={Uri.EscapeDataString(relativePath)}";

    private static string Encode(string? value) => WebUtility.HtmlEncode(value ?? string.Empty);

    private const string Styles = """
body { margin: 0; font-family: "Iowan Old Style", "Palatino Linotype", "Book Antiqua", Georgia, serif; background: radial-gradient(circle at top left, rgba(232, 188, 124, 0.22), transparent 30%), linear-gradient(180deg, #f5efe5 0%, #ebe0d0 100%); color: #1f1a16; }
a { color: #7d341f; }
code, pre, select, input, textarea, button { font-family: "Cascadia Code", "Consolas", monospace; }
.shell { max-width: 1180px; margin: 0 auto; padding: 24px; }
.topbar, .footer, .hero, .panel, .project-card, .section-card { backdrop-filter: blur(8px); background: rgba(255, 249, 241, 0.82); border: 1px solid rgba(91, 56, 35, 0.16); box-shadow: 0 18px 40px rgba(57, 35, 21, 0.08); }
.topbar, .footer { border-radius: 24px; padding: 18px 22px; }
.topbar { display: flex; justify-content: space-between; gap: 20px; align-items: end; margin-bottom: 20px; }
.brand { font-size: 1.4rem; font-weight: 700; text-decoration: none; }
.topbar-copy, .muted { color: #695748; }
.selector { display: grid; gap: 8px; min-width: 280px; }
.selector select, input, textarea { width: 100%; border-radius: 14px; border: 1px solid rgba(91, 56, 35, 0.2); padding: 12px 14px; background: rgba(255, 255, 255, 0.86); }
.hero, .panel, .project-card, .section-card { border-radius: 28px; padding: 24px; }
.hero { margin-bottom: 20px; position: relative; overflow: hidden; }
.hero::after { content: ""; position: absolute; inset: auto -60px -60px auto; width: 180px; height: 180px; border-radius: 999px; background: rgba(125, 52, 31, 0.08); }
.hero.compact h1 { font-size: 2.2rem; }
.eyebrow { text-transform: uppercase; letter-spacing: 0.12em; font-size: 0.8rem; color: #8a6041; }
h1, h2, h3 { margin-top: 0; }
.lede { max-width: 70ch; font-size: 1.05rem; }
.panel { margin-bottom: 20px; }
.panel-header { display: flex; justify-content: space-between; gap: 16px; align-items: baseline; margin-bottom: 16px; }
.project-grid, .two-up { display: grid; gap: 20px; }
.project-grid { grid-template-columns: repeat(auto-fit, minmax(220px, 1fr)); }
.two-up { grid-template-columns: repeat(auto-fit, minmax(320px, 1fr)); }
.list { display: grid; gap: 10px; padding-left: 18px; }
.badge { display: inline-flex; padding: 4px 10px; border-radius: 999px; font-size: 0.84rem; text-transform: uppercase; letter-spacing: 0.08em; background: #ead7b6; }
.badge-draft, .badge-proposed { background: #f0c98b; }
.badge-approved { background: #b8d1b0; }
.badge-superseded, .badge-deprecated { background: #d9c6bb; }
table { width: 100%; border-collapse: collapse; }
th, td { text-align: left; vertical-align: top; padding: 12px 10px; border-bottom: 1px solid rgba(91, 56, 35, 0.12); }
.meta-grid { display: grid; grid-template-columns: repeat(auto-fit, minmax(180px, 1fr)); gap: 14px; }
.meta-grid dt { color: #8a6041; font-size: 0.85rem; text-transform: uppercase; letter-spacing: 0.08em; }
.meta-grid dd { margin: 6px 0 0; }
.section-stack { display: grid; gap: 14px; }
.body-card, .section-card { background: rgba(255, 255, 255, 0.72); border-radius: 18px; padding: 16px; border: 1px solid rgba(91, 56, 35, 0.1); }
pre { white-space: pre-wrap; margin: 0; }
.edit-form { display: grid; gap: 16px; }
.edit-form label { display: grid; gap: 8px; }
.button { display: inline-flex; align-items: center; justify-content: center; width: fit-content; border: none; border-radius: 999px; padding: 12px 18px; background: #7d341f; color: #fff8f3; text-decoration: none; cursor: pointer; }
.button.ghost { background: transparent; color: #7d341f; border: 1px solid rgba(125, 52, 31, 0.24); }
.button.disabled { background: #d9c6bb; color: #695748; cursor: not-allowed; }
.button.danger { background: #9d3d30; color: #fff8f3; }
.review-nav, .decision-actions { display: flex; flex-wrap: wrap; gap: 12px; align-items: center; }
.queue-position { display: inline-flex; border: 1px solid rgba(125, 52, 31, 0.18); border-radius: 999px; padding: 8px 12px; background: rgba(255, 255, 255, 0.58); }
.decision-panel { border-color: rgba(91, 56, 35, 0.24); }
.alert { border-color: rgba(146, 50, 40, 0.34); }
.note { background: rgba(245, 232, 210, 0.85); }
.footer { margin-top: 20px; }
@media (max-width: 720px) { .shell { padding: 16px; } .topbar { align-items: stretch; } }
""";
}
