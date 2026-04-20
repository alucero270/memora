using Microsoft.Data.Sqlite;
using Memora.Core.Artifacts;
using Memora.Core.Projects;
using Memora.Index.Relationships;
using Memora.Index.Rebuild;
using Memora.Storage.Persistence;
using Memora.Storage.Workspaces;

namespace Memora.Index.Tests.Relationships;

public sealed class ArtifactRelationshipIndexTests : IDisposable
{
    private readonly string _workspacesRootPath = Path.Combine(
        Path.GetTempPath(),
        "memora-relationship-index-tests",
        Guid.NewGuid().ToString("N"));

    private readonly ArtifactFileStore _fileStore = new();
    private readonly SqliteIndexRebuilder _rebuilder = new();
    private readonly ArtifactRelationshipIndex _index = new();

    [Fact]
    public void GetApprovedRelationships_UsesLatestApprovedRevisionOnly()
    {
        var workspace = CreateWorkspace("alpha-workspace", "alpha", "Alpha");
        _fileStore.Save(workspace, CreateCharterArtifact("alpha", "CHR-001"));
        _fileStore.Save(workspace, CreateDecisionArtifact("alpha", "ADR-001", revision: 1, new ArtifactLinks(["CHR-001"], [], [], [])));
        _fileStore.Save(workspace, CreateDecisionArtifact("alpha", "ADR-001", revision: 2, new ArtifactLinks([], ["CHR-001"], [], [])));

        using var connection = CreateConnection();
        var rebuildResult = _rebuilder.Rebuild(connection, _workspacesRootPath);

        Assert.True(rebuildResult.Success);

        var relationships = _index.GetApprovedRelationships(connection, "alpha");

        Assert.Equal(
            [new PersistedArtifactRelationship("alpha", "ADR-001", 2, ArtifactRelationshipKind.Affects, "CHR-001")],
            relationships);
    }

    [Fact]
    public void GetIncomingRelationships_IgnoresDraftRelationshipRows()
    {
        var workspace = CreateWorkspace("alpha-workspace", "alpha", "Alpha");
        _fileStore.Save(workspace, CreateCharterArtifact("alpha", "CHR-001"));
        _fileStore.Save(workspace, CreateDecisionArtifact("alpha", "ADR-001", revision: 1, ArtifactLinks.Empty));
        _fileStore.Save(workspace, CreatePlanArtifact("alpha", ArtifactStatus.Draft, revision: 1) with
        {
            Links = new ArtifactLinks([], ["CHR-001"], [], [])
        });

        using var connection = CreateConnection();
        var rebuildResult = _rebuilder.Rebuild(connection, _workspacesRootPath);

        Assert.True(rebuildResult.Success);
        Assert.Empty(_index.GetIncomingRelationships(connection, "alpha", "CHR-001"));
    }

    public void Dispose()
    {
        if (Directory.Exists(_workspacesRootPath))
        {
            Directory.Delete(_workspacesRootPath, recursive: true);
        }
    }

    private ProjectWorkspace CreateWorkspace(string directoryName, string projectId, string name)
    {
        var workspaceRootPath = Path.Combine(_workspacesRootPath, directoryName);
        Directory.CreateDirectory(workspaceRootPath);
        File.WriteAllText(
            Path.Combine(workspaceRootPath, "project.json"),
            $$"""
              {
                "projectId": "{{projectId}}",
                "name": "{{name}}",
                "status": "active"
              }
              """);
        return new ProjectWorkspace(new ProjectMetadata(projectId, name, "active"), workspaceRootPath);
    }

    private static SqliteConnection CreateConnection()
    {
        var connection = new SqliteConnection("Data Source=:memory:");
        connection.Open();
        return connection;
    }

    private static ProjectCharterArtifact CreateCharterArtifact(string projectId, string id) =>
        new(
            id,
            projectId,
            ArtifactStatus.Approved,
            $"Charter {id}",
            new DateTimeOffset(2026, 4, 20, 9, 0, 0, TimeSpan.Zero),
            new DateTimeOffset(2026, 4, 20, 9, 5, 0, TimeSpan.Zero),
            1,
            ["charter"],
            "user",
            "fixture",
            ArtifactLinks.Empty,
            """
            ## Problem Statement
            Keep relationships deterministic.

            ## Primary Users / Stakeholders
            Engineers.

            ## Current Pain
            Relationship truth drifts.

            ## Desired Outcome
            Relationships rebuild cleanly.

            ## Definition of Success
            Approved links are persisted and queryable.
            """,
            new Dictionary<string, string>(StringComparer.Ordinal)
            {
                ["Problem Statement"] = "Keep relationships deterministic.",
                ["Primary Users / Stakeholders"] = "Engineers.",
                ["Current Pain"] = "Relationship truth drifts.",
                ["Desired Outcome"] = "Relationships rebuild cleanly.",
                ["Definition of Success"] = "Approved links are persisted and queryable."
            });

    private static ArchitectureDecisionArtifact CreateDecisionArtifact(
        string projectId,
        string id,
        int revision,
        ArtifactLinks links) =>
        new(
            id,
            projectId,
            ArtifactStatus.Approved,
            $"Decision {id}",
            new DateTimeOffset(2026, 4, 20, 10, 0, 0, TimeSpan.Zero),
            new DateTimeOffset(2026, 4, 20, 10, revision, 0, TimeSpan.Zero),
            revision,
            ["decision"],
            "user",
            "fixture",
            links,
            """
            ## Context
            Relationships matter.

            ## Decision
            Persist approved relationships.

            ## Alternatives Considered
            Ignore them.

            ## Consequences
            Traceability stays explainable.
            """,
            new Dictionary<string, string>(StringComparer.Ordinal)
            {
                ["Context"] = "Relationships matter.",
                ["Decision"] = "Persist approved relationships.",
                ["Alternatives Considered"] = "Ignore them.",
                ["Consequences"] = "Traceability stays explainable."
            },
            "2026-04-20");

    private static PlanArtifact CreatePlanArtifact(string projectId, ArtifactStatus status, int revision) =>
        new(
            "PLN-001",
            projectId,
            status,
            "Plan",
            new DateTimeOffset(2026, 4, 20, 11, 0, 0, TimeSpan.Zero),
            new DateTimeOffset(2026, 4, 20, 11, revision, 0, TimeSpan.Zero),
            revision,
            ["plan"],
            "user",
            "fixture",
            ArtifactLinks.Empty,
            """
            ## Goal
            Query relationships.

            ## Scope
            Approved graph only.

            ## Acceptance Criteria
            - persistence is deterministic

            ## Notes
            Keep it small.
            """,
            new Dictionary<string, string>(StringComparer.Ordinal)
            {
                ["Goal"] = "Query relationships.",
                ["Scope"] = "Approved graph only.",
                ["Acceptance Criteria"] = "- persistence is deterministic",
                ["Notes"] = "Keep it small."
            },
            ArtifactPriority.Normal,
            true);
}
