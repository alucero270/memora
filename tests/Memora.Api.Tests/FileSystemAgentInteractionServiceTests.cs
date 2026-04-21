using Memora.Api.Services;
using Memora.Core.AgentInteraction;
using Memora.Core.Artifacts;
using Memora.Core.Projects;
using Memora.Storage.Persistence;
using Memora.Storage.Workspaces;

namespace Memora.Api.Tests;

public sealed class FileSystemAgentInteractionServiceTests : IDisposable
{
    private readonly string _workspacesRootPath = Path.Combine(
        Path.GetTempPath(),
        "memora-agent-service-tests",
        Guid.NewGuid().ToString("N"));

    private readonly ArtifactFileStore _fileStore = new();

    [Fact]
    public void ProposeArtifact_PersistsProposalInDraftStorage()
    {
        var workspace = CreateWorkspace("memora");
        var service = new FileSystemAgentInteractionService(_workspacesRootPath);

        var response = service.ProposeArtifact(
            new ProposeArtifactRequest(
                "memora",
                "ADR-101",
                ArtifactType.Decision,
                CreateDecisionContent()));

        Assert.True(response.IsSuccess);
        Assert.Equal(ArtifactStatus.Proposed, response.ResultingStatus);
        Assert.True(File.Exists(Path.Combine(workspace.DraftsRootPath, "decision", "ADR-101.r0001.md")));
        Assert.False(File.Exists(Path.Combine(workspace.CanonicalDecisionsPath, "ADR-101.r0001.md")));
    }

    [Fact]
    public void ProposeUpdate_CreatesNewProposedRevisionWithoutChangingApprovedFile()
    {
        var workspace = CreateWorkspace("memora");
        _fileStore.Save(workspace, CreateApprovedDecisionArtifact());
        var service = new FileSystemAgentInteractionService(_workspacesRootPath);

        var response = service.ProposeUpdate(
            new ProposeUpdateRequest(
                "memora",
                "ADR-001",
                1,
                CreateDecisionContent("Updated context decision")));

        Assert.True(response.IsSuccess);
        Assert.Equal(2, response.Revision);
        Assert.True(File.Exists(Path.Combine(workspace.CanonicalDecisionsPath, "ADR-001.r0001.md")));
        Assert.True(File.Exists(Path.Combine(workspace.DraftsRootPath, "decision", "ADR-001.r0002.md")));
    }

    [Fact]
    public void ProposeArtifact_InvalidProposal_ReturnsValidationErrorsAndDoesNotWriteFile()
    {
        var workspace = CreateWorkspace("memora");
        var service = new FileSystemAgentInteractionService(_workspacesRootPath);

        var response = service.ProposeArtifact(
            new ProposeArtifactRequest(
                "memora",
                "ADR-102",
                ArtifactType.Decision,
                new ArtifactProposalContent(
                    "Invalid decision",
                    "agent",
                    "Missing decision date.",
                    ["context"],
                    new Dictionary<string, string>(StringComparer.Ordinal)
                    {
                        ["Context"] = "Need deterministic context.",
                        ["Decision"] = "Still missing required type-specific values.",
                        ["Alternatives Considered"] = "Implicit behavior.",
                        ["Consequences"] = "Validation should reject this."
                    })));

        Assert.False(response.IsSuccess);
        var error = Assert.Single(response.Errors, error => error.Code == "artifact.frontmatter.missing");
        Assert.Contains("code: artifact.frontmatter.missing", error.Message, StringComparison.Ordinal);
        Assert.Contains("path: decision_date", error.Message, StringComparison.Ordinal);
        Assert.False(File.Exists(Path.Combine(workspace.DraftsRootPath, "decision", "ADR-102.r0001.md")));
    }

    [Fact]
    public void RecordOutcome_PersistsOutcomeArtifactInDraftStorage()
    {
        var workspace = CreateWorkspace("memora");
        var service = new FileSystemAgentInteractionService(_workspacesRootPath);

        var response = service.RecordOutcome(
            new RecordOutcomeRequest(
                "memora",
                "OUT-001",
                CreateOutcomeContent()));

        Assert.True(response.IsSuccess);
        Assert.Equal(ArtifactStatus.Proposed, response.ResultingStatus);
        Assert.Equal(OutcomeKind.Success, response.OutcomeKind);
        Assert.True(File.Exists(Path.Combine(workspace.DraftsRootPath, "outcome", "OUT-001.r0001.md")));
    }

