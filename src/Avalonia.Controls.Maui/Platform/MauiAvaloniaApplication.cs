using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Controls.Maui.LifecycleEvents;
using Microsoft.Maui.LifecycleEvents;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Maui;
using Microsoft.Maui.Hosting;
using Microsoft.Maui.Platform;
using System;
using System.Collections.Generic;
using System.Text;

namespace Avalonia.Controls.Maui.Platform;

public abstract class MauiAvaloniaApplication : Application, IPlatformApplication
{
    protected abstract MauiApp CreateMauiApp();

    public static new MauiAvaloniaApplication Current => (MauiAvaloniaApplication)global::Avalonia.Application.Current!;


    public Window MainWindow { get; protected set; } = null!;

    public IServiceProvider Services { get; protected set; } = null!;

    public IApplication Application { get; protected set; } = null!;


    public override void OnFrameworkInitializationCompleted()
    {
        var mauiApp = CreateMauiApp();

        Services = mauiApp.Services;

        var args = EventArgs.Empty;
        Services.InvokeLifecycleEvents<AvaloniaLifecycle.OnLaunching>(del => del(this, args));

        Application = Services.GetRequiredService<IApplication>();

        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            var avaloniaWindow = CreatePlatformWindow();
            desktop.MainWindow = avaloniaWindow;
            MainWindow = avaloniaWindow;
        }
        else if (ApplicationLifetime is ISingleViewApplicationLifetime singleView)
        {
            var content = CreatePlatformContent();
            singleView.MainView = content;
        }

        Services.InvokeLifecycleEvents<AvaloniaLifecycle.OnLaunched>(del => del(this, args));

        base.OnFrameworkInitializationCompleted();
    }

    private Control CreatePlatformContent()
    {
        var mauiContext = new MauiContext(Services);

        Services.InvokeLifecycleEvents<AvaloniaLifecycle.OnMauiContextCreated>(del => del(mauiContext));

        var activationState = new ActivationState(mauiContext);
        var window = Application.CreateWindow(activationState);

        var content = window.ToPlatform(mauiContext) as Control
            ?? throw new InvalidOperationException($"The window handler for {window.GetType().FullName} must return a Control");

        // Note: OnWindowCreated event expects a Window, but for single-view platforms we only have a Control
        // Skip the event for now - consider creating a separate event for single-view platforms in the future

        return content;
    }

    private MauiAvaloniaWindow CreatePlatformWindow()
    {

        var mauiContext = new MauiContext(Services);

        Services.InvokeLifecycleEvents<AvaloniaLifecycle.OnMauiContextCreated>(del => del(mauiContext));

        var activationState = new ActivationState(mauiContext);
        var window = Application.CreateWindow(activationState);

        var test = window.ToPlatform(mauiContext);
        var avaloniaWindow = window.ToPlatform(mauiContext) as MauiAvaloniaWindow
            ?? throw new InvalidOperationException($"The window handler for {window.GetType().FullName} must be a {nameof(MauiAvaloniaWindow)}");

        Services.InvokeLifecycleEvents<AvaloniaLifecycle.OnWindowCreated>(del => del(avaloniaWindow));

        return avaloniaWindow;
    }
}