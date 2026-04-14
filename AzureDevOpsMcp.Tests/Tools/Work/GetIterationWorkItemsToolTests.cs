using System.Text.Json;
using AzureDevOpsMcp.Configuration;
using AzureDevOpsMcp.Services;
using AzureDevOpsMcp.Tools.Work;
using NSubstitute;

namespace AzureDevOpsMcp.Tests.Tools.Work;

public class GetIterationWorkItemsToolTests
{
    private readonly AdoOptions _options = new()
    {
        ServerUrl = "https://dev.azure.com/testorg",
        PatToken = "test-pat",
        Project = "DefaultProject"
    };

    [Fact(DisplayName = "執行工具（取得迭代工作項目）- 回傳包含工作項目的 JSON")]
    public async Task Execute_ReturnsIterationWorkItems()
    {
        // Arrange
        var service = Substitute.For<IAdoWorkService>();
        var json = JsonDocument.Parse("""{"workItemRelations":[{"rel":null,"target":{"id":1}},{"rel":null,"target":{"id":2}}]}""").RootElement;
        service.GetIterationWorkItemsAsync("DefaultProject", "sprint-guid-123", null).Returns(json);

        // Act
        var result = await GetIterationWorkItemsTool.Execute(service, _options, "sprint-guid-123");

        // Assert
        Assert.Contains("workItemRelations", result);
        await service.Received(1).GetIterationWorkItemsAsync("DefaultProject", "sprint-guid-123", null);
    }

    [Fact(DisplayName = "執行工具並傳入 team - 正確傳遞 team 參數至服務")]
    public async Task Execute_PassesTeamParam()
    {
        // Arrange
        var service = Substitute.For<IAdoWorkService>();
        var json = JsonDocument.Parse("""{"workItemRelations":[]}""").RootElement;
        service.GetIterationWorkItemsAsync("DefaultProject", "iter-id", "Backend Team").Returns(json);

        // Act
        await GetIterationWorkItemsTool.Execute(service, _options, "iter-id", team: "Backend Team");

        // Assert
        await service.Received(1).GetIterationWorkItemsAsync("DefaultProject", "iter-id", "Backend Team");
    }

    [Fact(DisplayName = "執行工具（未提供專案名稱）- 自動使用預設專案")]
    public async Task Execute_UsesDefaultProject_WhenProjectIsNull()
    {
        // Arrange
        var service = Substitute.For<IAdoWorkService>();
        var json = JsonDocument.Parse("""{"workItemRelations":[]}""").RootElement;
        service.GetIterationWorkItemsAsync("DefaultProject", "iter-abc", null).Returns(json);

        // Act
        await GetIterationWorkItemsTool.Execute(service, _options, "iter-abc");

        // Assert
        await service.Received(1).GetIterationWorkItemsAsync("DefaultProject", "iter-abc", null);
    }

    [Fact(DisplayName = "執行工具（無專案設定）- 應拋出 ArgumentException")]
    public async Task Execute_ThrowsWhenNoProject()
    {
        // Arrange
        var service = Substitute.For<IAdoWorkService>();
        var options = new AdoOptions { ServerUrl = "https://test", PatToken = "pat" };

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() =>
            GetIterationWorkItemsTool.Execute(service, options, "iter-id"));
    }
}
