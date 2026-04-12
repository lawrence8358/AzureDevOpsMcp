using System.ComponentModel;
using AzureDevOpsMcp.Services;
using ModelContextProtocol.Server;

namespace AzureDevOpsMcp.Tools.WorkItems;

/// <summary>提供在工作項目之間建立連結的 MCP 工具。</summary>
[McpServerToolType]
public static class AddLinkTool
{
    /// <summary>在兩個工作項目之間建立指定類型的連結。</summary>
    [
        McpServerTool(Name = "mcp_ado_work_items_add_link"), 
        Description("Add a relationship link between two work items. Common link types: System.LinkTypes.Related, System.LinkTypes.Hierarchy-Forward (child), System.LinkTypes.Hierarchy-Reverse (parent), System.LinkTypes.Dependency-Forward, System.LinkTypes.Dependency-Reverse.")
    ]
    public static async Task<string> Execute(
        IAdoWorkItemsService workItemsService,
        [Description("Source work item ID")] int id,
        [Description("Target work item ID")] int targetId,
        [Description("Link type (e.g., System.LinkTypes.Hierarchy-Forward)")] string linkType,
        [Description("Optional comment for the link")] string? comment = null)
    {
        var result = await workItemsService.AddLinkAsync(id, targetId, linkType, comment);
        return result.ToString();
    }
}
