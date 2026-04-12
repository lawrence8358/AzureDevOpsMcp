using System.ComponentModel;
using AzureDevOpsMcp.Configuration;
using AzureDevOpsMcp.Services;
using ModelContextProtocol.Server;

namespace AzureDevOpsMcp.Tools.WorkItems;

/// <summary>提供在工作項目上新增留言的 MCP 工具。</summary>
[McpServerToolType]
public static class AddCommentTool
{
    /// <summary>在指定工作項目上新增一則留言。</summary>
    [
        McpServerTool(Name = "mcp_ado_work_items_add_comment"), 
        Description("Add a comment to a work item")
    ]
    public static async Task<string> Execute(
        IAdoWorkItemsService workItemsService,
        AdoOptions adoOptions,
        [Description("Work item ID")] int workItemId,
        [Description("Comment text")] string text,
        [Description("Project name (optional if ADO_PROJECT is set)")] string? project = null)
    {
        var resolvedProject = project ?? adoOptions.Project
            ?? throw new ArgumentException("Project is required. Set ADO_PROJECT environment variable or provide the project parameter.");
        var result = await workItemsService.AddCommentAsync(resolvedProject, workItemId, text);
        return result.ToString();
    }
}
