using System.ComponentModel;
using AzureDevOpsMcp.Configuration;
using AzureDevOpsMcp.Services;
using ModelContextProtocol.Server;

namespace AzureDevOpsMcp.Tools.Builds;

/// <summary>提供排入新建置任務的 MCP 工具。</summary>
[McpServerToolType]
public static class QueueBuildTool
{
    /// <summary>依指定的建置定義排入新的建置任務。</summary>
    [
        McpServerTool(Name = "mcp_ado_builds_queue"), 
        Description("Trigger and queue a new build (run a pipeline). Requires a pipeline definition ID; optionally specify a source branch and JSON parameters. Also described as: run pipeline, start build, kick off build.")
    ]
    public static async Task<string> Execute(
        IAdoBuildsService buildsService,
        AdoOptions adoOptions,
        [Description("Build definition ID")] int definitionId,
        [Description("Project name (optional if ADO_PROJECT is set)")] string? project = null,
        [Description("Source branch")] string? sourceBranch = null,
        [Description("Build parameters as JSON string")] string? parameters = null)
    {
        var resolvedProject = project ?? adoOptions.Project
            ?? throw new ArgumentException("Project is required. Set ADO_PROJECT environment variable or provide the project parameter.");
        var result = await buildsService.QueueBuildAsync(resolvedProject, definitionId, sourceBranch, parameters);
        return result.ToString();
    }
}
