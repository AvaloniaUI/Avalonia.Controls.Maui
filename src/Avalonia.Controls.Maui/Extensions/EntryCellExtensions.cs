using Microsoft.Maui.Controls;

namespace Avalonia.Controls.Maui.Extensions;

/// <summary>
/// Extension methods for <see cref="MauiEntryCell"/>.
/// </summary>
public static class EntryCellExtensions
{
    /// <summary>
    /// Updates the label text from the <see cref="EntryCell"/>.
    /// </summary>
    /// <param name="platformView">The platform view.</param>
    /// <param name="cell">The MAUI cell.</param>
    public static void UpdateLabel(this MauiEntryCell platformView, EntryCell cell)
    {
        platformView.Label.Text = cell.Label ?? string.Empty;
        platformView.Label.IsVisible = !string.IsNullOrEmpty(cell.Label);
    }

    /// <summary>
    /// Updates the input text from the <see cref="EntryCell"/>.
    /// </summary>
    /// <param name="platformView">The platform view.</param>
    /// <param name="cell">The MAUI cell.</param>
    /// <param name="isUpdating">A flag indicating if the update originates from the platform.</param>
    public static void UpdateText(this MauiEntryCell platformView, EntryCell cell, bool isUpdating)
    {
        if (isUpdating) return;
        platformView.Input.Text = cell.Text ?? string.Empty;
    }

    /// <summary>
    /// Updates the placeholder text from the <see cref="EntryCell"/>.
    /// </summary>
    /// <param name="platformView">The platform view.</param>
    /// <param name="cell">The MAUI cell.</param>
    public static void UpdatePlaceholder(this MauiEntryCell platformView, EntryCell cell)
    {
        platformView.Input.Watermark = cell.Placeholder;
    }

    /// <summary>
    /// Updates the label color from the <see cref="EntryCell"/>.
    /// </summary>
    /// <param name="platformView">The platform view.</param>
    /// <param name="cell">The MAUI cell.</param>
    public static void UpdateLabelColor(this MauiEntryCell platformView, EntryCell cell)
    {
        if (cell.LabelColor != null)
        {
            platformView.Label.Foreground = cell.LabelColor.ToAvaloniaBrush();
        }
        else
        {
            platformView.Label.ClearValue(Avalonia.Controls.TextBlock.ForegroundProperty);
        }
    }

    /// <summary>
    /// Updates the horizontal text alignment from the <see cref="EntryCell"/>.
    /// </summary>
    /// <param name="platformView">The platform view.</param>
    /// <param name="cell">The MAUI cell.</param>
    public static void UpdateHorizontalTextAlignment(this MauiEntryCell platformView, EntryCell cell)
    {
        platformView.Input.TextAlignment = cell.HorizontalTextAlignment switch
        {
            Microsoft.Maui.TextAlignment.Start => Avalonia.Media.TextAlignment.Left,
            Microsoft.Maui.TextAlignment.Center => Avalonia.Media.TextAlignment.Center,
            Microsoft.Maui.TextAlignment.End => Avalonia.Media.TextAlignment.Right,
            _ => Avalonia.Media.TextAlignment.Left
        };
    }

    /// <summary>
    /// Updates the keyboard input scope from the <see cref="EntryCell"/>.
    /// </summary>
    /// <param name="platformView">The platform view.</param>
    /// <param name="cell">The MAUI cell.</param>
    public static void UpdateKeyboard(this MauiEntryCell platformView, EntryCell cell)
    {
        // Avalonia TextBox does not support InputScope (desktop platform)
        // This is a no-op but included for API completeness
    }

    /// <summary>
    /// Updates the vertical text alignment from the <see cref="EntryCell"/>.
    /// </summary>
    /// <param name="platformView">The platform view.</param>
    /// <param name="cell">The MAUI cell.</param>
    public static void UpdateVerticalTextAlignment(this MauiEntryCell platformView, EntryCell cell)
    {
        platformView.Input.VerticalContentAlignment = cell.VerticalTextAlignment switch
        {
            Microsoft.Maui.TextAlignment.Start => Avalonia.Layout.VerticalAlignment.Top,
            Microsoft.Maui.TextAlignment.Center => Avalonia.Layout.VerticalAlignment.Center,
            Microsoft.Maui.TextAlignment.End => Avalonia.Layout.VerticalAlignment.Bottom,
            _ => Avalonia.Layout.VerticalAlignment.Center
        };
    }

    /// <summary>
    /// Updates the IsEnabled state from the <see cref="Cell"/>.
    /// </summary>
    /// <param name="platformView">The platform view.</param>
    /// <param name="cell">The MAUI cell.</param>
    public static void UpdateIsEnabled(this MauiEntryCell platformView, Cell cell)
    {
        platformView.IsEnabled = cell.IsEnabled;
        // Reduce opacity for clearer disabled visual feedback
        platformView.Opacity = cell.IsEnabled ? 1.0 : 0.75;
    }

    /// <summary>
    /// Updates the context actions (right-click menu) from the <see cref="Cell"/>.
    /// </summary>
    /// <param name="platformView">The platform view.</param>
    /// <param name="cell">The MAUI cell.</param>
    public static void UpdateContextActions(this MauiEntryCell platformView, Cell cell)
    {
        platformView.ContextMenu = CellExtensions.BuildContextMenu(cell);
    }
}
