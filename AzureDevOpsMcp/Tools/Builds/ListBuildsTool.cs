using System.ComponentModel;
using AzureDevOpsMcp.Configuration;
using AzureDevOpsMcp.Services;
using ModelContextProtocol.Server;

namespace AzureDevOpsMcp.Tools.Builds;

/// <summary>提供列出 Azure DevOps 專案建置紀錄的 MCP 工具。</summary>
[McpServerToolType]
public static class ListBuildsTool
{
    /// <summary>列出指定專案中的建置執行紀錄。</summary>
    [
        McpServerTool(Name = "mcp_ado_builds_list"), 
        Description("List build run history (executions) in a project. Filter by pipeline definition ID, branch name, status (inProgress, completed), or result (succeeded, failed). Use this for past runs — not for pipeline definitions.")
    ]
    public static async Task<string> Execute(
        IAdoBuildsService buildsService,
        AdoOptions adoOptions,
        [Description("Project name (optional if ADO_PROJECT is set)")] string? project = null,
        [Description("Filter by definition ID")] int? definitionId = null,
        [Description("Filter by status (inProgress, completed, cancelling, postponed, notStarted, all)")] string? statusFilter = null,
        [Description("Filter by result (succeeded, partiallySucceeded, failed, canceled)")] string? resultFilter = null,
        [Description("Filter by branch name")] string? branchName = null,
        [Description("Maximum number of results")] int? top = null)
    {
        var resolvedProject = project ?? adoOptions.Project
            ?? throw new ArgumentException("Project is required. Set ADO_PROJECT environment variable or provide the project parameter.");
        var result = await buildsService.ListBuildsAsync(resolvedProject, definitionId, statusFilter, resultFilter, branchName, top);
        return result.ToString();
    }
}
