using Microsoft.Maui;
using Microsoft.Maui.Platform;
#if IOS || MACCATALYST
using PlatformImage = UIKit.UIImage;
#elif ANDROID
using PlatformImage = Android.Graphics.Drawables.Drawable;
#elif WINDOWS
using PlatformImage = Microsoft.UI.Xaml.Media.ImageSource;
#else
using PlatformImage = System.Object;
#endif

namespace Avalonia.Controls.Maui.Handlers;

/// <summary>
/// Base implementation of <see cref="IImageSourcePartSetter"/> that holds a weak reference to the
/// owning handler. Mirrors MAUI's internal ImageSourcePartSetter, which is not accessible outside
/// the MAUI assemblies.
/// </summary>
/// <typeparam name="T">The handler type that owns the image source part.</typeparam>
internal abstract class ImageSourcePartSetter<T> : IImageSourcePartSetter
    where T : class, IElementHandler
{
    readonly WeakReference<T> _handler;

    public ImageSourcePartSetter(T handler) =>
        _handler = new(handler);

    public IImageSourcePart? ImageSourcePart =>
        Handler?.VirtualView as IImageSourcePart ?? Handler?.VirtualView as IImage;

    public T? Handler =>
        _handler.TryGetTarget(out var handler) ? handler : null;

    IElementHandler? IImageSourcePartSetter.Handler => Handler;

    public abstract void SetImageSource(PlatformImage? platformImage);
}
