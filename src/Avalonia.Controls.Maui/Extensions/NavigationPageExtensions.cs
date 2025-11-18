using System.Reflection;
using Microsoft.Maui.Controls;

namespace Avalonia.Controls.Maui;

/// <summary>
/// Extension methods for NavigationPage to ensure proper MAUI handler usage on Avalonia platform.
/// HACK: This is needed because NavigationPage defaults to legacy navigation system on non-Windows/Android/Tizen platforms.
/// </summary>
public static class NavigationPageExtensions
{
    /// <summary>
    /// Creates a NavigationPage that uses MAUI handlers instead of legacy navigation system.
    /// This is required for Avalonia platform because NavigationPage defaults to legacy mode
    /// on non-Windows/Android/Tizen platforms.
    /// </summary>
    /// <param name="rootPage">The root page for the navigation stack.</param>
    /// <returns>A NavigationPage configured to use MAUI handlers.</returns>
    public static NavigationPage CreateForMauiHandlers(Page? rootPage = null)
    {
        // Create NavigationPage with setForMaui = true using internal constructor
        var constructor = typeof(NavigationPage).GetConstructor(
            BindingFlags.Instance | BindingFlags.NonPublic,
            null,
            new[] { typeof(bool), typeof(Page) },
            null);

        if (constructor == null)
            throw new InvalidOperationException("Could not find NavigationPage internal constructor");

        var navPage = (NavigationPage)constructor.Invoke(new object?[] { true, rootPage });
        return navPage;
    }
}
