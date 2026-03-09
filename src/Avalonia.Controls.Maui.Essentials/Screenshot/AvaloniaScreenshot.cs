using Avalonia.Media.Imaging;
using Avalonia.Threading;
using Microsoft.Maui.Media;

namespace Avalonia.Controls.Maui.Essentials;

/// <summary>
/// Avalonia implementation of IScreenshot that captures the current TopLevel window by rendering it to a RenderTargetBitmap.
/// </summary>
public class AvaloniaScreenshot : IScreenshot
{
    readonly IAvaloniaEssentialsPlatformProvider _platformProvider;

    /// <summary>
    /// Initializes a new instance of the <see cref="AvaloniaScreenshot"/> class with the specified platform provider.
    /// </summary>
    /// <param name="platformProvider">The platform provider used to retrieve the Avalonia TopLevel window for screen capture.</param>
    public AvaloniaScreenshot(IAvaloniaEssentialsPlatformProvider platformProvider)
    {
        _platformProvider = platformProvider;
    }

    /// <summary>
    /// Gets a value indicating whether screen capture is supported; always returns <see langword="true"/> for Avalonia platforms.
    /// </summary>
    public bool IsCaptureSupported => true;

    /// <summary>
    /// Captures a screenshot of the current TopLevel window by rendering it to a bitmap on the UI thread.
    /// </summary>
    /// <returns>An IScreenshotResult containing the captured image data and its pixel dimensions.</returns>
    /// <exception cref="InvalidOperationException">Thrown when the TopLevel cannot be retrieved or the window has zero size.</exception>
    public async Task<IScreenshotResult> CaptureAsync()
    {
        var topLevel = _platformProvider.GetTopLevel()
            ?? throw new InvalidOperationException("Unable to get Avalonia TopLevel. Ensure the application has been fully initialized.");

        return await Dispatcher.UIThread.InvokeAsync(() =>
        {
            var scaling = topLevel.RenderScaling;
            var clientSize = topLevel.ClientSize;
            var pixelWidth = (int)(clientSize.Width * scaling);
            var pixelHeight = (int)(clientSize.Height * scaling);

            if (pixelWidth <= 0 || pixelHeight <= 0)
                throw new InvalidOperationException("Cannot capture a screenshot of a zero-sized window.");

            var pixelSize = new PixelSize(pixelWidth, pixelHeight);
            var dpi = new Vector(96 * scaling, 96 * scaling);
            var renderTarget = new RenderTargetBitmap(pixelSize, dpi);
            renderTarget.Render(topLevel);

            return (IScreenshotResult)new AvaloniaScreenshotResult(renderTarget, pixelWidth, pixelHeight);
        });
    }
}