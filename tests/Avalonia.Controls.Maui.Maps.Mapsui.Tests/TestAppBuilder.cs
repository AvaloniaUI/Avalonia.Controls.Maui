using Avalonia;
using Avalonia.Headless;
using Avalonia.Controls.Maui.Tests;

[assembly: AvaloniaTestApplication(typeof(Avalonia.Controls.Maui.Maps.Mapsui.Tests.TestAppBuilder))]

namespace Avalonia.Controls.Maui.Maps.Mapsui.Tests;

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
