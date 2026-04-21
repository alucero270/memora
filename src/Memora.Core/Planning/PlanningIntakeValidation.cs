using System.Collections.ObjectModel;
using Memora.Core.Validation;

namespace Memora.Core.Planning;

public sealed record PlanningIntakeValidationIssue(
    string Code,
    string Message,
    string? Path = null)
{
    public string DiagnosticMessage => ValidationDiagnosticFormatter.Format(Code, Message, Path);
}

public sealed class PlanningIntakeValidationResult
{
    public PlanningIntakeValidationResult(IEnumerable<PlanningIntakeValidationIssue> issues)
    {
        Issues = new ReadOnlyCollection<PlanningIntakeValidationIssue>(issues.ToList());
    }

    public bool IsValid => Issues.Count == 0;

    public IReadOnlyList<PlanningIntakeValidationIssue> Issues { get; }

    public static PlanningIntakeValidationResult Success { get; } = new([]);
}

internal sealed class PlanningIntakeValidationCollector
{
    private readonly List<PlanningIntakeValidationIssue> _issues = [];

    public bool HasIssues => _issues.Count > 0;

    public void Add(string code, string message, string? path = null)
    {
        _issues.Add(new PlanningIntakeValidationIssue(code, message, path));
    }

    public PlanningIntakeValidationResult ToResult() => new(_issues);
}
