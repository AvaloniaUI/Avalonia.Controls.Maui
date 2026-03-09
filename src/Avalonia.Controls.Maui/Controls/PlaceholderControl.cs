using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Media;

namespace Avalonia.Controls.Maui.Controls;

/// <summary>
/// A placeholder control that displays a "not available" message for unimplemented MAUI controls.
/// </summary>
public class PlaceholderControl : Border
{
    private readonly TextBlock _textBlock;

    /// <summary>
    /// Initializes a new instance of the <see cref="PlaceholderControl"/> class.
    /// </summary>
    /// <param name="message">The placeholder message to display.</param>
    public PlaceholderControl(string message = "This control is not available")
    {
        _textBlock = new TextBlock
        {
            Text = message,
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center,
            Foreground = Brushes.Gray,
        };

        Background = new SolidColorBrush(Color.FromArgb(20, 128, 128, 128));
        Child = _textBlock;
    }

    /// <summary>
    /// Gets or sets the placeholder message text.
    /// </summary>
    public string Message
    {
        get => _textBlock.Text ?? string.Empty;
        set => _textBlock.Text = value;
    }
}
