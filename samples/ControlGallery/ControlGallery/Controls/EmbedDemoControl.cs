using Avalonia;
using Avalonia.Controls;
using AvaButton = Avalonia.Controls.Button;
using AvaCheckBox = Avalonia.Controls.CheckBox;
using AvaSlider = Avalonia.Controls.Slider;
using AvaProgressBar = Avalonia.Controls.ProgressBar;

namespace ControlGallery.Controls;

public class EmbedDemoControl : UserControl
{
    public EmbedDemoControl()
    {
        Content = new StackPanel
        {
            Spacing = 16,
            Children =
            {
                new TextBlock
                {
                    Text = "Hello from Avalonia!",
                    FontSize = 28,
                },
                new TextBlock
                {
                    TextWrapping = Avalonia.Media.TextWrapping.Wrap,
                    Text = "These controls below are rendered entirely by Avalonia, hosted inside a .NET MAUI page via AvaloniaView.",
                },
                new AvaButton
                {
                    Content = "Click Me - Avalonia Button",
                },
                new TextBox
                {
                    Watermark = "Type something here...",
                },
                new AvaCheckBox
                {
                    Content = "An Avalonia CheckBox",
                },
                new AvaSlider
                {
                    Minimum = 0,
                    Maximum = 100,
                    Value = 50,
                },
                new AvaProgressBar
                {
                    Minimum = 0,
                    Maximum = 100,
                    Value = 75,
                },
            },
        };
    }
}
