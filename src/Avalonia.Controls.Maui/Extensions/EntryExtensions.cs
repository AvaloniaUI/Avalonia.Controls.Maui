using Avalonia.Controls.Maui.Controls;
using Avalonia.Controls.Maui.Platform;
using Microsoft.Maui;
using AvaloniaTextBox = Avalonia.Controls.TextBox;
using AvaloniaTextAlignment = Avalonia.Media.TextAlignment;
using AvaloniaVerticalAlignment = Avalonia.Layout.VerticalAlignment;
using Avalonia.Input.TextInput;

namespace Avalonia.Controls.Maui.Extensions;

/// <summary>
/// Extension methods for <see cref="Avalonia.Controls.TextBox"/> to support <see cref="IEntry"/> properties.
/// </summary>
public static class EntryExtensions
{
    /// <summary>
    /// Updates the text of the TextBox from the Entry.
    /// </summary>
    /// <param name="textBox">The Avalonia TextBox.</param>
    /// <param name="entry">The .NET MAUI Entry.</param>
    public static void UpdateText(this AvaloniaTextBox textBox, IEntry entry)
    {
        var text = entry.Text;

        if (entry is Microsoft.Maui.Controls.Entry mauiEntry)
        {
             text = TextTransformUtilities.GetTransformedText(entry.Text, mauiEntry.TextTransform);
        }
        else
        {
#pragma warning disable IL2075
             var prop = entry.GetType().GetProperty("TextTransform");
#pragma warning restore IL2075
             if (prop != null && prop.PropertyType == typeof(TextTransform))
             {
                 var transform = (TextTransform)prop.GetValue(entry)!;
                 text = TextTransformUtilities.GetTransformedText(entry.Text, transform);
             }
        }

        if (textBox.Text != text)
            textBox.Text = text;
    }

    /// <summary>
    /// Updates the text color of the TextBox from the Entry.
    /// </summary>
    /// <param name="textBox">The Avalonia TextBox.</param>
    /// <param name="entry">The .NET MAUI Entry.</param>
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

    /// <summary>
    /// Updates the character spacing of the TextBox from the Entry.
    /// </summary>
    /// <param name="textBox">The Avalonia TextBox.</param>
    /// <param name="entry">The .NET MAUI Entry.</param>
    public static void UpdateCharacterSpacing(this AvaloniaTextBox textBox, IEntry entry)
    {
        textBox.LetterSpacing = entry.CharacterSpacing;
    }

    /// <summary>
    /// Updates the horizontal text alignment of the TextBox from the Entry.
    /// </summary>
    /// <param name="textBox">The Avalonia TextBox.</param>
    /// <param name="entry">The .NET MAUI Entry.</param>
    public static void UpdateHorizontalTextAlignment(this AvaloniaTextBox textBox, IEntry entry)
    {
        textBox.TextAlignment = entry.HorizontalTextAlignment switch
        {
            TextAlignment.Start => AvaloniaTextAlignment.Left,
            TextAlignment.Center => AvaloniaTextAlignment.Center,
            TextAlignment.End => AvaloniaTextAlignment.Right,
            _ => AvaloniaTextAlignment.Left
        };
    }

    /// <summary>
    /// Updates the vertical text alignment of the TextBox from the Entry.
    /// </summary>
    /// <param name="textBox">The Avalonia TextBox.</param>
    /// <param name="entry">The .NET MAUI Entry.</param>
    public static void UpdateVerticalTextAlignment(this AvaloniaTextBox textBox, IEntry entry)
    {
        textBox.VerticalContentAlignment = entry.VerticalTextAlignment switch
        {
            TextAlignment.Start => AvaloniaVerticalAlignment.Top,
            TextAlignment.Center => AvaloniaVerticalAlignment.Center,
            TextAlignment.End => AvaloniaVerticalAlignment.Bottom,
            _ => AvaloniaVerticalAlignment.Top
        };
    }

    /// <summary>
    /// Updates whether the TextBox displays password characters.
    /// </summary>
    /// <param name="textBox">The Avalonia TextBox.</param>
    /// <param name="entry">The .NET MAUI Entry.</param>
    public static void UpdateIsPassword(this AvaloniaTextBox textBox, IEntry entry)
    {
        textBox.PasswordChar = entry.IsPassword ? '●' : '\0';
    }

    /// <summary>
    /// Updates whether the TextBox is read-only.
    /// </summary>
    /// <param name="textBox">The Avalonia TextBox.</param>
    /// <param name="entry">The .NET MAUI Entry.</param>
    public static void UpdateIsReadOnly(this AvaloniaTextBox textBox, IEntry entry)
    {
        textBox.IsReadOnly = entry.IsReadOnly;
    }

