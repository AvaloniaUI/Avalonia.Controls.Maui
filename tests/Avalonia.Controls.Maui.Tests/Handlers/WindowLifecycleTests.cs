using Avalonia.Controls;
using Avalonia.Controls.Maui.Platform;
using Avalonia.Headless.XUnit;
using Avalonia.Threading;
using Microsoft.Maui;
using MauiApplication = Microsoft.Maui.Controls.Application;
using MauiContentPage = Microsoft.Maui.Controls.ContentPage;
using MauiWindow = Microsoft.Maui.Controls.Window;
using WindowHandler = Avalonia.Controls.Maui.Handlers.WindowHandler;

namespace Avalonia.Controls.Maui.Tests.Handlers;

/// <summary>
/// Verifies that the Avalonia window lifecycle is bridged to MAUI's <see cref="IWindow"/> lifecycle,
/// which in turn drives the MAUI <see cref="MauiApplication"/> lifecycle (OnStart/OnResume/OnSleep).
/// </summary>
public class WindowLifecycleTests : HandlerTestBase
{
    private sealed class LifecycleTestApplication : MauiApplication
    {
        public int StartCount { get; private set; }
        public int ResumeCount { get; private set; }
        public int SleepCount { get; private set; }

        protected override void OnStart() => StartCount++;
        protected override void OnResume() => ResumeCount++;
        protected override void OnSleep() => SleepCount++;
    }

    private sealed class LifecycleHarness
    {
        public required WindowHandler Handler { get; init; }
        public required MauiWindow Window { get; init; }
        public required MauiAvaloniaWindow Platform { get; init; }
        public required LifecycleTestApplication App { get; init; }
    }

    private LifecycleHarness CreateHarness()
    {
        var app = new LifecycleTestApplication();
        var page = new MauiContentPage();
        var window = new MauiWindow(page) { Width = 400, Height = 300 };

        // Window.Application resolves from the logical parent; parenting the window to the
        // application is what lets IWindow.Created()/Stopped()/Resumed() cascade to the app.
        window.Parent = app;

        var handler = new WindowHandler();
        handler.SetMauiContext(MauiContext);
        window.Handler = handler;
        handler.SetVirtualView(window);

        var platform = (MauiAvaloniaWindow)handler.PlatformView!;

        return new LifecycleHarness
        {
            Handler = handler,
            Window = window,
            Platform = platform,
            App = app,
        };
    }

    [AvaloniaFact(DisplayName = "Opening the window raises Window.Created and Application.OnStart")]
    public async Task OpeningWindowRaisesCreatedAndStart()
    {
        await InvokeOnMainThreadAsync(() =>
        {
            var harness = CreateHarness();
            var created = 0;
            harness.Window.Created += (_, _) => created++;

            harness.Platform.Show();
            Dispatcher.UIThread.RunJobs();

            Assert.Equal(1, created);
            Assert.Equal(1, harness.App.StartCount);

            harness.Platform.Close();
        });
    }

    [AvaloniaFact(DisplayName = "Minimizing the window raises Window.Stopped and Application.OnSleep")]
    public async Task MinimizingWindowRaisesStoppedAndSleep()
    {
        await InvokeOnMainThreadAsync(() =>
        {
            var harness = CreateHarness();
            harness.Platform.Show();
            Dispatcher.UIThread.RunJobs();

            var stopped = 0;
            harness.Window.Stopped += (_, _) => stopped++;

            harness.Platform.WindowState = WindowState.Minimized;

            Assert.Equal(1, stopped);
            Assert.Equal(1, harness.App.SleepCount);

            harness.Platform.Close();
        });
    }

    [AvaloniaFact(DisplayName = "Restoring a minimized window raises Window.Resumed and Application.OnResume")]
    public async Task RestoringWindowRaisesResumedAndResume()
    {
        await InvokeOnMainThreadAsync(() =>
        {
            var harness = CreateHarness();
            harness.Platform.Show();
            Dispatcher.UIThread.RunJobs();
            harness.Platform.WindowState = WindowState.Minimized;

            var resumed = 0;
            harness.Window.Resumed += (_, _) => resumed++;

            harness.Platform.WindowState = WindowState.Normal;

            Assert.Equal(1, resumed);
            Assert.Equal(1, harness.App.ResumeCount);

            harness.Platform.Close();
        });
    }

    [AvaloniaFact(DisplayName = "Closing the window raises Window.Destroying")]
    public async Task ClosingWindowRaisesDestroying()
    {
        await InvokeOnMainThreadAsync(() =>
        {
            var harness = CreateHarness();
            harness.Platform.Show();
            Dispatcher.UIThread.RunJobs();

            var destroying = 0;
            harness.Window.Destroying += (_, _) => destroying++;

            harness.Platform.Close();

            Assert.Equal(1, destroying);
        });
    }

    [AvaloniaFact(DisplayName = "A window opened while minimized raises Stopped so a later restore is symmetric")]
    public async Task WindowOpenedWhileMinimizedRaisesStoppedThenResumed()
    {
        await InvokeOnMainThreadAsync(() =>
        {
            var harness = CreateHarness();

            var stopped = 0;
            var resumed = 0;
            harness.Window.Stopped += (_, _) => stopped++;
            harness.Window.Resumed += (_, _) => resumed++;

            // Open the window already minimized; Created must be followed by a Stopped so that the
            // later restore produces a matching Resumed rather than an orphaned one.
            harness.Platform.WindowState = WindowState.Minimized;
            harness.Platform.Show();
            Dispatcher.UIThread.RunJobs();

            Assert.Equal(1, stopped);
            Assert.Equal(1, harness.App.SleepCount);
            Assert.Equal(0, resumed);

            harness.Platform.WindowState = WindowState.Normal;

            Assert.Equal(1, resumed);
            Assert.Equal(1, harness.App.ResumeCount);
            Assert.Equal(1, stopped);

            harness.Platform.Close();
        });
    }

    // Avalonia's headless platform does not raise Activated/Deactivated focus events, so these
    // are driven through the lifecycle manager seam that the real platform events also call.
    [AvaloniaFact(DisplayName = "Activation forwards to Window.Activated and Window.Deactivated")]
    public async Task ActivationForwardsToWindow()
    {
        await InvokeOnMainThreadAsync(() =>
        {
            var harness = CreateHarness();
            harness.Platform.Show();

            var activated = 0;
            var deactivated = 0;
            harness.Window.Activated += (_, _) => activated++;
            harness.Window.Deactivated += (_, _) => deactivated++;

            var manager = harness.Handler.LifecycleManager!;

            manager.NotifyActivated();
            Assert.Equal(1, activated);
            Assert.True(harness.Window.IsActivated);

            manager.NotifyDeactivated();
            Assert.Equal(1, deactivated);
            Assert.False(harness.Window.IsActivated);

            harness.Platform.Close();
        });
    }

    [AvaloniaFact(DisplayName = "Repeated activation does not raise Window.Activated twice")]
    public async Task RepeatedActivationIsIdempotent()
    {
        await InvokeOnMainThreadAsync(() =>
        {
            var harness = CreateHarness();
            harness.Platform.Show();

            var activated = 0;
            harness.Window.Activated += (_, _) => activated++;

            var manager = harness.Handler.LifecycleManager!;
            manager.NotifyActivated();
            manager.NotifyActivated();

            Assert.Equal(1, activated);

            harness.Platform.Close();
        });
    }
}
