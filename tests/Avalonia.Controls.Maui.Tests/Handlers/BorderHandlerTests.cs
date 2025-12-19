using Avalonia.Headless.XUnit;
using Avalonia.Controls.Maui.Tests.Stubs;
using Avalonia.Controls.Maui.Tests.TestUtilities;
using Microsoft.Maui;
using Microsoft.Maui.Controls.Shapes;
using Microsoft.Maui.Graphics;
using MauiBorderHandler = Avalonia.Controls.Maui.Handlers.BorderHandler;

namespace Avalonia.Controls.Maui.Tests.Handlers;

public partial class BorderHandlerTests : HandlerTestBase<MauiBorderHandler, BorderStub>
{
    [AvaloniaFact(DisplayName = "Stroke Initializes Correctly")]
    public async Task StrokeInitializesCorrectly()
    {
        var color = Colors.Red;
        var border = new BorderStub
        {
            Stroke = new SolidPaint(color),
            StrokeThickness = 1.0
        };

        var platformColor = await GetValueAsync(border, GetNativeStrokeColor);

        Assert.NotNull(platformColor);
        ColorComparisonHelpers.AssertColorsAreEqual(color, platformColor);
    }

    [AvaloniaFact(DisplayName = "Stroke Thickness Initializes Correctly")]
    public async Task StrokeThicknessInitializesCorrectly()
    {
        var border = new BorderStub
        {
            StrokeThickness = 5.0
        };

        await ValidatePropertyInitValue(
            border,
            () => border.StrokeThickness,
            GetNativeStrokeThickness,
            border.StrokeThickness);
    }

    [AvaloniaFact(DisplayName = "Background Initializes Correctly")]
    public async Task BackgroundInitializesCorrectly()
    {
        var color = Colors.Blue;
        var border = new BorderStub
        {
            Background = new SolidPaint(color)
        };

        var platformColor = await GetValueAsync(border, GetNativeBackgroundColor);

        Assert.NotNull(platformColor);
        ColorComparisonHelpers.AssertColorsAreEqual(color, platformColor);
    }

    [AvaloniaFact(DisplayName = "Padding Initializes Correctly")]
    public async Task PaddingInitializesCorrectly()
    {
        var border = new BorderStub
        {
            Padding = new Microsoft.Maui.Thickness(10, 20, 30, 40)
        };

        await ValidatePropertyInitValue(
            border,
            () => border.Padding,
            GetNativePadding,
            border.Padding);
    }

    [AvaloniaFact(DisplayName = "Stroke Shape Initializes Correctly")]
    public async Task StrokeShapeInitializesCorrectly()
    {
        var roundRect = new RoundRectangle
        {
            CornerRadius = new Microsoft.Maui.CornerRadius(10)
        };

        var border = new BorderStub
        {
            Shape = roundRect
        };

        var platformShape = await GetValueAsync(border, GetNativeShape);

        Assert.NotNull(platformShape);
        Assert.Equal(roundRect, platformShape);
    }

    [AvaloniaTheory(DisplayName = "Stroke Thickness Updates Correctly")]
    [InlineData(1.0, 5.0)]
    [InlineData(5.0, 1.0)]
    [InlineData(0.0, 10.0)]
    public async Task StrokeThicknessUpdatesCorrectly(double initialThickness, double newThickness)
    {
        var border = new BorderStub
        {
            StrokeThickness = initialThickness
        };

        await ValidatePropertyUpdatesValue(
            border,
            nameof(IBorderStroke.StrokeThickness),
            GetNativeStrokeThickness,
            newThickness,
            initialThickness);
    }

    [AvaloniaFact(DisplayName = "Stroke Dash Pattern Initializes Correctly")]
    public async Task StrokeDashPatternInitializesCorrectly()
    {
        var dashPattern = new[] { 5f, 2f, 3f, 2f };
        var border = new BorderStub
        {
            StrokeDashPattern = dashPattern
        };

        var platformDashPattern = await GetValueAsync(border, GetNativeStrokeDashPattern);

        Assert.NotNull(platformDashPattern);
        Assert.Equal(dashPattern, platformDashPattern);
    }

    [AvaloniaFact(DisplayName = "Stroke Dash Offset Initializes Correctly")]
    public async Task StrokeDashOffsetInitializesCorrectly()
    {
        var border = new BorderStub
        {
            StrokeDashOffset = 3.5f
        };

        await ValidatePropertyInitValue(
            border,
            () => border.StrokeDashOffset,
            GetNativeStrokeDashOffset,
            border.StrokeDashOffset);
    }

