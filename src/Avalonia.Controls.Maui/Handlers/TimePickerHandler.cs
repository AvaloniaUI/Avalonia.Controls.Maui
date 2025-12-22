using Avalonia.Controls.Maui.Platform;
using Microsoft.Maui;
using Microsoft.Maui.Handlers;
using PlatformView = Avalonia.Controls.TimePicker;

namespace Avalonia.Controls.Maui.Handlers;

public class TimePickerHandler : ViewHandler<ITimePicker, PlatformView>, ITimePickerHandler
{
    public static IPropertyMapper<ITimePicker, ITimePickerHandler> Mapper = new PropertyMapper<ITimePicker, ITimePickerHandler>(ViewHandler.ViewMapper)
    {
        [nameof(ITimePicker.CharacterSpacing)] = MapCharacterSpacing,
        [nameof(ITimePicker.Font)] = MapFont,
        [nameof(ITimePicker.Format)] = MapFormat,
        [nameof(ITimePicker.TextColor)] = MapTextColor,
        [nameof(ITimePicker.Time)] = MapTime,
    };

    public static CommandMapper<ITimePicker, ITimePickerHandler> CommandMapper = new(ViewCommandMapper)
    {
    };

    public TimePickerHandler() : base(Mapper, CommandMapper)
    {
    }

    public TimePickerHandler(IPropertyMapper? mapper)
        : base(mapper ?? Mapper, CommandMapper)
    {
    }

    public TimePickerHandler(IPropertyMapper? mapper, CommandMapper? commandMapper)
        : base(mapper ?? Mapper, commandMapper ?? CommandMapper)
    {
    }

    ITimePicker ITimePickerHandler.VirtualView => VirtualView;

    object ITimePickerHandler.PlatformView => PlatformView;

    protected override PlatformView CreatePlatformView() => new PlatformView();

    public override bool NeedsContainer => false;

    protected override void ConnectHandler(PlatformView platformView)
    {
        platformView.SelectedTimeChanged += OnSelectedTimeChanged;
        base.ConnectHandler(platformView);
    }

    protected override void DisconnectHandler(PlatformView platformView)
    {
        platformView.SelectedTimeChanged -= OnSelectedTimeChanged;
        base.DisconnectHandler(platformView);
    }

    public static void MapTime(ITimePickerHandler handler, ITimePicker timePicker)
    {
        if (handler.PlatformView is not PlatformView platformView)
            return;

        platformView.UpdateTime(timePicker);
    }

    public static void MapFormat(ITimePickerHandler handler, ITimePicker timePicker)
    {
        if (handler.PlatformView is not PlatformView platformView)
            return;

        platformView.UpdateFormat(timePicker);
    }

    public static void MapCharacterSpacing(ITimePickerHandler handler, ITimePicker timePicker)
    {
        if (handler.PlatformView is not PlatformView platformView)
            return;

        platformView.UpdateCharacterSpacing(timePicker);
    }

    public static void MapFont(ITimePickerHandler handler, ITimePicker timePicker)
    {
        if (handler.PlatformView is not PlatformView platformView)
            return;

        var fontManager = handler.GetRequiredService<IFontManager>();
        platformView.UpdateFont(timePicker, fontManager);
    }

    public static void MapTextColor(ITimePickerHandler handler, ITimePicker timePicker)
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