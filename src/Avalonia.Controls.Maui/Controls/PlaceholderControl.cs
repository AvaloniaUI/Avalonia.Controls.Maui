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

    public string Message
    {
        get => _textBlock.Text ?? string.Empty;
        set => _textBlock.Text = value;
    }
}
