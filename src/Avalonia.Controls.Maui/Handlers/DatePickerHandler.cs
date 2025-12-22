using Avalonia.Controls.Maui.Platform;
using Microsoft.Maui;
using Microsoft.Maui.Handlers;
using PlatformView = Avalonia.Controls.DatePicker;

namespace Avalonia.Controls.Maui.Handlers;

public class DatePickerHandler : ViewHandler<IDatePicker, PlatformView>, IDatePickerHandler
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

    object IDatePickerHandler.PlatformView => PlatformView;

    protected override PlatformView CreatePlatformView() => new PlatformView();

    public override bool NeedsContainer => false;

    protected override void ConnectHandler(PlatformView platformView)
    {
        platformView.SelectedDateChanged += OnSelectedDateChanged;
        base.ConnectHandler(platformView);
    }

    protected override void DisconnectHandler(PlatformView platformView)
    {
        platformView.SelectedDateChanged -= OnSelectedDateChanged;
        base.DisconnectHandler(platformView);
    }

    public static void MapDate(IDatePickerHandler handler, IDatePicker datePicker)
    {
        if (handler.PlatformView is not PlatformView platformView)
            return;

        platformView.UpdateDate(datePicker);
    }

    public static void MapMinimumDate(IDatePickerHandler handler, IDatePicker datePicker)
    {
        if (handler.PlatformView is not PlatformView platformView)
            return;

        platformView.UpdateMinimumDate(datePicker);
    }

    public static void MapMaximumDate(IDatePickerHandler handler, IDatePicker datePicker)
    {
        if (handler.PlatformView is not PlatformView platformView)
            return;

        platformView.UpdateMaximumDate(datePicker);
    }

    public static void MapFormat(IDatePickerHandler handler, IDatePicker datePicker)
    {
        if (handler.PlatformView is not PlatformView platformView)
            return;

        platformView.UpdateFormat(datePicker);
    }

    public static void MapCharacterSpacing(IDatePickerHandler handler, IDatePicker datePicker)
    {
        if (handler.PlatformView is not PlatformView platformView)
            return;

        platformView.UpdateCharacterSpacing(datePicker);
    }

    public static void MapFont(IDatePickerHandler handler, IDatePicker datePicker)
    {
        if (handler.PlatformView is not PlatformView platformView)
            return;

        var fontManager = handler.GetRequiredService<IFontManager>();
        platformView.UpdateFont(datePicker, fontManager);
    }

    public static void MapTextColor(IDatePickerHandler handler, IDatePicker datePicker)
    {
        if (handler.PlatformView is not PlatformView platformView)
            return;

        platformView.UpdateTextColor(datePicker);
    }
    
    void OnSelectedDateChanged(object? sender, DatePickerSelectedValueChangedEventArgs e)
    {
        if (VirtualView != null && e.NewDate.HasValue)
            VirtualView.Date = e.NewDate.Value.DateTime;
    }
}