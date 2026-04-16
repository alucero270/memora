namespace Memora.Ui.Operator;

internal static class SampleWorkspacesBootstrapper
{
    public static string PrepareDefaultRoot(string contentRootPath)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(contentRootPath);

        var sourceRoot = Path.GetFullPath(Path.Combine(contentRootPath, "..", "..", "samples", "workspaces"));
        if (!Directory.Exists(sourceRoot))
        {
            throw new DirectoryNotFoundException($"Sample workspaces were not found at '{sourceRoot}'.");
        }

        var targetRoot = Path.Combine(Path.GetTempPath(), "Memora", "UiShell", "workspaces");
        if (Directory.Exists(targetRoot) && Directory.EnumerateFileSystemEntries(targetRoot).Any())
        {
            return targetRoot;
        }

        CopyDirectory(sourceRoot, targetRoot);
        return targetRoot;
    }

    private static void CopyDirectory(string sourceDirectory, string targetDirectory)
    {
        Directory.CreateDirectory(targetDirectory);

        foreach (var filePath in Directory.EnumerateFiles(sourceDirectory))
        {
            var fileName = Path.GetFileName(filePath);
            File.Copy(filePath, Path.Combine(targetDirectory, fileName), overwrite: true);
        }

        foreach (var directoryPath in Directory.EnumerateDirectories(sourceDirectory))
        {
            var directoryName = Path.GetFileName(directoryPath);
            CopyDirectory(directoryPath, Path.Combine(targetDirectory, directoryName));
        }
    }
}
