using System.ComponentModel;
using AzureDevOpsMcp.Configuration;
using AzureDevOpsMcp.Services;
using ModelContextProtocol.Server;

namespace AzureDevOpsMcp.Tools.Git;

/// <summary>提供列出 Azure DevOps 專案中所有儲存庫的 MCP 工具。</summary>
[McpServerToolType]
public static class ListRepositoriesTool
{
    /// <summary>列出指定專案下的所有 Git 儲存庫。</summary>
    [
        McpServerTool(Name = "mcp_ado_git_list_repositories"), 
        Description("List all Git repositories in a project.")
    ]
    public static async Task<string> Execute(
        IAdoRepositoriesService reposService,
        AdoOptions adoOptions,
        [Description("Project name (optional if ADO_PROJECT is set)")] string? project = null)
    {
        var resolvedProject = project ?? adoOptions.Project
            ?? throw new ArgumentException("Project is required. Set ADO_PROJECT environment variable or provide the project parameter.");
        var result = await reposService.ListRepositoriesAsync(resolvedProject);
        return result.ToString();
    }
}
