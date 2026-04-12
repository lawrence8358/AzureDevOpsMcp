using AzureDevOpsMcp.Authentication;
using AzureDevOpsMcp.Configuration;
using AzureDevOpsMcp.Services;
using AzureDevOpsMcp.Tools.Builds;
using AzureDevOpsMcp.Tools.Core;
using AzureDevOpsMcp.Tools.Git;
using AzureDevOpsMcp.Tools.PullRequests;
using AzureDevOpsMcp.Tools.Work;
using AzureDevOpsMcp.Tools.WorkItems;
using ModelContextProtocol;
using System.Text.Json;

namespace AzureDevOpsMcp.Extensions;

/// <summary>提供 Azure DevOps MCP 服務與工具的 DI 容器擴充方法。</summary>
public static class ServiceCollectionExtensions
{
    #region Members

    private static readonly Dictionary<string, Type[]> _domainTools = new(StringComparer.OrdinalIgnoreCase)
    {
        ["core"] = [typeof(ListProjectsTool)],
        ["work-items"] = [
            typeof(GetWorkItemTool), typeof(BatchGetWorkItemsTool),
            typeof(GetCommentsTool), typeof(GetUpdatesTool), typeof(QueryByWiqlTool)
        ],
        ["work-items-write"] = [
            typeof(CreateWorkItemTool), typeof(UpdateWorkItemTool), typeof(DeleteWorkItemTool),
            typeof(AddCommentTool), typeof(AddLinkTool)
        ],
        ["git"] = [
            typeof(ListRepositoriesTool), typeof(ListBranchesTool), typeof(GetItemTool),
            typeof(GetCommitsTool)
        ],
        ["pull-requests"] = [
            typeof(ListPullRequestsTool), typeof(GetPullRequestTool), typeof(CreatePullRequestTool),
            typeof(UpdatePullRequestTool), typeof(GetPrThreadsTool), typeof(CreatePrThreadTool)
        ],
        ["builds"] = [
            typeof(ListBuildDefinitionsTool), typeof(ListBuildsTool), typeof(GetBuildTool),
            typeof(QueueBuildTool), typeof(GetBuildLogsTool)
        ],
        ["work"] = [
            typeof(ListIterationsTool), typeof(GetIterationWorkItemsTool),
            typeof(ListBacklogsTool)
        ]
    };

    #endregion

    #region Public Methods

    /// <summary>向 DI 容器註冊 Azure DevOps 相關服務，包含 HttpClient 設定與各 Domain 服務。</summary>
    /// <param name="services">DI 服務集合。</param>
    /// <param name="options">命令列解析後的選項。</param>
    public static IServiceCollection AddAdoServices(
        this IServiceCollection services,
        CommandLineOptions options)
    {
        var adoOptions = new AdoOptions
        {
            ServerUrl = Environment.GetEnvironmentVariable("ADO_ORG")
                ?? throw new InvalidOperationException("ADO_ORG environment variable is required."),
            PatToken = Environment.GetEnvironmentVariable("ADO_PAT")
                ?? throw new InvalidOperationException("ADO_PAT environment variable is required."),
            Project = Environment.GetEnvironmentVariable("ADO_PROJECT")
        };

        services.AddSingleton(adoOptions);

        services.AddHttpClient("AzureDevOps", client =>
        {
            client.BaseAddress = new Uri(adoOptions.ServerUrl.TrimEnd('/') + "/");
        })
        .AddHttpMessageHandler(() => new PatAuthHandler(adoOptions.PatToken));

        var allDomains = options.Domains.Count == 0;

        if (allDomains || options.Domains.Contains("core"))
            services.AddSingleton<IAdoCoreService, AdoCoreService>();
        if (allDomains || options.Domains.Contains("work-items") || options.Domains.Contains("work-items-write"))
            services.AddSingleton<IAdoWorkItemsService, AdoWorkItemsService>();
        if (allDomains || options.Domains.Contains("git") || options.Domains.Contains("pull-requests"))
            services.AddSingleton<IAdoRepositoriesService, AdoRepositoriesService>();
        if (allDomains || options.Domains.Contains("builds"))
            services.AddSingleton<IAdoBuildsService, AdoBuildsService>();
        if (allDomains || options.Domains.Contains("work"))
            services.AddSingleton<IAdoWorkService, AdoWorkService>();

        return services;
    }

    /// <summary>依照 Domain 篩選設定，向 MCP Server Builder 註冊對應的工具類別。</summary>
    /// <param name="builder">MCP Server 建構器。</param>
    /// <param name="domains">要載入的 Domain 名稱集合。</param>
    public static IMcpServerBuilder WithAdoTools(
        this IMcpServerBuilder builder, HashSet<string> domains)
    {
        var allDomains = domains.Count == 0;
        var toolTypes = _domainTools
            .Where(kv => allDomains || domains.Contains(kv.Key))
            .SelectMany(kv => kv.Value);

        // WithTools(IEnumerable<Type>, JsonSerializerOptions) を明示的に呼ぶ。
        // JsonSerializerOptions を省略すると WithTools<T>(IMcpServerBuilder, T, JsonSerializerOptions?)
        // の型推論で T = IEnumerable<Type> になり、IEnumerable 自体をツールクラスとして
        // スキャンするため属性が見つからず tools/list ハンドラが登録されない。
        return builder.WithTools(toolTypes, (JsonSerializerOptions?)null);
    }

    #endregion
}
