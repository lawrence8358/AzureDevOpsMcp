using System.ComponentModel;
using AzureDevOpsMcp.Configuration;
using AzureDevOpsMcp.Services;
using ModelContextProtocol.Server;

namespace AzureDevOpsMcp.Tools.Git;

/// <summary>提供列出儲存庫分支清單的 MCP 工具。</summary>
[McpServerToolType]
public static class ListBranchesTool
{
    /// <summary>列出指定儲存庫中的所有分支。</summary>
    [
        McpServerTool(Name = "mcp_ado_git_list_branches"), 
        Description("List branches in a Git repository. Optionally filter by branch name prefix.")
    ]
    public static async Task<string> Execute(
        IAdoRepositoriesService reposService,
        AdoOptions adoOptions,
        [Description("Repository ID or name")] string repositoryId,
        [Description("Project name (optional if ADO_PROJECT is set)")] string? project = null,
        [Description("Branch name filter")] string? filter = null)
    {
        var resolvedProject = project ?? adoOptions.Project
            ?? throw new ArgumentException("Project is required. Set ADO_PROJECT environment variable or provide the project parameter.");
        var result = await reposService.ListBranchesAsync(repositoryId, resolvedProject, filter);
        return result.ToString();
    }
}
