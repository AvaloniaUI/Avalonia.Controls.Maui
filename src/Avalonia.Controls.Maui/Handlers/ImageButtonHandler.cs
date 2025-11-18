using Avalonia.Interactivity;
using Avalonia.Controls.Maui.Platform;
using Microsoft.Maui;
using Microsoft.Maui.Handlers;
using Microsoft.Maui.Platform;
using PlatformView = Avalonia.Controls.Maui.Platform.MauiImageButton;

namespace Avalonia.Controls.Maui.Handlers;

public class ImageButtonHandler : ViewHandler<IImageButton, MauiImageButton>, IImageButtonHandler
{
    public static IPropertyMapper<IImage, IImageHandler> ImageMapper = new PropertyMapper<IImage, IImageHandler>(ImageHandler.Mapper);

    public static IPropertyMapper<IImageButton, IImageButtonHandler> Mapper = new PropertyMapper<IImageButton, IImageButtonHandler>(ImageMapper, ViewHandler.ViewMapper)
    {
        [nameof(IButtonStroke.StrokeThickness)] = MapStrokeThickness,
        [nameof(IButtonStroke.StrokeColor)] = MapStrokeColor,
        [nameof(IButtonStroke.CornerRadius)] = MapCornerRadius,
        [nameof(IImageButton.Padding)] = MapPadding,
        [nameof(IImageButton.Background)] = MapBackground,
    };

    public static CommandMapper<IImageButton, IImageButtonHandler> CommandMapper = new(ViewCommandMapper);

    private ImageSourcePartLoader? _imageSourcePartLoader;

    public virtual ImageSourcePartLoader SourceLoader =>
        _imageSourcePartLoader ??= new ImageSourcePartLoader(new ImageButtonImageSourcePartSetter(this));

    public ImageButtonHandler() : base(Mapper, CommandMapper)
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

    IImageButton IImageButtonHandler.VirtualView => VirtualView;

    IImage IImageHandler.VirtualView => VirtualView;

    object IImageHandler.PlatformView =>
        PlatformView.GetImage() ?? throw new InvalidOperationException("ImageButton did not contain an Image element.");

    object IImageButtonHandler.PlatformView => PlatformView;

    ImageSourcePartLoader IImageHandler.SourceLoader => SourceLoader;

    protected override MauiImageButton CreatePlatformView()
    {
        return new MauiImageButton();
    }

    public static void MapBackground(IImageButtonHandler handler, IImageButton imageButton)
    {
        if (handler.PlatformView is null || handler.VirtualView is null)
            return;
        ((PlatformView)(handler.PlatformView)).Background = imageButton.Background?.ToPlatform();
    }

    public static void MapStrokeColor(IImageButtonHandler handler, IButtonStroke buttonStroke)
    {
        if (handler.PlatformView is null || handler.VirtualView is null)
            return;
        if (buttonStroke.StrokeColor == null)
            return;
        ((PlatformView)(handler.PlatformView)).BorderBrush = buttonStroke.StrokeColor.ToPlatform();
    }

    public static void MapStrokeThickness(IImageButtonHandler handler, IButtonStroke buttonStroke)
    {
        if (handler.PlatformView is null || handler.VirtualView is null)
            return;
        ((PlatformView)(handler.PlatformView)).BorderThickness = new global::Avalonia.Thickness(buttonStroke.StrokeThickness);
    }

    public static void MapCornerRadius(IImageButtonHandler handler, IButtonStroke buttonStroke)
    {
        if (handler.PlatformView is null || handler.VirtualView is null)
            return;
        ((PlatformView)(handler.PlatformView)).CornerRadius = new global::Avalonia.CornerRadius(buttonStroke.CornerRadius);
    }

    public static void MapPadding(IImageButtonHandler handler, IImageButton imageButton)
    {
        if (handler.PlatformView is null || handler.VirtualView is null)
            return;
        ((PlatformView)(handler.PlatformView)).Padding = imageButton.Padding.ToThickness();
    }

    protected override void ConnectHandler(PlatformView platformView)
    {
        platformView.Click += OnClick;
        base.ConnectHandler(platformView);
    }

    protected override void DisconnectHandler(PlatformView platformView)
    {
        platformView.Click -= OnClick;
        base.DisconnectHandler(platformView);
    }

    void OnClick(object? sender, RoutedEventArgs e)
    {
        if (VirtualView is IImageButton imageButton)
        {
            imageButton.Clicked();
            imageButton.Released();
        }
    }

    partial class ImageButtonImageSourcePartSetter : ImageSourcePartSetter<IImageButtonHandler>
    {
        public ImageButtonImageSourcePartSetter(IImageButtonHandler handler)
            : base(handler)
        {
        }

        public override void SetImageSource(object? platformImage)
        {
            if (Handler?.PlatformView is MauiImageButton imageButton)
            {
                var image = imageButton.GetImage();
                if (image != null && platformImage is global::Avalonia.Media.Imaging.Bitmap bitmap)
                {
                    image.Source = bitmap;
                }
            }
        }
    }
}
