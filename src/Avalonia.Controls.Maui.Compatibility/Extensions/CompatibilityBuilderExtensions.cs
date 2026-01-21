using Microsoft.Maui.Hosting;

namespace Avalonia.Controls.Maui.Compatibility;

/// <summary>
/// Extension methods for adding compatibility handlers to MAUI applications.
/// These handlers support legacy/obsolete MAUI controls using Avalonia implementations.
/// </summary>
public static class CompatibilityBuilderExtensions
{
    /// <summary>
    /// Adds Avalonia-based handlers for legacy MAUI controls including ListView, Frame,
    /// TableView, and Cell types. These controls are considered obsolete in modern MAUI.
    /// </summary>
    /// <remarks>
    /// Call this method AFTER UseAvaloniaApp() to register compatibility handlers.
    ///
    /// Recommended alternatives:
    /// - Frame -> Border
    /// - ListView -> CollectionView
    /// - TableView -> CollectionView with grouping
    /// </remarks>
    public static MauiAppBuilder UseAvaloniaCompatibility(this MauiAppBuilder builder)
    {
        return builder.ConfigureMauiHandlers(handlers =>
        {
#pragma warning disable CS0618
            handlers.AddHandler<Microsoft.Maui.Controls.Frame, Handlers.FrameHandler>();
#pragma warning restore CS0618
            handlers.AddHandler<Microsoft.Maui.Controls.ListView, Handlers.ListViewHandler>();
            handlers.AddHandler<Microsoft.Maui.Controls.TableView, Handlers.TableViewHandler>();
            handlers.AddHandler<Microsoft.Maui.Controls.TextCell, TextCellHandler>();
            handlers.AddHandler<Microsoft.Maui.Controls.ImageCell, ImageCellHandler>();
            handlers.AddHandler<Microsoft.Maui.Controls.ViewCell, ViewCellHandler>();
            handlers.AddHandler<Microsoft.Maui.Controls.SwitchCell, SwitchCellHandler>();
            handlers.AddHandler<Microsoft.Maui.Controls.EntryCell, EntryCellHandler>();
        });
    }
}
