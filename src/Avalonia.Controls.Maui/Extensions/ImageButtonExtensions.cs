using Avalonia.Controls.Maui.Handlers;
using Avalonia.Controls.Primitives;
using Microsoft.Maui;
using PlatformView = Avalonia.Controls.Maui.MauiImageButton;

namespace Avalonia.Controls.Maui.Platform;

/// <summary>
/// Provides extension methods for updating <see cref="MauiImageButton"/> properties from cross-platform
/// .NET MAUI image button interfaces. These methods handle the platform-specific implementation details
/// for rendering image buttons on Avalonia-based platforms, including appearance and interaction.
/// </summary>
/// <remarks>
/// This class is primarily used by <see cref="ImageButtonHandler"/> to apply property changes from the
/// cross-platform virtual view to the native Avalonia control. Each method is designed to be
/// lightweight and perform only the necessary updates to maintain optimal performance during
/// property change notifications.
/// </remarks>
public static class ImageButtonExtensions
{
    /// <summary>
    /// Updates the background color of the image button.
    /// </summary>
    /// <param name="platformView">The platform image button control.</param>
    /// <param name="imageButton">The cross-platform image button.</param>
    public static void UpdateImageButtonBackground(this PlatformView platformView, IImageButton imageButton)
    {
        if (imageButton.Background != null)
        {
            platformView.Background = imageButton.Background.ToPlatform();
        }
        else
        {
            platformView.ClearValue(TemplatedControl.BackgroundProperty);
        }
    }

    /// <summary>
    /// Updates the border stroke color of the image button.
    /// </summary>
    /// <param name="platformView">The platform image button control.</param>
    /// <param name="imageButton">The cross-platform image button.</param>
    public static void UpdateStrokeColor(this PlatformView platformView, IImageButton imageButton)
    {
        if (imageButton is not IButtonStroke stroke)
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
    /// Updates the border thickness of the image button.
    /// </summary>
    /// <param name="platformView">The platform image button control.</param>
    /// <param name="imageButton">The cross-platform image button.</param>
    public static void UpdateStrokeThickness(this PlatformView platformView, IImageButton imageButton)
    {
        if (imageButton is not IButtonStroke stroke)
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
    /// Updates the corner radius of the image button.
    /// </summary>
    /// <param name="platformView">The platform image button control.</param>
    /// <param name="imageButton">The cross-platform image button.</param>
    public static void UpdateCornerRadius(this PlatformView platformView, IImageButton imageButton)
    {
        if (imageButton is not IButtonStroke stroke)
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
    /// Updates the internal padding of the image button.
    /// </summary>
    /// <param name="platformView">The platform image button control.</param>
    /// <param name="imageButton">The cross-platform image button.</param>
    public static void UpdatePadding(this PlatformView platformView, IImageButton imageButton)
    {
        if (imageButton is not IPadding padding)
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
    /// Updates the image source for the image button once a platform image has been resolved.
    /// </summary>
    /// <param name="platformView">The platform image button control.</param>
    /// <param name="image">The resolved Avalonia image.</param>
    public static void UpdateImageSource(this PlatformView platformView, Avalonia.Media.IImage? image)
    {
        platformView.ImageSource = image;
    }

    /// <summary>
    /// Updates the aspect (scaling mode) of the image button.
    /// </summary>
    /// <param name="platformView">The platform image button control.</param>
    /// <param name="imageButton">The cross-platform image button.</param>
    public static void UpdateAspect(this PlatformView platformView, IImageButton imageButton)
    {
        if (imageButton is not IImage image)
            return;

        platformView.UpdateAspect(image.Aspect);
    }
}
