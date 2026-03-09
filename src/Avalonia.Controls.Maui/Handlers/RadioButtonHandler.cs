using Avalonia.Controls.Maui.Platform;
using Microsoft.Maui;
using PlatformView = Avalonia.Controls.Maui.MauiRadioButton;

namespace Avalonia.Controls.Maui.Handlers;

/// <summary>Avalonia handler for <see cref="IRadioButton"/>.</summary>
public class RadioButtonHandler : ViewHandler<IRadioButton, PlatformView>
{
    /// <summary>Property mapper for <see cref="RadioButtonHandler"/>.</summary>
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
        ["BorderColor"] = MapStrokeColor,
        ["BorderWidth"] = MapStrokeThickness,
        ["ContentTemplate"] = MapContent,
        ["TextTransform"] = MapContent,
        ["Value"] = MapValue,
    };

    /// <summary>Command mapper for <see cref="RadioButtonHandler"/>.</summary>
    public static CommandMapper<IRadioButton, RadioButtonHandler> CommandMapper = new(ViewCommandMapper)
    {
    };

    /// <summary>Initializes a new instance of <see cref="RadioButtonHandler"/>.</summary>
    public RadioButtonHandler() : base(Mapper, CommandMapper)
    {
    }

    /// <summary>Initializes a new instance of <see cref="RadioButtonHandler"/>.</summary>
    /// <param name="mapper">The property mapper to use, or <see langword="null"/> to use the default.</param>
    public RadioButtonHandler(IPropertyMapper? mapper)
        : base(mapper ?? Mapper, CommandMapper)
    {
    }

    /// <summary>Initializes a new instance of <see cref="RadioButtonHandler"/>.</summary>
    /// <param name="mapper">The property mapper to use, or <see langword="null"/> to use the default.</param>
    /// <param name="commandMapper">The command mapper to use, or <see langword="null"/> to use the default.</param>
    public RadioButtonHandler(IPropertyMapper? mapper, CommandMapper? commandMapper)
        : base(mapper ?? Mapper, commandMapper ?? CommandMapper)
    {
    }

    /// <summary>Creates the Avalonia platform view for this handler.</summary>
    protected override PlatformView CreatePlatformView()
    {
        return new PlatformView();
    }

    /// <summary>Gets a value indicating whether this handler requires a container view.</summary>
    public override bool NeedsContainer => false;

    /// <inheritdoc/>
    protected override void ConnectHandler(PlatformView platformView)
    {
        platformView.IsCheckedChanged += OnIsCheckedChanged;
        base.ConnectHandler(platformView);
    }

    /// <inheritdoc/>
    protected override void DisconnectHandler(PlatformView platformView)
    {
        platformView.IsCheckedChanged -= OnIsCheckedChanged;
        base.DisconnectHandler(platformView);
    }

    /// <summary>Maps the IsChecked property to the platform view.</summary>
    /// <param name="handler">The handler for the radio button.</param>
    /// <param name="radioButton">The virtual view.</param>
    public static void MapIsChecked(RadioButtonHandler handler, IRadioButton radioButton)
    {
        if (handler.PlatformView is null || handler.VirtualView is null)
            return;

        ((PlatformView)handler.PlatformView).UpdateIsChecked(radioButton);
    }

    /// <summary>Maps the Content property to the platform view.</summary>
    /// <param name="handler">The handler for the radio button.</param>
    /// <param name="radioButton">The virtual view.</param>
    public static void MapContent(RadioButtonHandler handler, IRadioButton radioButton)
    {
        if (handler.PlatformView is null || handler.VirtualView is null)
            return;

        RadioButtonExtensions.UpdateContent((PlatformView)handler.PlatformView, handler, radioButton);
    }

    /// <summary>Maps the StrokeColor property to the platform view.</summary>
    /// <param name="handler">The handler for the radio button.</param>
    /// <param name="radioButton">The virtual view.</param>
    public static void MapStrokeColor(RadioButtonHandler handler, IRadioButton radioButton)
    {
        if (handler.PlatformView is null || handler.VirtualView is null)
            return;

        ((PlatformView)handler.PlatformView).UpdateStrokeColor(radioButton);
    }

    /// <summary>Maps the StrokeThickness property to the platform view.</summary>
    /// <param name="handler">The handler for the radio button.</param>
    /// <param name="radioButton">The virtual view.</param>
    public static void MapStrokeThickness(RadioButtonHandler handler, IRadioButton radioButton)
    {
        if (handler.PlatformView is null || handler.VirtualView is null)
            return;

        ((PlatformView)handler.PlatformView).UpdateStrokeThickness(radioButton);
    }

    /// <summary>Maps the CornerRadius property to the platform view.</summary>
    /// <param name="handler">The handler for the radio button.</param>
    /// <param name="radioButton">The virtual view.</param>
    public static void MapCornerRadius(RadioButtonHandler handler, IRadioButton radioButton)
    {
        if (handler.PlatformView is null || handler.VirtualView is null)
            return;

        ((PlatformView)handler.PlatformView).UpdateCornerRadius(radioButton);
    }

    /// <summary>Maps the CharacterSpacing property to the platform view.</summary>
    /// <param name="handler">The handler for the radio button.</param>
    /// <param name="radioButton">The virtual view.</param>
    public static void MapCharacterSpacing(RadioButtonHandler handler, IRadioButton radioButton)
    {
        if (handler.PlatformView is null || handler.VirtualView is null)
            return;

        ((PlatformView)handler.PlatformView).UpdateCharacterSpacing(radioButton);
    }

    /// <summary>Maps the Font property to the platform view.</summary>
    /// <param name="handler">The handler for the radio button.</param>
    /// <param name="radioButton">The virtual view.</param>
    public static void MapFont(RadioButtonHandler handler, IRadioButton radioButton)
    {
        if (handler.PlatformView is null || handler.VirtualView is null)
            return;

        var fontManager = handler.GetRequiredService<IFontManager>();
        ((PlatformView)handler.PlatformView).UpdateFont(radioButton, fontManager);
    }

    /// <summary>Maps the TextColor property to the platform view.</summary>
    /// <param name="handler">The handler for the radio button.</param>
    /// <param name="radioButton">The virtual view.</param>
    public static void MapTextColor(RadioButtonHandler handler, IRadioButton radioButton)
    {
        if (handler.PlatformView is null || handler.VirtualView is null)
            return;

        ((PlatformView)handler.PlatformView).UpdateTextColor(radioButton);
    }

    /// <summary>Maps the GroupName property to the platform view.</summary>
    /// <param name="handler">The handler for the radio button.</param>
    /// <param name="radioButton">The virtual view.</param>
    public static void MapGroupName(RadioButtonHandler handler, IRadioButton radioButton)
    {
        if (handler.PlatformView is null || handler.VirtualView is null)
            return;

        ((PlatformView)handler.PlatformView).UpdateGroupName(radioButton);
    }

    /// <summary>Maps the Value property to the platform view.</summary>
    /// <param name="handler">The handler for the radio button.</param>
    /// <param name="radioButton">The virtual view.</param>
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
