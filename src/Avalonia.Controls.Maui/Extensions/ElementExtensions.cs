namespace Avalonia.Controls.Maui;

public static class ElementExtensions
{
    public static global::Avalonia.Thickness ToThickness(this Microsoft.Maui.Thickness thickness)
    {
        return new global::Avalonia.Thickness(
                        double.IsNaN(thickness.Left) ? 0 : thickness.Left,
                        double.IsNaN(thickness.Top) ? 0 : thickness.Top,
                        double.IsNaN(thickness.Right) ? 0 : thickness.Right,
                        double.IsNaN(thickness.Bottom) ? 0 : thickness.Bottom);
    }
}