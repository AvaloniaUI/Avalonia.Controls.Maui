using Avalonia.Controls.Maui.Tests.Stubs;
using Avalonia.Controls.Maui.Tests.TestUtilities;
using Avalonia.Headless.XUnit;
using Microsoft.Maui;
using Microsoft.Maui.Controls.Shapes;
using Microsoft.Maui.Graphics;
using MauiIndicatorViewHandler = Avalonia.Controls.Maui.Handlers.IndicatorViewHandler;
using PipsPager = Avalonia.Controls.Maui.Controls.PipsPager;

namespace Avalonia.Controls.Maui.Tests.Handlers;

public class IndicatorViewHandlerTests : HandlerTestBase<MauiIndicatorViewHandler, IndicatorViewStub>
{
    [AvaloniaFact(DisplayName = "Handler creates PipsPager")]
    public async Task HandlerCreatesPipsPager()
    {
        var indicator = new IndicatorViewStub { Count = 3, Position = 0 };
        var handler = await CreateHandlerAsync(indicator);

        Assert.NotNull(handler.PlatformView);
        Assert.IsType<PipsPager>(handler.PlatformView);
    }

    [AvaloniaFact(DisplayName = "Count initializes NumberOfPages correctly")]
    public async Task CountInitializesNumberOfPagesCorrectly()
    {
        var indicator = new IndicatorViewStub { Count = 5 };

        await ValidatePropertyInitValue(
            indicator,
            () => indicator.Count,
            handler => GetPlatformView(handler).NumberOfPages,
            5);
    }

