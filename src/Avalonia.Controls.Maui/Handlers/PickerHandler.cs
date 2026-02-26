using Avalonia.Controls.Maui.Platform;
using Microsoft.Maui;
using PlatformView = System.Object;

namespace Avalonia.Controls.Maui.Handlers;

/// <summary>Avalonia handler for <see cref="IPicker"/>.</summary>
public partial class PickerHandler : ViewHandler<IPicker, MauiComboBox>
{
    bool _isUpdatingSelection;

    /// <summary>Property mapper for <see cref="PickerHandler"/>.</summary>
    public static IPropertyMapper<IPicker, PickerHandler> Mapper = new PropertyMapper<IPicker, PickerHandler>(ViewMapper)
    {
        [nameof(IPicker.CharacterSpacing)] = MapCharacterSpacing,
        [nameof(IPicker.Font)] = MapFont,
        [nameof(IPicker.Items)] = MapItems,
        [nameof(IPicker.SelectedIndex)] = MapSelectedIndex,
        [nameof(IPicker.TextColor)] = MapTextColor,
        [nameof(IPicker.Title)] = MapTitle,
        [nameof(IPicker.TitleColor)] = MapTitleColor,
        [nameof(ITextAlignment.HorizontalTextAlignment)] = MapHorizontalTextAlignment,
        [nameof(ITextAlignment.VerticalTextAlignment)] = MapVerticalTextAlignment,
    };

    /// <summary>Maps the Items property to the platform view.</summary>
    /// <param name="handler">The handler for the picker.</param>
    /// <param name="picker">The virtual view.</param>
    public static void MapItems(PickerHandler handler, IPicker picker)
    {
        if (handler.PlatformView is null || handler.VirtualView is null)
            return;

        handler._isUpdatingSelection = true;
        try
        {
            handler.PlatformView.UpdateItems(handler.VirtualView);
            handler.PlatformView.UpdateSelectedIndex(handler.VirtualView);
        }
        finally
        {
            handler._isUpdatingSelection = false;
        }
    }

    /// <summary>Maps the VerticalTextAlignment property to the platform view.</summary>
    /// <param name="handler">The handler for the picker.</param>
    /// <param name="picker">The virtual view.</param>
    public static void MapVerticalTextAlignment(PickerHandler handler, IPicker picker)
    {
        if (handler.PlatformView is null || handler.VirtualView is null)
            return;
        handler.PlatformView.UpdateVerticalTextAlignment(handler.VirtualView);
    }

    /// <summary>Maps the HorizontalTextAlignment property to the platform view.</summary>
    /// <param name="handler">The handler for the picker.</param>
    /// <param name="picker">The virtual view.</param>
    public static void MapHorizontalTextAlignment(PickerHandler handler, IPicker picker)
    {
        if (handler.PlatformView is null || handler.VirtualView is null)
            return;
        handler.PlatformView.UpdateHorizontalTextAlignment(handler.VirtualView);
    }

    /// <summary>Maps the TitleColor property to the platform view.</summary>
    /// <param name="handler">The handler for the picker.</param>
    /// <param name="picker">The virtual view.</param>
    public static void MapTitleColor(PickerHandler handler, IPicker picker)
    {
        if (handler.PlatformView is null || handler.VirtualView is null)
            return;
        handler.PlatformView.UpdateTitleColor(handler.VirtualView);
    }

    /// <summary>Maps the Title property to the platform view.</summary>
    /// <param name="handler">The handler for the picker.</param>
    /// <param name="picker">The virtual view.</param>
    public static void MapTitle(PickerHandler handler, IPicker picker)
    {
        if (handler.PlatformView is null || handler.VirtualView is null)
            return;
        handler.PlatformView.UpdateTitle(handler.VirtualView);
    }

    /// <summary>Maps the TextColor property to the platform view.</summary>
    /// <param name="handler">The handler for the picker.</param>
    /// <param name="picker">The virtual view.</param>
    public static void MapTextColor(PickerHandler handler, IPicker picker)
    {
        if (handler.PlatformView is null || handler.VirtualView is null)
            return;
        handler.PlatformView.UpdateTextColor(handler.VirtualView);
    }

    /// <summary>Maps the SelectedIndex property to the platform view.</summary>
    /// <param name="handler">The handler for the picker.</param>
    /// <param name="picker">The virtual view.</param>
    public static void MapSelectedIndex(PickerHandler handler, IPicker picker)
    {
        if (handler.PlatformView is null || handler.VirtualView is null)
            return;
        handler._isUpdatingSelection = true;
        try
        {
            handler.PlatformView.UpdateSelectedIndex(handler.VirtualView);
        }
        finally
        {
            handler._isUpdatingSelection = false;
        }
    }

