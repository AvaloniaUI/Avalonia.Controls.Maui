using Avalonia.Platform;
using Avalonia.Styling;
using Microsoft.Maui.ApplicationModel;
using System;
using System.Globalization;
using System.Reflection;

namespace Avalonia.Controls.Maui.Services;

/// <summary>
/// Avalonia implementation of the <see cref="IAppInfo"/> interface.
/// Provides application information and settings for Avalonia-based MAUI applications.
/// </summary>
internal class AvaloniaAppInfoImplementation : IAppInfo
{
    static readonly Assembly _launchingAssembly = Assembly.GetEntryAssembly() ?? Assembly.GetExecutingAssembly();

    /// <summary>
    /// Gets the package name of the application from the assembly title attribute.
    /// </summary>
    public string PackageName =>
        _launchingAssembly.GetCustomAttribute<AssemblyTitleAttribute>()?.Title ?? string.Empty;

    /// <summary>
    /// Gets the name of the application from the assembly title attribute.
    /// </summary>
    public string Name =>
        _launchingAssembly.GetCustomAttribute<AssemblyTitleAttribute>()?.Title ?? string.Empty;

    /// <summary>
    /// Gets the version of the application from the assembly version information.
    /// </summary>
    public Version Version =>
        _launchingAssembly.GetName().Version ?? new Version(1, 0, 0, 0);

    /// <summary>
    /// Gets the version string representation of the application version.
    /// </summary>
    public string VersionString => Version.ToString();

    /// <summary>
    /// Gets the build string from the revision number of the application version.
    /// </summary>
    public string BuildString => Version.Revision.ToString(CultureInfo.InvariantCulture);

    /// <summary>
    /// Shows the settings UI for the application.
    /// This is not implemented for Avalonia and does nothing when called.
    /// </summary>
    public void ShowSettingsUI()
    {
        // Not implemented for Avalonia - could potentially open system settings
    }

    /// <summary>
    /// Gets the theme of the platform: the desktop's light or dark setting, which an app's own theme
    /// (<c>Application.UserAppTheme</c>) does not change.
    /// Returns <see cref="AppTheme.Unspecified"/> if the theme cannot be determined.
    /// </summary>
    public AppTheme RequestedTheme
    {
        get
        {
            var application = Application.Current;
            if (application == null)
                return AppTheme.Unspecified;

            if (application.PlatformSettings is { } platformSettings)
            {
                return platformSettings.GetColorValues().ThemeVariant == PlatformThemeVariant.Dark
                    ? AppTheme.Dark
                    : AppTheme.Light;
            }

            // Without platform settings, the application's theme is the best indication
            var theme = application.ActualThemeVariant;
            if (theme == ThemeVariant.Dark)
                return AppTheme.Dark;
            if (theme == ThemeVariant.Light)
                return AppTheme.Light;

            return AppTheme.Unspecified;
        }
    }

    /// <summary>
    /// Gets the packaging model of the application.
    /// Always returns <see cref="AppPackagingModel.Unpackaged"/> for Avalonia applications.
    /// </summary>
    public AppPackagingModel PackagingModel => AppPackagingModel.Unpackaged;

    /// <summary>
    /// Gets the requested layout direction based on the current culture's text direction.
    /// Returns <see cref="LayoutDirection.RightToLeft"/> for RTL cultures,
    /// otherwise <see cref="LayoutDirection.LeftToRight"/>.
    /// </summary>
    public LayoutDirection RequestedLayoutDirection =>
        CultureInfo.CurrentCulture.TextInfo.IsRightToLeft
            ? LayoutDirection.RightToLeft
            : LayoutDirection.LeftToRight;
}
