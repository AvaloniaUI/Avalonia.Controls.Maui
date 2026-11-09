using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using Avalonia.Platform;
using Microsoft.Maui;
using Microsoft.Maui.Platform;
using Avalonia.Controls.Maui.Services;
using Avalonia.Controls.Maui.Platform;
using Avalonia.Controls.Maui.Extensions;
using Microsoft.Extensions.Logging;
using Avalonia.Controls.Maui.Controls.Gif;
using Avalonia.Animation;
using AImage = Avalonia.Controls.Image;
using AGrid = Avalonia.Controls.Grid;
using IImage = Microsoft.Maui.IImage;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Primitives;

namespace Avalonia.Controls.Maui.Handlers;

/// <summary>Avalonia handler for <see cref="IImage"/>.</summary>
public partial class ImageHandler : ViewHandler<IImage, AGrid>
{
    private readonly AImage _staticImage;
    private GifImage? _gifImage;
    private CancellationTokenSource? _loadCts;
    private ImageSourceLoader? _staticImageLoader;

    private static readonly ConcurrentDictionary<string, Uri?> AssetCache = new();

    /// <summary>Property mapper for <see cref="ImageHandler"/>.</summary>
    public static IPropertyMapper<IImage, ImageHandler> Mapper = new PropertyMapper<IImage, ImageHandler>(ViewHandler.ViewMapper)
    {
        [nameof(IImage.Aspect)] = MapAspect,
        [nameof(IImage.IsAnimationPlaying)] = MapIsAnimationPlaying,
        [nameof(IImage.Source)] = MapSource,
        [nameof(IView.Clip)] = MapClip,
        // IsLoading is read-only and updated automatically by the handler
    };

    /// <summary>Command mapper for <see cref="ImageHandler"/>.</summary>
    public static CommandMapper<IImage, ImageHandler> CommandMapper = new(ViewHandler.ViewCommandMapper);

    /// <summary>Initializes a new instance of <see cref="ImageHandler"/>.</summary>
    public ImageHandler() : base(Mapper, CommandMapper)
    {
        _staticImage = new AImage { IsVisible = true };
    }

    /// <summary>Initializes a new instance of <see cref="ImageHandler"/>.</summary>
    /// <param name="mapper">The property mapper to use, or <c>null</c> to use the default mapper.</param>
    public ImageHandler(IPropertyMapper? mapper) : base(mapper ?? Mapper, CommandMapper)
    {
        _staticImage = new AImage { IsVisible = true };
    }

    /// <summary>Initializes a new instance of <see cref="ImageHandler"/>.</summary>
    /// <param name="mapper">The property mapper to use, or <c>null</c> to use the default mapper.</param>
    /// <param name="commandMapper">The command mapper to use, or <c>null</c> to use the default command mapper.</param>
    public ImageHandler(IPropertyMapper? mapper, CommandMapper? commandMapper) : base(mapper ?? Mapper, commandMapper ?? CommandMapper)
    {
        _staticImage = new AImage { IsVisible = true };
    }

    /// <summary>Creates the Avalonia platform view for this handler.</summary>
    protected override AGrid CreatePlatformView()
    {
        var grid = new AGrid();
        var staticImage = _staticImage;
        grid.Children.Add(staticImage);
        return grid;
    }

    /// <inheritdoc/>
    protected override void ConnectHandler(AGrid platformView)
    {
        base.ConnectHandler(platformView);
        _staticImageLoader = new ImageSourceLoader(_staticImage, MauiContext!, ApplyStaticImage);
    }

    /// <inheritdoc/>
    protected override void DisconnectHandler(AGrid platformView)
    {
        _loadCts?.Cancel();
        _loadCts?.Dispose();
        _loadCts = null;

        _staticImageLoader?.Dispose();
        _staticImageLoader = null;

        ResetClip(_staticImage);
        _staticImage.Source = null;

        if (_gifImage != null)
        {
            ResetClip(_gifImage);
            try { _gifImage.Source = null!; } catch { }
            _gifImage = null;
        }

        base.DisconnectHandler(platformView);
    }

