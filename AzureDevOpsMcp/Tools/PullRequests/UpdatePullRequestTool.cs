using System.ComponentModel;
using AzureDevOpsMcp.Configuration;
using AzureDevOpsMcp.Services;
using ModelContextProtocol.Server;

namespace AzureDevOpsMcp.Tools.PullRequests;

/// <summary>提供更新現有拉取請求的 MCP 工具。</summary>
[McpServerToolType]
public static class UpdatePullRequestTool
{
    /// <summary>更新指定拉取請求的狀態、標題或描述。</summary>
    [
        McpServerTool(Name = "mcp_ado_pr_update_pull_request"), 
        Description("Update an existing pull request. Change its title, description, or status. Set status to 'completed' to merge the PR, or 'abandoned' to close it without merging.")
    ]
    public static async Task<string> Execute(
        IAdoRepositoriesService reposService,
        AdoOptions adoOptions,
        [Description("Repository ID or name")] string repositoryId,
        [Description("Pull request ID")] int pullRequestId,
        [Description("Project name (optional if ADO_PROJECT is set)")] string? project = null,
        [Description("New status (active, completed, abandoned)")] string? status = null,
        [Description("New title")] string? title = null,
        [Description("New description")] string? description = null)
    {
        var resolvedProject = project ?? adoOptions.Project
            ?? throw new ArgumentException("Project is required. Set ADO_PROJECT environment variable or provide the project parameter.");
        if (status == null && title == null && description == null)
            throw new ArgumentException("At least one of status, title, or description must be provided.");
        var result = await reposService.UpdatePullRequestAsync(repositoryId, pullRequestId, resolvedProject, status, title, description);
        return result.ToString();
    }
}
