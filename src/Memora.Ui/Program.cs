using Memora.Ui.Operator;
using Memora.Ui.Rendering;

var builder = WebApplication.CreateBuilder(args);

var configuredWorkspacesRoot = builder.Configuration["MemoraUi:WorkspacesRoot"];
var shellOptions = string.IsNullOrWhiteSpace(configuredWorkspacesRoot)
    ? new OperatorShellOptions(
        SampleWorkspacesBootstrapper.PrepareDefaultRoot(builder.Environment.ContentRootPath),
        UsesSeededSampleRoot: true)
    : new OperatorShellOptions(
        Path.GetFullPath(configuredWorkspacesRoot),
        UsesSeededSampleRoot: false);

builder.Services.AddSingleton(shellOptions);
builder.Services.AddSingleton<LocalOperatorWorkspaceService>();

var app = builder.Build();

app.MapGet("/", (LocalOperatorWorkspaceService service, OperatorShellOptions options) =>
{
    var projects = service.GetProjects();
    var html = OperatorShellPageRenderer.RenderHome(options, projects);
    return Results.Content(html, "text/html");
});

app.MapGet("/projects/{projectId}", (string projectId, LocalOperatorWorkspaceService service, OperatorShellOptions options) =>
{
    var snapshot = service.TryGetProject(projectId);
    if (snapshot is null)
    {
        return Results.NotFound();
    }

    var html = OperatorShellPageRenderer.RenderProject(options, service.GetProjects(), snapshot);
    return Results.Content(html, "text/html");
});

app.MapGet("/projects/{projectId}/artifacts", (string projectId, string? path, LocalOperatorWorkspaceService service, OperatorShellOptions options) =>
{
    if (string.IsNullOrWhiteSpace(path))
    {
        return Results.Redirect($"/projects/{Uri.EscapeDataString(projectId)}");
    }

    var artifactView = service.TryGetArtifactView(projectId, path);
    if (artifactView is null)
    {
        return Results.NotFound();
    }

    var html = OperatorShellPageRenderer.RenderArtifact(options, service.GetProjects(), artifactView, []);
    return Results.Content(html, "text/html");
});

app.MapPost("/projects/{projectId}/artifacts/edit", async (string projectId, HttpRequest request, LocalOperatorWorkspaceService service, OperatorShellOptions options) =>
{
    var form = await request.ReadFormAsync();
    var relativePath = form["path"].ToString();

    if (string.IsNullOrWhiteSpace(relativePath))
    {
        return Results.BadRequest();
    }

    var editInput = OperatorArtifactEditInput.FromForm(form);
    var result = service.EditDraft(projectId, relativePath, editInput);

    if (result.IsSuccess)
    {
        return Results.Redirect($"/projects/{Uri.EscapeDataString(projectId)}/artifacts?path={Uri.EscapeDataString(result.RelativePath!)}");
    }

    var artifactView = service.TryGetArtifactView(projectId, relativePath);
    if (artifactView is null)
    {
        return Results.NotFound();
    }

    var html = OperatorShellPageRenderer.RenderArtifact(options, service.GetProjects(), artifactView, result.ValidationErrors);
    return Results.Content(html, "text/html", statusCode: StatusCodes.Status400BadRequest);
});

app.MapGet("/projects/{projectId}/queue", (string projectId, LocalOperatorWorkspaceService service, OperatorShellOptions options) =>
{
    var snapshot = service.TryGetProject(projectId);
    if (snapshot is null)
    {
        return Results.NotFound();
    }

    var html = OperatorShellPageRenderer.RenderQueue(options, service.GetProjects(), snapshot);
    return Results.Content(html, "text/html");
});

app.MapGet("/projects/{projectId}/review", (string projectId, string? path, LocalOperatorWorkspaceService service, OperatorShellOptions options) =>
{
    if (string.IsNullOrWhiteSpace(path))
    {
        return Results.Redirect($"/projects/{Uri.EscapeDataString(projectId)}/queue");
    }

    var artifactView = service.TryGetArtifactView(projectId, path);
    if (artifactView is null || !artifactView.SelectedArtifact.IsPendingReview)
    {
        return Results.NotFound();
    }

    var html = OperatorShellPageRenderer.RenderReview(options, service.GetProjects(), artifactView);
    return Results.Content(html, "text/html");
});

app.Run();

public partial class Program;
