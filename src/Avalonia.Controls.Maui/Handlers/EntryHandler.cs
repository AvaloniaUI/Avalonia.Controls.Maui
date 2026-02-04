using Avalonia.Controls.Maui.Extensions;
using Avalonia.Controls.Maui.Platform;
using Avalonia.Input;
using Avalonia.Input.TextInput;
using Microsoft.Maui;
using Avalonia.Controls.Maui.Controls;

namespace Avalonia.Controls.Maui.Handlers;

public class EntryHandler : ViewHandler<IEntry, MauiEntry>
{
    public static IPropertyMapper<IEntry, EntryHandler> Mapper = new PropertyMapper<IEntry, EntryHandler>(ViewHandler.ViewMapper)
    {
        [nameof(IEntry.Background)] = MapBackground,
        [nameof(IEntry.CharacterSpacing)] = MapCharacterSpacing,
        [nameof(IEntry.ClearButtonVisibility)] = MapClearButtonVisibility,
        [nameof(IEntry.Font)] = MapFont,
        [nameof(IEntry.IsPassword)] = MapIsPassword,
        [nameof(IEntry.HorizontalTextAlignment)] = MapHorizontalTextAlignment,
        [nameof(IEntry.VerticalTextAlignment)] = MapVerticalTextAlignment,
        [nameof(IEntry.IsReadOnly)] = MapIsReadOnly,
        [nameof(IEntry.IsTextPredictionEnabled)] = MapIsTextPredictionEnabled,
        [nameof(IEntry.IsSpellCheckEnabled)] = MapIsSpellCheckEnabled,
        [nameof(IEntry.Keyboard)] = MapKeyboard,
        [nameof(IEntry.MaxLength)] = MapMaxLength,
        [nameof(IEntry.Placeholder)] = MapPlaceholder,
        [nameof(IEntry.PlaceholderColor)] = MapPlaceholderColor,
        [nameof(IEntry.ReturnType)] = MapReturnType,
        [nameof(IEntry.Text)] = MapText,
        [nameof(IEntry.TextColor)] = MapTextColor,
        [nameof(IEntry.CursorPosition)] = MapCursorPosition,
        [nameof(IEntry.SelectionLength)] = MapSelectionLength,
        [nameof(Microsoft.Maui.Controls.Entry.TextTransform)] = MapTextTransform
    };
    
    public static CommandMapper<IEntry, EntryHandler> CommandMapper = new(ViewCommandMapper);
    
    public EntryHandler() : base(Mapper, CommandMapper)
    {
    }
    
    public EntryHandler(IPropertyMapper? mapper)
        : base(mapper ?? Mapper, CommandMapper)
    {
    }
    
    public EntryHandler(IPropertyMapper mapper, CommandMapper? commandMapper = null)
        : base(mapper, commandMapper)
    {
    }

    protected override MauiEntry CreatePlatformView()
    {
        return new MauiEntry();
    }

    protected override void ConnectHandler(MauiEntry platformView)
    {
        base.ConnectHandler(platformView);
        platformView.TextChanged += OnTextChanged;
        platformView.KeyDown += OnKeyDown;
    }

    protected override void DisconnectHandler(MauiEntry platformView)
    {
        platformView.TextChanged -= OnTextChanged;
        platformView.KeyDown -= OnKeyDown;
        base.DisconnectHandler(platformView);
    }
    
    public override bool NeedsContainer => false;
    
    public static void MapBackground(EntryHandler handler, IEntry entry)
    {
        handler.UpdateValue(nameof(IViewHandler.ContainerView));
        handler.PlatformView?.UpdateBackground(entry);
    }
    
    public static void MapText(EntryHandler handler, IEntry entry) =>
        handler.PlatformView?.UpdateText(entry);
    
    public static void MapTextColor(EntryHandler handler, IEntry entry) =>
        handler.PlatformView?.UpdateTextColor(entry);
    
    public static void MapCharacterSpacing(EntryHandler handler, IEntry entry) =>
        handler.PlatformView?.UpdateCharacterSpacing(entry);
    
    public static void MapFont(EntryHandler handler, IEntry entry)
    {
        var fontManager = handler.GetRequiredService<IFontManager>();
        handler.PlatformView?.UpdateFont(entry, fontManager);
    }
    
    public static void MapHorizontalTextAlignment(EntryHandler handler, IEntry entry) =>
        handler.PlatformView?.UpdateHorizontalTextAlignment(entry);
    
    public static void MapVerticalTextAlignment(EntryHandler handler, IEntry entry) =>
        handler.PlatformView?.UpdateVerticalTextAlignment(entry);
    
    public static void MapIsPassword(EntryHandler handler, IEntry entry) =>
        handler.PlatformView?.UpdateIsPassword(entry);
    
    public static void MapIsReadOnly(EntryHandler handler, IEntry entry) =>
        handler.PlatformView?.UpdateIsReadOnly(entry);
    
    public static void MapIsTextPredictionEnabled(EntryHandler handler, IEntry entry)
    {
        if (handler.PlatformView is MauiEntry textBox)
        {
            TextInputOptions.SetShowSuggestions(textBox, entry.IsTextPredictionEnabled);
        }
    }
    
    public static void MapIsSpellCheckEnabled(EntryHandler handler, IEntry entry) =>
        handler.PlatformView?.UpdateIsSpellCheckEnabled(entry);
    
    public static void MapKeyboard(EntryHandler handler, IEntry entry)
    {
        handler.PlatformView?.UpdateKeyboard(entry.Keyboard);
    }
    
    public static void MapMaxLength(EntryHandler handler, IEntry entry) =>
        handler.PlatformView?.UpdateMaxLength(entry);
    
    public static void MapPlaceholder(EntryHandler handler, IEntry entry) =>
        handler.PlatformView?.UpdatePlaceholder(entry);
    
    public static void MapPlaceholderColor(EntryHandler handler, IEntry entry) =>
        handler.PlatformView?.UpdatePlaceholderColor(entry);
    
    public static void MapReturnType(EntryHandler handler, IEntry entry) =>
        handler.PlatformView?.UpdateReturnType(entry);
    
    public static void MapClearButtonVisibility(EntryHandler handler, IEntry entry) =>
        handler.PlatformView?.UpdateClearButtonVisibility(entry);
    
    public static void MapCursorPosition(EntryHandler handler, IEntry entry) =>
        handler.PlatformView?.UpdateCursorPosition(entry);
    
    public static void MapSelectionLength(EntryHandler handler, IEntry entry) =>
        handler.PlatformView?.UpdateSelectionLength(entry);
    
    public static void MapTextTransform(EntryHandler handler, IEntry entry) =>
        handler.PlatformView?.UpdateTextTransform(entry);
    
    private void OnTextChanged(object? sender, TextChangedEventArgs e)
    {
        if (VirtualView == null || PlatformView == null)
            return;

        VirtualView.Text = PlatformView.Text ?? string.Empty;
    }

    private void OnKeyDown(object? sender, KeyEventArgs e)
    {
        if (e.Key == Key.Enter && VirtualView != null)
        {
            VirtualView.Completed();
        }
    }
}