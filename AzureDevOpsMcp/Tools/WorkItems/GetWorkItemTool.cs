using System.ComponentModel;
using AzureDevOpsMcp.Services;
using ModelContextProtocol.Server;

namespace AzureDevOpsMcp.Tools.WorkItems;

/// <summary>提供取得指定工作項目詳情的 MCP 工具。</summary>
[McpServerToolType]
public static class GetWorkItemTool
{
    /// <summary>依 ID 取得指定工作項目的詳細資訊。</summary>
    [
        McpServerTool(Name = "mcp_ado_work_items_get"), 
        Description("Get a work item by ID")
    ]
    public static async Task<string> Execute(
        IAdoWorkItemsService workItemsService,
        [Description("Work item ID")] int id,
        [Description("Expand options (e.g., all, relations, fields)")] string? expand = null)
    {
        var result = await workItemsService.GetWorkItemAsync(id, expand);
        return result.ToString();
    }
}
