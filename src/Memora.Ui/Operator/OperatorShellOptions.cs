namespace Memora.Ui.Operator;

public sealed record OperatorShellOptions(
    string WorkspacesRootPath,
    bool UsesSeededSampleRoot)
{
    public string NormalizedWorkspacesRootPath { get; } = Path.GetFullPath(WorkspacesRootPath);
}
