using Microsoft.Maui;
using Microsoft.Maui.Handlers;
using AvaloniaDatePicker = global::Avalonia.Controls.DatePicker;

namespace Avalonia.Controls.Maui.Handlers;

/// <summary>
/// Handler for MAUI IDatePicker to Avalonia DatePicker mapping
/// </summary>
public class DatePickerHandler : ViewHandler<IDatePicker, AvaloniaDatePicker>, IDatePickerHandler
{
    public static IPropertyMapper<IDatePicker, IDatePickerHandler> Mapper = new PropertyMapper<IDatePicker, IDatePickerHandler>(ViewHandler.ViewMapper)
    {
        [nameof(IDatePicker.CharacterSpacing)] = MapCharacterSpacing,
        [nameof(IDatePicker.Date)] = MapDate,
        [nameof(IDatePicker.Font)] = MapFont,
        [nameof(IDatePicker.Format)] = MapFormat,
        [nameof(IDatePicker.MaximumDate)] = MapMaximumDate,
        [nameof(IDatePicker.MinimumDate)] = MapMinimumDate,
        [nameof(IDatePicker.TextColor)] = MapTextColor,
    };

    public static CommandMapper<IPicker, IDatePickerHandler> CommandMapper = new(ViewCommandMapper)
    {
    };

    public DatePickerHandler() : base(Mapper, CommandMapper)
    {
    }

    public DatePickerHandler(IPropertyMapper? mapper)
        : base(mapper ?? Mapper, CommandMapper)
    {
    }

    public DatePickerHandler(IPropertyMapper? mapper, CommandMapper? commandMapper)
        : base(mapper ?? Mapper, commandMapper ?? CommandMapper)
    {
    }

    IDatePicker IDatePickerHandler.VirtualView => VirtualView;

    System.Object IDatePickerHandler.PlatformView => PlatformView;

    protected override AvaloniaDatePicker CreatePlatformView()
    {
        return new AvaloniaDatePicker();
    }

    public override bool NeedsContainer => false;

    protected override void ConnectHandler(AvaloniaDatePicker platformView)
    {
        platformView.SelectedDateChanged += OnSelectedDateChanged;
        base.ConnectHandler(platformView);
    }

    protected override void DisconnectHandler(AvaloniaDatePicker platformView)
    {
        platformView.SelectedDateChanged -= OnSelectedDateChanged;
        base.DisconnectHandler(platformView);
    }

    private void OnSelectedDateChanged(object? sender, global::Avalonia.Controls.DatePickerSelectedValueChangedEventArgs e)
    {
        if (VirtualView != null && e.NewDate.HasValue)
        {
            VirtualView.Date = e.NewDate.Value.DateTime;
        }
    }

    public static void MapDate(IDatePickerHandler handler, IDatePicker datePicker)
    {
        if (handler.PlatformView is null || handler.VirtualView is null)
            return;

        var platformView = (AvaloniaDatePicker)handler.PlatformView;
        platformView.SelectedDate = new DateTimeOffset(datePicker.Date);
    }

    public static void MapMinimumDate(IDatePickerHandler handler, IDatePicker datePicker)
    {
        if (handler.PlatformView is null || handler.VirtualView is null)
            return;

        // Note: Avalonia DatePicker doesn't have built-in MinimumDate property
        // This would require custom validation or template modification
        // TODO: Implement custom validation for minimum date
    }

    public static void MapMaximumDate(IDatePickerHandler handler, IDatePicker datePicker)
    {
        if (handler.PlatformView is null || handler.VirtualView is null)
            return;

        // Note: Avalonia DatePicker doesn't have built-in MaximumDate property
        // This would require custom validation or template modification
        // TODO: Implement custom validation for maximum date
    }

    public static void MapFormat(IDatePickerHandler handler, IDatePicker datePicker)
    {
        if (handler.PlatformView is null || handler.VirtualView is null)
            return;

        // Note: Avalonia DatePicker uses separate DayFormat, MonthFormat, YearFormat properties
        // MAUI uses a single Format string. This would require parsing the format string
        // and mapping to individual format properties
        // TODO: Implement format string parsing and mapping
    }

    public static void MapCharacterSpacing(IDatePickerHandler handler, IDatePicker datePicker)
    {
        if (handler.PlatformView is null || handler.VirtualView is null)
            return;

        // Note: Avalonia DatePicker doesn't directly support character spacing
        // This would require custom styling
        // TODO: Implement character spacing through custom styling
    }

    public static void MapFont(IDatePickerHandler handler, IDatePicker datePicker)
    {
        if (handler.PlatformView is null || handler.VirtualView is null)
            return;

        var fontManager = handler.GetRequiredService<IFontManager>();
        var platformView = (AvaloniaDatePicker)handler.PlatformView;
        var font = datePicker.Font;

        if (font.IsDefault)
            return;

        if (font.Size > 0)
            platformView.FontSize = font.Size;

        if (font.Family != null)
            platformView.FontFamily = font.Family;
    }

    public static void MapTextColor(IDatePickerHandler handler, IDatePicker datePicker)
    {
        if (handler.PlatformView is null || handler.VirtualView is null)
            return;

        var platformView = (AvaloniaDatePicker)handler.PlatformView;

        if (datePicker.TextColor != null)
        {
            platformView.Foreground = datePicker.TextColor.ToPlatform();
        }
    }
}
