using Avalonia.Controls.Maui.Handlers;
using Avalonia.Controls.Maui.Services;
using Avalonia.Controls.Primitives;
using Microsoft.Maui;
using PlatformView = Avalonia.Controls.Maui.Platform.MauiButton;

namespace Avalonia.Controls.Maui.Platform;

/// <summary>
/// Provides extension methods for updating <see cref="MauiButton"/> properties from cross-platform
/// .NET MAUI button interfaces. These methods handle the platform-specific implementation details
/// for rendering buttons on Avalonia-based platforms, including appearance, text styling, and interaction.
/// </summary>
/// <remarks>
/// This class is primarily used by <see cref="ButtonHandler"/> to apply property changes from the
/// cross-platform virtual view to the native Avalonia control. Each method is designed to be
/// lightweight and perform only the necessary updates to maintain optimal performance during
/// property change notifications.
/// </remarks>
public static class ButtonExtensions
{
    /// <summary>
    /// Updates the border stroke color of the button.
    /// </summary>
    /// <param name="platformView">The platform button control.</param>
    /// <param name="button">The cross-platform button.</param>
    public static void UpdateStrokeColor(this PlatformView platformView, IButton button)
    {
        if (button is not IButtonStroke stroke)
            return;

        if (stroke.StrokeColor != null)
        {
            platformView.BorderBrush = stroke.StrokeColor.ToPlatform();
        }
        else
        {
            platformView.ClearValue(TemplatedControl.BorderBrushProperty);
        }
    }

    /// <summary>
    /// Updates the border thickness of the button.
    /// </summary>
    /// <param name="platformView">The platform button control.</param>
    /// <param name="button">The cross-platform button.</param>
    public static void UpdateStrokeThickness(this PlatformView platformView, IButton button)
    {
        if (button is not IButtonStroke stroke)
            return;

        platformView.BorderThickness = new Thickness(stroke.StrokeThickness);
    }

    /// <summary>
    /// Updates the corner radius of the button.
    /// </summary>
    /// <param name="platformView">The platform button control.</param>
    /// <param name="button">The cross-platform button.</param>
    public static void UpdateCornerRadius(this PlatformView platformView, IButton button)
    {
        if (button is not IButtonStroke stroke)
            return;

        platformView.CornerRadius = new CornerRadius(stroke.CornerRadius);
    }

    /// <summary>
    /// Updates the text content of the button.
    /// </summary>
    /// <param name="platformView">The platform button control.</param>
    /// <param name="button">The cross-platform button.</param>
    public static void UpdateText(this PlatformView platformView, IButton button)
    {
        // Try to apply text transformation
        if (button is Microsoft.Maui.Controls.Button mauiButton)
        {
            platformView.Text = TextTransformUtilities.GetTransformedText(mauiButton.Text, mauiButton.TextTransform);
            return;
        }

        // Fall back to just use the text
        if (button is IText text)
        {
            platformView.Text = text.Text;
        }
    }

    /// <summary>
    /// Updates the text color of the button.
    /// </summary>
    /// <param name="platformView">The platform button control.</param>
    /// <param name="button">The cross-platform button.</param>
    public static void UpdateTextColor(this PlatformView platformView, IButton button)
    {
        if (button is not ITextStyle textStyle)
            return;

        if (textStyle.TextColor != null)
        {
            platformView.Foreground = textStyle.TextColor.ToPlatform();
        }
        else
        {
            platformView.ClearValue(TemplatedControl.ForegroundProperty);
        }
    }

    /// <summary>
    /// Updates the character spacing (letter spacing) of the button text.
    /// </summary>
    /// <param name="platformView">The platform button control.</param>
    /// <param name="button">The cross-platform button.</param>
    public static void UpdateCharacterSpacing(this PlatformView platformView, IButton button)
    {
        if (button is not ITextStyle textStyle)
            return;

        platformView.CharacterSpacing = textStyle.CharacterSpacing;

        var textBlock = platformView.GetTextBlock();

        if (textBlock != null)
        {
            textBlock.LetterSpacing = textStyle.CharacterSpacing;
        }
    }

    /// <summary>
    /// Updates the font properties of the button text including family, size, weight, and style.
    /// </summary>
    /// <param name="platformView">The platform button control.</param>
    /// <param name="button">The cross-platform button.</param>
    /// <param name="fontManager">The font manager service for resolving fonts.</param>
    public static void UpdateFont(this PlatformView platformView, IButton button, IFontManager fontManager)
    {
        if (button is not ITextStyle textStyle)
            return;

        var textBlock = platformView.GetTextBlock();

        if (textBlock is null)
            return;

        textBlock.UpdateFont(textStyle, fontManager);
    }

    /// <summary>
    /// Updates the internal padding of the button.
    /// </summary>
    /// <param name="platformView">The platform button control.</param>
    /// <param name="button">The cross-platform button.</param>
    public static void UpdatePadding(this PlatformView platformView, IButton button)
    {
        if (button is not IPadding padding)
            return;

        platformView.Padding = padding.Padding.ToThickness();
    }

    /// <summary>
    /// Updates the image source for the button asynchronously.
    /// </summary>
    /// <param name="platformView">The platform button control.</param>
    /// <param name="button">The cross-platform button.</param>
    /// <param name="imageSourceServiceProvider">The image source service provider.</param>
    public static async Task UpdateImageSourceAsync(this PlatformView platformView, IButton button, IImageSourceServiceProvider imageSourceServiceProvider)
    {
        if (button is not IImageSourcePart imageSourcePart)
            return;

        var imageSource = imageSourcePart.Source;
        if (imageSource == null)
        {
            platformView.ImageSource = null;
            return;
        }

        try
        {
            var serviceSource = imageSourceServiceProvider.GetImageSourceService(imageSource.GetType());

            if (serviceSource is IAvaloniaImageSourceService service)
            {
                var result = await service.GetImageAsync(imageSource, 1.0f);
                if (result?.Value is global::Avalonia.Media.Imaging.Bitmap bitmap)
                {
                    platformView.ImageSource = bitmap;
                }
                else
                {
                    platformView.ImageSource = null;
                }
            }
            else
            {
                platformView.ImageSource = null;
            }
        }
        catch
        {
            platformView.ImageSource = null;
        }
    }
}