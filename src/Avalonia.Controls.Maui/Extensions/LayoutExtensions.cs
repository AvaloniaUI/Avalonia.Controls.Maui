using Microsoft.Maui;
using Microsoft.Maui.Primitives;
using AvaloniaHorizontalAlignment = Avalonia.Layout.HorizontalAlignment;
using AvaloniaVerticalAlignment = Avalonia.Layout.VerticalAlignment;

namespace Avalonia.Controls.Maui;

/// <summary>
/// Provides extension methods for converting .NET MAUI layout alignment enums to Avalonia alignment types.
/// </summary>
public static class LayoutExtensions
{
    /// <summary>
    /// Converts a .NET MAUI <see cref="LayoutAlignment"/> to an Avalonia <see cref="AvaloniaHorizontalAlignment"/>.
    /// </summary>
    /// <param name="alignment">The .NET MAUI layout alignment to convert.</param>
    /// <returns>The corresponding Avalonia <see cref="AvaloniaHorizontalAlignment"/> value.</returns>
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

    /// <summary>
    /// Converts a .NET MAUI <see cref="LayoutAlignment"/> to an Avalonia <see cref="AvaloniaVerticalAlignment"/>.
    /// </summary>
    /// <param name="alignment">The .NET MAUI layout alignment to convert.</param>
    /// <returns>The corresponding Avalonia <see cref="AvaloniaVerticalAlignment"/> value.</returns>
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
