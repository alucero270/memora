using System.Text.Json;
using Memora.Core.Projects;

namespace Memora.Storage.Workspaces;

public sealed class WorkspaceDiscovery
{
    public ProjectWorkspace Load(string workspaceRootPath)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(workspaceRootPath);

        var normalizedWorkspaceRootPath = Path.GetFullPath(workspaceRootPath);
        var metadataPath = Path.Combine(normalizedWorkspaceRootPath, "project.json");

        if (!File.Exists(metadataPath))
        {
            throw new FileNotFoundException("Workspace metadata file was not found.", metadataPath);
        }

        var metadata = LoadMetadata(metadataPath);
        return new ProjectWorkspace(metadata, normalizedWorkspaceRootPath);
    }

    public IReadOnlyList<ProjectWorkspace> Discover(string workspacesRootPath)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(workspacesRootPath);

        var normalizedWorkspacesRootPath = Path.GetFullPath(workspacesRootPath);
        if (!Directory.Exists(normalizedWorkspacesRootPath))
        {
            throw new DirectoryNotFoundException($"Workspace root directory '{normalizedWorkspacesRootPath}' was not found.");
        }

        var workspaces = Directory
            .EnumerateDirectories(normalizedWorkspacesRootPath)
            .OrderBy(path => path, StringComparer.Ordinal)
            .Where(path => File.Exists(Path.Combine(path, "project.json")))
            .Select(Load)
            .ToList();

        EnsureDistinctProjectIds(workspaces);
        return workspaces;
    }

    private static ProjectMetadata LoadMetadata(string metadataPath)
    {
        using var document = JsonDocument.Parse(File.ReadAllText(metadataPath));
        if (document.RootElement.ValueKind != JsonValueKind.Object)
        {
            throw new InvalidDataException($"Workspace metadata '{metadataPath}' must contain a JSON object.");
        }

        var projectId = ReadRequiredString(document.RootElement, "projectId", metadataPath);
        var name = ReadRequiredString(document.RootElement, "name", metadataPath);
        var status = ReadOptionalString(document.RootElement, "status", metadataPath);

        return new ProjectMetadata(projectId, name, status);
    }

    private static string ReadRequiredString(JsonElement element, string propertyName, string metadataPath)
    {
        var value = ReadOptionalString(element, propertyName, metadataPath);
        if (value is null)
        {
            throw new InvalidDataException($"Workspace metadata '{metadataPath}' must contain a non-empty '{propertyName}' value.");
        }

        return value;
    }

    private static string? ReadOptionalString(JsonElement element, string propertyName, string metadataPath)
    {
        if (!element.TryGetProperty(propertyName, out var property))
        {
            return null;
        }

        if (property.ValueKind == JsonValueKind.Null)
        {
            return null;
        }

        if (property.ValueKind != JsonValueKind.String)
        {
            throw new InvalidDataException($"Workspace metadata '{metadataPath}' property '{propertyName}' must be a string.");
        }

        var value = property.GetString()?.Trim();
        return string.IsNullOrWhiteSpace(value) ? null : value;
    }

    private static void EnsureDistinctProjectIds(IReadOnlyList<ProjectWorkspace> workspaces)
    {
        var duplicates = workspaces
            .GroupBy(workspace => workspace.ProjectId, StringComparer.Ordinal)
            .Where(group => group.Count() > 1)
            .ToList();

        if (duplicates.Count == 0)
        {
            return;
        }

        var duplicateDetails = duplicates
            .Select(group => $"{group.Key}: {string.Join(", ", group.Select(workspace => workspace.RootPath))}");

        throw new InvalidDataException(
            $"Workspace discovery found duplicate project ids. {string.Join("; ", duplicateDetails)}");
    }
}
