using System.Text.Json;
using AzureDevOpsMcp.Configuration;
using AzureDevOpsMcp.Services;
using AzureDevOpsMcp.Tools.Builds;
using NSubstitute;

namespace AzureDevOpsMcp.Tests.Tools.Builds;

public class ListBuildsToolTests
{
    private readonly AdoOptions _options = new()
    {
        ServerUrl = "https://dev.azure.com/testorg",
        PatToken = "test-pat",
        Project = "DefaultProject"
    };

    [Fact(DisplayName = "執行工具（基本列表）- 回傳包含建置紀錄的 JSON")]
    public async Task Execute_ReturnsBuilds()
    {
        // Arrange
        var service = Substitute.For<IAdoBuildsService>();
        var json = JsonDocument.Parse("""{"value":[{"id":1,"buildNumber":"20260414.1","status":"completed"}]}""").RootElement;
        service.ListBuildsAsync("DefaultProject", null, null, null, null, null).Returns(json);

        // Act
        var result = await ListBuildsTool.Execute(service, _options);

        // Assert
        Assert.Contains("20260414.1", result);
        await service.Received(1).ListBuildsAsync("DefaultProject", null, null, null, null, null);
    }

    [Fact(DisplayName = "執行工具並傳入篩選參數 - 正確傳遞篩選條件至服務")]
    public async Task Execute_PassesFilters()
    {
        // Arrange
        var service = Substitute.For<IAdoBuildsService>();
        var json = JsonDocument.Parse("""{"value":[]}""").RootElement;
        service.ListBuildsAsync("DefaultProject", 5, "completed", "succeeded", "main", 10).Returns(json);

        // Act
        await ListBuildsTool.Execute(service, _options,
            definitionId: 5,
            statusFilter: "completed",
            resultFilter: "succeeded",
            branchName: "main",
            top: 10);

        // Assert
        await service.Received(1).ListBuildsAsync("DefaultProject", 5, "completed", "succeeded", "main", 10);
    }

    [Fact(DisplayName = "執行工具（指定專案）- 使用指定的專案名稱而非預設專案")]
    public async Task Execute_UsesProvidedProject_OverDefault()
    {
        // Arrange
        var service = Substitute.For<IAdoBuildsService>();
        var json = JsonDocument.Parse("""{"value":[{"id":2}]}""").RootElement;
        service.ListBuildsAsync("ExplicitProject", null, null, null, null, null).Returns(json);

        // Act
        var result = await ListBuildsTool.Execute(service, _options, project: "ExplicitProject");

        // Assert
        Assert.Contains("2", result);
        await service.Received(1).ListBuildsAsync("ExplicitProject", null, null, null, null, null);
    }

    [Fact(DisplayName = "執行工具（無專案設定）- 應拋出 ArgumentException")]
    public async Task Execute_ThrowsWhenNoProject()
    {
        // Arrange
        var service = Substitute.For<IAdoBuildsService>();
        var options = new AdoOptions { ServerUrl = "https://test", PatToken = "pat" };

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() =>
            ListBuildsTool.Execute(service, options));
    }
}
