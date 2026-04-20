using Memora.Core.AgentInteraction;
using Memora.Core.Artifacts;
using Memora.Mcp.Server;

namespace Memora.Mcp.Tests;

public sealed class MemoraMcpServerTests
{
    [Fact]
    public void Server_ExposesExpectedCoreAgentLoopTools()
    {
        var server = new MemoraMcpServer(new TestAgentInteractionService());

        Assert.Equal(
            ["get_context", "propose_artifact", "propose_update", "record_outcome"],
            server.Tools.Select(tool => tool.Name));
    }

    [Fact]
    public void Server_ExposesProjectResourceTemplate()
    {
        var server = new MemoraMcpServer(new TestAgentInteractionService());

        var resource = Assert.Single(server.Resources);
        Assert.Equal("memora://projects/{projectId}", resource.UriTemplate);
    }

    [Fact]
    public void GetContext_ForwardsToSharedAgentInteractionService()
    {
        var server = new MemoraMcpServer(new TestAgentInteractionService());

        var response = server.GetContext(new GetContextRequest("memora", "Prepare milestone 3 context."));

        Assert.True(response.IsSuccess);
        Assert.NotNull(response.Bundle);
        Assert.Equal(3, response.Bundle.Layers.Count);
    }

    [Fact]
    public void ProposeArtifact_ForwardsProposalOnlyBehavior()
    {
        var server = new MemoraMcpServer(new TestAgentInteractionService());

        var response = server.ProposeArtifact(
            new ProposeArtifactRequest("memora", "ADR-001", ArtifactType.Decision, CreateContent()));

        Assert.True(response.IsSuccess);
        Assert.Equal(ArtifactStatus.Proposed, response.ResultingStatus);
    }

    [Fact]
    public void Errors_AreSurfacedWithoutProtocolLayerRewriting()
    {
        var server = new MemoraMcpServer(new FailingAgentInteractionService());

        var response = server.GetContext(new GetContextRequest("memora", "Prepare milestone 3 context."));

        Assert.False(response.IsSuccess);
        Assert.Equal("context.validation", response.Errors[0].Code);
    }

    private static ArtifactProposalContent CreateContent() =>
        new(
            "Context decision",
            "agent",
            "Need a reviewable proposal.",
            ["context"],
            new Dictionary<string, string>(StringComparer.Ordinal)
            {
                ["Context"] = "Need deterministic context.",
                ["Decision"] = "Keep the contract explicit."
            });

    private sealed class TestAgentInteractionService : IAgentInteractionService
    {
        public ProjectLookupResponse GetProject(string projectId) =>
            new(projectId, "Memora", "active", []);

        public GetContextResponse GetContext(GetContextRequest request) =>
            new(
                new AgentContextBundle(
                    request,
                    [
                        new AgentContextLayer(AgentContextLayerKind.Layer1, [CreateArtifact("CHR-001")]),
                        new AgentContextLayer(AgentContextLayerKind.Layer2, [CreateArtifact("ADR-001")]),
                        new AgentContextLayer(AgentContextLayerKind.Layer3, [])
                    ]),
                []);

        public ProposalResponse ProposeArtifact(ProposeArtifactRequest request) =>
            new(request.ProjectId, request.ArtifactId, request.ArtifactType, ArtifactStatus.Proposed, 1, []);

        public ProposalResponse ProposeUpdate(ProposeUpdateRequest request) =>
            new(request.ProjectId, request.ArtifactId, ArtifactType.Decision, ArtifactStatus.Proposed, request.ExpectedRevision + 1, []);

        public OutcomeResponse RecordOutcome(RecordOutcomeRequest request) =>
            new(request.ProjectId, request.ArtifactId, ArtifactStatus.Proposed, 1, OutcomeKind.Success, []);

        private static AgentContextArtifact CreateArtifact(string id) =>
            new(
                new ArchitectureDecisionArtifact(
                    id,
                    "memora",
                    ArtifactStatus.Approved,
                    "Context decision",
                    new DateTimeOffset(2026, 4, 17, 10, 0, 0, TimeSpan.Zero),
                    new DateTimeOffset(2026, 4, 17, 10, 15, 0, TimeSpan.Zero),
                    1,
                    ["context"],
                    "user",
                    "mcp tests",
                    ArtifactLinks.Empty,
                    """
                    ## Context
                    Deterministic context is required.

                    ## Decision
                    Keep MCP as a thin adapter.

                    ## Alternatives Considered
                    Protocol-specific business logic.

                    ## Consequences
                    Shared services remain reusable.
                    """,
                    new Dictionary<string, string>(StringComparer.Ordinal)
                    {
                        ["Context"] = "Deterministic context is required.",
                        ["Decision"] = "Keep MCP as a thin adapter.",
                        ["Alternatives Considered"] = "Protocol-specific business logic.",
                        ["Consequences"] = "Shared services remain reusable."
                    },
                    "2026-04-17"),
                [new AgentContextInclusionReason("approved-default", "Included from approved context grounding.", [])]);
    }

    private sealed class FailingAgentInteractionService : IAgentInteractionService
    {
        public ProjectLookupResponse GetProject(string projectId) =>
            new(projectId, null, null, [new AgentInteractionError("project.not_found", "Project not found.", "project_id")]);

        public GetContextResponse GetContext(GetContextRequest request) =>
            new(null, [new AgentInteractionError("context.validation", "Task description is invalid.", "task_description")]);

        public ProposalResponse ProposeArtifact(ProposeArtifactRequest request) =>
            new(request.ProjectId, request.ArtifactId, request.ArtifactType, ArtifactStatus.Proposed, 0, [new AgentInteractionError("proposal.validation", "Invalid proposal.", "body")]);

        public ProposalResponse ProposeUpdate(ProposeUpdateRequest request) =>
            new(request.ProjectId, request.ArtifactId, ArtifactType.Decision, ArtifactStatus.Proposed, 0, [new AgentInteractionError("proposal.validation", "Invalid update.", "body")]);

        public OutcomeResponse RecordOutcome(RecordOutcomeRequest request) =>
            new(request.ProjectId, request.ArtifactId, ArtifactStatus.Proposed, 0, OutcomeKind.Mixed, [new AgentInteractionError("outcome.validation", "Invalid outcome.", "body")]);
    }
}
