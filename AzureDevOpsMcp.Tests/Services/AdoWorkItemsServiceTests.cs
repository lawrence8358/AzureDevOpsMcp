using System.Net;
using AzureDevOpsMcp.Configuration;
using AzureDevOpsMcp.Services;
using AzureDevOpsMcp.Tests.Helpers;
using NSubstitute;

namespace AzureDevOpsMcp.Tests.Services;

public class AdoWorkItemsServiceTests
{
    private readonly AdoOptions _options = new()
    {
        ServerUrl = "https://dev.azure.com/testorg",
        PatToken = "test-pat"
    };

    private (AdoWorkItemsService service, MockHttpMessageHandler handler) CreateService(string jsonResponse)
    {
        var handler = new MockHttpMessageHandler(jsonResponse);
        var httpClient = new HttpClient(handler) { BaseAddress = new Uri(_options.ServerUrl.TrimEnd('/') + "/") };
        var factory = Substitute.For<IHttpClientFactory>();
        factory.CreateClient("AzureDevOps").Returns(httpClient);
        return (new AdoWorkItemsService(factory, _options), handler);
    }

    private (AdoWorkItemsService service, MockHttpMessageHandler handler) CreateService(MockHttpMessageHandler handler)
    {
        var httpClient = new HttpClient(handler) { BaseAddress = new Uri(_options.ServerUrl.TrimEnd('/') + "/") };
        var factory = Substitute.For<IHttpClientFactory>();
        factory.CreateClient("AzureDevOps").Returns(httpClient);
        return (new AdoWorkItemsService(factory, _options), handler);
    }

    [Fact(DisplayName = "取得工作項目 - 呼叫正確的 API 端點")]
    public async Task GetWorkItemAsync_CallsCorrectEndpoint()
    {
        // Arrange
        var (service, handler) = CreateService("""{"id":42,"fields":{"System.Title":"Test"}}""");

        // Act
        await service.GetWorkItemAsync(42);

        // Assert
        Assert.NotNull(handler.LastRequest);
        Assert.Equal(HttpMethod.Get, handler.LastRequest!.Method);
        Assert.Contains("_apis/wit/workitems/42", handler.LastRequest.RequestUri!.ToString());
        Assert.Contains("api-version=7.1", handler.LastRequest.RequestUri.ToString());
    }

    [Fact(DisplayName = "取得工作項目並指定展開選項 - 包含 expand 查詢參數")]
    public async Task GetWorkItemAsync_WithExpand_IncludesQueryParam()
    {
        // Arrange
        var (service, handler) = CreateService("""{"id":42,"fields":{}}""");

        // Act
        await service.GetWorkItemAsync(42, expand: "all");

        // Assert
        var url = handler.LastRequest!.RequestUri!.ToString();
        Assert.Contains("$expand=all", url);
    }

    [Fact(DisplayName = "建立工作項目 - 使用 POST 方法並呼叫正確端點")]
    public async Task CreateWorkItemAsync_UsesPostAndCorrectEndpoint()
    {
        // Arrange
        var (service, handler) = CreateService("""{"id":1,"fields":{"System.Title":"NewItem"}}""");

        // Act
        await service.CreateWorkItemAsync("MyProject", "Task", "NewItem");

        // Assert
        Assert.Equal(HttpMethod.Post, handler.LastRequest!.Method);
        Assert.Contains("MyProject/_apis/wit/workitems/$Task", handler.LastRequest.RequestUri!.ToString());
        Assert.Contains("api-version=7.1", handler.LastRequest.RequestUri.ToString());
    }

    [Fact(DisplayName = "建立工作項目 - 設定 JSON Patch 內容類型")]
    public async Task CreateWorkItemAsync_SetsJsonPatchContentType()
    {
        // Arrange
        var (service, handler) = CreateService("""{"id":1,"fields":{}}""");

        // Act
        await service.CreateWorkItemAsync("MyProject", "Bug", "BugTitle");

        // Assert
        Assert.Equal("application/json-patch+json", handler.LastRequest!.Content!.Headers.ContentType!.MediaType);
    }

