using Avalonia.Media.Imaging;
using Microsoft.Maui.Media;

namespace Avalonia.Controls.Maui.Essentials;

public class AvaloniaScreenshotResult : IScreenshotResult
{
    readonly RenderTargetBitmap _bitmap;

    public AvaloniaScreenshotResult(RenderTargetBitmap bitmap, int width, int height)
    {
        _bitmap = bitmap;
        Width = width;
        Height = height;
    }

    public int Width { get; }
    public int Height { get; }

    public Task<Stream> OpenReadAsync(ScreenshotFormat format = ScreenshotFormat.Png, int quality = 100)
    {
        // Avalonia's Save() only supports PNG; format parameter is acknowledged but always produces PNG.
        // TODO: Add warning to tell user?
        var stream = new MemoryStream();
        _bitmap.Save(stream);
        stream.Position = 0;
        return Task.FromResult<Stream>(stream);
    }

    public async Task CopyToAsync(Stream destination, ScreenshotFormat format = ScreenshotFormat.Png, int quality = 100)
    {
        using var stream = await OpenReadAsync(format, quality).ConfigureAwait(false);
        await stream.CopyToAsync(destination).ConfigureAwait(false);
    }
}
