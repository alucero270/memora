using Microsoft.Data.Sqlite;

namespace Memora.Index.Schema;

public sealed class SqliteIndexSchema
{
    public const int CurrentVersion = 1;

    public void EnsureCreated(SqliteConnection connection)
    {
        ArgumentNullException.ThrowIfNull(connection);

        var shouldClose = connection.State == System.Data.ConnectionState.Closed;
        if (shouldClose)
        {
            connection.Open();
        }

        try
        {
            using var transaction = connection.BeginTransaction();

            ExecuteNonQuery(
                connection,
                transaction,
                """
                PRAGMA foreign_keys = ON;

                CREATE TABLE IF NOT EXISTS schema_info (
                    singleton_id INTEGER NOT NULL PRIMARY KEY CHECK (singleton_id = 1),
                    schema_version INTEGER NOT NULL
                );

                INSERT INTO schema_info (singleton_id, schema_version)
                VALUES (1, 1)
                ON CONFLICT(singleton_id) DO NOTHING;

                CREATE TABLE IF NOT EXISTS projects (
                    project_id TEXT NOT NULL PRIMARY KEY,
                    name TEXT NOT NULL,
                    status TEXT NULL,
                    workspace_root_path TEXT NOT NULL,
                    project_metadata_path TEXT NOT NULL
                );

                CREATE TABLE IF NOT EXISTS artifacts (
                    project_id TEXT NOT NULL,
                    artifact_id TEXT NOT NULL,
                    artifact_type TEXT NOT NULL,
                    latest_status TEXT NOT NULL,
                    latest_revision INTEGER NOT NULL,
                    title TEXT NOT NULL,
                    latest_updated_at_utc TEXT NOT NULL,
                    PRIMARY KEY (project_id, artifact_id),
                    FOREIGN KEY (project_id) REFERENCES projects(project_id) ON DELETE CASCADE
                );

                CREATE TABLE IF NOT EXISTS artifact_revisions (
                    project_id TEXT NOT NULL,
                    artifact_id TEXT NOT NULL,
                    revision INTEGER NOT NULL,
                    artifact_status TEXT NOT NULL,
                    title TEXT NOT NULL,
                    file_path TEXT NOT NULL,
                    created_at_utc TEXT NOT NULL,
                    updated_at_utc TEXT NOT NULL,
                    is_canonical INTEGER NOT NULL,
                    PRIMARY KEY (project_id, artifact_id, revision),
                    FOREIGN KEY (project_id, artifact_id) REFERENCES artifacts(project_id, artifact_id) ON DELETE CASCADE
                );

                CREATE TABLE IF NOT EXISTS artifact_relationships (
                    project_id TEXT NOT NULL,
                    source_artifact_id TEXT NOT NULL,
                    source_revision INTEGER NOT NULL,
                    relationship_kind TEXT NOT NULL,
                    target_artifact_id TEXT NOT NULL,
                    PRIMARY KEY (project_id, source_artifact_id, source_revision, relationship_kind, target_artifact_id),
                    FOREIGN KEY (project_id, source_artifact_id, source_revision)
                        REFERENCES artifact_revisions(project_id, artifact_id, revision)
                        ON DELETE CASCADE
                );

                CREATE INDEX IF NOT EXISTS idx_artifacts_project_type_status
                    ON artifacts (project_id, artifact_type, latest_status);

                CREATE INDEX IF NOT EXISTS idx_artifact_revisions_project_status
                    ON artifact_revisions (project_id, artifact_status, revision);

                CREATE INDEX IF NOT EXISTS idx_artifact_relationships_target
                    ON artifact_relationships (project_id, target_artifact_id, relationship_kind);
                """);

            transaction.Commit();
        }
        finally
        {
            if (shouldClose)
            {
                connection.Close();
            }
        }
    }

    private static void ExecuteNonQuery(SqliteConnection connection, SqliteTransaction transaction, string commandText)
    {
        using var command = connection.CreateCommand();
        command.Transaction = transaction;
        command.CommandText = commandText;
        command.ExecuteNonQuery();
    }
}
