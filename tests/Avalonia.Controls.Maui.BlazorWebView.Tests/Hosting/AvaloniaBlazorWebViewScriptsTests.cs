using System.Text.Json;
using Jint;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebView;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.JSInterop;
using Avalonia.Controls.Maui.BlazorWebView.Hosting;
using BlazorDispatcher = Microsoft.AspNetCore.Components.Dispatcher;

namespace Avalonia.Controls.Maui.BlazorWebView.Tests.Hosting;

public class AvaloniaBlazorWebViewScriptsTests
{
    [Fact]
    public void InitializationScript_InstallsBlazorHybridExternalBridge()
    {
        var script = AvaloniaBlazorWebViewScripts.CreateInitializationScript("document-token");

        Assert.Contains("window.external", script);
        Assert.Contains("external.sendMessage = function (message)", script);
        Assert.Contains("window.__avaloniaBlazorWebViewMessagePrefix + message", script);
        Assert.Contains("external.receiveMessage = function (callback)", script);
        Assert.Contains("window.__avaloniaBlazorWebViewDispatchMessage = function (message)", script);
    }

    [Fact]
    public void InitializationScript_StartsBlazorOnlyWhenBlazorStartIsAvailable()
    {
        var script = AvaloniaBlazorWebViewScripts.CreateInitializationScript("document-token");

        Assert.Contains("!window.Blazor || !window.Blazor.start", script);
        Assert.Contains("typeof window.invokeCSharpAction !== 'function'", script);
        Assert.Contains("setTimeout(startBlazorWhenReady, 20)", script);
        Assert.Contains("Promise.resolve(window.Blazor.start())", script);
        Assert.Contains("window.__avaloniaBlazorWebViewStarted = true", script);
    }

    [Fact]
    public void InitializationScript_QueuesOutboundMessagesUntilNativeBridgeIsAvailable()
    {
        var nativeMessages = new List<string>();
        var engine = CreateEngine(nativeMessages, hasNativeBridge: false);

        engine.Execute(AvaloniaBlazorWebViewScripts.CreateInitializationScript("document-token"));
        engine.Execute("window.external.sendMessage('__bwv:queued')");

        Assert.Empty(nativeMessages);

        engine.Execute("window.invokeCSharpAction = captureNativeMessage; pendingTimer()");

        Assert.Equal(
            ["__avalonia_blazor_webview_message__:document-token:__bwv:queued"],
            nativeMessages);
    }

    [Fact]
    public void InitializationAndDispatchScripts_ExecuteBidirectionalBridge()
    {
        var nativeMessages = new List<string>();
        var engine = CreateEngine(nativeMessages, hasNativeBridge: true);

        engine.Execute(AvaloniaBlazorWebViewScripts.CreateInitializationScript("document-token"));
        engine.Execute("window.external.sendMessage('__bwv:outbound')");
        engine.Execute("var receivedMessage = null; window.external.receiveMessage(function (message) { receivedMessage = message; })");
        engine.Execute(AvaloniaBlazorWebViewScripts.CreateDispatchMessageScript("__bwv:inbound"));

        Assert.Equal(
            ["__avalonia_blazor_webview_message__:document-token:__bwv:outbound"],
            nativeMessages);
        Assert.Equal("__bwv:inbound", engine.GetValue("receivedMessage").AsString());
    }

    [Fact]
    public void InitializationScript_EmbedsDocumentSpecificMessagePrefix()
    {
        var script = AvaloniaBlazorWebViewScripts.CreateInitializationScript("document-token");

        Assert.Contains("__avalonia_blazor_webview_message__:document-token:", script);
    }

    [Fact]
    public void TryUnwrapMessage_AcceptsOnlyTheCurrentDocumentToken()
    {
        const string payload = "__bwv:[\"AttachPage\"]";
        const string wrappedMessage = "__avalonia_blazor_webview_message__:current:" + payload;

        Assert.True(AvaloniaBlazorWebViewScripts.TryUnwrapMessage("current", wrappedMessage, out var unwrapped));
        Assert.Equal(payload, unwrapped);
        Assert.False(AvaloniaBlazorWebViewScripts.TryUnwrapMessage("previous", wrappedMessage, out _));
    }

    [Theory]
    [InlineData("__bwv:[\"AttachPage\",\"http://127.0.0.1:49152/\",\"http://127.0.0.1:49152/\"]")]
    [InlineData("__bwv:[\"BeginInvokeJS\",2,\"quote: \\\" and newline: \\n\",null]")]
    [InlineData("__bwv:[\"EndInvokeDotNet\",3,true,\"<script>alert('x')</script>\"]")]
    public void CreateDispatchMessageScript_JsonEncodesMessagePayload(string message)
    {
        var script = AvaloniaBlazorWebViewScripts.CreateDispatchMessageScript(message);
        var encodedMessage = JsonSerializer.Serialize(message);

        Assert.Equal(
            $"window.__avaloniaBlazorWebViewDispatchMessage && window.__avaloniaBlazorWebViewDispatchMessage({encodedMessage});",
            script);
    }

