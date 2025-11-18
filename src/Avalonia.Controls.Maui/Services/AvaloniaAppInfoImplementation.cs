using Avalonia.Styling;
using Microsoft.Maui.ApplicationModel;
using System;
using System.Globalization;
using System.Reflection;

namespace Avalonia.Controls.Maui.Services;

internal class AvaloniaAppInfoImplementation : IAppInfo
{
    static readonly Assembly _launchingAssembly = Assembly.GetEntryAssembly() ?? Assembly.GetExecutingAssembly();

    public string PackageName =>
        _launchingAssembly.GetCustomAttribute<AssemblyTitleAttribute>()?.Title ?? string.Empty;

    public string Name =>
        _launchingAssembly.GetCustomAttribute<AssemblyTitleAttribute>()?.Title ?? string.Empty;

    public Version Version =>
        _launchingAssembly.GetName().Version ?? new Version(1, 0, 0, 0);

    public string VersionString => Version.ToString();

    public string BuildString => Version.Revision.ToString(CultureInfo.InvariantCulture);

    public void ShowSettingsUI()
    {
        // Not implemented for Avalonia - could potentially open system settings
    }

    public AppTheme RequestedTheme
    {
        get
        {
            // Check if we're running on the UI thread and have access to the application
            if (Application.Current != null)
            {
                // Avalonia uses ThemeVariant for theme detection
                var theme = Application.Current.ActualThemeVariant;

                if (theme == ThemeVariant.Dark)
                    return AppTheme.Dark;
                else if (theme == ThemeVariant.Light)
                    return AppTheme.Light;
            }

            // Default to unspecified if we can't determine the theme
            return AppTheme.Unspecified;
        }
    }

    public AppPackagingModel PackagingModel => AppPackagingModel.Unpackaged;

    public LayoutDirection RequestedLayoutDirection =>
        CultureInfo.CurrentCulture.TextInfo.IsRightToLeft
            ? LayoutDirection.RightToLeft
            : LayoutDirection.LeftToRight;
}
