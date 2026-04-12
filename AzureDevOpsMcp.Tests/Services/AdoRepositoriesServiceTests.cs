using System.Net;
using AzureDevOpsMcp.Services;
using AzureDevOpsMcp.Tests.Helpers;
using NSubstitute;

namespace AzureDevOpsMcp.Tests.Services;

public class AdoRepositoriesServiceTests
{
    private (AdoRepositoriesService service, MockHttpMessageHandler handler) CreateService(string jsonResponse)
    {
        var handler = new MockHttpMessageHandler(jsonResponse);
        var httpClient = new HttpClient(handler) { BaseAddress = new Uri("https://dev.azure.com/testorg/") };
        var factory = Substitute.For<IHttpClientFactory>();
        factory.CreateClient("AzureDevOps").Returns(httpClient);
        return (new AdoRepositoriesService(factory), handler);
    }

    [Fact(DisplayName = "列出所有儲存庫 - 呼叫正確的 API 端點")]
    public async Task ListRepositoriesAsync_CallsCorrectEndpoint()
    {
        // Arrange
        var (service, handler) = CreateService("""{"value":[],"count":0}""");

        // Act
        await service.ListRepositoriesAsync("MyProject");

        // Assert
        Assert.Equal(HttpMethod.Get, handler.LastRequest!.Method);
        var url = handler.LastRequest.RequestUri!.ToString();
        Assert.Contains("MyProject/_apis/git/repositories", url);
        Assert.Contains("api-version=7.1", url);
    }

    [Fact(DisplayName = "列出所有儲存庫 - 正確回傳 JSON 資料")]
    public async Task ListRepositoriesAsync_ReturnsJsonElement()
    {
        // Arrange
        var (service, _) = CreateService("""{"value":[{"id":"repo1","name":"TestRepo"}],"count":1}""");

        // Act
        var result = await service.ListRepositoriesAsync("MyProject");

        // Assert
        Assert.Contains("TestRepo", result.ToString());
    }

    [Fact(DisplayName = "列出 PR 並套用篩選 - 呼叫正確端點並包含篩選參數")]
    public async Task ListPullRequestsAsync_CallsCorrectEndpointWithFilters()
    {
        // Arrange
        var (service, handler) = CreateService("""{"value":[],"count":0}""");

        // Act
        await service.ListPullRequestsAsync("repo1", "MyProject", status: "active", top: 5);

        // Assert
        var url = handler.LastRequest!.RequestUri!.ToString();
        Assert.Contains("MyProject/_apis/git/repositories/repo1/pullrequests", url);
        Assert.Contains("searchCriteria.status=active", url);
        Assert.Contains("$top=5", url);
    }

    [Fact(DisplayName = "列出分支並指定篩選 - URL 包含 heads 前綴")]
    public async Task ListBranchesAsync_IncludesHeadsFilter()
    {
        // Arrange
        var (service, handler) = CreateService("""{"value":[],"count":0}""");

        // Act
        await service.ListBranchesAsync("repo1", "MyProject", filter: "main");

        // Assert
        var url = handler.LastRequest!.RequestUri!.ToString();
        Assert.Contains("MyProject/_apis/git/repositories/repo1/refs", url);
        Assert.Contains("filter=heads/main", url);
    }

    [Fact(DisplayName = "取得提交記錄並指定分支與筆數 - 包含 searchCriteria 查詢參數")]
    public async Task GetCommitsAsync_IncludesSearchCriteria()
    {
        // Arrange
        var (service, handler) = CreateService("""{"value":[],"count":0}""");

        // Act
        await service.GetCommitsAsync("repo1", "MyProject", branch: "main", top: 10);

        // Assert
        var url = handler.LastRequest!.RequestUri!.ToString();
        Assert.Contains("MyProject/_apis/git/repositories/repo1/commits", url);
        Assert.Contains("searchCriteria.itemVersion.version=main", url);
        Assert.Contains("$top=10", url);
    }
}
