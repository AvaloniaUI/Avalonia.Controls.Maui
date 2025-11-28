using Avalonia.Headless.XUnit;
using Avalonia.Controls.Maui.Tests.TestUtilities;
using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;
using MauiRectangleHandler = Avalonia.Controls.Maui.Tests.Stubs.RectangleStubHandler;
using RectangleStub = Avalonia.Controls.Maui.Tests.Stubs.RectangleStub;

namespace Avalonia.Controls.Maui.Tests.Handlers;

public class ShapeViewHandlerTests : HandlerTestBase<MauiRectangleHandler, RectangleStub>
{
    [AvaloniaFact(DisplayName = "Fill Maps To Platform Brush")]
    public async Task FillMapsToPlatformBrush()
    {
        var shape = new RectangleStub
        {
            Fill = new SolidColorBrush(Colors.Blue)
        };

        var handler = await CreateHandlerAsync(shape);
        var fillColor = GetPlatformBrushColor(handler.PlatformView.Fill);

        ColorComparisonHelpers.AssertColorsAreEqual(Colors.Blue, fillColor);
    }
    
    [AvaloniaFact(DisplayName = "Stroke Maps To Platform Brush")]
    public async Task StrokeMapsToPlatformBrush()
    {
        var shape = new RectangleStub
        {
            Stroke = new SolidColorBrush(Colors.Red),
            StrokeThickness = 4
        };

        var handler = await CreateHandlerAsync(shape);
        var strokeColor = GetPlatformBrushColor(handler.PlatformView.Stroke);

        ColorComparisonHelpers.AssertColorsAreEqual(Colors.Red, strokeColor);
        Assert.Equal(4, handler.PlatformView.StrokeThickness);
    }
    
    [AvaloniaFact(DisplayName = "Aspect Maps To Stretch")]
    public async Task AspectMapsToStretch()
    {
        var shape = new RectangleStub
        {
            Aspect = PathAspect.AspectFit
        };

        var handler = await CreateHandlerAsync(shape);

        Assert.Equal(global::Avalonia.Media.Stretch.Uniform, handler.PlatformView.Stretch);
    }
    
    [AvaloniaFact(DisplayName = "Stroke Dash Pattern Maps Correctly")]
    public async Task StrokeDashPatternMapsCorrectly()
    {
        var shape = new RectangleStub
        {
            StrokeDashPattern = [2f, 1f],
            StrokeDashOffset = 3
        };

        var handler = await CreateHandlerAsync(shape);

        Assert.NotNull(handler.PlatformView.StrokeDashArray);
        Assert.Equal(new[] { 2d, 1d }, handler.PlatformView.StrokeDashArray);
        Assert.Equal(3, handler.PlatformView.StrokeDashOffset);
    }
    
    [AvaloniaTheory(DisplayName = "Stroke Cap Maps Correctly")]
    [InlineData(LineCap.Butt, Media.PenLineCap.Flat)]
    [InlineData(LineCap.Round, Media.PenLineCap.Round)]
    [InlineData(LineCap.Square, Media.PenLineCap.Square)]
    public async Task StrokeCapMapsCorrectly(LineCap cap, Media.PenLineCap expected)
    {
        var shape = new RectangleStub
        {
            StrokeLineCap = cap
        };

        var handler = await CreateHandlerAsync(shape);

        Assert.Equal(expected, handler.PlatformView.StrokeLineCap);
    }
    
    [AvaloniaTheory(DisplayName = "Stroke Join Maps Correctly")]
    [InlineData(LineJoin.Miter, Media.PenLineJoin.Miter)]
    [InlineData(LineJoin.Round, Media.PenLineJoin.Round)]
    [InlineData(LineJoin.Bevel, Media.PenLineJoin.Bevel)]
    public async Task StrokeJoinMapsCorrectly(LineJoin join, Media.PenLineJoin expected)
    {
        var shape = new RectangleStub
        {
            StrokeLineJoin = join
        };

        var handler = await CreateHandlerAsync(shape);

        Assert.Equal(expected, handler.PlatformView.StrokeJoin);
    }

    private static Color? GetPlatformBrushColor(global::Avalonia.Media.IBrush? brush)
    {
        if (brush is global::Avalonia.Media.ISolidColorBrush solidColorBrush)
        {
            var c = solidColorBrush.Color;
            return Color.FromRgba(c.R, c.G, c.B, c.A);
        }

        return null;
    }
}
