# Avalonia.Controls.Maui.BlazorWebView

Adds Blazor Hybrid support for `Microsoft.AspNetCore.Components.WebView.Maui.BlazorWebView` when running MAUI apps through the Avalonia.Controls.Maui desktop backend.

Register the handler after `UseAvaloniaApp()`:

```csharp
builder
    .UseMauiApp<App>()
    .UseAvaloniaApp()
    .UseAvaloniaBlazorWebView();
```

App projects should still reference `Microsoft.AspNetCore.Components.WebView.Maui` directly, as in the official MAUI Blazor Hybrid template, so the app owns the Blazor static-web-assets targets. The package includes a build check that reports this explicitly when the official package reference is missing.

The platform view is the official Avalonia `NativeWebView`. The handler builds the ASP.NET Core `WebViewManager` integration on top of that control for static assets, navigation, root components, hot reload, developer tools, and JS/.NET messaging. `BlazorWebViewInitializing` and `BlazorWebViewInitialized` receive `AvaloniaBlazorWebViewInitializingEventArgs` and `AvaloniaBlazorWebViewInitializedEventArgs`, respectively, so handlers can cast the official event arguments and access its `WebView` property.

ASP.NET Core supplies Blazor assets as response streams, while Avalonia `NativeWebView.WebResourceRequested` currently exposes the request but has no public response setter. The handler therefore serves those streams to the same `NativeWebView` from a loopback-only HTTP origin. This is an asset transport for the Avalonia control, not a replacement WebView. Each control receives a random loopback port and each loaded document receives a random IPC token.

The official `BlazorWebView.WebResourceRequested` custom-response event is not supported yet because neither Avalonia's public event nor MAUI's neutral event arguments provide a generic response channel. The per-control loopback port also means that WebView storage keyed by origin is not guaranteed to persist across launches.
