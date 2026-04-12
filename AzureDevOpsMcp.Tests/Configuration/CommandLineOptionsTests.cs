using AzureDevOpsMcp.Configuration;

namespace AzureDevOpsMcp.Tests.Configuration;

public class CommandLineOptionsTests
{
    [Fact(DisplayName = "解析命令列參數（無引數）- 預設使用 STDIO 模式並載入全部 Domain")]
    public void Parse_NoArgs_DefaultsToStdioAllDomains()
    {
        // Arrange & Act
        var options = CommandLineOptions.Parse([]);

        // Assert
        Assert.Equal(TransportMode.Stdio, options.Transport);
        Assert.Empty(options.Domains);
    }

    [Fact(DisplayName = "解析命令列參數（--transport http）- 使用 HTTP 模式")]
    public void Parse_HttpTransport()
    {
        // Arrange & Act
        var options = CommandLineOptions.Parse(["--transport", "http"]);

        // Assert
        Assert.Equal(TransportMode.Http, options.Transport);
    }

    [Fact(DisplayName = "解析命令列參數（--transport stdio）- 使用 STDIO 模式")]
    public void Parse_StdioExplicit()
    {
        // Arrange & Act
        var options = CommandLineOptions.Parse(["--transport", "stdio"]);

        // Assert
        Assert.Equal(TransportMode.Stdio, options.Transport);
    }

    [Fact(DisplayName = "解析命令列參數（單一 Domain）- 正確載入指定 Domain")]
    public void Parse_SingleDomain()
    {
        // Arrange & Act
        var options = CommandLineOptions.Parse(["--domains", "core"]);

        // Assert
        Assert.Single(options.Domains);
        Assert.Contains("core", options.Domains);
    }

    [Fact(DisplayName = "解析命令列參數（多個 Domain）- 正確載入所有指定 Domain")]
    public void Parse_MultipleDomains()
    {
        // Arrange & Act
        var options = CommandLineOptions.Parse(["--domains", "core", "work", "git"]);

        // Assert
        Assert.Equal(3, options.Domains.Count);
        Assert.Contains("core", options.Domains);
        Assert.Contains("work", options.Domains);
        Assert.Contains("git", options.Domains);
    }

    [Fact(DisplayName = "解析命令列參數（Transport 與 Domain 組合）- 正確解析所有選項")]
    public void Parse_CombinedTransportAndDomains()
    {
        // Arrange & Act
        var options = CommandLineOptions.Parse(["-t", "http", "-d", "builds", "workitems"]);

        // Assert
        Assert.Equal(TransportMode.Http, options.Transport);
        Assert.Equal(2, options.Domains.Count);
        Assert.Contains("builds", options.Domains);
        Assert.Contains("workitems", options.Domains);
    }

    [Fact(DisplayName = "解析命令列參數（大小寫混合 Domain）- 正規化為小寫")]
    public void Parse_DomainsCaseInsensitive()
    {
        // Arrange & Act
        var options = CommandLineOptions.Parse(["--domains", "Core", "WORK"]);

        // Assert
        Assert.Contains("core", options.Domains);
        Assert.Contains("work", options.Domains);
    }
}
