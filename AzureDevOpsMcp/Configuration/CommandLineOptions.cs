namespace AzureDevOpsMcp.Configuration;

/// <summary>MCP Server 的傳輸模式。</summary>
public enum TransportMode
{
    /// <summary>標準輸入/輸出模式（預設）。</summary>
    Stdio,

    /// <summary>HTTP（SSE）傳輸模式。</summary>
    Http
}

/// <summary>解析後的命令列選項，控制 MCP Server 的傳輸方式與載入的工具域。</summary>
public class CommandLineOptions
{
    #region Properties

    /// <summary>伺服器傳輸模式（Stdio 或 Http）。預設為 <see cref="TransportMode.Stdio"/>。</summary>
    public TransportMode Transport { get; init; } = TransportMode.Stdio;

    /// <summary>要載入的工具域名稱集合（core、work-items、work-items-write、git、pull-requests、builds、work）。空集合代表載入全部域。</summary>
    public HashSet<string> Domains { get; init; } = [];

    #endregion

    #region Public Methods

    /// <summary>解析命令列引數並回傳對應的 <see cref="CommandLineOptions"/> 執行個體。</summary>
    /// <param name="args">應用程式啟動時傳入的命令列引數陣列。</param>
    public static CommandLineOptions Parse(string[] args)
    {
        var transport = TransportMode.Stdio;
        var domains = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var isDomainMode = false;

        for (var i = 0; i < args.Length; i++)
        {
            switch (args[i])
            {
                case "--transport" or "-t":
                    isDomainMode = false;
                    if (i + 1 < args.Length)
                    {
                        transport = args[++i].ToLowerInvariant() switch
                        {
                            "http" => TransportMode.Http,
                            _ => TransportMode.Stdio
                        };
                    }
                    break;

                case "--domains" or "-d":
                    isDomainMode = true;
                    break;

                default:
                    if (isDomainMode && !args[i].StartsWith('-'))
                        domains.Add(args[i].ToLowerInvariant());
                    else
                        isDomainMode = false;
                    break;
            }
        }

        return new CommandLineOptions
        {
            Transport = transport,
            Domains = domains
        };
    }

    #endregion
}
