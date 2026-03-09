#nullable disable
using Microsoft.Maui;
using Microsoft.Maui.Controls.Shapes;
using Microsoft.Maui.Graphics;
using Avalonia.Controls.Maui.Extensions;
using PlatformView = Avalonia.Controls.Maui.Platform.MauiRoundRectangle;

namespace Avalonia.Controls.Maui.Handlers.Shapes;

/// <summary>Avalonia handler for IRoundRectangle shapes.</summary>
public partial class RoundRectangleHandler : ShapeViewHandler<RoundRectangle, PlatformView>
{
    /// <summary>Property mapper for <see cref="RoundRectangleHandler"/>.</summary>
    public static new IPropertyMapper<RoundRectangle, IShapeViewHandler> Mapper = new PropertyMapper<RoundRectangle, IShapeViewHandler>(ShapeViewHandler.Mapper)
    {
        [nameof(RoundRectangle.CornerRadius)] = MapCornerRadius
    };

    /// <summary>Initializes a new instance of <see cref="RoundRectangleHandler"/>.</summary>
    public RoundRectangleHandler() : base(Mapper)
    {
    }

    /// <summary>Initializes a new instance of <see cref="RoundRectangleHandler"/>.</summary>
    /// <param name="mapper">The property mapper to use.</param>
    public RoundRectangleHandler(IPropertyMapper mapper) : base(mapper ?? Mapper)
    {
    }

    /// <summary>Creates the Avalonia platform view for this handler.</summary>
    protected override PlatformView CreatePlatformView()
    {
        return new PlatformView();
    }

    /// <summary>Maps the CornerRadius property to the platform view.</summary>
    /// <param name="handler">The shape view handler.</param>
    /// <param name="roundRectangle">The round rectangle.</param>
    public static void MapCornerRadius(IShapeViewHandler handler, RoundRectangle roundRectangle)
    {
        if (handler.PlatformView is PlatformView platformView)
        {
            platformView.UpdateCornerRadius(roundRectangle);
        }
    }
}
