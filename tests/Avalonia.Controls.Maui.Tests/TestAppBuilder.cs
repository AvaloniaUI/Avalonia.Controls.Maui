using Avalonia;
using Avalonia.Headless;

[assembly: AvaloniaTestApplication(typeof(Avalonia.Controls.Maui.Tests.TestAppBuilder))]

namespace Avalonia.Controls.Maui.Tests;

/// <summary>
/// Configures the Avalonia application for headless testing.
/// </summary>
public class TestAppBuilder
{
    public static AppBuilder BuildAvaloniaApp() => AppBuilder.Configure<App>()
        .UseHeadless(new AvaloniaHeadlessPlatformOptions());
}
