using Microsoft.Data.Sqlite;
using Memora.Core.Artifacts;
using Memora.Index.Relationships;

namespace Memora.Index.Traceability;

public sealed class TraceabilityQueryService
{
    private readonly ArtifactRelationshipIndex _relationshipIndex = new();

    public TraceabilityQueryResult Query(SqliteConnection connection, TraceabilityQuery query)
    {
        ArgumentNullException.ThrowIfNull(connection);
        ArgumentNullException.ThrowIfNull(query);

        var errors = Validate(connection, query);
        if (errors.Count > 0)
        {
            return new TraceabilityQueryResult(query, [], errors);
        }

        var relationships = _relationshipIndex.GetApprovedRelationships(connection, query.ProjectId);
        var paths = query.Kind switch
        {
            TraceabilityQueryKind.Direct => BuildDirectPaths(query.ArtifactId, relationships),
            TraceabilityQueryKind.Dependency => BuildDependencyPaths(query.ArtifactId, relationships),
            TraceabilityQueryKind.Impact => BuildImpactPaths(query.ArtifactId, relationships),
            _ => throw new ArgumentOutOfRangeException(nameof(query.Kind), query.Kind, "Unsupported traceability query kind.")
        };

        return new TraceabilityQueryResult(query, paths, []);
    }

    private static IReadOnlyList<TraceabilityQueryError> Validate(SqliteConnection connection, TraceabilityQuery query)
    {
        var errors = new List<TraceabilityQueryError>();

        if (string.IsNullOrWhiteSpace(query.ProjectId))
        {
            errors.Add(new TraceabilityQueryError(
                "traceability.query.project_id.required",
                "Project id is required.",
                "projectId"));
        }

        if (string.IsNullOrWhiteSpace(query.ArtifactId))
        {
            errors.Add(new TraceabilityQueryError(
                "traceability.query.artifact_id.required",
                "Artifact id is required.",
                "artifactId"));
        }

        if (errors.Count > 0)
        {
            return errors;
        }

        if (!ApprovedArtifactExists(connection, query.ProjectId, query.ArtifactId))
        {
            errors.Add(new TraceabilityQueryError(
                "traceability.query.artifact.not_found",
                $"Approved artifact '{query.ArtifactId}' was not found in project '{query.ProjectId}'.",
                "artifactId"));
        }

        return errors;
    }

    private static bool ApprovedArtifactExists(SqliteConnection connection, string projectId, string artifactId)
    {
        var shouldClose = connection.State == System.Data.ConnectionState.Closed;
        if (shouldClose)
        {
            connection.Open();
        }

        try
        {
            using var command = connection.CreateCommand();
            command.CommandText =
                """
                SELECT COUNT(*)
                FROM artifact_revisions
                WHERE project_id = $project_id
                    AND artifact_id = $artifact_id
                    AND artifact_status = 'approved'
                    AND is_canonical = 1;
                """;
            command.Parameters.AddWithValue("$project_id", projectId);
            command.Parameters.AddWithValue("$artifact_id", artifactId);
            return Convert.ToInt64(command.ExecuteScalar()) > 0;
        }
        finally
        {
            if (shouldClose)
            {
                connection.Close();
            }
        }
    }

    private static IReadOnlyList<TraceabilityPath> BuildDirectPaths(
        string anchorArtifactId,
        IReadOnlyList<PersistedArtifactRelationship> relationships)
    {
        var outgoing = relationships
            .Where(relationship => relationship.SourceArtifactId == anchorArtifactId)
            .OrderBy(relationship => relationship.Kind)
            .ThenBy(relationship => relationship.TargetArtifactId, StringComparer.Ordinal)
            .Select(relationship => CreatePath(
                anchorArtifactId,
                relationship.TargetArtifactId,
                new TraceabilityPathSegment(
                    relationship.SourceArtifactId,
                    relationship.Kind,
                    relationship.TargetArtifactId,
                    TraceabilityTraversalDirection.Outgoing)));

        var incoming = relationships
            .Where(relationship => relationship.TargetArtifactId == anchorArtifactId)
            .OrderBy(relationship => relationship.Kind)
            .ThenBy(relationship => relationship.SourceArtifactId, StringComparer.Ordinal)
            .Select(relationship => CreatePath(
                anchorArtifactId,
                relationship.SourceArtifactId,
                new TraceabilityPathSegment(
                    relationship.SourceArtifactId,
                    relationship.Kind,
                    relationship.TargetArtifactId,
                    TraceabilityTraversalDirection.Incoming)));

        return outgoing.Concat(incoming).ToArray();
    }

