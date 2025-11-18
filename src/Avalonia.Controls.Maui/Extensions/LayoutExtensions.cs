using Microsoft.Maui;
using Microsoft.Maui.Primitives;
using AvaloniaHorizontalAlignment = Avalonia.Layout.HorizontalAlignment;
using AvaloniaVerticalAlignment = Avalonia.Layout.VerticalAlignment;

namespace Avalonia.Controls.Maui;

public static class LayoutExtensions
{
    public static AvaloniaHorizontalAlignment ToAvaloniaHorizontalAlignment(this LayoutAlignment alignment)
    {
        return alignment switch
        {
            LayoutAlignment.Fill => AvaloniaHorizontalAlignment.Stretch,
            LayoutAlignment.Start => AvaloniaHorizontalAlignment.Left,
            LayoutAlignment.Center => AvaloniaHorizontalAlignment.Center,
            LayoutAlignment.End => AvaloniaHorizontalAlignment.Right,
            _ => AvaloniaHorizontalAlignment.Stretch
        };
    }

    public static AvaloniaVerticalAlignment ToAvaloniaVerticalAlignment(this LayoutAlignment alignment)
    {
        return alignment switch
        {
            LayoutAlignment.Fill => AvaloniaVerticalAlignment.Stretch,
            LayoutAlignment.Start => AvaloniaVerticalAlignment.Top,
            LayoutAlignment.Center => AvaloniaVerticalAlignment.Center,
            LayoutAlignment.End => AvaloniaVerticalAlignment.Bottom,
            _ => AvaloniaVerticalAlignment.Stretch
        };
    }
}
