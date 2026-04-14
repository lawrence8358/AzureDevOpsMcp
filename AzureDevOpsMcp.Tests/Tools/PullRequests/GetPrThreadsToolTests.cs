using System.Text.Json;
using AzureDevOpsMcp.Configuration;
using AzureDevOpsMcp.Services;
using AzureDevOpsMcp.Tools.PullRequests;
using NSubstitute;

namespace AzureDevOpsMcp.Tests.Tools.PullRequests;

public class GetPrThreadsToolTests
{
    private readonly AdoOptions _options = new()
    {
        ServerUrl = "https://dev.azure.com/testorg",
        PatToken = "test-pat",
        Project = "DefaultProject"
    };

    [Fact(DisplayName = "執行工具（取得 threads）- 回傳包含討論串的 JSON")]
    public async Task Execute_ReturnsThreads()
    {
        // Arrange
        var service = Substitute.For<IAdoRepositoriesService>();
        var json = JsonDocument.Parse("""{"value":[{"id":1,"status":"active","comments":[{"content":"Please review"}]}],"count":1}""").RootElement;
        service.GetPrThreadsAsync("myrepo", 30, "DefaultProject").Returns(json);

        // Act
        var result = await GetPrThreadsTool.Execute(service, _options, "myrepo", 30);

        // Assert
        Assert.Contains("Please review", result);
        await service.Received(1).GetPrThreadsAsync("myrepo", 30, "DefaultProject");
    }

    [Fact(DisplayName = "執行工具（未提供專案名稱）- 自動使用預設專案")]
    public async Task Execute_UsesDefaultProject_WhenProjectIsNull()
    {
        // Arrange
        var service = Substitute.For<IAdoRepositoriesService>();
        var json = JsonDocument.Parse("""{"value":[],"count":0}""").RootElement;
        service.GetPrThreadsAsync("repo1", 5, "DefaultProject").Returns(json);

        // Act
        await GetPrThreadsTool.Execute(service, _options, "repo1", 5);

        // Assert
        await service.Received(1).GetPrThreadsAsync("repo1", 5, "DefaultProject");
    }

    [Fact(DisplayName = "執行工具（無專案設定）- 應拋出 ArgumentException")]
    public async Task Execute_ThrowsWhenNoProject()
    {
        // Arrange
        var service = Substitute.For<IAdoRepositoriesService>();
        var options = new AdoOptions { ServerUrl = "https://test", PatToken = "pat" };

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() =>
            GetPrThreadsTool.Execute(service, options, "repo1", 1));
    }
}
