using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;

namespace Memora.Ui.Tests;

public sealed class OperatorShellSmokeTests : IClassFixture<OperatorShellFactory>
{
    private readonly OperatorShellFactory _factory;

    public OperatorShellSmokeTests(OperatorShellFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task Root_renders_project_selector()
    {
        using var client = _factory.CreateClient();

        var html = await client.GetStringAsync("/");

        Assert.Contains("Select a project", html);
        Assert.Contains("Demo Project", html);
        Assert.Contains("Project Selector", html);
    }

    [Fact]
    public async Task Project_page_renders_artifact_browser_and_queue()
    {
        using var client = _factory.CreateClient();

        var html = await client.GetStringAsync("/projects/demo-project");

        Assert.Contains("Artifact Browser", html);
        Assert.Contains("Approval Queue", html);
        Assert.Contains("CHR-001.r0001.md", html);
    }

    [Fact]
    public async Task Artifact_page_renders_draft_editor()
    {
        using var client = _factory.CreateClient();

        var html = await client.GetStringAsync("/projects/demo-project/artifacts?path=drafts%2Fplan%2FPLN-001.r0001.md");

        Assert.Contains("Edit Draft", html);
        Assert.Contains("Save new draft revision", html);
        Assert.Contains("Expand Milestone 1 test coverage", html);
    }

    [Fact]
    public async Task Review_page_renders_revision_preview()
    {
        using var client = _factory.CreateClient();

        var html = await client.GetStringAsync("/projects/demo-project/review?path=drafts%2Fplan%2FPLN-001.r0001.md");

        Assert.Contains("Revision Review", html);
        Assert.Contains("Current UI boundary", html);
        Assert.Contains("approval and rejection persistence remain outside this UI slice", html, StringComparison.OrdinalIgnoreCase);
    }
}

public sealed class OperatorShellFactory : WebApplicationFactory<Program>
{
    private readonly string _tempRootPath;

    public OperatorShellFactory()
    {
        var repositoryRoot = FindRepositoryRoot();
        var sourceRoot = Path.Combine(repositoryRoot, "samples", "workspaces");
        _tempRootPath = Path.Combine(Path.GetTempPath(), "Memora.Ui.Tests", Guid.NewGuid().ToString("N"));
        CopyDirectory(sourceRoot, _tempRootPath);
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureAppConfiguration((_, configBuilder) =>
        {
            configBuilder.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["MemoraUi:WorkspacesRoot"] = _tempRootPath
            });
        });
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);

        if (disposing && Directory.Exists(_tempRootPath))
        {
            Directory.Delete(_tempRootPath, recursive: true);
        }
    }

    private static string FindRepositoryRoot()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);

        while (directory is not null)
        {
            if (File.Exists(Path.Combine(directory.FullName, "Memora.sln")))
            {
                return directory.FullName;
            }

            directory = directory.Parent;
        }

        throw new DirectoryNotFoundException("Could not locate the repository root for Memora.Ui tests.");
    }

    private static void CopyDirectory(string sourceDirectory, string targetDirectory)
    {
        Directory.CreateDirectory(targetDirectory);

        foreach (var filePath in Directory.EnumerateFiles(sourceDirectory))
        {
            File.Copy(filePath, Path.Combine(targetDirectory, Path.GetFileName(filePath)), overwrite: true);
        }

        foreach (var directoryPath in Directory.EnumerateDirectories(sourceDirectory))
        {
            CopyDirectory(
                directoryPath,
                Path.Combine(targetDirectory, Path.GetFileName(directoryPath)));
        }
    }
}
