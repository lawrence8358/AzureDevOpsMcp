namespace AzureDevOpsMcp.Services;

/// <summary>提供 <see cref="HttpResponseMessage"/> 的擴充方法。</summary>
public static class HttpResponseExtensions
{
    /// <summary>確認 HTTP 回應成功，失敗時將 Azure DevOps API 的回應內容一併附加至例外訊息。</summary>
    /// <param name="response">要確認的 HTTP 回應訊息。</param>
    public static async Task EnsureSuccessWithBodyAsync(this HttpResponseMessage response)
    {
        if (!response.IsSuccessStatusCode)
        {
            var body = await response.Content.ReadAsStringAsync();
            throw new HttpRequestException(
                $"Azure DevOps API request failed with status {(int)response.StatusCode} ({response.StatusCode}). Response: {body}",
                null,
                response.StatusCode);
        }
    }
}
