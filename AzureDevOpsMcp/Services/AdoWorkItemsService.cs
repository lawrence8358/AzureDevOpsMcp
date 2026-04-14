using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using AzureDevOpsMcp.Configuration;

namespace AzureDevOpsMcp.Services;

/// <summary>實作 Azure DevOps Work Items API 操作的服務。</summary>
public class AdoWorkItemsService : IAdoWorkItemsService
{
    #region Members

    private readonly HttpClient _httpClient;
    private readonly AdoOptions _options;

    #endregion

    #region Constructors

    /// <summary>初始化 <see cref="AdoWorkItemsService"/> 的新執行個體。</summary>
    /// <param name="httpClientFactory">用於建立 HTTP 用戶端的工廠。</param>
    /// <param name="options">Azure DevOps 連線設定選項。</param>
    public AdoWorkItemsService(IHttpClientFactory httpClientFactory, AdoOptions options)
    {
        _httpClient = httpClientFactory.CreateClient("AzureDevOps");
        _options = options;
    }

    #endregion

    #region Public Methods

    /// <summary>依 ID 取得指定工作項目的詳細資訊。</summary>
    /// <param name="id">工作項目 ID。</param>
    /// <param name="expand">擴充欄位選項（如 all、relations 等）。</param>
    public async Task<JsonElement> GetWorkItemAsync(int id, string? expand = null)
    {
        var query = new List<string> { "api-version=7.1" };
        if (expand != null) query.Add($"$expand={Uri.EscapeDataString(expand)}");

        var url = $"_apis/wit/workitems/{id}?{string.Join("&", query)}";
        var response = await _httpClient.GetAsync(url);
        await response.EnsureSuccessWithBodyAsync();
        return await ParseResponseAsync(response);
    }

    /// <summary>在指定專案中建立新的工作項目。</summary>
    /// <param name="project">Azure DevOps 專案名稱。</param>
    /// <param name="type">工作項目類型（如 Bug、Task、User Story 等）。</param>
    /// <param name="title">工作項目標題。</param>
    /// <param name="description">工作項目描述。</param>
    /// <param name="assignedTo">指派對象（使用者名稱或 Email）。</param>
    /// <param name="areaPath">區域路徑。</param>
    /// <param name="iterationPath">迭代路徑。</param>
    /// <param name="fields">要設定的額外欄位名稱與值對應字典。</param>
    public async Task<JsonElement> CreateWorkItemAsync(string project, string type, string title, string? description = null, string? assignedTo = null, string? areaPath = null, string? iterationPath = null, Dictionary<string, object>? fields = null)
    {
        var patchDoc = new List<object>
        {
            new { op = "add", path = "/fields/System.Title", value = title }
        };
        if (description != null) patchDoc.Add(new { op = "add", path = "/fields/System.Description", value = description });
        if (assignedTo != null) patchDoc.Add(new { op = "add", path = "/fields/System.AssignedTo", value = assignedTo });
        if (areaPath != null) patchDoc.Add(new { op = "add", path = "/fields/System.AreaPath", value = areaPath });
        if (iterationPath != null) patchDoc.Add(new { op = "add", path = "/fields/System.IterationPath", value = iterationPath });
        if (fields != null)
        {
            foreach (var (key, value) in fields)
                patchDoc.Add(new { op = "add", path = $"/fields/{key}", value });
        }

        var json = JsonSerializer.Serialize(patchDoc);
        var content = new StringContent(json, Encoding.UTF8);
        content.Headers.ContentType = new MediaTypeHeaderValue("application/json-patch+json");

        var encodedType = Uri.EscapeDataString(type);
        var url = $"{Uri.EscapeDataString(project)}/_apis/wit/workitems/${encodedType}?api-version=7.1";
        var response = await _httpClient.PostAsync(url, content);
        await response.EnsureSuccessWithBodyAsync();
        return await ParseResponseAsync(response);
    }

