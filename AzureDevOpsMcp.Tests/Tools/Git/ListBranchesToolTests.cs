using System.Net;
using System.Text.Json;
using AzureDevOpsMcp.Configuration;
using AzureDevOpsMcp.Services;
using AzureDevOpsMcp.Tools.Git;
using NSubstitute;
using NSubstitute.ExceptionExtensions;

namespace AzureDevOpsMcp.Tests.Tools.Git;

public class ListBranchesToolTests
{
    private readonly AdoOptions _options = new()
    {
        ServerUrl = "https://dev.azure.com/testorg",
        PatToken = "test-pat",
        Project = "DefaultProject"
    };

    [Fact(DisplayName = "執行工具（列出所有分支）- 回傳包含分支名稱的 JSON")]
    public async Task Execute_ReturnsBranches()
    {
        // Arrange
        var service = Substitute.For<IAdoRepositoriesService>();
        var json = JsonDocument.Parse("""{"value":[{"name":"refs/heads/main"},{"name":"refs/heads/develop"}],"count":2}""").RootElement;
        service.ListBranchesAsync("myrepo", "DefaultProject", null).Returns(json);

        // Act
        var result = await ListBranchesTool.Execute(service, _options, "myrepo");

        // Assert
        Assert.Contains("main", result);
        Assert.Contains("develop", result);
        await service.Received(1).ListBranchesAsync("myrepo", "DefaultProject", null);
    }

    [Fact(DisplayName = "執行工具並傳入 filter 參數 - 正確傳遞篩選字串至服務")]
    public async Task Execute_PassesFilterParam()
    {
        // Arrange
        var service = Substitute.For<IAdoRepositoriesService>();
        var json = JsonDocument.Parse("""{"value":[{"name":"refs/heads/feature/xyz"}],"count":1}""").RootElement;
        service.ListBranchesAsync("myrepo", "DefaultProject", "feature").Returns(json);

        // Act
        var result = await ListBranchesTool.Execute(service, _options, "myrepo", filter: "feature");

        // Assert
        Assert.Contains("feature/xyz", result);
        await service.Received(1).ListBranchesAsync("myrepo", "DefaultProject", "feature");
    }

    [Fact(DisplayName = "執行工具（未提供專案名稱）- 自動使用預設專案")]
    public async Task Execute_UsesDefaultProject_WhenProjectIsNull()
    {
        // Arrange
        var service = Substitute.For<IAdoRepositoriesService>();
        var json = JsonDocument.Parse("""{"value":[],"count":0}""").RootElement;
        service.ListBranchesAsync("repo1", "DefaultProject", null).Returns(json);

        // Act
        await ListBranchesTool.Execute(service, _options, "repo1");

        // Assert
        await service.Received(1).ListBranchesAsync("repo1", "DefaultProject", null);
    }

    [Fact(DisplayName = "儲存庫不存在（GitRepositoryNotFoundException）- 應回傳含操作引導的訊息")]
    public async Task Execute_ReturnsGuidanceMessage_WhenRepositoryNotFound()
    {
        // Arrange
        var service = Substitute.For<IAdoRepositoriesService>();
        var errorBody = """{"$id":"1","typeKey":"GitRepositoryNotFoundException","message":"TF401019: The Git repository with name or identifier BadRepo does not exist.","errorCode":0}""";
        service.ListBranchesAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string?>())
            .ThrowsAsync(new HttpRequestException(
                $"Azure DevOps API request failed with status 404 (NotFound). Response: {errorBody}",
                null,
                HttpStatusCode.NotFound));

        // Act
        var result = await ListBranchesTool.Execute(service, _options, "BadRepo");

        // Assert
        Assert.Contains("BadRepo", result);
        Assert.Contains("mcp_ado_git_list_repositories", result);
    }
}
