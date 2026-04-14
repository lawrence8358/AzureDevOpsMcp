using System.Text.Json;
using AzureDevOpsMcp.Services;
using AzureDevOpsMcp.Tools.WorkItems;
using NSubstitute;

namespace AzureDevOpsMcp.Tests.Tools.WorkItems;

public class UpdateWorkItemToolTests
{
    [Fact(DisplayName = "執行工具（更新欄位）- 回傳包含更新後欄位的 JSON")]
    public async Task Execute_UpdatesFields()
    {
        // Arrange
        var service = Substitute.For<IAdoWorkItemsService>();
        var fields = new Dictionary<string, object>
        {
            ["System.State"] = "Active",
            ["System.AssignedTo"] = "alice@example.com"
        };
        var json = JsonDocument.Parse("""{"id":10,"fields":{"System.State":"Active","System.AssignedTo":"alice@example.com"}}""").RootElement;
        service.UpdateWorkItemAsync(10, Arg.Any<Dictionary<string, object>>()).Returns(json);

        // Act
        var result = await UpdateWorkItemTool.Execute(service, 10, fields);

        // Assert
        Assert.Contains("Active", result);
        await service.Received(1).UpdateWorkItemAsync(10, Arg.Any<Dictionary<string, object>>());
    }

    [Fact(DisplayName = "執行工具（驗證輸出 JSON）- 輸出應包含 id 欄位")]
    public async Task Execute_OutputContainsIdField()
    {
        // Arrange
        var service = Substitute.For<IAdoWorkItemsService>();
        var fields = new Dictionary<string, object> { ["System.Title"] = "Updated Title" };
        var json = JsonDocument.Parse("""{"id":15,"rev":3,"fields":{"System.Title":"Updated Title"}}""").RootElement;
        service.UpdateWorkItemAsync(15, Arg.Any<Dictionary<string, object>>()).Returns(json);

        // Act
        var result = await UpdateWorkItemTool.Execute(service, 15, fields);

        // Assert
        var doc = JsonDocument.Parse(result);
        Assert.True(doc.RootElement.TryGetProperty("id", out _));
        Assert.True(doc.RootElement.TryGetProperty("rev", out _));
    }

    [Fact(DisplayName = "執行工具並傳入多個欄位 - 正確傳遞 fields 字典至服務")]
    public async Task Execute_PassesFieldsDictionary()
    {
        // Arrange
        var service = Substitute.For<IAdoWorkItemsService>();
        var fields = new Dictionary<string, object>
        {
            ["System.State"] = "Resolved",
            ["System.Reason"] = "Fixed",
            ["Microsoft.VSTS.Common.Priority"] = 1
        };
        var json = JsonDocument.Parse("""{"id":20}""").RootElement;
        service.UpdateWorkItemAsync(20, Arg.Is<Dictionary<string, object>>(d =>
            d.ContainsKey("System.State") && d.ContainsKey("System.Reason"))).Returns(json);

        // Act
        await UpdateWorkItemTool.Execute(service, 20, fields);

        // Assert
        await service.Received(1).UpdateWorkItemAsync(20,
            Arg.Is<Dictionary<string, object>>(d => d.Count == 3 && d.ContainsKey("Microsoft.VSTS.Common.Priority")));
    }
}
