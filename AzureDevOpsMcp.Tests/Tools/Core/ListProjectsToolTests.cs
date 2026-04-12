using System.Text.Json;
using AzureDevOpsMcp.Services;
using AzureDevOpsMcp.Tools.Core;
using NSubstitute;

namespace AzureDevOpsMcp.Tests.Tools.Core;

public class ListProjectsToolTests
{
    [Fact(DisplayName = "執行工具 - 呼叫服務並回傳結果")]
    public async Task Execute_CallsServiceAndReturnsResult()
    {
        // Arrange
        var service = Substitute.For<IAdoCoreService>();
        var json = JsonDocument.Parse("""{"value":[{"name":"MyProject"}]}""").RootElement;
        service.ListProjectsAsync(null, null, null).Returns(json);

        // Act
        var result = await ListProjectsTool.Execute(service);

        // Assert
        Assert.Contains("MyProject", result);
        await service.Received(1).ListProjectsAsync(null, null, null);
    }

    [Fact(DisplayName = "執行工具並傳入篩選條件 - 正確傳遞篩選參數至服務")]
    public async Task Execute_PassesFilters()
    {
        // Arrange
        var service = Substitute.For<IAdoCoreService>();
        var json = JsonDocument.Parse("""{"value":[]}""").RootElement;
        service.ListProjectsAsync("wellFormed", 10, 5).Returns(json);

        // Act
        await ListProjectsTool.Execute(service, "wellFormed", 10, 5);

        // Assert
        await service.Received(1).ListProjectsAsync("wellFormed", 10, 5);
    }
}
