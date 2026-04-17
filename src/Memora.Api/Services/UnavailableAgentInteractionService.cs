using Memora.Core.AgentInteraction;
using Memora.Core.Artifacts;

namespace Memora.Api.Services;

internal sealed class UnavailableAgentInteractionService : IAgentInteractionService
{
    public ProjectLookupResponse GetProject(string projectId) =>
        new(
            projectId,
            null,
            null,
            [new AgentInteractionError("project.not_found", $"Project '{projectId}' was not found.", "project_id")]);

    public GetContextResponse GetContext(GetContextRequest request) =>
        new(
            null,
            [new AgentInteractionError("context.not_configured", "Context assembly service is not configured.", "service")]);

    public ProposalResponse ProposeArtifact(ProposeArtifactRequest request) =>
        new(
            request.ProjectId,
            request.ArtifactId,
            request.ArtifactType,
            ArtifactStatus.Proposed,
            0,
            [new AgentInteractionError("proposal.not_configured", "Proposal submission service is not configured.", "service")]);

    public ProposalResponse ProposeUpdate(ProposeUpdateRequest request) =>
        new(
            request.ProjectId,
            request.ArtifactId,
            ArtifactType.Plan,
            ArtifactStatus.Proposed,
            0,
            [new AgentInteractionError("proposal.not_configured", "Proposal submission service is not configured.", "service")]);

    public OutcomeResponse RecordOutcome(RecordOutcomeRequest request) =>
        new(
            request.ProjectId,
            request.ArtifactId,
            ArtifactStatus.Proposed,
            0,
            OutcomeKind.Mixed,
            [new AgentInteractionError("outcome.not_configured", "Outcome recording service is not configured.", "service")]);
}
