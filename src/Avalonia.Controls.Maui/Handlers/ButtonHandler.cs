using Avalonia.Interactivity;
using Avalonia.Controls.Maui.Platform;
using Avalonia.Input;
using Microsoft.Maui;
using Microsoft.Maui.Handlers;
using Microsoft.Maui.Platform;
using PlatformView = Avalonia.Controls.Maui.Platform.MauiButton;

namespace Avalonia.Controls.Maui.Handlers;

public class ButtonHandler : ViewHandler<IButton, PlatformView>, IButtonHandler
{
    public static IPropertyMapper<IButton, IButtonHandler> Mapper = new PropertyMapper<IButton, IButtonHandler>(ViewHandler.ViewMapper)
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
        
        // IButtonStroke properties
        [nameof(IButtonStroke.StrokeThickness)] = MapStrokeThickness,
        [nameof(IButtonStroke.StrokeColor)] = MapStrokeColor,
        [nameof(IButtonStroke.CornerRadius)] = MapCornerRadius,
        
        // IImage properties
        [nameof(IImage.Source)] = MapImageSource,
    };

    public static CommandMapper<IButton, IButtonHandler> CommandMapper = new(ViewCommandMapper);

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

    public ImageSourcePartLoader ImageSourceLoader => null!;

    IButton IButtonHandler.VirtualView => VirtualView;

    System.Object IButtonHandler.PlatformView => PlatformView;

    protected override MauiButton CreatePlatformView()
    {
        return new MauiButton();
    }

    public static void MapBackground(IButtonHandler handler, IButton button)
    {
        if (handler.PlatformView is not PlatformView platformView || handler.VirtualView is null)
            return;

        platformView.UpdateBackground(handler.VirtualView);
    }

    public static void MapStrokeColor(IButtonHandler handler, IButton button)
    {
        if (handler.PlatformView is not PlatformView platformView || handler.VirtualView is null)
            return;

        platformView.UpdateStrokeColor(handler.VirtualView);
    }

    public static void MapStrokeThickness(IButtonHandler handler, IButton button)
    {
        if (handler.PlatformView is not PlatformView platformView || handler.VirtualView is null)
            return;

        platformView.UpdateStrokeThickness(handler.VirtualView);
    }

    public static void MapCornerRadius(IButtonHandler handler, IButton button)
    {
        if (handler.PlatformView is not PlatformView platformView || handler.VirtualView is null)
            return;

        platformView.UpdateCornerRadius(handler.VirtualView);
    }

    public static void MapText(IButtonHandler handler, IButton button)
    {
        if (handler.PlatformView is not PlatformView platformView || handler.VirtualView is null)
            return;

        platformView.UpdateText(handler.VirtualView);
    }

    public static void MapTextColor(IButtonHandler handler, IButton button)
    {
        if (handler.PlatformView is not PlatformView platformView || handler.VirtualView is null)
            return;

        platformView.UpdateTextColor(handler.VirtualView);
    }

    public static void MapCharacterSpacing(IButtonHandler handler, IButton button)
    {
        if (handler.PlatformView is not PlatformView platformView || handler.VirtualView is null)
            return;

        platformView.UpdateCharacterSpacing(handler.VirtualView);
    }

    public static void MapFont(IButtonHandler handler, IButton button)
    {
        if (handler.PlatformView is not PlatformView platformView || handler.VirtualView is null)
            return;

        var fontManager = handler.GetRequiredService<IFontManager>();
        platformView.UpdateFont(handler.VirtualView, fontManager);
    }

    public static void MapPadding(IButtonHandler handler, IButton button)
    {
        if (handler.PlatformView is not PlatformView platformView || handler.VirtualView is null)
            return;

        platformView.UpdatePadding(handler.VirtualView);
    }
    
    public static void MapImageSource(IButtonHandler handler, IButton button)
    {
        if (handler.PlatformView is not PlatformView platformView || handler.VirtualView is null)
            return;

        var imageSourceServiceProvider = handler.GetRequiredService<IImageSourceServiceProvider>();
        platformView.UpdateImageSourceAsync(handler.VirtualView, imageSourceServiceProvider).FireAndForget(handler);
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
}