using Memora.Core.Artifacts;
using Memora.Core.Validation;

namespace Memora.Core.Tests.Artifacts;

public sealed class LifecycleTransitionValidatorTests
{
    private readonly ArtifactFactory _factory = new();

    [Theory]
    [InlineData(ArtifactStatus.Proposed, ArtifactStatus.Draft)]
    [InlineData(ArtifactStatus.Draft, ArtifactStatus.Approved)]
    [InlineData(ArtifactStatus.Draft, ArtifactStatus.Deprecated)]
    [InlineData(ArtifactStatus.Approved, ArtifactStatus.Superseded)]
    [InlineData(ArtifactStatus.Approved, ArtifactStatus.Deprecated)]
    public void AllowedTransitions_AreAccepted(ArtifactStatus from, ArtifactStatus to)
    {
        var result = LifecycleTransitionValidator.Validate(from, to);

        Assert.True(result.IsValid);
    }

    [Fact]
    public void ProposedToApproved_IsRejected()
    {
        var result = LifecycleTransitionValidator.Validate(ArtifactStatus.Proposed, ArtifactStatus.Approved);

        Assert.False(result.IsValid);
        Assert.Contains(result.Issues, issue => issue.Code == "artifact.lifecycle.transition.invalid");
    }

    [Fact]
    public void ApprovedArtifactWithoutNewRevision_IsRejected()
    {
        var current = CreateArtifact(ArtifactStatus.Approved, 2);
        var next = CreateArtifact(ArtifactStatus.Deprecated, 2);

        var result = LifecycleTransitionValidator.Validate(current, next);

        Assert.False(result.IsValid);
        Assert.Contains(result.Issues, issue => issue.Path == "revision" && issue.Code == "artifact.lifecycle.revision.required");
    }

    [Fact]
    public void LifecycleTransition_WithChangedIdentity_IsRejected()
    {
        var current = CreateArtifact(ArtifactStatus.Draft, 1);
        var next = current with { Id = "CHR-999", ProjectId = "other-project" };

        var result = LifecycleTransitionValidator.Validate(current, next);

        Assert.False(result.IsValid);
        Assert.Contains(result.Issues, issue => issue.Path == "id" && issue.Code == "artifact.lifecycle.id.mismatch");
        Assert.Contains(result.Issues, issue => issue.Path == "project_id" && issue.Code == "artifact.lifecycle.project_id.mismatch");
    }

    private ProjectCharterArtifact CreateArtifact(ArtifactStatus status, int revision)
    {
        var frontmatter = ArtifactTestBuilder.CreateFrontmatter(ArtifactType.Charter);
        var sections = ArtifactTestBuilder.CreateSections(ArtifactType.Charter);
        frontmatter["status"] = status.ToSchemaValue();
        frontmatter["revision"] = revision;

        var result = _factory.Create(frontmatter, ArtifactTestBuilder.CreateBody(sections), sections);
        return Assert.IsType<ProjectCharterArtifact>(result.Artifact);
    }
}
