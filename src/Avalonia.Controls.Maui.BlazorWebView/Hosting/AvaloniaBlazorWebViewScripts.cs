using System.Text.Json;

namespace Avalonia.Controls.Maui.BlazorWebView.Hosting;

internal static class AvaloniaBlazorWebViewScripts
{
    private const string MessagePrefix = "__avalonia_blazor_webview_message__:";

    public static string CreateInitializationScript(string documentToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(documentToken);

        var encodedMessagePrefix = JsonSerializer.Serialize(MessagePrefix + documentToken + ":");
        return $$"""
        (function () {
            window.__avaloniaBlazorWebViewMessagePrefix = {{encodedMessagePrefix}};

            if (!window.__avaloniaBlazorWebViewBridge) {
                const receiveMessageCallbacks = [];
                const pendingSendMessages = [];
                const external = window.external || {};
                let pendingSendFlushTimer = null;

                function hasNativeBridge() {
                    return typeof window.invokeCSharpAction === 'function';
                }

                function trySendMessage(message) {
                    if (!hasNativeBridge()) {
                        return false;
                    }

                    window.invokeCSharpAction(window.__avaloniaBlazorWebViewMessagePrefix + message);
                    return true;
                }

                function schedulePendingSendFlush() {
                    if (pendingSendFlushTimer) {
                        return;
                    }

                    pendingSendFlushTimer = setTimeout(function () {
                        pendingSendFlushTimer = null;
                        flushPendingSendMessages();
                    }, 20);
                }

                function flushPendingSendMessages() {
                    while (pendingSendMessages.length && hasNativeBridge()) {
                        window.invokeCSharpAction(window.__avaloniaBlazorWebViewMessagePrefix + pendingSendMessages.shift());
                    }

                    if (pendingSendMessages.length) {
                        schedulePendingSendFlush();
                    }
                }

                external.sendMessage = function (message) {
                    if (!trySendMessage(message)) {
                        pendingSendMessages.push(message);
                        schedulePendingSendFlush();
                    }
                };

                external.receiveMessage = function (callback) {
                    receiveMessageCallbacks.push(callback);
                };

                window.external = external;
                window.__avaloniaBlazorWebViewDispatchMessage = function (message) {
                    receiveMessageCallbacks.forEach(function (callback) {
                        callback(message);
                    });
                };

                window.__avaloniaBlazorWebViewBridge = true;
            }

            function startBlazorWhenReady() {
                if (window.__avaloniaBlazorWebViewStarted) {
                    return;
                }

                if (!window.Blazor || !window.Blazor.start || typeof window.invokeCSharpAction !== 'function') {
                    setTimeout(startBlazorWhenReady, 20);
                    return;
                }

                window.__avaloniaBlazorWebViewStarted = true;
                try {
                    Promise.resolve(window.Blazor.start()).catch(function (error) {
                        if (!String(error).includes('already started')) {
                            console.error(error);
                        }
                    });
                } catch (error) {
                    if (!String(error).includes('already started')) {
                        console.error(error);
                    }
                }
            }

            startBlazorWhenReady();
        })();
        """;
    }

    public static bool TryUnwrapMessage(string documentToken, string message, out string payload)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(documentToken);

        var expectedPrefix = MessagePrefix + documentToken + ":";
        if (message.StartsWith(expectedPrefix, StringComparison.Ordinal))
        {
            payload = message[expectedPrefix.Length..];
            return true;
        }

        payload = string.Empty;
        return false;
    }

    public static string CreateDispatchMessageScript(string message)
    {
        var encodedMessage = JsonSerializer.Serialize(message);
        return $"window.__avaloniaBlazorWebViewDispatchMessage && window.__avaloniaBlazorWebViewDispatchMessage({encodedMessage});";
    }
}
