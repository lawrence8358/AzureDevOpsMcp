using System.Net.Http.Headers;
using System.Text;

namespace AzureDevOpsMcp.Authentication;

/// <summary>為 HTTP 請求加入 Azure DevOps PAT（個人存取權杖）認證標頭的委派處理器。</summary>
public class PatAuthHandler : DelegatingHandler
{
    #region Members

    private readonly string _credentials;

    #endregion

    #region Constructors

    /// <summary>初始化 <see cref="PatAuthHandler"/> 的新執行個體。</summary>
    /// <param name="patToken">Azure DevOps 個人存取權杖（PAT）。</param>
    /// <exception cref="ArgumentException">當 <paramref name="patToken"/> 為 null 或空白時擲出。</exception>
    public PatAuthHandler(string patToken)
    {
        if (string.IsNullOrWhiteSpace(patToken))
            throw new ArgumentException("PAT token cannot be null or empty.", nameof(patToken));

        _credentials = Convert.ToBase64String(Encoding.ASCII.GetBytes($":{patToken}"));
    }

    #endregion

    #region Protected Methods

    /// <summary>傳送 HTTP 請求前自動加入 Basic 認證標頭。</summary>
    /// <param name="request">即將傳送的 HTTP 請求訊息。</param>
    /// <param name="cancellationToken">取消作業的語彙基元。</param>
    protected override Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request, CancellationToken cancellationToken)
    {
        request.Headers.Authorization = new AuthenticationHeaderValue("Basic", _credentials);
        return base.SendAsync(request, cancellationToken);
    }

    #endregion
}
