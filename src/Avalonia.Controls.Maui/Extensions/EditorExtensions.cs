using Avalonia.Controls.Maui.Controls;
using Avalonia.Controls.Maui.Platform;
using Microsoft.Maui;
using AvaloniaTextBox = Avalonia.Controls.TextBox;
using AvaloniaTextAlignment = Avalonia.Media.TextAlignment;
using AvaloniaVerticalAlignment = Avalonia.Layout.VerticalAlignment;

namespace Avalonia.Controls.Maui.Extensions
{
    /// <summary>
    /// Extension methods for <see cref="IEditor"/> mapping.
    /// </summary>
    public static class EditorExtensions
    {
        /// <summary>
        /// Updates the text of the editor.
        /// </summary>
        /// <param name="textBox">The platform text box.</param>
        /// <param name="editor">The virtual view.</param>
        public static void UpdateEditorText(this AvaloniaTextBox textBox, IEditor editor)
        {
            var text = editor.Text;

            if (editor is Microsoft.Maui.Controls.Editor mauiEditor)
            {
                text = TextTransformUtilities.GetTransformedText(editor.Text, mauiEditor.TextTransform);
            }

            if (textBox.Text != text)
                textBox.Text = text;
        }

        /// <summary>
        /// Updates the text color of the editor.
        /// </summary>
        /// <param name="textBox">The platform text box.</param>
        /// <param name="editor">The virtual view.</param>
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

        /// <summary>
        /// Updates the character spacing of the editor.
        /// </summary>
        /// <param name="textBox">The platform text box.</param>
        /// <param name="editor">The virtual view.</param>
        public static void UpdateEditorCharacterSpacing(this AvaloniaTextBox textBox, IEditor editor)
        {
            if (editor.CharacterSpacing != 0)
            {
                textBox.LetterSpacing = editor.CharacterSpacing;
            }
            else
            {
                textBox.LetterSpacing = 0;
            }
        }

        /// <summary>
        /// Updates the font of the editor.
        /// </summary>
        /// <param name="textBox">The platform text box.</param>
        /// <param name="editor">The virtual view.</param>
        /// <param name="fontManager">The font manager.</param>
        public static void UpdateEditorFont(this AvaloniaTextBox textBox, IEditor editor, IFontManager fontManager)
        {
            textBox.UpdateFont(editor, fontManager);
        }

        /// <summary>
        /// Updates the horizontal text alignment of the editor.
        /// </summary>
        /// <param name="textBox">The platform text box.</param>
        /// <param name="editor">The virtual view.</param>
        public static void UpdateEditorHorizontalTextAlignment(this AvaloniaTextBox textBox, IEditor editor)
        {
            switch (editor.HorizontalTextAlignment)
            {
                case TextAlignment.Start:
                    textBox.TextAlignment = AvaloniaTextAlignment.Left;
                    break;
                case TextAlignment.Center:
                    textBox.TextAlignment = AvaloniaTextAlignment.Center;
                    break;
                case TextAlignment.End:
                    textBox.TextAlignment = AvaloniaTextAlignment.Right;
                    break;
            }
        }

        /// <summary>
        /// Updates the vertical text alignment of the editor.
        /// </summary>
        /// <param name="textBox">The platform text box.</param>
        /// <param name="editor">The virtual view.</param>
        public static void UpdateEditorVerticalTextAlignment(this AvaloniaTextBox textBox, IEditor editor)
        {
            switch (editor.VerticalTextAlignment)
            {
                case TextAlignment.Start:
                    textBox.VerticalContentAlignment = AvaloniaVerticalAlignment.Top;
                    break;
                case TextAlignment.Center:
                    textBox.VerticalContentAlignment = AvaloniaVerticalAlignment.Center;
                    break;
                case TextAlignment.End:
                    textBox.VerticalContentAlignment = AvaloniaVerticalAlignment.Bottom;
                    break;
            }
        }

        /// <summary>
        /// Updates the IsReadOnly property of the editor.
        /// </summary>
        /// <param name="textBox">The platform text box.</param>
        /// <param name="editor">The virtual view.</param>
        public static void UpdateEditorIsReadOnly(this AvaloniaTextBox textBox, IEditor editor)
        {
            textBox.IsReadOnly = editor.IsReadOnly;
        }

        /// <summary>
        /// Updates the MaxLength property of the editor.
        /// </summary>
        /// <param name="textBox">The platform text box.</param>
        /// <param name="editor">The virtual view.</param>
        public static void UpdateEditorMaxLength(this AvaloniaTextBox textBox, IEditor editor)
        {
            textBox.MaxLength = editor.MaxLength;
        }

