using Microsoft.Data.Sqlite;
using Memora.Core.Artifacts;

namespace Memora.Index.Relationships;

public sealed class ArtifactRelationshipIndex
{
    public IReadOnlyList<PersistedArtifactRelationship> GetOutgoingRelationships(
        SqliteConnection connection,
        string projectId,
        string artifactId)
    {
        ArgumentNullException.ThrowIfNull(connection);
        ArgumentException.ThrowIfNullOrWhiteSpace(projectId);
        ArgumentException.ThrowIfNullOrWhiteSpace(artifactId);

        return QueryRelationships(
            connection,
            projectId,
            artifactId,
            outgoing: true);
    }

    public IReadOnlyList<PersistedArtifactRelationship> GetIncomingRelationships(
        SqliteConnection connection,
        string projectId,
        string artifactId)
    {
        ArgumentNullException.ThrowIfNull(connection);
        ArgumentException.ThrowIfNullOrWhiteSpace(projectId);
        ArgumentException.ThrowIfNullOrWhiteSpace(artifactId);

        return QueryRelationships(
            connection,
            projectId,
            artifactId,
            outgoing: false);
    }

    public IReadOnlyList<PersistedArtifactRelationship> GetApprovedRelationships(
        SqliteConnection connection,
        string projectId)
    {
        ArgumentNullException.ThrowIfNull(connection);
        ArgumentException.ThrowIfNullOrWhiteSpace(projectId);

        const string commandText =
            """
            WITH latest_approved_revisions AS (
                SELECT project_id, artifact_id, MAX(revision) AS latest_revision
                FROM artifact_revisions
                WHERE artifact_status = 'approved' AND is_canonical = 1
                GROUP BY project_id, artifact_id
            )
            SELECT
                ar.project_id,
                ar.source_artifact_id,
                ar.source_revision,
                ar.relationship_kind,
                ar.target_artifact_id
            FROM artifact_relationships ar
            INNER JOIN latest_approved_revisions latest
                ON latest.project_id = ar.project_id
                AND latest.artifact_id = ar.source_artifact_id
                AND latest.latest_revision = ar.source_revision
            WHERE ar.project_id = $project_id
            ORDER BY
                ar.source_artifact_id,
                ar.relationship_kind,
                ar.target_artifact_id,
                ar.source_revision;
            """;

        return ExecuteQuery(connection, commandText, command =>
        {
            command.Parameters.AddWithValue("$project_id", projectId);
        });
    }

    private static IReadOnlyList<PersistedArtifactRelationship> QueryRelationships(
        SqliteConnection connection,
        string projectId,
        string artifactId,
        bool outgoing)
    {
        var filterColumn = outgoing ? "ar.source_artifact_id" : "ar.target_artifact_id";
        var orderBy = outgoing
            ? "ar.relationship_kind, ar.target_artifact_id, ar.source_revision"
            : "ar.source_artifact_id, ar.relationship_kind, ar.source_revision";

        var commandText =
            $"""
            WITH latest_approved_revisions AS (
                SELECT project_id, artifact_id, MAX(revision) AS latest_revision
                FROM artifact_revisions
                WHERE artifact_status = 'approved' AND is_canonical = 1
                GROUP BY project_id, artifact_id
            )
            SELECT
                ar.project_id,
                ar.source_artifact_id,
                ar.source_revision,
                ar.relationship_kind,
                ar.target_artifact_id
            FROM artifact_relationships ar
            INNER JOIN latest_approved_revisions latest
                ON latest.project_id = ar.project_id
                AND latest.artifact_id = ar.source_artifact_id
                AND latest.latest_revision = ar.source_revision
            WHERE ar.project_id = $project_id
                AND {filterColumn} = $artifact_id
            ORDER BY {orderBy};
            """;

        return ExecuteQuery(connection, commandText, command =>
        {
            command.Parameters.AddWithValue("$project_id", projectId);
            command.Parameters.AddWithValue("$artifact_id", artifactId);
        });
    }

    private static IReadOnlyList<PersistedArtifactRelationship> ExecuteQuery(
        SqliteConnection connection,
        string commandText,
        Action<SqliteCommand> configure)
    {
        var shouldClose = connection.State == System.Data.ConnectionState.Closed;
        if (shouldClose)
        {
            connection.Open();
        }

        try
        {
            using var command = connection.CreateCommand();
            command.CommandText = commandText;
            configure(command);

            using var reader = command.ExecuteReader();
            var relationships = new List<PersistedArtifactRelationship>();
            while (reader.Read())
            {
                relationships.Add(new PersistedArtifactRelationship(
                    reader.GetString(0),
                    reader.GetString(1),
                    reader.GetInt32(2),
                    ParseRelationshipKind(reader.GetString(3)),
                    reader.GetString(4)));
            }

            return relationships;
        }
        finally
        {
            if (shouldClose)
            {
                connection.Close();
            }
        }
    }

    private static ArtifactRelationshipKind ParseRelationshipKind(string relationshipKind)
    {
        if (ArtifactLinks.TryParseKind(relationshipKind, out var kind))
        {
            return kind;
        }

        throw new InvalidOperationException($"Unsupported relationship kind '{relationshipKind}' was loaded from the index.");
    }
}
