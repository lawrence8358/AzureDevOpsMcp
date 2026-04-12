using System.Text.Json;
using AzureDevOpsMcp.Configuration;
using AzureDevOpsMcp.Services;
using AzureDevOpsMcp.Tools.Work;
using NSubstitute;

namespace AzureDevOpsMcp.Tests.Tools.Work;

public class ListIterationsToolTests
{
    private readonly AdoOptions _options = new()
    {
        ServerUrl = "https://dev.azure.com/testorg",
        PatToken = "test-pat",
        Project = "DefaultProject"
    };

    [Fact(DisplayName = "執行工具（指定專案）- 使用指定的專案名稱列出迭代")]
    public async Task Execute_UsesProvidedProject()
    {
        // Arrange
        var service = Substitute.For<IAdoWorkService>();
        var json = JsonDocument.Parse("""{"value":[{"id":"iter1","name":"Sprint 1"}]}""").RootElement;
        service.ListIterationsAsync("ExplicitProject", null, null).Returns(json);

        // Act
        var result = await ListIterationsTool.Execute(service, _options, "ExplicitProject");

        // Assert
        Assert.Contains("Sprint 1", result);
        await service.Received(1).ListIterationsAsync("ExplicitProject", null, null);
    }

    [Fact(DisplayName = "執行工具（未提供專案名稱）- 自動使用預設專案")]
    public async Task Execute_UsesDefaultProject_WhenProjectIsNull()
    {
        // Arrange
        var service = Substitute.For<IAdoWorkService>();
        var json = JsonDocument.Parse("""{"value":[]}""").RootElement;
        service.ListIterationsAsync("DefaultProject", null, null).Returns(json);

        // Act
        await ListIterationsTool.Execute(service, _options);

        // Assert
        await service.Received(1).ListIterationsAsync("DefaultProject", null, null);
    }

    [Fact(DisplayName = "執行工具（無專案設定）- 應拋出 ArgumentException")]
    public async Task Execute_ThrowsWhenNoProject()
    {
        // Arrange
        var service = Substitute.For<IAdoWorkService>();
        var options = new AdoOptions { ServerUrl = "https://test", PatToken = "pat" };

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() =>
            ListIterationsTool.Execute(service, options));
    }

    [Fact(DisplayName = "執行工具並傳入團隊與時間框架 - 正確傳遞篩選參數至服務")]
    public async Task Execute_PassesTeamAndTimeframe()
    {
        // Arrange
        var service = Substitute.For<IAdoWorkService>();
        var json = JsonDocument.Parse("""{"value":[]}""").RootElement;
        service.ListIterationsAsync("DefaultProject", "TeamA", "current").Returns(json);

        // Act
        await ListIterationsTool.Execute(service, _options, team: "TeamA", timeframe: "current");

        // Assert
        await service.Received(1).ListIterationsAsync("DefaultProject", "TeamA", "current");
    }
}