    [AvaloniaTheory(DisplayName = "Stroke Line Cap Initializes Correctly")]
    [InlineData(LineCap.Butt)]
    [InlineData(LineCap.Round)]
    [InlineData(LineCap.Square)]
    public async Task StrokeLineCapInitializesCorrectly(LineCap lineCap)
    {
        var border = new BorderStub
        {
            StrokeLineCap = lineCap
        };

        await ValidatePropertyInitValue(
            border,
            () => border.StrokeLineCap,
            GetNativeStrokeLineCap,
            border.StrokeLineCap);
    }

    [AvaloniaTheory(DisplayName = "Stroke Line Join Initializes Correctly")]
    [InlineData(LineJoin.Miter)]
    [InlineData(LineJoin.Round)]
    [InlineData(LineJoin.Bevel)]
    public async Task StrokeLineJoinInitializesCorrectly(LineJoin lineJoin)
    {
        var border = new BorderStub
        {
            StrokeLineJoin = lineJoin
        };

        await ValidatePropertyInitValue(
            border,
            () => border.StrokeLineJoin,
            GetNativeStrokeLineJoin,
            border.StrokeLineJoin);
    }

    [AvaloniaFact(DisplayName = "Stroke Miter Limit Initializes Correctly")]
    public async Task StrokeMiterLimitInitializesCorrectly()
    {
        var border = new BorderStub
        {
            StrokeMiterLimit = 8f
        };

        await ValidatePropertyInitValue(
            border,
            () => border.StrokeMiterLimit,
            GetNativeStrokeMiterLimit,
            border.StrokeMiterLimit);
    }

    [AvaloniaFact(DisplayName = "Null Stroke Sets Null Brush")]
    public async Task NullStrokeSetsNullBrush()
    {
        var border = new BorderStub
        {
            Stroke = null
        };

        var platformStroke = await GetValueAsync(border, GetNativeStrokeColor);

        Assert.Null(platformStroke);
    }

    [AvaloniaFact(DisplayName = "Null Background Sets Null Brush")]
    public async Task NullBackgroundSetsNullBrush()
    {
        var border = new BorderStub
        {
            Background = null
        };

        var platformBackground = await GetValueAsync(border, GetNativeBackgroundColor);

        Assert.Null(platformBackground);
    }

    [AvaloniaFact(DisplayName = "Content Initializes Correctly")]
    public async Task ContentInitializesCorrectly()
    {
        var content = new Microsoft.Maui.Controls.Label { Text = "Test Content" };
        var border = new BorderStub
        {
            Content = content
        };

        var platformView = await GetValueAsync(border, handler =>
            handler.PlatformView);

        Assert.NotNull(platformView);
    }

    [AvaloniaFact(DisplayName = "Ellipse Shape Initializes Correctly")]
    public async Task EllipseShapeInitializesCorrectly()
    {
        var ellipse = new Ellipse();

        var border = new BorderStub
        {
            Shape = ellipse
        };

        var platformShape = await GetValueAsync(border, GetNativeShape);

        Assert.NotNull(platformShape);
        Assert.Equal(ellipse, platformShape);
    }

    [AvaloniaFact(DisplayName = "Polygon Shape Initializes Correctly")]
    public async Task PolygonShapeInitializesCorrectly()
    {
        var polygon = new Microsoft.Maui.Controls.Shapes.Polygon
        {
            Points = new Microsoft.Maui.Controls.PointCollection
            {
                new Microsoft.Maui.Graphics.Point(0, 0),
                new Microsoft.Maui.Graphics.Point(50, 100),
                new Microsoft.Maui.Graphics.Point(100, 0)
            }
        };

        var border = new BorderStub
        {
            Shape = polygon
        };

        var platformShape = await GetValueAsync(border, GetNativeShape);

        Assert.NotNull(platformShape);
        Assert.Equal(polygon, platformShape);
    }

    [AvaloniaFact(DisplayName = "Gradient Stroke Initializes Correctly")]
    public async Task GradientStrokeInitializesCorrectly()
    {
        var gradientPaint = new LinearGradientPaint
        {
            StartColor = Colors.Red,
            EndColor = Colors.Blue
        };

        var border = new BorderStub
        {
            Stroke = gradientPaint,
            StrokeThickness = 4.0
        };

        var platformView = await GetValueAsync(border, handler =>
            handler.PlatformView);

        Assert.NotNull(platformView);
        Assert.NotNull(platformView.Stroke);
        Assert.IsType<Avalonia.Media.LinearGradientBrush>(platformView.Stroke);
    }

