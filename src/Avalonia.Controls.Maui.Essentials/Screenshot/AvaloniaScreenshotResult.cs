using Avalonia.Media.Imaging;
using Microsoft.Maui.Media;

namespace Avalonia.Controls.Maui.Essentials;

/// <summary>
/// Wraps an Avalonia RenderTargetBitmap as an IScreenshotResult, providing stream-based access to the captured image data.
/// </summary>
/// <remarks>
/// The output is always PNG regardless of the requested ScreenshotFormat, because Avalonia's bitmap Save method only supports PNG encoding.
/// </remarks>
public class AvaloniaScreenshotResult : IScreenshotResult
{
    readonly RenderTargetBitmap _bitmap;

    /// <summary>
    /// Initializes a new instance of the <see cref="AvaloniaScreenshotResult"/> class with the specified rendered bitmap and dimensions.
    /// </summary>
    /// <param name="bitmap">The Avalonia RenderTargetBitmap containing the captured screenshot.</param>
    /// <param name="width">The width of the screenshot in pixels.</param>
    /// <param name="height">The height of the screenshot in pixels.</param>
    public AvaloniaScreenshotResult(RenderTargetBitmap bitmap, int width, int height)
    {
        _bitmap = bitmap;
        Width = width;
        Height = height;
    }

    /// <summary>
    /// Gets the width of the screenshot in pixels.
    /// </summary>
    public int Width { get; }

    /// <summary>
    /// Gets the height of the screenshot in pixels.
    /// </summary>
    public int Height { get; }

    /// <summary>
    /// Opens a readable stream containing the screenshot image data encoded as PNG.
    /// </summary>
    /// <param name="format">The desired screenshot format. This parameter is accepted but ignored; output is always PNG.</param>
    /// <param name="quality">The desired image quality (0-100). This parameter is accepted but ignored for PNG output.</param>
    /// <returns>A stream positioned at the beginning containing the PNG-encoded screenshot data.</returns>
    public Task<Stream> OpenReadAsync(ScreenshotFormat format = ScreenshotFormat.Png, int quality = 100)
    {
        // Avalonia's Save() only supports PNG; format parameter is acknowledged but always produces PNG.
        // TODO: Add warning to tell user?
        var stream = new MemoryStream();
        _bitmap.Save(stream);
        stream.Position = 0;
        return Task.FromResult<Stream>(stream);
    }

    /// <summary>
    /// Copies the screenshot image data encoded as PNG to the specified destination stream.
    /// </summary>
    /// <param name="destination">The stream to copy the screenshot data to.</param>
    /// <param name="format">The desired screenshot format. This parameter is accepted but ignored; output is always PNG.</param>
    /// <param name="quality">The desired image quality (0-100). This parameter is accepted but ignored for PNG output.</param>
    /// <returns>A task that represents the asynchronous copy operation.</returns>
    public async Task CopyToAsync(Stream destination, ScreenshotFormat format = ScreenshotFormat.Png, int quality = 100)
    {
        using var stream = await OpenReadAsync(format, quality).ConfigureAwait(false);
        await stream.CopyToAsync(destination).ConfigureAwait(false);
    }
}
