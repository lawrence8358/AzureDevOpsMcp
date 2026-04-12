using System.ComponentModel;
using AzureDevOpsMcp.Configuration;
using AzureDevOpsMcp.Services;
using ModelContextProtocol.Server;

namespace AzureDevOpsMcp.Tools.WorkItems;

/// <summary>提供取得工作項目留言清單的 MCP 工具。</summary>
[McpServerToolType]
public static class GetCommentsTool
{
    /// <summary>取得指定工作項目的所有留言。</summary>
    [
        McpServerTool(Name = "mcp_ado_work_items_get_comments"), 
        Description("Get all comments on a work item.")
    ]
    public static async Task<string> Execute(
        IAdoWorkItemsService workItemsService,
        AdoOptions adoOptions,
        [Description("Work item ID")] int workItemId,
        [Description("Project name (optional if ADO_PROJECT is set)")] string? project = null,
        [Description("Maximum number of comments to return")] int? top = null)
    {
        var resolvedProject = project ?? adoOptions.Project
            ?? throw new ArgumentException("Project is required. Set ADO_PROJECT environment variable or provide the project parameter.");
        var result = await workItemsService.GetCommentsAsync(resolvedProject, workItemId, top);
        return result.ToString();
    }
}
