using System.Text.Json;
using AzureDevOpsMcp.Configuration;
using AzureDevOpsMcp.Services;
using AzureDevOpsMcp.Tools.PullRequests;
using NSubstitute;

namespace AzureDevOpsMcp.Tests.Tools.PullRequests;

public class CreatePullRequestToolTests
{
    private readonly AdoOptions _options = new()
    {
        ServerUrl = "https://dev.azure.com/testorg",
        PatToken = "test-pat",
        Project = "DefaultProject"
    };

    [Fact(DisplayName = "執行工具（基本建立）- 成功建立 PR 並回傳含 pullRequestId 的 JSON")]
    public async Task Execute_CreatesPullRequest()
    {
        // Arrange
        var service = Substitute.For<IAdoRepositoriesService>();
        var json = JsonDocument.Parse("""{"pullRequestId":55,"title":"My PR","status":"active"}""").RootElement;
        service.CreatePullRequestAsync("myrepo", "DefaultProject", "feature/abc", "main", "My PR", null, null).Returns(json);

        // Act
        var result = await CreatePullRequestTool.Execute(service, _options, "myrepo", "feature/abc", "main", "My PR");

        // Assert
        Assert.Contains("55", result);
        Assert.Contains("My PR", result);
        await service.Received(1).CreatePullRequestAsync("myrepo", "DefaultProject", "feature/abc", "main", "My PR", null, null);
    }

    [Fact(DisplayName = "執行工具並傳入 description 與 reviewers - 正確傳遞選填參數")]
    public async Task Execute_PassesDescriptionAndReviewers()
    {
        // Arrange
        var service = Substitute.For<IAdoRepositoriesService>();
        var json = JsonDocument.Parse("""{"pullRequestId":56,"title":"Feature PR"}""").RootElement;
        service.CreatePullRequestAsync("repo1", "DefaultProject",
            "refs/heads/feature/xyz", "refs/heads/main", "Feature PR",
            "Some description", Arg.Is<string[]?>(r => r != null && r.Length == 1 && r[0] == "user-guid-123"))
            .Returns(json);

        // Act
        var result = await CreatePullRequestTool.Execute(service, _options, "repo1",
            "refs/heads/feature/xyz", "refs/heads/main", "Feature PR",
            description: "Some description", reviewers: ["user-guid-123"]);

        // Assert
        Assert.Contains("56", result);
        await service.Received(1).CreatePullRequestAsync("repo1", "DefaultProject",
            "refs/heads/feature/xyz", "refs/heads/main", "Feature PR",
            "Some description", Arg.Is<string[]?>(r => r != null && r.Length == 1));
    }

    [Fact(DisplayName = "執行工具（未提供專案名稱）- 自動使用預設專案")]
    public async Task Execute_UsesDefaultProject_WhenProjectIsNull()
    {
        // Arrange
        var service = Substitute.For<IAdoRepositoriesService>();
        var json = JsonDocument.Parse("""{"pullRequestId":57}""").RootElement;
        service.CreatePullRequestAsync("repo1", "DefaultProject", "src", "tgt", "Title", null, null).Returns(json);

        // Act
        await CreatePullRequestTool.Execute(service, _options, "repo1", "src", "tgt", "Title");

        // Assert
        await service.Received(1).CreatePullRequestAsync("repo1", "DefaultProject", "src", "tgt", "Title", null, null);
    }

    [Fact(DisplayName = "執行工具（無專案設定）- 應拋出 ArgumentException")]
    public async Task Execute_ThrowsWhenNoProject()
    {
        // Arrange
        var service = Substitute.For<IAdoRepositoriesService>();
        var options = new AdoOptions { ServerUrl = "https://test", PatToken = "pat" };

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() =>
            CreatePullRequestTool.Execute(service, options, "repo1", "src", "tgt", "Title"));
    }
}
