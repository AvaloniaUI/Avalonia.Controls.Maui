using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Avalonia.Controls.Maui.Handlers;
using Avalonia.Headless.XUnit;

namespace Avalonia.Controls.Maui.Tests.Handlers;

public class WebViewCookieSynchronizerTests
{
    [AvaloniaFact(DisplayName = "SyncToPlatform adds and removes cookies for the current uri")]
    public async Task SyncToPlatformUpdatesCookieStore()
    {
        var uri = new Uri("https://httpbin.org/cookies");
        var cookieContainer = new CookieContainer();
        cookieContainer.Add(new Cookie("MauiCookie", "Hmmm Cookies!", "/", "httpbin.org"));

        var cookieStore = new FakeCookieStore(new[]
        {
            new Cookie("StaleCookie", "old", "/", "httpbin.org")
        });

        await WebViewCookieSynchronizer.SyncToPlatformAsync(cookieStore, cookieContainer, uri);

        Assert.Contains(cookieStore.Cookies, cookie => cookie.Name == "MauiCookie" && cookie.Value == "Hmmm Cookies!");
        Assert.DoesNotContain(cookieStore.Cookies, cookie => cookie.Name == "StaleCookie");
    }

    [AvaloniaFact(DisplayName = "SyncToVirtualView updates the MAUI CookieContainer from platform cookies")]
    public async Task SyncToVirtualViewUpdatesCookieContainer()
    {
        var uri = new Uri("https://httpbin.org/cookies");
        var cookieContainer = new CookieContainer();
        cookieContainer.Add(new Cookie("MauiCookie", "old-value", "/", "httpbin.org"));

        var cookieStore = new FakeCookieStore(new[]
        {
            new Cookie("MauiCookie", "new-value", "/", "httpbin.org"),
            new Cookie("PlatformCookie", "added", "/", "httpbin.org")
        });

        await WebViewCookieSynchronizer.SyncToVirtualViewAsync(cookieStore, cookieContainer, uri);

        var cookies = cookieContainer.GetCookies(uri).Cast<Cookie>().ToList();
        Assert.Contains(cookies, cookie => cookie.Name == "MauiCookie" && cookie.Value == "new-value");
        Assert.Contains(cookies, cookie => cookie.Name == "PlatformCookie" && cookie.Value == "added");
    }

    private sealed class FakeCookieStore : IWebViewCookieStore
    {
        private readonly List<Cookie> _cookies;

        public FakeCookieStore(IEnumerable<Cookie> cookies)
        {
            _cookies = cookies.Select(CloneCookie).ToList();
        }

        public IReadOnlyList<Cookie> Cookies => _cookies;

        public void AddOrUpdateCookie(Cookie cookie)
        {
            var existing = _cookies.FirstOrDefault(candidate =>
                candidate.Name == cookie.Name &&
                string.Equals(candidate.Domain, cookie.Domain, StringComparison.OrdinalIgnoreCase) &&
                string.Equals(candidate.Path, cookie.Path, StringComparison.Ordinal));

            if (existing is null)
            {
                _cookies.Add(CloneCookie(cookie));
                return;
            }

            existing.Value = cookie.Value;
            existing.Expires = cookie.Expires;
            existing.HttpOnly = cookie.HttpOnly;
            existing.Secure = cookie.Secure;
        }

        public void DeleteCookie(string name, string domain, string path)
        {
            _cookies.RemoveAll(cookie =>
                cookie.Name == name &&
                string.Equals(cookie.Domain, domain, StringComparison.OrdinalIgnoreCase) &&
                string.Equals(cookie.Path, path, StringComparison.Ordinal));
        }

        public Task<IReadOnlyList<Cookie>> GetCookiesAsync()
        {
            return Task.FromResult<IReadOnlyList<Cookie>>(_cookies.Select(CloneCookie).ToList());
        }

        private static Cookie CloneCookie(Cookie cookie)
        {
            return new Cookie(cookie.Name, cookie.Value, cookie.Path, cookie.Domain)
            {
                Expires = cookie.Expires,
                HttpOnly = cookie.HttpOnly,
                Secure = cookie.Secure,
            };
        }
    }
}
