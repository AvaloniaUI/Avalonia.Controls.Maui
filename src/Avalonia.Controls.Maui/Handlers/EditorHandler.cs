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

public class EditorHandler : ViewHandler<IEditor, AvaloniaTextBox>, IEditorHandler
{
    public static IPropertyMapper<IEditor, IEditorHandler> Mapper = new PropertyMapper<IEditor, IEditorHandler>(ViewHandler.ViewMapper)
    {
        [nameof(IEditor.Background)] = MapBackground,
        [nameof(IEditor.CharacterSpacing)] = MapCharacterSpacing,
        [nameof(IEditor.Font)] = MapFont,
        [nameof(IEditor.IsReadOnly)] = MapIsReadOnly,
        [nameof(IEditor.IsTextPredictionEnabled)] = MapIsTextPredictionEnabled,
        [nameof(IEditor.IsSpellCheckEnabled)] = MapIsSpellCheckEnabled,
        [nameof(IEditor.MaxLength)] = MapMaxLength,
        [nameof(IEditor.Placeholder)] = MapPlaceholder,
        [nameof(IEditor.PlaceholderColor)] = MapPlaceholderColor,
        [nameof(IEditor.Text)] = MapText,
        [nameof(IEditor.TextColor)] = MapTextColor,
        [nameof(IEditor.HorizontalTextAlignment)] = MapHorizontalTextAlignment,
        [nameof(IEditor.VerticalTextAlignment)] = MapVerticalTextAlignment,
        [nameof(IEditor.Keyboard)] = MapKeyboard,
        [nameof(IEditor.CursorPosition)] = MapCursorPosition,
        [nameof(IEditor.SelectionLength)] = MapSelectionLength
    };

    public static CommandMapper<IEditor, IEditorHandler> CommandMapper = new(ViewCommandMapper)
    {
    };

    public EditorHandler() : base(Mapper, CommandMapper)
    {
    }

    public EditorHandler(IPropertyMapper? mapper)
        : base(mapper ?? Mapper, CommandMapper)
    {
    }

    public EditorHandler(IPropertyMapper mapper, CommandMapper? commandMapper = null)
        : base(mapper, commandMapper)
    {
    }

    IEditor IEditorHandler.VirtualView => VirtualView;

    System.Object IEditorHandler.PlatformView => PlatformView;

    protected override AvaloniaTextBox CreatePlatformView()
    {
        return new AvaloniaTextBox
        {
            AcceptsReturn = true,
            TextWrapping = TextWrapping.Wrap
        };
    }

    public override bool NeedsContainer => false;

    public static void MapBackground(IEditorHandler handler, IEditor editor)
    {
        handler.UpdateValue(nameof(IViewHandler.ContainerView));
        ((AvaloniaTextBox)handler.PlatformView)?.UpdateBackground(editor);
    }

    public static void MapText(IEditorHandler handler, IEditor editor) =>
        ((AvaloniaTextBox)handler.PlatformView)?.UpdateEditorText(editor);

    public static void MapTextColor(IEditorHandler handler, IEditor editor) =>
        ((AvaloniaTextBox)handler.PlatformView)?.UpdateEditorTextColor(editor);

    public static void MapCharacterSpacing(IEditorHandler handler, IEditor editor) =>
        ((AvaloniaTextBox)handler.PlatformView)?.UpdateEditorCharacterSpacing(editor);

    public static void MapFont(IEditorHandler handler, IEditor editor)
    {
        var fontManager = handler.GetRequiredService<IFontManager>();
        ((AvaloniaTextBox)handler.PlatformView)?.UpdateFont(editor, fontManager);
    }

    public static void MapHorizontalTextAlignment(IEditorHandler handler, IEditor editor) =>
        ((AvaloniaTextBox)handler.PlatformView)?.UpdateEditorHorizontalTextAlignment(editor);

    public static void MapVerticalTextAlignment(IEditorHandler handler, IEditor editor) =>
        ((AvaloniaTextBox)handler.PlatformView)?.UpdateEditorVerticalTextAlignment(editor);

