using Avalonia.Input.TextInput;
using Microsoft.Maui;
using AvaloniaTextBox = Avalonia.Controls.TextBox;

namespace Avalonia.Controls.Maui.Extensions;

/// <summary>
/// Extension methods for mapping .NET MAUI <see cref="Keyboard"/> to Avalonia's <see cref="TextInputOptions"/>.
/// </summary>
public static class KeyboardExtensions
{
    /// <summary>
    /// Updates the keyboard/input method options on the TextBox based on the MAUI Keyboard type.
    /// </summary>
    /// <param name="textBox">The Avalonia TextBox.</param>
    /// <param name="keyboard">The .NET MAUI Keyboard type.</param>
    public static void UpdateKeyboard(this AvaloniaTextBox textBox, Keyboard? keyboard)
    {
        if (keyboard == null)
        {
            // Reset to defaults
            TextInputOptions.SetContentType(textBox, TextInputContentType.Normal);
            TextInputOptions.SetAutoCapitalization(textBox, true);
            TextInputOptions.SetShowSuggestions(textBox, null);
            return;
        }

        var contentType = keyboard.ToTextInputContentType();
        TextInputOptions.SetContentType(textBox, contentType);

        // Configure auto-capitalization based on keyboard type
        var autoCapitalize = keyboard != Keyboard.Plain && 
                             keyboard != Keyboard.Email && 
                             keyboard != Keyboard.Url;
        TextInputOptions.SetAutoCapitalization(textBox, autoCapitalize);

        // Configure suggestions. Chat keyboard should show suggestions, Plain should not
        bool? showSuggestions = null;
        if (keyboard == Keyboard.Chat)
            showSuggestions = true;
        else if (keyboard == Keyboard.Plain)
            showSuggestions = false;
        
        TextInputOptions.SetShowSuggestions(textBox, showSuggestions);
    }

    /// <summary>
    /// Converts a .NET MAUI <see cref="Keyboard"/> to an Avalonia <see cref="TextInputContentType"/>.
    /// </summary>
    /// <param name="keyboard">The MAUI Keyboard.</param>
    /// <returns>The corresponding Avalonia TextInputContentType.</returns>
    public static TextInputContentType ToTextInputContentType(this Keyboard keyboard)
    {
        if (keyboard == Keyboard.Email)
            return TextInputContentType.Email;
        
        if (keyboard == Keyboard.Numeric)
            return TextInputContentType.Number;
        
        if (keyboard == Keyboard.Telephone)
            return TextInputContentType.Digits;
        
        if (keyboard == Keyboard.Url)
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
