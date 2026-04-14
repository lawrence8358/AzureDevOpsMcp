using System.ComponentModel;
using System.Net;
using AzureDevOpsMcp.Configuration;
using AzureDevOpsMcp.Services;
using ModelContextProtocol.Server;

namespace AzureDevOpsMcp.Tools.Git;

/// <summary>提供從儲存庫取得指定檔案或資料夾的 MCP 工具。</summary>
[McpServerToolType]
public static class GetItemTool
{
    /// <summary>從儲存庫中取得指定路徑的檔案或資料夾內容。</summary>
    [
        McpServerTool(Name = "mcp_ado_git_get_item"), 
        Description("Read and retrieve the content of a file or directory listing from a Git repository. Use this to read source code, config files, or browse folder structure. Specify branch to read from a specific branch.")
    ]
    public static async Task<string> Execute(
        IAdoRepositoriesService reposService,
        AdoOptions adoOptions,
        [Description("Repository ID or name")] string repositoryId,
        [Description("File path in the repository")] string path,
        [Description("Project name (optional if ADO_PROJECT is set)")] string? project = null,
        [Description("Branch name")] string? branch = null)
    {
        var resolvedProject = project ?? adoOptions.Project
            ?? throw new ArgumentException("Project is required. Set ADO_PROJECT environment variable or provide the project parameter.");
        try
        {
            var result = await reposService.GetItemAsync(repositoryId, path, resolvedProject, branch);
            return result.ToString();
        }
        catch (HttpRequestException ex) when (ex.StatusCode == HttpStatusCode.NotFound
            && HttpResponseExtensions.TryGetAdoTypeKey(ex.Message) == "GitRepositoryNotFoundException")
        {
            return $"Repository '{repositoryId}' was not found in project '{resolvedProject}'. "
                 + $"Use mcp_ado_git_list_repositories with project='{resolvedProject}' to see available repositories and get the correct name or ID.";
        }
    }
}
