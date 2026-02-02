using System;
using Microsoft.Maui;
using PlatformView = Avalonia.Controls.Maui.Stepper;

namespace Avalonia.Controls.Maui.Handlers;

public partial class StepperHandler : ViewHandler<IStepper, PlatformView>
{
    public static IPropertyMapper<IStepper, StepperHandler> Mapper = new PropertyMapper<IStepper, StepperHandler>(ViewHandler.ViewMapper)
    {
        [nameof(IStepper.Interval)] = MapIncrement,
        [nameof(IStepper.Maximum)] = MapMaximum,
        [nameof(IStepper.Minimum)] = MapMinimum,
        [nameof(IStepper.Value)] = MapValue,
        [nameof(IStepper.Background)] = MapBackground,
    };

    public static CommandMapper<IStepper, StepperHandler> CommandMapper = new(ViewHandler.ViewCommandMapper)
    {
    };

    public StepperHandler() : base(Mapper, CommandMapper)
    {
    }

    public StepperHandler(IPropertyMapper? mapper)
        : base(mapper ?? Mapper, CommandMapper)
    {
    }

    public StepperHandler(IPropertyMapper? mapper, CommandMapper? commandMapper)
        : base(mapper ?? Mapper, commandMapper ?? CommandMapper)
    {
    }

    protected override PlatformView CreatePlatformView()
    {
        return new PlatformView();
    }

    protected override void ConnectHandler(PlatformView platformView)
    {
        base.ConnectHandler(platformView);
        platformView.ValueChanged += OnValueChanged;
    }

    protected override void DisconnectHandler(PlatformView platformView)
    {
        platformView.ValueChanged -= OnValueChanged;
        base.DisconnectHandler(platformView);
    }

    private void OnValueChanged(object? sender, EventArgs e)
    {
        if (VirtualView == null || PlatformView == null)
            return;

        VirtualView.Value = PlatformView.Value;
    }

    public static void MapMinimum(StepperHandler handler, IStepper stepper)
    {
        if (handler.PlatformView is PlatformView platformView)
        {
            platformView.Minimum = stepper.Minimum;
        }
    }

    public static void MapMaximum(StepperHandler handler, IStepper stepper)
    {
        if (handler.PlatformView is PlatformView platformView)
        {
            platformView.Maximum = stepper.Maximum;
        }
    }

    public static void MapIncrement(StepperHandler handler, IStepper stepper)
    {
        if (handler.PlatformView is PlatformView platformView)
        {
            platformView.Increment = stepper.Interval;
        }
    }

    public static void MapValue(StepperHandler handler, IStepper stepper)
    {
        if (handler.PlatformView is PlatformView platformView)
        {
            platformView.Value = stepper.Value;
        }
    }

    public static void MapBackground(StepperHandler handler, IStepper stepper)
    {
        ViewHandler.MapBackground(handler, stepper);
    }
}
