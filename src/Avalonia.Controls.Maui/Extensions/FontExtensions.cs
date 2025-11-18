namespace Avalonia.Controls.Maui;

public static class FontExtensions
{
    public static Avalonia.Media.FontWeight ToAvalonia(this Microsoft.Maui.FontWeight weight) => weight switch
    {
        Microsoft.Maui.FontWeight.Thin => Avalonia.Media.FontWeight.Thin,
        Microsoft.Maui.FontWeight.Light => Avalonia.Media.FontWeight.Light,
        Microsoft.Maui.FontWeight.Medium => Avalonia.Media.FontWeight.Medium,
        Microsoft.Maui.FontWeight.Bold => Avalonia.Media.FontWeight.Bold,
        Microsoft.Maui.FontWeight.Black => Avalonia.Media.FontWeight.Black,
        _ => Avalonia.Media.FontWeight.Normal
    };

    public static global::Avalonia.Media.FontStyle ToPlatformFontStyle(this Microsoft.Maui.Controls.FontAttributes fontAttributes)
    {
        if (fontAttributes.HasFlag(Microsoft.Maui.Controls.FontAttributes.Italic))
            return global::Avalonia.Media.FontStyle.Italic;
        return global::Avalonia.Media.FontStyle.Normal;
    }
}