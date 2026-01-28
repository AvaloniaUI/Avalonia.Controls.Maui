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
        .UseHarfBuzz()
        .UseSkia()
        .UseHeadless(new AvaloniaHeadlessPlatformOptions
        {
            UseHeadlessDrawing = false
        });
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
            // Core control stubs
            handlers.AddHandler<Tests.Stubs.ActivityIndicatorStub, Avalonia.Controls.Maui.Handlers.ActivityIndicatorHandler>();
            handlers.AddHandler<Tests.Stubs.BorderStub, Avalonia.Controls.Maui.Handlers.BorderHandler>();
            handlers.AddHandler<Tests.Stubs.BoxViewStub, Avalonia.Controls.Maui.Handlers.BoxViewHandler>();
            handlers.AddHandler<Tests.Stubs.ButtonStub, Avalonia.Controls.Maui.Handlers.ButtonHandler>();
            handlers.AddHandler<Tests.Stubs.CheckBoxStub, Avalonia.Controls.Maui.Handlers.CheckBoxHandler>();
            handlers.AddHandler<Tests.Stubs.ImageStub, Avalonia.Controls.Maui.Handlers.ImageHandler>();
            handlers.AddHandler<Tests.Stubs.ImageButtonStub, Avalonia.Controls.Maui.Handlers.ImageButtonHandler>();
            handlers.AddHandler<Tests.Stubs.LabelStub, Avalonia.Controls.Maui.Handlers.LabelHandler>();
            handlers.AddHandler<Tests.Stubs.PageStub, Avalonia.Controls.Maui.Handlers.PageHandler>();
            handlers.AddHandler<Tests.Stubs.PickerStub, Avalonia.Controls.Maui.Handlers.PickerHandler>();
            handlers.AddHandler<Tests.Stubs.ProgressBarStub, Avalonia.Controls.Maui.Handlers.ProgressBarHandler>();
            handlers.AddHandler<Tests.Stubs.ScrollViewStub, Avalonia.Controls.Maui.Handlers.ScrollViewHandler>();
            handlers.AddHandler<Tests.Stubs.SliderStub, Avalonia.Controls.Maui.Handlers.SliderHandler>();
            handlers.AddHandler<Tests.Stubs.TabbedPageStub, Avalonia.Controls.Maui.Handlers.TabbedPageHandler>();

            // SwipeView stubs
            handlers.AddHandler<Tests.Stubs.SwipeViewStub, Avalonia.Controls.Maui.Handlers.SwipeViewHandler>();
            handlers.AddHandler<ISwipeItemMenuItem, Avalonia.Controls.Maui.Handlers.SwipeItemMenuItemHandler>();
            handlers.AddHandler<ISwipeItemView, Avalonia.Controls.Maui.Handlers.SwipeItemViewHandler>();

            // Shape stubs
            handlers.AddHandler<Tests.Stubs.RectangleStub, Tests.Stubs.RectangleStubHandler>();
            handlers.AddHandler<Tests.Stubs.EllipseStub, Tests.Stubs.EllipseStubHandler>();
            handlers.AddHandler<Tests.Stubs.LineStub, Tests.Stubs.LineStubHandler>();
            handlers.AddHandler<Tests.Stubs.PolygonStub, Tests.Stubs.PolygonStubHandler>();
            handlers.AddHandler<Tests.Stubs.PolylineStub, Tests.Stubs.PolylineStubHandler>();
            handlers.AddHandler<Tests.Stubs.PathStub, Tests.Stubs.PathStubHandler>();
            handlers.AddHandler<Tests.Stubs.RoundRectangleStub, Tests.Stubs.RoundRectangleStubHandler>();
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