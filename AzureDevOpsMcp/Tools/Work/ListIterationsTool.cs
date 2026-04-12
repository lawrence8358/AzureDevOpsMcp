using System.ComponentModel;
using AzureDevOpsMcp.Configuration;
using AzureDevOpsMcp.Services;
using ModelContextProtocol.Server;

namespace AzureDevOpsMcp.Tools.Work;

/// <summary>提供列出 Azure DevOps 專案迭代（Sprint）的 MCP 工具。</summary>
[McpServerToolType]
public static class ListIterationsTool
{
    /// <summary>列出指定專案與團隊的所有迭代（Sprint）。</summary>
    [
        McpServerTool(Name = "mcp_ado_work_list_iterations"), 
        Description("List iterations (sprints) for a team in a project. Filter by timeframe (past, current, future) to find the currently active sprint or upcoming ones.")
    ]
    public static async Task<string> Execute(
        IAdoWorkService workService,
        AdoOptions adoOptions,
        [Description("Project name (optional if ADO_PROJECT is set)")] string? project = null,
        [Description("Team name")] string? team = null,
        [Description("Timeframe filter (past, current, future)")] string? timeframe = null)
    {
        var resolvedProject = project ?? adoOptions.Project
            ?? throw new ArgumentException("Project is required. Set ADO_PROJECT environment variable or provide the project parameter.");
        var result = await workService.ListIterationsAsync(resolvedProject, team, timeframe);
        return result.ToString();
    }
}
