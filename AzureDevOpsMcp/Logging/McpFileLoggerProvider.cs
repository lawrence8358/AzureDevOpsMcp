using Microsoft.Extensions.Logging;

namespace AzureDevOpsMcp.Logging;

/// <summary>
/// 為 MCP STDIO 模式提供檔案式日誌記錄的 ILoggerProvider。
/// STDIO 模式下 stdout 被 MCP JSON-RPC 協議占用，因此所有日誌必須寫入檔案（及 stderr）。
/// </summary>
public sealed class McpFileLoggerProvider : ILoggerProvider
{
    #region Members

    private readonly StreamWriter _writer;
    private readonly LogLevel _minimumLevel;

    #endregion

    #region Constructors

    /// <summary>初始化 <see cref="McpFileLoggerProvider"/> 的新執行個體。</summary>
    /// <param name="logFilePath">日誌檔案完整路徑；目錄不存在時將自動建立。</param>
    /// <param name="minimumLevel">最低記錄等級，預設為 <see cref="LogLevel.Debug"/>。</param>
    public McpFileLoggerProvider(string logFilePath, LogLevel minimumLevel = LogLevel.Debug)
    {
        _minimumLevel = minimumLevel;
        Directory.CreateDirectory(Path.GetDirectoryName(Path.GetFullPath(logFilePath))!);
        _writer = new StreamWriter(logFilePath, append: true, System.Text.Encoding.UTF8) { AutoFlush = true };
        _writer.WriteLine($"{'=',-70}");
        _writer.WriteLine($"=== MCP Server Started  {DateTime.Now:yyyy-MM-dd HH:mm:ss}                          ===");
        _writer.WriteLine($"{'=',-70}");
    }

    #endregion

    #region Public Methods

    /// <summary>建立指定類別名稱的 Logger 執行個體。</summary>
    /// <param name="categoryName">Logger 類別名稱（通常為完整類別型別名稱）。</param>
    public ILogger CreateLogger(string categoryName) =>
        new McpFileLogger(categoryName, _writer, _minimumLevel);

    /// <summary>釋放 StreamWriter 資源並寫入結束標記。</summary>
    public void Dispose()
    {
        try
        {
            _writer.WriteLine($"=== MCP Server Stopped  {DateTime.Now:yyyy-MM-dd HH:mm:ss}                          ===");
            _writer.Flush();
        }
        catch { /* ignore */ }
        finally
        {
            _writer.Dispose();
        }
    }

    #endregion
}

/// <summary>將訊息寫入 StreamWriter（檔案）的 ILogger 實作，同時將 Warning 以上等級輸出至 stderr。</summary>
internal sealed class McpFileLogger : ILogger
{
    #region Members

    private static readonly object _syncRoot = new();

    private readonly string _categoryName;
    private readonly StreamWriter _writer;
    private readonly LogLevel _minimumLevel;

    #endregion

    #region Constructors

    internal McpFileLogger(string categoryName, StreamWriter writer, LogLevel minimumLevel)
    {
        _categoryName = categoryName;
        _writer = writer;
        _minimumLevel = minimumLevel;
    }

    #endregion

    #region Public Methods

    /// <summary>開始新的 Log 範圍（目前不支援，回傳 null）。</summary>
    public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;

    /// <summary>判斷指定記錄等級是否已啟用。</summary>
    /// <param name="logLevel">要判斷的記錄等級。</param>
    public bool IsEnabled(LogLevel logLevel) => logLevel >= _minimumLevel;

    /// <summary>記錄一條日誌訊息至檔案，並在等級 ≥ Warning 時同步輸出至 stderr。</summary>
    public void Log<TState>(
        LogLevel logLevel,
        EventId eventId,
        TState state,
        Exception? exception,
        Func<TState, Exception?, string> formatter)
    {
        if (!IsEnabled(logLevel)) return;

        var levelLabel = logLevel switch
        {
            LogLevel.Trace => "TRC",
            LogLevel.Debug => "DBG",
            LogLevel.Information => "INF",
            LogLevel.Warning => "WRN",
            LogLevel.Error => "ERR",
            LogLevel.Critical => "CRT",
            _ => "???"
        };

        // 只取最後一段類別名稱以保持日誌可讀性
        var shortCategory = _categoryName.Contains('.')
            ? _categoryName[(_categoryName.LastIndexOf('.') + 1)..]
            : _categoryName;

        var message = formatter(state, exception);
        var line = $"[{DateTime.Now:HH:mm:ss.fff}] [{levelLabel}] [{shortCategory}] {message}";

        lock (_syncRoot)
        {
            _writer.WriteLine(line);
            if (exception != null)
                _writer.WriteLine($"           {exception}");
        }

        // Warning 以上同步輸出到 stderr，讓 VS Code Output 面板可立即看到
        if (logLevel >= LogLevel.Warning)
        {
            Console.Error.WriteLine(line);
            if (exception != null)
                Console.Error.WriteLine($"           {exception}");
        }
    }

    #endregion
}
