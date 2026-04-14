using System.Text.Json;
using AzureDevOpsMcp.Configuration;
using AzureDevOpsMcp.Services;
using AzureDevOpsMcp.Tools.PullRequests;
using NSubstitute;

namespace AzureDevOpsMcp.Tests.Tools.PullRequests;

public class CreatePrThreadToolTests
{
    private readonly AdoOptions _options = new()
    {
        ServerUrl = "https://dev.azure.com/testorg",
        PatToken = "test-pat",
        Project = "DefaultProject"
    };

    [Fact(DisplayName = "執行工具（建立評論）- 成功建立討論串並回傳 JSON")]
    public async Task Execute_CreatesThread()
    {
        // Arrange
        var service = Substitute.For<IAdoRepositoriesService>();
        var json = JsonDocument.Parse("""{"id":10,"status":"active","comments":[{"content":"LGTM!"}]}""").RootElement;
        service.CreatePrThreadAsync("myrepo", 30, "DefaultProject", "LGTM!", null).Returns(json);

        // Act
        var result = await CreatePrThreadTool.Execute(service, _options, "myrepo", 30, "LGTM!");

        // Assert
        Assert.Contains("LGTM!", result);
        await service.Received(1).CreatePrThreadAsync("myrepo", 30, "DefaultProject", "LGTM!", null);
    }

    [Fact(DisplayName = "執行工具並傳入 status - 正確傳遞討論串狀態至服務")]
    public async Task Execute_PassesStatus()
    {
        // Arrange
        var service = Substitute.For<IAdoRepositoriesService>();
        var json = JsonDocument.Parse("""{"id":11,"status":"fixed","comments":[{"content":"Fixed in next commit"}]}""").RootElement;
        service.CreatePrThreadAsync("myrepo", 31, "DefaultProject", "Fixed in next commit", "fixed").Returns(json);

        // Act
        var result = await CreatePrThreadTool.Execute(service, _options, "myrepo", 31, "Fixed in next commit", status: "fixed");

        // Assert
        Assert.Contains("fixed", result);
        await service.Received(1).CreatePrThreadAsync("myrepo", 31, "DefaultProject", "Fixed in next commit", "fixed");
    }

    [Fact(DisplayName = "執行工具（未提供專案名稱）- 自動使用預設專案")]
    public async Task Execute_UsesDefaultProject_WhenProjectIsNull()
    {
        // Arrange
        var service = Substitute.For<IAdoRepositoriesService>();
        var json = JsonDocument.Parse("""{"id":12}""").RootElement;
        service.CreatePrThreadAsync("repo1", 10, "DefaultProject", "Comment", null).Returns(json);

        // Act
        await CreatePrThreadTool.Execute(service, _options, "repo1", 10, "Comment");

        // Assert
        await service.Received(1).CreatePrThreadAsync("repo1", 10, "DefaultProject", "Comment", null);
    }

    [Fact(DisplayName = "執行工具（無專案設定）- 應拋出 ArgumentException")]
    public async Task Execute_ThrowsWhenNoProject()
    {
        // Arrange
        var service = Substitute.For<IAdoRepositoriesService>();
        var options = new AdoOptions { ServerUrl = "https://test", PatToken = "pat" };

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() =>
            CreatePrThreadTool.Execute(service, options, "repo1", 1, "content"));
    }
}
