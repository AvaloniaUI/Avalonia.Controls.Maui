using Avalonia.Controls.Maui.Handlers;
using Avalonia.Controls.Maui.Tests.Stubs;
using Avalonia.Headless.XUnit;
using Avalonia.Media;
using Microsoft.Maui.Graphics;

namespace Avalonia.Controls.Maui.Tests.Handlers;

public class PageHandlerTests : HandlerTestBase<PageHandler, PageStub>
{
    [AvaloniaFact(DisplayName = "MapBackground Updates Platform Background With SolidBrush")]
    public async Task MapBackground_Updates_Platform_Background_With_SolidBrush()
    {
        var stub = new PageStub
        {
            Background = new SolidPaint(Microsoft.Maui.Graphics.Colors.Red),
            WidthRequest = 400,
            HeightRequest = 600
        };

        var handler = await CreateHandlerAsync<PageHandler>(stub);

        await InvokeOnMainThreadAsync(() =>
        {
            // First ensure background is set by triggering the mapping
            handler.UpdateValue(nameof(PageStub.Background));
            
            Assert.NotNull(handler.PlatformView.Background);
            var brush = Assert.IsType<SolidColorBrush>(handler.PlatformView.Background);
            Assert.Equal(Media.Colors.Red, brush.Color);
        });
    }

    [AvaloniaFact(DisplayName = "MapBackground Updates Platform Background With LinearGradient")]
    public async Task MapBackground_Updates_Platform_Background_With_LinearGradient()
    {
        var stub = new PageStub
        {
            Background = new LinearGradientPaint(
                new PaintGradientStop[] 
                { 
                    new PaintGradientStop(0f, Microsoft.Maui.Graphics.Colors.Blue),
                    new PaintGradientStop(1f, Microsoft.Maui.Graphics.Colors.Green)
                },
                new Microsoft.Maui.Graphics.Point(0, 0),
                new Microsoft.Maui.Graphics.Point(1, 1)),
            WidthRequest = 400,
            HeightRequest = 600
        };

        var handler = await CreateHandlerAsync<PageHandler>(stub);

        await InvokeOnMainThreadAsync(() =>
        {
            handler.UpdateValue(nameof(PageStub.Background));

            Assert.NotNull(handler.PlatformView.Background);
            Assert.IsType<LinearGradientBrush>(handler.PlatformView.Background);
        });
    }

    [AvaloniaFact(DisplayName = "MapBackground Clears Platform Background When Set To Null")]
    public async Task MapBackground_Clears_Platform_Background_When_Set_To_Null()
    {
        var stub = new PageStub
        {
            Background = new SolidPaint(Microsoft.Maui.Graphics.Colors.Blue),
            WidthRequest = 400,
            HeightRequest = 600
        };

        var handler = await CreateHandlerAsync<PageHandler>(stub);

        // First ensure background is set by triggering the mapping
        await InvokeOnMainThreadAsync(() =>
        {
            handler.UpdateValue(nameof(PageStub.Background));
        });

        await InvokeOnMainThreadAsync(() =>
        {
            Assert.NotNull(handler.PlatformView.Background);
        });

        // Clear the background
        await InvokeOnMainThreadAsync(() =>
        {
            stub.Background = null;
            handler.UpdateValue(nameof(PageStub.Background));
        });

        await InvokeOnMainThreadAsync(() =>
        {
            Assert.Null(handler.PlatformView.Background);
        });
    }

    [AvaloniaFact(DisplayName = "MapBackground Handles Different Colors")]
    public async Task MapBackground_Handles_Different_Colors()
    {
        var colors = new[]
        {
            (Microsoft.Maui.Graphics.Colors.Red, Media.Colors.Red),
            (Microsoft.Maui.Graphics.Colors.Green, Media.Colors.Green),
            (Microsoft.Maui.Graphics.Colors.Blue, Media.Colors.Blue),
            (Microsoft.Maui.Graphics.Colors.Yellow, Media.Colors.Yellow),
        };

        foreach (var (mauiColor, avaloniaColor) in colors)
        {
            var stub = new PageStub
            {
                Background = new SolidPaint(mauiColor),
                WidthRequest = 400,
                HeightRequest = 600
            };

            var handler = await CreateHandlerAsync<PageHandler>(stub);

            await InvokeOnMainThreadAsync(() =>
            {
                handler.UpdateValue(nameof(PageStub.Background));

                Assert.NotNull(handler.PlatformView.Background);
                var brush = Assert.IsType<Media.SolidColorBrush>(handler.PlatformView.Background);
                Assert.Equal(avaloniaColor, brush.Color);
            });
        }
    }

    [AvaloniaFact(DisplayName = "MapBackground Updates Value When Changed")]
    public async Task MapBackground_Updates_Value_When_Changed()
    {
        var stub = new PageStub
        {
            Background = new SolidPaint(Microsoft.Maui.Graphics.Colors.Red),
            WidthRequest = 400,
            HeightRequest = 600
        };

        var handler = await CreateHandlerAsync<PageHandler>(stub);

        // Verify initial value
        await InvokeOnMainThreadAsync(() =>
        {
            handler.UpdateValue(nameof(PageStub.Background));

            var brush = handler.PlatformView.Background as SolidColorBrush;
            Assert.NotNull(brush);
            Assert.Equal(Media.Colors.Red, brush.Color);
        });

        // Change the background
        await InvokeOnMainThreadAsync(() =>
        {
            stub.Background = new SolidPaint(Microsoft.Maui.Graphics.Colors.Green);
            handler.UpdateValue(nameof(PageStub.Background));
        });

        // Verify updated value
        await InvokeOnMainThreadAsync(() =>
        {
            var brush = handler.PlatformView.Background as SolidColorBrush;
            Assert.NotNull(brush);
            Assert.Equal(Media.Colors.Green, brush.Color);
        });
    }
}
