using Memora.Core.Validation;

namespace Memora.Storage.Parsing;

public sealed class ArtifactMarkdownParser
{
    private readonly ArtifactFactory _artifactFactory = new();

    public ArtifactCreationResult Parse(string markdown)
    {
        ArgumentNullException.ThrowIfNull(markdown);

        var frontmatterResult = FrontmatterBlockParser.Parse(markdown);

        if (!frontmatterResult.Validation.IsValid)
        {
            return new ArtifactCreationResult(null, frontmatterResult.Validation);
        }

        var sections = MarkdownSectionExtractor.Extract(frontmatterResult.Body);
        return _artifactFactory.Create(frontmatterResult.Frontmatter, frontmatterResult.Body, sections);
    }
}

internal sealed record FrontmatterParseResult(
    IReadOnlyDictionary<string, object?> Frontmatter,
    string Body,
    ArtifactValidationResult Validation);
