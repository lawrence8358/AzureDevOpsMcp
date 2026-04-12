using System.ComponentModel;
using AzureDevOpsMcp.Configuration;
using AzureDevOpsMcp.Services;
using ModelContextProtocol.Server;

namespace AzureDevOpsMcp.Tools.PullRequests;

/// <summary>提供取得指定拉取請求詳情的 MCP 工具。</summary>
[McpServerToolType]
public static class GetPullRequestTool
{
    /// <summary>依 ID 取得指定拉取請求的詳細資訊。</summary>
    [
        McpServerTool(Name = "mcp_ado_pr_get_pull_request"), 
        Description("Get detailed information about a specific pull request by ID, including status, source/target branch, reviewers, and merge status.")
    ]
    public static async Task<string> Execute(
        IAdoRepositoriesService reposService,
        AdoOptions adoOptions,
        [Description("Repository ID or name")] string repositoryId,
        [Description("Pull request ID")] int pullRequestId,
        [Description("Project name (optional if ADO_PROJECT is set)")] string? project = null)
    {
        var resolvedProject = project ?? adoOptions.Project
            ?? throw new ArgumentException("Project is required. Set ADO_PROJECT environment variable or provide the project parameter.");
        var result = await reposService.GetPullRequestAsync(repositoryId, pullRequestId, resolvedProject);
        return result.ToString();
    }
}