    [Fact]
    public void RecordOutcome_InvalidOutcome_ReturnsValidationErrorsAndDoesNotWriteFile()
    {
        var workspace = CreateWorkspace("memora");
        var service = new FileSystemAgentInteractionService(_workspacesRootPath);

        var response = service.RecordOutcome(
            new RecordOutcomeRequest(
                "memora",
                "OUT-002",
                new ArtifactProposalContent(
                    "Incomplete outcome",
                    "agent",
                    "Missing outcome kind.",
                    ["outcome"],
                    new Dictionary<string, string>(StringComparer.Ordinal)
                    {
                        ["What Happened"] = "Outcome data exists.",
                        ["Why"] = "Need to validate outcome submissions.",
                        ["Impact"] = "Still missing type-specific outcome data.",
                        ["Follow-up"] = "Add the missing outcome kind."
                    })));

        Assert.False(response.IsSuccess);
        var error = Assert.Single(response.Errors, error => error.Code == "artifact.frontmatter.missing");
        Assert.Contains("code: artifact.frontmatter.missing", error.Message, StringComparison.Ordinal);
        Assert.Contains("path: outcome", error.Message, StringComparison.Ordinal);
        Assert.False(File.Exists(Path.Combine(workspace.DraftsRootPath, "outcome", "OUT-002.r0001.md")));
    }

    public void Dispose()
    {
        if (Directory.Exists(_workspacesRootPath))
        {
            Directory.Delete(_workspacesRootPath, recursive: true);
        }
    }

    private ProjectWorkspace CreateWorkspace(string projectId)
    {
        Directory.CreateDirectory(_workspacesRootPath);
        var workspaceRootPath = Path.Combine(_workspacesRootPath, projectId);
        Directory.CreateDirectory(workspaceRootPath);
        File.WriteAllText(
            Path.Combine(workspaceRootPath, "project.json"),
            $$"""
              {
                "projectId": "{{projectId}}",
                "name": "Memora",
                "status": "active"
              }
              """);

        return new ProjectWorkspace(new ProjectMetadata(projectId, "Memora", "active"), workspaceRootPath);
    }

    private static ArtifactProposalContent CreateDecisionContent(string title = "Context decision") =>
        new(
            title,
            "agent",
            "Need a reviewable proposal.",
            ["context"],
            new Dictionary<string, string>(StringComparer.Ordinal)
            {
                ["Context"] = "Need deterministic context.",
                ["Decision"] = "Keep the contract explicit.",
                ["Alternatives Considered"] = "Duplicated endpoint logic.",
                ["Consequences"] = "Shared services stay reusable."
            },
            AgentArtifactLinks.Empty,
            new Dictionary<string, object?>(StringComparer.Ordinal)
            {
                ["decision_date"] = "2026-04-17"
            });

    private static ArtifactProposalContent CreateOutcomeContent() =>
        new(
            "Execution outcome",
            "agent",
            "Need a reviewable outcome record.",
            ["outcome"],
            new Dictionary<string, string>(StringComparer.Ordinal)
            {
                ["What Happened"] = "Execution completed successfully.",
                ["Why"] = "Outcome recording should stay proposal-only.",
                ["Impact"] = "The proposal path can now record structured outcomes.",
                ["Follow-up"] = "Review and approve the recorded outcome."
            },
            AgentArtifactLinks.Empty,
            new Dictionary<string, object?>(StringComparer.Ordinal)
            {
                ["outcome"] = "success"
            });

    private static ArchitectureDecisionArtifact CreateApprovedDecisionArtifact() =>
        new(
            "ADR-001",
            "memora",
            ArtifactStatus.Approved,
            "Current context decision",
            new DateTimeOffset(2026, 4, 17, 9, 0, 0, TimeSpan.Zero),
            new DateTimeOffset(2026, 4, 17, 9, 30, 0, TimeSpan.Zero),
            1,
            ["context"],
            "user",
            "seed approved decision",
            ArtifactLinks.Empty,
            """
            ## Context
            Deterministic context is required.

            ## Decision
            Keep the current approved decision.

            ## Alternatives Considered
            Replacing approved truth directly.

            ## Consequences
            Updates must stay proposal-only.
            """,
            new Dictionary<string, string>(StringComparer.Ordinal)
            {
                ["Context"] = "Deterministic context is required.",
                ["Decision"] = "Keep the current approved decision.",
                ["Alternatives Considered"] = "Replacing approved truth directly.",
                ["Consequences"] = "Updates must stay proposal-only."
            },
            "2026-04-17");
}
