using System.Linq;
using Avalonia.Controls.Maui.Platform;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Maui;

namespace Avalonia.Controls.Maui.Handlers;

/// <summary>Avalonia handler for <see cref="IStackNavigationView"/>.</summary>
public partial class NavigationViewHandler : ViewHandler<IStackNavigationView, NavigationView>
{
    private StackNavigationManager? _navigationManager;
    private ILogger<NavigationViewHandler>? _logger;

    /// <summary>Property mapper for <see cref="NavigationViewHandler"/>.</summary>
    public static IPropertyMapper<IStackNavigationView, NavigationViewHandler> Mapper =
        new PropertyMapper<IStackNavigationView, NavigationViewHandler>(ViewMapper)
        {
        };

    /// <summary>Command mapper for <see cref="NavigationViewHandler"/>.</summary>
    public static CommandMapper<IStackNavigationView, NavigationViewHandler> CommandMapper =
        new(ViewCommandMapper)
        {
            [nameof(IStackNavigation.RequestNavigation)] = RequestNavigation
        };

    /// <summary>Initializes a new instance of <see cref="NavigationViewHandler"/>.</summary>
    public NavigationViewHandler() : base(Mapper, CommandMapper)
    {
    }

    /// <summary>Initializes a new instance of <see cref="NavigationViewHandler"/>.</summary>
    /// <param name="mapper">The property mapper to use, or <c>null</c> to use the default mapper.</param>
    public NavigationViewHandler(IPropertyMapper? mapper)
        : base(mapper ?? Mapper, CommandMapper)
    {
    }

    /// <summary>Initializes a new instance of <see cref="NavigationViewHandler"/>.</summary>
    /// <param name="mapper">The property mapper to use, or <c>null</c> to use the default mapper.</param>
    /// <param name="commandMapper">The command mapper to use, or <c>null</c> to use the default command mapper.</param>
    public NavigationViewHandler(IPropertyMapper? mapper, CommandMapper? commandMapper)
        : base(mapper ?? Mapper, commandMapper ?? CommandMapper)
    {
    }

    ILogger? Logger =>
        _logger ??= MauiContext?.Services?.GetService<ILoggerFactory>()?.CreateLogger<NavigationViewHandler>();

    /// <summary>Creates the Avalonia platform view for this handler.</summary>
    protected override NavigationView CreatePlatformView()
    {
        _navigationManager = CreateNavigationManager();

        if (VirtualView == null)
        {
            throw new InvalidOperationException($"{nameof(VirtualView)} must be set to create a NavigationView");
        }

        var view = new NavigationView
        {
            CrossPlatformLayout = VirtualView
        };

        Logger?.LogDebug("Created NavigationView platform control");

        return view;
    }

    /// <inheritdoc/>
    protected override void ConnectHandler(NavigationView platformView)
    {
        base.ConnectHandler(platformView);
        _navigationManager?.Connect(VirtualView, platformView);

        // Initialize navigation stack if pages already exist
        // NavigationPage adds pages before the handler is connected
        if (VirtualView is Microsoft.Maui.Controls.NavigationPage navPage &&
            navPage.Navigation?.NavigationStack?.Count > 0)
        {
            var navStack = navPage.Navigation.NavigationStack.Cast<IView>().ToList();
            var initialRequest = new NavigationRequest(navStack, animated: false);
            _navigationManager?.NavigateTo(initialRequest);
        }

        Logger?.LogDebug("Connected NavigationViewHandler");
    }

    /// <inheritdoc/>
    protected override void DisconnectHandler(NavigationView platformView)
    {
        _navigationManager?.Disconnect(VirtualView, platformView);
        base.DisconnectHandler(platformView);
        Logger?.LogDebug("Disconnected NavigationViewHandler");
    }

    /// <inheritdoc/>
    public override void SetVirtualView(IView view)
    {
        base.SetVirtualView(view);

        _ = PlatformView ?? throw new InvalidOperationException($"{nameof(PlatformView)} should have been set by base class.");
        _ = VirtualView ?? throw new InvalidOperationException($"{nameof(VirtualView)} should have been set by base class.");

        PlatformView.CrossPlatformLayout = VirtualView;
    }

    /// <summary>Maps the RequestNavigation command to the platform view.</summary>
    /// <param name="handler">The handler for the navigation view.</param>
    /// <param name="stackNavigation">The stack navigation interface.</param>
    /// <param name="args">The navigation request arguments.</param>
    public static void RequestNavigation(NavigationViewHandler handler, IStackNavigation stackNavigation, object? args)
    {
        if (handler is NavigationViewHandler platformHandler && args is NavigationRequest navigationRequest)
        {
            platformHandler._navigationManager?.NavigateTo(navigationRequest);
        }
        else
        {
            throw new InvalidOperationException("Args must be NavigationRequest");
        }
    }

    /// <summary>
    /// Creates the navigation manager for this platform.
    /// Can be overridden in derived classes for custom navigation behavior.
    /// </summary>
    protected virtual StackNavigationManager CreateNavigationManager() =>
        _navigationManager ??= new StackNavigationManager(MauiContext ?? throw new InvalidOperationException("MauiContext cannot be null"));
}
