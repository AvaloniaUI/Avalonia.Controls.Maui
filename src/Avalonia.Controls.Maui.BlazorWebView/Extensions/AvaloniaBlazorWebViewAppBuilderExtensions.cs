using Microsoft.AspNetCore.Components.WebView.Maui;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Maui.Hosting;
using AvaloniaBlazorWebViewHandler = Avalonia.Controls.Maui.BlazorWebView.Handlers.BlazorWebViewHandler;

/// <summary>
/// Provides extensions for enabling Blazor Hybrid on the Avalonia.Controls.Maui backend.
/// </summary>
public static class AvaloniaBlazorWebViewAppBuilderExtensions
{
    /// <summary>
    /// Registers Microsoft MAUI Blazor Hybrid services and maps <see cref="BlazorWebView"/>
    /// to the Avalonia.Controls.Maui Blazor WebView handler.
    /// </summary>
    /// <param name="builder">The MAUI app builder.</param>
    /// <returns>The configured MAUI app builder.</returns>
    public static MauiAppBuilder UseAvaloniaBlazorWebView(this MauiAppBuilder builder)
    {
        builder.Services.AddMauiBlazorWebView();

        return builder.ConfigureMauiHandlers(handlers =>
        {
            handlers.AddHandler<IBlazorWebView, AvaloniaBlazorWebViewHandler>();
            handlers.AddHandler<BlazorWebView, AvaloniaBlazorWebViewHandler>();
        });
    }
}
