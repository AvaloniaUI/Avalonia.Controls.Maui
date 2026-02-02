using Avalonia.Controls;
using Avalonia.Controls.Maui.Extensions;
using Avalonia.Controls.Maui.Platform;
using Avalonia.Input.TextInput;
using Avalonia.Media;
using Microsoft.Maui;
using Microsoft.Maui.Graphics;
using System;
using AvaloniaTextBox = Avalonia.Controls.TextBox;
using AvaloniaTextAlignment = Avalonia.Media.TextAlignment;
using AvaloniaVerticalAlignment = Avalonia.Layout.VerticalAlignment;

namespace Avalonia.Controls.Maui.Handlers;

public class EntryHandler : ViewHandler<IEntry, AvaloniaTextBox>
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
        [nameof(IEntry.SelectionLength)] = MapSelectionLength
    };

    public static CommandMapper<IEntry, EntryHandler> CommandMapper = new(ViewCommandMapper)
    {
    };

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

    protected override AvaloniaTextBox CreatePlatformView()
    {
        return new AvaloniaTextBox();
    }

    protected override void ConnectHandler(AvaloniaTextBox platformView)
    {
        base.ConnectHandler(platformView);
        platformView.TextChanged += OnTextChanged;
    }

    protected override void DisconnectHandler(AvaloniaTextBox platformView)
    {
        platformView.TextChanged -= OnTextChanged;
        base.DisconnectHandler(platformView);
    }

    private void OnTextChanged(object? sender, TextChangedEventArgs e)
    {
        if (VirtualView == null || PlatformView == null)
            return;

        VirtualView.Text = PlatformView.Text ?? string.Empty;
    }

    public override bool NeedsContainer => false;

    public static void MapBackground(EntryHandler handler, IEntry entry)
    {
        handler.UpdateValue(nameof(IViewHandler.ContainerView));
        ((AvaloniaTextBox)handler.PlatformView)?.UpdateBackground(entry);
    }

    public static void MapText(EntryHandler handler, IEntry entry) =>
        ((AvaloniaTextBox)handler.PlatformView)?.UpdateText(entry);

    public static void MapTextColor(EntryHandler handler, IEntry entry) =>
        ((AvaloniaTextBox)handler.PlatformView)?.UpdateTextColor(entry);

    public static void MapCharacterSpacing(EntryHandler handler, IEntry entry) =>
        ((AvaloniaTextBox)handler.PlatformView)?.UpdateCharacterSpacing(entry);

    public static void MapFont(EntryHandler handler, IEntry entry)
    {
        var fontManager = handler.GetRequiredService<IFontManager>();
        ((AvaloniaTextBox)handler.PlatformView)?.UpdateFont(entry, fontManager);
    }

    public static void MapHorizontalTextAlignment(EntryHandler handler, IEntry entry) =>
        ((AvaloniaTextBox)handler.PlatformView)?.UpdateHorizontalTextAlignment(entry);

    public static void MapVerticalTextAlignment(EntryHandler handler, IEntry entry) =>
        ((AvaloniaTextBox)handler.PlatformView)?.UpdateVerticalTextAlignment(entry);

    public static void MapIsPassword(EntryHandler handler, IEntry entry) =>
        ((AvaloniaTextBox)handler.PlatformView)?.UpdateIsPassword(entry);

    public static void MapIsReadOnly(EntryHandler handler, IEntry entry) =>
        ((AvaloniaTextBox)handler.PlatformView)?.UpdateIsReadOnly(entry);

    public static void MapIsTextPredictionEnabled(EntryHandler handler, IEntry entry)
    {
        if (handler.PlatformView is AvaloniaTextBox textBox)
        {
            TextInputOptions.SetShowSuggestions(textBox, entry.IsTextPredictionEnabled);
        }
    }

    public static void MapIsSpellCheckEnabled(EntryHandler handler, IEntry entry)
    {
        // Avalonia doesn't have direct spell check support
    }

    public static void MapKeyboard(EntryHandler handler, IEntry entry)
    {
        ((AvaloniaTextBox)handler.PlatformView)?.UpdateKeyboard(entry.Keyboard);
    }

    public static void MapMaxLength(EntryHandler handler, IEntry entry) =>
        ((AvaloniaTextBox)handler.PlatformView)?.UpdateMaxLength(entry);

    public static void MapPlaceholder(EntryHandler handler, IEntry entry) =>
        ((AvaloniaTextBox)handler.PlatformView)?.UpdatePlaceholder(entry);

    public static void MapPlaceholderColor(EntryHandler handler, IEntry entry)
    {
        // Avalonia TextBox doesn't directly support placeholder color customization
    }

    public static void MapReturnType(EntryHandler handler, IEntry entry)
    {
        // Return type is typically handled by platform-specific keyboard
    }

    public static void MapClearButtonVisibility(EntryHandler handler, IEntry entry) =>
        ((AvaloniaTextBox)handler.PlatformView)?.UpdateClearButtonVisibility(entry);

    public static void MapCursorPosition(EntryHandler handler, IEntry entry) =>
        ((AvaloniaTextBox)handler.PlatformView)?.UpdateCursorPosition(entry);

    public static void MapSelectionLength(EntryHandler handler, IEntry entry) =>
        ((AvaloniaTextBox)handler.PlatformView)?.UpdateSelectionLength(entry);
}

