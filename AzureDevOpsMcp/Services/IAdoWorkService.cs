using System.Text.Json;

namespace AzureDevOpsMcp.Services;

/// <summary>提供 Azure DevOps Work API 操作的服務介面，用於存取 Sprint 與 Backlog 資訊。</summary>
public interface IAdoWorkService
{
    /// <summary>列出指定專案或團隊的所有迭代（Sprint）。</summary>
    /// <param name="project">專案名稱。</param>
    /// <param name="team">團隊名稱，若為 null 則使用預設團隊。</param>
    /// <param name="timeframe">時間框架篩選（current/past/future）。</param>
    Task<JsonElement> ListIterationsAsync(string project, string? team = null, string? timeframe = null);

    /// <summary>取得指定迭代中的所有工作項目。</summary>
    /// <param name="project">專案名稱。</param>
    /// <param name="iterationId">迭代 ID。</param>
    /// <param name="team">團隊名稱，若為 null 則使用預設團隊。</param>
    Task<JsonElement> GetIterationWorkItemsAsync(string project, string iterationId, string? team = null);

    /// <summary>列出指定專案或團隊的所有待辦項目清單（Backlog）。</summary>
    /// <param name="project">專案名稱。</param>
    /// <param name="team">團隊名稱，若為 null 則使用預設團隊。</param>
    Task<JsonElement> ListBacklogsAsync(string project, string? team = null);
}
