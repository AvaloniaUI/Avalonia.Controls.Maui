using Avalonia.Headless.XUnit;
using Microsoft.Maui.Controls.Shapes;
using PathStub = Avalonia.Controls.Maui.Tests.Stubs.PathStub;
using MauiPathHandler = Avalonia.Controls.Maui.Tests.Stubs.PathStubHandler;

namespace Avalonia.Controls.Maui.Tests.Handlers;

public class PathHandlerTests : HandlerTestBase<MauiPathHandler, PathStub>
{
    [AvaloniaFact(DisplayName = "Path Data Maps Correctly")]
    public async Task PathDataMapsCorrectly()
    {
        var path = new PathStub
        {
            Data = new PathGeometry
            {
                Figures = new PathFigureCollection
                {
                    new PathFigure
                    {
                        StartPoint = new Microsoft.Maui.Graphics.Point(0, 0),
                        Segments = new PathSegmentCollection
                        {
                            new LineSegment { Point = new Microsoft.Maui.Graphics.Point(50, 0) },
                            new LineSegment { Point = new Microsoft.Maui.Graphics.Point(50, 50) },
                            new LineSegment { Point = new Microsoft.Maui.Graphics.Point(0, 50) },
                            new LineSegment { Point = new Microsoft.Maui.Graphics.Point(0, 0) }
                        }
                    }
                }
            }
        };

        var handler = await CreateHandlerAsync(path);

        Assert.NotNull(handler.PlatformView.Data);
    }
    
    [AvaloniaFact(DisplayName = "Render Transform Maps Correctly")]
    public async Task RenderTransformMapsCorrectly()
    {
        var path = new PathStub
        {
            Data = new PathGeometry
            {
                Figures = new PathFigureCollection
                {
                    new PathFigure
                    {
                        StartPoint = new Microsoft.Maui.Graphics.Point(0, 0),
                        Segments = new PathSegmentCollection
                        {
                            new LineSegment { Point = new Microsoft.Maui.Graphics.Point(10, 10) }
                        }
                    }
                }
            },
            RenderTransform = new RotateTransform
            {
                Angle = 45,
                CenterX = 5,
                CenterY = 5
            }
        };

        var handler = await CreateHandlerAsync(path);

        Assert.IsType<global::Avalonia.Media.RotateTransform>(handler.PlatformView.RenderTransform);
    }
}
