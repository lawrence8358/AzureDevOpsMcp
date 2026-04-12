using System.ComponentModel;
using AzureDevOpsMcp.Services;
using ModelContextProtocol.Server;

namespace AzureDevOpsMcp.Tools.WorkItems;

/// <summary>提供批次取得多個工作項目的 MCP 工具。</summary>
[McpServerToolType]
public static class BatchGetWorkItemsTool
{
    /// <summary>依多個 ID 批次取得工作項目。</summary>
    [
        McpServerTool(Name = "mcp_ado_work_items_batch_get"), 
        Description("Bulk retrieve multiple work items by a list of IDs in a single call. More efficient than fetching each item individually.")
    ]
    public static async Task<string> Execute(
        IAdoWorkItemsService workItemsService,
        [Description("Array of work item IDs")] int[] ids,
        [Description("Expand options")] string? expand = null)
    {
        var result = await workItemsService.BatchGetWorkItemsAsync(ids, expand);
        return result.ToString();
    }
}