    [Fact(DisplayName = "以 WIQL 查詢工作項目 - 使用 POST 發送查詢")]
    public async Task QueryByWiqlAsync_PostsWiqlQuery()
    {
        // Arrange
        var (service, handler) = CreateService("""{"workItems":[{"id":1}]}""");

        // Act
        await service.QueryByWiqlAsync("MyProject", "SELECT [System.Id] FROM workitems", top: 10);

        // Assert
        Assert.Equal(HttpMethod.Post, handler.LastRequest!.Method);
        var url = handler.LastRequest.RequestUri!.ToString();
        Assert.Contains("MyProject/_apis/wit/wiql", url);
        Assert.Contains("$top=10", url);
    }

    [Fact(DisplayName = "刪除工作項目 - 使用 DELETE 方法")]
    public async Task DeleteWorkItemAsync_UsesDeleteMethod()
    {
        // Arrange
        var (service, handler) = CreateService("""{"id":42}""");

        // Act
        await service.DeleteWorkItemAsync(42, destroy: true);

        // Assert
        Assert.Equal(HttpMethod.Delete, handler.LastRequest!.Method);
        var url = handler.LastRequest.RequestUri!.ToString();
        Assert.Contains("_apis/wit/workitems/42", url);
        Assert.Contains("destroy=true", url);
    }

    [Fact(DisplayName = "批次取得工作項目 - 以 POST 傳送 ID 清單至正確端點")]
    public async Task BatchGetWorkItemsAsync_PostsIdsToCorrectEndpoint()
    {
        // Arrange
        var (service, handler) = CreateService("""{"value":[{"id":1},{"id":2}]}""");

        // Act
        await service.BatchGetWorkItemsAsync([1, 2], expand: "all");

        // Assert
        Assert.Equal(HttpMethod.Post, handler.LastRequest!.Method);
        Assert.Contains("_apis/wit/workitemsbatch", handler.LastRequest.RequestUri!.ToString());
    }

    [Fact(DisplayName = "取得工作項目（API 回傳 404）- 應拋出 HttpRequestException")]
    public async Task GetWorkItemAsync_Returns404_ThrowsException()
    {
        // Arrange
        var handler = new MockHttpMessageHandler("""{"message":"Work item not found"}""", System.Net.HttpStatusCode.NotFound);
        var (service, _) = CreateService(handler);

        // Act & Assert
        await Assert.ThrowsAsync<HttpRequestException>(() => service.GetWorkItemAsync(999));
    }

    [Fact(DisplayName = "取得工作項目（API 回傳 401）- 應拋出 HttpRequestException")]
    public async Task GetWorkItemAsync_Returns401_ThrowsException()
    {
        // Arrange
        var handler = new MockHttpMessageHandler("""{"message":"Unauthorized"}""", System.Net.HttpStatusCode.Unauthorized);
        var (service, _) = CreateService(handler);

        // Act & Assert
        await Assert.ThrowsAsync<HttpRequestException>(() => service.GetWorkItemAsync(42));
    }

    [Fact(DisplayName = "新增工作項目連結 - 使用 PATCH 方法並包含正確的關聯 URL")]
    public async Task AddLinkAsync_UsesPatchWithCorrectRelationUrl()
    {
        // Arrange
        var handler = new MockHttpMessageHandler("""{"id":1,"relations":[]}""");
        var (service, mockHandler) = CreateService(handler);

        // Act
        await service.AddLinkAsync(id: 1, targetId: 2, linkType: "System.LinkTypes.Related");

        // Assert
        Assert.Equal(HttpMethod.Patch, mockHandler.LastRequest!.Method);
        Assert.Contains("_apis/wit/workitems/1", mockHandler.LastRequest.RequestUri!.ToString());
        Assert.NotNull(mockHandler.LastRequestBody);
        Assert.Contains("System.LinkTypes.Related", mockHandler.LastRequestBody);
        Assert.Contains("https://dev.azure.com/testorg/_apis/wit/workitems/2", mockHandler.LastRequestBody);
    }

