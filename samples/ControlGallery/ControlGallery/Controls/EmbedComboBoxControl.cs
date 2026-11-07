using Avalonia.Controls;

namespace ControlGallery.Controls;

/// <summary>
/// A picker-sized Avalonia <see cref="ComboBox"/>. It is hosted in a small
/// <see cref="Avalonia.Controls.Maui.Controls.AvaloniaView"/> to show that its drop-down
/// can extend past the bounds of the host view.
/// </summary>
public class EmbedComboBoxControl : UserControl
{
    public EmbedComboBoxControl()
    {
        Content = new ComboBox
        {
            HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Stretch,
            PlaceholderText = "Pick a fruit",
            ItemsSource = new[]
            {
                "Apple", "Banana", "Cherry", "Grape", "Lemon", "Mango", "Orange", "Peach", "Pear", "Plum",
            },
        };
    }
}
