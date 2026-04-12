using System.Text.Json;

namespace AzureDevOpsMcp.Services;

/// <summary>提供 Azure DevOps Core API 操作的服務介面。</summary>
public interface IAdoCoreService
{
    /// <summary>列出 Azure DevOps 組織下的所有專案。</summary>
    /// <param name="stateFilter">專案狀態篩選（如 wellFormed、all 等）。</param>
    /// <param name="top">最多回傳筆數。</param>
    /// <param name="skip">略過筆數（分頁用）。</param>
    Task<JsonElement> ListProjectsAsync(string? stateFilter = null, int? top = null, int? skip = null);
}
