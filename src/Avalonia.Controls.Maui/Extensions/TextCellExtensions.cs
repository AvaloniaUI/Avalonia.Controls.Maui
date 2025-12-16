using Microsoft.Maui.Controls;

namespace Avalonia.Controls.Maui.Extensions;

/// <summary>
/// Extension methods for <see cref="MauiTextCell"/>.
/// </summary>
public static class TextCellExtensions
{
    /// <summary>
    /// Updates the text of the primary label from the <see cref="TextCell"/>.
    /// </summary>
    /// <param name="platformView">The platform view.</param>
    /// <param name="cell">The MAUI cell.</param>
    public static void UpdateText(this MauiTextCell platformView, TextCell cell)
    {
        platformView.PrimaryLabel.Text = cell.Text ?? string.Empty;
        platformView.PrimaryLabel.IsVisible = !string.IsNullOrEmpty(cell.Text);
    }

    /// <summary>
    /// Updates the text of the secondary label from the <see cref="TextCell"/>.
    /// </summary>
    /// <param name="platformView">The platform view.</param>
    /// <param name="cell">The MAUI cell.</param>
    public static void UpdateDetail(this MauiTextCell platformView, TextCell cell)
    {
        platformView.SecondaryLabel.Text = cell.Detail ?? string.Empty;
        platformView.SecondaryLabel.IsVisible = !string.IsNullOrEmpty(cell.Detail);
    }

    /// <summary>
    /// Updates the color of the primary label from the <see cref="TextCell"/>.
    /// </summary>
    /// <param name="platformView">The platform view.</param>
    /// <param name="cell">The MAUI cell.</param>
    public static void UpdateTextColor(this MauiTextCell platformView, TextCell cell)
    {
        if (cell.TextColor != null)
        {
            platformView.PrimaryLabel.Foreground = cell.TextColor.ToAvaloniaBrush();
        }
        else
        {
            platformView.PrimaryLabel.ClearValue(Avalonia.Controls.TextBlock.ForegroundProperty);
        }
    }

    /// <summary>
    /// Updates the color of the secondary label from the <see cref="TextCell"/>.
    /// </summary>
    /// <param name="platformView">The platform view.</param>
    /// <param name="cell">The MAUI cell.</param>
    public static void UpdateDetailColor(this MauiTextCell platformView, TextCell cell)
    {
        if (cell.DetailColor != null)
        {
            platformView.SecondaryLabel.Foreground = cell.DetailColor.ToAvaloniaBrush();
        }
        else
        {
            platformView.SecondaryLabel.ClearValue(Avalonia.Controls.TextBlock.ForegroundProperty);
        }
    }

    /// <summary>
    /// Updates the IsEnabled state from the <see cref="Cell"/>.
    /// </summary>
    /// <param name="platformView">The platform view.</param>
    /// <param name="cell">The MAUI cell.</param>
    public static void UpdateIsEnabled(this MauiTextCell platformView, Cell cell)
    {
        platformView.IsEnabled = cell.IsEnabled;
        // Reduce opacity for clearer disabled visual feedback
        platformView.Opacity = cell.IsEnabled ? 1.0 : 0.75;
    }

    /// <summary>
    /// Updates the height from the <see cref="Cell"/>.
    /// </summary>
    /// <param name="platformView">The platform view.</param>
    /// <param name="cell">The MAUI cell.</param>
    public static void UpdateHeight(this MauiTextCell platformView, Cell cell)
    {
        // Cell.Height defaults to -1 (auto-size)
        if (cell.Height > 0)
        {
            platformView.Height = cell.Height;
            platformView.MinHeight = cell.Height;
        }
        else
        {
            platformView.ClearValue(Avalonia.Layout.Layoutable.HeightProperty);
            platformView.MinHeight = 44; // Default MinHeight
        }
    }

    /// <summary>
    /// Updates the context actions (right-click menu) from the <see cref="Cell"/>.
    /// </summary>
    /// <param name="platformView">The platform view.</param>
    /// <param name="cell">The MAUI cell.</param>
    public static void UpdateContextActions(this MauiTextCell platformView, Cell cell)
    {
        platformView.ContextMenu = CellExtensions.BuildContextMenu(cell);
    }
}
