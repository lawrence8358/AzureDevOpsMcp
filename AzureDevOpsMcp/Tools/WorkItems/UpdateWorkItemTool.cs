using System.ComponentModel;
using AzureDevOpsMcp.Services;
using ModelContextProtocol.Server;

namespace AzureDevOpsMcp.Tools.WorkItems;

/// <summary>提供更新現有工作項目欄位的 MCP 工具。</summary>
[McpServerToolType]
public static class UpdateWorkItemTool
{
    /// <summary>更新指定工作項目的一或多個欄位。</summary>
    [
        McpServerTool(Name = "mcp_ado_work_items_update"), 
        Description("Update fields of an existing work item by providing field key-value pairs (e.g., title, state, assignedTo, priority, description). Pass a fields object with System field names or friendly names as keys.")
    ]
    public static async Task<string> Execute(
        IAdoWorkItemsService workItemsService,
        [Description("Work item ID")] int id,
        [Description("Fields to update as key-value pairs")] Dictionary<string, object> fields)
    {
        var result = await workItemsService.UpdateWorkItemAsync(id, fields);
        return result.ToString();
    }
}