        /// <summary>
        /// Updates the placeholder text of the editor.
        /// </summary>
        /// <param name="textBox">The platform text box.</param>
        /// <param name="editor">The virtual view.</param>
        public static void UpdateEditorPlaceholder(this AvaloniaTextBox textBox, IEditor editor)
        {
            textBox.Watermark = editor.Placeholder;
        }

        /// <summary>
        /// Updates the placeholder color of the editor.
        /// </summary>
        /// <param name="textBox">The platform text box.</param>
        /// <param name="editor">The virtual view.</param>
        public static void UpdateEditorPlaceholderColor(this AvaloniaTextBox textBox, IEditor editor)
        {
            if (textBox is MauiEditor mauiEditor)
            {
                if (editor.PlaceholderColor != null)
                {
                    mauiEditor.PlaceholderForeground = editor.PlaceholderColor.ToPlatform();
                }
                else
                {
                    mauiEditor.ClearValue(MauiEditor.PlaceholderForegroundProperty);
                }
            }
        }

        /// <summary>
        /// Updates the cursor position of the editor.
        /// </summary>
        /// <param name="textBox">The platform text box.</param>
        /// <param name="editor">The virtual view.</param>
        public static void UpdateEditorCursorPosition(this AvaloniaTextBox textBox, IEditor editor)
        {
            if (textBox.CaretIndex != editor.CursorPosition)
                textBox.CaretIndex = editor.CursorPosition;
        }

        /// <summary>
        /// Updates the selection length of the editor.
        /// </summary>
        /// <param name="textBox">The platform text box.</param>
        /// <param name="editor">The virtual view.</param>
        public static void UpdateEditorSelectionLength(this AvaloniaTextBox textBox, IEditor editor)
        {
            if (textBox.SelectionStart != editor.CursorPosition || textBox.SelectionEnd != editor.CursorPosition + editor.SelectionLength)
            {
                textBox.SelectionStart = editor.CursorPosition;
                textBox.SelectionEnd = editor.CursorPosition + editor.SelectionLength;
            }
        }
        
        /// <summary>
        /// Updates the text prediction property of the editor.
        /// </summary>
        /// <param name="textBox">The platform text box.</param>
        /// <param name="editor">The virtual view.</param>
        [NotImplemented("Avalonia TextBox does not currently support disabling text prediction.")]
        public static void UpdateEditorIsTextPredictionEnabled(this AvaloniaTextBox textBox, IEditor editor)
        {
             // Not supported
        }
        
        /// <summary>
        /// Updates the spell check property of the editor.
        /// </summary>
        /// <param name="textBox">The platform text box.</param>
        /// <param name="editor">The virtual view.</param>
        [NotImplemented("Avalonia TextBox does not currently support disabling spell check.")]
        public static void UpdateEditorIsSpellCheckEnabled(this AvaloniaTextBox textBox, IEditor editor)
        {
             // Not supported
        }
        
        /// <summary>
        /// Updates the keyboard property of the editor.
        /// </summary>
        /// <param name="textBox">The platform text box.</param>
        /// <param name="editor">The virtual view.</param>
        [NotImplemented("Custom keyboard mapping is not yet implemented for Avalonia Editor.")]
        public static void UpdateEditorKeyboard(this AvaloniaTextBox textBox, IEditor editor)
        {
             // Not supported
        }

        /// <summary>
        /// Updates the text transform of the editor.
        /// </summary>
        /// <param name="textBox">The platform text box.</param>
        /// <param name="editor">The virtual view.</param>
        public static void UpdateEditorTextTransform(this AvaloniaTextBox textBox, IEditor editor)
        {
             textBox.UpdateEditorText(editor);
        }

        /// <summary>
        /// Updates the auto-size behavior of the editor.
        /// </summary>
        /// <param name="textBox">The platform text box.</param>
        /// <param name="editor">The virtual view.</param>
        public static void UpdateEditorAutoSize(this AvaloniaTextBox textBox, IEditor editor)
        {
             if (editor is Microsoft.Maui.Controls.Editor mauiEditor)
             {
                 switch (mauiEditor.AutoSize)
                 {
                     case Microsoft.Maui.Controls.EditorAutoSizeOption.Disabled:
                         // Fixed height, respect HeightRequest if set
                         textBox.MinHeight = 0;
                         textBox.MaxHeight = double.PositiveInfinity;
                         break;
                     case Microsoft.Maui.Controls.EditorAutoSizeOption.TextChanges:
                         // Auto-expand based on text content
                         textBox.MinHeight = 40; // Minimum reasonable height
                         textBox.MaxHeight = double.PositiveInfinity;
                         break;
                 }
             }
        }
    }
}
