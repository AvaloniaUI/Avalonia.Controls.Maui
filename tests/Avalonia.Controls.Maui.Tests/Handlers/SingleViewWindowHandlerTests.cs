using Avalonia.Controls;
using Avalonia.Controls.Maui.Platform;
using Avalonia.Headless.XUnit;
using Avalonia.Threading;
using Microsoft.Maui;
using MauiContentPage = Microsoft.Maui.Controls.ContentPage;
using MauiWindow = Microsoft.Maui.Controls.Window;
using SingleViewWindowHandler = Avalonia.Controls.Maui.Handlers.SingleViewWindowHandler;

namespace Avalonia.Controls.Maui.Tests.Handlers;

public class SingleViewWindowHandlerTests : HandlerTestBase
{
    private (MauiWindow Window, MauiAvaloniaContent Content) CreateWindow()
    {
        var window = new MauiWindow(new MauiContentPage());
        var handler = new SingleViewWindowHandler();
        handler.SetMauiContext(MauiContext);
        window.Handler = handler;
        handler.SetVirtualView(window);

        return (window, (MauiAvaloniaContent)handler.PlatformView!);
    }

    [AvaloniaFact(DisplayName = "Window.FlowDirection flows to the single-view content")]
    public async Task FlowDirectionMapsToContent()
    {
        await InvokeOnMainThreadAsync(() =>
        {
            var (window, content) = CreateWindow();
            var host = new Window { Content = content };
            host.Show();
            Dispatcher.UIThread.RunJobs();

            var page = (Control)window.Page!.Handler!.PlatformView!;
            Assert.Equal(Media.FlowDirection.LeftToRight, page.FlowDirection);

            window.FlowDirection = FlowDirection.RightToLeft;
            Assert.Equal(Media.FlowDirection.RightToLeft, content.FlowDirection);
            Assert.Equal(Media.FlowDirection.RightToLeft, page.FlowDirection);

            window.FlowDirection = FlowDirection.LeftToRight;
            Assert.Equal(Media.FlowDirection.LeftToRight, page.FlowDirection);

            host.Close();
        });
    }

    [AvaloniaFact(DisplayName = "The single-view content's size is reported as the Window frame")]
    public async Task ContentSizeUpdatesWindowFrame()
    {
        await InvokeOnMainThreadAsync(() =>
        {
            var (window, content) = CreateWindow();
            var sizeChanged = 0;
            window.SizeChanged += (_, _) => sizeChanged++;

            var host = new Window { Width = 1280, Height = 800, Content = content };
            host.Show();
            Dispatcher.UIThread.RunJobs();

            Assert.Equal(1280, window.Width);
            Assert.Equal(800, window.Height);
            Assert.Equal(1, sizeChanged);

            host.Width = 900;
            Dispatcher.UIThread.RunJobs();

            Assert.Equal(900, window.Width);
            Assert.Equal(800, window.Height);
            Assert.Equal(2, sizeChanged);

            host.Close();
        });
    }
}
