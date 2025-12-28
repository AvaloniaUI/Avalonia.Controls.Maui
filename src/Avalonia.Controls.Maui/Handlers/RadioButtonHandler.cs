using Avalonia.Controls.Maui.Platform;
using Microsoft.Maui;
using Microsoft.Maui.Handlers;
using PlatformView = Avalonia.Controls.Maui.MauiRadioButton;

namespace Avalonia.Controls.Maui.Handlers;

public class RadioButtonHandler : ViewHandler<IRadioButton, PlatformView>
{
    public static IPropertyMapper<IRadioButton, RadioButtonHandler> Mapper = new PropertyMapper<IRadioButton, RadioButtonHandler>(ViewHandler.ViewMapper)
    {
        [nameof(IRadioButton.IsChecked)] = MapIsChecked,
        [nameof(ITextStyle.CharacterSpacing)] = MapCharacterSpacing,
        [nameof(ITextStyle.Font)] = MapFont,
        [nameof(ITextStyle.TextColor)] = MapTextColor,
        [nameof(IRadioButton.Content)] = MapContent,
        [nameof(IButtonStroke.StrokeColor)] = MapStrokeColor,
        [nameof(IButtonStroke.StrokeThickness)] = MapStrokeThickness,
        [nameof(IButtonStroke.CornerRadius)] = MapCornerRadius,
        ["ContentTemplate"] = MapContent,
        ["GroupName"] = MapGroupName,
        ["TextTransform"] = MapContent,
        ["Value"] = MapValue,
    };

    public static CommandMapper<IRadioButton, RadioButtonHandler> CommandMapper = new(ViewCommandMapper)
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

    protected override PlatformView CreatePlatformView()
    {
        return new PlatformView();
    }

    public override bool NeedsContainer => false;

    protected override void ConnectHandler(PlatformView platformView)
    {
        platformView.IsCheckedChanged += OnIsCheckedChanged;
        base.ConnectHandler(platformView);
    }

    protected override void DisconnectHandler(PlatformView platformView)
    {
        platformView.IsCheckedChanged -= OnIsCheckedChanged;
        base.DisconnectHandler(platformView);
    }

    public static void MapIsChecked(RadioButtonHandler handler, IRadioButton radioButton)
    {
        if (handler.PlatformView is null || handler.VirtualView is null)
            return;

        ((PlatformView)handler.PlatformView).UpdateIsChecked(radioButton);
    }

    public static void MapContent(RadioButtonHandler handler, IRadioButton radioButton)
    {
        if (handler.PlatformView is null || handler.VirtualView is null)
            return;

        RadioButtonExtensions.UpdateContent((PlatformView)handler.PlatformView, handler, radioButton);
    }

    public static void MapStrokeColor(RadioButtonHandler handler, IRadioButton radioButton)
    {
        if (handler.PlatformView is null || handler.VirtualView is null)
            return;

        ((PlatformView)handler.PlatformView).UpdateStrokeColor(radioButton);
    }

    public static void MapStrokeThickness(RadioButtonHandler handler, IRadioButton radioButton)
    {
        if (handler.PlatformView is null || handler.VirtualView is null)
            return;

        ((PlatformView)handler.PlatformView).UpdateStrokeThickness(radioButton);
    }

    public static void MapCornerRadius(RadioButtonHandler handler, IRadioButton radioButton)
    {
        if (handler.PlatformView is null || handler.VirtualView is null)
            return;

        ((PlatformView)handler.PlatformView).UpdateCornerRadius(radioButton);
    }

    public static void MapCharacterSpacing(RadioButtonHandler handler, IRadioButton radioButton)
    {
        if (handler.PlatformView is null || handler.VirtualView is null)
            return;

        ((PlatformView)handler.PlatformView).UpdateCharacterSpacing(radioButton);
    }

    public static void MapFont(RadioButtonHandler handler, IRadioButton radioButton)
    {
        if (handler.PlatformView is null || handler.VirtualView is null)
            return;

        var fontManager = handler.GetRequiredService<IFontManager>();
        ((PlatformView)handler.PlatformView).UpdateFont(radioButton, fontManager);
    }

    public static void MapTextColor(RadioButtonHandler handler, IRadioButton radioButton)
    {
        if (handler.PlatformView is null || handler.VirtualView is null)
            return;

        ((PlatformView)handler.PlatformView).UpdateTextColor(radioButton);
    }

    public static void MapGroupName(RadioButtonHandler handler, IRadioButton radioButton)
    {
        if (handler.PlatformView is null || handler.VirtualView is null)
            return;

        ((PlatformView)handler.PlatformView).UpdateGroupName(radioButton);
    }

    public static void MapValue(RadioButtonHandler handler, IRadioButton radioButton)
    {
        if (handler.PlatformView is null || handler.VirtualView is null)
            return;

        ((PlatformView)handler.PlatformView).UpdateValue(radioButton);
    }

    private void OnIsCheckedChanged(object? sender, Interactivity.RoutedEventArgs e)
    {
        if (VirtualView != null && sender is PlatformView radioButton)
        {
            VirtualView.IsChecked = radioButton.IsChecked == true;
        }
    }
}
