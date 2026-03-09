using Avalonia.Interactivity;
using Avalonia.Controls.Maui.Platform;
using Avalonia.Controls.Maui.Services;
using Avalonia.Input;
using Microsoft.Maui;
using Microsoft.Maui.Platform;
using System.Threading;
using System.Threading.Tasks;
using PlatformView = Avalonia.Controls.Maui.MauiImageButton;

namespace Avalonia.Controls.Maui.Handlers;

/// <summary>Avalonia handler for <see cref="IImageButton"/>.</summary>
public partial class ImageButtonHandler : ViewHandler<IImageButton, PlatformView>
{
    private CancellationTokenSource? _imageSourceCts;
    private ImageSourcePartLoader? _imageSourcePartLoader;

    /// <summary>Property mapper for <see cref="ImageButtonHandler"/>.</summary>
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

    /// <summary>Command mapper for <see cref="ImageButtonHandler"/>.</summary>
    public static CommandMapper<IImageButton, ImageButtonHandler> CommandMapper = new(ViewCommandMapper);

    /// <summary>Initializes a new instance of <see cref="ImageButtonHandler"/>.</summary>
    public ImageButtonHandler()
        : base(Mapper, CommandMapper)
    {
    }

    /// <summary>Initializes a new instance of <see cref="ImageButtonHandler"/>.</summary>
    /// <param name="mapper">The property mapper to use, or <c>null</c> to use the default mapper.</param>
    public ImageButtonHandler(IPropertyMapper? mapper)
        : base(mapper ?? Mapper, CommandMapper)
    {
    }

    /// <summary>Initializes a new instance of <see cref="ImageButtonHandler"/>.</summary>
    /// <param name="mapper">The property mapper to use, or <c>null</c> to use the default mapper.</param>
    /// <param name="commandMapper">The command mapper to use, or <c>null</c> to use the default command mapper.</param>
    public ImageButtonHandler(IPropertyMapper? mapper, CommandMapper? commandMapper)
        : base(mapper ?? Mapper, commandMapper ?? CommandMapper)
    {
    }

    /// <summary>Gets the image source part loader for this handler.</summary>
    public virtual ImageSourcePartLoader SourceLoader =>
        _imageSourcePartLoader ??= new ImageSourcePartLoader(new ImageButtonImageSourcePartSetter(this));


    /// <summary>Creates the Avalonia platform view for this handler.</summary>
    protected override MauiImageButton CreatePlatformView()
    {
        return new MauiImageButton();
    }

    /// <summary>Maps the Background property to the platform view.</summary>
    /// <param name="handler">The handler.</param>
    /// <param name="imageButton">The virtual view.</param>
    public static void MapBackground(ImageButtonHandler handler, IImageButton imageButton)
    {
        if (handler.PlatformView is not PlatformView platformView || handler.VirtualView is null)
            return;

        platformView.UpdateImageButtonBackground(handler.VirtualView);
    }

    /// <summary>Maps the StrokeColor property to the platform view.</summary>
    /// <param name="handler">The handler.</param>
    /// <param name="imageButton">The virtual view.</param>
    public static void MapStrokeColor(ImageButtonHandler handler, IImageButton imageButton)
    {
        if (handler.PlatformView is not PlatformView platformView || handler.VirtualView is null)
            return;

        platformView.UpdateStrokeColor(handler.VirtualView);
    }

    /// <summary>Maps the StrokeThickness property to the platform view.</summary>
    /// <param name="handler">The handler.</param>
    /// <param name="imageButton">The virtual view.</param>
    public static void MapStrokeThickness(ImageButtonHandler handler, IImageButton imageButton)
    {
        if (handler.PlatformView is not PlatformView platformView || handler.VirtualView is null)
            return;

        platformView.UpdateStrokeThickness(handler.VirtualView);
    }

    /// <summary>Maps the CornerRadius property to the platform view.</summary>
    /// <param name="handler">The handler.</param>
    /// <param name="imageButton">The virtual view.</param>
    public static void MapCornerRadius(ImageButtonHandler handler, IImageButton imageButton)
    {
        if (handler.PlatformView is not PlatformView platformView || handler.VirtualView is null)
            return;

        platformView.UpdateCornerRadius(handler.VirtualView);
    }

    /// <summary>Maps the Padding property to the platform view.</summary>
    /// <param name="handler">The handler.</param>
    /// <param name="imageButton">The virtual view.</param>
    public static void MapPadding(ImageButtonHandler handler, IImageButton imageButton)
    {
        if (handler.PlatformView is not PlatformView platformView || handler.VirtualView is null)
            return;

        platformView.UpdatePadding(handler.VirtualView);
    }

    /// <summary>Maps the Aspect property to the platform view.</summary>
    /// <param name="handler">The handler.</param>
    /// <param name="imageButton">The virtual view.</param>
    public static void MapAspect(ImageButtonHandler handler, IImageButton imageButton)
    {
        if (handler.PlatformView is not PlatformView platformView || handler.VirtualView is null)
            return;

        platformView.UpdateAspect(handler.VirtualView);
    }

    /// <summary>Maps the ImageSource property to the platform view.</summary>
    /// <param name="handler">The handler.</param>
    /// <param name="imageButton">The virtual view.</param>
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

    /// <inheritdoc/>
    protected override void ConnectHandler(PlatformView platformView)
    {
        base.ConnectHandler(platformView);

        platformView.AddHandler(InputElement.PointerPressedEvent, OnPointerPressed, RoutingStrategies.Tunnel);
        platformView.AddHandler(InputElement.PointerReleasedEvent, OnPointerReleased, RoutingStrategies.Tunnel);
        platformView.Click += OnClick;
    }

    /// <inheritdoc/>
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

#if !IOS && !MACCATALYST && !ANDROID && !WINDOWS
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
