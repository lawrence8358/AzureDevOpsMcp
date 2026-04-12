using System.Text.Json;

namespace AzureDevOpsMcp.Services;

/// <summary>提供 Azure DevOps Git Repositories API 操作的服務介面。</summary>
public interface IAdoRepositoriesService
{
    /// <summary>列出指定專案下的所有 Git 儲存庫。</summary>
    /// <param name="project">專案名稱。</param>
    Task<JsonElement> ListRepositoriesAsync(string project);

    /// <summary>列出指定儲存庫的所有分支。</summary>
    /// <param name="repositoryId">儲存庫 ID 或名稱。</param>
    /// <param name="project">專案名稱。</param>
    /// <param name="filter">分支名稱篩選字串。</param>
    Task<JsonElement> ListBranchesAsync(string repositoryId, string project, string? filter = null);

    /// <summary>取得儲存庫中指定路徑的檔案或目錄內容。</summary>
    /// <param name="repositoryId">儲存庫 ID 或名稱。</param>
    /// <param name="path">檔案或目錄路徑。</param>
    /// <param name="project">專案名稱。</param>
    /// <param name="branch">指定分支，預設為預設分支。</param>
    Task<JsonElement> GetItemAsync(string repositoryId, string path, string project, string? branch = null);

    /// <summary>取得指定儲存庫的提交歷史記錄。</summary>
    /// <param name="repositoryId">儲存庫 ID 或名稱。</param>
    /// <param name="project">專案名稱。</param>
    /// <param name="branch">分支名稱。</param>
    /// <param name="itemPath">篩選特定路徑的提交。</param>
    /// <param name="author">作者篩選。</param>
    /// <param name="top">最多回傳筆數。</param>
    Task<JsonElement> GetCommitsAsync(string repositoryId, string project, string? branch = null, string? itemPath = null, string? author = null, int? top = null);

    /// <summary>列出指定儲存庫中的 Pull Request。</summary>
    /// <param name="repositoryId">儲存庫 ID 或名稱。</param>
    /// <param name="project">專案名稱。</param>
    /// <param name="status">狀態篩選（active/completed/abandoned）。</param>
    /// <param name="creatorId">建立者 ID 篩選。</param>
    /// <param name="reviewerId">審查者 ID 篩選。</param>
    /// <param name="top">最多回傳筆數。</param>
    Task<JsonElement> ListPullRequestsAsync(string repositoryId, string project, string? status = null, string? creatorId = null, string? reviewerId = null, int? top = null);

    /// <summary>取得指定 Pull Request 的詳細資訊。</summary>
    /// <param name="repositoryId">儲存庫 ID 或名稱。</param>
    /// <param name="pullRequestId">Pull Request ID。</param>
    /// <param name="project">專案名稱。</param>
    Task<JsonElement> GetPullRequestAsync(string repositoryId, int pullRequestId, string project);

    /// <summary>建立新的 Pull Request。</summary>
    /// <param name="repositoryId">儲存庫 ID 或名稱。</param>
    /// <param name="project">專案名稱。</param>
    /// <param name="sourceBranch">來源分支名稱。</param>
    /// <param name="targetBranch">目標分支名稱。</param>
    /// <param name="title">PR 標題。</param>
    /// <param name="description">PR 描述。</param>
    /// <param name="reviewers">審查者 ID 陣列。</param>
    Task<JsonElement> CreatePullRequestAsync(string repositoryId, string project, string sourceBranch, string targetBranch, string title, string? description = null, string[]? reviewers = null);

    /// <summary>更新指定 Pull Request 的狀態或標題等屬性。</summary>
    /// <param name="repositoryId">儲存庫 ID 或名稱。</param>
    /// <param name="pullRequestId">Pull Request ID。</param>
    /// <param name="project">專案名稱。</param>
    /// <param name="status">新狀態（active/completed/abandoned）。</param>
    /// <param name="title">新標題。</param>
    /// <param name="description">新描述。</param>
    Task<JsonElement> UpdatePullRequestAsync(string repositoryId, int pullRequestId, string project, string? status = null, string? title = null, string? description = null);

    /// <summary>取得指定 Pull Request 的討論串。</summary>
    /// <param name="repositoryId">儲存庫 ID 或名稱。</param>
    /// <param name="pullRequestId">Pull Request ID。</param>
    /// <param name="project">專案名稱。</param>
    Task<JsonElement> GetPrThreadsAsync(string repositoryId, int pullRequestId, string project);

    /// <summary>在指定 Pull Request 建立新的討論串。</summary>
    /// <param name="repositoryId">儲存庫 ID 或名稱。</param>
    /// <param name="pullRequestId">Pull Request ID。</param>
    /// <param name="project">專案名稱。</param>
    /// <param name="content">討論串初始留言內容。</param>
    /// <param name="status">討論串狀態（active/fixed/wontFix/closed/byDesign/pending）。</param>
    Task<JsonElement> CreatePrThreadAsync(string repositoryId, int pullRequestId, string project, string content, string? status = null);
}
