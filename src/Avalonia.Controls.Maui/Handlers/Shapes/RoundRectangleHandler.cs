#nullable disable
using Microsoft.Maui;
using Microsoft.Maui.Controls.Shapes;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Handlers;
using PlatformView = Avalonia.Controls.Maui.Platform.MauiRoundRectangle;

namespace Avalonia.Controls.Maui.Handlers.Shapes;

internal partial class RoundRectangleHandler : ShapeViewHandler<RoundRectangle, PlatformView>
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
            var radius = roundRectangle.CornerRadius;
            platformView.CornerRadius = new global::Avalonia.CornerRadius(
                radius.TopLeft,
                radius.TopRight,
                radius.BottomRight,
                radius.BottomLeft);
        }
    }
}