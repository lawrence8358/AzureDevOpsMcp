using System.Net;

namespace AzureDevOpsMcp.Tests.Helpers;

/// <summary>用於單元測試的 HTTP 訊息處理器 Mock，支援自訂回應並記錄請求詳情。</summary>
public class MockHttpMessageHandler : HttpMessageHandler
{
    private readonly Func<HttpRequestMessage, HttpResponseMessage> _handler;

    /// <summary>取得最後一次收到的 HTTP 請求訊息。</summary>
    public HttpRequestMessage? LastRequest { get; private set; }

    /// <summary>取得最後一次收到的 HTTP 請求 Body 內容字串。</summary>
    public string? LastRequestBody { get; private set; }

    /// <summary>建立使用自訂處理函式的 MockHttpMessageHandler。</summary>
    /// <param name="handler">接收請求並回傳回應的委派函式。</param>
    public MockHttpMessageHandler(Func<HttpRequestMessage, HttpResponseMessage> handler)
    {
        _handler = handler;
    }

    /// <summary>建立回傳固定 JSON 字串與指定狀態碼的 MockHttpMessageHandler。</summary>
    /// <param name="jsonResponse">要回傳的 JSON 字串。</param>
    /// <param name="statusCode">HTTP 回應狀態碼，預設為 200 OK。</param>
    public MockHttpMessageHandler(string jsonResponse, HttpStatusCode statusCode = HttpStatusCode.OK)
        : this(_ => new HttpResponseMessage(statusCode) { Content = new StringContent(jsonResponse, System.Text.Encoding.UTF8, "application/json") })
    {
    }

    /// <summary>處理 HTTP 請求，記錄請求詳情後回傳設定的回應。</summary>
    /// <param name="request">傳入的 HTTP 請求訊息。</param>
    /// <param name="cancellationToken">取消作業的語彙基元。</param>
    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        LastRequest = request;
        if (request.Content != null)
            LastRequestBody = await request.Content.ReadAsStringAsync(cancellationToken);
        return _handler(request);
    }
}
