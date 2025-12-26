using System.Threading;
using System.Threading.Tasks;
using Android.Content;
using Android.Graphics.Drawables;
using Android.Widget;
using Microsoft.Maui;

namespace Avalonia.Controls.Maui.Services;

public partial class AvaloniaFileImageSourceService
{
    /// <summary>
    /// Platform-specific implementation for Android.
    /// Returns null as Avalonia handles the actual image rendering.
    /// </summary>
    public Task<IImageSourceServiceResult<Drawable>?> GetDrawableAsync(IImageSource imageSource, Context context, CancellationToken cancellationToken = default)
    {
        // Avalonia handles image rendering, return null to indicate no native drawable
        return Task.FromResult<IImageSourceServiceResult<Drawable>?>(null);
    }

    /// <summary>
    /// Platform-specific implementation for Android.
    /// Returns null as Avalonia handles the actual image rendering.
    /// </summary>
    public Task<IImageSourceServiceResult?> LoadDrawableAsync(IImageSource imageSource, ImageView imageView, CancellationToken cancellationToken = default)
    {
        // Avalonia handles image rendering, return null to indicate no native drawable
        return Task.FromResult<IImageSourceServiceResult?>(null);
    }
}
