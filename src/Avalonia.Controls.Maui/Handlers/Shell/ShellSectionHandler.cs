using System.Linq;
using Avalonia.Controls.Maui.Platform;
using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Internals;
using Microsoft.Maui.Handlers;
using AvaloniaNavigationPage = Avalonia.Controls.NavigationPage;

namespace Avalonia.Controls.Maui.Handlers.Shell;

/// <summary>Avalonia handler for <see cref="ShellSection"/>.</summary>
public partial class ShellSectionHandler : ElementHandler<ShellSection, AvaloniaNavigationPage>
{
    /// <summary>Property mapper for <see cref="ShellSectionHandler"/>.</summary>
    public static IPropertyMapper<ShellSection, ShellSectionHandler> Mapper =
        new PropertyMapper<ShellSection, ShellSectionHandler>(ElementHandler.ElementMapper)
        {
            [nameof(ShellSection.CurrentItem)] = MapCurrentItem,
            [nameof(ShellSection.Items)] = MapItems,
        };

    /// <summary>Command mapper for <see cref="ShellSectionHandler"/>.</summary>
    public static CommandMapper<ShellSection, ShellSectionHandler> CommandMapper =
        new CommandMapper<ShellSection, ShellSectionHandler>(ElementHandler.ElementCommandMapper)
        {
            [nameof(IStackNavigation.RequestNavigation)] = RequestNavigation
        };

    AvaloniaNavigationPage? _navigationPage;
    ShellStackNavigationManager? _navigationManager;
    bool _syncing;

    /// <summary>Initializes a new instance of <see cref="ShellSectionHandler"/>.</summary>
    public ShellSectionHandler() : base(Mapper, CommandMapper) { }

    /// <summary>Initializes a new instance of <see cref="ShellSectionHandler"/>.</summary>
    /// <param name="mapper">The property mapper to use.</param>
    /// <param name="commandMapper">The command mapper to use.</param>
    public ShellSectionHandler(IPropertyMapper? mapper, CommandMapper? commandMapper)
        : base(mapper ?? Mapper, commandMapper ?? CommandMapper) { }

    /// <summary>Creates the Avalonia platform view for this handler.</summary>
    protected override AvaloniaNavigationPage CreatePlatformElement()
    {
        _navigationPage = new AvaloniaNavigationPage
        {
            HorizontalAlignment = Layout.HorizontalAlignment.Stretch,
            VerticalAlignment = Layout.VerticalAlignment.Stretch,
            MinWidth = 0,
            MinHeight = 0,
        };

        return _navigationPage;
    }

    /// <inheritdoc/>
    protected override void ConnectHandler(AvaloniaNavigationPage platformView)
    {
        base.ConnectHandler(platformView);

        if (_navigationPage != null && MauiContext != null)
        {
            _navigationManager = new ShellStackNavigationManager(MauiContext);
            _navigationManager.Connect(VirtualView, _navigationPage);
        }

        if (VirtualView is IShellSectionController sectionController)
            sectionController.NavigationRequested += OnNavigationRequested;

        platformView.AttachedToVisualTree += OnAttachedToVisualTree;

        SyncNavigationStack(animated: false);
    }

    /// <inheritdoc/>
    protected override void DisconnectHandler(AvaloniaNavigationPage platformView)
    {
        if (VirtualView is IShellSectionController sectionController)
            sectionController.NavigationRequested -= OnNavigationRequested;

        if (_navigationManager != null && _navigationPage != null)
            _navigationManager.Disconnect(VirtualView, _navigationPage);

        _navigationManager = null;
        platformView.AttachedToVisualTree -= OnAttachedToVisualTree;

        base.DisconnectHandler(platformView);
    }

    /// <summary>Maps the CurrentItem property to the platform view.</summary>
    /// <param name="handler">The shell section handler.</param>
    /// <param name="section">The MAUI ShellSection virtual view.</param>
    public static void MapCurrentItem(ShellSectionHandler handler, ShellSection section)
        => handler.SyncNavigationStack(animated: false);

    /// <summary>Maps the Items property to the platform view.</summary>
    /// <param name="handler">The shell section handler.</param>
    /// <param name="section">The MAUI ShellSection virtual view.</param>
    public static void MapItems(ShellSectionHandler handler, ShellSection section)
        => handler.SyncNavigationStack(animated: false);

    /// <summary>Handles a navigation request command from MAUI's TCS lifecycle.</summary>
    /// <param name="handler">The shell section handler.</param>
    /// <param name="view">The stack navigation view.</param>
    /// <param name="arg">The navigation request argument.</param>
    public static void RequestNavigation(ShellSectionHandler handler, IStackNavigation view, object? arg)
    {
        if (arg is NavigationRequest request && handler._navigationManager != null)
            _ = handler._navigationManager.NavigateTo(request);
    }

    /// <summary>
    /// Synchronizes the navigation stack by building the target stack from the
    /// current ShellContent page and any pushed pages, then requesting navigation
    /// through MAUI's TCS lifecycle via <c>((IStackNavigation)VirtualView).RequestNavigation()</c>.
    /// </summary>
    /// <param name="animated">Whether the navigation should be animated.</param>
    private void SyncNavigationStack(bool animated)
    {
        if (_navigationManager == null || VirtualView == null || _navigationPage == null || MauiContext == null)
            return;

        // Guard against re-entrancy: RequestNavigation on VirtualView synchronously
        // invokes the handler command which calls NavigateTo.
        if (_syncing)
            return;

        if (VirtualView.CurrentItem is not IShellContentController contentController)
            return;

        var page = contentController.GetOrCreateContent();
        if (page == null)
            return;

        // Build the target stack: root page from ShellContent + any pushed pages
        var targetStack = new List<IView> { page };

        var externalStack = VirtualView.Navigation?.NavigationStack;
        if (externalStack != null && externalStack.Count > 1)
        {
            for (int i = 1; i < externalStack.Count; i++)
            {
                if (externalStack[i] is IView pushed)
                    targetStack.Add(pushed);
            }
        }

        // Skip if the manager already has this exact stack
        var currentStack = _navigationManager.NavigationStack;
        if (currentStack.Count == targetStack.Count
            && currentStack.Count > 0
            && ReferenceEquals(currentStack[currentStack.Count - 1], targetStack[targetStack.Count - 1]))
        {
            return;
        }

        _syncing = true;
        try
        {
            ((IStackNavigation)VirtualView).RequestNavigation(
                new NavigationRequest(targetStack, animated));
        }
        finally
        {
            _syncing = false;
        }
    }

    private void OnNavigationRequested(object? sender, NavigationRequestedEventArgs e)
        => SyncNavigationStack(animated: e.Animated);

    private void OnAttachedToVisualTree(object? sender, VisualTreeAttachmentEventArgs e)
        => SyncNavigationStack(animated: false);
}
