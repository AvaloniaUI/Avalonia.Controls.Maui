using Avalonia.Controls.Maui.Platform;
using Microsoft.Maui;
using Microsoft.Maui.Controls;

namespace Avalonia.Controls.Maui.Handlers;

public class BoxViewHandler : ViewHandler<BoxView, Border>
{
    public static IPropertyMapper<BoxView, BoxViewHandler> Mapper = new PropertyMapper<BoxView, BoxViewHandler>(ViewHandler.ViewMapper)
    {
        [nameof(BoxView.Color)] = MapColor,
        [nameof(BoxView.CornerRadius)] = MapCornerRadius,
    };
    
    public static CommandMapper<BoxView, BoxViewHandler> CommandMapper = new(ViewCommandMapper);
    
    public BoxViewHandler() : base(Mapper, CommandMapper)
    {
    }
    
    public BoxViewHandler(IPropertyMapper? mapper)
        : base(mapper ?? Mapper, CommandMapper)
    {
    }
    
    public BoxViewHandler(IPropertyMapper? mapper, CommandMapper? commandMapper)
        : base(mapper ?? Mapper, commandMapper ?? CommandMapper)
    {
    }

    protected override Border CreatePlatformView()
    {
        return new Border();
    }

    public static void MapBackground(BoxViewHandler handler, BoxView boxView)
    {
        if (handler.PlatformView is Border platformView)
        {
            platformView.UpdateBackground(boxView);
        }
    }
    
    public static void MapColor(BoxViewHandler handler, BoxView boxView)
    {
        if (handler.PlatformView is Border platformView)
        {
            platformView.UpdateColor(boxView);
        }
    }
    
    public static void MapCornerRadius(BoxViewHandler handler, BoxView boxView)
    {
        if (handler.PlatformView is Border platformView)
        {
            platformView.UpdateCornerRadius(boxView);
        }
    }
}