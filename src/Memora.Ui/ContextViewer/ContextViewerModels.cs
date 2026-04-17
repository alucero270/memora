namespace Memora.Ui.ContextViewer;

public sealed record ContextViewerRequest(
    string ProjectId,
    string TaskDescription,
    bool IncludeDraftArtifacts,
    bool IncludeLayer3History);

public sealed record ContextViewerPageModel(
    string? ProjectId,
    string? TaskDescription,
    bool IncludeDraftArtifacts,
    bool IncludeLayer3History,
    string? ErrorMessage,
    IReadOnlyList<ContextViewerLayer> Layers);

public sealed record ContextViewerLayer(
    string Name,
    IReadOnlyList<ContextViewerArtifact> Artifacts);

public sealed record ContextViewerArtifact(
    string Id,
    string Title,
    string Type,
    string Status,
    IReadOnlyList<string> InclusionReasons);
