using System.ComponentModel;
using AzureDevOpsMcp.Configuration;
using AzureDevOpsMcp.Services;
using ModelContextProtocol.Server;

namespace AzureDevOpsMcp.Tools.Git;

/// <summary>提供取得儲存庫提交紀錄的 MCP 工具。</summary>
[McpServerToolType]
public static class GetCommitsTool
{
    /// <summary>取得指定儲存庫的提交歷史紀錄。</summary>
    [
        McpServerTool(Name = "mcp_ado_git_get_commits"), 
        Description("Retrieve commit history from a Git repository. Filter by branch, file path (itemPath), author name or email, and limit count with top.")
    ]
    public static async Task<string> Execute(
        IAdoRepositoriesService reposService,
        AdoOptions adoOptions,
        [Description("Repository ID or name")] string repositoryId,
        [Description("Project name (optional if ADO_PROJECT is set)")] string? project = null,
        [Description("Branch name")] string? branch = null,
        [Description("Filter by item path")] string? itemPath = null,
        [Description("Filter by author")] string? author = null,
        [Description("Maximum number of commits")] int? top = null)
    {
        var resolvedProject = project ?? adoOptions.Project
            ?? throw new ArgumentException("Project is required. Set ADO_PROJECT environment variable or provide the project parameter.");
        var result = await reposService.GetCommitsAsync(repositoryId, resolvedProject, branch, itemPath, author, top);
        return result.ToString();
    }
}
