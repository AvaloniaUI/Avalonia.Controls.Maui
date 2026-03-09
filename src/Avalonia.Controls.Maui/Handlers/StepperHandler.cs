using System;
using Microsoft.Maui;
using PlatformView = Avalonia.Controls.Maui.Stepper;

namespace Avalonia.Controls.Maui.Handlers;

/// <summary>Avalonia handler for <see cref="IStepper"/>.</summary>
public partial class StepperHandler : ViewHandler<IStepper, PlatformView>
{
    /// <summary>Property mapper for <see cref="StepperHandler"/>.</summary>
    public static IPropertyMapper<IStepper, StepperHandler> Mapper = new PropertyMapper<IStepper, StepperHandler>(ViewHandler.ViewMapper)
    {
        [nameof(IStepper.Interval)] = MapIncrement,
        [nameof(IStepper.Maximum)] = MapMaximum,
        [nameof(IStepper.Minimum)] = MapMinimum,
        [nameof(IStepper.Value)] = MapValue,
        [nameof(IStepper.Background)] = MapBackground,
    };

    /// <summary>Command mapper for <see cref="StepperHandler"/>.</summary>
    public static CommandMapper<IStepper, StepperHandler> CommandMapper = new(ViewHandler.ViewCommandMapper)
    {
    };

    /// <summary>Initializes a new instance of <see cref="StepperHandler"/>.</summary>
    public StepperHandler() : base(Mapper, CommandMapper)
    {
    }

    /// <summary>Initializes a new instance of <see cref="StepperHandler"/>.</summary>
    /// <param name="mapper">The property mapper to use, or <see langword="null"/> to use the default.</param>
    public StepperHandler(IPropertyMapper? mapper)
        : base(mapper ?? Mapper, CommandMapper)
    {
    }

    /// <summary>Initializes a new instance of <see cref="StepperHandler"/>.</summary>
    /// <param name="mapper">The property mapper to use, or <see langword="null"/> to use the default.</param>
    /// <param name="commandMapper">The command mapper to use, or <see langword="null"/> to use the default.</param>
    public StepperHandler(IPropertyMapper? mapper, CommandMapper? commandMapper)
        : base(mapper ?? Mapper, commandMapper ?? CommandMapper)
    {
    }

    /// <summary>Creates the Avalonia platform view for this handler.</summary>
    protected override PlatformView CreatePlatformView()
    {
        return new PlatformView();
    }

    /// <inheritdoc/>
    protected override void ConnectHandler(PlatformView platformView)
    {
        base.ConnectHandler(platformView);
        platformView.ValueChanged += OnValueChanged;
    }

    /// <inheritdoc/>
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

    /// <summary>Maps the Minimum property to the platform view.</summary>
    /// <param name="handler">The handler for the stepper.</param>
    /// <param name="stepper">The virtual view.</param>
    public static void MapMinimum(StepperHandler handler, IStepper stepper)
    {
        if (handler.PlatformView is PlatformView platformView)
        {
            platformView.Minimum = stepper.Minimum;
        }
    }

    /// <summary>Maps the Maximum property to the platform view.</summary>
    /// <param name="handler">The handler for the stepper.</param>
    /// <param name="stepper">The virtual view.</param>
    public static void MapMaximum(StepperHandler handler, IStepper stepper)
    {
        if (handler.PlatformView is PlatformView platformView)
        {
            platformView.Maximum = stepper.Maximum;
        }
    }

    /// <summary>Maps the Increment property to the platform view.</summary>
    /// <param name="handler">The handler for the stepper.</param>
    /// <param name="stepper">The virtual view.</param>
    public static void MapIncrement(StepperHandler handler, IStepper stepper)
    {
        if (handler.PlatformView is PlatformView platformView)
        {
            platformView.Increment = stepper.Interval;
        }
    }

    /// <summary>Maps the Value property to the platform view.</summary>
    /// <param name="handler">The handler for the stepper.</param>
    /// <param name="stepper">The virtual view.</param>
    public static void MapValue(StepperHandler handler, IStepper stepper)
    {
        if (handler.PlatformView is PlatformView platformView)
        {
            platformView.Value = stepper.Value;
        }
    }

    /// <summary>Maps the Background property to the platform view.</summary>
    /// <param name="handler">The handler for the stepper.</param>
    /// <param name="stepper">The virtual view.</param>
    public static void MapBackground(StepperHandler handler, IStepper stepper)
    {
        ViewHandler.MapBackground(handler, stepper);
    }
}