public static class EntryTextBoxExtensions
{
    public static void UpdateText(this AvaloniaTextBox textBox, IEntry entry)
    {
        if (textBox.Text != entry.Text)
            textBox.Text = entry.Text;
    }

    public static void UpdateTextColor(this AvaloniaTextBox textBox, IEntry entry)
    {
        if (entry.TextColor != null)
        {
            textBox.Foreground = entry.TextColor.ToPlatform();
        }
        else
        {
            textBox.ClearValue(AvaloniaTextBox.ForegroundProperty);
        }
    }

    public static void UpdateCharacterSpacing(this AvaloniaTextBox textBox, IEntry entry)
    {
        if (entry.CharacterSpacing != 0)
        {
            textBox.LetterSpacing = (int)(entry.CharacterSpacing * textBox.FontSize / 1000);
        }
        else
        {
            textBox.LetterSpacing = 0;
        }
    }

    public static void UpdateHorizontalTextAlignment(this AvaloniaTextBox textBox, IEntry entry)
    {
        switch (entry.HorizontalTextAlignment)
        {
            case Microsoft.Maui.TextAlignment.Start:
                textBox.TextAlignment = AvaloniaTextAlignment.Left;
                break;
            case Microsoft.Maui.TextAlignment.Center:
                textBox.TextAlignment = AvaloniaTextAlignment.Center;
                break;
            case Microsoft.Maui.TextAlignment.End:
                textBox.TextAlignment = AvaloniaTextAlignment.Right;
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    public static void UpdateVerticalTextAlignment(this AvaloniaTextBox textBox, IEntry entry)
    {
        // TODO: Vertical Text Alignment is not directly supported in Avalonia TextBox yet.
    }

    public static void UpdateIsPassword(this AvaloniaTextBox textBox, IEntry entry)
    {
        textBox.PasswordChar = entry.IsPassword ? '●' : '\0';
    }

    public static void UpdateIsReadOnly(this AvaloniaTextBox textBox, IEntry entry)
    {
        textBox.IsReadOnly = entry.IsReadOnly;
    }

    public static void UpdateMaxLength(this AvaloniaTextBox textBox, IEntry entry)
    {
        textBox.MaxLength = entry.MaxLength;
    }

    public static void UpdatePlaceholder(this AvaloniaTextBox textBox, IEntry entry)
    {
        textBox.Watermark = entry.Placeholder;
    }

    public static void UpdateClearButtonVisibility(this AvaloniaTextBox textBox, IEntry entry)
    {
    }

    public static void UpdateCursorPosition(this AvaloniaTextBox textBox, IEntry entry)
    {
        if (textBox.CaretIndex != entry.CursorPosition)
            textBox.CaretIndex = entry.CursorPosition;
    }

    public static void UpdateSelectionLength(this AvaloniaTextBox textBox, IEntry entry)
    {
        if (textBox.SelectionStart != entry.CursorPosition || textBox.SelectionEnd != entry.CursorPosition + entry.SelectionLength)
        {
            textBox.SelectionStart = entry.CursorPosition;
            textBox.SelectionEnd = entry.CursorPosition + entry.SelectionLength;
        }
    }

    internal static void UpdateBackground(this AvaloniaTextBox textBox, IView view)
    {
        if (view.Background != null)
        {
            textBox.Background = view.Background.ToPlatform();
        }
        else
        {
            textBox.ClearValue(AvaloniaTextBox.BackgroundProperty);
        }
    }
}