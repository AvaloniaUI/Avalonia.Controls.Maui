using AvaloniaThickness = Avalonia.Thickness;
using MauiThickness = Microsoft.Maui.Thickness;

namespace Avalonia.Controls.Maui;

/// <summary>
/// Provides extension methods for converting .NET MAUI elements to Avalonia elements.
/// </summary>
public static class ElementExtensions
{
    /// <summary>
    /// Converts a <see cref="MauiThickness"/> to an <see cref="AvaloniaThickness"/>.
    /// </summary>
    /// <remarks>
    /// .NET MAUI NaN values are converted to 0.
    /// </remarks>
    /// <param name="thickness">The thickness to convert.</param>
    /// <returns>Converted thickness as <see cref="AvaloniaThickness"/>.</returns>
    public static AvaloniaThickness ToPlatform(this MauiThickness thickness)
    {
        return new AvaloniaThickness(
                        double.IsNaN(thickness.Left) ? 0 : thickness.Left,
                        double.IsNaN(thickness.Top) ? 0 : thickness.Top,
                        double.IsNaN(thickness.Right) ? 0 : thickness.Right,
                        double.IsNaN(thickness.Bottom) ? 0 : thickness.Bottom);
    }
}