    [Fact]
    public async Task BlazorWebViewProtocol_CompletesIJSRuntimeInvokeAsync_WhenJavaScriptEndsInvocation()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddMauiBlazorWebView();

        await using var provider = services.BuildServiceProvider();
        await using var manager = new ProtocolTestWebViewManager(provider);

        var result = new TaskCompletionSource<string>(TaskCreationOptions.RunContinuationsAsynchronously);

        await manager.AddRootComponentAsync(
            typeof(InteropRootComponent),
            "#app",
            ParameterView.FromDictionary(new Dictionary<string, object?>
            {
                [nameof(InteropRootComponent.Result)] = result,
            }));

        manager.ReceiveFromJavaScript("AttachPage", manager.AppBaseUri.AbsoluteUri, manager.AppBaseUri.AbsoluteUri);

        Assert.Equal("echo:ping", await result.Task.WaitAsync(TimeSpan.FromSeconds(5)));
        Assert.Contains(manager.BeginInvokeJsInvocations, invocation =>
            invocation.Identifier == "interop.echo" &&
            invocation.ArgsJson.Contains("ping", StringComparison.Ordinal));
    }

    private static Engine CreateEngine(List<string> nativeMessages, bool hasNativeBridge)
    {
        var engine = new Engine();
        engine.SetValue("captureNativeMessage", (Action<string>)nativeMessages.Add);
        engine.Execute(
            """
            var pendingTimer = null;
            var setTimeout = function (callback) {
                pendingTimer = callback;
                return 1;
            };
            var window = { __avaloniaBlazorWebViewStarted: true };
            """);

        if (hasNativeBridge)
            engine.Execute("window.invokeCSharpAction = captureNativeMessage");

        return engine;
    }

    private sealed class ProtocolTestWebViewManager : WebViewManager
    {
        private const string IpcPrefix = "__bwv:";

        public ProtocolTestWebViewManager(IServiceProvider provider)
            : base(
                provider,
                BlazorDispatcher.CreateDefault(),
                new Uri("http://127.0.0.1:5000/"),
                new NullFileProvider(),
                new JSComponentConfigurationStore(),
                "index.html")
        {
        }

        public Uri AppBaseUri => new("http://127.0.0.1:5000/");

        public List<BeginInvokeJsInvocation> BeginInvokeJsInvocations { get; } = [];

        public void ReceiveFromJavaScript(string messageType, params object?[] args)
        {
            MessageReceived(AppBaseUri, CreateMessage(messageType, args));
        }

        protected override void NavigateCore(Uri absoluteUri)
        {
        }

        protected override void SendMessage(string message)
        {
            if (!message.StartsWith(IpcPrefix, StringComparison.Ordinal))
                return;

            using var document = JsonDocument.Parse(message[IpcPrefix.Length..]);
            var payload = document.RootElement;
            var messageType = payload[0].GetString();

            switch (messageType)
            {
                case "BeginInvokeJS":
                    HandleBeginInvokeJS(payload);
                    break;
                case "RenderBatch":
                    ReceiveFromJavaScript("OnRenderCompleted", payload[1].GetInt64(), null);
                    break;
            }
        }

        private void HandleBeginInvokeJS(JsonElement payload)
        {
            var asyncHandle = payload[1].GetInt64();
            var identifier = payload[2].GetString()!;
            var argsJson = payload[3].GetString()!;

            BeginInvokeJsInvocations.Add(new BeginInvokeJsInvocation(identifier, argsJson));

            object? result = identifier == "interop.echo"
                ? $"echo:{GetFirstStringArgument(argsJson)}"
                : null;

            var completion = JsonSerializer.Serialize(new object?[] { asyncHandle, true, result });
            ReceiveFromJavaScript("EndInvokeJS", asyncHandle, true, completion);
        }

        private static string? GetFirstStringArgument(string argsJson)
        {
            using var document = JsonDocument.Parse(argsJson);
            return document.RootElement[0].GetString();
        }

        private static string CreateMessage(string messageType, params object?[] args)
        {
            var payload = new object?[args.Length + 1];
            payload[0] = messageType;
            args.CopyTo(payload, 1);
            return $"{IpcPrefix}{JsonSerializer.Serialize(payload)}";
        }
    }

    public sealed record BeginInvokeJsInvocation(string Identifier, string ArgsJson);

    private sealed class InteropRootComponent : ComponentBase
    {
        [Inject]
        public IJSRuntime JS { get; set; } = null!;

        [Parameter]
        public TaskCompletionSource<string> Result { get; set; } = null!;

        protected override void BuildRenderTree(RenderTreeBuilder builder)
        {
            builder.OpenElement(0, "div");
            builder.AddContent(1, "Interop");
            builder.CloseElement();
        }

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (!firstRender)
                return;

            try
            {
                var value = await JS.InvokeAsync<string>("interop.echo", "ping");
                Result.TrySetResult(value);
            }
            catch (Exception ex)
            {
                Result.TrySetException(ex);
            }
        }
    }
}
