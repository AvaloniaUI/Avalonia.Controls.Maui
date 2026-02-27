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

/// <summary>
/// Abstract base class for Avalonia-based MAUI applications that bridges the Avalonia application lifecycle with the MAUI platform.
/// </summary>
/// <remarks>
/// Subclasses must implement <see cref="CreateMauiApp"/> to provide the configured <see cref="MauiApp"/> instance.
/// This class implements <see cref="IPlatformApplication"/> so that MAUI's platform services resolve against the Avalonia host.
/// </remarks>
public abstract class MauiAvaloniaApplication : Application, IPlatformApplication
{
    /// <summary>
    /// Gets or sets a value indicating whether the application is running in embedding mode,
    /// where MAUI is the host and Avalonia controls are embedded within MAUI views.
    /// When <see langword="true"/>, <see cref="OnFrameworkInitializationCompleted"/> skips
    /// the full MAUI bootstrap (which is handled by the native platform).
    /// </summary>
    internal static bool IsEmbeddingMode { get; set; }

    /// <summary>
    /// Creates and returns the configured <see cref="MauiApp"/> instance for this application.
    /// </summary>
    /// <returns>A fully configured <see cref="MauiApp"/>.</returns>
    protected abstract MauiApp CreateMauiApp();

    /// <summary>
    /// Gets the current <see cref="MauiAvaloniaApplication"/> instance.
    /// </summary>
    public static new MauiAvaloniaApplication Current => (MauiAvaloniaApplication)global::Avalonia.Application.Current!;


    /// <summary>
    /// Gets the main Avalonia <see cref="Window"/> created during application initialization.
    /// </summary>
    public Window MainWindow { get; protected set; } = null!;

    /// <summary>
    /// Gets the application-level <see cref="IServiceProvider"/> resolved from the MAUI dependency injection container.
    /// </summary>
    public IServiceProvider Services { get; protected set; } = null!;

    /// <summary>
    /// Gets the MAUI <see cref="IApplication"/> instance managed by this host.
    /// </summary>
    public IApplication Application { get; protected set; } = null!;


    /// <summary>
    /// The application-scoped MauiContext that contains the Avalonia Application.
    /// </summary>
    protected IMauiContext? ApplicationContext { get; private set; }

    /// <summary>
    /// Bootstraps the MAUI application within the Avalonia framework initialization pipeline.
    /// </summary>
    /// <remarks>
    /// This method creates the <see cref="MauiApp"/>, registers platform services, sets up the DI scope,
    /// connects the MAUI <see cref="IApplication"/> to its handler, and creates the platform window or single-view content
    /// depending on the active <see cref="Avalonia.Controls.ApplicationLifetimes"/> lifetime.
    /// </remarks>
    public override void OnFrameworkInitializationCompleted()
    {
        // In embedding mode, MAUI is the host and handles its own bootstrap.
        // Skip the full MAUI initialization to avoid a circular CreateMauiApp() call.
        if (IsEmbeddingMode)
        {
            base.OnFrameworkInitializationCompleted();
            return;
        }

        IPlatformApplication.Current = this;

        Styles.Add(new ControlStyles());

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
        var mauiContext = MakeAvaloniaWindowScope(ApplicationContext!);

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
        var mauiContext = MakeAvaloniaWindowScope(ApplicationContext!);

        Services.InvokeLifecycleEvents<AvaloniaLifecycle.OnMauiContextCreated>(del => del(mauiContext));

        var activationState = new ActivationState(mauiContext);
        var window = Application.CreateWindow(activationState);

        var avaloniaWindow = window.ToPlatform(mauiContext) as MauiAvaloniaWindow
            ?? throw new InvalidOperationException($"The window handler for {window.GetType().FullName} must be a {nameof(MauiAvaloniaWindow)}");

        Services.InvokeLifecycleEvents<AvaloniaLifecycle.OnWindowCreated>(del => del(avaloniaWindow));

        return avaloniaWindow;
    }

    /// <summary>
    /// Creates a window-scoped MauiContext with a proper DI scope.
    /// </summary>
    private static IMauiContext MakeAvaloniaWindowScope(IMauiContext mauiContext)
    {
        var scope = mauiContext.Services.CreateScope();
        var scopedContext = new MauiContext(scope.ServiceProvider);
        scopedContext.SetWindowScope(scope);
        scopedContext.InitializeScopedServices();
        return scopedContext;
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