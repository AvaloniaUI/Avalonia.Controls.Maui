using Avalonia.Media.Imaging;
using Avalonia.Threading;
using Microsoft.Maui.Media;

namespace Avalonia.Controls.Maui.Essentials;

public class AvaloniaScreenshot : IScreenshot
{
    readonly IAvaloniaEssentialsPlatformProvider _platformProvider;

    public AvaloniaScreenshot(IAvaloniaEssentialsPlatformProvider platformProvider)
    {
        _platformProvider = platformProvider;
    }

    public bool IsCaptureSupported => true;

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