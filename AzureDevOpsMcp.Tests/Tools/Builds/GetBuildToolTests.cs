using System.Text.Json;
using AzureDevOpsMcp.Configuration;
using AzureDevOpsMcp.Services;
using AzureDevOpsMcp.Tools.Builds;
using NSubstitute;

namespace AzureDevOpsMcp.Tests.Tools.Builds;

public class GetBuildToolTests
{
    private readonly AdoOptions _options = new()
    {
        ServerUrl = "https://dev.azure.com/testorg",
        PatToken = "test-pat",
        Project = "DefaultProject"
    };

    [Fact(DisplayName = "執行工具（指定專案）- 使用指定的專案名稱取得建置並回傳含 id 的 JSON")]
    public async Task Execute_ReturnsCorrectBuildData()
    {
        // Arrange
        var service = Substitute.For<IAdoBuildsService>();
        var json = JsonDocument.Parse("""{"id":42,"buildNumber":"20260414.1","status":"completed","result":"succeeded"}""").RootElement;
        service.GetBuildAsync("ExplicitProject", 42).Returns(json);

        // Act
        var result = await GetBuildTool.Execute(service, _options, 42, "ExplicitProject");

        // Assert
        Assert.Contains("42", result);
        Assert.Contains("succeeded", result);
        await service.Received(1).GetBuildAsync("ExplicitProject", 42);
    }

    [Fact(DisplayName = "執行工具（未提供專案名稱）- 自動使用預設專案")]
    public async Task Execute_UsesDefaultProject_WhenProjectIsNull()
    {
        // Arrange
        var service = Substitute.For<IAdoBuildsService>();
        var json = JsonDocument.Parse("""{"id":1,"buildNumber":"20260414.1"}""").RootElement;
        service.GetBuildAsync("DefaultProject", 1).Returns(json);

        // Act
        var result = await GetBuildTool.Execute(service, _options, 1);

        // Assert
        Assert.Contains("20260414.1", result);
        await service.Received(1).GetBuildAsync("DefaultProject", 1);
    }

    [Fact(DisplayName = "執行工具（無專案設定）- 應拋出 ArgumentException")]
    public async Task Execute_ThrowsWhenNoProject()
    {
        // Arrange
        var service = Substitute.For<IAdoBuildsService>();
        var options = new AdoOptions { ServerUrl = "https://test", PatToken = "pat" };

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() =>
            GetBuildTool.Execute(service, options, 1));
    }
}
