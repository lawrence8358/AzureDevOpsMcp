using AzureDevOpsMcp.Configuration;
using AzureDevOpsMcp.Extensions;
using AzureDevOpsMcp.Logging;
using Microsoft.Extensions.Logging;
using ModelContextProtocol;

var options = CommandLineOptions.Parse(args);

if (options.Transport == TransportMode.Http)
{
    var builder = WebApplication.CreateBuilder(args);
    builder.Services.AddAdoServices(options);
    builder.Services
        .AddMcpServer()
        .WithHttpTransport()
        .WithAdoTools(options.Domains);

    var app = builder.Build();
    app.MapGet("/health", () => Results.Ok(new { status = "healthy", timestamp = DateTime.UtcNow }));
    app.MapMcp();
    app.Run();
}
else
{
    // STDIO 模式：stdout 由 MCP JSON-RPC 協議占用，所有日誌必須寫入檔案（及 stderr）。
    // 日誌位置：<執行目錄>/logs/mcp-YYYYMMDD.log
    var logFile = Path.Combine(AppContext.BaseDirectory, "logs", $"mcp-{DateTime.Now:yyyyMMdd}.log");

    try
    {
        var builder = Host.CreateEmptyApplicationBuilder(settings: null);

        builder.Logging.ClearProviders();
        builder.Logging.AddProvider(new McpFileLoggerProvider(logFile));
        builder.Logging.SetMinimumLevel(LogLevel.Debug);

        builder.Services.AddAdoServices(options);
        builder.Services
            .AddMcpServer()
            .WithStdioServerTransport()
            .WithAdoTools(options.Domains);

        var host = builder.Build();

        var logger = host.Services.GetRequiredService<ILoggerFactory>()
            .CreateLogger("Startup");
        var domainList = options.Domains.Count > 0
            ? string.Join(", ", options.Domains)
            : "all (core, work-items, work-items-write, git, pull-requests, builds, work)";
        logger.LogInformation("MCP Server 啟動中 | Transport: STDIO | Domains: {Domains}", domainList);
        logger.LogInformation("ADO_ORG: {Url}", Environment.GetEnvironmentVariable("ADO_ORG") ?? "(未設定)");
        logger.LogInformation("ADO_PROJECT: {Project}", Environment.GetEnvironmentVariable("ADO_PROJECT") ?? "(未設定)");
        logger.LogInformation("Log 檔案位置: {LogFile}", logFile);

        await host.RunAsync();
    }
    catch (Exception ex)
    {
        // 將啟動錯誤同時寫入 stderr（VS Code 可立即顯示）與日誌檔案
        var msg = $"[{DateTime.Now:HH:mm:ss}] [CRT] MCP Server 啟動失敗：{ex}";
        Console.Error.WriteLine(msg);
        try
        {
            Directory.CreateDirectory(Path.GetDirectoryName(logFile)!);
            await File.AppendAllTextAsync(logFile, msg + Environment.NewLine);
        }
        catch { /* 若連日誌都無法寫入則放棄 */ }

        Environment.Exit(1);
    }
}
