using System.Text;
using System.Text.Json;

namespace AzureDevOpsMcp.Services;

/// <summary>實作 Azure DevOps Build API 操作的服務。</summary>
public class AdoBuildsService : IAdoBuildsService
{
    #region Members

    private readonly HttpClient _httpClient;

    #endregion

    #region Constructors

    /// <summary>初始化 <see cref="AdoBuildsService"/> 的新執行個體。</summary>
    /// <param name="httpClientFactory">用於建立 HTTP 用戶端的工廠。</param>
    public AdoBuildsService(IHttpClientFactory httpClientFactory)
    {
        _httpClient = httpClientFactory.CreateClient("AzureDevOps");
    }

    #endregion

    #region Public Methods

    /// <summary>列出指定專案的建置定義。</summary>
    /// <param name="project">Azure DevOps 專案名稱。</param>
    /// <param name="name">建置定義名稱篩選條件。</param>
    /// <param name="top">最多回傳筆數。</param>
    public async Task<JsonElement> ListDefinitionsAsync(string project, string? name = null, int? top = null)
    {
        var query = new List<string> { "api-version=7.1" };
        if (name != null) query.Add($"name={Uri.EscapeDataString(name)}");
        if (top != null) query.Add($"$top={top}");

        var url = $"{Uri.EscapeDataString(project)}/_apis/build/definitions?{string.Join("&", query)}";
        var response = await _httpClient.GetAsync(url);
        await response.EnsureSuccessWithBodyAsync();
        return await ParseResponseAsync(response);
    }

    /// <summary>列出指定專案的建置記錄。</summary>
    /// <param name="project">Azure DevOps 專案名稱。</param>
    /// <param name="definitionId">建置定義 ID 篩選條件。</param>
    /// <param name="statusFilter">建置狀態篩選條件（如 inProgress、completed 等）。</param>
    /// <param name="resultFilter">建置結果篩選條件（如 succeeded、failed 等）。</param>
    /// <param name="branchName">分支名稱篩選條件。</param>
    /// <param name="top">最多回傳筆數。</param>
    public async Task<JsonElement> ListBuildsAsync(string project, int? definitionId = null, string? statusFilter = null, string? resultFilter = null, string? branchName = null, int? top = null)
    {
        var query = new List<string> { "api-version=7.1" };
        if (definitionId != null) query.Add($"definitions={definitionId}");
        if (statusFilter != null) query.Add($"statusFilter={Uri.EscapeDataString(statusFilter)}");
        if (resultFilter != null) query.Add($"resultFilter={Uri.EscapeDataString(resultFilter)}");
        if (branchName != null) query.Add($"branchName={Uri.EscapeDataString(branchName)}");
        if (top != null) query.Add($"$top={top}");

        var url = $"{Uri.EscapeDataString(project)}/_apis/build/builds?{string.Join("&", query)}";
        var response = await _httpClient.GetAsync(url);
        await response.EnsureSuccessWithBodyAsync();
        return await ParseResponseAsync(response);
    }

    /// <summary>取得指定建置的詳細資訊。</summary>
    /// <param name="project">Azure DevOps 專案名稱。</param>
    /// <param name="buildId">建置記錄 ID。</param>
    public async Task<JsonElement> GetBuildAsync(string project, int buildId)
    {
        var url = $"{Uri.EscapeDataString(project)}/_apis/build/builds/{buildId}?api-version=7.1";
        var response = await _httpClient.GetAsync(url);
        await response.EnsureSuccessWithBodyAsync();
        return await ParseResponseAsync(response);
    }

    /// <summary>依建置定義排入新的建置工作。</summary>
    /// <param name="project">Azure DevOps 專案名稱。</param>
    /// <param name="definitionId">建置定義 ID。</param>
    /// <param name="sourceBranch">來源分支名稱。</param>
    /// <param name="parameters">建置參數（JSON 字串格式）。</param>
    public async Task<JsonElement> QueueBuildAsync(string project, int definitionId, string? sourceBranch = null, string? parameters = null)
    {
        var body = new Dictionary<string, object>
        {
            ["definition"] = new { id = definitionId }
        };
        if (sourceBranch != null) body["sourceBranch"] = sourceBranch.StartsWith("refs/") ? sourceBranch : $"refs/heads/{sourceBranch}";
        if (parameters != null) body["parameters"] = parameters;

        var json = JsonSerializer.Serialize(body);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        var url = $"{Uri.EscapeDataString(project)}/_apis/build/builds?api-version=7.1";
        var response = await _httpClient.PostAsync(url, content);
        await response.EnsureSuccessWithBodyAsync();
        return await ParseResponseAsync(response);
    }

    /// <summary>取得指定建置的日誌內容。</summary>
    /// <param name="project">Azure DevOps 專案名稱。</param>
    /// <param name="buildId">建置記錄 ID。</param>
    /// <param name="logId">指定日誌 ID；若為 null 則回傳日誌清單。</param>
    public async Task<JsonElement> GetBuildLogsAsync(string project, int buildId, int? logId = null)
    {
        var url = logId != null
            ? $"{Uri.EscapeDataString(project)}/_apis/build/builds/{buildId}/logs/{logId}?api-version=7.1"
            : $"{Uri.EscapeDataString(project)}/_apis/build/builds/{buildId}/logs?api-version=7.1";

        var response = await _httpClient.GetAsync(url);
        await response.EnsureSuccessWithBodyAsync();

        if (logId != null)
        {
            var text = await response.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(JsonSerializer.Serialize(new { value = text }));
            return doc.RootElement.Clone();
        }

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
