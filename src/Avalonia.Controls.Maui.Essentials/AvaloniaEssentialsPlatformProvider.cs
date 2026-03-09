using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;

namespace Avalonia.Controls.Maui.Essentials;

/// <summary>
/// Provides platform-specific services and information for Avalonia-based implementations of Microsoft.Maui.Essentials features.
/// </summary>
internal class AvaloniaEssentialsPlatformProvider : IAvaloniaEssentialsPlatformProvider
{
    /// <inheritdoc/>
    public TopLevel? GetTopLevel()
    {
        var lifetime = Application.Current?.ApplicationLifetime;

        return lifetime switch
        {
            IClassicDesktopStyleApplicationLifetime desktop => desktop.MainWindow,
            ISingleViewApplicationLifetime singleView => TopLevel.GetTopLevel(singleView.MainView),
            _ => null
        };
    }
}
