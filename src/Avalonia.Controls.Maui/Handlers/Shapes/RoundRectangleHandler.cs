#nullable disable
using Microsoft.Maui;
using Microsoft.Maui.Controls.Shapes;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Handlers;
using Avalonia.Controls.Maui.Extensions;
using PlatformView = Avalonia.Controls.Maui.Platform.MauiRoundRectangle;

namespace Avalonia.Controls.Maui.Handlers.Shapes;

public partial class RoundRectangleHandler : ShapeViewHandler<RoundRectangle, PlatformView>
{
    public static new IPropertyMapper<RoundRectangle, IShapeViewHandler> Mapper = new PropertyMapper<RoundRectangle, IShapeViewHandler>(ShapeViewHandler.Mapper)
    {
        [nameof(RoundRectangle.CornerRadius)] = MapCornerRadius
    };

    public RoundRectangleHandler() : base(Mapper)
    {
    }

    public RoundRectangleHandler(IPropertyMapper mapper) : base(mapper ?? Mapper)
    {
    }

    protected override PlatformView CreatePlatformView()
    {
        return new PlatformView();
    }

    public static void MapCornerRadius(IShapeViewHandler handler, RoundRectangle roundRectangle)
    {
        if (handler.PlatformView is PlatformView platformView)
        {
            platformView.UpdateCornerRadius(roundRectangle);
        }
    }
}
