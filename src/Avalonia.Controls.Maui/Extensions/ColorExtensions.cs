using Avalonia.Media;

namespace Avalonia.Controls.Maui.Extensions;

public static class ColorExtensions
{
    public static IBrush? ToAvaloniaBrush(this Microsoft.Maui.Graphics.Color? color)
    {
        if (color == null)
            return null;

        return new SolidColorBrush(color.ToAvaloniaColor());
    }

    public static Color ToAvaloniaColor(this Microsoft.Maui.Graphics.Color color)
    {
        return Color.FromArgb(
            (byte)(color.Alpha * 255),
            (byte)(color.Red * 255),
            (byte)(color.Green * 255),
            (byte)(color.Blue * 255));
    }
}