    /// <inheritdoc/>
    /// <remarks>
    /// As on iOS and Android, an image with no size request measures at most its natural size: it shrinks to fit
    /// the constraints but does not grow. A width or height request lets an <see cref="Aspect.AspectFit"/> image grow.
    /// </remarks>
    public override Microsoft.Maui.Graphics.Size GetDesiredSize(double widthConstraint, double heightConstraint)
    {
        var desired = base.GetDesiredSize(widthConstraint, heightConstraint);

        var dispatcher = PlatformViewOrNull?.Dispatcher;
        if (dispatcher is null)
            return desired;

        return dispatcher.CheckAccess()
            ? MeasureStaticImage(widthConstraint, heightConstraint, desired)
            : dispatcher.InvokeAsync(() => MeasureStaticImage(widthConstraint, heightConstraint, desired)).GetAwaiter().GetResult();
    }

    private Microsoft.Maui.Graphics.Size MeasureStaticImage(double widthConstraint, double heightConstraint, Microsoft.Maui.Graphics.Size desired)
    {
        // Avalonia's Image scales up to the constraints, so a static image measures from its natural size instead
        if (VirtualView is not { } view || !_staticImage.IsVisible || _staticImage.Source is not { } source)
            return desired;

        var natural = new Microsoft.Maui.Graphics.Size(source.Size.Width, source.Size.Height);
        if (natural.Width <= 0 || natural.Height <= 0)
            return desired;

        var widthIsExplicit = Dimension.IsExplicitSet(view.Width);
        var heightIsExplicit = Dimension.IsExplicitSet(view.Height);
        // A request is clamped before fitting, as Android does, so the other axis follows the clamped size
        widthConstraint = widthIsExplicit
            ? Math.Min(Math.Max(view.Width, Dimension.ResolveMinimum(view.MinimumWidth)), view.MaximumWidth)
            : Math.Min(widthConstraint, view.MaximumWidth);
        heightConstraint = heightIsExplicit
            ? Math.Min(Math.Max(view.Height, Dimension.ResolveMinimum(view.MinimumHeight)), view.MaximumHeight)
            : Math.Min(heightConstraint, view.MaximumHeight);

        var size = FitImage(natural, widthConstraint, heightConstraint, view.Aspect, widthIsExplicit, heightIsExplicit);

        return new Microsoft.Maui.Graphics.Size(
            ResolveConstraints(size.Width, view.Width, view.MinimumWidth, view.MaximumWidth),
            ResolveConstraints(size.Height, view.Height, view.MinimumHeight, view.MaximumHeight));
    }

    /// <summary>
    /// The size an image asks for within the constraints, as MAUI's <c>SizeThatFitsImage</c> computes it on iOS.
    /// </summary>
    internal static Microsoft.Maui.Graphics.Size FitImage(
        Microsoft.Maui.Graphics.Size natural,
        double widthConstraint,
        double heightConstraint,
        Aspect aspect,
        bool widthIsExplicit,
        bool heightIsExplicit)
    {
        if (aspect != Aspect.AspectFit)
            return new Microsoft.Maui.Graphics.Size(Math.Min(natural.Width, widthConstraint), Math.Min(natural.Height, heightConstraint));

        // An unconstrained axis never limits the scale
        var widthRatio = double.IsPositiveInfinity(widthConstraint) ? double.PositiveInfinity : widthConstraint / natural.Width;
        var heightRatio = double.IsPositiveInfinity(heightConstraint) ? double.PositiveInfinity : heightConstraint / natural.Height;
        var scale = Math.Min(widthRatio, heightRatio);

        if (!widthIsExplicit && !heightIsExplicit)
            scale = Math.Min(scale, 1);

        return new Microsoft.Maui.Graphics.Size(natural.Width * scale, natural.Height * scale);
    }

    private static double ResolveConstraints(double measured, double explicitSize, double minimum, double maximum)
    {
        var resolved = Dimension.IsExplicitSet(explicitSize) ? explicitSize : measured;
        resolved = Math.Max(resolved, Dimension.ResolveMinimum(minimum));
        return Math.Min(resolved, maximum);
    }

