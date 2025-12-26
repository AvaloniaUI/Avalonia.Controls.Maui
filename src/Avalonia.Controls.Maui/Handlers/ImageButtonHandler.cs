using Avalonia.Interactivity;
using Avalonia.Controls.Maui.Platform;
using Avalonia.Controls.Maui.Services;
using Avalonia.Input;
using Microsoft.Maui;
using Microsoft.Maui.Handlers;
using Microsoft.Maui.Platform;
using System.Threading;
using System.Threading.Tasks;
using PlatformView = Avalonia.Controls.Maui.MauiImageButton;

namespace Avalonia.Controls.Maui.Handlers;

public partial class ImageButtonHandler : ViewHandler<IImageButton, PlatformView>
{
    private CancellationTokenSource? _imageSourceCts;
    private ImageSourcePartLoader? _imageSourcePartLoader;

    public static IPropertyMapper<IImageButton, ImageButtonHandler> Mapper = new PropertyMapper<IImageButton, ImageButtonHandler>(ViewHandler.ViewMapper)
    {
        // IImage properties
        [nameof(IImage.Source)] = MapImageSource,
        [nameof(IImage.Aspect)] = MapAspect,

        // IButton properties
        [nameof(IImageButton.Background)] = MapBackground,
        [nameof(IImageButton.Padding)] = MapPadding,

        // IButtonStroke properties
        [nameof(IButtonStroke.StrokeThickness)] = MapStrokeThickness,
        [nameof(IButtonStroke.StrokeColor)] = MapStrokeColor,
        [nameof(IButtonStroke.CornerRadius)] = MapCornerRadius,
    };

    public static CommandMapper<IImageButton, ImageButtonHandler> CommandMapper = new(ViewCommandMapper);

    public ImageButtonHandler()
        : base(Mapper, CommandMapper)
    {
    }

    public ImageButtonHandler(IPropertyMapper? mapper)
        : base(mapper ?? Mapper, CommandMapper)
    {
    }

    public ImageButtonHandler(IPropertyMapper? mapper, CommandMapper? commandMapper)
        : base(mapper ?? Mapper, commandMapper ?? CommandMapper)
    {
    }

    public virtual ImageSourcePartLoader SourceLoader =>
        _imageSourcePartLoader ??= new ImageSourcePartLoader(new ImageButtonImageSourcePartSetter(this));
        

    protected override MauiImageButton CreatePlatformView()
    {
        return new MauiImageButton();
    }

    public static void MapBackground(ImageButtonHandler handler, IImageButton imageButton)
    {
        if (handler.PlatformView is not PlatformView platformView || handler.VirtualView is null)
            return;

        platformView.UpdateImageButtonBackground(handler.VirtualView);
    }

    public static void MapStrokeColor(ImageButtonHandler handler, IImageButton imageButton)
    {
        if (handler.PlatformView is not PlatformView platformView || handler.VirtualView is null)
            return;

        platformView.UpdateStrokeColor(handler.VirtualView);
    }

    public static void MapStrokeThickness(ImageButtonHandler handler, IImageButton imageButton)
    {
        if (handler.PlatformView is not PlatformView platformView || handler.VirtualView is null)
            return;

        platformView.UpdateStrokeThickness(handler.VirtualView);
    }

    public static void MapCornerRadius(ImageButtonHandler handler, IImageButton imageButton)
    {
        if (handler.PlatformView is not PlatformView platformView || handler.VirtualView is null)
            return;

        platformView.UpdateCornerRadius(handler.VirtualView);
    }

    public static void MapPadding(ImageButtonHandler handler, IImageButton imageButton)
    {
        if (handler.PlatformView is not PlatformView platformView || handler.VirtualView is null)
            return;

        platformView.UpdatePadding(handler.VirtualView);
    }

    public static void MapAspect(ImageButtonHandler handler, IImageButton imageButton)
    {
        if (handler.PlatformView is not PlatformView platformView || handler.VirtualView is null)
            return;

        platformView.UpdateAspect(handler.VirtualView);
    }

    public static void MapImageSource(ImageButtonHandler handler, IImageButton imageButton)
    {
        if (handler is not ImageButtonHandler imageButtonHandler || handler.VirtualView is null)
            return;

        imageButtonHandler._imageSourceCts?.Cancel();
        imageButtonHandler._imageSourceCts = null;

        var imageSource = imageButton.Source;

        if (imageSource == null)
        {
            if (handler.PlatformView is PlatformView pv)
            {
                pv.UpdateImageSource(null);
            }
            return;
        }

        var cts = new CancellationTokenSource();
        imageButtonHandler._imageSourceCts = cts;
        _ = imageButtonHandler.LoadImageSourceAsync(imageSource, cts.Token);
    }

    protected override void ConnectHandler(PlatformView platformView)
    {
        base.ConnectHandler(platformView);

        platformView.AddHandler(InputElement.PointerPressedEvent, OnPointerPressed, RoutingStrategies.Tunnel);
        platformView.AddHandler(InputElement.PointerReleasedEvent, OnPointerReleased, RoutingStrategies.Tunnel);
        platformView.Click += OnClick;
    }

    protected override void DisconnectHandler(PlatformView platformView)
    {
        platformView.RemoveHandler(InputElement.PointerPressedEvent, OnPointerPressed);
        platformView.RemoveHandler(InputElement.PointerReleasedEvent, OnPointerReleased);
        platformView.Click -= OnClick;
        _imageSourceCts?.Cancel();
        _imageSourceCts = null;

        base.DisconnectHandler(platformView);
    }

    void OnPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (VirtualView is IImageButton imageButton)
        {
            imageButton.Pressed();
        }
    }

    void OnClick(object? sender, RoutedEventArgs e)
    {
        if (VirtualView is IImageButton imageButton)
        {
            imageButton.Clicked();
        }
    }

    void OnPointerReleased(object? sender, PointerReleasedEventArgs e)
    {
        if (VirtualView is IImageButton imageButton)
        {
            imageButton.Released();
        }
    }

    private async Task LoadImageSourceAsync(IImageSource imageSource, CancellationToken token)
    {
        try
        {
            var provider = this.GetRequiredService<IImageSourceServiceProvider>();
            if (provider.GetImageSourceService(imageSource.GetType()) is IAvaloniaImageSourceService service)
            {
                var result = await service.GetImageAsync(imageSource, 1.0f, token);
                if (token.IsCancellationRequested)
                    return;

                if (PlatformView is PlatformView platformView)
                {
                    platformView.UpdateImageSource(result?.Value as Avalonia.Media.IImage);
                }
            }
            else if (PlatformView is PlatformView platformView)
            {
                platformView.UpdateImageSource(null);
            }
        }
        catch
        {
            if (PlatformView is PlatformView platformView)
            {
                platformView.UpdateImageSource(null);
            }
        }
    }

    partial class ImageButtonImageSourcePartSetter : ImageSourcePartSetter<ImageButtonHandler>
    {
        public ImageButtonImageSourcePartSetter(ImageButtonHandler handler)
            : base(handler)
        {
        }

#if !IOS && !MACCATALYST && !ANDROID
        public override void SetImageSource(object? platformImage)
        {
            if (Handler?.PlatformView is PlatformView imageButton)
            {
                imageButton.UpdateImageSource(platformImage as Avalonia.Media.IImage);
            }
        }
#endif
    }
}