    /// <summary>Maps the Font property to the platform view.</summary>
    /// <param name="handler">The handler for the picker.</param>
    /// <param name="picker">The virtual view.</param>
    public static void MapFont(PickerHandler handler, IPicker picker)
    {
        if (handler.PlatformView is null || handler.VirtualView is null)
            return;
        var fontManager = handler.GetRequiredService<IFontManager>();
        handler.PlatformView.UpdateFont(handler.VirtualView, fontManager);
        handler.PlatformView.UpdateCharacterSpacing(handler.VirtualView);
    }

    /// <summary>Maps the CharacterSpacing property to the platform view.</summary>
    /// <param name="handler">The handler for the picker.</param>
    /// <param name="picker">The virtual view.</param>
    public static void MapCharacterSpacing(PickerHandler handler, IPicker picker)
    {
        if (handler.PlatformView is null || handler.VirtualView is null)
            return;
        handler.PlatformView.UpdateCharacterSpacing(handler.VirtualView);
    }

    /// <summary>Command mapper for <see cref="PickerHandler"/>.</summary>
    public static CommandMapper<IPicker, PickerHandler> CommandMapper = new(ViewCommandMapper)
    {
    };

    /// <summary>Initializes a new instance of <see cref="PickerHandler"/>.</summary>
    public PickerHandler() : base(Mapper, CommandMapper)
    {
    }

    /// <summary>Initializes a new instance of <see cref="PickerHandler"/>.</summary>
    /// <param name="mapper">The property mapper to use, or <see langword="null"/> to use the default.</param>
    public PickerHandler(IPropertyMapper? mapper)
        : base(mapper ?? Mapper, CommandMapper)
    {
    }

    /// <summary>Initializes a new instance of <see cref="PickerHandler"/>.</summary>
    /// <param name="mapper">The property mapper to use, or <see langword="null"/> to use the default.</param>
    /// <param name="commandMapper">The command mapper to use, or <see langword="null"/> to use the default.</param>
    public PickerHandler(IPropertyMapper? mapper, CommandMapper? commandMapper)
        : base(mapper ?? Mapper, commandMapper ?? CommandMapper)
    {
    }

    /// <summary>Creates the Avalonia platform view for this handler.</summary>
    protected override MauiComboBox CreatePlatformView()
    {
        return new MauiComboBox();
    }

    /// <inheritdoc/>
    protected override void ConnectHandler(MauiComboBox platformView)
    {
        base.ConnectHandler(platformView);

        platformView.DropDownOpened += OnDropDownOpened;
        platformView.DropDownClosed += OnDropDownClosed;
        platformView.SelectionChanged += OnSelectionChanged;
        
        if (VirtualView != null)
        {
            _isUpdatingSelection = true;
            try
            {
                platformView.SelectedIndex = VirtualView.SelectedIndex;
            }
            finally
            {
                _isUpdatingSelection = false;
            }
        }
    }

    /// <inheritdoc/>
    protected override void DisconnectHandler(MauiComboBox platformView)
    {
        platformView.DropDownOpened -= OnDropDownOpened;
        platformView.DropDownClosed -= OnDropDownClosed;
        platformView.SelectionChanged -= OnSelectionChanged;

        base.DisconnectHandler(platformView);
    }

    private void OnDropDownOpened(object? sender, EventArgs e)
    {
        if (VirtualView is null || PlatformView is null)
            return;
        
        // TODO: Implement Picker.Opened event (API shown for .NET 10; current target is .NET 9) https://learn.microsoft.com/en-us/dotnet/api/microsoft.maui.controls.picker.opened?view=net-maui-10.0
    }
    
    private void OnDropDownClosed(object? sender, EventArgs e)
    {
        if (VirtualView is null || PlatformView is null)
            return;
        
        // TODO: Implement Picker.Closed event (API shown for .NET 10; current target is .NET 9) https://learn.microsoft.com/en-us/dotnet/api/microsoft.maui.controls.picker.closed?view=net-maui-10.0
    }
    
    private void OnSelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (VirtualView is null || PlatformView is null)
            return;

        if (_isUpdatingSelection)
            return;

        VirtualView.SelectedIndex = PlatformView.SelectedIndex;
    }
}
