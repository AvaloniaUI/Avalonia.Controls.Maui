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
        else if (keyboard == Microsoft.Maui.Keyboard.Telephone)
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

        // Configure auto-capitalization based on keyboard type
        var autoCapitalize = keyboard != Microsoft.Maui.Keyboard.Plain && 
                             keyboard != Microsoft.Maui.Keyboard.Email && 
                             keyboard != Microsoft.Maui.Keyboard.Url;
        TextInputOptions.SetAutoCapitalization(textBox, autoCapitalize);

        // Configure suggestions. Chat keyboard shows suggestions; Plain keyboard does not.
        bool? showSuggestions = null;
        if (keyboard == Microsoft.Maui.Keyboard.Chat)
            showSuggestions = true;
        else if (keyboard == Microsoft.Maui.Keyboard.Plain)
            showSuggestions = false;
        
        TextInputOptions.SetShowSuggestions(textBox, showSuggestions);
    }

    /// <summary>
    /// Converts a .NET MAUI <see cref="Microsoft.Maui.Keyboard"/> to an Avalonia <see cref="TextInputContentType"/>.
    /// </summary>
    /// <param name="keyboard">The MAUI Keyboard.</param>
    /// <returns>The corresponding Avalonia TextInputContentType.</returns>
    public static TextInputContentType ToTextInputContentType(this Microsoft.Maui.Keyboard keyboard)
    {
        if (keyboard == Microsoft.Maui.Keyboard.Email)
            return TextInputContentType.Email;
        
        if (keyboard == Microsoft.Maui.Keyboard.Numeric)
            return TextInputContentType.Number;
        
        if (keyboard == Microsoft.Maui.Keyboard.Telephone)
            return TextInputContentType.Digits;
        
        if (keyboard == Microsoft.Maui.Keyboard.Url)
            return TextInputContentType.Url;
        
        // Text, Chat, Plain, and Default all map to Normal
        return TextInputContentType.Normal;
    }

    /// <summary>
    /// Updates the multiline text input option based on whether the control accepts returns.
    /// </summary>
    /// <param name="textBox">The Avalonia TextBox.</param>
    /// <param name="multiline">Whether the input is multiline.</param>
    public static void UpdateMultiline(this AvaloniaTextBox textBox, bool multiline)
    {
        TextInputOptions.SetMultiline(textBox, multiline);
    }
}
