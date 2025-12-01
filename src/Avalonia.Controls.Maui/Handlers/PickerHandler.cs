using Avalonia.Controls;
using Avalonia.Controls.Maui.Platform;
using Microsoft.Maui;
using Microsoft.Maui.Handlers;
using Microsoft.Maui.Platform;
using PlatformView = System.Object;

namespace Avalonia.Controls.Maui.Handlers;

public partial class PickerHandler : ViewHandler<IPicker, ComboBox>, IPickerHandler
{
    bool _isUpdatingSelection;

    public static IPropertyMapper<IPicker, IPickerHandler> Mapper = new PropertyMapper<IPicker, PickerHandler>(ViewMapper)
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

    public static void MapVerticalTextAlignment(PickerHandler handler, IPicker picker)
    {
        if (handler.PlatformView is null || handler.VirtualView is null)
            return;
        handler.PlatformView.UpdateVerticalTextAlignment(handler.VirtualView);
    }

    public static void MapHorizontalTextAlignment(PickerHandler handler, IPicker picker)
    {
        if (handler.PlatformView is null || handler.VirtualView is null)
            return;
        handler.PlatformView.UpdateHorizontalTextAlignment(handler.VirtualView);
    }

    public static void MapTitleColor(PickerHandler handler, IPicker picker)
    {
        if (handler.PlatformView is null || handler.VirtualView is null)
            return;
        handler.PlatformView.UpdateTitleColor(handler.VirtualView);
    }

    public static void MapTitle(PickerHandler handler, IPicker picker)
    {
        if (handler.PlatformView is null || handler.VirtualView is null)
            return;
        handler.PlatformView.UpdateTitle(handler.VirtualView);
    }

    public static void MapTextColor(PickerHandler handler, IPicker picker)
    {
        if (handler.PlatformView is null || handler.VirtualView is null)
            return;
        handler.PlatformView.UpdateTextColor(handler.VirtualView);
    }

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

    public static void MapFont(PickerHandler handler, IPicker picker)
    {
        if (handler.PlatformView is null || handler.VirtualView is null)
            return;
        var fontManager = handler.GetRequiredService<IFontManager>();
        handler.PlatformView.UpdateFont(handler.VirtualView, fontManager);
        handler.PlatformView.UpdateCharacterSpacing(handler.VirtualView);
    }

    public static void MapCharacterSpacing(PickerHandler handler, IPicker picker)
    {
        if (handler.PlatformView is null || handler.VirtualView is null)
            return;
        handler.PlatformView.UpdateCharacterSpacing(handler.VirtualView);
    }

    public static CommandMapper<IPicker, IPickerHandler> CommandMapper = new(ViewCommandMapper)
    {
    };

    public PickerHandler() : base(Mapper, CommandMapper)
    {
    }

    public PickerHandler(IPropertyMapper? mapper)
        : base(mapper ?? Mapper, CommandMapper)
    {
    }

    public PickerHandler(IPropertyMapper? mapper, CommandMapper? commandMapper)
        : base(mapper ?? Mapper, commandMapper ?? CommandMapper)
    {
    }

    IPicker IPickerHandler.VirtualView => VirtualView;

    PlatformView IPickerHandler.PlatformView => PlatformView;

    protected override ComboBox CreatePlatformView()
    {
        return new ComboBox();
    }

    protected override void ConnectHandler(ComboBox platformView)
    {
        base.ConnectHandler(platformView);
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

    protected override void DisconnectHandler(ComboBox platformView)
    {
        platformView.SelectionChanged -= OnSelectionChanged;

        base.DisconnectHandler(platformView);
    }

    void OnSelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (VirtualView is null || PlatformView is null)
            return;

        if (_isUpdatingSelection)
            return;

        VirtualView.SelectedIndex = PlatformView.SelectedIndex;
    }
}
