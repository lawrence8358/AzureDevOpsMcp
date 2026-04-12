using System.ComponentModel;
using AzureDevOpsMcp.Configuration;
using AzureDevOpsMcp.Services;
using ModelContextProtocol.Server;

namespace AzureDevOpsMcp.Tools.Work;

/// <summary>提供取得指定迭代中工作項目的 MCP 工具。</summary>
[McpServerToolType]
public static class GetIterationWorkItemsTool
{
    /// <summary>取得指定迭代（Sprint）中的所有工作項目。</summary>
    [
        McpServerTool(Name = "mcp_ado_work_get_iteration_work_items"), 
        Description("Get all work items assigned to a specific iteration (sprint) by iteration ID.")
    ]
    public static async Task<string> Execute(
        IAdoWorkService workService,
        AdoOptions adoOptions,
        [Description("Iteration ID")] string iterationId,
        [Description("Project name (optional if ADO_PROJECT is set)")] string? project = null,
        [Description("Team name")] string? team = null)
    {
        var resolvedProject = project ?? adoOptions.Project
            ?? throw new ArgumentException("Project is required. Set ADO_PROJECT environment variable or provide the project parameter.");
        var result = await workService.GetIterationWorkItemsAsync(resolvedProject, iterationId, team);
        return result.ToString();
    }
}
