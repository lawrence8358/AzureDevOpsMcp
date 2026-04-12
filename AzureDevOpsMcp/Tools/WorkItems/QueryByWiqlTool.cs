using System.ComponentModel;
using AzureDevOpsMcp.Configuration;
using AzureDevOpsMcp.Services;
using ModelContextProtocol.Server;

namespace AzureDevOpsMcp.Tools.WorkItems;

/// <summary>提供以 WIQL 語法查詢工作項目的 MCP 工具。</summary>
[McpServerToolType]
public static class QueryByWiqlTool
{
    /// <summary>使用 WIQL 查詢語法搜尋符合條件的工作項目。</summary>
    [
        McpServerTool(Name = "mcp_ado_work_items_query_by_wiql"), 
        Description("Search and query work items using WIQL (Work Item Query Language — similar to SQL). Use this to find work items by state, type, assignee, area path, iteration, or any field. Example: SELECT [System.Id] FROM WorkItems WHERE [System.State] = 'Active'")
    ]
    public static async Task<string> Execute(
        IAdoWorkItemsService workItemsService,
        AdoOptions adoOptions,
        [Description("WIQL query string")] string query,
        [Description("Project name (optional if ADO_PROJECT is set)")] string? project = null,
        [Description("Maximum number of results")] int? top = null)
    {
        var resolvedProject = project ?? adoOptions.Project
            ?? throw new ArgumentException("Project is required. Set ADO_PROJECT environment variable or provide the project parameter.");
        var result = await workItemsService.QueryByWiqlAsync(resolvedProject, query, top);
        return result.ToString();
    }
}
