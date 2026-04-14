using System.Text.Json;
using AzureDevOpsMcp.Services;
using AzureDevOpsMcp.Tools.WorkItems;
using NSubstitute;

namespace AzureDevOpsMcp.Tests.Tools.WorkItems;

public class GetUpdatesToolTests
{
    [Fact(DisplayName = "執行工具（取得更新歷史）- 回傳包含修改紀錄的 JSON")]
    public async Task Execute_ReturnsUpdates()
    {
        // Arrange
        var service = Substitute.For<IAdoWorkItemsService>();
        var json = JsonDocument.Parse("""{"count":2,"value":[{"id":1,"rev":2,"fields":{"System.State":{"oldValue":"New","newValue":"Active"}}},{"id":2,"rev":3}]}""").RootElement;
        service.GetUpdatesAsync(42, null).Returns(json);

        // Act
        var result = await GetUpdatesTool.Execute(service, 42);

        // Assert
        Assert.Contains("Active", result);
        await service.Received(1).GetUpdatesAsync(42, null);
    }

    [Fact(DisplayName = "執行工具並傳入 top 參數 - 正確傳遞 top 至服務")]
    public async Task Execute_PassesTopParam()
    {
        // Arrange
        var service = Substitute.For<IAdoWorkItemsService>();
        var json = JsonDocument.Parse("""{"count":1,"value":[{"id":1,"rev":1}]}""").RootElement;
        service.GetUpdatesAsync(42, 3).Returns(json);

        // Act
        await GetUpdatesTool.Execute(service, 42, top: 3);

        // Assert
        await service.Received(1).GetUpdatesAsync(42, 3);
    }

    [Fact(DisplayName = "執行工具（不同工作項目 ID）- 正確傳遞 id 至服務")]
    public async Task Execute_PassesCorrectWorkItemId()
    {
        // Arrange
        var service = Substitute.For<IAdoWorkItemsService>();
        var json = JsonDocument.Parse("""{"count":1,"value":[{"id":5,"rev":10}]}""").RootElement;
        service.GetUpdatesAsync(999, null).Returns(json);

        // Act
        var result = await GetUpdatesTool.Execute(service, 999);

        // Assert
        Assert.Contains("10", result);
        await service.Received(1).GetUpdatesAsync(999, null);
    }
}
