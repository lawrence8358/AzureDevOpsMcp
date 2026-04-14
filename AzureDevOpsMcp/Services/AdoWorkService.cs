using System.Text.Json;

namespace AzureDevOpsMcp.Services;

/// <summary>實作 Azure DevOps Work API 操作的服務，用於存取 Sprint 與 Backlog 資訊。</summary>
public class AdoWorkService : IAdoWorkService
{
    #region Members

    private readonly HttpClient _httpClient;

    #endregion

    #region Constructors

    /// <summary>初始化 <see cref="AdoWorkService"/> 的新執行個體。</summary>
    /// <param name="httpClientFactory">用於建立 HTTP 用戶端的工廠。</param>
    public AdoWorkService(IHttpClientFactory httpClientFactory)
    {
        _httpClient = httpClientFactory.CreateClient("AzureDevOps");
    }

    #endregion

    #region Public Methods

    /// <summary>列出指定專案或團隊的所有迭代（Sprint）。</summary>
    /// <param name="project">Azure DevOps 專案名稱。</param>
    /// <param name="team">團隊名稱；若為 null 則使用預設團隊。</param>
    /// <param name="timeframe">時間框架篩選條件（如 current）。</param>
    public async Task<JsonElement> ListIterationsAsync(string project, string? team = null, string? timeframe = null)
    {
        var teamSegment = team != null ? $"/{Uri.EscapeDataString(team)}" : string.Empty;
        var query = new List<string> { "api-version=7.1" };
        if (timeframe != null) query.Add($"$timeframe={Uri.EscapeDataString(timeframe)}");

        var url = $"{Uri.EscapeDataString(project)}{teamSegment}/_apis/work/teamsettings/iterations?{string.Join("&", query)}";
        var response = await _httpClient.GetAsync(url);
        await response.EnsureSuccessWithBodyAsync();
        return await ParseResponseAsync(response);
    }

    /// <summary>取得指定迭代中的所有工作項目。</summary>
    /// <param name="project">Azure DevOps 專案名稱。</param>
    /// <param name="iterationId">迭代識別碼。</param>
    /// <param name="team">團隊名稱；若為 null 則使用預設團隊。</param>
    public async Task<JsonElement> GetIterationWorkItemsAsync(string project, string iterationId, string? team = null)
    {
        var teamSegment = team != null ? $"/{Uri.EscapeDataString(team)}" : string.Empty;
        // 技術債：Iteration Work Items API 截至 2026-04 仍為 Preview 版本（7.1-preview.1）。
        // 待 Microsoft 發布正式版本後升級。
        var url = $"{Uri.EscapeDataString(project)}{teamSegment}/_apis/work/teamsettings/iterations/{Uri.EscapeDataString(iterationId)}/workitems?api-version=7.1-preview.1";
        var response = await _httpClient.GetAsync(url);
        await response.EnsureSuccessWithBodyAsync();
        return await ParseResponseAsync(response);
    }

    /// <summary>列出指定專案或團隊的所有待辦項目清單（Backlog）。</summary>
    /// <param name="project">Azure DevOps 專案名稱。</param>
    /// <param name="team">團隊名稱；若為 null 則使用預設團隊。</param>
    public async Task<JsonElement> ListBacklogsAsync(string project, string? team = null)
    {
        var teamSegment = team != null ? $"/{Uri.EscapeDataString(team)}" : string.Empty;
        var url = $"{Uri.EscapeDataString(project)}{teamSegment}/_apis/work/backlogs?api-version=7.1";
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
