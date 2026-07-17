using System.Runtime.CompilerServices;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Authentication;
using Microsoft.Maui.Devices;
using Microsoft.Maui.Hosting;
using Microsoft.Maui.Media;
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

        EssentialsDefaults.SetScreenshot(null, new Avalonia.Controls.Maui.Essentials.AvaloniaScreenshot(platformProvider));
        EssentialsDefaults.SetFilePicker(null, new Avalonia.Controls.Maui.Essentials.AvaloniaFilePicker(platformProvider));
        EssentialsDefaults.SetMediaPicker(null, new Avalonia.Controls.Maui.Essentials.AvaloniaMediaPicker(platformProvider));
        EssentialsDefaults.SetHapticFeedback(null, new Avalonia.Controls.Maui.Essentials.AvaloniaHapticFeedback());
        EssentialsDefaults.SetPreferences(null, new Avalonia.Controls.Maui.Essentials.AvaloniaPreferences());
        EssentialsDefaults.SetFileSystem(null, new Avalonia.Controls.Maui.Essentials.AvaloniaFileSystem());
        EssentialsDefaults.SetWebAuthenticator(null, new Avalonia.Controls.Maui.Essentials.AvaloniaWebAuthenticator(platformProvider));

        return builder;
    }

    /// <summary>
    /// Installs Avalonia implementations into the Microsoft.Maui.Essentials static facades.
    /// The facades only expose internal SetDefault/SetCurrent hooks, so these accessors use
    /// <see cref="UnsafeAccessorAttribute"/> instead of MAUI internals.
    /// </summary>
    static class EssentialsDefaults
    {
        [UnsafeAccessor(UnsafeAccessorKind.StaticMethod, Name = "SetDefault")]
        internal static extern void SetScreenshot(
            [UnsafeAccessorType("Microsoft.Maui.Media.Screenshot, Microsoft.Maui.Essentials")] object? facade,
            IScreenshot? implementation);

        [UnsafeAccessor(UnsafeAccessorKind.StaticMethod, Name = "SetDefault")]
        internal static extern void SetFilePicker(
            [UnsafeAccessorType("Microsoft.Maui.Storage.FilePicker, Microsoft.Maui.Essentials")] object? facade,
            IFilePicker? implementation);

        [UnsafeAccessor(UnsafeAccessorKind.StaticMethod, Name = "SetDefault")]
        internal static extern void SetMediaPicker(
            [UnsafeAccessorType("Microsoft.Maui.Media.MediaPicker, Microsoft.Maui.Essentials")] object? facade,
            IMediaPicker? implementation);

        [UnsafeAccessor(UnsafeAccessorKind.StaticMethod, Name = "SetDefault")]
        internal static extern void SetHapticFeedback(
            [UnsafeAccessorType("Microsoft.Maui.Devices.HapticFeedback, Microsoft.Maui.Essentials")] object? facade,
            IHapticFeedback? implementation);

        [UnsafeAccessor(UnsafeAccessorKind.StaticMethod, Name = "SetDefault")]
        internal static extern void SetPreferences(
            [UnsafeAccessorType("Microsoft.Maui.Storage.Preferences, Microsoft.Maui.Essentials")] object? facade,
            IPreferences? implementation);

        [UnsafeAccessor(UnsafeAccessorKind.StaticMethod, Name = "SetCurrent")]
        internal static extern void SetFileSystem(
            [UnsafeAccessorType("Microsoft.Maui.Storage.FileSystem, Microsoft.Maui.Essentials")] object? facade,
            IFileSystem? implementation);

        [UnsafeAccessor(UnsafeAccessorKind.StaticMethod, Name = "SetDefault")]
        internal static extern void SetWebAuthenticator(
            [UnsafeAccessorType("Microsoft.Maui.Authentication.WebAuthenticator, Microsoft.Maui.Essentials")] object? facade,
            IWebAuthenticator? implementation);
    }
}
