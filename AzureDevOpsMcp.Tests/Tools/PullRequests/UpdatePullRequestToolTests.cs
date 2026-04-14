using System.Text.Json;
using AzureDevOpsMcp.Configuration;
using AzureDevOpsMcp.Services;
using AzureDevOpsMcp.Tools.PullRequests;
using NSubstitute;

namespace AzureDevOpsMcp.Tests.Tools.PullRequests;

public class UpdatePullRequestToolTests
{
    private readonly AdoOptions _options = new()
    {
        ServerUrl = "https://dev.azure.com/testorg",
        PatToken = "test-pat",
        Project = "DefaultProject"
    };

    [Fact(DisplayName = "執行工具（更新 status）- 正確傳遞 status 至服務並回傳結果")]
    public async Task Execute_UpdatesStatus()
    {
        // Arrange
        var service = Substitute.For<IAdoRepositoriesService>();
        var json = JsonDocument.Parse("""{"pullRequestId":20,"status":"completed"}""").RootElement;
        service.UpdatePullRequestAsync("myrepo", 20, "DefaultProject", "completed", null, null).Returns(json);

        // Act
        var result = await UpdatePullRequestTool.Execute(service, _options, "myrepo", 20, status: "completed");

        // Assert
        Assert.Contains("completed", result);
        await service.Received(1).UpdatePullRequestAsync("myrepo", 20, "DefaultProject", "completed", null, null);
    }

    [Fact(DisplayName = "執行工具（更新 title）- 正確傳遞 title 至服務")]
    public async Task Execute_UpdatesTitle()
    {
        // Arrange
        var service = Substitute.For<IAdoRepositoriesService>();
        var json = JsonDocument.Parse("""{"pullRequestId":21,"title":"Updated Title"}""").RootElement;
        service.UpdatePullRequestAsync("myrepo", 21, "DefaultProject", null, "Updated Title", null).Returns(json);

        // Act
        var result = await UpdatePullRequestTool.Execute(service, _options, "myrepo", 21, title: "Updated Title");

        // Assert
        Assert.Contains("Updated Title", result);
        await service.Received(1).UpdatePullRequestAsync("myrepo", 21, "DefaultProject", null, "Updated Title", null);
    }

    [Fact(DisplayName = "執行工具（未提供專案名稱）- 自動使用預設專案")]
    public async Task Execute_UsesDefaultProject_WhenProjectIsNull()
    {
        // Arrange
        var service = Substitute.For<IAdoRepositoriesService>();
        var json = JsonDocument.Parse("""{"pullRequestId":22,"status":"active"}""").RootElement;
        service.UpdatePullRequestAsync("repo1", 22, "DefaultProject", "active", null, null).Returns(json);

        // Act
        await UpdatePullRequestTool.Execute(service, _options, "repo1", 22, status: "active");

        // Assert
        await service.Received(1).UpdatePullRequestAsync("repo1", 22, "DefaultProject", "active", null, null);
    }

    [Fact(DisplayName = "執行工具（無專案設定）- 應拋出 ArgumentException")]
    public async Task Execute_ThrowsWhenNoProject()
    {
        // Arrange
        var service = Substitute.For<IAdoRepositoriesService>();
        var options = new AdoOptions { ServerUrl = "https://test", PatToken = "pat" };

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() =>
            UpdatePullRequestTool.Execute(service, options, "repo1", 1));
    }
}
