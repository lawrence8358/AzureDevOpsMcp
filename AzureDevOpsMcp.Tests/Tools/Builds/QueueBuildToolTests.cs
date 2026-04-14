using System.Text.Json;
using AzureDevOpsMcp.Configuration;
using AzureDevOpsMcp.Services;
using AzureDevOpsMcp.Tools.Builds;
using NSubstitute;

namespace AzureDevOpsMcp.Tests.Tools.Builds;

public class QueueBuildToolTests
{
    private readonly AdoOptions _options = new()
    {
        ServerUrl = "https://dev.azure.com/testorg",
        PatToken = "test-pat",
        Project = "DefaultProject"
    };

    [Fact(DisplayName = "執行工具（指定專案）- 正常排入建置並回傳建置資訊")]
    public async Task Execute_QueuesBuild_WithExplicitProject()
    {
        // Arrange
        var service = Substitute.For<IAdoBuildsService>();
        var json = JsonDocument.Parse("""{"id":100,"status":"notStarted","definition":{"id":5}}""").RootElement;
        service.QueueBuildAsync("ExplicitProject", 5, null, null).Returns(json);

        // Act
        var result = await QueueBuildTool.Execute(service, _options, 5, "ExplicitProject");

        // Assert
        Assert.Contains("100", result);
        Assert.Contains("notStarted", result);
        await service.Received(1).QueueBuildAsync("ExplicitProject", 5, null, null);
    }

    [Fact(DisplayName = "執行工具並傳入 sourceBranch - 原樣傳遞來源分支至服務")]
    public async Task Execute_PassesSourceBranch_AsIs()
    {
        // Arrange
        var service = Substitute.For<IAdoBuildsService>();
        var json = JsonDocument.Parse("""{"id":101,"sourceBranch":"refs/heads/feature/my-branch"}""").RootElement;
        service.QueueBuildAsync("DefaultProject", 5, "refs/heads/feature/my-branch", null).Returns(json);

        // Act
        var result = await QueueBuildTool.Execute(service, _options, 5, sourceBranch: "refs/heads/feature/my-branch");

        // Assert
        Assert.Contains("feature/my-branch", result);
        await service.Received(1).QueueBuildAsync("DefaultProject", 5, "refs/heads/feature/my-branch", null);
    }

    [Fact(DisplayName = "執行工具（未提供專案名稱）- 自動使用預設專案")]
    public async Task Execute_UsesDefaultProject_WhenProjectIsNull()
    {
        // Arrange
        var service = Substitute.For<IAdoBuildsService>();
        var json = JsonDocument.Parse("""{"id":102}""").RootElement;
        service.QueueBuildAsync("DefaultProject", 3, null, null).Returns(json);

        // Act
        await QueueBuildTool.Execute(service, _options, 3);

        // Assert
        await service.Received(1).QueueBuildAsync("DefaultProject", 3, null, null);
    }

    [Fact(DisplayName = "執行工具（無專案設定）- 應拋出 ArgumentException")]
    public async Task Execute_ThrowsWhenNoProject()
    {
        // Arrange
        var service = Substitute.For<IAdoBuildsService>();
        var options = new AdoOptions { ServerUrl = "https://test", PatToken = "pat" };

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() =>
            QueueBuildTool.Execute(service, options, 1));
    }
}
