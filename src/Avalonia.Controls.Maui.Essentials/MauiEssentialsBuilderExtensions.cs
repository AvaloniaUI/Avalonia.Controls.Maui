using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Hosting;
using Microsoft.Maui.Storage;

/// <summary>
/// Extension methods for configuring Avalonia-based Microsoft.Maui.Essentials services.
/// </summary>
public static class MauiEssentialsBuilderExtensions
{
    /// <summary>
    /// Configures the app to use Avalonia-based implementations of Microsoft.Maui.Essentials services.
    /// This is required to use any of the Essentials features in an Avalonia.Controls.Maui app with the default static instance.
    /// </summary>
    /// <param name="builder">The MauiAppBuilder instance.</param>
    /// <returns>The updated MauiAppBuilder instance.</returns>
    public static MauiAppBuilder UseAvaloniaEssentials(this MauiAppBuilder builder)
    {
        var platformProvider = new Avalonia.Controls.Maui.Essentials.AvaloniaEssentialsPlatformProvider();
        var preferences = new Avalonia.Controls.Maui.Essentials.AvaloniaPreferences();

        Microsoft.Maui.Media.Screenshot.SetDefault(new Avalonia.Controls.Maui.Essentials.AvaloniaScreenshot(platformProvider));
        Microsoft.Maui.Storage.FilePicker.SetDefault(new Avalonia.Controls.Maui.Essentials.AvaloniaFilePicker(platformProvider));
        Microsoft.Maui.Media.MediaPicker.SetDefault(new Avalonia.Controls.Maui.Essentials.AvaloniaMediaPicker(platformProvider));
        Microsoft.Maui.Devices.HapticFeedback.SetDefault(new Avalonia.Controls.Maui.Essentials.AvaloniaHapticFeedback());
        Microsoft.Maui.Storage.Preferences.SetDefault(preferences);
        FileSystem.SetCurrent(new Avalonia.Controls.Maui.Essentials.AvaloniaFileSystem());
        Microsoft.Maui.Authentication.WebAuthenticator.SetDefault(new Avalonia.Controls.Maui.Essentials.AvaloniaWebAuthenticator(platformProvider));
        builder.Services.RemoveAll<IPreferences>();
        builder.Services.AddSingleton<IPreferences>(preferences);
        builder.Services.TryAddSingleton<IMauiInitializeService, Avalonia.Controls.Maui.Essentials.AvaloniaVersionTrackingInitializer>();
        builder.Services.TryAddSingleton<IVersionTracking>(services =>
            new Avalonia.Controls.Maui.Essentials.AvaloniaVersionTracking(
                services.GetRequiredService<IPreferences>(),
                services.GetRequiredService<IAppInfo>()));

        return builder;
    }
}
