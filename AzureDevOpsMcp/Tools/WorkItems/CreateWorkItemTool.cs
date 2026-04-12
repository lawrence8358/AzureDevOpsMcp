using System.ComponentModel;
using AzureDevOpsMcp.Configuration;
using AzureDevOpsMcp.Services;
using ModelContextProtocol.Server;

namespace AzureDevOpsMcp.Tools.WorkItems;

/// <summary>提供在 Azure DevOps 專案中建立新工作項目的 MCP 工具。</summary>
[McpServerToolType]
public static class CreateWorkItemTool
{
    /// <summary>在指定專案中建立新的工作項目。</summary>
    [
        McpServerTool(Name = "mcp_ado_work_items_create"), 
        Description("Create a new work item. Supported types include: Bug, Task, User Story, Feature, Epic, Issue, and Test Case.")
    ]
    public static async Task<string> Execute(
        IAdoWorkItemsService workItemsService,
        AdoOptions adoOptions,
        [Description("Work item type (e.g., Bug, Task, User Story)")] string type,
        [Description("Work item title")] string title,
        [Description("Project name (optional if ADO_PROJECT is set)")] string? project = null,
        [Description("Work item description")] string? description = null,
        [Description("Assigned to user")] string? assignedTo = null,
        [Description("Area path")] string? areaPath = null,
        [Description("Iteration path")] string? iterationPath = null)
    {
        var resolvedProject = project ?? adoOptions.Project
            ?? throw new ArgumentException("Project is required. Set ADO_PROJECT environment variable or provide the project parameter.");
        var result = await workItemsService.CreateWorkItemAsync(resolvedProject, type, title, description, assignedTo, areaPath, iterationPath);
        return result.ToString();
    }
}
