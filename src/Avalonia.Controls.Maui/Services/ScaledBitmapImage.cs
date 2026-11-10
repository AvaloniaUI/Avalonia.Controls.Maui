using System;
using Avalonia.Media;
using Avalonia.Media.Imaging;

namespace Avalonia.Controls.Maui.Services;

/// <summary>
/// A bitmap with more than one device pixel per device-independent unit, such as an <c>@2x</c> file variant.
/// </summary>
public sealed class ScaledBitmapImage : IImage, IDisposable
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ScaledBitmapImage"/> class.
    /// </summary>
    /// <param name="bitmap">The bitmap, which the image owns.</param>
    /// <param name="scale">The number of bitmap pixels per device-independent unit.</param>
    public ScaledBitmapImage(Bitmap bitmap, double scale)
    {
        ArgumentNullException.ThrowIfNull(bitmap);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(scale);

        Bitmap = bitmap;
        Scale = scale;
    }

    /// <summary>
    /// Gets the bitmap.
    /// </summary>
    public Bitmap Bitmap { get; }

    /// <summary>
    /// Gets the number of bitmap pixels per device-independent unit.
    /// </summary>
    public double Scale { get; }

    /// <inheritdoc/>
    public Size Size => new(Bitmap.PixelSize.Width / Scale, Bitmap.PixelSize.Height / Scale);

    /// <inheritdoc/>
    void IImage.Draw(DrawingContext context, Rect sourceRect, Rect destRect)
    {
        // Bitmap draws its source rectangle in pixels
        var pixels = new Rect(sourceRect.X * Scale, sourceRect.Y * Scale, sourceRect.Width * Scale, sourceRect.Height * Scale);
        context.DrawImage(Bitmap, pixels, destRect);
    }

    /// <inheritdoc/>
    public void Dispose() => Bitmap.Dispose();
}
