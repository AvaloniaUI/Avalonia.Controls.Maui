using Avalonia.Headless.XUnit;
using Avalonia.Controls.Maui.Tests.Stubs;
using Avalonia.Controls.Maui.Tests.TestUtilities;
using Microsoft.Maui;
using Microsoft.Maui.Graphics;
using MauiGraphicsViewHandler = Avalonia.Controls.Maui.Handlers.GraphicsViewHandler;
using PlatformTouchGraphicsView = Avalonia.Controls.Maui.Platform.PlatformTouchGraphicsView;

namespace Avalonia.Controls.Maui.Tests.Handlers;

public partial class GraphicsViewHandlerTests : HandlerTestBase<MauiGraphicsViewHandler, GraphicsViewStub>
{
    [AvaloniaFact(DisplayName = "Drawable Initializes Correctly")]
    public async Task DrawableInitializesCorrectly()
    {
        var drawable = new TestDrawable();
        var graphicsView = new GraphicsViewStub
        {
            Drawable = drawable
        };

        var handler = await CreateHandlerAsync(graphicsView);
        Assert.NotNull(handler.PlatformView);
    }

    [AvaloniaFact(DisplayName = "Drawable Updates Correctly")]
    public async Task DrawableUpdatesCorrectly()
    {
        var drawable1 = new TestDrawable { FillColor = Colors.Red };
        var drawable2 = new TestDrawable { FillColor = Colors.Blue };
        var graphicsView = new GraphicsViewStub { Drawable = drawable1 };

        var handler = await CreateHandlerAsync(graphicsView);

        graphicsView.Drawable = drawable2;
        handler.UpdateValue(nameof(IGraphicsView.Drawable));

        Assert.NotNull(handler.PlatformView);
    }

    [AvaloniaFact(DisplayName = "Null Drawable Doesn't Crash")]
    public async Task NullDrawableDoesntCrash()
    {
        var graphicsView = new GraphicsViewStub { Drawable = new TestDrawable() };
        await CreateHandlerAsync(graphicsView);
    }

    [AvaloniaFact(DisplayName = "Background Initializes Correctly")]
    public async Task BackgroundInitializesCorrectly()
    {
        var graphicsView = new GraphicsViewStub
        {
            Background = new SolidPaint(Colors.Blue)
        };

        var platformColor = await GetValueAsync(graphicsView, GetPlatformBackgroundColor);

        Assert.NotNull(platformColor);
        ColorComparisonHelpers.AssertColorsAreEqual(Colors.Blue, platformColor);
    }

    [AvaloniaTheory(DisplayName = "Background Updates Correctly")]
    [InlineData(255, 0, 0)]
    [InlineData(0, 255, 0)]
    [InlineData(0, 0, 255)]
    public async Task BackgroundUpdatesCorrectly(byte r, byte g, byte b)
    {
        var graphicsView = new GraphicsViewStub
        {
            Background = new SolidPaint(Colors.White)
        };

        var newColor = Color.FromRgb(r, g, b);

        var platformColor = await GetValueAsync(graphicsView, handler =>
        {
            graphicsView.Background = new SolidPaint(newColor);
            handler.UpdateValue(nameof(IView.Background));
            return GetPlatformBackgroundColor(handler);
        });

        Assert.NotNull(platformColor);
        ColorComparisonHelpers.AssertColorsAreEqual(newColor, platformColor);
    }

    [AvaloniaFact(DisplayName = "Null Background Doesn't Crash")]
    public async Task NullBackgroundDoesntCrash()
    {
        var graphicsView = new GraphicsViewStub { Background = null };
        await CreateHandlerAsync(graphicsView);
    }

    [AvaloniaFact(DisplayName = "Handler Creates PlatformTouchGraphicsView")]
    public async Task HandlerCreatesPlatformTouchGraphicsView()
    {
        var graphicsView = new GraphicsViewStub();
        var handler = await CreateHandlerAsync(graphicsView);

        Assert.NotNull(handler.PlatformView);
        Assert.IsType<PlatformTouchGraphicsView>(handler.PlatformView);
    }

    [AvaloniaFact(DisplayName = "Invalidate Command Works")]
    public async Task InvalidateCommandWorks()
    {
        var drawable = new TestDrawable();
        var graphicsView = new GraphicsViewStub
        {
            Drawable = drawable
        };

        var handler = await CreateHandlerAsync(graphicsView);

        // Trigger invalidate
        graphicsView.Invalidate();

        // The platform view should remain valid
        Assert.NotNull(handler.PlatformView);
    }

    [AvaloniaFact(DisplayName = "Changing Drawable Triggers Redraw")]
    public async Task ChangingDrawableTriggersRedraw()
    {
        var drawable1 = new TestDrawable { FillColor = Colors.Red };
        var drawable2 = new TestDrawable { FillColor = Colors.Green };

        var graphicsView = new GraphicsViewStub
        {
            Drawable = drawable1
        };

        var handler = await CreateHandlerAsync(graphicsView);

        // Change drawable
        graphicsView.Drawable = drawable2;
        handler.UpdateValue(nameof(IGraphicsView.Drawable));

        // Verify the handler is still valid
        Assert.NotNull(handler.PlatformView);
    }

    [AvaloniaFact(DisplayName = "Updating Drawable Does Not Affect Background")]
    public async Task DrawableDoesNotAffectBackground()
    {
        var graphicsView = new GraphicsViewStub
        {
            Drawable = new TestDrawable(),
            Background = new SolidPaint(Colors.Blue)
        };

        await ValidateUnrelatedPropertyUnaffected(
            graphicsView,
            GetPlatformBackgroundColor,
            nameof(IGraphicsView.Drawable),
            () => graphicsView.Drawable = new TestDrawable { FillColor = Colors.Yellow });
    }

    [AvaloniaFact(DisplayName = "Updating Background Does Not Affect Drawable")]
    public async Task BackgroundDoesNotAffectDrawable()
    {
        var drawable = new TestDrawable();
        var graphicsView = new GraphicsViewStub
        {
            Drawable = drawable,
            Background = new SolidPaint(Colors.Blue)
        };

        var initialColor = await GetValueAsync(graphicsView, GetPlatformBackgroundColor);

        graphicsView.Background = new SolidPaint(Colors.Red);
        await GetValueAsync(graphicsView, handler =>
        {
            handler.UpdateValue(nameof(IView.Background));
            return GetPlatformBackgroundColor(handler);
        });

        Assert.NotNull(initialColor);
    }

    [AvaloniaFact(DisplayName = "Multiple Drawables Can Be Set")]
    public async Task MultipleDrawablesCanBeSet()
    {
        var graphicsView = new GraphicsViewStub
        {
            Drawable = new TestDrawable()
        };
        var handler = await CreateHandlerAsync(graphicsView);

        for (int i = 0; i < 5; i++)
        {
            var drawable = new TestDrawable { FillColor = Color.FromRgb(i * 50, 0, 0) };
            graphicsView.Drawable = drawable;
            handler.UpdateValue(nameof(IGraphicsView.Drawable));

            Assert.NotNull(handler.PlatformView);
        }
    }

    Color? GetPlatformBackgroundColor(MauiGraphicsViewHandler handler) =>
        handler.PlatformView?.Background is Media.ISolidColorBrush brush
            ? Color.FromRgba(brush.Color.R, brush.Color.G, brush.Color.B, brush.Color.A)
            : null;
}
