using Avalonia.Controls.Maui.Services;
using Avalonia.Headless.XUnit;
using Avalonia.Platform;
using Avalonia.Styling;
using Microsoft.Maui.ApplicationModel;

namespace Avalonia.Controls.Maui.Tests.Services;

public class AvaloniaAppInfoImplementationTests
{
    [AvaloniaTheory(DisplayName = "RequestedTheme reports the desktop theme, not the app's own theme")]
    [InlineData(false)]
    [InlineData(true)]
    public void RequestedThemeReportsDesktopTheme(bool appUsesOppositeTheme)
    {
        var avaloniaApp = Application.Current!;
        var original = avaloniaApp.RequestedThemeVariant;
        try
        {
            var desktopTheme = avaloniaApp.PlatformSettings!.GetColorValues().ThemeVariant;
            var expected = desktopTheme == PlatformThemeVariant.Dark ? AppTheme.Dark : AppTheme.Light;

            // The app picks a theme of its own, as with Application.UserAppTheme
            avaloniaApp.RequestedThemeVariant = appUsesOppositeTheme
                ? (desktopTheme == PlatformThemeVariant.Dark ? ThemeVariant.Light : ThemeVariant.Dark)
                : ThemeVariant.Default;

            Assert.Equal(expected, new AvaloniaAppInfoImplementation().RequestedTheme);
        }
        finally
        {
            avaloniaApp.RequestedThemeVariant = original;
        }
    }
}