    private static IReadOnlyList<TraceabilityPath> BuildDependencyPaths(
        string anchorArtifactId,
        IReadOnlyList<PersistedArtifactRelationship> relationships)
    {
        var outgoingDependencies = relationships
            .Where(relationship => relationship.Kind == ArtifactRelationshipKind.DependsOn)
            .GroupBy(relationship => relationship.SourceArtifactId, StringComparer.Ordinal)
            .ToDictionary(
                group => group.Key,
                group => (IReadOnlyList<PersistedArtifactRelationship>)group
                    .OrderBy(relationship => relationship.TargetArtifactId, StringComparer.Ordinal)
                    .ThenBy(relationship => relationship.SourceRevision)
                    .ToArray(),
                StringComparer.Ordinal);

        return Traverse(
            anchorArtifactId,
            currentArtifactId => outgoingDependencies.TryGetValue(currentArtifactId, out var edges)
                ? edges.Select(edge => new TraversalStep(
                    edge.TargetArtifactId,
                    new TraceabilityPathSegment(
                        edge.SourceArtifactId,
                        edge.Kind,
                        edge.TargetArtifactId,
                        TraceabilityTraversalDirection.Outgoing)))
                : []);
    }

    private static IReadOnlyList<TraceabilityPath> BuildImpactPaths(
        string anchorArtifactId,
        IReadOnlyList<PersistedArtifactRelationship> relationships)
    {
        var outgoingAffects = relationships
            .Where(relationship => relationship.Kind == ArtifactRelationshipKind.Affects)
            .GroupBy(relationship => relationship.SourceArtifactId, StringComparer.Ordinal)
            .ToDictionary(
                group => group.Key,
                group => (IReadOnlyList<PersistedArtifactRelationship>)group
                    .OrderBy(relationship => relationship.TargetArtifactId, StringComparer.Ordinal)
                    .ThenBy(relationship => relationship.SourceRevision)
                    .ToArray(),
                StringComparer.Ordinal);

        var incomingDependencies = relationships
            .Where(relationship => relationship.Kind == ArtifactRelationshipKind.DependsOn)
            .GroupBy(relationship => relationship.TargetArtifactId, StringComparer.Ordinal)
            .ToDictionary(
                group => group.Key,
                group => (IReadOnlyList<PersistedArtifactRelationship>)group
                    .OrderBy(relationship => relationship.SourceArtifactId, StringComparer.Ordinal)
                    .ThenBy(relationship => relationship.SourceRevision)
                    .ToArray(),
                StringComparer.Ordinal);

        return Traverse(
            anchorArtifactId,
            currentArtifactId =>
            {
                var steps = new List<TraversalStep>();
                if (outgoingAffects.TryGetValue(currentArtifactId, out var affects))
                {
                    steps.AddRange(affects.Select(edge => new TraversalStep(
                        edge.TargetArtifactId,
                        new TraceabilityPathSegment(
                            edge.SourceArtifactId,
                            edge.Kind,
                            edge.TargetArtifactId,
                            TraceabilityTraversalDirection.Outgoing))));
                }

                if (incomingDependencies.TryGetValue(currentArtifactId, out var dependents))
                {
                    steps.AddRange(dependents.Select(edge => new TraversalStep(
                        edge.SourceArtifactId,
                        new TraceabilityPathSegment(
                            edge.SourceArtifactId,
                            edge.Kind,
                            edge.TargetArtifactId,
                            TraceabilityTraversalDirection.Incoming))));
                }

                return steps
                    .OrderBy(step => step.Segment.RelationshipKind == ArtifactRelationshipKind.Affects ? 0 : 1)
                    .ThenBy(step => step.NextArtifactId, StringComparer.Ordinal)
                    .ThenBy(step => step.Segment.TraversalDirection)
                    .ToArray();
            });
    }

    private static IReadOnlyList<TraceabilityPath> Traverse(
        string anchorArtifactId,
        Func<string, IEnumerable<TraversalStep>> getSteps)
    {
        var paths = new List<TraceabilityPath>();
        var pending = new Queue<TraversalState>();
        pending.Enqueue(new TraversalState(
            [anchorArtifactId],
            []));

        while (pending.Count > 0)
        {
            var current = pending.Dequeue();
            foreach (var step in getSteps(current.ArtifactIds[^1]))
            {
                if (current.ArtifactIds.Contains(step.NextArtifactId, StringComparer.Ordinal))
                {
                    continue;
                }

                var nextArtifactIds = current.ArtifactIds.Concat([step.NextArtifactId]).ToArray();
                var nextSegments = current.Segments.Concat([step.Segment]).ToArray();
                var nextState = new TraversalState(nextArtifactIds, nextSegments);

                paths.Add(new TraceabilityPath(nextArtifactIds, nextSegments));
                pending.Enqueue(nextState);
            }
        }

        return paths;
    }

    private static TraceabilityPath CreatePath(
        string anchorArtifactId,
        string nextArtifactId,
        TraceabilityPathSegment segment) =>
        new(
            [anchorArtifactId, nextArtifactId],
            [segment]);

    private sealed record TraversalStep(
        string NextArtifactId,
        TraceabilityPathSegment Segment);

    private sealed record TraversalState(
        IReadOnlyList<string> ArtifactIds,
        IReadOnlyList<TraceabilityPathSegment> Segments);
}
