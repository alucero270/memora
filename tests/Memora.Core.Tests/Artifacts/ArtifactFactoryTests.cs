using Memora.Core.Artifacts;
using Memora.Core.Validation;

namespace Memora.Core.Tests.Artifacts;

public sealed class ArtifactFactoryTests
{
    private readonly ArtifactFactory _factory = new();

    [Fact]
    public void ValidCharter_ParsesAndValidates()
    {
        var frontmatter = ArtifactTestBuilder.CreateFrontmatter(ArtifactType.Charter);
        var sections = ArtifactTestBuilder.CreateSections(ArtifactType.Charter);

        var result = _factory.Create(frontmatter, ArtifactTestBuilder.CreateBody(sections), sections);

        Assert.True(result.Validation.IsValid);
        var artifact = Assert.IsType<ProjectCharterArtifact>(result.Artifact);
        Assert.Equal("CHR-001", artifact.Id);
    }

    [Fact]
    public void InvalidStatusEnum_FailsValidation()
    {
        var frontmatter = ArtifactTestBuilder.CreateFrontmatter(ArtifactType.Charter);
        var sections = ArtifactTestBuilder.CreateSections(ArtifactType.Charter);
        frontmatter["status"] = "not_real";

        var result = _factory.Create(frontmatter, ArtifactTestBuilder.CreateBody(sections), sections);

        Assert.False(result.Validation.IsValid);
        Assert.Contains(result.Validation.Issues, issue => issue.Path == "status" && issue.Code == "artifact.status.invalid");
    }

    [Fact]
    public void InvalidTimestamp_FailsValidation()
    {
        var frontmatter = ArtifactTestBuilder.CreateFrontmatter(ArtifactType.Charter);
        var sections = ArtifactTestBuilder.CreateSections(ArtifactType.Charter);
        frontmatter["created_at"] = "2026-04-14T14:00:00+02:00";

        var result = _factory.Create(frontmatter, ArtifactTestBuilder.CreateBody(sections), sections);

        Assert.False(result.Validation.IsValid);
        Assert.Contains(result.Validation.Issues, issue => issue.Path == "created_at" && issue.Code == "artifact.timestamp.invalid");
    }

    [Fact]
    public void InvalidRelationshipKey_FailsValidation()
    {
        var frontmatter = ArtifactTestBuilder.CreateFrontmatter(ArtifactType.Charter);
        var sections = ArtifactTestBuilder.CreateSections(ArtifactType.Charter);
        var links = (Dictionary<string, object?>)frontmatter["links"]!;
        links["related_to"] = new List<object?>();

        var result = _factory.Create(frontmatter, ArtifactTestBuilder.CreateBody(sections), sections);

        Assert.False(result.Validation.IsValid);
        Assert.Contains(result.Validation.Issues, issue => issue.Path == "links.related_to" && issue.Code == "artifact.links.key.invalid");
    }

    [Fact]
    public void PlanMissingAcceptanceCriteria_FailsValidation()
    {
        var frontmatter = ArtifactTestBuilder.CreateFrontmatter(ArtifactType.Plan);
        var sections = ArtifactTestBuilder.CreateSections(ArtifactType.Plan);
        sections["Acceptance Criteria"] = "Needs measurable validation but no criteria are listed.";

        var result = _factory.Create(frontmatter, ArtifactTestBuilder.CreateBody(sections), sections);

        Assert.False(result.Validation.IsValid);
        Assert.Contains(result.Validation.Issues, issue => issue.Path == "body.sections.Acceptance Criteria" && issue.Code == "artifact.plan.acceptance_criteria.missing");
    }

    [Fact]
    public void SessionSummaryWithCanonicalTrue_FailsValidation()
    {
        var frontmatter = ArtifactTestBuilder.CreateFrontmatter(ArtifactType.SessionSummary);
        var sections = ArtifactTestBuilder.CreateSections(ArtifactType.SessionSummary);
        frontmatter["canonical"] = true;

        var result = _factory.Create(frontmatter, ArtifactTestBuilder.CreateBody(sections), sections);

        Assert.False(result.Validation.IsValid);
        Assert.Contains(result.Validation.Issues, issue => issue.Path == "canonical" && issue.Code == "artifact.session_summary.canonical.invalid");
    }

    [Theory]
    [MemberData(nameof(AllArtifactTypes))]
    public void MissingRequiredSection_FailsValidation(ArtifactType artifactType)
    {
        var frontmatter = ArtifactTestBuilder.CreateFrontmatter(artifactType);
        var sections = ArtifactTestBuilder.CreateSections(artifactType);
        var missingSection = Memora.Core.Validation.ArtifactBodyRules.GetRequiredSections(artifactType)[0];
        sections.Remove(missingSection);

        var result = _factory.Create(frontmatter, ArtifactTestBuilder.CreateBody(sections), sections);

        Assert.False(result.Validation.IsValid);
        Assert.Contains(result.Validation.Issues, issue => issue.Path == $"body.sections.{missingSection}" && issue.Code == "artifact.body.section.missing");
    }

    public static IEnumerable<object[]> AllArtifactTypes()
    {
        foreach (var artifactType in Enum.GetValues<ArtifactType>())
        {
            yield return [artifactType];
        }
    }
}
