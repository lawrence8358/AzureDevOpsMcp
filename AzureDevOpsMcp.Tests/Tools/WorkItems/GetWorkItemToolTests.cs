using System.Text.Json;
using AzureDevOpsMcp.Services;
using AzureDevOpsMcp.Tools.WorkItems;
using NSubstitute;

namespace AzureDevOpsMcp.Tests.Tools.WorkItems;

public class GetWorkItemToolTests
{
    [Fact(DisplayName = "執行工具（指定 ID）- 呼叫服務並回傳工作項目")]
    public async Task Execute_CallsServiceWithIdAndReturnsResult()
    {
        // Arrange
        var service = Substitute.For<IAdoWorkItemsService>();
        var json = JsonDocument.Parse("""{"id":42,"fields":{"System.Title":"Test WI"}}""").RootElement;
        service.GetWorkItemAsync(42, null).Returns(json);

        // Act
        var result = await GetWorkItemTool.Execute(service, 42);

        // Assert
        Assert.Contains("Test WI", result);
        await service.Received(1).GetWorkItemAsync(42, null);
    }

    [Fact(DisplayName = "執行工具並傳入展開選項 - 正確傳遞 expand 參數至服務")]
    public async Task Execute_PassesExpandOption()
    {
        // Arrange
        var service = Substitute.For<IAdoWorkItemsService>();
        var json = JsonDocument.Parse("""{"id":42,"fields":{}}""").RootElement;
        service.GetWorkItemAsync(42, "all").Returns(json);

        // Act
        await GetWorkItemTool.Execute(service, 42, "all");

        // Assert
        await service.Received(1).GetWorkItemAsync(42, "all");
    }
}