    /// <summary>Maps the Source property to the platform view.</summary>
    /// <param name="handler">The handler.</param>
    /// <param name="image">The virtual view.</param>
    public static void MapSource(ImageHandler handler, IImage image)
    {
        if (handler is ImageHandler h)
        {
            h._loadCts?.Cancel();
            h._loadCts = new CancellationTokenSource();

            _ = h.LoadSourceAsync(image, h._loadCts.Token);
        }
    }
    
    /// <summary>Maps the IsAnimationPlaying property to the platform view.</summary>
    /// <param name="handler">The handler.</param>
    /// <param name="image">The virtual view.</param>
    public static void MapIsAnimationPlaying(ImageHandler handler, IImage image)
    {
        (handler.PlatformView as AGrid)?.UpdateIsAnimationPlaying(image.IsAnimationPlaying);
    }

    /// <summary>Maps the Aspect property to the platform view.</summary>
    /// <param name="handler">The handler.</param>
    /// <param name="image">The virtual view.</param>
    public static void MapAspect(ImageHandler handler, IImage image)
    {
        (handler.PlatformView as AGrid)?.UpdateAspect(image.Aspect);
    }

    /// <summary>Maps the Clip property to the platform view.</summary>
    /// <param name="handler">The handler.</param>
    /// <param name="view">The virtual view.</param>
    public static void MapClip(ImageHandler handler, IView view)
    {
        if (handler is ImageHandler imageHandler)
        {
            imageHandler.UpdateImageClip(view);
        }
    }

    private void UpdateImageClip(IView view)
    {
        // Apply clip to the inner image controls instead of the container Grid so the
        // clip coordinates stay aligned with the rendered image content.
        _staticImage.UpdateClip(view);

        if (_gifImage != null)
        {
            _gifImage.UpdateClip(view);
        }
    }

    private static void ResetClip(global::Avalonia.Controls.Control control)
    {
        global::Avalonia.Controls.Maui.Extensions.ViewExtensions.DisposeClipSubscription(control);
        control.Clip = null;
    }

    internal async Task LoadSourceAsync(IImage image, CancellationToken token)
    {
        if (image.Source == null)
        {
            UpdateIsLoading(false);
            ClearImages();
            return;
        }

        try
        {
            UpdateIsLoading(true);

            // Detect if source is a Gif
            bool isGif = image.Source switch
            {
                IFileImageSource fileSource when fileSource.File.EndsWith(".gif", StringComparison.OrdinalIgnoreCase) => true,
                IUriImageSource uriSource when uriSource.Uri.ToString().Contains(".gif", StringComparison.OrdinalIgnoreCase) => true,
                _ => false
            };

            if (isGif && await TryLoadGifAsync(image.Source, image.IsAnimationPlaying, token))
            {
                if (!token.IsCancellationRequested)
                    UpdateIsLoading(false);
                return;
            }

            // The static loader clears the loading state when it applies the image
            if (!token.IsCancellationRequested)
                LoadStatic(image.Source);
        }
        catch (Exception ex)
        {
            Log(LogLevel.Error, ex, "Fatal error loading image source");
            ClearImages();
            UpdateIsLoading(false);
        }
    }

    private void UpdateIsLoading(bool isLoading)
    {
        if (VirtualView is IImageController mauiImage)
        {
            mauiImage.SetIsLoading(isLoading);
        }
    }

    private async Task<bool> TryLoadGifAsync(IImageSource source, bool shouldPlay, CancellationToken token)
    {
        Uri? gifUri = null;
        
        try 
        {
            gifUri = await ResolveGifUriAsync(source);
        }
        catch (Exception ex)
        {
            Log(LogLevel.Warning, ex, "Failed to resolve GIF URI.");
        }

        if (gifUri == null)
            return false;

        if (token.IsCancellationRequested) return true;
        
        // This check prevents the NullReferenceException inside GifImage.InitializeGif
        if (gifUri.Scheme == "avares" && !AssetLoader.Exists(gifUri))
        {
            Log(LogLevel.Warning, null, $"GIF asset missing in bundle: {gifUri}. Falling back.");
            return false;
        }

        EnsureGifControl(VirtualView);

        if (!PlatformView.Children.Contains(_gifImage!))
        {
            PlatformView.Children.Add(_gifImage!);
        }

        try
        {
            _gifImage!.Source = GifStreamSource.FromUri(gifUri);
            await Task.Yield();

            // A newer source took over during the yield; the loader and visibility are now its to set
            if (token.IsCancellationRequested)
                return true;

            _gifImage.IterationCount = shouldPlay
                ? IterationCount.Infinite
                : new IterationCount(0);

            _staticImageLoader?.SetSource(null);
            _staticImage.IsVisible = false;
            _gifImage.IsVisible = true;
            return true;
        }
        catch (Exception ex)
        {
            PlatformView.Children.Remove(_gifImage!);
            _gifImage = null; 
            
            Log(LogLevel.Error, ex, "GifImage crashed during property assignment.");
            return false;
        }
    }
    
