using Microsoft.Maui.Hosting;
using Microsoft.Maui.Storage;

namespace Avalonia.Controls.Maui.Essentials;

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
        var platformProvider = new AvaloniaEssentialsPlatformProvider();

        Microsoft.Maui.Media.Screenshot.SetDefault(new AvaloniaScreenshot(platformProvider));
        Microsoft.Maui.Storage.FilePicker.SetDefault(new AvaloniaFilePicker(platformProvider));
        Microsoft.Maui.Media.MediaPicker.SetDefault(new AvaloniaMediaPicker(platformProvider));
        FileSystem.SetCurrent(new AvaloniaFileSystem());

        return builder;
    }
}
