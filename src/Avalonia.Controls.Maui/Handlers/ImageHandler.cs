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

namespace Avalonia.Controls.Maui.Handlers;

/// <summary>Avalonia handler for <see cref="IImage"/>.</summary>
public partial class ImageHandler : ViewHandler<IImage, AGrid>
{
    private readonly AImage _staticImage;
    private GifImage? _gifImage;
    private CancellationTokenSource? _loadCts;
    private IDisposable? _currentImageResult;

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
    protected override void DisconnectHandler(AGrid platformView)
    {
        _loadCts?.Cancel();
        _loadCts?.Dispose();
        _loadCts = null;

        _currentImageResult?.Dispose();
        _currentImageResult = null;

        _staticImage.Source = null;

        if (_gifImage != null)
        {
            try { _gifImage.Source = null!; } catch { }
            _gifImage = null;
        }

        base.DisconnectHandler(platformView);
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
        // Apply clip to the inner image controls instead of the container Grid
        // This ensures the clip geometry coordinates align with the actual image content
        var avaloniaGeometry = (view.Clip as Microsoft.Maui.Controls.Shapes.Geometry)?.ToPlatform();

        _staticImage.Clip = avaloniaGeometry;

        if (_gifImage != null)
        {
            _gifImage.Clip = avaloniaGeometry;
        }
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

            if (isGif)
            {
                await LoadGifAsync(image.Source, image.IsAnimationPlaying, token);
            }
            else
            {
                await LoadStaticAsync(image.Source, token);
            }
        }
        catch (Exception ex)
        {
            Log(LogLevel.Error, ex, "Fatal error loading image source");
            ClearImages();
        }
        finally
        {
            // Clear loading state on success (unless cancelled)
            if (!token.IsCancellationRequested)
            {
                UpdateIsLoading(false);
            }
        }
    }

    private void UpdateIsLoading(bool isLoading)
    {
        if (VirtualView is IImageController mauiImage)
        {
            mauiImage.SetIsLoading(isLoading);
        }
    }

    private async Task LoadGifAsync(IImageSource source, bool shouldPlay, CancellationToken token)
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
        {
            // Fallback to static
            await LoadStaticAsync(source, token);
            return;
        }

        if (token.IsCancellationRequested) return;
        
        // This check prevents the NullReferenceException inside GifImage.InitializeGif
        if (gifUri.Scheme == "avares" && !AssetLoader.Exists(gifUri))
        {
            Log(LogLevel.Warning, null, $"GIF asset missing in bundle: {gifUri}. Falling back.");
            await LoadStaticAsync(source, token);
            return;
        }

        EnsureGifControl();

        if (!PlatformView.Children.Contains(_gifImage!))
        {
            PlatformView.Children.Add(_gifImage!);
        }

        try
        {
            _gifImage!.Source = GifStreamSource.FromUri(gifUri);
            await Task.Yield();
            
            _gifImage.IterationCount = shouldPlay
                ? IterationCount.Infinite
                : new IterationCount(0);

            _staticImage.IsVisible = false;
            _gifImage.IsVisible = true;
        }
        catch (Exception ex)
        {
            PlatformView.Children.Remove(_gifImage!);
            _gifImage = null; 
            
            Log(LogLevel.Error, ex, "GifImage crashed during property assignment.");
            await LoadStaticAsync(source, token);
        }
    }
    
    private void EnsureGifControl()
    {
        if (_gifImage == null)
        {
            _gifImage = new GifImage
            {
                IsVisible = false,
                Stretch = _staticImage.Stretch,
                Opacity = _staticImage.Opacity,
                Clip = _staticImage.Clip
            };
        }
        else
        {
            _gifImage.Stretch = _staticImage.Stretch;
            _gifImage.Opacity = _staticImage.Opacity;
            _gifImage.Clip = _staticImage.Clip;
        }
    }

    private async Task LoadStaticAsync(IImageSource source, CancellationToken token)
    {
        if (_gifImage != null)
        {
            _gifImage.IsVisible = false;
            try { _gifImage.Source = null!; } catch { }
        }

        var provider = this.GetRequiredService<IImageSourceServiceProvider>();

        if (provider.GetImageSourceService(GetImageSourceInterfaceType(source)) is IAvaloniaImageSourceService service)
        {
            try
            {
                var result = await service.GetImageAsync(source, 1.0f, token);
                if (token.IsCancellationRequested)
                {
                    (result as IDisposable)?.Dispose();
                    return;
                }

                var previousResult = _currentImageResult;

                if (result?.Value is { } bitmap)
                {
                    _staticImage.Source = bitmap;
                    _staticImage.IsVisible = true;
                    _currentImageResult = result as IDisposable;
                }
                else
                {
                    _staticImage.Source = null;
                    _currentImageResult = null;
                }

                // Dispose previous image result after replacing to avoid displaying a disposed bitmap
                previousResult?.Dispose();
            }
            catch (Exception ex)
            {
                Log(LogLevel.Warning, ex, "Failed to load static image fallback.");
                _staticImage.Source = null;
            }
        }
    }

    private void ClearImages()
    {
        _staticImage.Source = null;
        _staticImage.IsVisible = false;
        _currentImageResult?.Dispose();
        _currentImageResult = null;
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

    // UriImageSource implements both IUriImageSource and IStreamImageSource, which causes
    // an ambiguous match when MAUI's ImageSourceServiceProvider resolves by concrete type.
    // Resolve by interface type instead, checking more specific interfaces first.
    private static Type GetImageSourceInterfaceType(IImageSource source) => source switch
    {
        IFileImageSource => typeof(IFileImageSource),
        IFontImageSource => typeof(IFontImageSource),
        IUriImageSource => typeof(IUriImageSource),
        IStreamImageSource => typeof(IStreamImageSource),
        _ => source.GetType()
    };

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
