namespace Avalonia.Controls.Maui;

/// <summary>
/// Provides extension methods for converting .NET MAUI font types to their Avalonia equivalents.
/// </summary>
public static class FontExtensions
{
    /// <summary>
    /// Converts a .NET MAUI <see cref="Microsoft.Maui.FontWeight"/> to an Avalonia <see cref="Avalonia.Media.FontWeight"/>.
    /// </summary>
    /// <param name="weight">The .NET MAUI font weight to convert.</param>
    /// <returns>The corresponding Avalonia <see cref="Avalonia.Media.FontWeight"/> value.</returns>
    public static Avalonia.Media.FontWeight ToAvalonia(this Microsoft.Maui.FontWeight weight) => weight switch
    {
        Microsoft.Maui.FontWeight.Thin => Avalonia.Media.FontWeight.Thin,
        Microsoft.Maui.FontWeight.Light => Avalonia.Media.FontWeight.Light,
        Microsoft.Maui.FontWeight.Medium => Avalonia.Media.FontWeight.Medium,
        Microsoft.Maui.FontWeight.Bold => Avalonia.Media.FontWeight.Bold,
        Microsoft.Maui.FontWeight.Black => Avalonia.Media.FontWeight.Black,
        _ => Avalonia.Media.FontWeight.Normal
    };

    /// <summary>
    /// Converts .NET MAUI <see cref="Microsoft.Maui.Controls.FontAttributes"/> to an Avalonia <see cref="Avalonia.Media.FontStyle"/>.
    /// </summary>
    /// <param name="fontAttributes">The .NET MAUI font attributes to convert.</param>
    /// <returns><see cref="Avalonia.Media.FontStyle.Italic"/> if the italic flag is set; otherwise, <see cref="Avalonia.Media.FontStyle.Normal"/>.</returns>
    public static global::Avalonia.Media.FontStyle ToPlatformFontStyle(this Microsoft.Maui.Controls.FontAttributes fontAttributes)
    {
        if (fontAttributes.HasFlag(Microsoft.Maui.Controls.FontAttributes.Italic))
            return global::Avalonia.Media.FontStyle.Italic;
        return global::Avalonia.Media.FontStyle.Normal;
    }
}