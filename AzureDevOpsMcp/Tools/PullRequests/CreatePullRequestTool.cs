using System.ComponentModel;
using AzureDevOpsMcp.Configuration;
using AzureDevOpsMcp.Services;
using ModelContextProtocol.Server;

namespace AzureDevOpsMcp.Tools.PullRequests;

/// <summary>提供建立新拉取請求的 MCP 工具。</summary>
[McpServerToolType]
public static class CreatePullRequestTool
{
    /// <summary>在指定儲存庫中建立新的拉取請求。</summary>
    [
        McpServerTool(Name = "mcp_ado_pr_create_pull_request"), 
        Description("Create a new pull request from a source branch into a target branch. Specify title, optional description, and optional reviewer IDs.")
    ]
    public static async Task<string> Execute(
        IAdoRepositoriesService reposService,
        AdoOptions adoOptions,
        [Description("Repository ID or name")] string repositoryId,
        [Description("Source branch name")] string sourceBranch,
        [Description("Target branch name")] string targetBranch,
        [Description("Pull request title")] string title,
        [Description("Project name (optional if ADO_PROJECT is set)")] string? project = null,
        [Description("Pull request description")] string? description = null,
        [Description("Reviewer IDs")] string[]? reviewers = null)
    {
        var resolvedProject = project ?? adoOptions.Project
            ?? throw new ArgumentException("Project is required. Set ADO_PROJECT environment variable or provide the project parameter.");
        var result = await reposService.CreatePullRequestAsync(repositoryId, resolvedProject, sourceBranch, targetBranch, title, description, reviewers);
        return result.ToString();
    }
}
