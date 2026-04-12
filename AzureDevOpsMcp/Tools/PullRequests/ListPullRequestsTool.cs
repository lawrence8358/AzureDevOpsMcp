using System.ComponentModel;
using AzureDevOpsMcp.Configuration;
using AzureDevOpsMcp.Services;
using ModelContextProtocol.Server;

namespace AzureDevOpsMcp.Tools.PullRequests;

/// <summary>提供列出儲存庫拉取請求的 MCP 工具。</summary>
[McpServerToolType]
public static class ListPullRequestsTool
{
    /// <summary>列出指定儲存庫中的拉取請求清單。</summary>
    [
        McpServerTool(Name = "mcp_ado_pr_list_pull_requests"), 
        Description("List pull requests in a repository. Filter by status (active, completed, abandoned, all), creator ID, or reviewer ID.")
    ]
    public static async Task<string> Execute(
        IAdoRepositoriesService reposService,
        AdoOptions adoOptions,
        [Description("Repository ID or name")] string repositoryId,
        [Description("Project name (optional if ADO_PROJECT is set)")] string? project = null,
        [Description("PR status filter (active, completed, abandoned, all)")] string? status = null,
        [Description("Creator ID filter")] string? creatorId = null,
        [Description("Reviewer ID filter")] string? reviewerId = null,
        [Description("Maximum number of results")] int? top = null)
    {
        var resolvedProject = project ?? adoOptions.Project
            ?? throw new ArgumentException("Project is required. Set ADO_PROJECT environment variable or provide the project parameter.");
        var result = await reposService.ListPullRequestsAsync(repositoryId, resolvedProject, status, creatorId, reviewerId, top);
        return result.ToString();
    }
}
