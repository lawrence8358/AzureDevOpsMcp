using System.ComponentModel;
using AzureDevOpsMcp.Services;
using ModelContextProtocol.Server;

namespace AzureDevOpsMcp.Tools.WorkItems;

/// <summary>提供取得工作項目歷史變更紀錄的 MCP 工具。</summary>
[McpServerToolType]
public static class GetUpdatesTool
{
    /// <summary>取得指定工作項目的所有歷史更新紀錄。</summary>
    [
        McpServerTool(Name = "mcp_ado_work_items_get_updates"), 
        Description("Get the update history and change log for a work item, showing who changed what and when.")
    ]
    public static async Task<string> Execute(
        IAdoWorkItemsService workItemsService,
        [Description("Work item ID")] int id,
        [Description("Maximum number of updates to return")] int? top = null)
    {
        var result = await workItemsService.GetUpdatesAsync(id, top);
        return result.ToString();
    }
}
