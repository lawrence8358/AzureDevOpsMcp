using System.ComponentModel;
using AzureDevOpsMcp.Configuration;
using AzureDevOpsMcp.Services;
using ModelContextProtocol.Server;

namespace AzureDevOpsMcp.Tools.Builds;

/// <summary>提供取得建置執行日誌的 MCP 工具。</summary>
[McpServerToolType]
public static class GetBuildLogsTool
{
    /// <summary>取得指定建置的執行日誌內容。</summary>
    [
        McpServerTool(Name = "mcp_ado_builds_get_logs"), 
        Description("Retrieve execution logs for a specific build run by build ID. Optionally specify a logId to fetch a single log section; omit to get all log entries for the run.")
    ]
    public static async Task<string> Execute(
        IAdoBuildsService buildsService,
        AdoOptions adoOptions,
        [Description("Build ID")] int buildId,
        [Description("Project name (optional if ADO_PROJECT is set)")] string? project = null,
        [Description("Specific log ID to retrieve")] int? logId = null)
    {
        var resolvedProject = project ?? adoOptions.Project
            ?? throw new ArgumentException("Project is required. Set ADO_PROJECT environment variable or provide the project parameter.");
        var result = await buildsService.GetBuildLogsAsync(resolvedProject, buildId, logId);
        return result.ToString();
    }
}
