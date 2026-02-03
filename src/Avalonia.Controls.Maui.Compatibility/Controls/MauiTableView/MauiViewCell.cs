using Avalonia.Media;

namespace Avalonia.Controls.Maui.Compatibility;

/// <summary>
/// Platform control for rendering a MAUI ViewCell.
/// </summary>
public class MauiViewCell : Border
{
    /// <summary>
    /// Initializes a new instance of the <see cref="MauiViewCell"/> class.
    /// </summary>
    public MauiViewCell()
    {
        Padding = new Thickness(0);
        Background = Brushes.Transparent;
        MinHeight = 44;
    }
}
