using Avalonia.Controls.Maui.Controls;
using Avalonia.Controls.Maui.Platform;
using Microsoft.Maui;
using Microsoft.Maui.Controls;

namespace Avalonia.Controls.Maui.Handlers;

/// <summary>Avalonia handler for <see cref="CarouselView"/>.</summary>
[NotImplemented("CarouselView is not yet implemented for the Avalonia backend")]
public class CarouselViewHandler : ViewHandler<CarouselView, PlaceholderControl>
{
    /// <summary>Property mapper for <see cref="CarouselViewHandler"/>.</summary>
    public static IPropertyMapper<CarouselView, CarouselViewHandler> Mapper = new PropertyMapper<CarouselView, CarouselViewHandler>(ViewHandler.ViewMapper);

    /// <summary>Command mapper for <see cref="CarouselViewHandler"/>.</summary>
    public static CommandMapper<CarouselView, CarouselViewHandler> CommandMapper = new(ViewCommandMapper);

    /// <summary>Initializes a new instance of <see cref="CarouselViewHandler"/>.</summary>
    public CarouselViewHandler() : base(Mapper, CommandMapper)
    {
    }

    /// <summary>Initializes a new instance of <see cref="CarouselViewHandler"/>.</summary>
    /// <param name="mapper">The property mapper to use, or <c>null</c> to use the default mapper.</param>
    public CarouselViewHandler(IPropertyMapper? mapper)
        : base(mapper ?? Mapper, CommandMapper)
    {
    }

    /// <summary>Initializes a new instance of <see cref="CarouselViewHandler"/>.</summary>
    /// <param name="mapper">The property mapper to use, or <c>null</c> to use the default mapper.</param>
    /// <param name="commandMapper">The command mapper to use, or <c>null</c> to use the default command mapper.</param>
    public CarouselViewHandler(IPropertyMapper? mapper, CommandMapper? commandMapper)
        : base(mapper ?? Mapper, commandMapper ?? CommandMapper)
    {
    }

    /// <summary>Creates the Avalonia platform view for this handler.</summary>
    protected override PlaceholderControl CreatePlatformView()
    {
        return new PlaceholderControl("CarouselView is not available in this version of Avalonia.Controls.Maui");
    }
}
