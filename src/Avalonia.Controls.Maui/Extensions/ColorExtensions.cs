using Avalonia.Media;

namespace Avalonia.Controls.Maui.Extensions;

/// <summary>
/// Provides extension methods for converting .NET MAUI <see cref="Microsoft.Maui.Graphics.Color"/> values to Avalonia color and brush types.
/// </summary>
public static class ColorExtensions
{
    /// <summary>
    /// Converts a nullable .NET MAUI <see cref="Microsoft.Maui.Graphics.Color"/> to an Avalonia <see cref="IBrush"/>.
    /// </summary>
    /// <param name="color">The .NET MAUI color to convert, or <c>null</c>.</param>
    /// <returns>A <see cref="SolidColorBrush"/> representing the color, or <c>null</c> if the input is <c>null</c>.</returns>
    public static IBrush? ToAvaloniaBrush(this Microsoft.Maui.Graphics.Color? color)
    {
        if (color == null)
            return null;

        return new SolidColorBrush(color.ToAvaloniaColor());
    }

    /// <summary>
    /// Converts a .NET MAUI <see cref="Microsoft.Maui.Graphics.Color"/> to an Avalonia <see cref="Color"/> struct.
    /// </summary>
    /// <param name="color">The .NET MAUI color to convert.</param>
    /// <returns>An Avalonia <see cref="Color"/> with equivalent ARGB values.</returns>
    public static Color ToAvaloniaColor(this Microsoft.Maui.Graphics.Color color)
    {
        return Color.FromArgb(
            (byte)(color.Alpha * 255),
            (byte)(color.Red * 255),
            (byte)(color.Green * 255),
            (byte)(color.Blue * 255));
    }
}
