using System.Text.Json;

namespace Avalonia.Controls.Maui.Handlers;

internal readonly record struct WebViewHistoryState(bool CanGoBack, bool CanGoForward, string? Url);

internal static class WebViewHistorySynchronizer
{
    internal const string MessageType = "__AvaloniaControlsMaui_HistoryState";

    public static string InjectionScript { get; } = $$"""
        (() => {
          const messageType = "{{MessageType}}";
          const trackerKey = "__avaloniaControlsMauiHistoryTracker";

          const notify = () => {
            const tracker = window[trackerKey];
            if (!tracker || typeof window.invokeCSharpAction !== "function") {
              return;
            }

            const payload = JSON.stringify({
              type: messageType,
              canGoBack: tracker.index > 0,
              canGoForward: tracker.index < tracker.entries.length - 1,
              url: window.location.href
            });

            if (tracker.lastPayload === payload) {
              return;
            }

            tracker.lastPayload = payload;
            window.invokeCSharpAction(payload);
          };

          const normalizeUrl = (url) => {
            try {
              return url ? new URL(url, window.location.href).href : window.location.href;
            } catch {
              return window.location.href;
            }
          };

          if (window[trackerKey]) {
            notify();
            return true;
          }

          const tracker = {
            entries: [window.location.href],
            index: 0,
            lastPayload: null
          };

          const syncFromLocation = () => {
            const currentUrl = window.location.href;

            if (tracker.index > 0 && tracker.entries[tracker.index - 1] === currentUrl) {
              tracker.index -= 1;
            } else if (tracker.index + 1 < tracker.entries.length && tracker.entries[tracker.index + 1] === currentUrl) {
              tracker.index += 1;
            } else {
              const existingIndex = tracker.entries.lastIndexOf(currentUrl);
              if (existingIndex >= 0) {
                tracker.index = existingIndex;
              } else {
                tracker.entries = tracker.entries.slice(0, tracker.index + 1);
                tracker.entries.push(currentUrl);
                tracker.index = tracker.entries.length - 1;
              }
            }

            notify();
          };

          const wrap = (methodName, update) => {
            const original = window.history[methodName];
            if (typeof original !== "function") {
              return;
            }

            window.history[methodName] = function(...args) {
              const result = original.apply(this, args);
              update(args);
              notify();
              return result;
            };
          };

          wrap("pushState", (args) => {
            tracker.entries = tracker.entries.slice(0, tracker.index + 1);
            tracker.entries.push(normalizeUrl(args.length > 2 ? args[2] : undefined));
            tracker.index = tracker.entries.length - 1;
          });

          wrap("replaceState", (args) => {
            tracker.entries[tracker.index] = normalizeUrl(args.length > 2 ? args[2] : undefined);
          });

          window.addEventListener("popstate", syncFromLocation);
          window.addEventListener("hashchange", syncFromLocation);

          window[trackerKey] = tracker;
          notify();
          return true;
        })();
        """;

    public static bool TryParseMessage(string? body, out WebViewHistoryState state)
    {
        state = default;

        if (string.IsNullOrWhiteSpace(body))
            return false;

        try
        {
            using var document = JsonDocument.Parse(body);
            var root = document.RootElement;

            if (root.ValueKind != JsonValueKind.Object)
                return false;

            if (!root.TryGetProperty("type", out var typeElement) ||
                typeElement.GetString() != MessageType)
            {
                return false;
            }

            if (!root.TryGetProperty("canGoBack", out var canGoBackElement) ||
                canGoBackElement.ValueKind is not JsonValueKind.True and not JsonValueKind.False)
            {
                return false;
            }

            if (!root.TryGetProperty("canGoForward", out var canGoForwardElement) ||
                canGoForwardElement.ValueKind is not JsonValueKind.True and not JsonValueKind.False)
            {
                return false;
            }

            state = new WebViewHistoryState(
                canGoBackElement.GetBoolean(),
                canGoForwardElement.GetBoolean(),
                root.TryGetProperty("url", out var urlElement) ? urlElement.GetString() : null);

            return true;
        }
        catch (JsonException)
        {
            return false;
        }
    }

    public static WebViewHistoryState MergeWithNativeState(
        WebViewHistoryState state,
        bool nativeCanGoBack,
        bool nativeCanGoForward)
    {
        return new WebViewHistoryState(
            state.CanGoBack || nativeCanGoBack,
            state.CanGoForward || nativeCanGoForward,
            state.Url);
    }
}
