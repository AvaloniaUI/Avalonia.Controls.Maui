using Avalonia.Controls.Maui.Platform;
using Microsoft.Maui;
using PlatformView = Avalonia.Controls.DatePicker;

namespace Avalonia.Controls.Maui.Handlers;

/// <summary>Avalonia handler for <see cref="IDatePicker"/>.</summary>
public class DatePickerHandler : ViewHandler<IDatePicker, PlatformView>
{
    /// <summary>Property mapper for <see cref="DatePickerHandler"/>.</summary>
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

    /// <summary>Command mapper for <see cref="DatePickerHandler"/>.</summary>
    public static CommandMapper<IPicker, DatePickerHandler> CommandMapper = new(ViewCommandMapper)
    {
    };

    /// <summary>Initializes a new instance of <see cref="DatePickerHandler"/>.</summary>
    public DatePickerHandler() : base(Mapper, CommandMapper)
    {
    }

    /// <summary>Initializes a new instance of <see cref="DatePickerHandler"/>.</summary>
    /// <param name="mapper">The property mapper to use, or <see langword="null"/> to use the default.</param>
    public DatePickerHandler(IPropertyMapper? mapper)
        : base(mapper ?? Mapper, CommandMapper)
    {
    }

    /// <summary>Initializes a new instance of <see cref="DatePickerHandler"/>.</summary>
    /// <param name="mapper">The property mapper to use, or <see langword="null"/> to use the default.</param>
    /// <param name="commandMapper">The command mapper to use, or <see langword="null"/> to use the default.</param>
    public DatePickerHandler(IPropertyMapper? mapper, CommandMapper? commandMapper)
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
        platformView.SelectedDateChanged += OnSelectedDateChanged;
        base.ConnectHandler(platformView);
    }

    /// <inheritdoc/>
    protected override void DisconnectHandler(PlatformView platformView)
    {
        platformView.SelectedDateChanged -= OnSelectedDateChanged;
        base.DisconnectHandler(platformView);
    }

    /// <summary>Maps the Date property to the platform view.</summary>
    /// <param name="handler">The handler for the date picker.</param>
    /// <param name="datePicker">The virtual view.</param>
    public static void MapDate(DatePickerHandler handler, IDatePicker datePicker)
    {
        if (handler.PlatformView is not PlatformView platformView)
            return;

        platformView.UpdateDate(datePicker);
    }

    /// <summary>Maps the MinimumDate property to the platform view.</summary>
    /// <param name="handler">The handler for the date picker.</param>
    /// <param name="datePicker">The virtual view.</param>
    public static void MapMinimumDate(DatePickerHandler handler, IDatePicker datePicker)
    {
        if (handler.PlatformView is not PlatformView platformView)
            return;

        platformView.UpdateMinimumDate(datePicker);
    }

    /// <summary>Maps the MaximumDate property to the platform view.</summary>
    /// <param name="handler">The handler for the date picker.</param>
    /// <param name="datePicker">The virtual view.</param>
    public static void MapMaximumDate(DatePickerHandler handler, IDatePicker datePicker)
    {
        if (handler.PlatformView is not PlatformView platformView)
            return;

        platformView.UpdateMaximumDate(datePicker);
    }

    /// <summary>Maps the Format property to the platform view.</summary>
    /// <param name="handler">The handler for the date picker.</param>
    /// <param name="datePicker">The virtual view.</param>
    public static void MapFormat(DatePickerHandler handler, IDatePicker datePicker)
    {
        if (handler.PlatformView is not PlatformView platformView)
            return;

        platformView.UpdateFormat(datePicker);
    }

    /// <summary>Maps the CharacterSpacing property to the platform view.</summary>
    /// <param name="handler">The handler for the date picker.</param>
    /// <param name="datePicker">The virtual view.</param>
    public static void MapCharacterSpacing(DatePickerHandler handler, IDatePicker datePicker)
    {
        if (handler.PlatformView is not PlatformView platformView)
            return;

        platformView.UpdateCharacterSpacing(datePicker);
    }

    /// <summary>Maps the Font property to the platform view.</summary>
    /// <param name="handler">The handler for the date picker.</param>
    /// <param name="datePicker">The virtual view.</param>
    public static void MapFont(DatePickerHandler handler, IDatePicker datePicker)
    {
        if (handler.PlatformView is not PlatformView platformView)
            return;

        var fontManager = handler.GetRequiredService<IFontManager>();
        platformView.UpdateFont(datePicker, fontManager);
    }

    /// <summary>Maps the TextColor property to the platform view.</summary>
    /// <param name="handler">The handler for the date picker.</param>
    /// <param name="datePicker">The virtual view.</param>
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