#nullable disable
using Microsoft.Maui;
using Microsoft.Maui.Controls.Shapes;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Handlers;
using Avalonia.Controls.Maui.Extensions;
using PlatformView = global::Avalonia.Controls.Shapes.Path;
using Path = Microsoft.Maui.Controls.Shapes.Path;

namespace Avalonia.Controls.Maui.Handlers.Shapes;

public partial class PathHandler : ShapeViewHandler<Path, PlatformView>
{
    public static new IPropertyMapper<Path, IShapeViewHandler> Mapper = new PropertyMapper<Path, IShapeViewHandler>(ShapeViewHandler.Mapper)
    {
        [nameof(IShapeView.Shape)] = MapShape,
        [nameof(Path.Data)] = MapData,
        [nameof(Path.RenderTransform)] = MapRenderTransform,
    };

    public PathHandler() : base(Mapper)
    {
    }

    public PathHandler(IPropertyMapper mapper) : base(mapper ?? Mapper)
    {
    }

    protected override PlatformView CreatePlatformView()
    {
        return new PlatformView();
    }

    protected override void ConnectHandler(PlatformView platformView)
    {
        base.ConnectHandler(platformView);

        if (VirtualView?.RenderTransform != null)
        {
            VirtualView.RenderTransform.PropertyChanged += OnRenderTransformPropertyChanged;
        }
    }

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

    public static new void MapShape(IShapeViewHandler handler, IShapeView shape)
    {
        MapData(handler, shape as Path);
    }

    public static void MapData(IShapeViewHandler handler, Path path)
    {
        if (handler.PlatformView is PlatformView platformView && path?.Data != null)
        {
            platformView.Data = path.Data.ToAvaloniaGeometry();
        }
    }

    public static void MapRenderTransform(IShapeViewHandler handler, Path path)
    {
        if (handler.PlatformView is not PlatformView platformView)
            return;

        if (path?.RenderTransform != null)
        {
            var matrix = path.RenderTransform.Value;

            // Convert MAUI Transform to Avalonia Transform
            if (path.RenderTransform is Microsoft.Maui.Controls.Shapes.RotateTransform rotate)
            {
                platformView.RenderTransform = new global::Avalonia.Media.RotateTransform
                {
                    Angle = rotate.Angle
                };
                platformView.RenderTransformOrigin = new global::Avalonia.RelativePoint(
                    rotate.CenterX, rotate.CenterY, global::Avalonia.RelativeUnit.Absolute);
            }
            else
            {
                // Fallback to matrix transform
                platformView.RenderTransform = new global::Avalonia.Media.MatrixTransform(
                    new global::Avalonia.Matrix(
                        matrix.M11, matrix.M12,
                        matrix.M21, matrix.M22,
                        matrix.OffsetX, matrix.OffsetY));
            }
        }
        else
        {
            platformView.RenderTransform = null;
        }
    }
}