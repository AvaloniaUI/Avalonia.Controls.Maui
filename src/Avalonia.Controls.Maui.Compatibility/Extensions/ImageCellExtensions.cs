using Avalonia.Controls.Maui.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Avalonia.Controls.Maui.Extensions;

namespace Avalonia.Controls.Maui.Compatibility;

/// <summary>
/// Extension methods for <see cref="MauiImageCell"/>.
/// </summary>
public static class ImageCellExtensions
{
    /// <summary>
    /// Updates the text of the primary label from the <see cref="ImageCell"/>.
    /// </summary>
    /// <param name="platformView">The platform view.</param>
    /// <param name="cell">The MAUI cell.</param>
    public static void UpdateText(this MauiImageCell platformView, ImageCell cell)
    {
        platformView.PrimaryLabel.Text = cell.Text ?? string.Empty;
        platformView.PrimaryLabel.IsVisible = !string.IsNullOrEmpty(cell.Text);
    }

    /// <summary>
    /// Updates the text of the secondary label from the <see cref="ImageCell"/>.
    /// </summary>
    /// <param name="platformView">The platform view.</param>
    /// <param name="cell">The MAUI cell.</param>
    public static void UpdateDetail(this MauiImageCell platformView, ImageCell cell)
    {
        platformView.SecondaryLabel.Text = cell.Detail ?? string.Empty;
        platformView.SecondaryLabel.IsVisible = !string.IsNullOrEmpty(cell.Detail);
    }

    /// <summary>
    /// Updates the color of the primary label from the <see cref="ImageCell"/>.
    /// </summary>
    /// <param name="platformView">The platform view.</param>
    /// <param name="cell">The MAUI cell.</param>
    public static void UpdateTextColor(this MauiImageCell platformView, ImageCell cell)
    {
        if (cell.TextColor != null)
        {
            platformView.PrimaryLabel.Foreground = cell.TextColor.ToAvaloniaBrush();
        }
        else
        {
            platformView.PrimaryLabel.ClearValue(TextBlock.ForegroundProperty);
        }
    }

    /// <summary>
    /// Updates the color of the secondary label from the <see cref="ImageCell"/>.
    /// </summary>
    /// <param name="platformView">The platform view.</param>
    /// <param name="cell">The MAUI cell.</param>
    public static void UpdateDetailColor(this MauiImageCell platformView, ImageCell cell)
    {
        if (cell.DetailColor != null)
        {
            platformView.SecondaryLabel.Foreground = cell.DetailColor.ToAvaloniaBrush();
        }
        else
        {
            platformView.SecondaryLabel.ClearValue(TextBlock.ForegroundProperty);
        }
    }

    /// <summary>
    /// Updates the image source from the <see cref="ImageCell"/>.
    /// </summary>
    /// <param name="platformView">The platform view.</param>
    /// <param name="cell">The MAUI cell.</param>
    /// <param name="context">The MAUI context.</param>
    public static void UpdateImageSource(this MauiImageCell platformView, ImageCell cell, IMauiContext? context)
    {
        if (context == null) return;

        if (cell.ImageSource != null)
        {
            platformView.ImageView.IsVisible = true;
            _ = LoadImageAsync(platformView, cell, context);
        }
        else
        {
            platformView.ImageView.IsVisible = false;
            platformView.ImageView.Source = null;
        }
    }

    private static async Task LoadImageAsync(MauiImageCell platformView, ImageCell cell, IMauiContext context)
    {
        try
        {
            var services = context.Services;
            var imageSourceServiceProvider = services.GetRequiredService<IImageSourceServiceProvider>();
            var imageSourceService = imageSourceServiceProvider.GetRequiredImageSourceService(cell.ImageSource);

            var result = await imageSourceService.GetImageAsync(cell.ImageSource, 1.0f);

            if (result is IImageSourceServiceResult<Avalonia.Media.Imaging.Bitmap> bitmapResult)
            {
                platformView.ImageView.Source = bitmapResult.Value;
            }
        }
        catch
        {
            platformView.ImageView.IsVisible = false;
            platformView.ImageView.Source = null;
        }
    }

    /// <summary>
    /// Updates the IsEnabled state from the <see cref="Cell"/>.
    /// </summary>
    /// <param name="platformView">The platform view.</param>
    /// <param name="cell">The MAUI cell.</param>
    public static void UpdateIsEnabled(this MauiImageCell platformView, Cell cell)
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
    public static void UpdateContextActions(this MauiImageCell platformView, Cell cell)
    {
        platformView.ContextMenu = CellExtensions.BuildContextMenu(cell);
    }
}