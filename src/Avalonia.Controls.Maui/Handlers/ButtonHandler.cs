using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Controls.Maui.Platform;
using Microsoft.Maui;
using Microsoft.Maui.Handlers;
using Microsoft.Maui.Platform;
using System;
using System.Collections.Generic;
using System.Text;
using PlatformView = Avalonia.Controls.Maui.Platform.MauiButton;

namespace Avalonia.Controls.Maui.Handlers;

internal class ButtonHandler : ViewHandler<IButton, MauiButton>, IButtonHandler
{
    public static IPropertyMapper<ITextButton, IButtonHandler> TextButtonMapper = new PropertyMapper<ITextButton, IButtonHandler>()
    {
        [nameof(ITextStyle.CharacterSpacing)] = MapCharacterSpacing,
        [nameof(ITextStyle.Font)] = MapFont,
        [nameof(ITextStyle.TextColor)] = MapTextColor,
        [nameof(IText.Text)] = MapText
    };

    public static IPropertyMapper<IImage, IButtonHandler> ImageButtonMapper = new PropertyMapper<IImage, IButtonHandler>()
    {
        [nameof(IImage.Source)] = MapImageSource
    };

    public static IPropertyMapper<IButton, IButtonHandler> Mapper = new PropertyMapper<IButton, IButtonHandler>(TextButtonMapper, ImageButtonMapper, ViewHandler.ViewMapper)
    {
        [nameof(IButton.Background)] = MapBackground,
        [nameof(IButton.Padding)] = MapPadding,
        [nameof(IButtonStroke.StrokeThickness)] = MapStrokeThickness,
        [nameof(IButtonStroke.StrokeColor)] = MapStrokeColor,
        [nameof(IButtonStroke.CornerRadius)] = MapCornerRadius
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
        if (handler.PlatformView is null || handler.VirtualView is null)
            return;
        ((PlatformView)(handler.PlatformView)).Background = button.Background?.ToPlatform();
    }

    public static void MapStrokeColor(IButtonHandler handler, IButtonStroke buttonStroke)
    {
        if (handler.PlatformView is null || handler.VirtualView is null)
            return;
        if (buttonStroke.StrokeColor == null)
            return;
        ((PlatformView)(handler.PlatformView)).BorderBrush = buttonStroke.StrokeColor.ToPlatform();
    }

    public static void MapStrokeThickness(IButtonHandler handler, IButtonStroke buttonStroke)
    {
        if (handler.PlatformView is null || handler.VirtualView is null)
            return;
        ((PlatformView)(handler.PlatformView)).BorderThickness = new global::Avalonia.Thickness(buttonStroke.StrokeThickness);
    }

    public static void MapCornerRadius(IButtonHandler handler, IButtonStroke buttonStroke)
    {
        if (handler.PlatformView is null || handler.VirtualView is null)
            return;
        ((PlatformView)(handler.PlatformView)).CornerRadius = new global::Avalonia.CornerRadius(buttonStroke.CornerRadius);
    }

    public static void MapText(IButtonHandler handler, IText button)
    {
        if (handler.PlatformView is null || handler.VirtualView is null)
            return;

        var mauiButton = (MauiButton)handler.PlatformView;
        mauiButton.Text = button.Text;
    }

    public static void MapTextColor(IButtonHandler handler, ITextStyle button)
    {
        if (handler.PlatformView is null || handler.VirtualView is null || button.TextColor is null)
            return;

        var mauiButton = (MauiButton)handler.PlatformView;
        var textBlock = mauiButton.GetTextBlock();
        if (textBlock != null)
        {
            textBlock.Foreground = button.TextColor.ToPlatform();
        }
    }

    public static void MapCharacterSpacing(IButtonHandler handler, ITextStyle button)
    {
        if (handler.PlatformView is null || handler.VirtualView is null)
            return;

        var mauiButton = (MauiButton)handler.PlatformView;
        mauiButton.CharacterSpacing = button.CharacterSpacing;

        // Apply to TextBlock if available
        var textBlock = mauiButton.GetTextBlock();
        if (textBlock != null)
        {
            textBlock.LetterSpacing = button.CharacterSpacing;
        }
    }

    public static void MapFont(IButtonHandler handler, ITextStyle button)
    {
        if (handler.PlatformView is null || handler.VirtualView is null)
            return;

        var fontManager = handler.GetRequiredService<IFontManager>();
        var mauiButton = (MauiButton)handler.PlatformView;
        var textBlock = mauiButton.GetTextBlock();

        if (textBlock == null)
            return;

        textBlock.UpdateFont(button, fontManager);
    }

    public static void MapPadding(IButtonHandler handler, IButton button)
    {
        if (handler.PlatformView is null || handler.VirtualView is null)
            return;
        ((PlatformView)(handler.PlatformView)).Padding = button.Padding.ToThickness();
    }

    public static void MapImageSource(IButtonHandler handler, IImage button)
    {
        if (handler.PlatformView is null || handler.VirtualView is null)
            return;

        var mauiButton = (MauiButton)handler.PlatformView;

        // For now, just set a placeholder for image source
        // Full implementation would require image loading infrastructure
        // TODO: Implement proper image source loading when image infrastructure is ready
        if (button.Source != null)
        {
            // mauiButton.ImageSource = ...
        }
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
        // Check if handler is still connected before accessing VirtualView
        if (base.VirtualView is IButton button)
        {
            button.Clicked();
            button.Released();
        }
    }
}