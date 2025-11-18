using Microsoft.Maui;
using Microsoft.Maui.Handlers;
using AvaloniaTimePicker = global::Avalonia.Controls.TimePicker;

namespace Avalonia.Controls.Maui.Handlers;

/// <summary>
/// Handler for MAUI ITimePicker to Avalonia TimePicker mapping
/// </summary>
public class TimePickerHandler : ViewHandler<ITimePicker, AvaloniaTimePicker>, ITimePickerHandler
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

    System.Object ITimePickerHandler.PlatformView => PlatformView;

    protected override AvaloniaTimePicker CreatePlatformView()
    {
        return new AvaloniaTimePicker();
    }

    public override bool NeedsContainer => false;

    protected override void ConnectHandler(AvaloniaTimePicker platformView)
    {
        platformView.SelectedTimeChanged += OnSelectedTimeChanged;
        base.ConnectHandler(platformView);
    }

    protected override void DisconnectHandler(AvaloniaTimePicker platformView)
    {
        platformView.SelectedTimeChanged -= OnSelectedTimeChanged;
        base.DisconnectHandler(platformView);
    }

    private void OnSelectedTimeChanged(object? sender, global::Avalonia.Controls.TimePickerSelectedValueChangedEventArgs e)
    {
        if (VirtualView != null && e.NewTime.HasValue)
        {
            VirtualView.Time = e.NewTime.Value;
        }
    }

    public static void MapTime(ITimePickerHandler handler, ITimePicker timePicker)
    {
        if (handler.PlatformView is null || handler.VirtualView is null)
            return;

        var platformView = (AvaloniaTimePicker)handler.PlatformView;
        platformView.SelectedTime = timePicker.Time;
    }

    public static void MapFormat(ITimePickerHandler handler, ITimePicker timePicker)
    {
        if (handler.PlatformView is null || handler.VirtualView is null)
            return;

        var platformView = (AvaloniaTimePicker)handler.PlatformView;

        // Note: Avalonia TimePicker uses ClockIdentifier property for 12/24 hour format
        // MAUI uses a format string. We can detect common patterns:
        // - "t" or formats with "h" = 12 hour
        // - "T" or formats with "H" = 24 hour
        var format = timePicker.Format;
        if (!string.IsNullOrEmpty(format))
        {
            if (format.Contains("H") || format == "T")
            {
                platformView.ClockIdentifier = "24HourClock";
            }
            else if (format.Contains("h") || format == "t")
            {
                platformView.ClockIdentifier = "12HourClock";
            }
        }
    }

    public static void MapCharacterSpacing(ITimePickerHandler handler, ITimePicker timePicker)
    {
        if (handler.PlatformView is null || handler.VirtualView is null)
            return;

        // Note: Avalonia TimePicker doesn't directly support character spacing
        // This would require custom styling
        // TODO: Implement character spacing through custom styling
    }

    public static void MapFont(ITimePickerHandler handler, ITimePicker timePicker)
    {
        if (handler.PlatformView is null || handler.VirtualView is null)
            return;

        var fontManager = handler.GetRequiredService<IFontManager>();
        var platformView = (AvaloniaTimePicker)handler.PlatformView;
        var font = timePicker.Font;

        if (font.IsDefault)
            return;

        if (font.Size > 0)
            platformView.FontSize = font.Size;

        if (font.Family != null)
            platformView.FontFamily = font.Family;
    }

    public static void MapTextColor(ITimePickerHandler handler, ITimePicker timePicker)
    {
        if (handler.PlatformView is null || handler.VirtualView is null)
            return;

        var platformView = (AvaloniaTimePicker)handler.PlatformView;

        if (timePicker.TextColor != null)
        {
            platformView.Foreground = timePicker.TextColor.ToPlatform();
        }
    }
}