    /// <summary>更新指定工作項目的欄位。</summary>
    /// <param name="id">工作項目 ID。</param>
    /// <param name="fields">要更新的欄位名稱與值對應字典。</param>
    public async Task<JsonElement> UpdateWorkItemAsync(int id, Dictionary<string, object> fields)
    {
        var patchDoc = fields.Select(f => new { op = "add", path = $"/fields/{f.Key}", value = f.Value }).ToList();
        var json = JsonSerializer.Serialize(patchDoc);
        var content = new StringContent(json, Encoding.UTF8);
        content.Headers.ContentType = new MediaTypeHeaderValue("application/json-patch+json");

        using var request = new HttpRequestMessage(HttpMethod.Patch, $"_apis/wit/workitems/{id}?api-version=7.1") { Content = content };
        var response = await _httpClient.SendAsync(request);
        await response.EnsureSuccessWithBodyAsync();
        return await ParseResponseAsync(response);
    }

    /// <summary>刪除指定的工作項目。</summary>
    /// <param name="id">工作項目 ID。</param>
    /// <param name="destroy">若為 true 則永久刪除，否則移至資源回收筒。</param>
    public async Task<JsonElement> DeleteWorkItemAsync(int id, bool destroy = false)
    {
        var url = $"_apis/wit/workitems/{id}?api-version=7.1&destroy={destroy.ToString().ToLowerInvariant()}";
        var response = await _httpClient.DeleteAsync(url);
        await response.EnsureSuccessWithBodyAsync();

        // destroy=true returns 204 No Content with an empty body; soft delete returns 200 with JSON
        if (response.StatusCode == System.Net.HttpStatusCode.NoContent)
        {
            using var doc = JsonDocument.Parse($"{{\"deleted\":true,\"id\":{id},\"permanent\":true}}");
            return doc.RootElement.Clone();
        }

        return await ParseResponseAsync(response);
    }

    /// <summary>使用 WIQL 查詢語言取得工作項目清單。</summary>
    /// <param name="project">Azure DevOps 專案名稱。</param>
    /// <param name="query">WIQL 查詢字串。</param>
    /// <param name="top">最多回傳筆數。</param>
    public async Task<JsonElement> QueryByWiqlAsync(string project, string query, int? top = null)
    {
        var queryParams = new List<string> { "api-version=7.1" };
        if (top != null) queryParams.Add($"$top={top}");

        var body = JsonSerializer.Serialize(new { query });
        var content = new StringContent(body, Encoding.UTF8, "application/json");

        var url = $"{Uri.EscapeDataString(project)}/_apis/wit/wiql?{string.Join("&", queryParams)}";
        var response = await _httpClient.PostAsync(url, content);
        await response.EnsureSuccessWithBodyAsync();
        return await ParseResponseAsync(response);
    }

    /// <summary>在指定工作項目新增評論。</summary>
    /// <param name="project">Azure DevOps 專案名稱。</param>
    /// <param name="workItemId">工作項目 ID。</param>
    /// <param name="text">評論內容。</param>
    public async Task<JsonElement> AddCommentAsync(string project, int workItemId, string text)
    {
        var body = JsonSerializer.Serialize(new { text });
        var content = new StringContent(body, Encoding.UTF8, "application/json");

        // 技術債：Work Item Comments API 截至 2026-04 仍為 Preview 版本（7.1-preview.4）。
        // 待 Microsoft 發布正式版本後升級。
        var url = $"{Uri.EscapeDataString(project)}/_apis/wit/workitems/{workItemId}/comments?api-version=7.1-preview.4";
        var response = await _httpClient.PostAsync(url, content);
        await response.EnsureSuccessWithBodyAsync();
        return await ParseResponseAsync(response);
    }

