using System.ComponentModel;
using AzureDevOpsMcp.Configuration;
using AzureDevOpsMcp.Services;
using ModelContextProtocol.Server;

namespace AzureDevOpsMcp.Tools.Work;

/// <summary>提供列出 Azure DevOps 專案待辦清單的 MCP 工具。</summary>
[McpServerToolType]
public static class ListBacklogsTool
{
    /// <summary>列出指定專案與團隊的待辦清單（Backlog）。</summary>
    [
        McpServerTool(Name = "mcp_ado_work_list_backlogs"), 
        Description("List backlog levels (e.g., Epics, Features, User Stories, Tasks) and their work item counts for a team. Use this to understand backlog structure or navigate to product and sprint backlogs.")
    ]
    public static async Task<string> Execute(
        IAdoWorkService workService,
        AdoOptions adoOptions,
        [Description("Project name (optional if ADO_PROJECT is set)")] string? project = null,
        [Description("Team name")] string? team = null)
    {
        var resolvedProject = project ?? adoOptions.Project
            ?? throw new ArgumentException("Project is required. Set ADO_PROJECT environment variable or provide the project parameter.");
        var result = await workService.ListBacklogsAsync(resolvedProject, team);
        return result.ToString();
    }
}
