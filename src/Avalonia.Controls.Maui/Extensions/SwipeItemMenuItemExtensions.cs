using System;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Controls.Maui.Platform;
using Avalonia.Layout;
using Avalonia.Media;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Maui;
using Microsoft.Maui.Handlers;
using Avalonia.Controls.Maui.Services;
using PlatformView = Avalonia.Controls.Button;
using AvaloniaTemplatedControl = Avalonia.Controls.Primitives.TemplatedControl;

namespace Avalonia.Controls.Maui;

/// <summary>
/// Extension methods for mapping ISwipeItemMenuItem to Avalonia Button control.
/// </summary>
public static class SwipeItemMenuItemExtensions
{
    /// <summary>
    /// Updates the visibility of the button from the SwipeItemMenuItem.
    /// </summary>
    public static void UpdateVisibility(this PlatformView platformView, ISwipeItemMenuItem menuItem)
    {
        platformView.IsVisible = menuItem.Visibility == Microsoft.Maui.Visibility.Visible;
    }

    /// <summary>
    /// Updates the background of the button from the SwipeItemMenuItem.
    /// </summary>
    public static void UpdateBackground(this PlatformView platformView, ISwipeItemMenuItem menuItem)
    {
        // Try IView.Background first (for custom implementations)
        if (menuItem is IView view && view.Background != null)
        {
            platformView.Background = view.Background.ToPlatform();
        }
        // Otherwise check if there's a BackgroundColor property (standard SwipeItem)
        else if (menuItem is Microsoft.Maui.Controls.SwipeItem swipeItem && swipeItem.BackgroundColor != null)
        {
            var color = swipeItem.BackgroundColor;
            platformView.Background = new SolidColorBrush(
                Color.FromArgb(
                    (byte)(color.Alpha * 255),
                    (byte)(color.Red * 255),
                    (byte)(color.Green * 255),
                    (byte)(color.Blue * 255)));
        }
        else
        {
            platformView.ClearValue(AvaloniaTemplatedControl.BackgroundProperty);
        }
    }

    /// <summary>
    /// Updates the text content of the button from the SwipeItemMenuItem.
    /// </summary>
    public static void UpdateText(this PlatformView platformView, IMenuElement menuElement)
    {
        platformView.Content = menuElement.Text;
    }

    /// <summary>
    /// Updates the text color (foreground) of the button from the SwipeItemMenuItem.
    /// </summary>
    public static void UpdateTextColor(this PlatformView platformView, ITextStyle textStyle)
    {
        if (textStyle.TextColor != null)
        {
            var color = textStyle.TextColor;
            platformView.Foreground = new SolidColorBrush(
                Color.FromArgb(
                    (byte)(color.Alpha * 255),
                    (byte)(color.Red * 255),
                    (byte)(color.Green * 255),
                    (byte)(color.Blue * 255)));
        }
        else
        {
            platformView.ClearValue(AvaloniaTemplatedControl.ForegroundProperty);
        }
    }

    /// <summary>
    /// Updates the character spacing of the button text.
    /// </summary>
    public static void UpdateCharacterSpacing(this PlatformView platformView, ITextStyle textStyle)
    {
        // Avalonia doesn't have direct character spacing support on Button
        // This would require creating a custom TextBlock as content
        // For now, we'll skip this
    }

    /// <summary>
    /// Updates the font properties of the button from the SwipeItemMenuItem.
    /// </summary>
    public static void UpdateFont(this PlatformView platformView, ITextStyle textStyle, IElementHandler handler)
    {
        var fontManager = handler.GetRequiredService<IFontManager>();
        // Update font using the IFontManager
        var font = textStyle.Font;

        if (font.IsDefault)
            return;

        platformView.FontSize = fontManager.GetFontSizeAsDouble(font);
        platformView.FontFamily = Avalonia.Controls.Maui.FontManagerExtensions.GetFontFamily(fontManager, font);
        platformView.FontStyle = FontManager.ToAvaloniaFontStyle(font.Slant);
        platformView.FontWeight = FontManager.ToAvaloniaFontWeight(font.Weight);
    }

    /// <summary>
    /// Updates the icon/image source of the button from the SwipeItemMenuItem.
    /// </summary>
    public static async Task UpdateSourceAsync(this PlatformView platformView, IMenuElement menuElement, IElementHandler handler)
    {
        if (menuElement.Source == null)
        {
            // Reset to text-only button
            platformView.Content = menuElement.Text;
            return;
        }

        try
        {
            var imageSourceServiceProvider = handler.GetRequiredService<IImageSourceServiceProvider>();
            var serviceSource = imageSourceServiceProvider?.GetImageSourceService(menuElement.Source.GetType());

            if (serviceSource is IAvaloniaImageSourceService service)
            {
                var result = await service.GetImageAsync(menuElement.Source, 1.0f);
                if (result?.Value is global::Avalonia.Media.Imaging.Bitmap bitmap)
                {
                    // Create a StackPanel with image and text
                    var stackPanel = new StackPanel
                    {
                        Orientation = Orientation.Horizontal,
                        Spacing = 8,
                        HorizontalAlignment = HorizontalAlignment.Center,
                        VerticalAlignment = VerticalAlignment.Center
                    };

                    var image = new Image
                    {
                        Source = bitmap,
                        Width = 16,
                        Height = 16,
                        VerticalAlignment = VerticalAlignment.Center
                    };

                    stackPanel.Children.Add(image);

                    if (!string.IsNullOrEmpty(menuElement.Text))
                    {
                        var textBlock = new TextBlock
                        {
                            Text = menuElement.Text,
                            VerticalAlignment = VerticalAlignment.Center
                        };
                        stackPanel.Children.Add(textBlock);
                    }

                    platformView.Content = stackPanel;
                }
                else
                {
                    // Fallback to text
                    platformView.Content = menuElement.Text;
                }
            }
            else
            {
                // Fallback to text
                platformView.Content = menuElement.Text;
            }
        }
        catch (Exception ex)
        {
            handler.GetRequiredService<ILoggerFactory>()
                ?.CreateLogger(typeof(SwipeItemMenuItemExtensions))
                ?.LogError(ex, "Error loading SwipeItem icon source");

            // Fallback to text
            platformView.Content = menuElement.Text;
        }
    }
}
