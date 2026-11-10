using Avalonia.Controls.Maui.Platform;
using Avalonia.Headless.XUnit;
using Avalonia.Styling;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Maui;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Hosting;
using MauiApplication = Microsoft.Maui.Controls.Application;

namespace Avalonia.Controls.Maui.Tests.Platform;

public class MauiAvaloniaApplicationTests
{
    [AvaloniaFact(DisplayName = "UserAppTheme Set To The Platform Theme Is Kept When The Avalonia Theme Changes")]
    public void UserAppThemeSetToPlatformThemeIsKeptWhenAvaloniaThemeChanges()
    {
        var originalMauiApp = MauiApplication.Current;
        var originalPlatformApp = IPlatformApplication.Current;
        try
        {
            var avaloniaApp = new ThemeTestApplication();
            avaloniaApp.OnFrameworkInitializationCompleted();
            var mauiApp = (MauiApplication)avaloniaApp.Application;

            var platformIsDark = mauiApp.PlatformAppTheme == AppTheme.Dark;
            var appTheme = platformIsDark ? ThemeVariant.Dark : ThemeVariant.Light;
            var otherTheme = platformIsDark ? ThemeVariant.Light : ThemeVariant.Dark;

            // MAUI does not report this change, because the theme in effect stays the same
            mauiApp.UserAppTheme = mauiApp.PlatformAppTheme;

            // Avalonia changes to the other theme, as it does when the platform theme changes
            avaloniaApp.RequestedThemeVariant = otherTheme;

            Assert.Equal(appTheme, avaloniaApp.RequestedThemeVariant);
            Assert.Equal(appTheme, avaloniaApp.ActualThemeVariant);
        }
        finally
        {
            MauiApplication.Current = originalMauiApp;
            IPlatformApplication.Current = originalPlatformApp;
        }
    }

    private sealed class ThemeTestApplication : MauiAvaloniaApplication
    {
        protected override MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder.ConfigureTestBuilder();
            builder.Services.AddSingleton<IApplication, MauiApplication>();
            builder.Services.AddSingleton<Application>(this);
            return builder.Build();
        }
    }
}
