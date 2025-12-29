using Avalonia.Controls.Maui.Extensions;
using Microsoft.Maui;
using Microsoft.Maui.Handlers;
using PlatformView = Avalonia.Controls.Maui.ProgressRing;

namespace Avalonia.Controls.Maui.Handlers;

public partial class ActivityIndicatorHandler : ViewHandler<IActivityIndicator, ProgressRing>
{
    public static IPropertyMapper<IActivityIndicator, ActivityIndicatorHandler> Mapper = new PropertyMapper<IActivityIndicator, ActivityIndicatorHandler>(ViewHandler.ViewMapper)
    {
        [nameof(IActivityIndicator.Color)] = MapColor,
        [nameof(IActivityIndicator.IsRunning)] = MapIsRunning,
    };

    public static CommandMapper<IActivityIndicator, ActivityIndicatorHandler> CommandMapper = new(ViewCommandMapper);

    public ActivityIndicatorHandler() : base(Mapper, CommandMapper)
    {
    }

    public ActivityIndicatorHandler(IPropertyMapper? mapper)
        : base(mapper ?? Mapper, CommandMapper)
    {
    }

    public ActivityIndicatorHandler(IPropertyMapper? mapper, CommandMapper? commandMapper)
        : base(mapper ?? Mapper, commandMapper ?? CommandMapper)
    {
    }

    protected override ProgressRing CreatePlatformView()
    {
        return new ProgressRing
        {
            IsIndeterminate = true
        };
    }

    public static void MapIsRunning(ActivityIndicatorHandler handler, IActivityIndicator activityIndicator)
    {
        if (handler.PlatformView is PlatformView platformView)
        {
            platformView.UpdateIsRunning(activityIndicator);
        }
    }

    public static void MapColor(ActivityIndicatorHandler handler, IActivityIndicator activityIndicator)
    {
        if (handler.PlatformView is PlatformView platformView)
        {
            platformView.UpdateColor(activityIndicator);
        }
    }
}