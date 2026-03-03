using System.Linq;
using Avalonia.Animation;
using Avalonia.Controls;
using Avalonia.Media;
using Microsoft.Maui;
using Avalonia.Controls.Maui.Controls.Gif;

namespace Avalonia.Controls.Maui.Platform;

/// <summary>
/// Provides extension methods to map <see cref="Microsoft.Maui.IImage"/> properties onto the Avalonia
/// <see cref="Image"/> platform control.
/// </summary>
public static class ImageExtensions
{
    /// <summary>
    /// Updates the aspect (stretch) property on all image children in the container.
    /// </summary>
    /// <param name="container"></param>
    /// <param name="aspect"></param>
    public static void UpdateAspect(this Grid container, Aspect aspect)
    {
        var stretch = aspect switch
        {
            Aspect.AspectFill => Stretch.UniformToFill,
            Aspect.AspectFit => Stretch.Uniform,
            Aspect.Fill => Stretch.Fill,
            Aspect.Center => Stretch.None,
            _ => Stretch.Uniform
        };

        // Update static image
        var staticImage = container.Children.OfType<Image>().FirstOrDefault();
        if (staticImage != null)
            staticImage.Stretch = stretch;

        // Update GIF image if present
        var gifImage = container.Children.OfType<GifImage>().FirstOrDefault();
        if (gifImage != null)
            gifImage.Stretch = stretch;
    }

    /// <summary>
    /// Updates the animation playing state of a GIF image in the container.
    /// </summary>
    /// <param name="container"></param>
    /// <param name="shouldPlay"></param>
    public static void UpdateIsAnimationPlaying(this Grid container, bool shouldPlay)
    {
        var gifImage = container.Children.OfType<GifImage>().FirstOrDefault();
        if (gifImage != null && gifImage.IsVisible)
        {
            gifImage.IterationCount = shouldPlay
                ? Animation.IterationCount.Infinite
                : new Animation.IterationCount(0);
        }
    }
}
