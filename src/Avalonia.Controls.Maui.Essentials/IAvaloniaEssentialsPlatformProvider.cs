using Avalonia.Controls;

namespace Avalonia.Controls.Maui.Essentials;

/// <summary>
/// Provides access to the Avalonia TopLevel for Essentials implementations that require platform-level access, such as file pickers and screenshots.
/// </summary>
public interface IAvaloniaEssentialsPlatformProvider
{
    /// <summary>
    /// Gets the current Avalonia TopLevel, which provides access to platform services such as the storage provider, clipboard, and screen information.
    /// </summary>
    /// <returns>The current TopLevel instance, or <c>null</c> if the application has not been fully initialized or no TopLevel is available.</returns>
    TopLevel? GetTopLevel();
}