    [AvaloniaFact(DisplayName = "Stroke Color Updates Correctly")]
    public async Task StrokeColorUpdatesCorrectly()
    {
        var initialColor = Colors.Black;
        var newColor = Colors.Red;
        
        var border = new BorderStub
        {
            Stroke = new SolidPaint(initialColor),
            StrokeThickness = 2.0
        };

        var handler = await CreateHandlerAsync(border);
        var initialNative = GetNativeStrokeColor(handler);
        Assert.NotNull(initialNative);
        
        border.Stroke = new SolidPaint(newColor);
        handler.UpdateValue(nameof(IBorderStroke.Stroke));
        
        var newNative = GetNativeStrokeColor(handler);
        Assert.NotNull(newNative);
        ColorComparisonHelpers.AssertColorsAreEqual(newColor, newNative);
    }

    [AvaloniaFact(DisplayName = "Background Updates Correctly")]
    public async Task BackgroundUpdatesCorrectly()
    {
        var initialColor = Colors.White;
        var newColor = Colors.LightBlue;

        var border = new BorderStub
        {
            Background = new SolidPaint(initialColor)
        };

        var handler = await CreateHandlerAsync(border);
        var initialNative = GetNativeBackgroundColor(handler);
        Assert.NotNull(initialNative);
        
        border.Background = new SolidPaint(newColor);
        handler.UpdateValue(nameof(IBorderView.Background));
        
        var newNative = GetNativeBackgroundColor(handler);
        Assert.NotNull(newNative);
        ColorComparisonHelpers.AssertColorsAreEqual(newColor, newNative);
    }

    [AvaloniaFact(DisplayName = "Asymmetric Corner Radius Initializes Correctly")]
    public async Task AsymmetricCornerRadiusInitializesCorrectly()
    {
        var roundRect = new RoundRectangle
        {
            CornerRadius = new Microsoft.Maui.CornerRadius(5, 10, 15, 20)
        };

        var border = new BorderStub
        {
            Shape = roundRect
        };

        var platformShape = await GetValueAsync(border, GetNativeShape);

        Assert.NotNull(platformShape);
        Assert.Equal(roundRect, platformShape);
    }

    [AvaloniaFact(DisplayName = "Shape Updates Correctly")]
    public async Task ShapeUpdatesCorrectly()
    {
        var initialShape = new RoundRectangle
        {
            CornerRadius = new Microsoft.Maui.CornerRadius(10)
        };

        var border = new BorderStub
        {
            Shape = initialShape
        };

        var handler = await CreateHandlerAsync(border);
        
        var initialNative = GetNativeShape(handler);
        Assert.Equal(initialShape, initialNative);

        var newShape = new RoundRectangle
        {
            CornerRadius = new Microsoft.Maui.CornerRadius(30)
        };
        border.Shape = newShape;
        handler.UpdateValue(nameof(IBorderStroke.Shape));

        var updatedNative = GetNativeShape(handler);
        Assert.Equal(newShape, updatedNative);
    }

    [AvaloniaFact(DisplayName = "Padding Updates Correctly")]
    public async Task PaddingUpdatesCorrectly()
    {
        var initialPadding = new Microsoft.Maui.Thickness(5);

        var border = new BorderStub
        {
            Padding = initialPadding
        };

        var handler = await CreateHandlerAsync(border);
        
        var initialNative = GetNativePadding(handler);
        Assert.Equal(initialPadding, initialNative);

        var newPadding = new Microsoft.Maui.Thickness(20, 10, 15, 25);
        border.Padding = newPadding;
        handler.UpdateValue(nameof(IBorderView.Padding));

        var updatedNative = GetNativePadding(handler);
        Assert.Equal(newPadding, updatedNative);
    }

    [AvaloniaFact(DisplayName = "Zero Stroke Thickness Initializes Correctly")]
    public async Task ZeroStrokeThicknessInitializesCorrectly()
    {
        var border = new BorderStub
        {
            StrokeThickness = 0
        };

        await ValidatePropertyInitValue(
            border,
            () => border.StrokeThickness,
            GetNativeStrokeThickness,
            0.0);
    }

