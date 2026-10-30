using CommunityToolkit.Maui;
using Microsoft.Maui.Hosting;
using AvaloniaDispatcher = Avalonia.Threading.Dispatcher;
using MauiWindow = Microsoft.Maui.Controls.Window;
using MauiWindowHandler = Avalonia.Controls.Maui.Handlers.WindowHandler;

namespace Avalonia.Controls.Maui.Tests.CommunityToolkit;

/// <summary>
/// Hosts Community Toolkit views in a real MAUI window so that layout, gestures, input,
/// animations and modal navigation run the way they do in an app.
/// </summary>
public abstract class ToolkitTestBase : HandlerTestBase
{
    // Toolkit views need the app's dispatcher when they are created, before the window exists
    protected ToolkitTestBase() => EnsureHandlerCreated();

    protected override MauiAppBuilder ConfigureBuilder(MauiAppBuilder mauiAppBuilder)
    {
#pragma warning disable CA1416 // The toolkit marks this as supported on the native platforms only
        return base.ConfigureBuilder(mauiAppBuilder).UseMauiCommunityToolkit();
#pragma warning restore CA1416
    }

    /// <summary>
    /// Shows <paramref name="content"/> in a ContentPage inside a MAUI window, lays it out, runs
    /// <paramref name="test"/> and closes the window.
    /// </summary>
    protected Task RunInWindowAsync(Microsoft.Maui.Controls.View content, Func<ToolkitWindow, Task> test, double width = 400, double height = 600) =>
        RunInWindowAsync(new Microsoft.Maui.Controls.ContentPage { Content = content }, test, width, height);

    /// <summary>
    /// Shows <paramref name="page"/> in a MAUI window, lays it out, runs <paramref name="test"/>
    /// and closes the window.
    /// </summary>
    protected async Task RunInWindowAsync(Microsoft.Maui.Controls.Page page, Func<ToolkitWindow, Task> test, double width = 400, double height = 600)
    {
        EnsureHandlerCreated();

        var mauiWindow = new MauiWindow(page);
        var window = await InvokeOnMainThreadAsync(() =>
        {
            var handler = CreateHandler<MauiWindowHandler>(mauiWindow);
            var platformWindow = handler.PlatformView;
            platformWindow.Width = width;
            platformWindow.Height = height;
            platformWindow.Show();
            platformWindow.UpdateLayout();
            return new ToolkitWindow(mauiWindow, platformWindow);
        });

        try
        {
            await test(window);
        }
        finally
        {
            await InvokeOnMainThreadAsync(() => window.Platform.Close());
        }
    }

    /// <summary>
    /// Runs the dispatcher and lays out the window until <paramref name="condition"/> is true,
    /// or fails after <paramref name="timeoutMilliseconds"/>.
    /// </summary>
    protected static async Task WaitUntilAsync(ToolkitWindow window, Func<bool> condition, int timeoutMilliseconds = 3000, string? message = null)
    {
        var timeout = DateTime.UtcNow.AddMilliseconds(timeoutMilliseconds);
        while (true)
        {
            AvaloniaDispatcher.UIThread.RunJobs();
            window.Platform.UpdateLayout();
            if (condition())
                return;

            if (DateTime.UtcNow > timeout)
                Assert.Fail(message ?? "The condition was not met before the timeout.");

            await Task.Delay(10);
        }
    }

    /// <summary>
    /// Gets the Avalonia control for a MAUI view.
    /// </summary>
    protected static Control PlatformViewOf(Microsoft.Maui.IView view) =>
        Assert.IsAssignableFrom<Control>(view.Handler?.PlatformView);

    /// <summary>
    /// Gets the bounds of a MAUI view's Avalonia control relative to the window.
    /// </summary>
    protected static Rect BoundsInWindow(ToolkitWindow window, Microsoft.Maui.IView view)
    {
        var control = PlatformViewOf(view);
        var topLeft = control.TranslatePoint(default, window.Platform)!.Value;
        return new Rect(topLeft, control.Bounds.Size);
    }

    /// <summary>
    /// Clicks the center of a MAUI view with the left mouse button.
    /// </summary>
    protected static void Click(ToolkitWindow window, Microsoft.Maui.IView view)
    {
        var center = BoundsInWindow(window, view).Center;
        Avalonia.Headless.HeadlessWindowExtensions.MouseDown(window.Platform, center, Avalonia.Input.MouseButton.Left);
        Avalonia.Headless.HeadlessWindowExtensions.MouseUp(window.Platform, center, Avalonia.Input.MouseButton.Left);
        AvaloniaDispatcher.UIThread.RunJobs();
    }

    protected sealed record ToolkitWindow(MauiWindow Maui, Window Platform);
}
