using System.Text;
using System.Text.Json;

namespace AzureDevOpsMcp.Services;

/// <summary>實作 Azure DevOps Git Repositories API 操作的服務。</summary>
public class AdoRepositoriesService : IAdoRepositoriesService
{
    #region Members

    private readonly HttpClient _httpClient;

    #endregion

    #region Constructors

    /// <summary>初始化 <see cref="AdoRepositoriesService"/> 的新執行個體。</summary>
    /// <param name="httpClientFactory">用於建立 HTTP 用戶端的工廠。</param>
    public AdoRepositoriesService(IHttpClientFactory httpClientFactory)
    {
        _httpClient = httpClientFactory.CreateClient("AzureDevOps");
    }

    #endregion

    #region Public Methods

    /// <summary>列出指定專案下的所有 Git 儲存庫。</summary>
    /// <param name="project">Azure DevOps 專案名稱。</param>
    public async Task<JsonElement> ListRepositoriesAsync(string project)
    {
        var url = $"{Uri.EscapeDataString(project)}/_apis/git/repositories?api-version=7.1";
        var response = await _httpClient.GetAsync(url);
        await response.EnsureSuccessWithBodyAsync();
        return await ParseResponseAsync(response);
    }

    /// <summary>列出指定儲存庫的所有分支。</summary>
    /// <param name="repositoryId">Git 儲存庫識別碼或名稱。</param>
    /// <param name="project">Azure DevOps 專案名稱。</param>
    /// <param name="filter">分支名稱篩選條件。</param>
    public async Task<JsonElement> ListBranchesAsync(string repositoryId, string project, string? filter = null)
    {
        var query = new List<string> { "api-version=7.1" };
        query.Add(filter != null ? $"filter=heads/{Uri.EscapeDataString(filter)}" : "filter=heads/");

        var url = $"{Uri.EscapeDataString(project)}/_apis/git/repositories/{Uri.EscapeDataString(repositoryId)}/refs?{string.Join("&", query)}";
        var response = await _httpClient.GetAsync(url);
        await response.EnsureSuccessWithBodyAsync();
        return await ParseResponseAsync(response);
    }

    /// <summary>取得儲存庫中指定路徑的檔案或目錄內容。若路徑為目錄，自動改以清單模式回傳。</summary>
    /// <param name="repositoryId">Git 儲存庫識別碼或名稱。</param>
    /// <param name="path">檔案或目錄路徑。</param>
    /// <param name="project">Azure DevOps 專案名稱。</param>
    /// <param name="branch">分支名稱。</param>
    public async Task<JsonElement> GetItemAsync(string repositoryId, string path, string project, string? branch = null)
    {
        // 移除開頭斜線，Azure DevOps Server 的 items API 不接受帶前綴斜線的路徑
        if (path.StartsWith("/")) path = path[1..];

        var versionQuery = new List<string> { "api-version=7.1" };
        if (branch != null) versionQuery.Add($"versionDescriptor.version={Uri.EscapeDataString(NormalizeBranchName(branch))}&versionDescriptor.versionType=branch");
        var repoSegment = $"{Uri.EscapeDataString(project)}/_apis/git/repositories/{Uri.EscapeDataString(repositoryId)}/items";

        // First attempt: single item with content (works for blobs/files)
        var fileQuery = new List<string>(versionQuery) { $"path={Uri.EscapeDataString(path)}", "includeContent=true" };
        var fileUrl = $"{repoSegment}?{string.Join("&", fileQuery)}";
        var response = await _httpClient.GetAsync(fileUrl);

        if (response.IsSuccessStatusCode)
        {
            var item = await ParseResponseAsync(response);
            // ADO sometimes returns 200 with a "tree" object for directory paths instead of erroring.
            // Detect this and fall through to the directory listing.
            if (item.TryGetProperty("gitObjectType", out var objType) && objType.GetString() == "tree")
                return await GetDirectoryListingAsync(repoSegment, path, versionQuery);
            return item;
        }

        // ADO returns 500 for other directory paths with GitUnexpectedObjectTypeException
        if (response.StatusCode == System.Net.HttpStatusCode.InternalServerError)
        {
            var body = await response.Content.ReadAsStringAsync();
            if (body.Contains("GitUnexpectedObjectTypeException") || body.Contains("resolved to a Tree"))
                return await GetDirectoryListingAsync(repoSegment, path, versionQuery);
        }

        await response.EnsureSuccessWithBodyAsync();
        return await ParseResponseAsync(response); // unreachable, but satisfies compiler
    }

