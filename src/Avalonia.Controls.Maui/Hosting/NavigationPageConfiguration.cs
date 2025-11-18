using System.Reflection;
using Microsoft.Maui.Controls;

namespace Avalonia.Controls.Maui.Hosting;

/// <summary>
/// Configures NavigationPage to use MAUI handler system for Avalonia platform.
/// HACK: This is needed because NavigationPage defaults to legacy navigation system on non-Windows/Android/Tizen platforms.
/// </summary>
public static class NavigationPageConfiguration
{
    private static bool _configured = false;

    /// <summary>
    /// Ensures NavigationPage uses the MAUI handler system instead of legacy renderer system.
    /// This is required because NavigationPage checks the platform and defaults to legacy mode
    /// on non-Windows/Android/Tizen platforms.
    /// </summary>
    public static void ConfigureForMauiHandlers()
    {
        if (_configured)
            return;

        _configured = true;

        // Use reflection to modify the UseMauiHandler constant behavior
        // Since we can't modify the const, we need to ensure NavigationPages are created with setForMaui=true
        // This is done by modifying the internal field after construction

        // Hook into NavigationPage construction to ensure it uses MAUI navigation
        // Unfortunately, since UseMauiHandler is a compile-time constant, we need a different approach

        // The best approach is to patch this at the source level or document that users should
        // explicitly create NavigationPages that use MAUI handlers
    }

    /// <summary>
    /// Creates a NavigationPage that is configured to use MAUI handlers.
    /// Use this instead of `new NavigationPage()` to ensure proper handler usage.
    /// </summary>
    public static NavigationPage CreateNavigationPage(Page? rootPage = null)
    {
        // Create NavigationPage with setForMaui = true
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
