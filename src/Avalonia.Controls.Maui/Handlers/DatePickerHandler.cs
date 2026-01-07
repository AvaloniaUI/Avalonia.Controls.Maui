using Avalonia.Controls.Maui.Platform;
using Microsoft.Maui;
using Microsoft.Maui.Handlers;
using PlatformView = Avalonia.Controls.DatePicker;

namespace Avalonia.Controls.Maui.Handlers;

public class DatePickerHandler : ViewHandler<IDatePicker, PlatformView>
{
    public static IPropertyMapper<IDatePicker, DatePickerHandler> Mapper = new PropertyMapper<IDatePicker, DatePickerHandler>(ViewHandler.ViewMapper)
    {
        [nameof(IDatePicker.CharacterSpacing)] = MapCharacterSpacing,
        [nameof(IDatePicker.Date)] = MapDate,
        [nameof(IDatePicker.Font)] = MapFont,
        [nameof(IDatePicker.Format)] = MapFormat,
        [nameof(IDatePicker.MaximumDate)] = MapMaximumDate,
        [nameof(IDatePicker.MinimumDate)] = MapMinimumDate,
        [nameof(IDatePicker.TextColor)] = MapTextColor,
    };

    public static CommandMapper<IPicker, DatePickerHandler> CommandMapper = new(ViewCommandMapper)
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

    public static void MapDate(DatePickerHandler handler, IDatePicker datePicker)
    {
        if (handler.PlatformView is not PlatformView platformView)
            return;

        platformView.UpdateDate(datePicker);
    }

    public static void MapMinimumDate(DatePickerHandler handler, IDatePicker datePicker)
    {
        if (handler.PlatformView is not PlatformView platformView)
            return;

        platformView.UpdateMinimumDate(datePicker);
    }

    public static void MapMaximumDate(DatePickerHandler handler, IDatePicker datePicker)
    {
        if (handler.PlatformView is not PlatformView platformView)
            return;

        platformView.UpdateMaximumDate(datePicker);
    }

    public static void MapFormat(DatePickerHandler handler, IDatePicker datePicker)
    {
        if (handler.PlatformView is not PlatformView platformView)
            return;

        platformView.UpdateFormat(datePicker);
    }

    public static void MapCharacterSpacing(DatePickerHandler handler, IDatePicker datePicker)
    {
        if (handler.PlatformView is not PlatformView platformView)
            return;

        platformView.UpdateCharacterSpacing(datePicker);
    }

    public static void MapFont(DatePickerHandler handler, IDatePicker datePicker)
    {
        if (handler.PlatformView is not PlatformView platformView)
            return;

        var fontManager = handler.GetRequiredService<IFontManager>();
        platformView.UpdateFont(datePicker, fontManager);
    }

    public static void MapTextColor(DatePickerHandler handler, IDatePicker datePicker)
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