using Microsoft.Maui;
using Microsoft.Maui.Graphics;

namespace Avalonia.Controls.Maui.Tests.Stubs;

/// <summary>
/// Stub implementation of ISlider for handler testing.
/// </summary>
public class SliderStub : StubBase, ISlider
{
    double _minimum;
    double _maximum = 1d;
    double _value;
    Color _minimumTrackColor = Colors.Transparent;
    Color _maximumTrackColor = Colors.Transparent;
    Color _thumbColor = Colors.Transparent;
    IImageSource? _thumbImageSource;

    public double Minimum
    {
        get => _minimum;
        set => SetProperty(ref _minimum, value);
    }

    public double Maximum
    {
        get => _maximum;
        set => SetProperty(ref _maximum, value);
    }

    public double Value
    {
        get => _value;
        set => SetProperty(ref _value, value);
    }

    public Color MinimumTrackColor
    {
        get => _minimumTrackColor;
        set => SetProperty(ref _minimumTrackColor, value);
    }

    public Color MaximumTrackColor
    {
        get => _maximumTrackColor;
        set => SetProperty(ref _maximumTrackColor, value);
    }

    public Color ThumbColor
    {
        get => _thumbColor;
        set => SetProperty(ref _thumbColor, value);
    }

    public IImageSource ThumbImageSource
    {
        get => _thumbImageSource!;
        set => SetProperty(ref _thumbImageSource, value);
    }

    public void DragStarted()
    {
    }

    public void DragCompleted()
    {
    }
}
