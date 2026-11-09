using System;
using System.Threading;
using System.Threading.Tasks;
using Avalonia.Threading;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Maui;
using IImage = Avalonia.Media.IImage;

namespace Avalonia.Controls.Maui.Services;

/// <summary>
/// Loads an image source for a control at its <see cref="TopLevel"/>'s render scale, and loads it again
/// when the scale changes.
/// </summary>
/// <remarks>
/// The loader waits for the target to attach to a <see cref="TopLevel"/>, cancels a load that a newer
/// source, a detach or <see cref="Dispose"/> makes stale, and owns the result it applies: it disposes the
/// result it replaces and its own result on <see cref="Dispose"/>. All members are called on the UI thread.
/// </remarks>
internal sealed class ImageSourceLoader : IDisposable
{
    /// <summary>
    /// Holds the loader of an extension-method site, which has no handler lifetime to keep it in.
    /// </summary>
    static readonly AttachedProperty<ImageSourceLoader?> LoaderProperty =
        AvaloniaProperty.RegisterAttached<ImageSourceLoader, Control, ImageSourceLoader?>("Loader");

    readonly Control _target;
    readonly IMauiContext _context;
    readonly Action<IImage?> _apply;

    IImageSource? _source;
    CancellationTokenSource? _loadCts;
    IImageSourceServiceResult<IImage>? _result;
    double _resultScale;
    bool _loadNeeded;
    TopLevel? _topLevel;
    bool _disposed;

    /// <summary>
    /// Initializes a new instance of the <see cref="ImageSourceLoader"/> class.
    /// </summary>
    /// <param name="target">The control whose <see cref="TopLevel"/> gives the scale.</param>
    /// <param name="context">The MAUI context that provides the image source services.</param>
    /// <param name="apply">Called on the UI thread with each new image, or <see langword="null"/>.</param>
    public ImageSourceLoader(Control target, IMauiContext context, Action<IImage?> apply)
    {
        _target = target;
        _context = context;
        _apply = apply;

        _target.AttachedToVisualTree += OnAttachedToVisualTree;
        _target.DetachedFromVisualTree += OnDetachedFromVisualTree;
        Subscribe(TopLevel.GetTopLevel(_target));
    }

    /// <summary>
    /// Gets the scale the current image was loaded at.
    /// </summary>
    internal double ResultScale => _resultScale;

    /// <summary>
    /// Gets a value indicating whether the loader listens to a <see cref="TopLevel"/> for scale changes.
    /// </summary>
    internal bool IsSubscribed => _topLevel != null;

    /// <summary>
    /// Replaces the loader kept on <paramref name="target"/> with one for <paramref name="source"/>.
    /// </summary>
    public static ImageSourceLoader Set(Control target, IMauiContext context, IImageSource? source, Action<IImage?> apply)
    {
        target.GetValue(LoaderProperty)?.Dispose();

        var loader = new ImageSourceLoader(target, context, apply);
        target.SetValue(LoaderProperty, loader);
        loader.SetSource(source);
        return loader;
    }

    /// <summary>
    /// Disposes the loader kept on <paramref name="target"/>, if any.
    /// </summary>
    public static void Clear(Control target)
    {
        if (target.GetValue(LoaderProperty) is { } loader)
        {
            target.ClearValue(LoaderProperty);
            loader.Dispose();
        }
    }

    /// <summary>
    /// Starts a load for <paramref name="source"/>, or clears the image when it is <see langword="null"/>.
    /// </summary>
    public void SetSource(IImageSource? source)
    {
        if (_disposed)
            return;

        CancelLoad();
        _source = source;

        if (source == null)
        {
            _loadNeeded = false;
            ReplaceResult(null, 0);
            return;
        }

        _loadNeeded = true;
        if (_topLevel != null)
            StartLoad(_topLevel.RenderScaling);
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        if (_disposed)
            return;

        _disposed = true;
        CancelLoad();
        Unsubscribe();
        _target.AttachedToVisualTree -= OnAttachedToVisualTree;
        _target.DetachedFromVisualTree -= OnDetachedFromVisualTree;

        // Clear the image first so the control never draws a disposed bitmap
        if (_result != null)
        {
            _apply(null);
            _result.Dispose();
            _result = null;
        }
    }

