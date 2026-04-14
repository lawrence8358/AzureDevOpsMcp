using System.Text.Json;
using AzureDevOpsMcp.Services;
using AzureDevOpsMcp.Tools.WorkItems;
using NSubstitute;

namespace AzureDevOpsMcp.Tests.Tools.WorkItems;

public class DeleteWorkItemToolTests
{
    [Fact(DisplayName = "執行工具（軟刪除）- 移至回收桶並回傳 JSON")]
    public async Task Execute_SoftDeletes()
    {
        // Arrange
        var service = Substitute.For<IAdoWorkItemsService>();
        var json = JsonDocument.Parse("""{"id":42,"isDeleted":true}""").RootElement;
        service.DeleteWorkItemAsync(42, false).Returns(json);

        // Act
        var result = await DeleteWorkItemTool.Execute(service, 42);

        // Assert
        Assert.Contains("42", result);
        await service.Received(1).DeleteWorkItemAsync(42, false);
    }

    [Fact(DisplayName = "執行工具（永久刪除，回傳 permanent=true）- 回傳特殊永久刪除訊息")]
    public async Task Execute_PermanentDeletes_ReturnsSpecialMessage()
    {
        // Arrange
        var service = Substitute.For<IAdoWorkItemsService>();
        var json = JsonDocument.Parse("""{"permanent":true}""").RootElement;
        service.DeleteWorkItemAsync(99, true).Returns(json);

        // Act
        var result = await DeleteWorkItemTool.Execute(service, 99, destroy: true);

        // Assert
        Assert.Contains("99", result);
        Assert.Contains("permanently deleted", result);
        await service.Received(1).DeleteWorkItemAsync(99, true);
    }

    [Fact(DisplayName = "執行工具（永久刪除，回傳無 permanent 屬性）- 回傳原始 JSON")]
    public async Task Execute_PermanentDeletes_WhenNoPermanentProperty_ReturnsJson()
    {
        // Arrange
        var service = Substitute.For<IAdoWorkItemsService>();
        var json = JsonDocument.Parse("""{"id":77}""").RootElement;
        service.DeleteWorkItemAsync(77, true).Returns(json);

        // Act
        var result = await DeleteWorkItemTool.Execute(service, 77, destroy: true);

        // Assert
        Assert.Contains("77", result);
        Assert.DoesNotContain("permanently deleted", result);
    }
}
