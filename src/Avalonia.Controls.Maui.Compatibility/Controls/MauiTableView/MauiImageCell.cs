using Avalonia.Layout;
using Avalonia.Media;

namespace Avalonia.Controls.Maui;

/// <summary>
/// Platform control for rendering a MAUI ImageCell.
/// </summary>
public class MauiImageCell : Border
{
    /// <summary>
    /// Gets the image view.
    /// </summary>
    public Image ImageView { get; }

    /// <summary>
    /// Gets the primary text block.
    /// </summary>
    public TextBlock PrimaryLabel { get; }

    /// <summary>
    /// Gets the secondary (detail) text block.
    /// </summary>
    public TextBlock SecondaryLabel { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="MauiImageCell"/> class.
    /// </summary>
    public MauiImageCell()
    {
        ImageView = new Image
        {
            Width = 40,
            Height = 40,
            Stretch = Stretch.Uniform,
            VerticalAlignment = VerticalAlignment.Center,
            Margin = new Thickness(0, 0, 12, 0)
        };

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

        var textStack = new StackPanel
        {
            Orientation = Orientation.Vertical,
            Spacing = 2,
            VerticalAlignment = VerticalAlignment.Center,
            Children =
            {
                PrimaryLabel,
                SecondaryLabel
            }
        };

        var contentStack = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Spacing = 0,
            Margin = new Thickness(16, 12),
            Children =
            {
                ImageView,
                textStack
            }
        };

        MinHeight = 64;
        Background = Brushes.Transparent;
        Child = contentStack;
    }
}
