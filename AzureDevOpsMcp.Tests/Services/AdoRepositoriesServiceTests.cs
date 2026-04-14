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

    [Fact(DisplayName = "取得提交記錄（傳入 refs/heads/ 前綴）- 自動剝除前綴後送出請求")]
    public async Task GetCommitsAsync_StripRefsHeadsPrefix_FromBranch()
    {
        // Arrange
        var (service, handler) = CreateService("""{"value":[],"count":0}""");

        // Act
        await service.GetCommitsAsync("repo1", "MyProject", branch: "refs/heads/develop");

        // Assert
        var url = handler.LastRequest!.RequestUri!.ToString();
        Assert.Contains("searchCriteria.itemVersion.version=develop", url);
        Assert.DoesNotContain("refs", url);
    }

    [Fact(DisplayName = "取得檔案內容 - 使用 includeContent=true 並回傳 JSON")]
    public async Task GetItemAsync_File_ReturnsContent()
    {
        // Arrange
        var (service, handler) = CreateService("""{"path":"/src/Program.cs","content":"using System;"}""");

        // Act
        var result = await service.GetItemAsync("repo1", "/src/Program.cs", "MyProject");

        // Assert
        var url = handler.LastRequest!.RequestUri!.ToString();
        Assert.Contains("items", url);
        Assert.Contains("includeContent=true", url);
        Assert.Contains("Program.cs", result.ToString());
    }

    [Fact(DisplayName = "取得檔案內容（傳入 refs/heads/ 前綴）- 自動剝除前綴後送出請求")]
    public async Task GetItemAsync_StripRefsHeadsPrefix_FromBranch()
    {
        // Arrange
        var (service, handler) = CreateService("""{"path":"/src/Program.cs","content":""}""");

        // Act
        await service.GetItemAsync("repo1", "/src/Program.cs", "MyProject", branch: "refs/heads/main");

        // Assert
        var url = handler.LastRequest!.RequestUri!.ToString();
        Assert.Contains("versionDescriptor.version=main", url);
        Assert.DoesNotContain("refs", url);
    }

    [Fact(DisplayName = "取得目錄清單（500 Tree 錯誤）- 自動降級為 scopePath + recursionLevel=OneLevel")]
    public async Task GetItemAsync_Directory_FallsBackToRecursionLevel_On500()
    {
        // Arrange
        var treeErrorBody = """{"$id":"1","typeName":"Microsoft.TeamFoundation.Git.Server.GitUnexpectedObjectTypeException, Microsoft.TeamFoundation.Git.Server","typeKey":"GitUnexpectedObjectTypeException","message":"Expected a Blob, but objectId abc resolved to a Tree","errorCode":0,"eventId":3000}""";
        var dirListingBody = """{"value":[{"path":"/src","isFolder":true}],"count":1}""";

        int callCount = 0;
        var handler = new MockHttpMessageHandler(req =>
        {
            callCount++;
            if (callCount == 1)
                return new System.Net.Http.HttpResponseMessage(System.Net.HttpStatusCode.InternalServerError)
                {
                    Content = new System.Net.Http.StringContent(treeErrorBody, System.Text.Encoding.UTF8, "application/json")
                };
            return new System.Net.Http.HttpResponseMessage(System.Net.HttpStatusCode.OK)
            {
                Content = new System.Net.Http.StringContent(dirListingBody, System.Text.Encoding.UTF8, "application/json")
            };
        });
        var httpClient = new System.Net.Http.HttpClient(handler) { BaseAddress = new Uri("https://dev.azure.com/testorg/") };
        var factory = Substitute.For<IHttpClientFactory>();
        factory.CreateClient("AzureDevOps").Returns(httpClient);
        var service = new AdoRepositoriesService(factory);

        // Act
        var result = await service.GetItemAsync("repo1", "/src", "MyProject");

        // Assert
        Assert.Equal(2, callCount); // first call failed, second succeeded
        var lastUrl = handler.LastRequest!.RequestUri!.ToString();
        Assert.Contains("scopePath=", lastUrl);
        Assert.Contains("recursionLevel=OneLevel", lastUrl);
        Assert.DoesNotContain("includeContent", lastUrl);
        Assert.DoesNotContain("path=%2Fsrc", lastUrl);
        Assert.Contains("isFolder", result.ToString());
    }

    [Fact(DisplayName = "取得目錄清單（200 但 gitObjectType=tree）- 自動降級為 scopePath + recursionLevel=OneLevel")]
    public async Task GetItemAsync_Directory_FallsBackToRecursionLevel_On200WithTree()
    {
        // Arrange
        var treeItemBody = """{"objectId":"abc","gitObjectType":"tree","path":"/src","isFolder":true}""";
        var dirListingBody = """{"value":[{"path":"/src","isFolder":true},{"path":"/src/Program.cs","isFolder":false}],"count":2}""";

        int callCount = 0;
        var handler = new MockHttpMessageHandler(req =>
        {
            callCount++;
            if (callCount == 1)
                return new System.Net.Http.HttpResponseMessage(System.Net.HttpStatusCode.OK)
                {
                    Content = new System.Net.Http.StringContent(treeItemBody, System.Text.Encoding.UTF8, "application/json")
                };
            return new System.Net.Http.HttpResponseMessage(System.Net.HttpStatusCode.OK)
            {
                Content = new System.Net.Http.StringContent(dirListingBody, System.Text.Encoding.UTF8, "application/json")
            };
        });
        var httpClient = new System.Net.Http.HttpClient(handler) { BaseAddress = new Uri("https://dev.azure.com/testorg/") };
        var factory = Substitute.For<IHttpClientFactory>();
        factory.CreateClient("AzureDevOps").Returns(httpClient);
        var service = new AdoRepositoriesService(factory);

        // Act
        var result = await service.GetItemAsync("repo1", "/src", "MyProject");

        // Assert
        Assert.Equal(2, callCount); // detected tree on first call, fetched listing on second
        var lastUrl = handler.LastRequest!.RequestUri!.ToString();
        Assert.Contains("scopePath=", lastUrl);
        Assert.Contains("recursionLevel=OneLevel", lastUrl);
        Assert.Contains("count", result.ToString()); // directory listing format
    }
}
