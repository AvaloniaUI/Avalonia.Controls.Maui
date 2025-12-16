using Microsoft.Maui;

namespace Avalonia.Controls.Maui.Tests.Stubs;

/// <summary>
/// Stub implementation of IStepper for handler testing.
/// </summary>
public class StepperStub : StubBase, IStepper
{
    double _minimum;
    double _maximum = 100d;
    double _value;
    double _interval = 1d;

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

    public double Interval
    {
        get => _interval;
        set => SetProperty(ref _interval, value);
    }
}
