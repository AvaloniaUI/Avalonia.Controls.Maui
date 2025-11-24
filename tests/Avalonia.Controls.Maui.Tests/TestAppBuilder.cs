using Avalonia;
using Avalonia.Headless;
using Avalonia.Controls.Maui.Services;
using Avalonia.Controls.Maui.Tests;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Maui;
using Microsoft.Maui.Hosting;

[assembly: AvaloniaTestApplication(typeof(TestAppBuilder))]

namespace Avalonia.Controls.Maui.Tests;

public class TestAppBuilder
{
    public static AppBuilder BuildAvaloniaApp() => AppBuilder.Configure<App>()
        .UseHeadless(new AvaloniaHeadlessPlatformOptions());
}

public static class MauiTestAppBuilderExtensions
{
    public static MauiAppBuilder ConfigureTestBuilder(this MauiAppBuilder builder)
    {
        builder.ConfigureMauiHandlers(handlers =>
        {
            // Register Avalonia handlers
            handlers.AddHandler<ILabel, Avalonia.Controls.Maui.Handlers.LabelHandler>();
            handlers.AddHandler<IButton, Avalonia.Controls.Maui.Handlers.ButtonHandler>();
            handlers.AddHandler<IBorderView, Avalonia.Controls.Maui.Handlers.BorderHandler>();
            handlers.AddHandler<IPicker, Avalonia.Controls.Maui.Handlers.PickerHandler>();
            handlers.AddHandler<IContentView, Avalonia.Controls.Maui.Handlers.ContentViewHandler>();
            handlers.AddHandler<ILayout, Avalonia.Controls.Maui.Handlers.LayoutHandler>();
            handlers.AddHandler<IScrollView, Avalonia.Controls.Maui.Handlers.ScrollViewHandler>();
            handlers.AddHandler<ISwipeView, Avalonia.Controls.Maui.Handlers.SwipeViewHandler>();
            handlers.AddHandler<ISwipeItemMenuItem, Avalonia.Controls.Maui.Handlers.SwipeItemMenuItemHandler>();
            handlers.AddHandler<ISwipeItemView, Avalonia.Controls.Maui.Handlers.SwipeItemViewHandler>();
            handlers.AddHandler<IWindow, Avalonia.Controls.Maui.Handlers.WindowHandler>();
        });

        builder.Services.AddSingleton<IFontManager>(sp =>
            new FontManager(new FontRegistrar(), sp));

        // Register image source service
        builder.Services.AddSingleton<IImageSourceService<IFileImageSource>, AvaloniaFileImageSourceService>();

        return builder;
    }
}

// Minimal font registrar for testing
public class FontRegistrar : IFontRegistrar
{
    public void Register(string filename, string? alias, System.Reflection.Assembly assembly)
    {
        // No-op for tests
    }

    public void Register(string filename, string? alias)
    {
        // No-op for tests
    }

    public string? GetFont(string font)
    {
        return font;
    }
}