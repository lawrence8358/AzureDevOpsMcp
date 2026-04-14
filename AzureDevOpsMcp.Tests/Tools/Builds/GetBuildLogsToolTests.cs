using System.Text.Json;
using AzureDevOpsMcp.Configuration;
using AzureDevOpsMcp.Services;
using AzureDevOpsMcp.Tools.Builds;
using NSubstitute;

namespace AzureDevOpsMcp.Tests.Tools.Builds;

public class GetBuildLogsToolTests
{
    private readonly AdoOptions _options = new()
    {
        ServerUrl = "https://dev.azure.com/testorg",
        PatToken = "test-pat",
        Project = "DefaultProject"
    };

    [Fact(DisplayName = "執行工具（logId 為 null）- 取得所有日誌清單")]
    public async Task Execute_GetsAllLogs_WhenLogIdIsNull()
    {
        // Arrange
        var service = Substitute.For<IAdoBuildsService>();
        var json = JsonDocument.Parse("""{"count":3,"value":[{"id":1,"type":"Container"},{"id":2,"type":"Task"},{"id":3,"type":"Task"}]}""").RootElement;
        service.GetBuildLogsAsync("DefaultProject", 50, null).Returns(json);

        // Act
        var result = await GetBuildLogsTool.Execute(service, _options, 50);

        // Assert
        Assert.Contains("Container", result);
        await service.Received(1).GetBuildLogsAsync("DefaultProject", 50, null);
    }

    [Fact(DisplayName = "執行工具（指定 logId）- 取得特定日誌內容")]
    public async Task Execute_GetsSpecificLog_WhenLogIdProvided()
    {
        // Arrange
        var service = Substitute.For<IAdoBuildsService>();
        var json = JsonDocument.Parse("""{"id":2,"content":"##[section]Starting: Build step"}""").RootElement;
        service.GetBuildLogsAsync("DefaultProject", 50, 2).Returns(json);

        // Act
        var result = await GetBuildLogsTool.Execute(service, _options, 50, logId: 2);

        // Assert
        Assert.Contains("Build step", result);
        await service.Received(1).GetBuildLogsAsync("DefaultProject", 50, 2);
    }

    [Fact(DisplayName = "執行工具（未提供專案名稱）- 自動使用預設專案")]
    public async Task Execute_UsesDefaultProject_WhenProjectIsNull()
    {
        // Arrange
        var service = Substitute.For<IAdoBuildsService>();
        var json = JsonDocument.Parse("""{"count":0,"value":[]}""").RootElement;
        service.GetBuildLogsAsync("DefaultProject", 10, null).Returns(json);

        // Act
        await GetBuildLogsTool.Execute(service, _options, 10);

        // Assert
        await service.Received(1).GetBuildLogsAsync("DefaultProject", 10, null);
    }

    [Fact(DisplayName = "執行工具（無專案設定）- 應拋出 ArgumentException")]
    public async Task Execute_ThrowsWhenNoProject()
    {
        // Arrange
        var service = Substitute.For<IAdoBuildsService>();
        var options = new AdoOptions { ServerUrl = "https://test", PatToken = "pat" };

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() =>
            GetBuildLogsTool.Execute(service, options, 1));
    }
}
