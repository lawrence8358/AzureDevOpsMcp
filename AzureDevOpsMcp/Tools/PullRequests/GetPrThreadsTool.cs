using System.ComponentModel;
using AzureDevOpsMcp.Configuration;
using AzureDevOpsMcp.Services;
using ModelContextProtocol.Server;

namespace AzureDevOpsMcp.Tools.PullRequests;

/// <summary>提供取得拉取請求討論串的 MCP 工具。</summary>
[McpServerToolType]
public static class GetPrThreadsTool
{
    /// <summary>取得指定拉取請求上的所有討論串與留言。</summary>
    [
        McpServerTool(Name = "mcp_ado_pr_get_threads"), 
        Description("Retrieve all review comment threads on a pull request, including inline code review comments and general discussion threads.")
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
        var result = await reposService.GetPrThreadsAsync(repositoryId, pullRequestId, resolvedProject);
        return result.ToString();
    }
}
