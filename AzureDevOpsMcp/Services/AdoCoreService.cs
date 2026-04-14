using System.Text.Json;

namespace AzureDevOpsMcp.Services;

/// <summary>實作 Azure DevOps Core API 操作的服務。</summary>
public class AdoCoreService : IAdoCoreService
{
    #region Members

    private readonly HttpClient _httpClient;

    #endregion

    #region Constructors

    /// <summary>初始化 <see cref="AdoCoreService"/> 的新執行個體。</summary>
    /// <param name="httpClientFactory">用於建立 HTTP 用戶端的工廠。</param>
    public AdoCoreService(IHttpClientFactory httpClientFactory)
    {
        _httpClient = httpClientFactory.CreateClient("AzureDevOps");
    }

    #endregion

    #region Public Methods

    /// <summary>列出 Azure DevOps 組織下的所有專案。</summary>
    /// <param name="stateFilter">專案狀態篩選條件（如 wellFormed、deleting 等）。</param>
    /// <param name="top">最多回傳筆數。</param>
    /// <param name="skip">略過的筆數，用於分頁。</param>
    public async Task<JsonElement> ListProjectsAsync(string? stateFilter = null, int? top = null, int? skip = null)
    {
        var query = new List<string> { "api-version=7.1" };
        if (stateFilter != null) query.Add($"stateFilter={Uri.EscapeDataString(stateFilter)}");
        if (top != null) query.Add($"$top={top}");
        if (skip != null) query.Add($"$skip={skip}");

        var url = $"_apis/projects?{string.Join("&", query)}";
        var response = await _httpClient.GetAsync(url);
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