    private async Task<JsonElement> GetDirectoryListingAsync(string repoSegment, string path, List<string> versionQuery)
    {
        // Directory listing requires scopePath (not path) + recursionLevel
        var dirQuery = new List<string>(versionQuery) { $"scopePath={Uri.EscapeDataString(path)}", "recursionLevel=OneLevel" };
        var dirUrl = $"{repoSegment}?{string.Join("&", dirQuery)}";
        var dirResponse = await _httpClient.GetAsync(dirUrl);
        await dirResponse.EnsureSuccessWithBodyAsync();
        return await ParseResponseAsync(dirResponse);
    }

    /// <summary>取得指定儲存庫的提交歷史記錄。</summary>
    /// <param name="repositoryId">Git 儲存庫識別碼或名稱。</param>
    /// <param name="project">Azure DevOps 專案名稱。</param>
    /// <param name="branch">分支名稱篩選條件。</param>
    /// <param name="itemPath">篩選指定路徑的提交記錄。</param>
    /// <param name="author">提交作者篩選條件。</param>
    /// <param name="top">最多回傳筆數。</param>
    public async Task<JsonElement> GetCommitsAsync(string repositoryId, string project, string? branch = null, string? itemPath = null, string? author = null, int? top = null)
    {
        var query = new List<string> { "api-version=7.1" };
        if (branch != null) query.Add($"searchCriteria.itemVersion.version={Uri.EscapeDataString(NormalizeBranchName(branch))}");
        if (itemPath != null) query.Add($"searchCriteria.itemPath={Uri.EscapeDataString(itemPath)}");
        if (author != null) query.Add($"searchCriteria.author={Uri.EscapeDataString(author)}");
        if (top != null) query.Add($"$top={top}");

        var url = $"{Uri.EscapeDataString(project)}/_apis/git/repositories/{Uri.EscapeDataString(repositoryId)}/commits?{string.Join("&", query)}";
        var response = await _httpClient.GetAsync(url);
        await response.EnsureSuccessWithBodyAsync();
        return await ParseResponseAsync(response);
    }

    /// <summary>列出指定儲存庫中的 Pull Request。</summary>
    /// <param name="repositoryId">Git 儲存庫識別碼或名稱。</param>
    /// <param name="project">Azure DevOps 專案名稱。</param>
    /// <param name="status">狀態篩選條件（如 active、completed 等）。</param>
    /// <param name="creatorId">建立者 ID 篩選條件。</param>
    /// <param name="reviewerId">審閱者 ID 篩選條件。</param>
    /// <param name="top">最多回傳筆數。</param>
    public async Task<JsonElement> ListPullRequestsAsync(string repositoryId, string project, string? status = null, string? creatorId = null, string? reviewerId = null, int? top = null)
    {
        var query = new List<string> { "api-version=7.1" };
        if (status != null) query.Add($"searchCriteria.status={Uri.EscapeDataString(status)}");
        if (creatorId != null) query.Add($"searchCriteria.creatorId={Uri.EscapeDataString(creatorId)}");
        if (reviewerId != null) query.Add($"searchCriteria.reviewerId={Uri.EscapeDataString(reviewerId)}");
        if (top != null) query.Add($"$top={top}");

        var url = $"{Uri.EscapeDataString(project)}/_apis/git/repositories/{Uri.EscapeDataString(repositoryId)}/pullrequests?{string.Join("&", query)}";
        var response = await _httpClient.GetAsync(url);
        await response.EnsureSuccessWithBodyAsync();
        return await ParseResponseAsync(response);
    }

    /// <summary>取得指定 Pull Request 的詳細資訊。</summary>
    /// <param name="repositoryId">Git 儲存庫識別碼或名稱。</param>
    /// <param name="pullRequestId">Pull Request ID。</param>
    /// <param name="project">Azure DevOps 專案名稱。</param>
    public async Task<JsonElement> GetPullRequestAsync(string repositoryId, int pullRequestId, string project)
    {
        var url = $"{Uri.EscapeDataString(project)}/_apis/git/repositories/{Uri.EscapeDataString(repositoryId)}/pullrequests/{pullRequestId}?api-version=7.1";
        var response = await _httpClient.GetAsync(url);
        await response.EnsureSuccessWithBodyAsync();
        return await ParseResponseAsync(response);
    }

