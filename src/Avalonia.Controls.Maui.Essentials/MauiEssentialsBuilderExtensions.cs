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

        Microsoft.Maui.Media.Screenshot.SetDefault(new Avalonia.Controls.Maui.Essentials.AvaloniaScreenshot(platformProvider));
        Microsoft.Maui.Storage.FilePicker.SetDefault(new Avalonia.Controls.Maui.Essentials.AvaloniaFilePicker(platformProvider));
        Microsoft.Maui.Media.MediaPicker.SetDefault(new Avalonia.Controls.Maui.Essentials.AvaloniaMediaPicker(platformProvider));
        Microsoft.Maui.Devices.HapticFeedback.SetDefault(new Avalonia.Controls.Maui.Essentials.AvaloniaHapticFeedback());
        Microsoft.Maui.Storage.Preferences.SetDefault(new Avalonia.Controls.Maui.Essentials.AvaloniaPreferences());
        FileSystem.SetCurrent(new Avalonia.Controls.Maui.Essentials.AvaloniaFileSystem());
        Microsoft.Maui.Authentication.WebAuthenticator.SetDefault(new Avalonia.Controls.Maui.Essentials.AvaloniaWebAuthenticator(platformProvider));

        return builder;
    }
}
