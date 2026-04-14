using System.Text.Json;
using AzureDevOpsMcp.Services;
using AzureDevOpsMcp.Tools.WorkItems;
using NSubstitute;

namespace AzureDevOpsMcp.Tests.Tools.WorkItems;

public class AddLinkToolTests
{
    [Fact(DisplayName = "執行工具（建立連結）- 成功回傳含連結資訊的 JSON")]
    public async Task Execute_AddsLink()
    {
        // Arrange
        var service = Substitute.For<IAdoWorkItemsService>();
        var json = JsonDocument.Parse("""{"id":10,"relations":[{"rel":"System.LinkTypes.Related","url":"https://example.com/work/20"}]}""").RootElement;
        service.AddLinkAsync(10, 20, "System.LinkTypes.Related", null).Returns(json);

        // Act
        var result = await AddLinkTool.Execute(service, 10, 20, "System.LinkTypes.Related");

        // Assert
        Assert.Contains("System.LinkTypes.Related", result);
        await service.Received(1).AddLinkAsync(10, 20, "System.LinkTypes.Related", null);
    }

    [Fact(DisplayName = "執行工具並傳入不同的 linkType - 正確傳遞 linkType 參數至服務")]
    public async Task Execute_PassesLinkType()
    {
        // Arrange
        var service = Substitute.For<IAdoWorkItemsService>();
        var json = JsonDocument.Parse("""{"id":11,"relations":[{"rel":"System.LinkTypes.Hierarchy-Forward"}]}""").RootElement;
        service.AddLinkAsync(11, 22, "System.LinkTypes.Hierarchy-Forward", null).Returns(json);

        // Act
        var result = await AddLinkTool.Execute(service, 11, 22, "System.LinkTypes.Hierarchy-Forward");

        // Assert
        Assert.Contains("Hierarchy-Forward", result);
        await service.Received(1).AddLinkAsync(11, 22, "System.LinkTypes.Hierarchy-Forward", null);
    }

    [Fact(DisplayName = "執行工具並傳入 comment - 正確傳遞說明至服務")]
    public async Task Execute_PassesComment()
    {
        // Arrange
        var service = Substitute.For<IAdoWorkItemsService>();
        var json = JsonDocument.Parse("""{"id":12}""").RootElement;
        service.AddLinkAsync(12, 30, "System.LinkTypes.Related", "This is related").Returns(json);

        // Act
        await AddLinkTool.Execute(service, 12, 30, "System.LinkTypes.Related", "This is related");

        // Assert
        await service.Received(1).AddLinkAsync(12, 30, "System.LinkTypes.Related", "This is related");
    }
}
