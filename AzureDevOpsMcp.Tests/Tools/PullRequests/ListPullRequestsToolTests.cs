using System.Text.Json;
using AzureDevOpsMcp.Configuration;
using AzureDevOpsMcp.Services;
using AzureDevOpsMcp.Tools.PullRequests;
using NSubstitute;

namespace AzureDevOpsMcp.Tests.Tools.PullRequests;

public class ListPullRequestsToolTests
{
    private readonly AdoOptions _options = new()
    {
        ServerUrl = "https://dev.azure.com/testorg",
        PatToken = "test-pat",
        Project = "DefaultProject"
    };

    [Fact(DisplayName = "執行工具（指定專案）- 使用指定的專案名稱列出 PR")]
    public async Task Execute_UsesProvidedProject()
    {
        // Arrange
        var service = Substitute.For<IAdoRepositoriesService>();
        var json = JsonDocument.Parse("""{"value":[{"pullRequestId":1,"title":"PR1"}]}""").RootElement;
        service.ListPullRequestsAsync("repo1", "ExplicitProject", null, null, null, null).Returns(json);

        // Act
        var result = await ListPullRequestsTool.Execute(service, _options, "repo1", "ExplicitProject");

        // Assert
        Assert.Contains("PR1", result);
        await service.Received(1).ListPullRequestsAsync("repo1", "ExplicitProject", null, null, null, null);
    }

    [Fact(DisplayName = "執行工具（未提供專案名稱）- 自動使用預設專案")]
    public async Task Execute_UsesDefaultProject_WhenProjectIsNull()
    {
        // Arrange
        var service = Substitute.For<IAdoRepositoriesService>();
        var json = JsonDocument.Parse("""{"value":[]}""").RootElement;
        service.ListPullRequestsAsync("repo1", "DefaultProject", null, null, null, null).Returns(json);

        // Act
        await ListPullRequestsTool.Execute(service, _options, "repo1");

        // Assert
        await service.Received(1).ListPullRequestsAsync("repo1", "DefaultProject", null, null, null, null);
    }

    [Fact(DisplayName = "執行工具（無專案設定）- 應拋出 ArgumentException")]
    public async Task Execute_ThrowsWhenNoProject()
    {
        // Arrange
        var service = Substitute.For<IAdoRepositoriesService>();
        var options = new AdoOptions { ServerUrl = "https://test", PatToken = "pat" };

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() =>
            ListPullRequestsTool.Execute(service, options, "repo1"));
    }

    [Fact(DisplayName = "執行工具並傳入篩選條件 - 正確傳遞篩選參數至服務")]
    public async Task Execute_PassesFilters()
    {
        // Arrange
        var service = Substitute.For<IAdoRepositoriesService>();
        var json = JsonDocument.Parse("""{"value":[]}""").RootElement;
        service.ListPullRequestsAsync("repo1", "DefaultProject", "active", "creator1", "reviewer1", 10).Returns(json);

        // Act
        await ListPullRequestsTool.Execute(service, _options, "repo1", status: "active", creatorId: "creator1", reviewerId: "reviewer1", top: 10);

        // Assert
        await service.Received(1).ListPullRequestsAsync("repo1", "DefaultProject", "active", "creator1", "reviewer1", 10);
    }
}
