using Avalonia.Controls.Maui.Tests.Stubs;
using Avalonia.Headless.XUnit;
using MauiStepperHandler = Avalonia.Controls.Maui.Handlers.StepperHandler;
using AvaloniaStepper = Avalonia.Controls.Maui.Stepper;

namespace Avalonia.Controls.Maui.Tests.Handlers;

public class StepperHandlerTests : HandlerTestBase<MauiStepperHandler, StepperStub>
{
    [AvaloniaFact(DisplayName = "Minimum initializes correctly")]
    public async Task MinimumInitializesCorrectly()
    {
        var stepper = new StepperStub { Minimum = 5, Maximum = 100 };

        await ValidatePropertyInitValue(
            stepper,
            () => stepper.Minimum,
            handler => GetPlatformStepper(handler).Minimum,
            5d);
    }

    [AvaloniaFact(DisplayName = "Maximum initializes correctly")]
    public async Task MaximumInitializesCorrectly()
    {
        var stepper = new StepperStub { Maximum = 50 };

        await ValidatePropertyInitValue(
            stepper,
            () => stepper.Maximum,
            handler => GetPlatformStepper(handler).Maximum,
            50d);
    }

    [AvaloniaFact(DisplayName = "Value initializes correctly")]
    public async Task ValueInitializesCorrectly()
    {
        var stepper = new StepperStub { Value = 25, Maximum = 100 };

        await ValidatePropertyInitValue(
            stepper,
            () => stepper.Value,
            handler => GetPlatformStepper(handler).Value,
            25d);
    }

    [AvaloniaFact(DisplayName = "Interval initializes correctly")]
    public async Task IntervalInitializesCorrectly()
    {
        var stepper = new StepperStub { Interval = 5, Maximum = 100 };

        await ValidatePropertyInitValue(
            stepper,
            () => stepper.Interval,
            handler => GetPlatformStepper(handler).Increment,
            5d);
    }

    [AvaloniaFact(DisplayName = "Value updates correctly")]
    public async Task ValueUpdatesCorrectly()
    {
        var stepper = new StepperStub { Value = 0, Maximum = 100 };

        await ValidatePropertyUpdatesValue(
            stepper,
            nameof(StepperStub.Value),
            handler => GetPlatformStepper(handler).Value,
            expectedSetValue: 10d,
            expectedUnsetValue: 3d);
    }

    [AvaloniaFact(DisplayName = "Platform value change updates virtual view")]
    public async Task PlatformValueChangeUpdatesVirtualView()
    {
        var stepper = new StepperStub { Minimum = 0, Maximum = 100, Value = 0 };
        var handler = await CreateHandlerAsync(stepper);

        await InvokeOnMainThreadAsync(() =>
        {
            var platform = GetPlatformStepper(handler);
            platform.Value = 42;
        });

        Assert.Equal(42, stepper.Value);
    }

    [AvaloniaFact(DisplayName = "Platform clamps value to maximum")]
    public async Task PlatformClampsValueToMaximum()
    {
        var stepper = new StepperStub { Minimum = 0, Maximum = 10, Value = 20 };
        var handler = await CreateHandlerAsync(stepper);

        var platform = GetPlatformStepper(handler);
        Assert.Equal(10, platform.Value);
    }

    [AvaloniaFact(DisplayName = "Platform clamps value to minimum")]
    public async Task PlatformClampsValueToMinimum()
    {
        var stepper = new StepperStub { Minimum = 5, Maximum = 100, Value = 0 };
        var handler = await CreateHandlerAsync(stepper);

        var platform = GetPlatformStepper(handler);
        Assert.Equal(5, platform.Value);
    }

    [AvaloniaFact(DisplayName = "Minimum updates correctly")]
    public async Task MinimumUpdatesCorrectly()
    {
        var stepper = new StepperStub { Minimum = 0, Maximum = 100 };

        await ValidatePropertyUpdatesValue(
            stepper,
            nameof(StepperStub.Minimum),
            handler => GetPlatformStepper(handler).Minimum,
            expectedSetValue: 10d,
            expectedUnsetValue: 5d);
    }

    [AvaloniaFact(DisplayName = "Maximum updates correctly")]
    public async Task MaximumUpdatesCorrectly()
    {
        var stepper = new StepperStub { Minimum = 0, Maximum = 100 };

        await ValidatePropertyUpdatesValue(
            stepper,
            nameof(StepperStub.Maximum),
            handler => GetPlatformStepper(handler).Maximum,
            expectedSetValue: 50d,
            expectedUnsetValue: 75d);
    }

    [AvaloniaFact(DisplayName = "Interval updates correctly")]
    public async Task IntervalUpdatesCorrectly()
    {
        var stepper = new StepperStub { Interval = 1, Maximum = 100 };

        await ValidatePropertyUpdatesValue(
            stepper,
            nameof(StepperStub.Interval),
            handler => GetPlatformStepper(handler).Increment,
            expectedSetValue: 5d,
            expectedUnsetValue: 10d);
    }

    [AvaloniaFact(DisplayName = "Default values are set correctly")]
    public async Task DefaultValuesAreSetCorrectly()
    {
        var stepper = new StepperStub();
        var handler = await CreateHandlerAsync(stepper);

        var platform = GetPlatformStepper(handler);

        Assert.Equal(0, platform.Minimum);
        Assert.Equal(100, platform.Maximum);
        Assert.Equal(0, platform.Value);
        Assert.Equal(1, platform.Increment);
    }

    private static AvaloniaStepper GetPlatformStepper(MauiStepperHandler handler)
    {
        var stepper = handler.PlatformView;
        Assert.NotNull(stepper);
        return stepper;
    }
}
