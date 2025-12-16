using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Platform;

namespace Avalonia.Controls.Maui.Extensions;

/// <summary>
/// Extension methods for <see cref="MauiViewCell"/>.
/// </summary>
public static class ViewCellExtensions
{
    /// <summary>
    /// Updates the custom view content from the <see cref="ViewCell"/>.
    /// </summary>
    /// <param name="platformView">The platform view.</param>
    /// <param name="cell">The MAUI cell.</param>
    /// <param name="context">The MAUI context.</param>
    public static void UpdateView(this MauiViewCell platformView, ViewCell cell, IMauiContext? context)
    {
        if (context == null) return;

        platformView.Child = null;

        if (cell.View != null)
        {
            var viewPlatform = cell.View.ToPlatform(context);
            if (viewPlatform is Avalonia.Controls.Control control)
            {
                platformView.Child = control;
            }
        }
    }

    /// <summary>
    /// Updates the IsEnabled state from the <see cref="Cell"/>.
    /// </summary>
    /// <param name="platformView">The platform view.</param>
    /// <param name="cell">The MAUI cell.</param>
    public static void UpdateIsEnabled(this MauiViewCell platformView, Cell cell)
    {
        platformView.IsEnabled = cell.IsEnabled;
        // Reduce opacity for clearer disabled visual feedback
        platformView.Opacity = cell.IsEnabled ? 1.0 : 0.75;
    }
}
