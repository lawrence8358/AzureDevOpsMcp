using System.Net;
using AzureDevOpsMcp.Services;
using AzureDevOpsMcp.Tests.Helpers;
using NSubstitute;

namespace AzureDevOpsMcp.Tests.Services;

public class AdoBuildsServiceTests
{
    private (AdoBuildsService service, MockHttpMessageHandler handler) CreateService(string jsonResponse)
    {
        var handler = new MockHttpMessageHandler(jsonResponse);
        var httpClient = new HttpClient(handler) { BaseAddress = new Uri("https://dev.azure.com/testorg/") };
        var factory = Substitute.For<IHttpClientFactory>();
        factory.CreateClient("AzureDevOps").Returns(httpClient);
        return (new AdoBuildsService(factory), handler);
    }

    [Fact(DisplayName = "列出建置定義 - 呼叫正確的 API 端點")]
    public async Task ListDefinitionsAsync_CallsCorrectEndpoint()
    {
        // Arrange
        var (service, handler) = CreateService("""{"value":[],"count":0}""");

        // Act
        await service.ListDefinitionsAsync("MyProject");

        // Assert
        Assert.Equal(HttpMethod.Get, handler.LastRequest!.Method);
        var url = handler.LastRequest.RequestUri!.ToString();
        Assert.Contains("MyProject/_apis/build/definitions", url);
        Assert.Contains("api-version=7.1", url);
    }

    [Fact(DisplayName = "列出建置定義並套用篩選 - 包含對應查詢字串參數")]
    public async Task ListDefinitionsAsync_WithFilters_IncludesQueryParams()
    {
        // Arrange
        var (service, handler) = CreateService("""{"value":[],"count":0}""");

        // Act
        await service.ListDefinitionsAsync("MyProject", name: "CI-Pipeline", top: 5);

        // Assert
        var url = handler.LastRequest!.RequestUri!.ToString();
        Assert.Contains("name=CI-Pipeline", url);
        Assert.Contains("$top=5", url);
    }

    [Fact(DisplayName = "列出建置記錄並套用篩選 - 呼叫正確端點並包含篩選參數")]
    public async Task ListBuildsAsync_CallsCorrectEndpointWithFilters()
    {
        // Arrange
        var (service, handler) = CreateService("""{"value":[],"count":0}""");

        // Act
        await service.ListBuildsAsync("MyProject", definitionId: 1, statusFilter: "completed", top: 10);

        // Assert
        var url = handler.LastRequest!.RequestUri!.ToString();
        Assert.Contains("MyProject/_apis/build/builds", url);
        Assert.Contains("definitions=1", url);
        Assert.Contains("statusFilter=completed", url);
        Assert.Contains("$top=10", url);
    }

    [Fact(DisplayName = "佇列建置 - 使用 POST 方法")]
    public async Task QueueBuildAsync_UsesPostMethod()
    {
        // Arrange
        var (service, handler) = CreateService("""{"id":100,"buildNumber":"20260412.1"}""");

        // Act
        await service.QueueBuildAsync("MyProject", 1, sourceBranch: "main");

        // Assert
        Assert.Equal(HttpMethod.Post, handler.LastRequest!.Method);
        var url = handler.LastRequest.RequestUri!.ToString();
        Assert.Contains("MyProject/_apis/build/builds", url);
    }

    [Fact(DisplayName = "取得指定建置記錄 - 呼叫正確的 API 端點")]
    public async Task GetBuildAsync_CallsCorrectEndpoint()
    {
        // Arrange
        var (service, handler) = CreateService("""{"id":99,"buildNumber":"20260412.1"}""");

        // Act
        await service.GetBuildAsync("MyProject", 99);

        // Assert
        var url = handler.LastRequest!.RequestUri!.ToString();
        Assert.Contains("MyProject/_apis/build/builds/99", url);
        Assert.Contains("api-version=7.1", url);
    }

    [Fact(DisplayName = "取得建置日誌清單（不指定 logId）- 呼叫日誌清單端點")]
    public async Task GetBuildLogsAsync_NoLogId_CallsListEndpoint()
    {
        // Arrange
        var (service, handler) = CreateService("""{"value":[{"id":1},{"id":2}],"count":2}""");

        // Act
        await service.GetBuildLogsAsync("MyProject", buildId: 99);

        // Assert
        Assert.Equal(HttpMethod.Get, handler.LastRequest!.Method);
        var url = handler.LastRequest.RequestUri!.ToString();
        Assert.Contains("MyProject/_apis/build/builds/99/logs", url);
        Assert.Contains("api-version=7.1", url);
    }

    [Fact(DisplayName = "取得建置日誌（指定 logId）- 呼叫單一日誌端點並包裝回應")]
    public async Task GetBuildLogsAsync_WithLogId_CallsSingleLogEndpoint()
    {
        // Arrange
        var (service, handler) = CreateService("""Build log text content""");

        // Act
        await service.GetBuildLogsAsync("MyProject", buildId: 99, logId: 1);

        // Assert
        var url = handler.LastRequest!.RequestUri!.ToString();
        Assert.Contains("MyProject/_apis/build/builds/99/logs/1", url);
    }
}
