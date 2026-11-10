using System;
using Microsoft.Maui;
using IImage = Avalonia.Media.IImage;

namespace Avalonia.Controls.Maui.Services;

/// <summary>
/// Wraps an Avalonia <see cref="IImage"/> as an <see cref="IImageSourceServiceResult{IImage}"/>.
/// </summary>
/// <remarks>
/// The value is a <see cref="Avalonia.Media.Imaging.Bitmap"/> for bitmap sources, or a vector image
/// such as a <see cref="Avalonia.Media.DrawingImage"/>. Disposing the result disposes the value when it
/// is disposable, then runs the optional dispose action.
/// </remarks>
public class ImageSourceServiceResult : IImageSourceServiceResult<IImage>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ImageSourceServiceResult"/> class.
    /// </summary>
    /// <param name="value">The loaded image.</param>
    /// <param name="resolutionDependent">
    /// <see langword="true"/> when the image was loaded for a particular display scale and should be
    /// loaded again when the scale changes.
    /// </param>
    /// <param name="dispose">An optional callback invoked during disposal for additional cleanup.</param>
    public ImageSourceServiceResult(IImage value, bool resolutionDependent = false, Action? dispose = null)
    {
        Value = value;
        IsResolutionDependent = resolutionDependent;
        DisposeAction = dispose;
    }

    /// <summary>
    /// Gets the loaded image.
    /// </summary>
    public IImage Value { get; }

    /// <summary>
    /// Gets a value indicating whether the image depends on the display scale it was loaded for.
    /// </summary>
    public bool IsResolutionDependent { get; }

    private Action? DisposeAction { get; }

    /// <summary>
    /// Gets a value indicating whether this result has been disposed.
    /// </summary>
    public bool IsDisposed { get; private set; }

    /// <summary>
    /// Disposes the image when it is disposable, then invokes any additional cleanup action.
    /// </summary>
    public void Dispose()
    {
        if (IsDisposed)
            return;

        IsDisposed = true;
        (Value as IDisposable)?.Dispose();
        DisposeAction?.Invoke();
    }
}
