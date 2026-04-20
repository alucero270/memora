using Memora.Core.AgentInteraction;

namespace Memora.Mcp.Server;

internal static class McpContractHelpers
{
    internal static string RequireValue(string value, string parameterName) =>
        string.IsNullOrWhiteSpace(value)
            ? throw new ArgumentException("Value is required.", parameterName)
            : value.Trim();
}

public sealed record McpErrorContract(string Code, string Description)
{
    public string Code { get; } = McpContractHelpers.RequireValue(Code, nameof(Code));
    public string Description { get; } = McpContractHelpers.RequireValue(Description, nameof(Description));
}

public sealed record McpToolDefinition(
    string Name,
    string Description,
    string RequestContract,
    string ResponseContract,
    IReadOnlyList<McpErrorContract> Errors)
{
    public string Name { get; } = McpContractHelpers.RequireValue(Name, nameof(Name));
    public string Description { get; } = McpContractHelpers.RequireValue(Description, nameof(Description));
    public string RequestContract { get; } = McpContractHelpers.RequireValue(RequestContract, nameof(RequestContract));
    public string ResponseContract { get; } = McpContractHelpers.RequireValue(ResponseContract, nameof(ResponseContract));
    public IReadOnlyList<McpErrorContract> Errors { get; } = Errors?.ToArray() ?? throw new ArgumentNullException(nameof(Errors));
}

public sealed record McpResourceDefinition(
    string UriTemplate,
    string Name,
    string Description,
    string ResponseContract,
    IReadOnlyList<McpErrorContract> Errors)
{
    public string UriTemplate { get; } = McpContractHelpers.RequireValue(UriTemplate, nameof(UriTemplate));
    public string Name { get; } = McpContractHelpers.RequireValue(Name, nameof(Name));
    public string Description { get; } = McpContractHelpers.RequireValue(Description, nameof(Description));
    public string ResponseContract { get; } = McpContractHelpers.RequireValue(ResponseContract, nameof(ResponseContract));
    public IReadOnlyList<McpErrorContract> Errors { get; } = Errors?.ToArray() ?? throw new ArgumentNullException(nameof(Errors));
}

public sealed record McpResourceReadResult<TPayload>(string Uri, TPayload? Payload, IReadOnlyList<AgentInteractionError> Errors)
{
    public string Uri { get; } = McpContractHelpers.RequireValue(Uri, nameof(Uri));
    public TPayload? Payload { get; } = Payload;
    public IReadOnlyList<AgentInteractionError> Errors { get; } = Errors?.ToArray() ?? throw new ArgumentNullException(nameof(Errors));
    public bool IsSuccess => Errors.Count == 0;
}

public sealed class MemoraMcpServer
{
    private const string ProjectResourcePrefix = "memora://projects/";

    private readonly IAgentInteractionService _service;

    public MemoraMcpServer(IAgentInteractionService service)
    {
        _service = service ?? throw new ArgumentNullException(nameof(service));
    }

    public IReadOnlyList<McpToolDefinition> Tools { get; } =
    [
        new(
            "get_context",
            "Retrieve a deterministic context bundle for a project task.",
            nameof(GetContextRequest),
            nameof(GetContextResponse),
            [new McpErrorContract("context.validation", "The context request did not satisfy shared validation rules.")]),
        new(
            "propose_artifact",
            "Submit a new artifact proposal in non-canonical state.",
            nameof(ProposeArtifactRequest),
            nameof(ProposalResponse),
            [new McpErrorContract("proposal.validation", "The proposal did not satisfy shared validation rules.")]),
        new(
            "propose_update",
            "Submit a proposed update for an existing artifact.",
            nameof(ProposeUpdateRequest),
            nameof(ProposalResponse),
            [new McpErrorContract("proposal.validation", "The update proposal did not satisfy shared validation rules.")]),
        new(
            "record_outcome",
            "Record an execution outcome as a reviewable non-canonical artifact.",
            nameof(RecordOutcomeRequest),
            nameof(OutcomeResponse),
            [new McpErrorContract("outcome.validation", "The outcome request did not satisfy shared validation rules.")])
    ];

    public IReadOnlyList<McpResourceDefinition> Resources { get; } =
    [
        new(
            "memora://projects/{projectId}",
            "project",
            "Read project metadata for a Memora workspace.",
            nameof(ProjectLookupResponse),
            [
                new McpErrorContract("mcp.resource.uri.invalid", "The resource URI does not match the published project resource template."),
                new McpErrorContract("project.not_found", "The requested project could not be found in the configured workspace root.")
            ])
    ];

    public ProjectLookupResponse ReadProject(string projectId) => _service.GetProject(projectId);

    public McpResourceReadResult<ProjectLookupResponse> ReadResource(string uri)
    {
        if (!TryParseProjectResourceUri(uri, out var projectId))
        {
            return new McpResourceReadResult<ProjectLookupResponse>(
                uri,
                null,
                [new AgentInteractionError("mcp.resource.uri.invalid", "Resource URI must match memora://projects/{projectId}.", "uri")]);
        }

        var response = _service.GetProject(projectId);
        return new McpResourceReadResult<ProjectLookupResponse>(uri, response, response.Errors);
    }

    public GetContextResponse GetContext(GetContextRequest request) => _service.GetContext(request);

    public ProposalResponse ProposeArtifact(ProposeArtifactRequest request) => _service.ProposeArtifact(request);

    public ProposalResponse ProposeUpdate(ProposeUpdateRequest request) => _service.ProposeUpdate(request);

    public OutcomeResponse RecordOutcome(RecordOutcomeRequest request) => _service.RecordOutcome(request);

    private static bool TryParseProjectResourceUri(string uri, out string projectId)
    {
        projectId = string.Empty;

        if (string.IsNullOrWhiteSpace(uri) || !uri.StartsWith(ProjectResourcePrefix, StringComparison.Ordinal))
        {
            return false;
        }

        var candidate = uri[ProjectResourcePrefix.Length..].Trim();
        if (string.IsNullOrWhiteSpace(candidate) || candidate.Contains('/'))
        {
            return false;
        }

        projectId = candidate;
        return true;
    }
}
