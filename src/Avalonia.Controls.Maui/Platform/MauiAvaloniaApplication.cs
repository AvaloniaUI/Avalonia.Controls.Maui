using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Controls.Maui.LifecycleEvents;
using Avalonia.Styling;
using Microsoft.Maui.LifecycleEvents;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Maui;
using Microsoft.Maui.Hosting;
using Microsoft.Maui.Platform;
using System;
using Microsoft.Maui.ApplicationModel;

namespace Avalonia.Controls.Maui.Platform;

public abstract class MauiAvaloniaApplication : Application, IPlatformApplication
{
    protected abstract MauiApp CreateMauiApp();

    public static new MauiAvaloniaApplication Current => (MauiAvaloniaApplication)global::Avalonia.Application.Current!;


    public Window MainWindow { get; protected set; } = null!;

    public IServiceProvider Services { get; protected set; } = null!;

    public IApplication Application { get; protected set; } = null!;


    /// <summary>
    /// The application-scoped MauiContext that contains the Avalonia Application.
    /// </summary>
    protected IMauiContext? ApplicationContext { get; private set; }

    public override void OnFrameworkInitializationCompleted()
    {
        IPlatformApplication.Current = this;

        var mauiApp = CreateMauiApp();

        var rootContext = new MauiContext(mauiApp.Services);

        // Create application scope and register the Avalonia Application
        // We need to register with the correct type (Application) since MakeApplicationScope
        // registers as Object for non-platform builds
        ApplicationContext = MakeAvaloniaApplicationScope(rootContext, this);

        Services = ApplicationContext.Services;

        var args = EventArgs.Empty;
        Services.InvokeLifecycleEvents<AvaloniaLifecycle.OnLaunching>(del => del(this, args));

        Application = Services.GetRequiredService<IApplication>();

        // Subscribe to Avalonia theme changes to notify MAUI's AppThemeBinding system
        ActualThemeVariantChanged += OnActualThemeVariantChanged;

        // Connect the MAUI Application to its handler
        this.SetApplicationHandler(Application, ApplicationContext);

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

    /// <summary>
    /// Creates an application-scoped MauiContext and registers the Avalonia Application
    /// with the correct type so it can be resolved by the ApplicationHandler.
    /// </summary>
    private static MauiContext MakeAvaloniaApplicationScope(MauiContext mauiContext, Application avaloniaApplication)
    {
        var scopedContext = new MauiContext(mauiContext.Services);

        // Register the Avalonia Application with the correct type
        // This allows ApplicationHandler.CreatePlatformElement() to resolve it via GetService<Application>()
        scopedContext.AddSpecific(avaloniaApplication);

        return scopedContext;
    }

    /// <summary>
    /// Handles Avalonia's theme variant changes and notifies MAUI's theme system.
    /// This enables AppThemeBinding to update when the theme changes.
    /// </summary>
    private void OnActualThemeVariantChanged(object? sender, EventArgs e)
    {
        Application?.ThemeChanged();
    }
}