    public static void MapIsReadOnly(IEditorHandler handler, IEditor editor) =>
        ((AvaloniaTextBox)handler.PlatformView)?.UpdateEditorIsReadOnly(editor);

    public static void MapIsTextPredictionEnabled(IEditorHandler handler, IEditor editor)
    {
        // Avalonia doesn't have direct text prediction support
    }

    public static void MapIsSpellCheckEnabled(IEditorHandler handler, IEditor editor)
    {
        // Avalonia doesn't have direct spell check support
    }

    public static void MapKeyboard(IEditorHandler handler, IEditor editor)
    {
        // Keyboard handling is platform-specific and not directly applicable to Avalonia
    }

    public static void MapMaxLength(IEditorHandler handler, IEditor editor) =>
        ((AvaloniaTextBox)handler.PlatformView)?.UpdateEditorMaxLength(editor);

    public static void MapPlaceholder(IEditorHandler handler, IEditor editor) =>
        ((AvaloniaTextBox)handler.PlatformView)?.UpdateEditorPlaceholder(editor);

    public static void MapPlaceholderColor(IEditorHandler handler, IEditor editor)
    {
        // Avalonia TextBox doesn't directly support placeholder color customization
    }

    public static void MapCursorPosition(IEditorHandler handler, IEditor editor) =>
        ((AvaloniaTextBox)handler.PlatformView)?.UpdateEditorCursorPosition(editor);

    public static void MapSelectionLength(IEditorHandler handler, IEditor editor) =>
        ((AvaloniaTextBox)handler.PlatformView)?.UpdateEditorSelectionLength(editor);
}

public static class EditorTextBoxExtensions
{
    public static void UpdateEditorText(this AvaloniaTextBox textBox, IEditor editor)
    {
        if (textBox.Text != editor.Text)
            textBox.Text = editor.Text;
    }

    public static void UpdateEditorTextColor(this AvaloniaTextBox textBox, IEditor editor)
    {
        if (editor.TextColor != null)
        {
            textBox.Foreground = editor.TextColor.ToPlatform();
        }
        else
        {
            textBox.ClearValue(AvaloniaTextBox.ForegroundProperty);
        }
    }

    public static void UpdateEditorCharacterSpacing(this AvaloniaTextBox textBox, IEditor editor)
    {
        if (editor.CharacterSpacing != 0)
        {
            textBox.LetterSpacing = (int)(editor.CharacterSpacing * textBox.FontSize / 1000);
        }
        else
        {
            textBox.LetterSpacing = 0;
        }
    }

    public static void UpdateEditorHorizontalTextAlignment(this AvaloniaTextBox textBox, IEditor editor)
    {
        switch (editor.HorizontalTextAlignment)
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

    public static void UpdateEditorVerticalTextAlignment(this AvaloniaTextBox textBox, IEditor editor)
    {
        switch (editor.VerticalTextAlignment)
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

    public static void UpdateEditorIsReadOnly(this AvaloniaTextBox textBox, IEditor editor)
    {
        textBox.IsReadOnly = editor.IsReadOnly;
    }

    public static void UpdateEditorMaxLength(this AvaloniaTextBox textBox, IEditor editor)
    {
        textBox.MaxLength = editor.MaxLength;
    }

    public static void UpdateEditorPlaceholder(this AvaloniaTextBox textBox, IEditor editor)
    {
        textBox.Watermark = editor.Placeholder;
    }

    public static void UpdateEditorCursorPosition(this AvaloniaTextBox textBox, IEditor editor)
    {
        if (textBox.CaretIndex != editor.CursorPosition)
            textBox.CaretIndex = editor.CursorPosition;
    }

    public static void UpdateEditorSelectionLength(this AvaloniaTextBox textBox, IEditor editor)
    {
        if (textBox.SelectionStart != editor.CursorPosition || textBox.SelectionEnd != editor.CursorPosition + editor.SelectionLength)
        {
            textBox.SelectionStart = editor.CursorPosition;
            textBox.SelectionEnd = editor.CursorPosition + editor.SelectionLength;
        }
    }
}