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

    [AvaloniaFact(DisplayName = "Border Grows When Hidden Content Is Shown")]
    public async Task BorderGrowsWhenHiddenContentIsShown()
    {
        var details = new Microsoft.Maui.Controls.ContentView
        {
            Content = new Microsoft.Maui.Controls.BoxView { HeightRequest = 50 },
            IsVisible = false,
        };
        var grid = new Microsoft.Maui.Controls.Grid
        {
            RowDefinitions =
            {
                new Microsoft.Maui.Controls.RowDefinition(Microsoft.Maui.GridLength.Auto),
                new Microsoft.Maui.Controls.RowDefinition(Microsoft.Maui.GridLength.Auto),
            }
        };
        var header = new Microsoft.Maui.Controls.BoxView { HeightRequest = 20 };
        grid.Add(header);
        grid.Add(details);
        grid.SetRow(details, 1);
        var border = new Microsoft.Maui.Controls.Border
        {
            Margin = new Microsoft.Maui.Thickness(10),
            Padding = new Microsoft.Maui.Thickness(5),
            Content = new Microsoft.Maui.Controls.ContentView { Content = grid }
        };
        var page = new Microsoft.Maui.Controls.VerticalStackLayout
        {
            Children = { border }
        };

        await InvokeOnMainThreadAsync(() =>
        {
            var handler = CreateHandler<Avalonia.Controls.Maui.Handlers.LayoutHandler>(page);
            var window = new Window { Width = 200, Height = 200, Content = handler.PlatformView };
            window.Show();
            try
            {
                window.UpdateLayout();
                var platformBorder = (Control)border.Handler!.PlatformView!;
                var collapsedHeight = platformBorder.Bounds.Height;

                details.IsVisible = true;
                window.UpdateLayout();

                Assert.Equal(collapsedHeight + 50, platformBorder.Bounds.Height, 1);

                details.IsVisible = false;
                window.UpdateLayout();

                Assert.Equal(collapsedHeight, platformBorder.Bounds.Height, 1);
            }
            finally
            {
                window.Close();
            }
        });
    }

    [AvaloniaFact(DisplayName = "Content Is Arranged Inside The Stroke And Padding")]
    public async Task ContentIsArrangedInsideStrokeAndPadding()
    {
        var content = new Microsoft.Maui.Controls.BoxView();
        var border = new Microsoft.Maui.Controls.Border
        {
            WidthRequest = 200,
            HeightRequest = 100,
            StrokeThickness = 4,
            Padding = new Microsoft.Maui.Thickness(1, 2, 3, 5),
            Content = content,
        };

        var bounds = await RunBorderInWindowAsync(border, content);

        Assert.Equal(new Rect(5, 6, 200 - 4 - 4 - 1 - 3, 100 - 4 - 4 - 2 - 5), bounds);
    }

    [AvaloniaFact(DisplayName = "Border Sized To Its Content Includes The Stroke")]
    public async Task BorderSizedToContentIncludesStroke()
    {
        var content = new Microsoft.Maui.Controls.BoxView { WidthRequest = 50, HeightRequest = 30 };
        var border = new Microsoft.Maui.Controls.Border
        {
            StrokeThickness = 4,
            Padding = 10,
            HorizontalOptions = Microsoft.Maui.Controls.LayoutOptions.Start,
            VerticalOptions = Microsoft.Maui.Controls.LayoutOptions.Start,
            Content = content,
        };

        var bounds = await RunBorderInWindowAsync(border, content);

        Assert.Equal(new Rect(14, 14, 50, 30), bounds);
        Assert.Equal(50 + 2 * 10 + 2 * 4, ((Control)border.Handler!.PlatformView!).Bounds.Width, 1);
        Assert.Equal(30 + 2 * 10 + 2 * 4, ((Control)border.Handler!.PlatformView!).Bounds.Height, 1);
    }

    /// <summary>
    /// Lays out <paramref name="border"/> in a window and returns the bounds of
    /// <paramref name="content"/> relative to the border.
    /// </summary>
    Task<Rect> RunBorderInWindowAsync(Microsoft.Maui.Controls.Border border, Microsoft.Maui.Controls.View content) =>
        InvokeOnMainThreadAsync(() =>
        {
            var page = new Microsoft.Maui.Controls.Grid { Children = { border } };
            var handler = CreateHandler<Avalonia.Controls.Maui.Handlers.LayoutHandler>(page);
            var window = new Window { Width = 300, Height = 300, Content = handler.PlatformView };
            window.Show();
            try
            {
                window.UpdateLayout();
                var platformBorder = (Control)border.Handler!.PlatformView!;
                var platformContent = (Control)content.Handler!.PlatformView!;
                var topLeft = platformContent.TranslatePoint(default, platformBorder)!.Value;
                return new Rect(topLeft, platformContent.Bounds.Size);
            }
            finally
            {
                window.Close();
            }
        });

    [AvaloniaFact(DisplayName = "Content Is Clipped Inside The Stroke")]
    public async Task ContentIsClippedInsideStroke()
    {
        // Content that fills the border must not paint over the stroke
        var pixels = await RenderBorderAsync(new Rectangle());

        Assert.Equal(Avalonia.Media.Colors.Red, pixels(3, 50));
        Assert.Equal(Avalonia.Media.Colors.Red, pixels(50, 3));
        Assert.Equal(Avalonia.Media.Colors.Lime, pixels(50, 50));
    }

    [AvaloniaFact(DisplayName = "Content Is Clipped To The Inner Corners Of A Round Rectangle")]
    public async Task ContentIsClippedToInnerCornersOfRoundRectangle()
    {
        // With a corner radius of 30 and a stroke of 6, the inner corner has a radius of 24 around
        // (30, 30). (10, 10) is inside the outer corner but outside the inner one.
        var pixels = await RenderBorderAsync(new RoundRectangle { CornerRadius = 30 });

        Assert.NotEqual(Avalonia.Media.Colors.Lime, pixels(10, 10));
        Assert.Equal(Avalonia.Media.Colors.Lime, pixels(30, 30));
        Assert.Equal(Avalonia.Media.Colors.Red, pixels(3, 50));
    }

    [AvaloniaFact(DisplayName = "Content Does Not Cover The Stroke At Rounded Corners")]
    public async Task ContentDoesNotCoverStrokeAtRoundedCorners()
    {
        // Every pixel of the stroke drawn around an empty border must still be stroke once the
        // border has content, including where the corners curve. Antialiasing where the stroke
        // meets the content may tint a pixel slightly, but must not turn it to the content color.
        var empty = await RenderBorderAsync(new RoundRectangle { CornerRadius = 30 }, border => border.Content = null);
        var filled = await RenderBorderAsync(new RoundRectangle { CornerRadius = 30 });

        for (var y = 0; y < 100; y++)
        {
            for (var x = 0; x < 100; x++)
            {
                if (empty(x, y) == Avalonia.Media.Colors.Red)
                    Assert.True(filled(x, y) is { R: > 200, G: < 64 }, $"Content covers the stroke at ({x}, {y}): {filled(x, y)}.");
            }
        }
    }

    [AvaloniaFact(DisplayName = "Background Fills To The Edge Without A Stroke Brush")]
    public async Task BackgroundFillsToEdgeWithoutStrokeBrush()
    {
        var pixels = await RenderBorderAsync(new Rectangle(), border =>
        {
            border.Stroke = null;
            border.Background = Microsoft.Maui.Controls.Brush.Blue;
            border.Content = null;
        });

        Assert.Equal(Avalonia.Media.Colors.Blue, pixels(0, 50));
        Assert.Equal(Avalonia.Media.Colors.Blue, pixels(50, 0));
        Assert.Equal(Avalonia.Media.Colors.Blue, pixels(99, 50));
        Assert.Equal(Avalonia.Media.Colors.Blue, pixels(50, 99));
    }

    /// <summary>
    /// Renders a 100x100 border with a 6-wide red stroke and lime content that fills it, and returns
    /// a function that reads the color of a pixel of the border.
    /// </summary>
    async Task<Func<int, int, Avalonia.Media.Color>> RenderBorderAsync(IShape shape, Action<Microsoft.Maui.Controls.Border>? configure = null)
    {
        var border = new Microsoft.Maui.Controls.Border
        {
            WidthRequest = 100,
            HeightRequest = 100,
            HorizontalOptions = Microsoft.Maui.Controls.LayoutOptions.Start,
            VerticalOptions = Microsoft.Maui.Controls.LayoutOptions.Start,
            Stroke = Microsoft.Maui.Graphics.Colors.Red,
            StrokeThickness = 6,
            StrokeShape = (Microsoft.Maui.Controls.Shapes.Shape)shape,
            Padding = 0,
            Content = new Microsoft.Maui.Controls.BoxView { Color = Microsoft.Maui.Graphics.Colors.Lime },
        };
        configure?.Invoke(border);
        var page = new Microsoft.Maui.Controls.Grid { Children = { border } };

        var colors = await InvokeOnMainThreadAsync(() =>
        {
            var handler = CreateHandler<Avalonia.Controls.Maui.Handlers.LayoutHandler>(page);
            var window = new Window
            {
                Width = 120,
                Height = 120,
                Background = Avalonia.Media.Brushes.White,
                Content = handler.PlatformView,
            };
            window.Show();
            try
            {
                window.UpdateLayout();
                using var frame = Avalonia.Headless.HeadlessWindowExtensions.CaptureRenderedFrame(window)!;
                using var locked = frame.Lock();
                var isBgra = locked.Format == Avalonia.Platform.PixelFormat.Bgra8888;
                var colors = new Avalonia.Media.Color[100, 100];
                for (var y = 0; y < 100; y++)
                {
                    for (var x = 0; x < 100; x++)
                    {
                        var offset = y * locked.RowBytes + x * 4;
                        var first = System.Runtime.InteropServices.Marshal.ReadByte(locked.Address, offset);
                        var g = System.Runtime.InteropServices.Marshal.ReadByte(locked.Address, offset + 1);
                        var third = System.Runtime.InteropServices.Marshal.ReadByte(locked.Address, offset + 2);
                        colors[x, y] = isBgra
                            ? Avalonia.Media.Color.FromRgb(third, g, first)
                            : Avalonia.Media.Color.FromRgb(first, g, third);
                    }
                }

                return colors;
            }
            finally
            {
                window.Close();
            }
        });

        return (x, y) => colors[x, y];
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

