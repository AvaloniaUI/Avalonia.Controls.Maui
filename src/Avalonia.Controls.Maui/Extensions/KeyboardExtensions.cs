using Avalonia.Input.TextInput;
using AvaloniaTextBox = Avalonia.Controls.TextBox;

namespace Avalonia.Controls.Maui.Extensions;

/// <summary>
/// Extension methods for Avalonia TextBox to support .NET MAUI Keyboard.
/// </summary>
public static class KeyboardExtensions
{
    /// <summary>
    /// Updates the keyboard type of the TextBox from the Entry.
    /// </summary>
    /// <param name="textBox">The Avalonia TextBox.</param>
    /// <param name="keyboard">The .NET MAUI Keyboard.</param>
    public static void UpdateKeyboard(this AvaloniaTextBox textBox, Microsoft.Maui.Keyboard keyboard)
    {
        var contentType = TextInputContentType.Normal;

        if (keyboard == Microsoft.Maui.Keyboard.Email)
        {
            contentType = TextInputContentType.Email;
        }
        else if (keyboard == Microsoft.Maui.Keyboard.Text)
        {
            contentType = TextInputContentType.Normal;
        }
        else if (keyboard == Microsoft.Maui.Keyboard.Url)
        {
            contentType = TextInputContentType.Url;
        }
         if (keyboard == Microsoft.Maui.Keyboard.Telephone)
        {
            contentType = TextInputContentType.Digits;
        }
        else if (keyboard == Microsoft.Maui.Keyboard.Numeric)
        {
            contentType = TextInputContentType.Number;
        }
        else if (keyboard == Microsoft.Maui.Keyboard.Chat)
        {
            contentType = TextInputContentType.Normal;
        }

        // Set the content type hint
        TextInputOptions.SetContentType(textBox, contentType);
    }
}
