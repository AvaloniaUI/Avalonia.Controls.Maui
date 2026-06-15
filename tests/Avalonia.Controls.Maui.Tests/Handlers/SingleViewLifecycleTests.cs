using Avalonia.Controls;
using Avalonia.Controls.Maui.Platform;
using Avalonia.Headless.XUnit;
using Avalonia.Threading;
using Microsoft.Maui;
using MauiApplication = Microsoft.Maui.Controls.Application;
using MauiContentPage = Microsoft.Maui.Controls.ContentPage;
using MauiWindow = Microsoft.Maui.Controls.Window;
using SingleViewWindowHandler = Avalonia.Controls.Maui.Handlers.SingleViewWindowHandler;

namespace Avalonia.Controls.Maui.Tests.Handlers;

/// <summary>
/// Verifies that the single-view (browser/embedded) content lifecycle is bridged to MAUI's
/// <see cref="IWindow"/> lifecycle, which in turn drives the MAUI application lifecycle.
/// </summary>
public class SingleViewLifecycleTests : HandlerTestBase
{
    private sealed class LifecycleTestApplication : MauiApplication
    {
        public int StartCount { get; private set; }

        protected override void OnStart() => StartCount++;
    }

    private sealed class LifecycleHarness
    {
        public required SingleViewWindowHandler Handler { get; init; }
        public required MauiWindow Window { get; init; }
        public required MauiAvaloniaContent Content { get; init; }
        public required LifecycleTestApplication App { get; init; }
    }

    private LifecycleHarness CreateHarness()
    {
        var app = new LifecycleTestApplication();
        var page = new MauiContentPage();
        var window = new MauiWindow(page);

        // Window.Application resolves from the logical parent; parenting the window to the
        // application is what lets IWindow.Created() cascade to the app.
        window.Parent = app;

        var handler = new SingleViewWindowHandler();
        handler.SetMauiContext(MauiContext);
        window.Handler = handler;
        handler.SetVirtualView(window);

        var content = (MauiAvaloniaContent)handler.PlatformView!;

        return new LifecycleHarness
        {
            Handler = handler,
            Window = window,
            Content = content,
            App = app,
        };
    }

    [AvaloniaFact(DisplayName = "Loading the single-view content raises Window.Created and Application.OnStart")]
    public async Task LoadingContentRaisesCreatedAndStart()
    {
        await InvokeOnMainThreadAsync(() =>
        {
            var harness = CreateHarness();
            var created = 0;
            harness.Window.Created += (_, _) => created++;

            // Attach the single-view content to a live visual tree so Loaded fires.
            var host = new Window { Width = 400, Height = 300, Content = harness.Content };
            host.Show();
            Dispatcher.UIThread.RunJobs();

            Assert.Equal(1, created);
            Assert.Equal(1, harness.App.StartCount);

            host.Content = null;
            host.Close();
        });
    }

    [AvaloniaFact(DisplayName = "Unloading the single-view content raises Window.Destroying")]
    public async Task UnloadingContentRaisesDestroying()
    {
        await InvokeOnMainThreadAsync(() =>
        {
            var harness = CreateHarness();

            var host = new Window { Width = 400, Height = 300, Content = harness.Content };
            host.Show();
            Dispatcher.UIThread.RunJobs();

            var destroying = 0;
            harness.Window.Destroying += (_, _) => destroying++;

            host.Content = null;

            Assert.Equal(1, destroying);

            host.Close();
        });
    }

    [AvaloniaFact(DisplayName = "Application.OnStart is only raised once when the content reloads")]
    public async Task StartIsRaisedOnce()
    {
        await InvokeOnMainThreadAsync(() =>
        {
            var harness = CreateHarness();

            var host = new Window { Width = 400, Height = 300, Content = harness.Content };
            host.Show();
            Dispatcher.UIThread.RunJobs();

            // Detach and reattach the content; Created/OnStart must not fire again.
            host.Content = null;
            host.Content = harness.Content;
            Dispatcher.UIThread.RunJobs();

            Assert.Equal(1, harness.App.StartCount);

            host.Content = null;
            host.Close();
        });
    }
}