    [AvaloniaTheory(DisplayName = "Count updates NumberOfPages correctly")]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(3)]
    [InlineData(10)]
    public async Task CountUpdatesNumberOfPagesCorrectly(int count)
    {
        var indicator = new IndicatorViewStub { Count = 0 };

        var values = await GetValueAsync(indicator, (handler) =>
        {
            indicator.Count = count;
            handler.UpdateValue(nameof(IIndicatorView.Count));

            return new
            {
                ViewValue = indicator.Count,
                PlatformValue = GetPlatformView(handler).NumberOfPages
            };
        });

        Assert.Equal(count, values.ViewValue);
        Assert.Equal(count, values.PlatformValue);
    }

    [AvaloniaFact(DisplayName = "Position initializes SelectedPageIndex correctly")]
    public async Task PositionInitializesSelectedPageIndexCorrectly()
    {
        var indicator = new IndicatorViewStub { Count = 5, Position = 2 };

        await ValidatePropertyInitValue(
            indicator,
            () => indicator.Position,
            handler => GetPlatformView(handler).SelectedPageIndex,
            2);
    }

    [AvaloniaTheory(DisplayName = "Position updates SelectedPageIndex correctly")]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(4)]
    public async Task PositionUpdatesSelectedPageIndexCorrectly(int position)
    {
        var indicator = new IndicatorViewStub { Count = 5, Position = 0 };

        var values = await GetValueAsync(indicator, (handler) =>
        {
            indicator.Position = position;
            handler.UpdateValue(nameof(IIndicatorView.Position));

            return new
            {
                ViewValue = indicator.Position,
                PlatformValue = GetPlatformView(handler).SelectedPageIndex
            };
        });

        Assert.Equal(position, values.ViewValue);
        Assert.Equal(position, values.PlatformValue);
    }

    [AvaloniaFact(DisplayName = "HideSingle true with Count 1 hides the view")]
    public async Task HideSingleTrueWithCountOneHidesView()
    {
        var indicator = new IndicatorViewStub { Count = 1, HideSingle = true };
        var handler = await CreateHandlerAsync(indicator);

        Assert.False(handler.PlatformView.IsVisible);
    }

    [AvaloniaFact(DisplayName = "HideSingle false with Count 1 keeps the view visible")]
    public async Task HideSingleFalseWithCountOneKeepsViewVisible()
    {
        var indicator = new IndicatorViewStub { Count = 1, HideSingle = false };
        var handler = await CreateHandlerAsync(indicator);

        Assert.True(handler.PlatformView.IsVisible);
    }

    [AvaloniaFact(DisplayName = "HideSingle true with Count > 1 keeps the view visible")]
    public async Task HideSingleTrueWithCountGreaterThanOneKeepsViewVisible()
    {
        var indicator = new IndicatorViewStub { Count = 3, HideSingle = true };
        var handler = await CreateHandlerAsync(indicator);

        Assert.True(handler.PlatformView.IsVisible);
    }

    [AvaloniaFact(DisplayName = "MaximumVisible initializes MaxVisiblePips correctly")]
    public async Task MaximumVisibleInitializesMaxVisiblePipsCorrectly()
    {
        var indicator = new IndicatorViewStub { Count = 10, MaximumVisible = 5 };

        await ValidatePropertyInitValue(
            indicator,
            () => indicator.MaximumVisible,
            handler => GetPlatformView(handler).MaxVisiblePips,
            5);
    }

    [AvaloniaTheory(DisplayName = "MaximumVisible updates MaxVisiblePips correctly")]
    [InlineData(3)]
    [InlineData(5)]
    [InlineData(int.MaxValue)]
    public async Task MaximumVisibleUpdatesMaxVisiblePipsCorrectly(int maxVisible)
    {
        var indicator = new IndicatorViewStub { Count = 10, MaximumVisible = int.MaxValue };

        var values = await GetValueAsync(indicator, (handler) =>
        {
            indicator.MaximumVisible = maxVisible;
            handler.UpdateValue(nameof(IIndicatorView.MaximumVisible));

            return new
            {
                ViewValue = indicator.MaximumVisible,
                PlatformValue = GetPlatformView(handler).MaxVisiblePips
            };
        });

        Assert.Equal(maxVisible, values.ViewValue);
        Assert.Equal(maxVisible, values.PlatformValue);
    }

    [AvaloniaFact(DisplayName = "IndicatorColor sets resource brush")]
    public async Task IndicatorColorSetsResourceBrush()
    {
        var indicator = new IndicatorViewStub
        {
            Count = 3,
            IndicatorColor = new SolidPaint(Colors.Blue)
        };

        var platformColor = await GetValueAsync(indicator, GetPipFillColor);

        Assert.NotNull(platformColor);
        ColorComparisonHelpers.AssertColorsAreEqual(Colors.Blue, platformColor);
    }

    [AvaloniaTheory(DisplayName = "IndicatorColor updates resource brush")]
    [InlineData(255, 0, 0)]
    [InlineData(0, 255, 0)]
    [InlineData(0, 0, 255)]
    public async Task IndicatorColorUpdatesResourceBrush(byte r, byte g, byte b)
    {
        var indicator = new IndicatorViewStub
        {
            Count = 3,
            IndicatorColor = new SolidPaint(Colors.White)
        };

        var newColor = Color.FromRgb(r, g, b);

        var platformColor = await GetValueAsync(indicator, (handler) =>
        {
            indicator.IndicatorColor = new SolidPaint(newColor);
            handler.UpdateValue(nameof(IIndicatorView.IndicatorColor));
            return GetPipFillColor(handler);
        });

        Assert.NotNull(platformColor);
        ColorComparisonHelpers.AssertColorsAreEqual(newColor, platformColor);
    }

    [AvaloniaFact(DisplayName = "SelectedIndicatorColor sets resource brush")]
    public async Task SelectedIndicatorColorSetsResourceBrush()
    {
        var indicator = new IndicatorViewStub
        {
            Count = 3,
            SelectedIndicatorColor = new SolidPaint(Colors.Red)
        };

        var platformColor = await GetValueAsync(indicator, GetSelectedPipFillColor);

        Assert.NotNull(platformColor);
        ColorComparisonHelpers.AssertColorsAreEqual(Colors.Red, platformColor);
    }

    [AvaloniaTheory(DisplayName = "SelectedIndicatorColor updates resource brush")]
    [InlineData(255, 0, 0)]
    [InlineData(0, 128, 255)]
    [InlineData(128, 0, 128)]
    public async Task SelectedIndicatorColorUpdatesResourceBrush(byte r, byte g, byte b)
    {
        var indicator = new IndicatorViewStub
        {
            Count = 3,
            SelectedIndicatorColor = new SolidPaint(Colors.White)
        };

        var newColor = Color.FromRgb(r, g, b);

        var platformColor = await GetValueAsync(indicator, (handler) =>
        {
            indicator.SelectedIndicatorColor = new SolidPaint(newColor);
            handler.UpdateValue(nameof(IIndicatorView.SelectedIndicatorColor));
            return GetSelectedPipFillColor(handler);
        });

        Assert.NotNull(platformColor);
        ColorComparisonHelpers.AssertColorsAreEqual(newColor, platformColor);
    }

    [AvaloniaFact(DisplayName = "Null IndicatorColor removes resource key")]
    public async Task NullIndicatorColorRemovesResourceKey()
    {
        var indicator = new IndicatorViewStub
        {
            Count = 3,
            IndicatorColor = null!
        };

        var handler = await CreateHandlerAsync(indicator);
        Assert.False(handler.PlatformView.Resources.ContainsKey("PipsPagerSelectionIndicatorForeground"));
    }

    [AvaloniaFact(DisplayName = "Null SelectedIndicatorColor removes resource key")]
    public async Task NullSelectedIndicatorColorRemovesResourceKey()
    {
        var indicator = new IndicatorViewStub
        {
            Count = 3,
            SelectedIndicatorColor = null!
        };

        var handler = await CreateHandlerAsync(indicator);
        Assert.False(handler.PlatformView.Resources.ContainsKey("PipsPagerSelectionIndicatorForegroundSelected"));
    }

    [AvaloniaFact(DisplayName = "Updating Count does not affect Position")]
    public async Task UpdatingCountDoesNotAffectPosition()
    {
        var indicator = new IndicatorViewStub { Count = 5, Position = 2 };

        await ValidateUnrelatedPropertyUnaffected(
            indicator,
            handler => GetPlatformView(handler).SelectedPageIndex,
            nameof(IIndicatorView.Count),
            () => indicator.Count = 8);
    }

    [AvaloniaFact(DisplayName = "Updating Position does not affect Count")]
    public async Task UpdatingPositionDoesNotAffectCount()
    {
        var indicator = new IndicatorViewStub { Count = 5, Position = 1 };

        await ValidateUnrelatedPropertyUnaffected(
            indicator,
            handler => GetPlatformView(handler).NumberOfPages,
            nameof(IIndicatorView.Position),
            () => indicator.Position = 3);
    }

    [AvaloniaFact(DisplayName = "IndicatorSize sets pip size resources")]
    public async Task IndicatorSizeSetsResourceValues()
    {
        var indicator = new IndicatorViewStub { Count = 3, IndicatorSize = 12.0 };
        var handler = await CreateHandlerAsync(indicator);
        var pager = handler.PlatformView;

        Assert.True(pager.Resources.TryGetValue("PipsPagerPipSizeSelected", out var selected));
        Assert.Equal(12.0, (double)selected!);

        Assert.True(pager.Resources.TryGetValue("PipsPagerPipSize", out var unselected));
        Assert.Equal(12.0 * (2.0 / 3.0), (double)unselected!, 0.001);
    }

    [AvaloniaTheory(DisplayName = "IndicatorSize updates pip size resources")]
    [InlineData(6.0)]
    [InlineData(10.0)]
    [InlineData(20.0)]
    public async Task IndicatorSizeUpdatesPipSizeResources(double size)
    {
        var indicator = new IndicatorViewStub { Count = 3, IndicatorSize = 6.0 };

        var result = await GetValueAsync(indicator, (handler) =>
        {
            indicator.IndicatorSize = size;
            handler.UpdateValue(nameof(IIndicatorView.IndicatorSize));

            var pager = GetPlatformView(handler);
            pager.Resources.TryGetValue("PipsPagerPipSizeSelected", out var sel);
            pager.Resources.TryGetValue("PipsPagerPipSize", out var unsel);
            return new { Selected = (double)sel!, Unselected = (double)unsel! };
        });

        Assert.Equal(size, result.Selected, 0.001);
        Assert.Equal(size * (2.0 / 3.0), result.Unselected, 0.001);
    }

    [AvaloniaFact(DisplayName = "Null IndicatorsShape defaults to circle")]
    public async Task NullIndicatorsShapeDefaultsToCircle()
    {
        var indicator = new IndicatorViewStub { Count = 3, IndicatorsShape = null! };
        var handler = await CreateHandlerAsync(indicator);
        var pager = handler.PlatformView;

        Assert.True(pager.Resources.TryGetValue("PipsPagerPipCornerRadius", out var radius));
        Assert.Equal(new global::Avalonia.CornerRadius(999), (global::Avalonia.CornerRadius)radius!);
    }

    [AvaloniaFact(DisplayName = "Ellipse IndicatorsShape sets circle corner radius")]
    public async Task EllipseIndicatorsShapeSetsCircleCornerRadius()
    {
        var indicator = new IndicatorViewStub
        {
            Count = 3,
            IndicatorsShape = new Ellipse()
        };
        var handler = await CreateHandlerAsync(indicator);
        var pager = handler.PlatformView;

        Assert.True(pager.Resources.TryGetValue("PipsPagerPipCornerRadius", out var radius));
        Assert.Equal(new global::Avalonia.CornerRadius(999), (global::Avalonia.CornerRadius)radius!);
    }

    [AvaloniaFact(DisplayName = "Rectangle IndicatorsShape sets zero corner radius")]
    public async Task RectangleIndicatorsShapeSetsZeroCornerRadius()
    {
        var indicator = new IndicatorViewStub
        {
            Count = 3,
            IndicatorsShape = new Rectangle()
        };
        var handler = await CreateHandlerAsync(indicator);
        var pager = handler.PlatformView;

        Assert.True(pager.Resources.TryGetValue("PipsPagerPipCornerRadius", out var radius));
        Assert.Equal(new global::Avalonia.CornerRadius(0), (global::Avalonia.CornerRadius)radius!);
    }

    [AvaloniaFact(DisplayName = "IndicatorsShape updates corner radius when changed")]
    public async Task IndicatorsShapeUpdatesCornerRadius()
    {
        var indicator = new IndicatorViewStub
        {
            Count = 3,
            IndicatorsShape = new Ellipse()
        };

        var radius = await GetValueAsync(indicator, (handler) =>
        {
            indicator.IndicatorsShape = new Rectangle();
            handler.UpdateValue(nameof(IIndicatorView.IndicatorsShape));

            GetPlatformView(handler).Resources.TryGetValue("PipsPagerPipCornerRadius", out var r);
            return (global::Avalonia.CornerRadius)r!;
        });

        Assert.Equal(new global::Avalonia.CornerRadius(0), radius);
    }

    private static PipsPager GetPlatformView(MauiIndicatorViewHandler handler)
    {
        var view = handler.PlatformView;
        Assert.NotNull(view);
        return view;
    }

    private static Color? GetPipFillColor(MauiIndicatorViewHandler handler)
    {
        var pager = GetPlatformView(handler);
        if (pager.Resources.TryGetValue("PipsPagerSelectionIndicatorForeground", out var brush))
            return GetColorFromBrush(brush as global::Avalonia.Media.IBrush);
        return null;
    }

    private static Color? GetSelectedPipFillColor(MauiIndicatorViewHandler handler)
    {
        var pager = GetPlatformView(handler);
        if (pager.Resources.TryGetValue("PipsPagerSelectionIndicatorForegroundSelected", out var brush))
            return GetColorFromBrush(brush as global::Avalonia.Media.IBrush);
        return null;
    }

    private static Color? GetColorFromBrush(global::Avalonia.Media.IBrush? brush)
    {
        if (brush is global::Avalonia.Media.ISolidColorBrush solid)
        {
            var c = solid.Color;
            return Color.FromRgba(c.R, c.G, c.B, c.A);
        }

        return null;
    }
}
