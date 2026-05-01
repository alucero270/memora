using System.Text.Json;
using Memora.Api;
using Memora.Api.Services;
using Memora.Core.AgentInteraction;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddOpenApi();

// Pin web JSON defaults so the OpenAPI body and `ProjectStateViewSerializer.Serialize`
// (used on the MCP path) stay byte-identical. Drift here silently desyncs runtime surfaces.
builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
    options.SerializerOptions.PropertyNameCaseInsensitive = true;
    options.SerializerOptions.NumberHandling = System.Text.Json.Serialization.JsonNumberHandling.AllowReadingFromString;
    options.SerializerOptions.WriteIndented = false;
});

var workspacesRootPath = builder.Configuration["Memora:WorkspacesRootPath"] ??
                         Environment.GetEnvironmentVariable("MEMORA_WORKSPACES_ROOT");

if (string.IsNullOrWhiteSpace(workspacesRootPath))
{
    builder.Services.AddSingleton<IAgentInteractionService, UnavailableAgentInteractionService>();
}
else
{
    builder.Services.AddSingleton<IAgentInteractionService>(_ =>
        new FileSystemAgentInteractionService(workspacesRootPath));
}

var app = builder.Build();

app.MapOpenApi("/openapi.json");

app.MapGet(
    "/api/projects/{projectId}",
    (string projectId, IAgentInteractionService service) =>
        AgentInteractionHttpResults.FromProjectResponse(service.GetProject(projectId)));

app.MapPost(
    "/api/context",
    (GetContextRequest request, IAgentInteractionService service) =>
        AgentInteractionHttpResults.FromContextResponse(service.GetContext(request)));

app.MapPost(
    "/api/artifacts/proposals",
    (ProposeArtifactRequest request, IAgentInteractionService service) =>
        AgentInteractionHttpResults.FromProposalResponse(service.ProposeArtifact(request)));

app.MapPost(
    "/api/artifacts/updates",
    (ProposeUpdateRequest request, IAgentInteractionService service) =>
        AgentInteractionHttpResults.FromProposalResponse(service.ProposeUpdate(request)));

app.MapPost(
    "/api/outcomes",
    (RecordOutcomeRequest request, IAgentInteractionService service) =>
        AgentInteractionHttpResults.FromOutcomeResponse(service.RecordOutcome(request)));

app.Run();

public partial class Program;
