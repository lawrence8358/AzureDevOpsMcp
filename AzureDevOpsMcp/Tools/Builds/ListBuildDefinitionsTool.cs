using System.ComponentModel;
using AzureDevOpsMcp.Configuration;
using AzureDevOpsMcp.Services;
using ModelContextProtocol.Server;

namespace AzureDevOpsMcp.Tools.Builds;

/// <summary>提供列出 Azure DevOps 專案建置定義的 MCP 工具。</summary>
[McpServerToolType]
public static class ListBuildDefinitionsTool
{
    /// <summary>列出指定專案中的所有建置（Pipeline）定義。</summary>
    [
        McpServerTool(Name = "mcp_ado_builds_list_definitions"), 
        Description("List build and pipeline definitions (templates) in a project. Returns definition IDs and names used to queue a new build run.")
    ]
    public static async Task<string> Execute(
        IAdoBuildsService buildsService,
        AdoOptions adoOptions,
        [Description("Project name (optional if ADO_PROJECT is set)")] string? project = null,
        [Description("Filter by definition name")] string? name = null,
        [Description("Maximum number of results")] int? top = null)
    {
        var resolvedProject = project ?? adoOptions.Project
            ?? throw new ArgumentException("Project is required. Set ADO_PROJECT environment variable or provide the project parameter.");
        var result = await buildsService.ListDefinitionsAsync(resolvedProject, name, top);
        return result.ToString();
    }
}
