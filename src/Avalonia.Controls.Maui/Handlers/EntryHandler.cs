using Avalonia.Controls;
using Avalonia.Controls.Maui.Platform;
using Avalonia.Media;
using Microsoft.Maui;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Handlers;
using System;
using AvaloniaTextBox = Avalonia.Controls.TextBox;
using AvaloniaTextAlignment = Avalonia.Media.TextAlignment;
using AvaloniaVerticalAlignment = Avalonia.Layout.VerticalAlignment;

namespace Avalonia.Controls.Maui.Handlers;

public class EntryHandler : ViewHandler<IEntry, AvaloniaTextBox>, IEntryHandler
{
    public static IPropertyMapper<IEntry, IEntryHandler> Mapper = new PropertyMapper<IEntry, IEntryHandler>(ViewHandler.ViewMapper)
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

    public static CommandMapper<IEntry, IEntryHandler> CommandMapper = new(ViewCommandMapper)
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

    IEntry IEntryHandler.VirtualView => VirtualView;

    System.Object IEntryHandler.PlatformView => PlatformView;

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

    public static void MapBackground(IEntryHandler handler, IEntry entry)
    {
        handler.UpdateValue(nameof(IViewHandler.ContainerView));
        ((AvaloniaTextBox)handler.PlatformView)?.UpdateBackground(entry);
    }

    public static void MapText(IEntryHandler handler, IEntry entry) =>
        ((AvaloniaTextBox)handler.PlatformView)?.UpdateText(entry);

    public static void MapTextColor(IEntryHandler handler, IEntry entry) =>
        ((AvaloniaTextBox)handler.PlatformView)?.UpdateTextColor(entry);

    public static void MapCharacterSpacing(IEntryHandler handler, IEntry entry) =>
        ((AvaloniaTextBox)handler.PlatformView)?.UpdateCharacterSpacing(entry);

    public static void MapFont(IEntryHandler handler, IEntry entry)
    {
        var fontManager = handler.GetRequiredService<IFontManager>();
        ((AvaloniaTextBox)handler.PlatformView)?.UpdateFont(entry, fontManager);
    }

    public static void MapHorizontalTextAlignment(IEntryHandler handler, IEntry entry) =>
        ((AvaloniaTextBox)handler.PlatformView)?.UpdateHorizontalTextAlignment(entry);

    public static void MapVerticalTextAlignment(IEntryHandler handler, IEntry entry) =>
        ((AvaloniaTextBox)handler.PlatformView)?.UpdateVerticalTextAlignment(entry);

    public static void MapIsPassword(IEntryHandler handler, IEntry entry) =>
        ((AvaloniaTextBox)handler.PlatformView)?.UpdateIsPassword(entry);

    public static void MapIsReadOnly(IEntryHandler handler, IEntry entry) =>
        ((AvaloniaTextBox)handler.PlatformView)?.UpdateIsReadOnly(entry);

    public static void MapIsTextPredictionEnabled(IEntryHandler handler, IEntry entry)
    {
        // Avalonia doesn't have direct text prediction support
    }

    public static void MapIsSpellCheckEnabled(IEntryHandler handler, IEntry entry)
    {
        // Avalonia doesn't have direct spell check support
    }

    public static void MapKeyboard(IEntryHandler handler, IEntry entry)
    {
        // Keyboard handling is platform-specific and not directly applicable to Avalonia
    }

    public static void MapMaxLength(IEntryHandler handler, IEntry entry) =>
        ((AvaloniaTextBox)handler.PlatformView)?.UpdateMaxLength(entry);

    public static void MapPlaceholder(IEntryHandler handler, IEntry entry) =>
        ((AvaloniaTextBox)handler.PlatformView)?.UpdatePlaceholder(entry);

    public static void MapPlaceholderColor(IEntryHandler handler, IEntry entry)
    {
        // Avalonia TextBox doesn't directly support placeholder color customization
    }

    public static void MapReturnType(IEntryHandler handler, IEntry entry)
    {
        // Return type is typically handled by platform-specific keyboard
    }

    public static void MapClearButtonVisibility(IEntryHandler handler, IEntry entry) =>
        ((AvaloniaTextBox)handler.PlatformView)?.UpdateClearButtonVisibility(entry);

    public static void MapCursorPosition(IEntryHandler handler, IEntry entry) =>
        ((AvaloniaTextBox)handler.PlatformView)?.UpdateCursorPosition(entry);

    public static void MapSelectionLength(IEntryHandler handler, IEntry entry) =>
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
        switch (entry.VerticalTextAlignment)
        {
            case Microsoft.Maui.TextAlignment.Start:
                textBox.VerticalContentAlignment = AvaloniaVerticalAlignment.Top;
                break;
            case Microsoft.Maui.TextAlignment.Center:
                textBox.VerticalContentAlignment = AvaloniaVerticalAlignment.Center;
                break;
            case Microsoft.Maui.TextAlignment.End:
                textBox.VerticalContentAlignment = AvaloniaVerticalAlignment.Bottom;
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
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