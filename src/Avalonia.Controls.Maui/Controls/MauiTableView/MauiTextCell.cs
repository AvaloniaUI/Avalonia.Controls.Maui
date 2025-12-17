using Avalonia.Layout;
using Avalonia.Media;

namespace Avalonia.Controls.Maui;

/// <summary>
/// Platform control for rendering a MAUI TextCell.
/// </summary>
public class MauiTextCell : Border
{
    /// <summary>
    /// Gets the primary text block.
    /// </summary>
    public TextBlock PrimaryLabel { get; }

    /// <summary>
    /// Gets the secondary (detail) text block.
    /// </summary>
    public TextBlock SecondaryLabel { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="MauiTextCell"/> class.
    /// </summary>
    public MauiTextCell()
    {
        PrimaryLabel = new TextBlock
        {
            FontSize = 16,
            VerticalAlignment = VerticalAlignment.Center
        };

        SecondaryLabel = new TextBlock
        {
            FontSize = 13,
            Opacity = 0.7,
            VerticalAlignment = VerticalAlignment.Center
        };

        var stackPanel = new StackPanel
        {
            Orientation = Orientation.Vertical,
            VerticalAlignment = VerticalAlignment.Center,
            Spacing = 2,
            Margin = new Thickness(16, 10),
            Children =
            {
                PrimaryLabel,
                SecondaryLabel
            }
        };

        MinHeight = 44;
        Background = Brushes.Transparent;
        Cursor = new Input.Cursor(Input.StandardCursorType.Hand);
        Child = stackPanel;
    }
}
