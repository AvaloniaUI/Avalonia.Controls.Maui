using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace Avalonia.Controls.Maui.Handlers;

internal interface IWebViewCookieStore
{
    void AddOrUpdateCookie(Cookie cookie);

    void DeleteCookie(string name, string domain, string path);

    Task<IReadOnlyList<Cookie>> GetCookiesAsync();
}

internal static class WebViewCookieSynchronizer
{
    public static async Task SyncToPlatformAsync(IWebViewCookieStore cookieStore, CookieContainer? cookieContainer, Uri? uri)
    {
        if (!CanSync(uri) || cookieContainer is null)
            return;

        var desiredCookies = cookieContainer.GetCookies(uri!)
            .Cast<Cookie>()
            .Select(CloneCookie)
            .ToList();

        var existingCookies = FilterForUri(await cookieStore.GetCookiesAsync().ConfigureAwait(false), uri!).ToList();

        foreach (var cookie in desiredCookies)
            cookieStore.AddOrUpdateCookie(cookie);

        foreach (var cookie in existingCookies)
        {
            if (!desiredCookies.Any(candidate => CookieIdentityComparer.Equals(candidate, cookie)))
                cookieStore.DeleteCookie(cookie.Name, cookie.Domain, NormalizePath(cookie.Path));
        }
    }

    public static async Task SyncToVirtualViewAsync(IWebViewCookieStore cookieStore, CookieContainer? cookieContainer, Uri? uri)
    {
        if (!CanSync(uri) || cookieContainer is null)
            return;

        var virtualCookies = cookieContainer.GetCookies(uri!)
            .Cast<Cookie>()
            .ToList();

        var platformCookies = FilterForUri(await cookieStore.GetCookiesAsync().ConfigureAwait(false), uri!)
            .Select(CloneCookie)
            .ToList();

        foreach (var cookie in virtualCookies)
        {
            var platformCookie = platformCookies.FirstOrDefault(candidate => CookieIdentityComparer.Equals(candidate, cookie));
            if (platformCookie is null)
            {
                cookie.Expired = true;
                continue;
            }

            cookie.Value = platformCookie.Value;
            cookie.Expires = platformCookie.Expires;
            cookie.HttpOnly = platformCookie.HttpOnly;
            cookie.Secure = platformCookie.Secure;
        }

        foreach (var cookie in platformCookies)
        {
            if (virtualCookies.Any(candidate => CookieIdentityComparer.Equals(candidate, cookie)))
                continue;

            cookieContainer.Add(CloneCookie(cookie));
        }
    }

    private static bool CanSync(Uri? uri)
    {
        return uri is not null &&
               (uri.Scheme.Equals(Uri.UriSchemeHttp, StringComparison.OrdinalIgnoreCase) ||
                uri.Scheme.Equals(Uri.UriSchemeHttps, StringComparison.OrdinalIgnoreCase));
    }

    private static IEnumerable<Cookie> FilterForUri(IEnumerable<Cookie> cookies, Uri uri)
    {
        foreach (var cookie in cookies)
        {
            if (DomainMatches(uri.Host, cookie.Domain) && PathMatches(uri.AbsolutePath, cookie.Path))
                yield return cookie;
        }
    }

    private static bool DomainMatches(string host, string domain)
    {
        if (string.IsNullOrWhiteSpace(domain))
            return false;

        var normalizedDomain = domain.TrimStart('.').ToLowerInvariant();
        var normalizedHost = host.ToLowerInvariant();

        return normalizedHost.Equals(normalizedDomain, StringComparison.OrdinalIgnoreCase) ||
               normalizedHost.EndsWith("." + normalizedDomain, StringComparison.OrdinalIgnoreCase);
    }

    private static bool PathMatches(string requestPath, string? cookiePath)
    {
        var normalizedRequestPath = string.IsNullOrEmpty(requestPath) ? "/" : requestPath;
        return normalizedRequestPath.StartsWith(NormalizePath(cookiePath), StringComparison.Ordinal);
    }

    private static string NormalizePath(string? path)
    {
        if (string.IsNullOrWhiteSpace(path))
            return "/";

        return path.StartsWith("/", StringComparison.Ordinal) ? path : "/" + path;
    }

    private static Cookie CloneCookie(Cookie cookie)
    {
        return new Cookie(cookie.Name, cookie.Value, NormalizePath(cookie.Path), cookie.Domain)
        {
            Comment = cookie.Comment,
            CommentUri = cookie.CommentUri,
            Discard = cookie.Discard,
            Expired = cookie.Expired,
            Expires = cookie.Expires,
            HttpOnly = cookie.HttpOnly,
            Secure = cookie.Secure,
            Version = cookie.Version,
        };
    }

    private static class CookieIdentityComparer
    {
        public static bool Equals(Cookie x, Cookie y)
        {
            return x.Name.Equals(y.Name, StringComparison.Ordinal) &&
                   string.Equals(x.Domain, y.Domain, StringComparison.OrdinalIgnoreCase) &&
                   string.Equals(NormalizePath(x.Path), NormalizePath(y.Path), StringComparison.Ordinal);
        }
    }
}
