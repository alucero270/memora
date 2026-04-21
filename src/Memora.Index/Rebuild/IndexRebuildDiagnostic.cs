namespace Memora.Index.Rebuild;

public sealed record IndexRebuildDiagnostic(
    string FilePath,
    string Code,
    string Message,
    string Path)
{
    public string DiagnosticMessage =>
        $"{Message} (code: {Code}; path: {FormatPath(Path)}; file: {FilePath}; source: filesystem truth; index: derived SQLite index)";

    private static string FormatPath(string path) =>
        string.IsNullOrWhiteSpace(path) ? "not provided" : path;
}
