using System;
using Microsoft.Maui;
using Microsoft.Maui.Handlers;
using PlatformView = Avalonia.Controls.Control;

namespace Avalonia.Controls.Maui.Handlers;

public partial class StepperHandler : ViewHandler<IStepper, PlatformView>, IStepperHandler
{
    public static IPropertyMapper<IStepper, IStepperHandler> Mapper = new PropertyMapper<IStepper, IStepperHandler>(ViewHandler.ViewMapper)
    {
        [nameof(IStepper.Interval)] = MapIncrement,
        [nameof(IStepper.Maximum)] = MapMaximum,
        [nameof(IStepper.Minimum)] = MapMinimum,
        [nameof(IStepper.Value)] = MapValue,
        [nameof(IView.Background)] = MapBackground,
    };

    public static CommandMapper<IStepper, IStepperHandler> CommandMapper = new(ViewHandler.ViewCommandMapper)
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
        if (platformView is Stepper stepper)
        {
            stepper.ValueChanged += OnValueChanged;
        }
    }

    protected override void DisconnectHandler(PlatformView platformView)
    {
        if (platformView is Stepper stepper)
        {
            stepper.ValueChanged -= OnValueChanged;
        }
        base.DisconnectHandler(platformView);
    }

    private void OnValueChanged(object? sender, EventArgs e)
    {
        if (VirtualView == null || PlatformView == null)
            return;

        if (PlatformView is Stepper stepper)
        {
            VirtualView.Value = stepper.Value;
        }
    }

    public static void MapMinimum(IStepperHandler handler, IStepper stepper)
    {
        if (handler.PlatformView is Stepper platformView)
        {
            platformView.Minimum = stepper.Minimum;
        }
    }

    public static void MapMaximum(IStepperHandler handler, IStepper stepper)
    {
        if (handler.PlatformView is Stepper platformView)
        {
            platformView.Maximum = stepper.Maximum;
        }
    }

    public static void MapIncrement(IStepperHandler handler, IStepper stepper)
    {
        if (handler.PlatformView is Stepper platformView)
        {
            platformView.Increment = stepper.Interval;
        }
    }

    public static void MapValue(IStepperHandler handler, IStepper stepper)
    {
        if (handler.PlatformView is Stepper platformView)
        {
            platformView.Value = stepper.Value;
        }
    }

    public static void MapBackground(IStepperHandler handler, IStepper stepper)
    {
        ViewHandler.MapBackground(handler, stepper);
    }

    IStepper IStepperHandler.VirtualView => VirtualView;

    object IStepperHandler.PlatformView => PlatformView;
}
