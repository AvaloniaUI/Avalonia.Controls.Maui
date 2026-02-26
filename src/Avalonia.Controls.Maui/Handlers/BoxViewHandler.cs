using Avalonia.Controls.Maui.Platform;
using Microsoft.Maui;
using Microsoft.Maui.Controls;

namespace Avalonia.Controls.Maui.Handlers;

/// <summary>Avalonia handler for <see cref="BoxView"/>.</summary>
public class BoxViewHandler : ViewHandler<BoxView, Border>
{
    /// <summary>Property mapper for <see cref="BoxViewHandler"/>.</summary>
    public static IPropertyMapper<BoxView, BoxViewHandler> Mapper = new PropertyMapper<BoxView, BoxViewHandler>(ViewHandler.ViewMapper)
    {
        [nameof(BoxView.Color)] = MapColor,
        [nameof(BoxView.CornerRadius)] = MapCornerRadius,
    };
    
    /// <summary>Command mapper for <see cref="BoxViewHandler"/>.</summary>
    public static CommandMapper<BoxView, BoxViewHandler> CommandMapper = new(ViewCommandMapper);

    /// <summary>Initializes a new instance of <see cref="BoxViewHandler"/>.</summary>
    public BoxViewHandler() : base(Mapper, CommandMapper)
    {
    }

    /// <summary>Initializes a new instance of <see cref="BoxViewHandler"/>.</summary>
    /// <param name="mapper">The property mapper to use, or <c>null</c> to use the default mapper.</param>
    public BoxViewHandler(IPropertyMapper? mapper)
        : base(mapper ?? Mapper, CommandMapper)
    {
    }

    /// <summary>Initializes a new instance of <see cref="BoxViewHandler"/>.</summary>
    /// <param name="mapper">The property mapper to use, or <c>null</c> to use the default mapper.</param>
    /// <param name="commandMapper">The command mapper to use, or <c>null</c> to use the default command mapper.</param>
    public BoxViewHandler(IPropertyMapper? mapper, CommandMapper? commandMapper)
        : base(mapper ?? Mapper, commandMapper ?? CommandMapper)
    {
    }

    /// <summary>Creates the Avalonia platform view for this handler.</summary>
    protected override Border CreatePlatformView()
    {
        return new Border();
    }
    
    /// <summary>Maps the Color property to the platform view.</summary>
    /// <param name="handler">The handler.</param>
    /// <param name="boxView">The virtual view.</param>
    public static void MapColor(BoxViewHandler handler, BoxView boxView)
    {
        if (handler.PlatformView is Border platformView)
        {
            platformView.UpdateColor(boxView);
        }
    }

    /// <summary>Maps the CornerRadius property to the platform view.</summary>
    /// <param name="handler">The handler.</param>
    /// <param name="boxView">The virtual view.</param>
    public static void MapCornerRadius(BoxViewHandler handler, BoxView boxView)
    {
        if (handler.PlatformView is Border platformView)
        {
            platformView.UpdateCornerRadius(boxView);
        }
    }
}