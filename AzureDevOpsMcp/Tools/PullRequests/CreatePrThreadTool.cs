using System.ComponentModel;
using AzureDevOpsMcp.Configuration;
using AzureDevOpsMcp.Services;
using ModelContextProtocol.Server;

namespace AzureDevOpsMcp.Tools.PullRequests;

/// <summary>提供在拉取請求上建立新討論串的 MCP 工具。</summary>
[McpServerToolType]
public static class CreatePrThreadTool
{
    /// <summary>在指定拉取請求上建立新的討論串或留言。</summary>
    [
        McpServerTool(Name = "mcp_ado_pr_create_thread"), 
        Description("Add a new review comment thread or discussion to a pull request. Optionally set thread status (active, fixed, wontFix, closed, byDesign, pending).")
    ]
    public static async Task<string> Execute(
        IAdoRepositoriesService reposService,
        AdoOptions adoOptions,
        [Description("Repository ID or name")] string repositoryId,
        [Description("Pull request ID")] int pullRequestId,
        [Description("Comment content")] string content,
        [Description("Project name (optional if ADO_PROJECT is set)")] string? project = null,
        [Description("Thread status (active, fixed, wontFix, closed, byDesign, pending)")] string? status = null)
    {
        var resolvedProject = project ?? adoOptions.Project
            ?? throw new ArgumentException("Project is required. Set ADO_PROJECT environment variable or provide the project parameter.");
        var result = await reposService.CreatePrThreadAsync(repositoryId, pullRequestId, resolvedProject, content, status);
        return result.ToString();
    }
}
