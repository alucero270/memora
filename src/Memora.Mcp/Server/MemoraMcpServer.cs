using Memora.Core.AgentInteraction;

namespace Memora.Mcp.Server;

public sealed record McpToolDefinition(string Name, string Description);

public sealed record McpResourceDefinition(string UriTemplate, string Name, string Description);

public sealed class MemoraMcpServer
{
    private readonly IAgentInteractionService _service;

    public MemoraMcpServer(IAgentInteractionService service)
    {
        _service = service ?? throw new ArgumentNullException(nameof(service));
    }

    public IReadOnlyList<McpToolDefinition> Tools { get; } =
    [
        new("get_context", "Retrieve a deterministic context bundle for a project task."),
        new("propose_artifact", "Submit a new artifact proposal in non-canonical state."),
        new("propose_update", "Submit a proposed update for an existing artifact."),
        new("record_outcome", "Record an execution outcome as a reviewable non-canonical artifact.")
    ];

    public IReadOnlyList<McpResourceDefinition> Resources { get; } =
    [
        new("memora://projects/{projectId}", "project", "Read project metadata for a Memora workspace.")
    ];

    public ProjectLookupResponse ReadProject(string projectId) => _service.GetProject(projectId);

    public GetContextResponse GetContext(GetContextRequest request) => _service.GetContext(request);

    public ProposalResponse ProposeArtifact(ProposeArtifactRequest request) => _service.ProposeArtifact(request);

    public ProposalResponse ProposeUpdate(ProposeUpdateRequest request) => _service.ProposeUpdate(request);

    public OutcomeResponse RecordOutcome(RecordOutcomeRequest request) => _service.RecordOutcome(request);
}
