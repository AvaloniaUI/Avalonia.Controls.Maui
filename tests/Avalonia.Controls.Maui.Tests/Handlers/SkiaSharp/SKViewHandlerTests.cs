using Avalonia.Controls.Maui.SkiaSharp.Views.Handlers;
using Avalonia.Headless.XUnit;
using Microsoft.Maui.Controls;
using SkiaSharp.Views.Maui.Controls;
using AvaloniaHorizontalAlignment = Avalonia.Layout.HorizontalAlignment;
using AvaloniaVerticalAlignment = Avalonia.Layout.VerticalAlignment;
using MauiRect = Microsoft.Maui.Graphics.Rect;

namespace Avalonia.Controls.Maui.Tests.Handlers.SkiaSharp;

public class SKViewHandlerTests : HandlerTestBase
{
    [AvaloniaFact(DisplayName = "SKCanvasView maps base view properties to platform control")]
    public async Task SKCanvasViewMapsBaseViewProperties()
    {
        var view = new SKCanvasView
        {
            IsVisible = false,
            Opacity = 0.5,
            WidthRequest = 120,
            HeightRequest = 80,
        };

        var handler = await CreateHandlerAsync<SKCanvasViewHandler>(view);

        await InvokeOnMainThreadAsync(() =>
        {
            Assert.False(handler.PlatformView.IsVisible);
            Assert.Equal(0.5, handler.PlatformView.Opacity, 3);
            Assert.Equal(120, handler.PlatformView.Width, 3);
            Assert.Equal(80, handler.PlatformView.Height, 3);
        });
    }

    [AvaloniaFact(DisplayName = "SKGLView maps base view properties to platform control")]
    public async Task SKGLViewMapsBaseViewProperties()
    {
        var view = new SKGLView
        {
            IsVisible = false,
            Opacity = 0.5,
            WidthRequest = 120,
            HeightRequest = 80,
        };

        var handler = await CreateHandlerAsync<SKGLViewHandler>(view);

        await InvokeOnMainThreadAsync(() =>
        {
            Assert.False(handler.PlatformView.IsVisible);
            Assert.Equal(0.5, handler.PlatformView.Opacity, 3);
            Assert.Equal(120, handler.PlatformView.Width, 3);
            Assert.Equal(80, handler.PlatformView.Height, 3);
        });
    }

    [AvaloniaFact(DisplayName = "SKCanvasView with Start options fills its arranged frame")]
    public async Task SKCanvasViewWithStartOptionsFillsArrangedFrame()
    {
        var view = new SKCanvasView
        {
            HorizontalOptions = LayoutOptions.Start,
            VerticalOptions = LayoutOptions.Start,
        };

        var handler = await CreateHandlerAsync<SKCanvasViewHandler>(view);

        await InvokeOnMainThreadAsync(() =>
        {
            Assert.Equal(AvaloniaHorizontalAlignment.Left, handler.PlatformView.HorizontalAlignment);
            Assert.Equal(AvaloniaVerticalAlignment.Top, handler.PlatformView.VerticalAlignment);

            // MAUI measures with the space it then arranges the view in
            ((Microsoft.Maui.IView)view).Measure(100, 48);
            handler.PlatformArrange(new MauiRect(0, 0, 100, 48));

            Assert.Equal(100, handler.PlatformView.Bounds.Width, 3);
            Assert.Equal(48, handler.PlatformView.Bounds.Height, 3);
        });
    }

    [AvaloniaFact(DisplayName = "SKGLView with Center options fills its arranged frame")]
    public async Task SKGLViewWithCenterOptionsFillsArrangedFrame()
    {
        var view = new SKGLView
        {
            HorizontalOptions = LayoutOptions.Center,
            VerticalOptions = LayoutOptions.Center,
        };

        var handler = await CreateHandlerAsync<SKGLViewHandler>(view);

        await InvokeOnMainThreadAsync(() =>
        {
            Assert.Equal(AvaloniaHorizontalAlignment.Center, handler.PlatformView.HorizontalAlignment);
            Assert.Equal(AvaloniaVerticalAlignment.Center, handler.PlatformView.VerticalAlignment);

            // MAUI measures with the space it then arranges the view in
            ((Microsoft.Maui.IView)view).Measure(100, 48);
            handler.PlatformArrange(new MauiRect(0, 0, 100, 48));

            Assert.Equal(100, handler.PlatformView.Bounds.Width, 3);
            Assert.Equal(48, handler.PlatformView.Bounds.Height, 3);
        });
    }
}
