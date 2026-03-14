namespace MyConference.Services;

/// <summary>
/// An HTTP handler that routes requests through corsproxy.io to bypass CORS restrictions
/// in the browser environment.
/// </summary>
public class CorsProxyHandler : HttpClientHandler
{
    private const string CorsProxyBase = "https://corsproxy.io/?url=";

    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        if (request.RequestUri is not null)
        {
            var proxiedUrl = CorsProxyBase + Uri.EscapeDataString(request.RequestUri.ToString());
            request.RequestUri = new Uri(proxiedUrl);
        }

        return base.SendAsync(request, cancellationToken);
    }
}