    [AvaloniaFact(DisplayName = "Stroke Dash Pattern Updates Correctly")]
    public async Task StrokeDashPatternUpdatesCorrectly()
    {
        var initialPattern = new[] { 2f, 2f };

        var border = new BorderStub
        {
            StrokeDashPattern = initialPattern
        };

        var handler = await CreateHandlerAsync(border);
        
        var initialNative = GetNativeStrokeDashPattern(handler);
        Assert.Equal(initialPattern, initialNative);

        var newPattern = new[] { 5f, 3f, 1f, 3f };
        border.StrokeDashPattern = newPattern;
        handler.UpdateValue(nameof(IBorderStroke.StrokeDashPattern));

        var updatedNative = GetNativeStrokeDashPattern(handler);
        Assert.Equal(newPattern, updatedNative);
    }

    [AvaloniaFact(DisplayName = "Asymmetric Padding Initializes Correctly")]
    public async Task AsymmetricPaddingInitializesCorrectly()
    {
        var padding = new Microsoft.Maui.Thickness(5, 10, 15, 20);
        var border = new BorderStub
        {
            Padding = padding
        };

        await ValidatePropertyInitValue(
            border,
            () => border.Padding,
            GetNativePadding,
            padding);
    }

    [AvaloniaTheory(DisplayName = "Stroke Dash Offset Updates Correctly")]
    [InlineData(0f, 5f)]
    [InlineData(5f, 0f)]
    [InlineData(2.5f, 7.5f)]
    public async Task StrokeDashOffsetUpdatesCorrectly(float initial, float updated)
    {
        var border = new BorderStub
        {
            StrokeDashOffset = initial,
            StrokeDashPattern = [4f, 2f]
        };

        await ValidatePropertyUpdatesValue(
            border,
            nameof(IBorderStroke.StrokeDashOffset),
            GetNativeStrokeDashOffset,
            updated,
            initial);
    }

    [AvaloniaTheory(DisplayName = "Stroke Miter Limit Updates Correctly")]
    [InlineData(1f, 10f)]
    [InlineData(10f, 1f)]
    [InlineData(5f, 20f)]
    public async Task StrokeMiterLimitUpdatesCorrectly(float initial, float updated)
    {
        var border = new BorderStub
        {
            StrokeMiterLimit = initial
        };

        await ValidatePropertyUpdatesValue(
            border,
            nameof(IBorderStroke.StrokeMiterLimit),
            GetNativeStrokeMiterLimit,
            updated,
            initial);
    }

    Color? GetNativeStrokeColor(MauiBorderHandler handler)
    {
        if (handler.PlatformView is not MauiBorder borderView)
            return null;

        if (borderView.Stroke is Avalonia.Media.SolidColorBrush brush)
        {
            return new Color(
                brush.Color.R / 255f,
                brush.Color.G / 255f,
                brush.Color.B / 255f,
                brush.Color.A / 255f);
        }

        return null;
    }

    double GetNativeStrokeThickness(MauiBorderHandler handler)
    {
        return handler.PlatformView is MauiBorder borderView ? borderView.StrokeThickness : 0;
    }

    Color? GetNativeBackgroundColor(MauiBorderHandler handler) =>
        AvaloniaPropertyHelpers.GetNativeBackgroundColor(handler);

    Microsoft.Maui.Thickness GetNativePadding(MauiBorderHandler handler) =>
        AvaloniaPropertyHelpers.GetNativePadding(handler);

    IShape? GetNativeShape(MauiBorderHandler handler)
    {
        return handler.PlatformView is MauiBorder borderView ? borderView.Shape : null;
    }

    float[]? GetNativeStrokeDashPattern(MauiBorderHandler handler)
    {
        return handler.PlatformView is MauiBorder borderView ? borderView.StrokeDashPattern : null;
    }

    float GetNativeStrokeDashOffset(MauiBorderHandler handler)
    {
        return handler.PlatformView is MauiBorder borderView ? borderView.StrokeDashOffset : 0f;
    }

    LineCap GetNativeStrokeLineCap(MauiBorderHandler handler)
    {
        return handler.PlatformView is MauiBorder borderView ? borderView.StrokeLineCap : LineCap.Butt;
    }

    LineJoin GetNativeStrokeLineJoin(MauiBorderHandler handler)
    {
        return handler.PlatformView is MauiBorder borderView ? borderView.StrokeLineJoin : LineJoin.Miter;
    }

    float GetNativeStrokeMiterLimit(MauiBorderHandler handler)
    {
        return handler.PlatformView is MauiBorder borderView ? borderView.StrokeMiterLimit : 0f;
    }
}

