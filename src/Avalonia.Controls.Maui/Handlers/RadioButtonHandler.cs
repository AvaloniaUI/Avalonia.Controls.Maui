using Avalonia.Controls;
using Avalonia.Controls.Maui.Platform;
using Microsoft.Maui;
using Microsoft.Maui.Handlers;
using Microsoft.Maui.Platform;
using AvaloniaRadioButton = global::Avalonia.Controls.RadioButton;

namespace Avalonia.Controls.Maui.Handlers;

/// <summary>
/// Handler for MAUI IRadioButton to Avalonia RadioButton mapping
/// </summary>
public class RadioButtonHandler : ViewHandler<IRadioButton, AvaloniaRadioButton>, IRadioButtonHandler
{
    public static IPropertyMapper<IRadioButton, IRadioButtonHandler> Mapper = new PropertyMapper<IRadioButton, IRadioButtonHandler>(ViewHandler.ViewMapper)
    {
        [nameof(IRadioButton.IsChecked)] = MapIsChecked,
        [nameof(ITextStyle.CharacterSpacing)] = MapCharacterSpacing,
        [nameof(ITextStyle.Font)] = MapFont,
        [nameof(ITextStyle.TextColor)] = MapTextColor,
        [nameof(IRadioButton.Content)] = MapContent,
        [nameof(IRadioButton.StrokeColor)] = MapStrokeColor,
        [nameof(IRadioButton.StrokeThickness)] = MapStrokeThickness,
        [nameof(IRadioButton.CornerRadius)] = MapCornerRadius,
    };

    public static CommandMapper<IRadioButton, IRadioButtonHandler> CommandMapper = new(ViewCommandMapper)
    {
    };

    public RadioButtonHandler() : base(Mapper, CommandMapper)
    {
    }

    public RadioButtonHandler(IPropertyMapper? mapper)
        : base(mapper ?? Mapper, CommandMapper)
    {
    }

    public RadioButtonHandler(IPropertyMapper? mapper, CommandMapper? commandMapper)
        : base(mapper ?? Mapper, commandMapper ?? CommandMapper)
    {
    }

    IRadioButton IRadioButtonHandler.VirtualView => VirtualView;

    System.Object IRadioButtonHandler.PlatformView => PlatformView;

    protected override AvaloniaRadioButton CreatePlatformView()
    {
        return new AvaloniaRadioButton();
    }

    public override bool NeedsContainer => false;

    protected override void ConnectHandler(AvaloniaRadioButton platformView)
    {
        platformView.IsCheckedChanged += OnIsCheckedChanged;
        base.ConnectHandler(platformView);
    }

    protected override void DisconnectHandler(AvaloniaRadioButton platformView)
    {
        platformView.IsCheckedChanged -= OnIsCheckedChanged;
        base.DisconnectHandler(platformView);
    }

    private void OnIsCheckedChanged(object? sender, global::Avalonia.Interactivity.RoutedEventArgs e)
    {
        if (VirtualView != null && sender is AvaloniaRadioButton radioButton)
        {
            VirtualView.IsChecked = radioButton.IsChecked == true;
        }
    }

    public static void MapIsChecked(IRadioButtonHandler handler, IRadioButton radioButton)
    {
        if (handler.PlatformView is null || handler.VirtualView is null)
            return;

        var platformView = (AvaloniaRadioButton)handler.PlatformView;
        platformView.IsChecked = radioButton.IsChecked;
    }

    public static void MapContent(IRadioButtonHandler handler, IRadioButton radioButton)
    {
        if (handler.PlatformView is null || handler.VirtualView is null)
            return;

        var platformView = (AvaloniaRadioButton)handler.PlatformView;

        // If Content is IView, we need to convert it to a platform control
        if (radioButton.PresentedContent is IView view)
        {
            _ = handler.MauiContext ?? throw new InvalidOperationException($"MauiContext cannot be null");
            platformView.Content = (Control)view.ToPlatform(handler.MauiContext);
        }
        // Otherwise, use Content directly (string, etc.)
        else if (radioButton.Content != null)
        {
            platformView.Content = radioButton.Content;
        }
    }

    public static void MapStrokeColor(IRadioButtonHandler handler, IRadioButton radioButton)
    {
        if (handler.PlatformView is null || handler.VirtualView is null)
            return;

        var platformView = (AvaloniaRadioButton)handler.PlatformView;

        if (radioButton.StrokeColor != null)
        {
            platformView.BorderBrush = radioButton.StrokeColor.ToPlatform();
        }
    }

    public static void MapStrokeThickness(IRadioButtonHandler handler, IRadioButton radioButton)
    {
        if (handler.PlatformView is null || handler.VirtualView is null)
            return;

        var platformView = (AvaloniaRadioButton)handler.PlatformView;
        platformView.BorderThickness = new global::Avalonia.Thickness(radioButton.StrokeThickness);
    }

    public static void MapCornerRadius(IRadioButtonHandler handler, IRadioButton radioButton)
    {
        if (handler.PlatformView is null || handler.VirtualView is null)
            return;

        var platformView = (AvaloniaRadioButton)handler.PlatformView;
        platformView.CornerRadius = new global::Avalonia.CornerRadius(radioButton.CornerRadius);
    }

    public static void MapCharacterSpacing(IRadioButtonHandler handler, IRadioButton radioButton)
    {
        if (handler.PlatformView is null || handler.VirtualView is null)
            return;

        // Note: Avalonia RadioButton doesn't directly support character spacing
        // This would require custom styling
        // TODO: Implement character spacing through custom styling
    }

    public static void MapFont(IRadioButtonHandler handler, IRadioButton radioButton)
    {
        if (handler.PlatformView is null || handler.VirtualView is null)
            return;

        var fontManager = handler.GetRequiredService<IFontManager>();
        var platformView = (AvaloniaRadioButton)handler.PlatformView;

        platformView.UpdateFont(radioButton, fontManager);
    }

    public static void MapTextColor(IRadioButtonHandler handler, IRadioButton radioButton)
    {
        if (handler.PlatformView is null || handler.VirtualView is null)
            return;

        var platformView = (AvaloniaRadioButton)handler.PlatformView;

        if (radioButton.TextColor != null)
        {
            platformView.Foreground = radioButton.TextColor.ToPlatform();
        }
    }
}
