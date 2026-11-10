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
}
