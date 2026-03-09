#nullable disable
using Microsoft.Maui;
using Microsoft.Maui.Graphics;
using PlatformView = global::Avalonia.Controls.Shapes.Ellipse;
using Ellipse = Microsoft.Maui.Controls.Shapes.Ellipse;

namespace Avalonia.Controls.Maui.Handlers.Shapes;

/// <summary>Avalonia handler for IEllipse shapes.</summary>
public partial class EllipseHandler : ShapeViewHandler<Ellipse, PlatformView>
{
    /// <summary>Property mapper for <see cref="EllipseHandler"/>.</summary>
    public static new IPropertyMapper<Ellipse, IShapeViewHandler> Mapper = new PropertyMapper<Ellipse, IShapeViewHandler>(ShapeViewHandler.Mapper)
    {
        [nameof(IShapeView.Shape)] = MapShape,
    };

    /// <summary>Initializes a new instance of <see cref="EllipseHandler"/>.</summary>
    public EllipseHandler() : base(Mapper)
    {
    }

    /// <summary>Initializes a new instance of <see cref="EllipseHandler"/>.</summary>
    /// <param name="mapper">The property mapper to use.</param>
    public EllipseHandler(IPropertyMapper mapper) : base(mapper ?? Mapper)
    {
    }

    /// <summary>Creates the Avalonia platform view for this handler.</summary>
    protected override PlatformView CreatePlatformView()
    {
        return new PlatformView();
    }

    /// <summary>Maps the Shape property to the platform view.</summary>
    /// <param name="handler">The shape view handler.</param>
    /// <param name="shape">The shape view.</param>
    public static new void MapShape(IShapeViewHandler handler, IShapeView shape)
    {
        // Ellipse shape is automatically handled by the Avalonia.Controls.Shapes.Ellipse control
        // No additional mapping needed - the control renders itself as an ellipse
    }
}
