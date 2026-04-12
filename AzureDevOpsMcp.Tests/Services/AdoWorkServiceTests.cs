using System.Net;
using AzureDevOpsMcp.Services;
using AzureDevOpsMcp.Tests.Helpers;
using NSubstitute;

namespace AzureDevOpsMcp.Tests.Services;

public class AdoWorkServiceTests
{
    private (AdoWorkService service, MockHttpMessageHandler handler) CreateService(string jsonResponse)
    {
        var handler = new MockHttpMessageHandler(jsonResponse);
        var httpClient = new HttpClient(handler) { BaseAddress = new Uri("https://dev.azure.com/testorg/") };
        var factory = Substitute.For<IHttpClientFactory>();
        factory.CreateClient("AzureDevOps").Returns(httpClient);
        return (new AdoWorkService(factory), handler);
    }

    [Fact(DisplayName = "列出迭代 - 呼叫正確的 API 端點")]
    public async Task ListIterationsAsync_CallsCorrectEndpoint()
    {
        // Arrange
        var (service, handler) = CreateService("""{"value":[],"count":0}""");

        // Act
        await service.ListIterationsAsync("MyProject");

        // Assert
        Assert.Equal(HttpMethod.Get, handler.LastRequest!.Method);
        var url = handler.LastRequest.RequestUri!.ToString();
        Assert.Contains("MyProject/_apis/work/teamsettings/iterations", url);
        Assert.Contains("api-version=7.1", url);
    }

    [Fact(DisplayName = "列出迭代並指定團隊與時間框架 - 包含對應查詢參數")]
    public async Task ListIterationsAsync_WithTeamAndTimeframe_IncludesParams()
    {
        // Arrange
        var (service, handler) = CreateService("""{"value":[],"count":0}""");

        // Act
        await service.ListIterationsAsync("MyProject", team: "TeamA", timeframe: "current");

        // Assert
        var url = handler.LastRequest!.RequestUri!.ToString();
        Assert.Contains("MyProject/TeamA/_apis/work/teamsettings/iterations", url);
        Assert.Contains("$timeframe=current", url);
    }

    [Fact(DisplayName = "列出待辦清單 - 呼叫正確端點並帶入團隊名稱")]
    public async Task ListBacklogsAsync_CallsCorrectEndpoint()
    {
        // Arrange
        var (service, handler) = CreateService("""{"value":[],"count":0}""");

        // Act
        await service.ListBacklogsAsync("MyProject", team: "TeamB");

        // Assert
        var url = handler.LastRequest!.RequestUri!.ToString();
        Assert.Contains("MyProject/TeamB/_apis/work/backlogs", url);
        Assert.Contains("api-version=7.1", url);
    }

    [Fact(DisplayName = "取得迭代工作項目 - 呼叫正確端點並包含迭代 ID")]
    public async Task GetIterationWorkItemsAsync_CallsCorrectEndpoint()
    {
        // Arrange
        var (service, handler) = CreateService("""{"workItemRelations":[],"_links":{}}""");

        // Act
        await service.GetIterationWorkItemsAsync("MyProject", "iteration-id-1", team: "TeamA");

        // Assert
        var url = handler.LastRequest!.RequestUri!.ToString();
        Assert.Contains("MyProject/TeamA/_apis/work/teamsettings/iterations/iteration-id-1/workitems", url);
    }
}