    [Fact(DisplayName = "更新工作項目 - 使用 PATCH 方法並設定 JSON-Patch 內容類型")]
    public async Task UpdateWorkItemAsync_UsesPatchWithJsonPatchContentType()
    {
        // Arrange
        var handler = new MockHttpMessageHandler("""{"id":42}""");
        var (service, mockHandler) = CreateService(handler);
        var fields = new Dictionary<string, object> { ["System.State"] = "Active" };

        // Act
        await service.UpdateWorkItemAsync(42, fields);

        // Assert
        Assert.Equal(HttpMethod.Patch, mockHandler.LastRequest!.Method);
        Assert.Contains("_apis/wit/workitems/42", mockHandler.LastRequest.RequestUri!.ToString());
        Assert.Equal("application/json-patch+json", mockHandler.LastRequest.Content!.Headers.ContentType!.MediaType);
        Assert.NotNull(mockHandler.LastRequestBody);
        Assert.Contains("System.State", mockHandler.LastRequestBody);
    }

    [Fact(DisplayName = "新增評論 - 使用 POST 方法至正確端點")]
    public async Task AddCommentAsync_UsesPostToCorrectEndpoint()
    {
        // Arrange
        var handler = new MockHttpMessageHandler("""{"id":1,"text":"Test comment"}""");
        var (service, mockHandler) = CreateService(handler);

        // Act
        await service.AddCommentAsync("MyProject", workItemId: 42, text: "Test comment");

        // Assert
        Assert.Equal(HttpMethod.Post, mockHandler.LastRequest!.Method);
        var url = mockHandler.LastRequest.RequestUri!.ToString();
        Assert.Contains("MyProject/_apis/wit/workitems/42/comments", url);
        Assert.Contains("api-version=7.1-preview.4", url);
    }

    [Fact(DisplayName = "取得工作項目變更歷程 - 包含正確的 top 參數")]
    public async Task GetUpdatesAsync_IncludesTopParam()
    {
        // Arrange
        var handler = new MockHttpMessageHandler("""{"value":[]}""");
        var (service, mockHandler) = CreateService(handler);

        // Act
        await service.GetUpdatesAsync(id: 42, top: 5);

        // Assert
        var url = mockHandler.LastRequest!.RequestUri!.ToString();
        Assert.Contains("_apis/wit/workitems/42/updates", url);
        Assert.Contains("$top=5", url);
    }

    [Fact(DisplayName = "建立工作項目 - Request Body 包含正確的 System.Title 欄位與值")]
    public async Task CreateWorkItemAsync_RequestBodyContainsCorrectFields()
    {
        // Arrange
        var (service, handler) = CreateService("""{"id":1,"fields":{}}""");

        // Act
        await service.CreateWorkItemAsync("MyProject", "Task", title: "My Title", description: "My Desc");

        // Assert
        Assert.NotNull(handler.LastRequestBody);
        Assert.Contains("/fields/System.Title", handler.LastRequestBody);
        Assert.Contains("My Title", handler.LastRequestBody);
        Assert.Contains("/fields/System.Description", handler.LastRequestBody);
        Assert.Contains("My Desc", handler.LastRequestBody);
    }

    [Fact(DisplayName = "建立工作項目（API 回傳 403）- 應拋出 HttpRequestException 並含錯誤訊息")]
    public async Task CreateWorkItemAsync_Returns403_ThrowsExceptionWithBody()
    {
        // Arrange
        var handler = new MockHttpMessageHandler("""{"message":"Access denied - insufficient permissions"}""", System.Net.HttpStatusCode.Forbidden);
        var (service, _) = CreateService(handler);

        // Act & Assert
        var ex = await Assert.ThrowsAsync<HttpRequestException>(
            () => service.CreateWorkItemAsync("MyProject", "Task", "Title"));
        Assert.Contains("Access denied", ex.Message);
    }
}
