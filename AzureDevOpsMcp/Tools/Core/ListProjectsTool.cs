using System.ComponentModel;
using AzureDevOpsMcp.Services;
using ModelContextProtocol.Server;

namespace AzureDevOpsMcp.Tools.Core;

/// <summary>提供列出 Azure DevOps 組織專案清單的 MCP 工具。</summary>
[McpServerToolType]
public static class ListProjectsTool
{
    /// <summary>列出 Azure DevOps 組織下所有可見的專案。</summary>
    [
        McpServerTool(Name = "mcp_ado_core_list_projects"), 
        Description("List all projects in the Azure DevOps organization")
    ]
    public static async Task<string> Execute(
        IAdoCoreService coreService,
        [Description("Filter by project state (e.g., wellFormed, createPending, deleting, new, all)")] string? stateFilter = null,
        [Description("Maximum number of projects to return")] int? top = null,
        [Description("Number of projects to skip")] int? skip = null)
    {
        var result = await coreService.ListProjectsAsync(stateFilter, top, skip);
        return result.ToString();
    }
}
