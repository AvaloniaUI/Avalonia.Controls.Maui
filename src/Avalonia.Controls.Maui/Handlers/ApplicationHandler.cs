using Microsoft.Maui.Handlers;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Styling;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Maui;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Platform;

namespace Avalonia.Controls.Maui.Handlers;

/// <summary>
/// Avalonia handler for <see cref="IApplication"/>.
/// </summary>
public partial class ApplicationHandler : ElementHandler<IApplication, Application>
{
    /// <summary>
    /// The command key used to terminate the application.
    /// </summary>
    public const string TerminateCommandKey = "Terminate";

    /// <summary>
    /// Property mapper for <see cref="ApplicationHandler"/>.
    /// </summary>
    public static IPropertyMapper<IApplication, ApplicationHandler> Mapper = new PropertyMapper<IApplication, ApplicationHandler>(ElementMapper)
    {
        [nameof(IApplication.UserAppTheme)] = MapUserAppTheme,
    };

    /// <summary>
    /// Command mapper for <see cref="ApplicationHandler"/>.
    /// </summary>
    public static CommandMapper<IApplication, ApplicationHandler> CommandMapper = new(ElementCommandMapper)
    {
        [TerminateCommandKey] = MapTerminate,
        [nameof(IApplication.OpenWindow)] = MapOpenWindow,
        [nameof(IApplication.CloseWindow)] = MapCloseWindow,
        [nameof(IApplication.ActivateWindow)] = MapActivateWindow,
    };

    ILogger<ApplicationHandler>? _logger;

    /// <summary>
    /// Initializes a new instance of <see cref="ApplicationHandler"/>.
    /// </summary>
    public ApplicationHandler()
        : base(Mapper, CommandMapper)
    {
    }

    /// <summary>
    /// Initializes a new instance of <see cref="ApplicationHandler"/>.
    /// </summary>
    /// <param name="mapper">The property mapper to use, or <see langword="null"/> to use the default.</param>
    public ApplicationHandler(IPropertyMapper? mapper)
        : base(mapper ?? Mapper, CommandMapper)
    {
    }

    /// <summary>
    /// Initializes a new instance of <see cref="ApplicationHandler"/>.
    /// </summary>
    /// <param name="mapper">The property mapper to use, or <see langword="null"/> to use the default.</param>
    /// <param name="commandMapper">The command mapper to use, or <see langword="null"/> to use the default.</param>
    public ApplicationHandler(IPropertyMapper? mapper, CommandMapper? commandMapper)
        : base(mapper ?? Mapper, commandMapper ?? CommandMapper)
    {
    }

    /// <summary>
    /// Gets the logger instance for this handler.
    /// </summary>
    ILogger? Logger =>
        _logger ??= MauiContext?.Services?.GetService<ILoggerFactory>()?.CreateLogger<ApplicationHandler>();

    /// <summary>
    /// Creates the Avalonia <see cref="Application"/> platform element for this handler.
    /// </summary>
    /// <returns>The Avalonia <see cref="Application"/> instance from the service provider.</returns>
    protected override Application CreatePlatformElement() =>
        MauiContext?.Services?.GetService<Application>() ?? throw new InvalidOperationException($"MauiContext did not have a valid application.");

    /// <summary>
    /// Maps the abstract "Terminate" command to the platform-specific implementations.
    /// </summary>
    /// <param name="handler">The associated handler.</param>
    /// <param name="application">The associated <see cref="IApplication"/> instance.</param>
    /// <param name="args">The associated command arguments.</param>
    public static void MapTerminate(ApplicationHandler handler, IApplication application, object? args)
    {
        if (handler.PlatformView?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktopLifetime)
        {
            desktopLifetime.Shutdown();
        }
        else if (handler.PlatformView?.ApplicationLifetime is IControlledApplicationLifetime controlledLifetime)
        {
            controlledLifetime.Shutdown();
        }
    }

    /// <summary>
    /// Maps the abstract <see cref="IApplication.OpenWindow"/> command to the platform-specific implementations.
    /// </summary>
    /// <param name="handler">The associated handler.</param>
    /// <param name="application">The associated <see cref="IApplication"/> instance.</param>
    /// <param name="args">The associated command arguments.</param>
    public static void MapOpenWindow(ApplicationHandler handler, IApplication application, object? args)
    {
        if (args is OpenWindowRequest request && handler.MauiContext is IMauiContext mauiContext)
        {
            var activationState = new ActivationState(mauiContext, request.State ?? new PersistedState());
            var window = application.CreateWindow(activationState);

            // For WASM, this will return as MauiAvaloniaContent from SingleViewWindowHandler, so it will not show.
            // We don't support multiple windows in WASM in Avalonia currently.
            var avaloniaWindow = window.ToPlatform(mauiContext) as Window;
            avaloniaWindow?.Show();
        }
    }

    /// <summary>
    /// Maps the abstract <see cref="IApplication.CloseWindow"/> command to the platform-specific implementations.
    /// </summary>
    /// <param name="handler">The associated handler.</param>
    /// <param name="application">The associated <see cref="IApplication"/> instance.</param>
    /// <param name="args">The associated command arguments.</param>
    public static void MapCloseWindow(ApplicationHandler handler, IApplication application, object? args)
    {
        if (args is IWindow window)
        {
            (window.Handler?.PlatformView as Window)?.Close();
        }
    }

    /// <summary>
    /// Maps the abstract <see cref="IApplication.ActivateWindow"/> command to the platform-specific implementations.
    /// </summary>
    /// <param name="handler">The associated handler.</param>
    /// <param name="application">The associated <see cref="IApplication"/> instance.</param>
    /// <param name="args">The associated command arguments.</param>
    public static void MapActivateWindow(ApplicationHandler handler, IApplication application, object? args)
    {
        if (args is IWindow window)
        {
            if (window.Handler?.PlatformView is Window avaloniaWindow)
            {
                avaloniaWindow.Activate();
            }
        }
    }

    /// <summary>
    /// Maps the MAUI UserAppTheme property to Avalonia's RequestedThemeVariant.
    /// This enables setting the Avalonia theme when the MAUI application theme is changed.
    /// </summary>
    /// <param name="handler">The associated handler.</param>
    /// <param name="application">The associated <see cref="IApplication"/> instance.</param>
    public static void MapUserAppTheme(ApplicationHandler handler, IApplication application)
    {
        if (handler.PlatformView is Application avaloniaApp)
        {
            var userTheme = application.UserAppTheme;

            ThemeVariant? requestedTheme = userTheme switch
            {
                AppTheme.Dark => ThemeVariant.Dark,
                AppTheme.Light => ThemeVariant.Light,
                AppTheme.Unspecified => null, // Use system theme
                _ => null
            };

            avaloniaApp.RequestedThemeVariant = requestedTheme;
        }
    }
}