    /// <summary>建立新的 Pull Request。</summary>
    /// <param name="repositoryId">Git 儲存庫識別碼或名稱。</param>
    /// <param name="project">Azure DevOps 專案名稱。</param>
    /// <param name="sourceBranch">來源分支名稱。</param>
    /// <param name="targetBranch">目標分支名稱。</param>
    /// <param name="title">Pull Request 標題。</param>
    /// <param name="description">Pull Request 描述。</param>
    /// <param name="reviewers">初始審閱者 ID 陣列。</param>
    public async Task<JsonElement> CreatePullRequestAsync(string repositoryId, string project, string sourceBranch, string targetBranch, string title, string? description = null, string[]? reviewers = null)
    {
        var body = new Dictionary<string, object>
        {
            ["sourceRefName"] = sourceBranch.StartsWith("refs/") ? sourceBranch : $"refs/heads/{sourceBranch}",
            ["targetRefName"] = targetBranch.StartsWith("refs/") ? targetBranch : $"refs/heads/{targetBranch}",
            ["title"] = title
        };
        if (description != null) body["description"] = description;
        if (reviewers is { Length: > 0 })
            body["reviewers"] = reviewers.Select(r => new { id = r }).ToArray();

        var json = JsonSerializer.Serialize(body);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        var url = $"{Uri.EscapeDataString(project)}/_apis/git/repositories/{Uri.EscapeDataString(repositoryId)}/pullrequests?api-version=7.1";
        var response = await _httpClient.PostAsync(url, content);
        await response.EnsureSuccessWithBodyAsync();
        return await ParseResponseAsync(response);
    }

    /// <summary>更新指定 Pull Request 的狀態或標題等屬性。</summary>
    /// <param name="repositoryId">Git 儲存庫識別碼或名稱。</param>
    /// <param name="pullRequestId">Pull Request ID。</param>
    /// <param name="project">Azure DevOps 專案名稱。</param>
    /// <param name="status">新的 Pull Request 狀態（如 active、completed 等）。</param>
    /// <param name="title">新的 Pull Request 標題。</param>
    /// <param name="description">新的 Pull Request 描述。</param>
    public async Task<JsonElement> UpdatePullRequestAsync(string repositoryId, int pullRequestId, string project, string? status = null, string? title = null, string? description = null)
    {
        var body = new Dictionary<string, object>();
        if (status != null) body["status"] = status;
        if (title != null) body["title"] = title;
        if (description != null) body["description"] = description;

        var json = JsonSerializer.Serialize(body);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        using var request = new HttpRequestMessage(HttpMethod.Patch, $"{Uri.EscapeDataString(project)}/_apis/git/repositories/{Uri.EscapeDataString(repositoryId)}/pullrequests/{pullRequestId}?api-version=7.1") { Content = content };
        var response = await _httpClient.SendAsync(request);
        await response.EnsureSuccessWithBodyAsync();
        return await ParseResponseAsync(response);
    }

    /// <summary>取得指定 Pull Request 的討論串。</summary>
    /// <param name="repositoryId">Git 儲存庫識別碼或名稱。</param>
    /// <param name="pullRequestId">Pull Request ID。</param>
    /// <param name="project">Azure DevOps 專案名稱。</param>
    public async Task<JsonElement> GetPrThreadsAsync(string repositoryId, int pullRequestId, string project)
    {
        var url = $"{Uri.EscapeDataString(project)}/_apis/git/repositories/{Uri.EscapeDataString(repositoryId)}/pullrequests/{pullRequestId}/threads?api-version=7.1";
        var response = await _httpClient.GetAsync(url);
        await response.EnsureSuccessWithBodyAsync();
        return await ParseResponseAsync(response);
    }

    /// <summary>在指定 Pull Request 建立新的討論串。</summary>
    /// <param name="repositoryId">Git 儲存庫識別碼或名稱。</param>
    /// <param name="pullRequestId">Pull Request ID。</param>
    /// <param name="project">Azure DevOps 專案名稱。</param>
    /// <param name="content">討論串內容。</param>
    /// <param name="status">討論串初始狀態（如 active、closed 等）。</param>
    public async Task<JsonElement> CreatePrThreadAsync(string repositoryId, int pullRequestId, string project, string content, string? status = null)
    {
        var body = new Dictionary<string, object>
        {
            ["comments"] = new[] { new { content, parentCommentId = 0, commentType = 1 } }
        };
        if (status != null) body["status"] = status switch
        {
            "active" => 1,
            "fixed" => 2,
            "wontFix" => 3,
            "closed" => 4,
            "byDesign" => 5,
            "pending" => 6,
            _ => throw new ArgumentException($"Invalid thread status: '{status}'. Valid values: active, fixed, wontFix, closed, byDesign, pending.")
        };

        var json = JsonSerializer.Serialize(body);
        var httpContent = new StringContent(json, Encoding.UTF8, "application/json");

        var url = $"{Uri.EscapeDataString(project)}/_apis/git/repositories/{Uri.EscapeDataString(repositoryId)}/pullrequests/{pullRequestId}/threads?api-version=7.1";
        var response = await _httpClient.PostAsync(url, httpContent);
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

    /// <summary>正規化分支名稱，移除 refs/heads/ 前綴（Azure DevOps versionDescriptor.versionType=branch 不接受完整 ref 格式）。</summary>
    private static string NormalizeBranchName(string branch)
        => branch.StartsWith("refs/heads/", StringComparison.OrdinalIgnoreCase)
            ? branch["refs/heads/".Length..]
            : branch;

    #endregion
}