    void OnAttachedToVisualTree(object? sender, VisualTreeAttachmentEventArgs e)
    {
        Subscribe(TopLevel.GetTopLevel(_target));
        if (_topLevel == null || _source == null)
            return;

        if (_loadNeeded || (_result is { IsResolutionDependent: true } && _resultScale != _topLevel.RenderScaling))
            StartLoad(_topLevel.RenderScaling);
    }

    void OnDetachedFromVisualTree(object? sender, VisualTreeAttachmentEventArgs e)
    {
        Unsubscribe();
        CancelLoad();
    }

    void OnScalingChanged(object? sender, EventArgs e)
    {
        if (_topLevel == null || _source == null)
            return;

        var scale = _topLevel.RenderScaling;
        if (_loadNeeded || (_result is { IsResolutionDependent: true } && _resultScale != scale))
            StartLoad(scale);
    }

    void Subscribe(TopLevel? topLevel)
    {
        if (ReferenceEquals(topLevel, _topLevel))
            return;

        Unsubscribe();
        _topLevel = topLevel;
        if (_topLevel != null)
            _topLevel.ScalingChanged += OnScalingChanged;
    }

    void Unsubscribe()
    {
        if (_topLevel != null)
        {
            _topLevel.ScalingChanged -= OnScalingChanged;
            _topLevel = null;
        }
    }

    void StartLoad(double scale)
    {
        CancelLoad();
        _loadNeeded = true;
        var cts = new CancellationTokenSource();
        _loadCts = cts;
        _ = LoadAsync(_source!, scale, cts);
    }

    void CancelLoad()
    {
        if (_loadCts != null)
        {
            _loadCts.Cancel();
            _loadCts = null;
        }
    }

    async Task LoadAsync(IImageSource source, double scale, CancellationTokenSource cts)
    {
        IImageSourceServiceResult<IImage>? result = null;
        try
        {
            var provider = _context.Services.GetRequiredService<IImageSourceServiceProvider>();
            if (provider.ResolveImageSourceService(source) is IAvaloniaImageSourceService service)
                result = await service.GetImageAsync(source, (float)scale, cts.Token);
        }
        catch (Exception ex)
        {
            if (!cts.IsCancellationRequested)
            {
                _context.Services.GetService<ILoggerFactory>()
                    ?.CreateLogger<ImageSourceLoader>()
                    .LogWarning(ex, "Failed to load image source {Source}", source);
            }
        }

        if (!Dispatcher.UIThread.CheckAccess())
        {
            await Dispatcher.UIThread.InvokeAsync(() => Complete(result, scale, cts));
            return;
        }

        Complete(result, scale, cts);
    }

    void Complete(IImageSourceServiceResult<IImage>? result, double scale, CancellationTokenSource cts)
    {
        if (cts.IsCancellationRequested || !ReferenceEquals(cts, _loadCts) || _disposed)
        {
            result?.Dispose();
            cts.Dispose();
            return;
        }

        _loadCts = null;
        _loadNeeded = false;
        cts.Dispose();
        ReplaceResult(result, scale);
    }

    void ReplaceResult(IImageSourceServiceResult<IImage>? result, double scale)
    {
        var previous = _result;
        if (previous == null && result == null)
        {
            _apply(null);
            return;
        }

        _result = result;
        _resultScale = scale;
        _apply(result?.Value);

        // Dispose after replacing, so the control never draws a disposed bitmap
        if (previous != null && !ReferenceEquals(previous, result))
            previous.Dispose();
    }
}
