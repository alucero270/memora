using Memora.Ui.ContextViewer;
using Memora.Ui.OperatorShell;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSingleton(sp =>
{
    var configuration = sp.GetRequiredService<IConfiguration>();
    var workspacesRootPath = configuration["MemoraUi:WorkspacesRoot"] ??
                             configuration["Memora:WorkspacesRootPath"] ??
                             Environment.GetEnvironmentVariable("MEMORA_WORKSPACES_ROOT") ??
                             Path.GetTempPath();
    return new FileSystemContextViewerService(workspacesRootPath);
});

builder.Services.AddSingleton(sp =>
{
    var configuration = sp.GetRequiredService<IConfiguration>();
    var workspacesRootPath = configuration["MemoraUi:WorkspacesRoot"] ??
                             configuration["Memora:WorkspacesRootPath"] ??
                             Environment.GetEnvironmentVariable("MEMORA_WORKSPACES_ROOT") ??
                             Path.GetTempPath();
    return new OperatorShellService(workspacesRootPath);
});

var app = builder.Build();

app.MapGet(
    "/",
    (OperatorShellService shellService) =>
        Results.Content(shellService.RenderRoot(), "text/html"));

app.MapGet(
    "/projects/{projectId}",
    (string projectId, OperatorShellService shellService) =>
        Results.Content(shellService.RenderProject(projectId), "text/html"));

app.MapGet(
    "/projects/{projectId}/artifacts",
    (string projectId, string path, OperatorShellService shellService) =>
        Results.Content(shellService.RenderArtifact(projectId, path), "text/html"));

app.MapGet(
    "/projects/{projectId}/review",
    (string projectId, string path, OperatorShellService shellService) =>
        Results.Content(shellService.RenderReview(projectId, path), "text/html"));

app.MapGet(
    "/context-viewer",
    (HttpRequest request, FileSystemContextViewerService service) =>
    {
        var projectId = request.Query["projectId"].ToString();
        var taskDescription = request.Query["taskDescription"].ToString();
        var includeDraftArtifacts = string.Equals(request.Query["includeDraftArtifacts"], "true", StringComparison.OrdinalIgnoreCase);
        var includeLayer3History = string.Equals(request.Query["includeLayer3History"], "true", StringComparison.OrdinalIgnoreCase);

        if (string.IsNullOrWhiteSpace(projectId) || string.IsNullOrWhiteSpace(taskDescription))
        {
            var emptyPage = new ContextViewerPageModel(projectId, taskDescription, includeDraftArtifacts, includeLayer3History, null, []);
            return Results.Content(service.RenderPage(emptyPage), "text/html");
        }

        var page = service.BuildPage(new ContextViewerRequest(projectId, taskDescription, includeDraftArtifacts, includeLayer3History));
        return Results.Content(service.RenderPage(page), "text/html");
    });

app.Run();

public partial class Program;
