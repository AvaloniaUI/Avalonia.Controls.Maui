using Microsoft.Maui.Controls;
using SolidColorBrush = Avalonia.Media.SolidColorBrush;

namespace Avalonia.Controls.Maui.Platform;

/// <summary>
/// Extension methods for mapping BoxView properties.
/// </summary>
public static class BoxViewExtensions
{
    /// <summary>
    /// Updates the Border's background color based on the BoxView's color.
    /// </summary>
    /// <param name="border">The Border control to update.</param>
    /// <param name="boxView">The .NET MAUI BoxView providing the color.</param>
    public static void UpdateColor(this Border border, BoxView boxView)
    {
        if (boxView.Color != null)
        {
            border.Background = boxView.Color.ToPlatform();
        }
        else
        {
            border.ClearValue(Border.BackgroundProperty);
        }
    }

    /// <summary>
    /// Updates the Border's corner radius based on the BoxView's corner radius.
    /// </summary>
    /// <param name="border">The Border control to update.</param>
    /// <param name="boxView">The .NET MAUI BoxView providing the corner radius.</param>
    public static void UpdateCornerRadius(this Border border, BoxView boxView)
    {
        var cornerRadius = boxView.CornerRadius;
        
        border.CornerRadius = new CornerRadius(
            cornerRadius.TopLeft,
            cornerRadius.TopRight,
            cornerRadius.BottomRight,
            cornerRadius.BottomLeft);
    }
}