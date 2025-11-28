using Avalonia.Headless.XUnit;
using Avalonia.Controls.Maui.Tests.Stubs;
using Avalonia.Controls.Maui.Tests.TestUtilities;
using Microsoft.Maui;
using Microsoft.Maui.Graphics;
using AvaloniaPanel = Avalonia.Controls.Panel;
using MauiPageHandler = Avalonia.Controls.Maui.Handlers.PageHandler;

namespace Avalonia.Controls.Maui.Tests.Handlers;

public partial class PageHandlerTests : HandlerTestBase<MauiPageHandler, PageStub>
{
    [AvaloniaFact(DisplayName = "Background Initializes Correctly")]
    public async Task BackgroundInitializesCorrectly()
    {
        var color = Colors.Blue;
        var page = new PageStub
        {
            Background = new SolidPaint(color)
        };

        var handler = await CreateHandlerAsync(page);
        var platformColor = GetPlatformBackgroundColor(handler);

        Assert.NotNull(platformColor);
        ColorComparisonHelpers.AssertColorsAreEqual(color, platformColor);
    }

    [AvaloniaFact(DisplayName = "Background Updates Correctly")]
    public async Task BackgroundUpdatesCorrectly()
    {
        var page = new PageStub
        {
            Background = new SolidPaint(Colors.Blue)
        };

        var handler = await CreateHandlerAsync(page);

        await InvokeOnMainThreadAsync(() =>
        {
            page.Background = new SolidPaint(Colors.Red);
            handler.UpdateValue(nameof(IContentView.Background));
        });

        var platformColor = GetPlatformBackgroundColor(handler);

        Assert.NotNull(platformColor);
        ColorComparisonHelpers.AssertColorsAreEqual(Colors.Red, platformColor);
    }

    [AvaloniaFact(DisplayName = "Null Background Clears Value")]
    public async Task NullBackgroundClearsValue()
    {
        var page = new PageStub
        {
            Background = new SolidPaint(Colors.Green)
        };

        var handler = await CreateHandlerAsync(page);

        // Verify initial background is set
        var initialColor = GetPlatformBackgroundColor(handler);
        Assert.NotNull(initialColor);
        ColorComparisonHelpers.AssertColorsAreEqual(Colors.Green, initialColor);

        // Set background to null
        await InvokeOnMainThreadAsync(() =>
        {
            page.Background = null;
            handler.UpdateValue(nameof(IContentView.Background));
        });

        // Verify background is cleared
        var panel = handler.PlatformView as AvaloniaPanel;
        Assert.NotNull(panel);
        Assert.Null(panel.Background);
    }

    [AvaloniaFact(DisplayName = "Null Background Does Not Crash")]
    public async Task NullBackgroundDoesNotCrash()
    {
        var page = new PageStub
        {
            Background = null
        };

        // Should not throw
        await CreateHandlerAsync(page);
    }

    [AvaloniaFact(DisplayName = "Background Changes From Value To Null")]
    public async Task BackgroundChangesFromValueToNull()
    {
        var page = new PageStub
        {
            Background = new SolidPaint(Colors.Purple)
        };

        var handler = await CreateHandlerAsync(page);

        // Verify initial background
        var panel = handler.PlatformView as AvaloniaPanel;
        Assert.NotNull(panel);
        Assert.NotNull(panel.Background);

        // Change to null
        await InvokeOnMainThreadAsync(() =>
        {
            page.Background = null;
            MauiPageHandler.MapBackground(handler, page);
        });

        // Background should be cleared (null)
        Assert.Null(panel.Background);
    }

    [AvaloniaFact(DisplayName = "Background Changes From Null To Value")]
    public async Task BackgroundChangesFromNullToValue()
    {
        var page = new PageStub
        {
            Background = null
        };

        var handler = await CreateHandlerAsync(page);

        var panel = handler.PlatformView as AvaloniaPanel;
        Assert.NotNull(panel);

        // Initially null
        Assert.Null(panel.Background);

        // Set to a color
        await InvokeOnMainThreadAsync(() =>
        {
            page.Background = new SolidPaint(Colors.Orange);
            MauiPageHandler.MapBackground(handler, page);
        });

        // Background should now be set
        Assert.NotNull(panel.Background);
        var platformColor = GetPlatformBackgroundColor(handler);
        Assert.NotNull(platformColor);
        ColorComparisonHelpers.AssertColorsAreEqual(Colors.Orange, platformColor);
    }

    [AvaloniaTheory(DisplayName = "Various Colors Work Correctly")]
    [InlineData(255, 0, 0)]      // Red
    [InlineData(0, 255, 0)]      // Green
    [InlineData(0, 0, 255)]      // Blue
    [InlineData(255, 255, 0)]    // Yellow
    [InlineData(255, 0, 255)]    // Magenta
    [InlineData(0, 255, 255)]    // Cyan
    public async Task VariousColorsWorkCorrectly(byte r, byte g, byte b)
    {
        var color = Color.FromRgb(r, g, b);
        var page = new PageStub
        {
            Background = new SolidPaint(color)
        };

        var handler = await CreateHandlerAsync(page);
        var platformColor = GetPlatformBackgroundColor(handler);

        Assert.NotNull(platformColor);
        ColorComparisonHelpers.AssertColorsAreEqual(color, platformColor);
    }

    [AvaloniaFact(DisplayName = "Transparent Background Works")]
    public async Task TransparentBackgroundWorks()
    {
        var page = new PageStub
        {
            Background = new SolidPaint(Colors.Transparent)
        };

        var handler = await CreateHandlerAsync(page);
        var platformColor = GetPlatformBackgroundColor(handler);

        Assert.NotNull(platformColor);
        Assert.Equal(0, platformColor.Alpha, 0.01f);
    }

    [AvaloniaTheory(DisplayName = "Semi-Transparent Backgrounds Work")]
    [InlineData(128)] // 50% alpha
    [InlineData(64)]  // 25% alpha
    [InlineData(192)] // 75% alpha
    public async Task SemiTransparentBackgroundsWork(byte alpha)
    {
        var color = Color.FromRgba((byte)255, (byte)0, (byte)0, alpha);
        var page = new PageStub
        {
            Background = new SolidPaint(color)
        };

        var handler = await CreateHandlerAsync(page);
        var platformColor = GetPlatformBackgroundColor(handler);

        Assert.NotNull(platformColor);
        Assert.Equal(alpha / 255f, platformColor.Alpha, 0.02f);
    }

    Color? GetPlatformBackgroundColor(MauiPageHandler handler)
    {
        var panel = handler.PlatformView as AvaloniaPanel;
        if (panel?.Background is Media.SolidColorBrush brush)
        {
            var color = brush.Color;
            return Color.FromRgba(color.R, color.G, color.B, color.A);
        }
        return null;
    }
}
