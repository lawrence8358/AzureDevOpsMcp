using System.Net;
using System.Text.Json;
using AzureDevOpsMcp.Configuration;
using AzureDevOpsMcp.Services;
using AzureDevOpsMcp.Tools.Git;
using NSubstitute;
using NSubstitute.ExceptionExtensions;

namespace AzureDevOpsMcp.Tests.Tools.Git;

public class GetItemToolTests
{
    private readonly AdoOptions _options = new()
    {
        ServerUrl = "https://dev.azure.com/testorg",
        PatToken = "test-pat",
        Project = "MyProject"
    };

    [Fact(DisplayName = "執行工具（取得檔案）- 正確回傳檔案內容")]
    public async Task Execute_ReturnsFileContent()
    {
        // Arrange
        var service = Substitute.For<IAdoRepositoriesService>();
        var json = JsonDocument.Parse("""{"path":"/src/Program.cs","content":"using System;"}""").RootElement;
        service.GetItemAsync("repo1", "/src/Program.cs", "MyProject", null).Returns(json);

        // Act
        var result = await GetItemTool.Execute(service, _options, "repo1", "/src/Program.cs");

        // Assert
        Assert.Contains("Program.cs", result);
        await service.Received(1).GetItemAsync("repo1", "/src/Program.cs", "MyProject", null);
    }

    [Fact(DisplayName = "執行工具（無專案設定）- 應拋出 ArgumentException")]
    public async Task Execute_ThrowsWhenNoProject()
    {
        // Arrange
        var service = Substitute.For<IAdoWorkItemsService>();
        var options = new AdoOptions { ServerUrl = "https://test", PatToken = "pat" };
        var repoService = Substitute.For<IAdoRepositoriesService>();

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() =>
            GetItemTool.Execute(repoService, options, "repo1", "/README.md"));
    }

    [Fact(DisplayName = "儲存庫不存在（404 GitRepositoryNotFoundException）- 應回傳含操作引導的訊息而非拋出例外")]
    public async Task Execute_Returns_GuidanceMessage_WhenRepositoryNotFound()
    {
        // Arrange
        var service = Substitute.For<IAdoRepositoriesService>();
        var errorBody = """{"$id":"1","typeKey":"GitRepositoryNotFoundException","message":"TF401019: The Git repository with name or identifier BadRepo does not exist.","errorCode":0}""";
        service.GetItemAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string?>())
            .ThrowsAsync(new HttpRequestException(
                $"Azure DevOps API request failed with status 404 (NotFound). Response: {errorBody}",
                null,
                HttpStatusCode.NotFound));

        // Act
        var result = await GetItemTool.Execute(service, _options, "BadRepo", "/README.md");

        // Assert
        Assert.Contains("BadRepo", result);
        Assert.Contains("mcp_ado_git_list_repositories", result);
        Assert.Contains("MyProject", result);
    }

    [Fact(DisplayName = "其他 404 錯誤（非儲存庫不存在）- 應重新拋出例外")]
    public async Task Execute_Rethrows_OnOther404()
    {
        // Arrange
        var service = Substitute.For<IAdoRepositoriesService>();
        var errorBody = """{"$id":"1","typeKey":"SomeOtherException","message":"Not found","errorCode":0}""";
        service.GetItemAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string?>())
            .ThrowsAsync(new HttpRequestException(
                $"Azure DevOps API request failed with status 404 (NotFound). Response: {errorBody}",
                null,
                HttpStatusCode.NotFound));

        // Act & Assert
        await Assert.ThrowsAsync<HttpRequestException>(() =>
            GetItemTool.Execute(service, _options, "repo1", "/README.md"));
    }
}
