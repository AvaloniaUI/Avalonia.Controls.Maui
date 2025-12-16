using Avalonia.Media;

namespace Avalonia.Controls.Maui.Extensions;

internal static class ColorExtensions
{
    public static IBrush? ToAvaloniaBrush(this Microsoft.Maui.Graphics.Color? color)
    {
        if (color == null)
            return null;

        return new SolidColorBrush(Avalonia.Media.Color.FromArgb(
            (byte)(color.Alpha * 255),
            (byte)(color.Red * 255),
            (byte)(color.Green * 255),
            (byte)(color.Blue * 255)));
    }
}
