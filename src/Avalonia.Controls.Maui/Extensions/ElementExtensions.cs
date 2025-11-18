namespace Avalonia.Controls.Maui;

public static class ElementExtensions
{
    public static global::Avalonia.Thickness ToThickness(this Microsoft.Maui.Thickness thickness)
    {
        return new global::Avalonia.Thickness(thickness.Left, thickness.Top, thickness.Right, thickness.Bottom);
    }
}