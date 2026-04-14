using Memora.Core.Validation;

namespace Memora.Storage.Parsing;

internal static class FrontmatterBlockParser
{
    public static FrontmatterParseResult Parse(string markdown)
    {
        var normalized = markdown.Replace("\r\n", "\n", StringComparison.Ordinal);
        var issues = new List<ArtifactValidationIssue>();

        if (!normalized.StartsWith("---\n", StringComparison.Ordinal))
        {
            issues.Add(new ArtifactValidationIssue(
                "frontmatter.missing",
                "Artifact markdown must begin with a frontmatter block delimited by '---'.",
                "frontmatter"));

            return new FrontmatterParseResult(new Dictionary<string, object?>(), string.Empty, new ArtifactValidationResult(issues));
        }

        var closingIndex = normalized.IndexOf("\n---\n", StringComparison.Ordinal);

        if (closingIndex < 0)
        {
            issues.Add(new ArtifactValidationIssue(
                "frontmatter.missing_end",
                "Artifact frontmatter block is missing the closing '---' delimiter.",
                "frontmatter"));

            return new FrontmatterParseResult(new Dictionary<string, object?>(), string.Empty, new ArtifactValidationResult(issues));
        }

        var frontmatterContent = normalized.Substring(4, closingIndex - 4);
        var body = normalized[(closingIndex + 5)..];
        var result = StrictFrontmatterParser.Parse(frontmatterContent);

        if (!result.Validation.IsValid)
        {
            return new FrontmatterParseResult(new Dictionary<string, object?>(), body, result.Validation);
        }

        return new FrontmatterParseResult(result.Frontmatter, body, ArtifactValidationResult.Success);
    }
}
