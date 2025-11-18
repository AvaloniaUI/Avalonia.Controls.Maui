using Avalonia.Controls.Maui.Platform;
using Microsoft.Maui;
using Microsoft.Maui.Handlers;
using PlatformView = Avalonia.Controls.Maui.Platform.MauiActivityIndicator;

namespace Avalonia.Controls.Maui.Handlers;

public partial class ActivityIndicatorHandler : ViewHandler<IActivityIndicator, MauiActivityIndicator>, IActivityIndicatorHandler
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

    protected override MauiActivityIndicator CreatePlatformView()
    {
        return new MauiActivityIndicator();
    }

    public static void MapIsRunning(IActivityIndicatorHandler handler, IActivityIndicator activityIndicator)
    {
        if (handler.PlatformView is null || handler.VirtualView is null)
            return;

        ((PlatformView)handler.PlatformView).IsRunning = activityIndicator.IsRunning;
    }

    public static void MapColor(IActivityIndicatorHandler handler, IActivityIndicator activityIndicator)
    {
        if (handler.PlatformView is null || handler.VirtualView is null)
            return;

        if (activityIndicator.Color != null)
        {
            ((PlatformView)handler.PlatformView).Color = activityIndicator.Color.ToPlatform();
        }
    }
}
