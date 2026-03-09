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

/// <summary>Avalonia handler for <see cref="IButton"/>.</summary>
public partial class ButtonHandler : ViewHandler<IButton, PlatformView>
{
    private CancellationTokenSource? _imageSourceCts;
    private ImageSourcePartLoader? _imageSourcePartLoader;

    /// <summary>Property mapper for <see cref="ButtonHandler"/>.</summary>
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

        [nameof(MButton.LineBreakMode)] = MapLineBreakMode,

        [nameof(IView.Shadow)] = MapButtonShadow,
    };

    /// <summary>Command mapper for <see cref="ButtonHandler"/>.</summary>
    public static CommandMapper<IButton, ButtonHandler> CommandMapper = new(ViewCommandMapper);

    /// <summary>Initializes a new instance of <see cref="ButtonHandler"/>.</summary>
    public ButtonHandler()
        : base(Mapper, CommandMapper)
    {
    }

    /// <summary>Initializes a new instance of <see cref="ButtonHandler"/>.</summary>
    /// <param name="mapper">The property mapper to use, or <see langword="null"/> to use the default.</param>
    public ButtonHandler(IPropertyMapper? mapper)
        : base(mapper ?? Mapper, CommandMapper)
    {
    }

    /// <summary>Initializes a new instance of <see cref="ButtonHandler"/>.</summary>
    /// <param name="mapper">The property mapper to use, or <see langword="null"/> to use the default.</param>
    /// <param name="commandMapper">The command mapper to use, or <see langword="null"/> to use the default.</param>
    public ButtonHandler(IPropertyMapper? mapper, CommandMapper? commandMapper)
        : base(mapper ?? Mapper, commandMapper ?? CommandMapper)
    {
    }

    /// <summary>Gets a value indicating whether this handler requires a container view.</summary>
    /// <remarks>
    /// Shadow is applied directly to the PlatformView, so the button does not need
    /// a ContainerView wrapper just for shadow. This avoids a rendering issue where
    /// DropShadowEffect on a parent Panel causes the button content to disappear
    /// when the button's visual state changes (hover/pressed).
    /// </remarks>
    public override bool NeedsContainer
    {
        get
        {
            if (VirtualView is not IView view)
                return false;

            // Still need a container for Clip; just not for Shadow alone
            return view.Clip != null;
        }
    }

    /// <summary>Gets the image source loader for loading button images.</summary>
    public ImageSourcePartLoader ImageSourceLoader =>
        _imageSourcePartLoader ??= new ImageSourcePartLoader(new ButtonImageSourcePartSetter(this));

    /// <summary>Creates the Avalonia platform view for this handler.</summary>
    protected override MauiButton CreatePlatformView()
    {
        return new MauiButton();
    }

    /// <summary>Maps the Shadow property to the platform view.</summary>
    /// <param name="handler">The handler for the button.</param>
    /// <param name="button">The virtual view.</param>
    public static void MapButtonShadow(ButtonHandler handler, IButton button)
    {
        if (handler.PlatformView is not PlatformView platformView || handler.VirtualView is not IView view)
            return;

        Avalonia.Controls.Maui.Extensions.ViewExtensions.UpdateShadow(platformView, view);
    }

    /// <summary>Maps the Background property to the platform view.</summary>
    /// <param name="handler">The handler for the button.</param>
    /// <param name="button">The virtual view.</param>
    public static void MapBackground(ButtonHandler handler, IButton button)
    {
        if (handler.PlatformView is not PlatformView platformView || handler.VirtualView is null)
            return;

        platformView.UpdateButtonBackground(handler.VirtualView);
    }

    /// <summary>Maps the StrokeColor property to the platform view.</summary>
    /// <param name="handler">The handler for the button.</param>
    /// <param name="button">The virtual view.</param>
    public static void MapStrokeColor(ButtonHandler handler, IButton button)
    {
        if (handler.PlatformView is not PlatformView platformView || handler.VirtualView is null)
            return;

        platformView.UpdateStrokeColor(handler.VirtualView);
    }

    /// <summary>Maps the StrokeThickness property to the platform view.</summary>
    /// <param name="handler">The handler for the button.</param>
    /// <param name="button">The virtual view.</param>
    public static void MapStrokeThickness(ButtonHandler handler, IButton button)
    {
        if (handler.PlatformView is not PlatformView platformView || handler.VirtualView is null)
            return;

        platformView.UpdateStrokeThickness(handler.VirtualView);
    }

    /// <summary>Maps the CornerRadius property to the platform view.</summary>
    /// <param name="handler">The handler for the button.</param>
    /// <param name="button">The virtual view.</param>
    public static void MapCornerRadius(ButtonHandler handler, IButton button)
    {
        if (handler.PlatformView is not PlatformView platformView || handler.VirtualView is null)
            return;

        platformView.UpdateCornerRadius(handler.VirtualView);
    }

    /// <summary>Maps the Text property to the platform view.</summary>
    /// <param name="handler">The handler for the button.</param>
    /// <param name="button">The virtual view.</param>
    public static void MapText(ButtonHandler handler, IButton button)
    {
        if (handler.PlatformView is not PlatformView platformView || handler.VirtualView is null)
            return;

        platformView.UpdateText(handler.VirtualView);
    }

    /// <summary>Maps the TextColor property to the platform view.</summary>
    /// <param name="handler">The handler for the button.</param>
    /// <param name="button">The virtual view.</param>
    public static void MapTextColor(ButtonHandler handler, IButton button)
    {
        if (handler.PlatformView is not PlatformView platformView || handler.VirtualView is null)
            return;

        platformView.UpdateTextColor(handler.VirtualView);
    }

    /// <summary>Maps the CharacterSpacing property to the platform view.</summary>
    /// <param name="handler">The handler for the button.</param>
    /// <param name="button">The virtual view.</param>
    public static void MapCharacterSpacing(ButtonHandler handler, IButton button)
    {
        if (handler.PlatformView is not PlatformView platformView || handler.VirtualView is null)
            return;

        platformView.UpdateCharacterSpacing(handler.VirtualView);
    }

    /// <summary>Maps the Font property to the platform view.</summary>
    /// <param name="handler">The handler for the button.</param>
    /// <param name="button">The virtual view.</param>
    public static void MapFont(ButtonHandler handler, IButton button)
    {
        if (handler.PlatformView is not PlatformView platformView || handler.VirtualView is null)
            return;

        var fontManager = handler.GetRequiredService<IFontManager>();
        platformView.UpdateFont(handler.VirtualView, fontManager);
    }

    /// <summary>Maps the Padding property to the platform view.</summary>
    /// <param name="handler">The handler for the button.</param>
    /// <param name="button">The virtual view.</param>
    public static void MapPadding(ButtonHandler handler, IButton button)
    {
        if (handler.PlatformView is not PlatformView platformView || handler.VirtualView is null)
            return;

        platformView.UpdatePadding(handler.VirtualView);
    }

    /// <summary>Maps the ImageSource property to the platform view.</summary>
    /// <param name="handler">The handler for the button.</param>
    /// <param name="button">The virtual view.</param>
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
    /// <summary>Maps the ContentLayout property to the platform view.</summary>
    /// <param name="handler">The handler for the button.</param>
    /// <param name="button">The virtual view.</param>
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

    /// <summary>Maps the LineBreakMode property to the platform view.</summary>
    /// <param name="handler">The handler for the button.</param>
    /// <param name="button">The virtual view.</param>
    public static void MapLineBreakMode(ButtonHandler handler, IButton button)
    {
        if (handler.PlatformView is not PlatformView platformView || handler.VirtualView is null)
            return;

        platformView.UpdateLineBreakMode(handler.VirtualView);
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

    /// <summary>Image source part setter for <see cref="ButtonHandler"/>.</summary>
    partial class ButtonImageSourcePartSetter : ImageSourcePartSetter<ButtonHandler>
    {
        /// <summary>Initializes a new instance of <see cref="ButtonImageSourcePartSetter"/>.</summary>
        /// <param name="handler">The button handler.</param>
        public ButtonImageSourcePartSetter(ButtonHandler handler)
            : base(handler)
        {
        }

#if !IOS && !MACCATALYST && !ANDROID && !WINDOWS
        /// <inheritdoc/>
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
