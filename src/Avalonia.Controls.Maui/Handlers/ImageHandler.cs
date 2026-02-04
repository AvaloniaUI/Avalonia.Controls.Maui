using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using Avalonia.Platform;
using Microsoft.Maui;
using Microsoft.Maui.Platform;
using Avalonia.Controls.Maui.Services;
using Avalonia.Controls.Maui.Platform;
using Avalonia.Controls.Maui.Extensions;
using Microsoft.Extensions.Logging;
using Avalonia.Labs.Gif;
using Avalonia.Animation;
using AImage = Avalonia.Controls.Image;
using AGrid = Avalonia.Controls.Grid;
using IImage = Microsoft.Maui.IImage;
using Microsoft.Maui.Controls;

namespace Avalonia.Controls.Maui.Handlers;

public partial class ImageHandler : ViewHandler<IImage, AGrid>
{
    private readonly AImage _staticImage;
    private GifImage? _gifImage;
    private CancellationTokenSource? _loadCts;

    private static readonly ConcurrentDictionary<string, Uri?> AssetCache = new();

    public static IPropertyMapper<IImage, ImageHandler> Mapper = new PropertyMapper<IImage, ImageHandler>(ViewHandler.ViewMapper)
    {
        [nameof(IImage.Aspect)] = MapAspect,
        [nameof(IImage.IsAnimationPlaying)] = MapIsAnimationPlaying,
        [nameof(IImage.Source)] = MapSource,
        [nameof(IView.Opacity)] = MapOpacity,
        [nameof(IView.Clip)] = MapClip,
        // IsLoading is read-only and updated automatically by the handler
    };

    public static CommandMapper<IImage, ImageHandler> CommandMapper = new(ViewHandler.ViewCommandMapper);

    public ImageHandler() : base(Mapper, CommandMapper)
    {
        _staticImage = new AImage { IsVisible = true };
    }

    public ImageHandler(IPropertyMapper? mapper) : base(mapper ?? Mapper, CommandMapper)
    {
        _staticImage = new AImage { IsVisible = true };
    }

    public ImageHandler(IPropertyMapper? mapper, CommandMapper? commandMapper) : base(mapper ?? Mapper, commandMapper ?? CommandMapper)
    {
        _staticImage = new AImage { IsVisible = true };
    }

    protected override AGrid CreatePlatformView()
    {
        var grid = new AGrid();
        var staticImage = _staticImage;
        grid.Children.Add(staticImage);
        return grid;
    }

    public static void MapSource(ImageHandler handler, IImage image)
    {
        if (handler is ImageHandler h)
        {
            h._loadCts?.Cancel();
            h._loadCts = new CancellationTokenSource();

            _ = h.LoadSourceAsync(image, h._loadCts.Token);
        }
    }
    
    public static void MapIsAnimationPlaying(ImageHandler handler, IImage image)
    {
        (handler.PlatformView as AGrid)?.UpdateIsAnimationPlaying(image.IsAnimationPlaying);
    }

    public static void MapOpacity(ImageHandler handler, IView view)
    {
        (handler.PlatformView as AGrid)?.UpdateImageOpacity(view.Opacity);
    }

    public static void MapAspect(ImageHandler handler, IImage image)
    {
        (handler.PlatformView as AGrid)?.UpdateAspect(image.Aspect);
    }

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
            _gifImage!.Source = gifUri;
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

        if (provider.GetImageSourceService(source.GetType()) is IAvaloniaImageSourceService service)
        {
            try 
            {
                var result = await service.GetImageAsync(source, 1.0f, token);
                if (token.IsCancellationRequested) return;

                if (result?.Value is { } bitmap)
                {
                    _staticImage.Source = bitmap;
                    _staticImage.IsVisible = true;
                }
                else
                {
                    _staticImage.Source = null;
                }
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
        result = null;
        var targetName = Path.GetFileName(fileName);

        var assemblies = new[]
        {
            Microsoft.Maui.Controls.Application.Current?.GetType().Assembly,
            Assembly.GetEntryAssembly(),
            Assembly.GetExecutingAssembly()
        }.Where(x => x != null).Distinct();

        foreach (var assembly in assemblies)
        {
            if (assembly == null) continue;
            var assemblyName = assembly.GetName().Name;
            var rootUri = new Uri($"avares://{assemblyName}/");

            try
            {
                var assets = AssetLoader.GetAssets(rootUri, null);
                
                foreach (var assetUri in assets)
                {
                    // Match suffix (handles folders automatically)
                    if (assetUri.ToString().EndsWith(targetName, StringComparison.OrdinalIgnoreCase))
                    {
                        result = assetUri;
                        return true;
                    }
                }
            }
            catch
            {
                // Assembly might not have any Avalonia resources
            }
        }

        return false;
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
