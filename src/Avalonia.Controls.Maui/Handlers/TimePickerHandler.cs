using Avalonia.Controls.Maui.Platform;
using Microsoft.Maui;
using PlatformView = Avalonia.Controls.TimePicker;

namespace Avalonia.Controls.Maui.Handlers;

/// <summary>Avalonia handler for <see cref="ITimePicker"/>.</summary>
public class TimePickerHandler : ViewHandler<ITimePicker, PlatformView>
{
    /// <summary>Property mapper for <see cref="TimePickerHandler"/>.</summary>
    public static IPropertyMapper<ITimePicker, TimePickerHandler> Mapper = new PropertyMapper<ITimePicker, TimePickerHandler>(ViewHandler.ViewMapper)
    {
        [nameof(ITimePicker.CharacterSpacing)] = MapCharacterSpacing,
        [nameof(ITimePicker.Font)] = MapFont,
        [nameof(ITimePicker.Format)] = MapFormat,
        [nameof(ITimePicker.TextColor)] = MapTextColor,
        [nameof(ITimePicker.Time)] = MapTime,
    };

    /// <summary>Command mapper for <see cref="TimePickerHandler"/>.</summary>
    public static CommandMapper<ITimePicker, TimePickerHandler> CommandMapper = new(ViewCommandMapper)
    {
    };

    /// <summary>Initializes a new instance of <see cref="TimePickerHandler"/>.</summary>
    public TimePickerHandler() : base(Mapper, CommandMapper)
    {
    }

    /// <summary>Initializes a new instance of <see cref="TimePickerHandler"/>.</summary>
    /// <param name="mapper">The property mapper to use, or <see langword="null"/> to use the default.</param>
    public TimePickerHandler(IPropertyMapper? mapper)
        : base(mapper ?? Mapper, CommandMapper)
    {
    }

    /// <summary>Initializes a new instance of <see cref="TimePickerHandler"/>.</summary>
    /// <param name="mapper">The property mapper to use, or <see langword="null"/> to use the default.</param>
    /// <param name="commandMapper">The command mapper to use, or <see langword="null"/> to use the default.</param>
    public TimePickerHandler(IPropertyMapper? mapper, CommandMapper? commandMapper)
        : base(mapper ?? Mapper, commandMapper ?? CommandMapper)
    {
    }

    /// <summary>Creates the Avalonia platform view for this handler.</summary>
    protected override PlatformView CreatePlatformView() => new PlatformView();

    /// <summary>Gets a value indicating whether this handler requires a container view.</summary>
    public override bool NeedsContainer => false;

    /// <inheritdoc/>
    protected override void ConnectHandler(PlatformView platformView)
    {
        platformView.SelectedTimeChanged += OnSelectedTimeChanged;
        base.ConnectHandler(platformView);
    }

    /// <inheritdoc/>
    protected override void DisconnectHandler(PlatformView platformView)
    {
        platformView.SelectedTimeChanged -= OnSelectedTimeChanged;
        base.DisconnectHandler(platformView);
    }

    /// <summary>Maps the Time property to the platform view.</summary>
    /// <param name="handler">The handler for the time picker.</param>
    /// <param name="timePicker">The virtual view.</param>
    public static void MapTime(TimePickerHandler handler, ITimePicker timePicker)
    {
        if (handler.PlatformView is not PlatformView platformView)
            return;

        platformView.UpdateTime(timePicker);
    }

    /// <summary>Maps the Format property to the platform view.</summary>
    /// <param name="handler">The handler for the time picker.</param>
    /// <param name="timePicker">The virtual view.</param>
    public static void MapFormat(TimePickerHandler handler, ITimePicker timePicker)
    {
        if (handler.PlatformView is not PlatformView platformView)
            return;

        platformView.UpdateFormat(timePicker);
    }

    /// <summary>Maps the CharacterSpacing property to the platform view.</summary>
    /// <param name="handler">The handler for the time picker.</param>
    /// <param name="timePicker">The virtual view.</param>
    public static void MapCharacterSpacing(TimePickerHandler handler, ITimePicker timePicker)
    {
        if (handler.PlatformView is not PlatformView platformView)
            return;

        platformView.UpdateCharacterSpacing(timePicker);
    }

    /// <summary>Maps the Font property to the platform view.</summary>
    /// <param name="handler">The handler for the time picker.</param>
    /// <param name="timePicker">The virtual view.</param>
    public static void MapFont(TimePickerHandler handler, ITimePicker timePicker)
    {
        if (handler.PlatformView is not PlatformView platformView)
            return;

        var fontManager = handler.GetRequiredService<IFontManager>();
        platformView.UpdateFont(timePicker, fontManager);
    }

    /// <summary>Maps the TextColor property to the platform view.</summary>
    /// <param name="handler">The handler for the time picker.</param>
    /// <param name="timePicker">The virtual view.</param>
    public static void MapTextColor(TimePickerHandler handler, ITimePicker timePicker)
    {
        if (handler.PlatformView is not PlatformView platformView)
            return;

        platformView.UpdateTextColor(timePicker);
    }
    
    void OnSelectedTimeChanged(object? sender, TimePickerSelectedValueChangedEventArgs e)
    {
        if (VirtualView != null && e.NewTime.HasValue)
            VirtualView.Time = e.NewTime.Value;
    }
}