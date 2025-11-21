using Avalonia.Controls.Maui.Extensions;
using Microsoft.Maui;
using Microsoft.Maui.Handlers;
using PlatformView = Avalonia.Controls.Maui.ProgressRing;

namespace Avalonia.Controls.Maui.Handlers;

public partial class ActivityIndicatorHandler : ViewHandler<IActivityIndicator, ProgressRing>, IActivityIndicatorHandler
{
    public static IPropertyMapper<IActivityIndicator, IActivityIndicatorHandler> Mapper = new PropertyMapper<IActivityIndicator, IActivityIndicatorHandler>(ViewHandler.ViewMapper)
    {
        [nameof(IActivityIndicator.Color)] = MapColor,
        [nameof(IActivityIndicator.IsRunning)] = MapIsRunning,
    };

    public static CommandMapper<IActivityIndicator, IActivityIndicatorHandler> CommandMapper = new(ViewCommandMapper);

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

    IActivityIndicator IActivityIndicatorHandler.VirtualView => VirtualView;

    object IActivityIndicatorHandler.PlatformView => PlatformView;

    protected override ProgressRing CreatePlatformView()
    {
        return new ProgressRing
        {
            IsIndeterminate = true
        };
    }

    public static void MapIsRunning(IActivityIndicatorHandler handler, IActivityIndicator activityIndicator)
    {
        if (handler.PlatformView is PlatformView platformView)
        {
            platformView.UpdateIsRunning(activityIndicator);
        }
    }

    public static void MapColor(IActivityIndicatorHandler handler, IActivityIndicator activityIndicator)
    {
        if (handler.PlatformView is PlatformView platformView)
        {
            platformView.UpdateColor(activityIndicator);
        }
    }
}