    private void EnsureGifControl(IView? view)
    {
        if (_gifImage == null)
        {
            _gifImage = new GifImage
            {
                IsVisible = false,
                Stretch = _staticImage.Stretch,
                Opacity = _staticImage.Opacity
            };
        }
        else
        {
            _gifImage.Stretch = _staticImage.Stretch;
            _gifImage.Opacity = _staticImage.Opacity;
        }

        if (view is not null)
        {
            _gifImage.UpdateClip(view);
        }
    }

    private void LoadStatic(IImageSource source)
    {
        if (_gifImage != null)
        {
            _gifImage.IsVisible = false;
            try { _gifImage.Source = null!; } catch { }
        }

        if (_staticImageLoader == null)
        {
            UpdateIsLoading(false);
            return;
        }

        _staticImageLoader.SetSource(source);
    }

    private void ApplyStaticImage(Avalonia.Media.IImage? image)
    {
        _staticImage.Source = image;
        if (image != null)
            _staticImage.IsVisible = true;

        UpdateIsLoading(false);
    }

    private void ClearImages()
    {
        _staticImageLoader?.SetSource(null);
        _staticImage.Source = null;
        _staticImage.IsVisible = false;
        if (_gifImage != null)
        {
            _gifImage.Source = null!;
            _gifImage.IsVisible = false;
        }
    }

    private async Task<Uri?> ResolveGifUriAsync(IImageSource source)
    {
        // 1. Handle Remote URLs (Download required)
        if (source is IUriImageSource uriSource && uriSource.Uri != null)
        {
            if (uriSource.Uri.Scheme == "http" || uriSource.Uri.Scheme == "https")
                return null;
            return uriSource.Uri;
        }

        if (source is IFileImageSource fileSource && !string.IsNullOrEmpty(fileSource.File))
        {
            var filePath = fileSource.File;
            
            // Check cache first
            if (AssetCache.TryGetValue(filePath, out var cachedUri))
                return cachedUri;

            if (Path.IsPathRooted(filePath) && File.Exists(filePath))
                return new Uri(filePath);
            
            if (TryFindEmbeddedGif(filePath, out var embeddedUri))
            {
                AssetCache[filePath] = embeddedUri;
                return embeddedUri;
            }
            
            // Avoid repeated scanning
            AssetCache[filePath] = null;
        }
        
        return null;
    }

    private bool TryFindEmbeddedGif(string fileName, out Uri? result)
    {
        return AvaloniaResourceHelper.TryResolveResourceUri(fileName, out result);
    }

    private void Log(LogLevel level, Exception? ex, string message)
    {
        var logger = this.GetRequiredService<ILoggerFactory>()?.CreateLogger<ImageHandler>();
        if (logger != null)
        {
            switch (level)
            {
                case LogLevel.Error: logger.LogError(ex, message); break;
                case LogLevel.Warning: logger.LogWarning(ex, message); break;
                default: logger.LogInformation(message); break;
            }
        }
    }

    /// <summary>Gets the image source part loader for this handler.</summary>
    public virtual ImageSourcePartLoader SourceLoader =>
        new ImageSourcePartLoader(new ImageImageSourcePartSetter(this));
        
    partial class ImageImageSourcePartSetter : ImageSourcePartSetter<ImageHandler>
    {
        public ImageImageSourcePartSetter(ImageHandler handler) : base(handler) { }
#if !IOS && !MACCATALYST && !ANDROID && !WINDOWS
        public override void SetImageSource(object? platformImage) { /* Handled by manual loaders */ }
#endif
    }
}
