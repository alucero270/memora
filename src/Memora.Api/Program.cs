using Memora.Api;
using Memora.Api.Services;
using Memora.Core.AgentInteraction;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSingleton<IAgentInteractionService, UnavailableAgentInteractionService>();

var app = builder.Build();

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
