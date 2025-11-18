using Avalonia.Headless.XUnit;
using Avalonia.Controls.Maui.Platform;
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

        await ValidatePropertyInitValue<Microsoft.Maui.Thickness>(
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
        var dashPattern = new float[] { 5f, 2f, 3f, 2f };
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

    // Platform-specific property getters
    Color? GetNativeStrokeColor(MauiBorderHandler handler)
    {
        if (handler.PlatformView is not BorderView borderView)
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
        return handler.PlatformView is BorderView borderView ? borderView.StrokeThickness : 0;
    }

    Color? GetNativeBackgroundColor(MauiBorderHandler handler) =>
        AvaloniaPropertyHelpers.GetNativeBackgroundColor(handler);

    Microsoft.Maui.Thickness GetNativePadding(MauiBorderHandler handler) =>
        AvaloniaPropertyHelpers.GetNativePadding(handler);

    IShape? GetNativeShape(MauiBorderHandler handler)
    {
        return handler.PlatformView is BorderView borderView ? borderView.Shape : null;
    }

    float[]? GetNativeStrokeDashPattern(MauiBorderHandler handler)
    {
        return handler.PlatformView is BorderView borderView ? borderView.StrokeDashPattern : null;
    }

    float GetNativeStrokeDashOffset(MauiBorderHandler handler)
    {
        return handler.PlatformView is BorderView borderView ? borderView.StrokeDashOffset : 0f;
    }

    LineCap GetNativeStrokeLineCap(MauiBorderHandler handler)
    {
        return handler.PlatformView is BorderView borderView ? borderView.StrokeLineCap : LineCap.Butt;
    }

    LineJoin GetNativeStrokeLineJoin(MauiBorderHandler handler)
    {
        return handler.PlatformView is BorderView borderView ? borderView.StrokeLineJoin : LineJoin.Miter;
    }

    float GetNativeStrokeMiterLimit(MauiBorderHandler handler)
    {
        return handler.PlatformView is BorderView borderView ? borderView.StrokeMiterLimit : 0f;
    }
}
