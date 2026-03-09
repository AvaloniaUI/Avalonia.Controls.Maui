using System.Runtime.CompilerServices;
using Avalonia.Controls.Maui.RenderTests.Infrastructure;
using Avalonia.Headless.XUnit;
using Avalonia.Media.Imaging;
using Avalonia.Threading;
using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;
using MauiContentPage = Microsoft.Maui.Controls.ContentPage;
using MauiTabbedPage = Microsoft.Maui.Controls.TabbedPage;

using AvaloniaBrushes = Avalonia.Media.Brushes;

namespace Avalonia.Controls.Maui.RenderTests.Tests;

public class TabbedPageRenderTests : RenderTestBase
{
    private async Task RenderTabbedPageToFile(
        Func<MauiTabbedPage> createTabbedPage,
        Action<MauiTabbedPage, IViewHandler>? afterSetup = null,
        double width = 400,
        double height = 300,
        [CallerMemberName] string testName = "")
    {
        EnsureHandlerCreated();

        await Dispatcher.UIThread.InvokeAsync(async () =>
        {
            var tabbedPage = createTabbedPage();
            var mauiContext = MauiContext;
            var handler = tabbedPage.ToHandler(mauiContext);
            var platformView = (Control)handler.PlatformView!;

            afterSetup?.Invoke(tabbedPage, (IViewHandler)handler);

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

    private static MauiTabbedPage CreateTabbedPageWithTabs()
    {
        var tabbedPage = new MauiTabbedPage();
        tabbedPage.Children.Add(new MauiContentPage
        {
            Title = "Home",
            Content = new Microsoft.Maui.Controls.Label { Text = "Home Page", TextColor = Colors.Black }
        });
        tabbedPage.Children.Add(new MauiContentPage
        {
            Title = "Settings",
            Content = new Microsoft.Maui.Controls.Label { Text = "Settings Page", TextColor = Colors.Black }
        });
        tabbedPage.Children.Add(new MauiContentPage
        {
            Title = "About",
            Content = new Microsoft.Maui.Controls.Label { Text = "About Page", TextColor = Colors.Black }
        });
        return tabbedPage;
    }

    [AvaloniaFact]
    public async Task Render_TabbedPage_Default()
    {
        await RenderTabbedPageToFile(CreateTabbedPageWithTabs);
        CompareImages(tolerance: 0.045);
    }

    [AvaloniaFact]
    public async Task Render_TabbedPage_BarBackgroundColor()
    {
        await RenderTabbedPageToFile(() =>
        {
            var tp = CreateTabbedPageWithTabs();
            tp.BarBackgroundColor = Colors.DarkBlue;
            return tp;
        });
        CompareImages(tolerance: 0.045);
    }

    [AvaloniaFact]
    public async Task Render_TabbedPage_BarBackgroundColor_Cleared()
    {
        await RenderTabbedPageToFile(
            () =>
            {
                var tp = CreateTabbedPageWithTabs();
                tp.BarBackgroundColor = Colors.DarkBlue;
                return tp;
            },
            afterSetup: (tp, handler) =>
            {
                tp.BarBackgroundColor = null;
                handler.UpdateValue(nameof(MauiTabbedPage.BarBackgroundColor));
            });
        CompareImages(tolerance: 0.045);
    }

    [AvaloniaFact]
    public async Task Render_TabbedPage_BarTextColor()
    {
        await RenderTabbedPageToFile(() =>
        {
            var tp = CreateTabbedPageWithTabs();
            tp.BarTextColor = Colors.Red;
            return tp;
        });
        CompareImages(tolerance: 0.045);
    }

    [AvaloniaFact]
    public async Task Render_TabbedPage_BarTextColor_Cleared()
    {
        await RenderTabbedPageToFile(
            () =>
            {
                var tp = CreateTabbedPageWithTabs();
                tp.BarTextColor = Colors.Red;
                return tp;
            },
            afterSetup: (tp, handler) =>
            {
                tp.BarTextColor = null;
                handler.UpdateValue(nameof(MauiTabbedPage.BarTextColor));
            });
        CompareImages(tolerance: 0.045);
    }

    [AvaloniaFact]
    public async Task Render_TabbedPage_SelectedTabColor()
    {
        await RenderTabbedPageToFile(() =>
        {
            var tp = CreateTabbedPageWithTabs();
            tp.SelectedTabColor = Colors.Orange;
            return tp;
        });
        CompareImages(tolerance: 0.045);
    }

    [AvaloniaFact]
    public async Task Render_TabbedPage_SelectedTabColor_Cleared()
    {
        await RenderTabbedPageToFile(
            () =>
            {
                var tp = CreateTabbedPageWithTabs();
                tp.SelectedTabColor = Colors.Orange;
                return tp;
            },
            afterSetup: (tp, handler) =>
            {
                tp.SelectedTabColor = null;
                handler.UpdateValue(nameof(MauiTabbedPage.SelectedTabColor));
            });
        CompareImages(tolerance: 0.045);
    }

    [AvaloniaFact]
    public async Task Render_TabbedPage_UnselectedTabColor()
    {
        await RenderTabbedPageToFile(() =>
        {
            var tp = CreateTabbedPageWithTabs();
            tp.UnselectedTabColor = Colors.Gray;
            return tp;
        });
        CompareImages(tolerance: 0.045);
    }

    [AvaloniaFact]
    public async Task Render_TabbedPage_UnselectedTabColor_Cleared()
    {
        await RenderTabbedPageToFile(
            () =>
            {
                var tp = CreateTabbedPageWithTabs();
                tp.UnselectedTabColor = Colors.Gray;
                return tp;
            },
            afterSetup: (tp, handler) =>
            {
                tp.UnselectedTabColor = null;
                handler.UpdateValue(nameof(MauiTabbedPage.UnselectedTabColor));
            });
        CompareImages(tolerance: 0.045);
    }

    [AvaloniaFact]
    public async Task Render_TabbedPage_AllColors()
    {
        await RenderTabbedPageToFile(() =>
        {
            var tp = CreateTabbedPageWithTabs();
            tp.BarBackgroundColor = Colors.DarkBlue;
            tp.SelectedTabColor = Colors.Yellow;
            tp.UnselectedTabColor = Colors.LightGray;
            return tp;
        });
        CompareImages(tolerance: 0.045);
    }

    [AvaloniaFact]
    public async Task Render_TabbedPage_AllColors_Cleared()
    {
        await RenderTabbedPageToFile(
            () =>
            {
                var tp = CreateTabbedPageWithTabs();
                tp.BarBackgroundColor = Colors.DarkBlue;
                tp.SelectedTabColor = Colors.Yellow;
                tp.UnselectedTabColor = Colors.LightGray;
                return tp;
            },
            afterSetup: (tp, handler) =>
            {
                tp.BarBackgroundColor = null;
                tp.SelectedTabColor = null;
                tp.UnselectedTabColor = null;
                handler.UpdateValue(nameof(MauiTabbedPage.BarBackgroundColor));
                handler.UpdateValue(nameof(MauiTabbedPage.SelectedTabColor));
                handler.UpdateValue(nameof(MauiTabbedPage.UnselectedTabColor));
            });
        CompareImages(tolerance: 0.045);
    }

    [AvaloniaFact]
    public async Task Render_TabbedPage_BarBackground_Brush()
    {
        await RenderTabbedPageToFile(() =>
        {
            var tp = CreateTabbedPageWithTabs();
            tp.BarBackground = new LinearGradientBrush
            {
                GradientStops = new GradientStopCollection
                {
                    new GradientStop(Colors.Blue, 0f),
                    new GradientStop(Colors.Green, 1f)
                }
            };
            return tp;
        });
        CompareImages(tolerance: 0.045);
    }

    [AvaloniaFact]
    public async Task Render_TabbedPage_BarBackground_Brush_Cleared()
    {
        await RenderTabbedPageToFile(
            () =>
            {
                var tp = CreateTabbedPageWithTabs();
                tp.BarBackground = new SolidColorBrush(Colors.Purple);
                return tp;
            },
            afterSetup: (tp, handler) =>
            {
                tp.BarBackground = null;
                handler.UpdateValue(nameof(MauiTabbedPage.BarBackground));
            });
        CompareImages(tolerance: 0.045);
    }
}
