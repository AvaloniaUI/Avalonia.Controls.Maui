using System.Diagnostics.CodeAnalysis;
using Avalonia.Controls.Maui.Handlers;
using Avalonia.Controls.Primitives;
using Avalonia.Media;
using Microsoft.Maui;
using PlatformView = Avalonia.Controls.Maui.MauiButton;
using MButton = Microsoft.Maui.Controls.Button;

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
    /// Updates the background color of the button.
    /// </summary>
    /// <param name="platformView">The platform button control.</param>
    /// <param name="button">The cross-platform button.</param>
    public static void UpdateButtonBackground(this PlatformView platformView, IButton button)
    {
        if (button.Background != null)
        {
            var brush = button.Background.ToPlatform();
            platformView.Background = brush;
            if (brush != null)
                UpdateButtonBackgroundStates(platformView, brush);
        }
        else
        {
            platformView.ClearValue(TemplatedControl.BackgroundProperty);
            ClearButtonBackgroundStates(platformView);
        }
    }

    private static void UpdateButtonBackgroundStates(PlatformView platformView, IBrush brush)
    {
        if (brush is SolidColorBrush solidBrush)
        {
            var baseColor = solidBrush.Color;
            platformView.Resources["ButtonBackgroundPointerOver"] = new SolidColorBrush(LightenColor(baseColor, 0.15));
            platformView.Resources["ButtonBackgroundPressed"] = new SolidColorBrush(DarkenColor(baseColor, 0.12));
        }
        else
        {
            // For non-solid brushes (gradients, etc.), use the same brush for all states
            platformView.Resources["ButtonBackgroundPointerOver"] = brush;
            platformView.Resources["ButtonBackgroundPressed"] = brush;
        }
    }

    private static void ClearButtonBackgroundStates(PlatformView platformView)
    {
        platformView.Resources.Remove("ButtonBackgroundPointerOver");
        platformView.Resources.Remove("ButtonBackgroundPressed");
    }

    private static Color LightenColor(Color color, double factor)
    {
        return Color.FromArgb(
            color.A,
            (byte)Math.Min(color.R + (255 - color.R) * factor, 255),
            (byte)Math.Min(color.G + (255 - color.G) * factor, 255),
            (byte)Math.Min(color.B + (255 - color.B) * factor, 255));
    }

    private static Color DarkenColor(Color color, double factor)
    {
        return Color.FromArgb(
            color.A,
            (byte)(color.R * (1 - factor)),
            (byte)(color.G * (1 - factor)),
            (byte)(color.B * (1 - factor)));
    }
    
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

        if (stroke.StrokeThickness < 0)
        {
            platformView.ClearValue(TemplatedControl.BorderThicknessProperty);
        }
        else
        {
            platformView.BorderThickness = new Thickness(stroke.StrokeThickness);
        }
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

        if (stroke.CornerRadius < 0)
        {
            platformView.ClearValue(TemplatedControl.CornerRadiusProperty);
        }
        else
        {
            platformView.CornerRadius = new CornerRadius(stroke.CornerRadius);
        }
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
            var brush = textStyle.TextColor.ToPlatform();
            platformView.Foreground = brush;
            if (brush != null)
            {
                platformView.Resources["ButtonForegroundPointerOver"] = brush;
                platformView.Resources["ButtonForegroundPressed"] = brush;
            }
        }
        else
        {
            platformView.ClearValue(TemplatedControl.ForegroundProperty);
            platformView.Resources.Remove("ButtonForegroundPointerOver");
            platformView.Resources.Remove("ButtonForegroundPressed");
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

        var mauiPadding = padding.Padding;
        if (mauiPadding == Microsoft.Maui.Thickness.Zero)
        {
            platformView.ClearValue(TemplatedControl.PaddingProperty);
        }
        else if (double.IsNaN(mauiPadding.Left) && double.IsNaN(mauiPadding.Top) && double.IsNaN(mauiPadding.Right) && double.IsNaN(mauiPadding.Bottom))
        {
            return;
        }
        else
        {
            platformView.Padding = mauiPadding.ToPlatform();
        }
    }

    /// <summary>
    /// Updates the image source for the button once a platform image has been resolved.
    /// </summary>
    /// <param name="platformView">The platform button control.</param>
    /// <param name="image">The resolved Avalonia image.</param>
    public static void UpdateImageSource(this PlatformView platformView, Avalonia.Media.IImage? image)
    {
        platformView.ImageSource = image;
    }

    /// <summary>
    /// Updates the content layout (image position and spacing) of the button.
    /// </summary>
    /// <param name="platformView">The platform button control.</param>
    /// <param name="button">The cross-platform button.</param>
    public static void UpdateContentLayout(this PlatformView platformView, MButton button)
    {
        platformView.UpdateContentLayout(button.ContentLayout);
    }

    /// <summary>
    /// Updates the line break mode of the button.
    /// </summary>
    /// <param name="platformView">The platform button control.</param>
    /// <param name="button">The cross-platform button.</param>
    public static void UpdateLineBreakMode(this PlatformView platformView, IButton button)
    {
        if (button is MButton mauiButton)
        {
            platformView.LineBreakMode = mauiButton.LineBreakMode;
        }
    }
}
