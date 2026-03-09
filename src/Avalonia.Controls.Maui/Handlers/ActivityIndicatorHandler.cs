using Avalonia.Controls.Maui.Extensions;
using Microsoft.Maui;
using PlatformView = Avalonia.Controls.Maui.ProgressRing;

namespace Avalonia.Controls.Maui.Handlers;

/// <summary>Avalonia handler for <see cref="IActivityIndicator"/>.</summary>
public partial class ActivityIndicatorHandler : ViewHandler<IActivityIndicator, ProgressRing>
{
    /// <summary>Property mapper for <see cref="ActivityIndicatorHandler"/>.</summary>
    public static IPropertyMapper<IActivityIndicator, ActivityIndicatorHandler> Mapper = new PropertyMapper<IActivityIndicator, ActivityIndicatorHandler>(ViewHandler.ViewMapper)
    {
        [nameof(IActivityIndicator.Color)] = MapColor,
        [nameof(IActivityIndicator.IsRunning)] = MapIsRunning,
    };

    /// <summary>Command mapper for <see cref="ActivityIndicatorHandler"/>.</summary>
    public static CommandMapper<IActivityIndicator, ActivityIndicatorHandler> CommandMapper = new(ViewCommandMapper);

    /// <summary>Initializes a new instance of <see cref="ActivityIndicatorHandler"/>.</summary>
    public ActivityIndicatorHandler() : base(Mapper, CommandMapper)
    {
    }

    /// <summary>Initializes a new instance of <see cref="ActivityIndicatorHandler"/>.</summary>
    /// <param name="mapper">The property mapper to use, or <c>null</c> to use the default mapper.</param>
    public ActivityIndicatorHandler(IPropertyMapper? mapper)
        : base(mapper ?? Mapper, CommandMapper)
    {
    }

    /// <summary>Initializes a new instance of <see cref="ActivityIndicatorHandler"/>.</summary>
    /// <param name="mapper">The property mapper to use, or <c>null</c> to use the default mapper.</param>
    /// <param name="commandMapper">The command mapper to use, or <c>null</c> to use the default command mapper.</param>
    public ActivityIndicatorHandler(IPropertyMapper? mapper, CommandMapper? commandMapper)
        : base(mapper ?? Mapper, commandMapper ?? CommandMapper)
    {
    }

    /// <summary>Creates the Avalonia platform view for this handler.</summary>
    protected override ProgressRing CreatePlatformView()
    {
        return new ProgressRing
        {
            IsIndeterminate = true
        };
    }

    /// <summary>Maps the IsRunning property to the platform view.</summary>
    /// <param name="handler">The handler.</param>
    /// <param name="activityIndicator">The virtual view.</param>
    public static void MapIsRunning(ActivityIndicatorHandler handler, IActivityIndicator activityIndicator)
    {
        if (handler.PlatformView is PlatformView platformView)
        {
            platformView.UpdateIsRunning(activityIndicator);
        }
    }

    /// <summary>Maps the Color property to the platform view.</summary>
    /// <param name="handler">The handler.</param>
    /// <param name="activityIndicator">The virtual view.</param>
    public static void MapColor(ActivityIndicatorHandler handler, IActivityIndicator activityIndicator)
    {
        if (handler.PlatformView is PlatformView platformView)
        {
            platformView.UpdateColor(activityIndicator);
        }
    }
}