    /// <summary>取得指定工作項目的所有評論。</summary>
    /// <param name="project">Azure DevOps 專案名稱。</param>
    /// <param name="workItemId">工作項目 ID。</param>
    /// <param name="top">最多回傳筆數。</param>
    public async Task<JsonElement> GetCommentsAsync(string project, int workItemId, int? top = null)
    {
        // 技術債：Work Item Comments API 截至 2026-04 仍為 Preview 版本（7.1-preview.4）。
        // 待 Microsoft 發布正式版本後升級。
        var query = new List<string> { "api-version=7.1-preview.4" };
        if (top != null) query.Add($"$top={top}");

        var url = $"{Uri.EscapeDataString(project)}/_apis/wit/workitems/{workItemId}/comments?{string.Join("&", query)}";
        var response = await _httpClient.GetAsync(url);
        await response.EnsureSuccessWithBodyAsync();
        return await ParseResponseAsync(response);
    }

    /// <summary>為指定工作項目新增關聯連結。</summary>
    /// <param name="id">工作項目 ID。</param>
    /// <param name="targetId">目標工作項目 ID。</param>
    /// <param name="linkType">連結類型（如 System.LinkTypes.Related 等）。</param>
    /// <param name="comment">連結附加說明文字。</param>
    public async Task<JsonElement> AddLinkAsync(int id, int targetId, string linkType, string? comment = null)
    {
        var serverUrl = _options.ServerUrl.TrimEnd('/');
        var patchDoc = new List<object>
        {
            new
            {
                op = "add",
                path = "/relations/-",
                value = new
                {
                    rel = linkType,
                    url = $"{serverUrl}/_apis/wit/workitems/{targetId}",
                    attributes = new { comment = comment ?? string.Empty }
                }
            }
        };

        var json = JsonSerializer.Serialize(patchDoc);
        var content = new StringContent(json, Encoding.UTF8);
        content.Headers.ContentType = new MediaTypeHeaderValue("application/json-patch+json");

        using var request = new HttpRequestMessage(HttpMethod.Patch, $"_apis/wit/workitems/{id}?api-version=7.1") { Content = content };
        var response = await _httpClient.SendAsync(request);
        await response.EnsureSuccessWithBodyAsync();
        return await ParseResponseAsync(response);
    }

    /// <summary>取得指定工作項目的變更歷程記錄。</summary>
    /// <param name="id">工作項目 ID。</param>
    /// <param name="top">最多回傳筆數。</param>
    public async Task<JsonElement> GetUpdatesAsync(int id, int? top = null)
    {
        var query = new List<string> { "api-version=7.1" };
        if (top != null) query.Add($"$top={top}");

        var url = $"_apis/wit/workitems/{id}/updates?{string.Join("&", query)}";
        var response = await _httpClient.GetAsync(url);
        await response.EnsureSuccessWithBodyAsync();
        return await ParseResponseAsync(response);
    }

    /// <summary>批次取得多個工作項目的詳細資訊。</summary>
    /// <param name="ids">工作項目 ID 陣列。</param>
    /// <param name="expand">擴充欄位選項（如 all、relations 等）。</param>
    public async Task<JsonElement> BatchGetWorkItemsAsync(int[] ids, string? expand = null)
    {
        var body = new Dictionary<string, object> { ["ids"] = ids };
        if (expand != null) body["$expand"] = expand;

        var json = JsonSerializer.Serialize(body);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        var url = "_apis/wit/workitemsbatch?api-version=7.1";
        var response = await _httpClient.PostAsync(url, content);
        await response.EnsureSuccessWithBodyAsync();
        return await ParseResponseAsync(response);
    }

    #endregion

    #region Private Methods

    /// <summary>解析 HTTP 回應內容並回傳 JSON 元素。</summary>
    /// <param name="response">HTTP 回應訊息。</param>
    private static async Task<JsonElement> ParseResponseAsync(HttpResponseMessage response)
    {
        using var doc = await JsonDocument.ParseAsync(await response.Content.ReadAsStreamAsync());
        return doc.RootElement.Clone();
    }

    #endregion
}