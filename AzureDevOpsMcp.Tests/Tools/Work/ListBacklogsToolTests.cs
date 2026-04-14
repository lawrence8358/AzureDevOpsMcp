using System.Text.Json;
using AzureDevOpsMcp.Configuration;
using AzureDevOpsMcp.Services;
using AzureDevOpsMcp.Tools.Work;
using NSubstitute;

namespace AzureDevOpsMcp.Tests.Tools.Work;

public class ListBacklogsToolTests
{
    private readonly AdoOptions _options = new()
    {
        ServerUrl = "https://dev.azure.com/testorg",
        PatToken = "test-pat",
        Project = "DefaultProject"
    };

    [Fact(DisplayName = "執行工具（列出 backlogs）- 回傳包含 backlog 層級的 JSON")]
    public async Task Execute_ListsBacklogs()
    {
        // Arrange
        var service = Substitute.For<IAdoWorkService>();
        var json = JsonDocument.Parse("""{"value":[{"id":"Microsoft.EpicCategory","name":"Epics"},{"id":"Microsoft.FeatureCategory","name":"Features"},{"id":"Microsoft.RequirementCategory","name":"Stories"}]}""").RootElement;
        service.ListBacklogsAsync("DefaultProject", null).Returns(json);

        // Act
        var result = await ListBacklogsTool.Execute(service, _options);

        // Assert
        Assert.Contains("Epics", result);
        Assert.Contains("Features", result);
        await service.Received(1).ListBacklogsAsync("DefaultProject", null);
    }

    [Fact(DisplayName = "執行工具並傳入 team - 正確傳遞 team 參數至服務")]
    public async Task Execute_PassesTeamParam()
    {
        // Arrange
        var service = Substitute.For<IAdoWorkService>();
        var json = JsonDocument.Parse("""{"value":[{"name":"Stories"}]}""").RootElement;
        service.ListBacklogsAsync("DefaultProject", "Frontend Team").Returns(json);

        // Act
        await ListBacklogsTool.Execute(service, _options, team: "Frontend Team");

        // Assert
        await service.Received(1).ListBacklogsAsync("DefaultProject", "Frontend Team");
    }

    [Fact(DisplayName = "執行工具（未提供專案名稱）- 自動使用預設專案")]
    public async Task Execute_UsesDefaultProject_WhenProjectIsNull()
    {
        // Arrange
        var service = Substitute.For<IAdoWorkService>();
        var json = JsonDocument.Parse("""{"value":[]}""").RootElement;
        service.ListBacklogsAsync("DefaultProject", null).Returns(json);

        // Act
        await ListBacklogsTool.Execute(service, _options);

        // Assert
        await service.Received(1).ListBacklogsAsync("DefaultProject", null);
    }

    [Fact(DisplayName = "執行工具（無專案設定）- 應拋出 ArgumentException")]
    public async Task Execute_ThrowsWhenNoProject()
    {
        // Arrange
        var service = Substitute.For<IAdoWorkService>();
        var options = new AdoOptions { ServerUrl = "https://test", PatToken = "pat" };

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() =>
            ListBacklogsTool.Execute(service, options));
    }
}
