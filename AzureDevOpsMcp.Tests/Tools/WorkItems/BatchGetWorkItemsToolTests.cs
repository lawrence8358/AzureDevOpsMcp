using System.Text.Json;
using AzureDevOpsMcp.Services;
using AzureDevOpsMcp.Tools.WorkItems;
using NSubstitute;

namespace AzureDevOpsMcp.Tests.Tools.WorkItems;

public class BatchGetWorkItemsToolTests
{
    [Fact(DisplayName = "執行工具（批次取得）- 回傳包含所有工作項目的 JSON")]
    public async Task Execute_ReturnsBatchWorkItems()
    {
        // Arrange
        var service = Substitute.For<IAdoWorkItemsService>();
        var json = JsonDocument.Parse("""{"count":2,"value":[{"id":1,"fields":{"System.Title":"Task A"}},{"id":2,"fields":{"System.Title":"Bug B"}}]}""").RootElement;
        service.BatchGetWorkItemsAsync(Arg.Is<int[]>(ids => ids.Length == 2), null).Returns(json);

        // Act
        var result = await BatchGetWorkItemsTool.Execute(service, [1, 2]);

        // Assert
        Assert.Contains("Task A", result);
        Assert.Contains("Bug B", result);
        await service.Received(1).BatchGetWorkItemsAsync(Arg.Is<int[]>(ids => ids[0] == 1 && ids[1] == 2), null);
    }

    [Fact(DisplayName = "執行工具並傳入 expand - 正確傳遞 expand 參數至服務")]
    public async Task Execute_PassesExpandParam()
    {
        // Arrange
        var service = Substitute.For<IAdoWorkItemsService>();
        var json = JsonDocument.Parse("""{"count":1,"value":[{"id":5}]}""").RootElement;
        service.BatchGetWorkItemsAsync(Arg.Any<int[]>(), "all").Returns(json);

        // Act
        await BatchGetWorkItemsTool.Execute(service, [5], expand: "all");

        // Assert
        await service.Received(1).BatchGetWorkItemsAsync(Arg.Any<int[]>(), "all");
    }

    [Fact(DisplayName = "執行工具並傳入 ids 陣列 - 正確傳遞 ids 至服務")]
    public async Task Execute_PassesIdsArray()
    {
        // Arrange
        var service = Substitute.For<IAdoWorkItemsService>();
        var json = JsonDocument.Parse("""{"count":3,"value":[{"id":10},{"id":20},{"id":30}]}""").RootElement;
        service.BatchGetWorkItemsAsync(
            Arg.Is<int[]>(ids => ids.Length == 3 && ids[0] == 10 && ids[1] == 20 && ids[2] == 30),
            null).Returns(json);

        // Act
        var result = await BatchGetWorkItemsTool.Execute(service, [10, 20, 30]);

        // Assert
        Assert.Contains("10", result);
        await service.Received(1).BatchGetWorkItemsAsync(
            Arg.Is<int[]>(ids => ids.Length == 3),
            null);
    }
}
