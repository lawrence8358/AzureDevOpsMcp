using System.Net;
using System.Text.Json;
using AzureDevOpsMcp.Configuration;
using AzureDevOpsMcp.Services;
using AzureDevOpsMcp.Tools.Git;
using NSubstitute;
using NSubstitute.ExceptionExtensions;

namespace AzureDevOpsMcp.Tests.Tools.Git;

public class GetCommitsToolTests
{
    private readonly AdoOptions _options = new()
    {
        ServerUrl = "https://dev.azure.com/testorg",
        PatToken = "test-pat",
        Project = "DefaultProject"
    };

    [Fact(DisplayName = "執行工具（基本取得）- 回傳包含 commitId 的提交紀錄")]
    public async Task Execute_ReturnsCommits()
    {
        // Arrange
        var service = Substitute.For<IAdoRepositoriesService>();
        var json = JsonDocument.Parse("""{"value":[{"commitId":"abc123","comment":"Initial commit","author":{"name":"Alice"}}],"count":1}""").RootElement;
        service.GetCommitsAsync("myrepo", "DefaultProject", null, null, null, null).Returns(json);

        // Act
        var result = await GetCommitsTool.Execute(service, _options, "myrepo");

        // Assert
        Assert.Contains("abc123", result);
        Assert.Contains("Initial commit", result);
        await service.Received(1).GetCommitsAsync("myrepo", "DefaultProject", null, null, null, null);
    }

    [Fact(DisplayName = "執行工具並傳入篩選參數 - 正確傳遞所有篩選條件至服務")]
    public async Task Execute_PassesFilters()
    {
        // Arrange
        var service = Substitute.For<IAdoRepositoriesService>();
        var json = JsonDocument.Parse("""{"value":[],"count":0}""").RootElement;
        service.GetCommitsAsync("myrepo", "DefaultProject", "main", "/src", "Alice", 5).Returns(json);

        // Act
        await GetCommitsTool.Execute(service, _options, "myrepo",
            branch: "main", itemPath: "/src", author: "Alice", top: 5);

        // Assert
        await service.Received(1).GetCommitsAsync("myrepo", "DefaultProject", "main", "/src", "Alice", 5);
    }

    [Fact(DisplayName = "執行工具（未提供專案名稱）- 自動使用預設專案")]
    public async Task Execute_UsesDefaultProject_WhenProjectIsNull()
    {
        // Arrange
        var service = Substitute.For<IAdoRepositoriesService>();
        var json = JsonDocument.Parse("""{"value":[],"count":0}""").RootElement;
        service.GetCommitsAsync("repo1", "DefaultProject", null, null, null, null).Returns(json);

        // Act
        await GetCommitsTool.Execute(service, _options, "repo1");

        // Assert
        await service.Received(1).GetCommitsAsync("repo1", "DefaultProject", null, null, null, null);
    }

    [Fact(DisplayName = "儲存庫不存在（GitRepositoryNotFoundException）- 應回傳含操作引導的訊息")]
    public async Task Execute_ReturnsGuidanceMessage_WhenRepositoryNotFound()
    {
        // Arrange
        var service = Substitute.For<IAdoRepositoriesService>();
        var errorBody = """{"$id":"1","typeKey":"GitRepositoryNotFoundException","message":"TF401019: The Git repository with name or identifier BadRepo does not exist.","errorCode":0}""";
        service.GetCommitsAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<int?>())
            .ThrowsAsync(new HttpRequestException(
                $"Azure DevOps API request failed with status 404 (NotFound). Response: {errorBody}",
                null,
                HttpStatusCode.NotFound));

        // Act
        var result = await GetCommitsTool.Execute(service, _options, "BadRepo");

        // Assert
        Assert.Contains("BadRepo", result);
        Assert.Contains("mcp_ado_git_list_repositories", result);
    }
}
