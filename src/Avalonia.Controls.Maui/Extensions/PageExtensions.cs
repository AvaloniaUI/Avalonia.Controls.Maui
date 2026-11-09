using Avalonia.Controls.Maui.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Maui;
using Microsoft.Maui.Controls;
using AvaloniaPanel = Avalonia.Controls.Panel;
using MauiPage = Microsoft.Maui.Controls.Page;

namespace Avalonia.Controls.Maui.Extensions;

/// <summary>
/// Extension methods for <see cref="MauiPage" />.
/// </summary>
public static class PageExtensions
{
    /// <summary>
    /// Updates the background image source of a platform view.
    /// </summary>
    /// <param name="platformView">The platform view to update.</param>
    /// <param name="page">The MAUI page containing the background image source.</param>
    /// <param name="mauiContext">The MAUI context.</param>
    public static void UpdateBackgroundImageSource(this AvaloniaPanel platformView, MauiPage page, IMauiContext? mauiContext)
    {
        if (mauiContext == null)
            return;

        if (page.BackgroundImageSource == null)
        {
            ImageSourceLoader.Clear(platformView);
            platformView.UpdateBackground(page);
            return;
        }

        ImageSourceLoader.Set(platformView, mauiContext, page.BackgroundImageSource, image =>
        {
            if (image == null)
            {
                platformView.UpdateBackground(page);
                return;
            }

            var brush = ImageBrushes.Create(image);
            brush.Stretch = Media.Stretch.Fill;
            platformView.Background = brush;
        });
    }

    /// <summary>
    /// Updates the background of a platform view.
    /// </summary>
    /// <param name="platformView">The platform view to update.</param>
    /// <param name="page">The MAUI page containing the background.</param>
    public static void UpdateBackground(this AvaloniaPanel platformView, MauiPage page)
    {
        if (!page.Background?.IsEmpty ?? false)
        {
            platformView.Background = page.Background.ToPlatform();
        }
        else if (page.BackgroundColor?.IsDefault() == false)
        {
            platformView.Background = page.BackgroundColor.ToPlatform();
        }
        else
        {
            platformView.ClearValue(AvaloniaPanel.BackgroundProperty);
        }
    }
}