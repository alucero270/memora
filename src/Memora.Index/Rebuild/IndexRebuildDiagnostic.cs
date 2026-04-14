namespace Memora.Index.Rebuild;

public sealed record IndexRebuildDiagnostic(
    string FilePath,
    string Code,
    string Message,
    string Path);
