using Memora.Core.Artifacts;
using Memora.Storage.Parsing;

namespace Memora.Storage.Tests.Parsing;

public sealed class ArtifactMarkdownParserTests
{
    private readonly ArtifactMarkdownParser _parser = new();

    [Fact]
    public void ValidCharterMarkdown_ParsesAndValidates()
    {
        const string markdown = """
                                 ---
                                 id: CHR-001
                                 project_id: memora
                                 type: charter
                                 status: draft
                                 title: Memora charter
                                 created_at: 2026-04-14T12:00:00Z
                                 updated_at: 2026-04-14T12:30:00Z
                                 revision: 1
                                 tags: []
                                 provenance: user
                                 reason: schema foundation
                                 links:
                                   depends_on: []
                                   affects:
                                     - ADR-001
                                   derived_from: []
                                   supersedes: []
                                 ---
                                 ## Problem Statement
                                 Durable project memory is missing.

                                 ## Primary Users / Stakeholders
                                 Product and engineering teams.

                                 ## Current Pain
                                 Context is lost between iterations.

                                 ## Desired Outcome
                                 Artifact state remains structured and reviewable.

                                 ## Definition of Success
                                 Teams can validate artifacts deterministically.
                                 """;

        var result = _parser.Parse(markdown);

        Assert.True(result.Validation.IsValid);
        var artifact = Assert.IsType<ProjectCharterArtifact>(result.Artifact);
        Assert.Equal("memora", artifact.ProjectId);
        Assert.Single(artifact.Links.Affects);
        Assert.Equal("ADR-001", artifact.Links.Affects[0]);
    }

    [Fact]
    public void InvalidFrontmatter_IsRejectedCleanly()
    {
        const string markdown = """
                                 ---
                                 id: CHR-001
                                 project_id memora
                                 type: charter
                                 ---
                                 ## Problem Statement
                                 Missing colon above.
                                 """;

        var result = _parser.Parse(markdown);

        Assert.False(result.Validation.IsValid);
        Assert.Null(result.Artifact);
        Assert.Contains(result.Validation.Issues, issue => issue.Code == "frontmatter.parse");
    }
}
