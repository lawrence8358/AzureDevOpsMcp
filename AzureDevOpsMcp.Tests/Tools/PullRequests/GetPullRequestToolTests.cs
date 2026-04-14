using System.Text.Json;
using AzureDevOpsMcp.Configuration;
using AzureDevOpsMcp.Services;
using AzureDevOpsMcp.Tools.PullRequests;
using NSubstitute;

namespace AzureDevOpsMcp.Tests.Tools.PullRequests;

public class GetPullRequestToolTests
{
    private readonly AdoOptions _options = new()
    {
        ServerUrl = "https://dev.azure.com/testorg",
        PatToken = "test-pat",
        Project = "DefaultProject"
    };

    [Fact(DisplayName = "執行工具（取得 PR）- 回傳包含 pullRequestId 的 JSON")]
    public async Task Execute_ReturnsPullRequestWithId()
    {
        // Arrange
        var service = Substitute.For<IAdoRepositoriesService>();
        var json = JsonDocument.Parse("""{"pullRequestId":88,"title":"Test PR","status":"active","sourceRefName":"refs/heads/feature"}""").RootElement;
        service.GetPullRequestAsync("myrepo", 88, "DefaultProject").Returns(json);

        // Act
        var result = await GetPullRequestTool.Execute(service, _options, "myrepo", 88);

        // Assert
        Assert.Contains("88", result);
        Assert.Contains("Test PR", result);
        var doc = JsonDocument.Parse(result);
        Assert.True(doc.RootElement.TryGetProperty("pullRequestId", out _));
        await service.Received(1).GetPullRequestAsync("myrepo", 88, "DefaultProject");
    }

    [Fact(DisplayName = "執行工具（未提供專案名稱）- 自動使用預設專案")]
    public async Task Execute_UsesDefaultProject_WhenProjectIsNull()
    {
        // Arrange
        var service = Substitute.For<IAdoRepositoriesService>();
        var json = JsonDocument.Parse("""{"pullRequestId":10}""").RootElement;
        service.GetPullRequestAsync("repo1", 10, "DefaultProject").Returns(json);

        // Act
        await GetPullRequestTool.Execute(service, _options, "repo1", 10);

        // Assert
        await service.Received(1).GetPullRequestAsync("repo1", 10, "DefaultProject");
    }

    [Fact(DisplayName = "執行工具（指定專案覆寫預設）- 使用指定的專案名稱呼叫服務")]
    public async Task Execute_UsesProvidedProject()
    {
        // Arrange
        var service = Substitute.For<IAdoRepositoriesService>();
        var json = JsonDocument.Parse("""{"pullRequestId":99,"title":"PR in explicit project"}""").RootElement;
        service.GetPullRequestAsync("myrepo", 99, "ExplicitProject").Returns(json);

        // Act
        var result = await GetPullRequestTool.Execute(service, _options, "myrepo", 99, "ExplicitProject");

        // Assert
        Assert.Contains("99", result);
        Assert.Contains("PR in explicit project", result);
        await service.Received(1).GetPullRequestAsync("myrepo", 99, "ExplicitProject");
    }
}
