#nullable disable
using Microsoft.Maui;
using Microsoft.Maui.Controls.Shapes;
using Microsoft.Maui.Graphics;
using Avalonia.Controls.Maui.Extensions;
using PlatformView = global::Avalonia.Controls.Shapes.Path;
using Path = Microsoft.Maui.Controls.Shapes.Path;

namespace Avalonia.Controls.Maui.Handlers.Shapes;

/// <summary>Avalonia handler for IPath shapes.</summary>
public partial class PathHandler : ShapeViewHandler<Path, PlatformView>
{
    /// <summary>Property mapper for <see cref="PathHandler"/>.</summary>
    public static new IPropertyMapper<Path, IShapeViewHandler> Mapper = new PropertyMapper<Path, IShapeViewHandler>(ShapeViewHandler.Mapper)
    {
        [nameof(IShapeView.Shape)] = MapShape,
        [nameof(Path.Data)] = MapData,
        [nameof(Path.RenderTransform)] = MapRenderTransform,
    };

    /// <summary>Initializes a new instance of <see cref="PathHandler"/>.</summary>
    public PathHandler() : base(Mapper)
    {
    }

    /// <summary>Initializes a new instance of <see cref="PathHandler"/>.</summary>
    /// <param name="mapper">The property mapper to use.</param>
    public PathHandler(IPropertyMapper mapper) : base(mapper ?? Mapper)
    {
    }

    /// <summary>Creates the Avalonia platform view for this handler.</summary>
    protected override PlatformView CreatePlatformView()
    {
        return new PlatformView();
    }

    /// <inheritdoc/>
    protected override void ConnectHandler(PlatformView platformView)
    {
        base.ConnectHandler(platformView);

        if (VirtualView?.RenderTransform != null)
        {
            VirtualView.RenderTransform.PropertyChanged += OnRenderTransformPropertyChanged;
        }
    }

    /// <inheritdoc/>
    protected override void DisconnectHandler(PlatformView platformView)
    {
        if (VirtualView?.RenderTransform != null)
        {
            VirtualView.RenderTransform.PropertyChanged -= OnRenderTransformPropertyChanged;
        }

        base.DisconnectHandler(platformView);
    }

    private void OnRenderTransformPropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        MapRenderTransform(this, VirtualView);
    }

    /// <summary>Maps the Shape property to the platform view.</summary>
    /// <param name="handler">The shape view handler.</param>
    /// <param name="shape">The shape view.</param>
    public static new void MapShape(IShapeViewHandler handler, IShapeView shape)
    {
        MapData(handler, shape as Path);
    }

    /// <summary>Maps the Data property to the platform view.</summary>
    /// <param name="handler">The shape view handler.</param>
    /// <param name="path">The path.</param>
    public static void MapData(IShapeViewHandler handler, Path path)
    {
        if (handler.PlatformView is PlatformView platformView)
        {
            platformView.UpdateData(path);
        }
    }

    /// <summary>Maps the RenderTransform property to the platform view.</summary>
    /// <param name="handler">The shape view handler.</param>
    /// <param name="path">The path.</param>
    public static void MapRenderTransform(IShapeViewHandler handler, Path path)
    {
        if (handler.PlatformView is PlatformView platformView)
        {
            platformView.UpdateRenderTransform(path);
        }
    }
}
