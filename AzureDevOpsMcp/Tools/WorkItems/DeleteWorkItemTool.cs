using System.ComponentModel;
using AzureDevOpsMcp.Services;
using ModelContextProtocol.Server;

namespace AzureDevOpsMcp.Tools.WorkItems;

/// <summary>提供刪除指定工作項目的 MCP 工具。</summary>
[McpServerToolType]
public static class DeleteWorkItemTool
{
    /// <summary>刪除指定的工作項目，可選擇永久刪除或移至回收桶。</summary>
    [
        McpServerTool(Name = "mcp_ado_work_items_delete"), 
        Description("Delete a work item. By default moves it to the recycle bin (destroy=false). Set destroy=true for permanent deletion with no recovery. Use with caution.")
    ]
    public static async Task<string> Execute(
        IAdoWorkItemsService workItemsService,
        [Description("Work item ID")] int id,
        [Description("Permanently delete (true) or move to recycle bin (false)")] bool destroy = false)
    {
        var result = await workItemsService.DeleteWorkItemAsync(id, destroy);
        return result.ToString();
    }
}
