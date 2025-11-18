using Avalonia.Platform;
using Microsoft.Maui;
using Microsoft.Maui.Handlers;
using Microsoft.Maui.Platform;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Avalonia.Controls.Maui.Services;
using Microsoft.Extensions.Logging;
using PlatformView = Avalonia.Controls.Image;
using Microsoft.Extensions.DependencyInjection;

namespace Avalonia.Controls.Maui.Handlers;

public partial class ImageHandler : ViewHandler<IImage, global::Avalonia.Controls.Image>, IImageHandler
{
    public static IPropertyMapper<IImage, IImageHandler> Mapper = new PropertyMapper<IImage, IImageHandler>(ViewHandler.ViewMapper)
    {
        [nameof(IImage.Aspect)] = MapAspect,
        [nameof(IImage.IsAnimationPlaying)] = MapIsAnimationPlaying,
        [nameof(IImage.Source)] = MapSource,
    };

    public static void MapSource(IImageHandler handler, IImage image) =>
        MapSourceAsync(handler, image).FireAndForget(handler);

    public static async Task MapSourceAsync(IImageHandler handler, IImage image)
    {
        var platformView = (PlatformView)handler.PlatformView;

        if (image.Source == null)
        {
            platformView.Source = null;
            return;
        }

        try
        {
            IImageSourceServiceProvider? _imageSourceServiceProvider = handler.GetRequiredService<IImageSourceServiceProvider>();

            var serviceSource = _imageSourceServiceProvider?.GetImageSourceService(image.Source.GetType());

            if (serviceSource is IAvaloniaImageSourceService service)
            {
                var result = await service.GetImageAsync(image.Source, 1.0f);
                if (result?.Value is global::Avalonia.Media.Imaging.Bitmap bitmap)
                {
                    platformView.Source = bitmap;
                }
                else
                {
                    platformView.Source = null;
                }
            }
            else
            {
                platformView.Source = null;
            }
        }
        catch (Exception ex)
        {
            handler.GetRequiredService<ILoggerFactory>()
                ?.CreateLogger<ImageHandler>()
                ?.LogError(ex, "Error loading image source");
            platformView.Source = null;
        }
    }


    private static void MapIsAnimationPlaying(IImageHandler handler, IImage image)
    {
    }

    private static void MapAspect(IImageHandler handler, IImage image)
    {
    }

    public static CommandMapper<IImage, IImageHandler> CommandMapper = new(ViewHandler.ViewCommandMapper)
    {
    };

    ImageSourcePartLoader? _imageSourcePartLoader;

    public virtual ImageSourcePartLoader SourceLoader =>
        _imageSourcePartLoader ??= new ImageSourcePartLoader(new ImageImageSourcePartSetter(this));

    public ImageHandler() : base(Mapper, CommandMapper)
    {
    }

    public ImageHandler(IPropertyMapper? mapper)
        : base(mapper ?? Mapper, CommandMapper)
    {
    }

    public ImageHandler(IPropertyMapper? mapper, CommandMapper? commandMapper)
        : base(mapper ?? Mapper, commandMapper ?? CommandMapper)
    {
    }


    // TODO MAUI: Should we remove all shadowing? 
    IImage IImageHandler.VirtualView => VirtualView;

    object IImageHandler.PlatformView => PlatformView;

    partial class ImageImageSourcePartSetter : ImageSourcePartSetter<IImageHandler>
    {
        public ImageImageSourcePartSetter(IImageHandler handler)
            : base(handler)
        {
        }

        public override void SetImageSource(object? platformImage)
        {
            throw new NotImplementedException();
        }
    }

    protected override PlatformView CreatePlatformView()
    {
        return new global::Avalonia.Controls.Image();
    }
}