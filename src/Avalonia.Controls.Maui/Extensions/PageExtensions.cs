using Avalonia.Controls.Maui.Services;
using Avalonia.Media.Imaging;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Maui;
using Microsoft.Maui.Controls;
using AvaloniaPanel = Avalonia.Controls.Panel;

namespace Avalonia.Controls.Maui.Extensions;

/// <summary>
/// Extension methods for <see cref="Page" />.
/// </summary>
public static class PageExtensions
{
    /// <summary>
    /// Updates the background image source of a platform view.
    /// </summary>
    /// <param name="platformView">The platform view to update.</param>
    /// <param name="page">The MAUI page containing the background image source.</param>
    /// <param name="mauiContext">The MAUI context.</param>
    public static void UpdateBackgroundImageSource(this AvaloniaPanel platformView, Page page, IMauiContext? mauiContext)
    {
        if (mauiContext == null)
            return;

        if (page.BackgroundImageSource != null)
        {
            var provider = mauiContext.Services.GetRequiredService<IImageSourceServiceProvider>();
            var service = provider.GetImageSourceService(page.BackgroundImageSource);

            if (service != null)
            {
                service.GetImageAsync(page.BackgroundImageSource)
                    .ContinueWith(async task =>
                    {
                        if (task.IsCompletedSuccessfully && task.Result != null)
                        {
                            var result = task.Result;
                            
                            Bitmap? bitmap = null;
                            if (result is IImageSourceServiceResult<Bitmap> bitmapResult)
                            {
                                bitmap = bitmapResult.Value;
                            }
                            
                            if (bitmap != null)
                            {
                                await Threading.Dispatcher.UIThread.InvokeAsync(() =>
                                {
                                    platformView.Background = new global::Avalonia.Media.ImageBrush
                                    {
                                        Source = bitmap,
                                        Stretch = Media.Stretch.Fill
                                    };
                                });
                            }
                        }
                    });
            }
        }
        else
        {
            platformView.ClearValue(AvaloniaPanel.BackgroundProperty);
        }
    }

    /// <summary>
    /// Updates the background of a platform view.
    /// </summary>
    /// <param name="platformView">The platform view to update.</param>
    /// <param name="page">The MAUI page containing the background.</param>
    public static void UpdateBackground(this AvaloniaPanel platformView, Page page)
    {
        if (page?.Background != null)
        {
            platformView.Background = page.Background.ToPlatform();
        }
        else
        {
            platformView.ClearValue(AvaloniaPanel.BackgroundProperty);
        }
    }
}