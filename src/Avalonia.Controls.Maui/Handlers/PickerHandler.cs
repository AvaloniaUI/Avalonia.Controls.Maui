using Avalonia.Controls;
using Microsoft.Maui;
using Microsoft.Maui.Handlers;
using Microsoft.Maui.Platform;
using PlatformView = System.Object;

namespace Avalonia.Controls.Maui.Handlers;

public partial class PickerHandler : ViewHandler<IPicker, ComboBox>, IPickerHandler
{
    public static IPropertyMapper<IPicker, IPickerHandler> Mapper = new PropertyMapper<IPicker, PickerHandler>(ViewMapper)
    {
        [nameof(IPicker.CharacterSpacing)] = MapCharacterSpacing,
        [nameof(IPicker.Font)] = MapFont,
        [nameof(IPicker.SelectedIndex)] = MapSelectedIndex,
        [nameof(IPicker.TextColor)] = MapTextColor,
        [nameof(IPicker.Title)] = MapTitle,
        [nameof(IPicker.TitleColor)] = MapTitleColor,
        [nameof(ITextAlignment.HorizontalTextAlignment)] = MapHorizontalTextAlignment,
        [nameof(ITextAlignment.VerticalTextAlignment)] = MapVerticalTextAlignment,
        [nameof(IPicker.Items)] = MapItems,
    };

    public static void MapItems(PickerHandler handler, IPicker picker)
    {
        if (handler.PlatformView is null || handler.VirtualView is null)
            return;
        handler.PlatformView.ItemsSource = new ItemDelegateList<string>(handler.VirtualView);
    }

    public static void MapVerticalTextAlignment(PickerHandler handler, IPicker picker)
    {
        if (handler.PlatformView is null || handler.VirtualView is null)
            return;
        switch (picker.VerticalTextAlignment)
        {
            case TextAlignment.Start:
                handler.PlatformView.VerticalContentAlignment = global::Avalonia.Layout.VerticalAlignment.Top;
                break;
            case TextAlignment.Center:
                handler.PlatformView.VerticalContentAlignment = global::Avalonia.Layout.VerticalAlignment.Center;
                break;
            case TextAlignment.End:
                handler.PlatformView.VerticalContentAlignment = global::Avalonia.Layout.VerticalAlignment.Bottom;
                break;
        }
    }

    public static void MapHorizontalTextAlignment(PickerHandler handler, IPicker picker)
    {
        if (handler.PlatformView is null || handler.VirtualView is null)
            return;
        switch (picker.HorizontalTextAlignment)
        {
            case TextAlignment.Start:
                handler.PlatformView.HorizontalContentAlignment = global::Avalonia.Layout.HorizontalAlignment.Left;
                break;
            case TextAlignment.Center:
                handler.PlatformView.HorizontalContentAlignment = global::Avalonia.Layout.HorizontalAlignment.Center;
                break;
            case TextAlignment.End:
                handler.PlatformView.HorizontalContentAlignment = global::Avalonia.Layout.HorizontalAlignment.Right;
                break;
        }
    }

    public static void MapTitleColor(PickerHandler handler, IPicker picker)
    {
        if (handler.PlatformView is null || handler.VirtualView is null)
            return;
        if (picker.TitleColor != null)
            handler.PlatformView.PlaceholderForeground = picker.TitleColor.ToPlatform();
    }

    public static void MapTitle(PickerHandler handler, IPicker picker)
    {
        if (handler.PlatformView is null || handler.VirtualView is null)
            return;
        handler.PlatformView.PlaceholderText = picker.Title;
    }

    public static void MapTextColor(PickerHandler handler, IPicker picker)
    {
        if (handler.PlatformView is null || handler.VirtualView is null)
            return;
        if (picker.TextColor != null)
            handler.PlatformView.Foreground = picker.TextColor.ToPlatform();
    }

    public static void MapSelectedIndex(PickerHandler handler, IPicker picker)
    {
        if (handler.PlatformView is null || handler.VirtualView is null)
            return;
        if (picker.SelectedIndex < 0 || picker.SelectedIndex >= picker.GetCount())
            handler.PlatformView.SelectedIndex = -1;
        else
            handler.PlatformView.SelectedIndex = picker.SelectedIndex;
    }

    public static void MapFont(PickerHandler handler, IPicker picker)
    {
        if (handler.PlatformView is null || handler.VirtualView is null)
            return;
        // HACK: This should be in an extension method.
        var fontManager = handler.GetRequiredService<IFontManager>();
        if (handler.PlatformView is null || handler.VirtualView is null)
            return;
        if (fontManager == null)
            throw new ArgumentNullException(nameof(fontManager));
        var font = picker.Font;
        var platformButton = (ComboBox)(handler.PlatformView);
        if (font.IsDefault)
            return;
        if (font.Size > 0)
            platformButton.FontSize = font.Size;
        if (font.Family != null)
            platformButton.FontFamily = font.Family;
    }

    public static void MapCharacterSpacing(PickerHandler handler, IPicker picker)
    {
        if (handler.PlatformView is null || handler.VirtualView is null)
            return;
        // TODO: Implement CharacterSpacing
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
}