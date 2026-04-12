using System.Text.Json;

namespace AzureDevOpsMcp.Services;

/// <summary>提供 Azure DevOps Build API 操作的服務介面。</summary>
public interface IAdoBuildsService
{
    /// <summary>列出指定專案的建置定義。</summary>
    /// <param name="project">專案名稱。</param>
    /// <param name="name">篩選定義名稱。</param>
    /// <param name="top">最多回傳筆數。</param>
    Task<JsonElement> ListDefinitionsAsync(string project, string? name = null, int? top = null);

    /// <summary>列出指定專案的建置記錄。</summary>
    /// <param name="project">專案名稱。</param>
    /// <param name="definitionId">建置定義 ID。</param>
    /// <param name="statusFilter">狀態篩選。</param>
    /// <param name="resultFilter">結果篩選。</param>
    /// <param name="branchName">分支名稱。</param>
    /// <param name="top">最多回傳筆數。</param>
    Task<JsonElement> ListBuildsAsync(string project, int? definitionId = null, string? statusFilter = null, string? resultFilter = null, string? branchName = null, int? top = null);

    /// <summary>取得指定建置的詳細資訊。</summary>
    /// <param name="project">專案名稱。</param>
    /// <param name="buildId">建置 ID。</param>
    Task<JsonElement> GetBuildAsync(string project, int buildId);

    /// <summary>依建置定義排入新的建置工作。</summary>
    /// <param name="project">專案名稱。</param>
    /// <param name="definitionId">建置定義 ID。</param>
    /// <param name="sourceBranch">來源分支名稱。</param>
    /// <param name="parameters">自訂建置參數（JSON 格式）。</param>
    Task<JsonElement> QueueBuildAsync(string project, int definitionId, string? sourceBranch = null, string? parameters = null);

    /// <summary>取得指定建置的日誌內容。</summary>
    /// <param name="project">專案名稱。</param>
    /// <param name="buildId">建置 ID。</param>
    /// <param name="logId">特定日誌 ID，若為 null 則回傳日誌清單。</param>
    Task<JsonElement> GetBuildLogsAsync(string project, int buildId, int? logId = null);
}
