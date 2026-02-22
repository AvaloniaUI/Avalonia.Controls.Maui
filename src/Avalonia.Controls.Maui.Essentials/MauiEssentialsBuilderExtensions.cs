using Microsoft.Maui.Hosting;

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

        return builder;
    }
}
