using Avalonia.Interactivity;
using Avalonia.Controls.Maui.Platform;
using Avalonia.Input;
using Avalonia.Controls.Maui.Services;
using Microsoft.Maui;
using Microsoft.Maui.Platform;
using System.Threading;
using System.Threading.Tasks;
using PlatformView = Avalonia.Controls.Maui.MauiButton;
using MButton = Microsoft.Maui.Controls.Button;

namespace Avalonia.Controls.Maui.Handlers;

public partial class ButtonHandler : ViewHandler<IButton, PlatformView>
{
    private CancellationTokenSource? _imageSourceCts;
    private ImageSourcePartLoader? _imageSourcePartLoader;

    public static IPropertyMapper<IButton, ButtonHandler> Mapper = new PropertyMapper<IButton, ButtonHandler>(ViewHandler.ViewMapper)
    {
        // IText properties
        [nameof(IText.Text)] = MapText,
        
        // ITextStyle properties
        [nameof(ITextStyle.CharacterSpacing)] = MapCharacterSpacing,
        [nameof(ITextStyle.Font)] = MapFont,
        [nameof(ITextStyle.TextColor)] = MapTextColor,
        
        // IButton properties
        [nameof(IButton.Background)] = MapBackground,
        [nameof(IButton.Padding)] = MapPadding,
        [nameof(MButton.ContentLayout)] = MapContentLayout,
        
        // IButtonStroke properties
        [nameof(IButtonStroke.StrokeThickness)] = MapStrokeThickness,
        [nameof(IButtonStroke.StrokeColor)] = MapStrokeColor,
        [nameof(IButtonStroke.CornerRadius)] = MapCornerRadius,
        
        // Button image properties
        [nameof(MButton.ImageSource)] = MapImageSource,
    };

    public static CommandMapper<IButton, ButtonHandler> CommandMapper = new(ViewCommandMapper);

    public ButtonHandler()
        : base(Mapper, CommandMapper)
    {
    }

    public ButtonHandler(IPropertyMapper? mapper)
        : base(mapper ?? Mapper, CommandMapper)
    {
    }

    public ButtonHandler(IPropertyMapper? mapper, CommandMapper? commandMapper)
        : base(mapper ?? Mapper, commandMapper ?? CommandMapper)
    {
    }

    public ImageSourcePartLoader ImageSourceLoader =>
        _imageSourcePartLoader ??= new ImageSourcePartLoader(new ButtonImageSourcePartSetter(this));

    protected override MauiButton CreatePlatformView()
    {
        return new MauiButton();
    }

    public static void MapBackground(ButtonHandler handler, IButton button)
    {
        if (handler.PlatformView is not PlatformView platformView || handler.VirtualView is null)
            return;

        platformView.UpdateButtonBackground(handler.VirtualView);
    }

    public static void MapStrokeColor(ButtonHandler handler, IButton button)
    {
        if (handler.PlatformView is not PlatformView platformView || handler.VirtualView is null)
            return;

        platformView.UpdateStrokeColor(handler.VirtualView);
    }

    public static void MapStrokeThickness(ButtonHandler handler, IButton button)
    {
        if (handler.PlatformView is not PlatformView platformView || handler.VirtualView is null)
            return;

        platformView.UpdateStrokeThickness(handler.VirtualView);
    }

    public static void MapCornerRadius(ButtonHandler handler, IButton button)
    {
        if (handler.PlatformView is not PlatformView platformView || handler.VirtualView is null)
            return;

        platformView.UpdateCornerRadius(handler.VirtualView);
    }

    public static void MapText(ButtonHandler handler, IButton button)
    {
        if (handler.PlatformView is not PlatformView platformView || handler.VirtualView is null)
            return;

        platformView.UpdateText(handler.VirtualView);
    }

    public static void MapTextColor(ButtonHandler handler, IButton button)
    {
        if (handler.PlatformView is not PlatformView platformView || handler.VirtualView is null)
            return;

        platformView.UpdateTextColor(handler.VirtualView);
    }

    public static void MapCharacterSpacing(ButtonHandler handler, IButton button)
    {
        if (handler.PlatformView is not PlatformView platformView || handler.VirtualView is null)
            return;

        platformView.UpdateCharacterSpacing(handler.VirtualView);
    }

    public static void MapFont(ButtonHandler handler, IButton button)
    {
        if (handler.PlatformView is not PlatformView platformView || handler.VirtualView is null)
            return;

        var fontManager = handler.GetRequiredService<IFontManager>();
        platformView.UpdateFont(handler.VirtualView, fontManager);
    }

    public static void MapPadding(ButtonHandler handler, IButton button)
    {
        if (handler.PlatformView is not PlatformView platformView || handler.VirtualView is null)
            return;

        platformView.UpdatePadding(handler.VirtualView);
    }
    
    public static void MapImageSource(ButtonHandler handler, IButton button)
    {
        if (handler is not ButtonHandler buttonHandler || handler.VirtualView is null)
            return;

        buttonHandler._imageSourceCts?.Cancel();
        buttonHandler._imageSourceCts = null;

        var imageSource = button switch
        {
            MButton mauiButton => mauiButton.ImageSource,
            IImageSourcePart imagePart => imagePart.Source,
            _ => null
        };

        if (imageSource == null)
        {
            if (handler.PlatformView is PlatformView pv)
            {
                pv.ImageSource = null;
            }
            return;
        }

        var cts = new CancellationTokenSource();
        buttonHandler._imageSourceCts = cts;
        _ = buttonHandler.LoadImageSourceAsync(imageSource, cts.Token);
    }

    public static void MapContentLayout(ButtonHandler handler, IButton button)
    {
        if (handler.PlatformView is not PlatformView platformView || handler.VirtualView is null)
            return;

        var layout = button switch
        {
            MButton mauiButton => mauiButton.ContentLayout,
            _ => null
        };

        if (layout is MButton.ButtonContentLayout value)
        {
            platformView.UpdateContentLayout(value);
        }
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
        if (VirtualView is IButton button)
        {
            button.Pressed();
        }
    }

    void OnClick(object? sender, RoutedEventArgs e)
    {
        if (VirtualView is IButton button)
        {
            button.Clicked();
        }
    }

    void OnPointerReleased(object? sender, PointerReleasedEventArgs e)
    {
        if (VirtualView is IButton button)
        {
            button.Released();
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
                    platformView.ImageSource = result?.Value as Avalonia.Media.IImage;
                }
            }
            else if (PlatformView is PlatformView platformView)
            {
                platformView.ImageSource = null;
            }
        }
        catch
        {
            if (PlatformView is PlatformView platformView)
            {
                platformView.ImageSource = null;
            }
        }
    }

    partial class ButtonImageSourcePartSetter : ImageSourcePartSetter<ButtonHandler>
    {
        public ButtonImageSourcePartSetter(ButtonHandler handler)
            : base(handler)
        {
        }

#if !IOS && !MACCATALYST && !ANDROID && !WINDOWS
        public override void SetImageSource(object? platformImage)
        {
            if (Handler?.PlatformView is PlatformView button)
            {
                button.ImageSource = platformImage as Avalonia.Media.IImage;
            }
        }
#endif
    }
}
