using System.Net;
using AzureDevOpsMcp.Configuration;
using AzureDevOpsMcp.Services;
using AzureDevOpsMcp.Tests.Helpers;
using NSubstitute;

namespace AzureDevOpsMcp.Tests.Services;

public class AdoCoreServiceTests
{
    private readonly AdoOptions _options = new()
    {
        ServerUrl = "https://dev.azure.com/testorg",
        PatToken = "test-pat"
    };

    private (AdoCoreService service, MockHttpMessageHandler handler) CreateService(string jsonResponse)
    {
        var handler = new MockHttpMessageHandler(jsonResponse);
        var httpClient = new HttpClient(handler) { BaseAddress = new Uri(_options.ServerUrl.TrimEnd('/') + "/") };
        var factory = Substitute.For<IHttpClientFactory>();
        factory.CreateClient("AzureDevOps").Returns(httpClient);
        return (new AdoCoreService(factory), handler);
    }

    [Fact(DisplayName = "列出所有專案 - 呼叫正確的 API 端點")]
    public async Task ListProjectsAsync_CallsCorrectEndpoint()
    {
        // Arrange
        var (service, handler) = CreateService("""{"value":[],"count":0}""");

        // Act
        await service.ListProjectsAsync();

        // Assert
        Assert.NotNull(handler.LastRequest);
        Assert.Equal(HttpMethod.Get, handler.LastRequest!.Method);
        Assert.Contains("_apis/projects", handler.LastRequest.RequestUri!.ToString());
        Assert.Contains("api-version=7.1", handler.LastRequest.RequestUri.ToString());
    }

    [Fact(DisplayName = "列出所有專案並套用篩選 - 包含對應查詢字串參數")]
    public async Task ListProjectsAsync_WithFilters_IncludesQueryParams()
    {
        // Arrange
        var (service, handler) = CreateService("""{"value":[],"count":0}""");

        // Act
        await service.ListProjectsAsync(stateFilter: "wellFormed", top: 10, skip: 5);

        // Assert
        var url = handler.LastRequest!.RequestUri!.ToString();
        Assert.Contains("stateFilter=wellFormed", url);
        Assert.Contains("$top=10", url);
        Assert.Contains("$skip=5", url);
    }

    [Fact(DisplayName = "列出所有專案 - 正確回傳 JSON 資料")]
    public async Task ListProjectsAsync_ReturnsJsonElement()
    {
        // Arrange
        var (service, _) = CreateService("""{"value":[{"id":"1","name":"TestProject"}],"count":1}""");

        // Act
        var result = await service.ListProjectsAsync();

        // Assert
        Assert.Contains("TestProject", result.ToString());
    }
}
