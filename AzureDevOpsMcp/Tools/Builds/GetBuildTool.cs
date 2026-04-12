using System.ComponentModel;
using AzureDevOpsMcp.Configuration;
using AzureDevOpsMcp.Services;
using ModelContextProtocol.Server;

namespace AzureDevOpsMcp.Tools.Builds;

/// <summary>提供取得指定建置詳情的 MCP 工具。</summary>
[McpServerToolType]
public static class GetBuildTool
{
    /// <summary>依 ID 取得指定建置的詳細資訊。</summary>
    [McpServerTool(Name = "mcp_ado_builds_get"), Description("Retrieve detailed information about a specific build run by ID, including status, result, source branch, queue time, start/finish time, and triggered parameters.")]
    public static async Task<string> Execute(
        IAdoBuildsService buildsService,
        AdoOptions adoOptions,
        [Description("Build ID")] int buildId,
        [Description("Project name (optional if ADO_PROJECT is set)")] string? project = null)
    {
        var resolvedProject = project ?? adoOptions.Project
            ?? throw new ArgumentException("Project is required. Set ADO_PROJECT environment variable or provide the project parameter.");
        var result = await buildsService.GetBuildAsync(resolvedProject, buildId);
        return result.ToString();
    }
}
