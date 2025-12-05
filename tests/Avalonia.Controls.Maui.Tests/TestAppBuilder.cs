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
        builder.UseAvaloniaApp();

        builder.Services.AddSingleton<IFontManager>(sp =>
            new FontManager(new FontRegistrar(), sp));

        // Register image source service
        builder.Services.AddSingleton<IImageSourceService<IFileImageSource>, AvaloniaFileImageSourceService>();

        // Register handlers for test stubs
        builder.ConfigureMauiHandlers(handlers =>
        {
            handlers.AddHandler<Tests.Stubs.ButtonStub, Avalonia.Controls.Maui.Handlers.ButtonHandler>();
            handlers.AddHandler<Tests.Stubs.LabelStub, Avalonia.Controls.Maui.Handlers.LabelHandler>();
            handlers.AddHandler<ISwipeItemMenuItem, Avalonia.Controls.Maui.Handlers.SwipeItemMenuItemHandler>();
            handlers.AddHandler<ISwipeItemView, Avalonia.Controls.Maui.Handlers.SwipeItemViewHandler>();
        });

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