    /// <summary>
    /// Updates the maximum length of text allowed in the TextBox.
    /// </summary>
    /// <param name="textBox">The Avalonia TextBox.</param>
    /// <param name="entry">The .NET MAUI Entry.</param>
    public static void UpdateMaxLength(this AvaloniaTextBox textBox, IEntry entry)
    {
        textBox.MaxLength = entry.MaxLength;
    }

    /// <summary>
    /// Updates the placeholder (watermark) text of the TextBox.
    /// </summary>
    /// <param name="textBox">The Avalonia TextBox.</param>
    /// <param name="entry">The .NET MAUI Entry.</param>
    public static void UpdatePlaceholder(this AvaloniaTextBox textBox, IEntry entry)
    {
        textBox.Watermark = entry.Placeholder;
    }

    /// <summary>
    /// Updates the clear button visibility of the TextBox.
    /// </summary>
    /// <param name="textBox">The Avalonia TextBox.</param>
    /// <param name="entry">The .NET MAUI Entry.</param>
    /// <remarks>
    /// Avalonia TextBox does not have built-in clear button support.
    /// This method is provided for API compatibility.
    /// </remarks>
    public static void UpdateClearButtonVisibility(this AvaloniaTextBox textBox, IEntry entry)
    {
        if (textBox is MauiEntry mauiEntry)
        {
            mauiEntry.ClearButtonVisibility = entry.ClearButtonVisibility;
        }
    }

    /// <summary>
    /// Updates the cursor position in the TextBox.
    /// </summary>
    /// <param name="textBox">The Avalonia TextBox.</param>
    /// <param name="entry">The .NET MAUI Entry.</param>
    public static void UpdateCursorPosition(this AvaloniaTextBox textBox, IEntry entry)
    {
        if (entry.SelectionLength > 0)
            return;

        if (textBox.CaretIndex != entry.CursorPosition)
            textBox.CaretIndex = entry.CursorPosition;
    }

    /// <summary>
    /// Updates the text selection length in the TextBox.
    /// </summary>
    /// <param name="textBox">The Avalonia TextBox.</param>
    /// <param name="entry">The .NET MAUI Entry.</param>
    public static void UpdateSelectionLength(this AvaloniaTextBox textBox, IEntry entry)
    {
        if (textBox.SelectionStart != entry.CursorPosition || textBox.SelectionEnd != entry.CursorPosition + entry.SelectionLength)
        {
            textBox.SelectionStart = entry.CursorPosition;
            textBox.SelectionEnd = entry.CursorPosition + entry.SelectionLength;
        }
    }

    /// <summary>
    /// Updates the background of the TextBox from the view.
    /// </summary>
    /// <param name="textBox">The Avalonia TextBox.</param>
    /// <param name="view">The .NET MAUI view.</param>
    public static void UpdateBackground(this AvaloniaTextBox textBox, IView view)
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

    /// <summary>
    /// Updates the spell check property of the entry.
    /// </summary>
    /// <param name="textBox">The platform text box.</param>
    /// <param name="entry">The virtual view.</param>
    [NotImplemented("Avalonia TextBox does not currently support disabling spell check.")]
    public static void UpdateIsSpellCheckEnabled(this AvaloniaTextBox textBox, IEntry entry)
    {
        // Not supported
    }

    /// <summary>
    /// Updates the placeholder color of the entry.
    /// </summary>
    /// <param name="textBox">The platform text box.</param>
    /// <param name="entry">The virtual view.</param>
    public static void UpdatePlaceholderColor(this AvaloniaTextBox textBox, IEntry entry)
    {
        if (textBox is MauiEntry mauiEntry)
        {
            if (entry.PlaceholderColor != null)
            {
                mauiEntry.PlaceholderForeground = entry.PlaceholderColor.ToPlatform();
            }
            else
            {
                mauiEntry.ClearValue(MauiEntry.PlaceholderForegroundProperty);
            }
        }
    }

    /// <summary>
    /// Updates the text transform of the entry.
    /// </summary>
    /// <param name="textBox">The platform text box.</param>
    /// <param name="entry">The virtual view.</param>
    public static void UpdateTextTransform(this AvaloniaTextBox textBox, IEntry entry)
    {
         textBox.UpdateText(entry);
    }

    /// <summary>
    /// Updates the return type of the entry.
    /// </summary>
    /// <param name="textBox">The platform text box.</param>
    /// <param name="entry">The virtual view.</param>
    public static void UpdateReturnType(this AvaloniaTextBox textBox, IEntry entry)
    {
        var returnKeyType = entry.ReturnType switch
        {
            ReturnType.Go => TextInputReturnKeyType.Go,
            ReturnType.Next => TextInputReturnKeyType.Next,
            ReturnType.Search => TextInputReturnKeyType.Search,
            ReturnType.Send => TextInputReturnKeyType.Send,
            ReturnType.Done => TextInputReturnKeyType.Done,
            _ => TextInputReturnKeyType.Default
        };
        TextInputOptions.SetReturnKeyType(textBox, returnKeyType);
    }
}