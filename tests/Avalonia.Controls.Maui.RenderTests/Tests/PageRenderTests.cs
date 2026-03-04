using System.Runtime.CompilerServices;
using Avalonia.Controls.Maui.RenderTests.Infrastructure;
using Avalonia.Headless.XUnit;
using Avalonia.Media.Imaging;
using Avalonia.Threading;
using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;

using AvaloniaBrushes = Avalonia.Media.Brushes;

namespace Avalonia.Controls.Maui.RenderTests.Tests;

public class PageRenderTests : RenderTestBase
{
    /// <summary>
    /// Renders a Page to a PNG file. Page is not a View, so we need a separate render method.
    /// </summary>
    private async Task RenderPageToFile(Page page, Action<IViewHandler>? setupPlatformView = null, [CallerMemberName] string testName = "")
    {
        EnsureHandlerCreated();

        await Dispatcher.UIThread.InvokeAsync(async () =>
        {
            var mauiContext = MauiContext;
            var handler = page.ToHandler(mauiContext);
            var platformView = (Control)((IViewHandler)handler).ContainerView! ?? (Control)handler.PlatformView!;

            setupPlatformView?.Invoke((IViewHandler)handler);

            var width = page.WidthRequest > 0 ? page.WidthRequest : 250;
            var height = page.HeightRequest > 0 ? page.HeightRequest : 120;

            ((IView)page).Measure(width, height);
            ((IView)page).Arrange(new Microsoft.Maui.Graphics.Rect(0, 0, width, height));

            platformView.Margin = new Avalonia.Thickness(20);

            var container = new Panel
            {
                Children = { platformView },
                Background = AvaloniaBrushes.White,
                Width = width + 40,
                Height = height + 40
            };

            var window = new Window
            {
                Content = container,
                Width = width + 40,
                Height = height + 40,
                Title = testName,
                SystemDecorations = WindowDecorations.None,
                Background = AvaloniaBrushes.White,
                CanResize = false,
                WindowStartupLocation = WindowStartupLocation.Manual
            };

            window.Show();

            for (int i = 0; i < 5; i++)
            {
                Dispatcher.UIThread.RunJobs(DispatcherPriority.Normal);
                Dispatcher.UIThread.RunJobs(DispatcherPriority.Render);
                await Task.Delay(20);
            }

            var pixelSize = new PixelSize((int)width + 40, (int)height + 40);
            var dpi = new Vector(96, 96);
            using var bitmap = new RenderTargetBitmap(pixelSize, dpi);
            bitmap.Render(window);

            var fileName = $"{GetType().Name}_{testName}.out.png";
            var outputPath = Path.Combine(Directory.GetCurrentDirectory(), fileName);

            bitmap.Save(outputPath);

            window.Close();
        });
    }

    [AvaloniaFact]
    public async Task Render_Page_BackgroundColor()
    {
        var page = new ContentPage
        {
            BackgroundColor = Colors.Red,
            WidthRequest = 200,
            HeightRequest = 200
        };

        await RenderPageToFile(page);
        CompareImages();
    }

    [AvaloniaFact]
    public async Task Render_Page_Background_SolidBrush()
    {
        var page = new ContentPage
        {
            Background = new SolidPaint(Colors.Blue),
            WidthRequest = 200,
            HeightRequest = 200
        };

        await RenderPageToFile(page);
        CompareImages();
    }

    [AvaloniaFact]
    public async Task Render_Page_GradientBackground()
    {
        var page = new ContentPage
        {
            Background = new LinearGradientPaint(
                new PaintGradientStop[]
                {
                    new PaintGradientStop(0f, Colors.Red),
                    new PaintGradientStop(1f, Colors.Blue),
                },
                new Microsoft.Maui.Graphics.Point(0, 0),
                new Microsoft.Maui.Graphics.Point(1, 1)),
            WidthRequest = 200,
            HeightRequest = 200
        };

        await RenderPageToFile(page);
        CompareImages();
    }

    [AvaloniaFact]
    public async Task Render_Page_NoBackground()
    {
        var page = new ContentPage
        {
            WidthRequest = 200,
            HeightRequest = 200
        };

        await RenderPageToFile(page);
        CompareImages();
    }

    [AvaloniaFact]
    public async Task Render_Page_BackgroundColor_Cleared()
    {
        var page = new ContentPage
        {
            BackgroundColor = Colors.Green,
            WidthRequest = 200,
            HeightRequest = 200
        };

        await RenderPageToFile(page, handler =>
        {
            // Clear the background color after the handler has applied it
            page.BackgroundColor = null;
            handler.UpdateValue(nameof(Page.Background));
        });
        CompareImages();
    }

    [AvaloniaFact]
    public async Task Render_Page_Background_Cleared()
    {
        var page = new ContentPage
        {
            Background = new SolidPaint(Colors.Purple),
            WidthRequest = 200,
            HeightRequest = 200
        };

        await RenderPageToFile(page, handler =>
        {
            // Clear the brush background after the handler has applied it
            page.Background = null;
            handler.UpdateValue(nameof(Page.Background));
        });
        CompareImages();
    }
}
