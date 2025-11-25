using Avalonia.Controls.ApplicationLifetimes;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Maui;
using Microsoft.Maui.Handlers;
using Microsoft.Maui.Platform;

namespace Avalonia.Controls.Maui.Handlers;

public partial class ApplicationHandler : ElementHandler<IApplication, Application>
{
    public const string TerminateCommandKey = "Terminate";

    public static IPropertyMapper<IApplication, ApplicationHandler> Mapper = new PropertyMapper<IApplication, ApplicationHandler>(ElementMapper)
    {
    };

    public static CommandMapper<IApplication, ApplicationHandler> CommandMapper = new(ElementCommandMapper)
    {
        [TerminateCommandKey] = MapTerminate,
        [nameof(IApplication.OpenWindow)] = MapOpenWindow,
        [nameof(IApplication.CloseWindow)] = MapCloseWindow,
        [nameof(IApplication.ActivateWindow)] = MapActivateWindow,
    };

    ILogger<ApplicationHandler>? _logger;

    public ApplicationHandler()
        : base(Mapper, CommandMapper)
    {
    }

    public ApplicationHandler(IPropertyMapper? mapper)
        : base(mapper ?? Mapper, CommandMapper)
    {
    }

    public ApplicationHandler(IPropertyMapper? mapper, CommandMapper? commandMapper)
        : base(mapper ?? Mapper, commandMapper ?? CommandMapper)
    {
    }

    ILogger? Logger =>
        _logger ??= MauiContext?.Services?.GetService<ILoggerFactory>()?.CreateLogger<ApplicationHandler>();

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
            var avaloniaWindow = window.ToPlatform(mauiContext) as global::Avalonia.Controls.Window;
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
            (window.Handler?.PlatformView as global::Avalonia.Controls.Window)?.Close();
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
            if (window.Handler?.PlatformView is global::Avalonia.Controls.Window avaloniaWindow)
            {
                avaloniaWindow.Activate();
            }
        }
    }
}
