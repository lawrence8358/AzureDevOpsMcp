using System.Text.Json;

namespace AzureDevOpsMcp.Services;

/// <summary>提供 Azure DevOps Work Items API 操作的服務介面。</summary>
public interface IAdoWorkItemsService
{
    /// <summary>依 ID 取得指定工作項目的詳細資訊。</summary>
    /// <param name="id">工作項目 ID。</param>
    /// <param name="expand">擴充欄位選項（如 all、relations 等）。</param>
    Task<JsonElement> GetWorkItemAsync(int id, string? expand = null);

    /// <summary>在指定專案中建立新的工作項目。</summary>
    /// <param name="project">專案名稱。</param>
    /// <param name="type">工作項目類型（如 Bug、Task、User Story）。</param>
    /// <param name="title">工作項目標題。</param>
    /// <param name="description">工作項目描述。</param>
    /// <param name="assignedTo">指派人員。</param>
    /// <param name="areaPath">區域路徑。</param>
    /// <param name="iterationPath">迭代路徑。</param>
    /// <param name="fields">其他自訂欄位的名稱與值對應字典。</param>
    Task<JsonElement> CreateWorkItemAsync(string project, string type, string title, string? description = null, string? assignedTo = null, string? areaPath = null, string? iterationPath = null, Dictionary<string, object>? fields = null);

    /// <summary>更新指定工作項目的欄位值。</summary>
    /// <param name="id">工作項目 ID。</param>
    /// <param name="fields">要更新的欄位名稱與值對應字典。</param>
    Task<JsonElement> UpdateWorkItemAsync(int id, Dictionary<string, object> fields);

    /// <summary>刪除指定的工作項目。</summary>
    /// <param name="id">工作項目 ID。</param>
    /// <param name="destroy">若為 true 則永久刪除，否則移至資源回收筒。</param>
    Task<JsonElement> DeleteWorkItemAsync(int id, bool destroy = false);

    /// <summary>使用 WIQL 查詢語言搜尋工作項目。</summary>
    /// <param name="project">專案名稱。</param>
    /// <param name="query">WIQL 查詢字串。</param>
    /// <param name="top">最多回傳筆數。</param>
    Task<JsonElement> QueryByWiqlAsync(string project, string query, int? top = null);

    /// <summary>在指定工作項目新增討論留言。</summary>
    /// <param name="project">專案名稱。</param>
    /// <param name="workItemId">工作項目 ID。</param>
    /// <param name="text">評論內容。</param>
    Task<JsonElement> AddCommentAsync(string project, int workItemId, string text);

    /// <summary>取得指定工作項目的所有討論留言。</summary>
    /// <param name="project">專案名稱。</param>
    /// <param name="workItemId">工作項目 ID。</param>
    /// <param name="top">最多回傳筆數。</param>
    Task<JsonElement> GetCommentsAsync(string project, int workItemId, int? top = null);

    /// <summary>在工作項目間建立關聯連結。</summary>
    /// <param name="id">工作項目 ID。</param>
    /// <param name="targetId">目標工作項目 ID。</param>
    /// <param name="linkType">連結類型。</param>
    /// <param name="comment">連結附加說明。</param>
    Task<JsonElement> AddLinkAsync(int id, int targetId, string linkType, string? comment = null);

    /// <summary>取得指定工作項目的歷史變更記錄。</summary>
    /// <param name="id">工作項目 ID。</param>
    /// <param name="top">最多回傳筆數。</param>
    Task<JsonElement> GetUpdatesAsync(int id, int? top = null);

    /// <summary>批次取得多個工作項目的詳細資訊。</summary>
    /// <param name="ids">工作項目 ID 陣列。</param>
    /// <param name="expand">擴充欄位選項。</param>
    Task<JsonElement> BatchGetWorkItemsAsync(int[] ids, string? expand = null);
}
