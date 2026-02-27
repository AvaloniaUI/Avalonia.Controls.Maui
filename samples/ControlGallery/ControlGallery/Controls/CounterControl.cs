using Avalonia;
using Avalonia.Controls;
using AvaHorizontalAlignment = Avalonia.Layout.HorizontalAlignment;
using AvaVerticalAlignment = Avalonia.Layout.VerticalAlignment;
using AvaButton = Avalonia.Controls.Button;

namespace ControlGallery.Controls;

public class CounterControl : UserControl
{
    public static readonly StyledProperty<int> CountProperty =
        AvaloniaProperty.Register<CounterControl, int>(nameof(Count));

    public int Count
    {
        get => GetValue(CountProperty);
        set => SetValue(CountProperty, value);
    }

    private readonly TextBlock _countText;

    public CounterControl()
    {
        _countText = new TextBlock
        {
            HorizontalAlignment = AvaHorizontalAlignment.Center,
            VerticalAlignment = AvaVerticalAlignment.Center,
            FontSize = 24,
        };

        var incrementButton = new AvaButton
        {
            Content = "Increment",
            HorizontalAlignment = AvaHorizontalAlignment.Center,
        };

        incrementButton.Click += (_, _) => Count++;

        Content = new StackPanel
        {
            VerticalAlignment = AvaVerticalAlignment.Center,
            HorizontalAlignment = AvaHorizontalAlignment.Center,
            Spacing = 16,
            Children =
            {
                _countText,
                incrementButton,
            },
        };

        UpdateText();
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);

        if (change.Property == CountProperty)
        {
            UpdateText();
        }
    }

    private void UpdateText()
    {
        _countText.Text = $"Count: {Count}";
    }
}
