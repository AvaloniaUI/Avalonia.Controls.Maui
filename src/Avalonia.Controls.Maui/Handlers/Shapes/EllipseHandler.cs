#nullable disable
using Microsoft.Maui;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Handlers;
using PlatformView = global::Avalonia.Controls.Shapes.Ellipse;
using Ellipse = Microsoft.Maui.Controls.Shapes.Ellipse;

namespace Avalonia.Controls.Maui.Handlers.Shapes;

public partial class EllipseHandler : ShapeViewHandler<Ellipse, PlatformView>
{
    public static new IPropertyMapper<Ellipse, IShapeViewHandler> Mapper = new PropertyMapper<Ellipse, IShapeViewHandler>(ShapeViewHandler.Mapper)
    {
        [nameof(IShapeView.Shape)] = MapShape,
    };

    public EllipseHandler() : base(Mapper)
    {
    }

    public EllipseHandler(IPropertyMapper mapper) : base(mapper ?? Mapper)
    {
    }

    protected override PlatformView CreatePlatformView()
    {
        return new PlatformView();
    }

    public static new void MapShape(IShapeViewHandler handler, IShapeView shape)
    {
        // Ellipse shape is automatically handled by the Avalonia.Controls.Shapes.Ellipse control
        // No additional mapping needed - the control renders itself as an ellipse
    }
}