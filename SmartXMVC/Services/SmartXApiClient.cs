using System.Net.Http.Headers;
namespace SmartXMVC.Services;

/// <summary>CLIENT-01: async streaming API transport managed by IHttpClientFactory (Microsoft, 2026e).</summary>
public sealed class SmartXApiClient(HttpClient http)
{
    /// <summary>CLIENT-02: forward a validated relative route and stream the response without whole-body buffering.</summary>
    public Task<HttpResponseMessage> SendAsync(string path, HttpRequest request, CancellationToken token)
    {
        var message = new HttpRequestMessage(new HttpMethod(request.Method), "api/" + path + request.QueryString);
        if (request.Method is "POST" or "PUT")
        {
            message.Content = new StreamContent(request.Body);
            if (!string.IsNullOrEmpty(request.ContentType)) message.Content.Headers.ContentType = MediaTypeHeaderValue.Parse(request.ContentType);
        }
        return SendAndDisposeAsync(message, token);
    }
    /// <summary>CLIENT-03: dispose the outbound request after headers arrive; caller owns the returned response.</summary>
    private async Task<HttpResponseMessage> SendAndDisposeAsync(HttpRequestMessage message, CancellationToken token)
    {
        using (message) return await http.SendAsync(message, HttpCompletionOption.ResponseHeadersRead, token);
    }
}
