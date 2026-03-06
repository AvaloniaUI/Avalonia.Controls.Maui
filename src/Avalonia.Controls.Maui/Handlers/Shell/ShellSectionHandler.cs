using Microsoft.Maui.Handlers;
using Avalonia.Animation;
using Microsoft.Maui;
using Microsoft.Maui.Controls;
using AvaloniaControl = Avalonia.Controls.Control;
using MauiShell = Microsoft.Maui.Controls.Shell;
using Avalonia.Controls.Maui.Extensions;
using MauiPage = Microsoft.Maui.Controls.Page;

namespace Avalonia.Controls.Maui.Handlers.Shell;

/// <summary>Avalonia handler for <see cref="ShellSection"/>.</summary>
public partial class ShellSectionHandler : ElementHandler<ShellSection, AvaloniaControl>, IStackNavigation
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

    TransitioningContentControl? _sectionContainer;
    readonly Stack<MauiPage> _navigationStack = new();
    MauiPage? _currentPage;
    bool _isNavigating;

    /// <summary>Default duration for content transitions.</summary>
    internal static readonly TimeSpan DefaultTransitionDuration = TimeSpan.FromMilliseconds(250);

    /// <summary>Duration for page slide push/pop transitions.</summary>
    internal static readonly TimeSpan PageSlideDuration = TimeSpan.FromMilliseconds(300);

    /// <summary>Duration for modal presentation transitions.</summary>
    internal static readonly TimeSpan ModalTransitionDuration = TimeSpan.FromMilliseconds(400);

    /// <summary>Initializes a new instance of <see cref="ShellSectionHandler"/>.</summary>
    public ShellSectionHandler() : base(Mapper, CommandMapper) { }

    /// <summary>Initializes a new instance of <see cref="ShellSectionHandler"/>.</summary>
    /// <param name="mapper">The property mapper to use.</param>
    /// <param name="commandMapper">The command mapper to use.</param>
    public ShellSectionHandler(IPropertyMapper? mapper, CommandMapper? commandMapper)
        : base(mapper ?? Mapper, commandMapper ?? CommandMapper) { }

    /// <summary>Creates the Avalonia platform view for this handler.</summary>
    protected override AvaloniaControl CreatePlatformElement()
    {
        _sectionContainer = new TransitioningContentControl
        {
            HorizontalAlignment = Layout.HorizontalAlignment.Stretch,
            VerticalAlignment = Layout.VerticalAlignment.Stretch,
            HorizontalContentAlignment = Layout.HorizontalAlignment.Stretch,
            VerticalContentAlignment = Layout.VerticalAlignment.Stretch,
            MinWidth = 0,
            MinHeight = 0
        };
        return _sectionContainer;
    }

    /// <inheritdoc/>
    protected override void ConnectHandler(AvaloniaControl platformView)
    {
        base.ConnectHandler(platformView);

        if (VirtualView is IShellSectionController sectionController)
            sectionController.NavigationRequested += OnNavigationRequested;

        UpdateCurrentItem();
        platformView.AttachedToVisualTree += OnAttachedToVisualTree;
    }

    /// <inheritdoc/>
    protected override void DisconnectHandler(AvaloniaControl platformView)
    {
        if (VirtualView is IShellSectionController sectionController)
            sectionController.NavigationRequested -= OnNavigationRequested;

        // Clear section container content to release any in-flight transition resources
        if (_sectionContainer != null)
        {
            _sectionContainer.PageTransition = null;
            _sectionContainer.Content = null;
        }

        // Release page references held by the navigation stack
        _navigationStack.Clear();
        _currentPage = null;

        platformView.AttachedToVisualTree -= OnAttachedToVisualTree;
        base.DisconnectHandler(platformView);
    }

    private void OnAttachedToVisualTree(object? sender, VisualTreeAttachmentEventArgs e)
    {
        if (_isNavigating)
            return;

        // Re-display when re-attached to visual tree (e.g. tab switching)
        _currentPage = null;
        SyncNavigationStack();
    }

    /// <summary>Maps the CurrentItem property to the platform view.</summary>
    /// <param name="handler">The shell section handler.</param>
    /// <param name="section">The MAUI ShellSection virtual view.</param>
    public static void MapCurrentItem(ShellSectionHandler handler, ShellSection section)
        => handler.UpdateCurrentItem();

    /// <summary>Maps the Items property to the platform view.</summary>
    /// <param name="handler">The shell section handler.</param>
    /// <param name="section">The MAUI ShellSection virtual view.</param>
    public static void MapItems(ShellSectionHandler handler, ShellSection section)
        => handler.UpdateCurrentItem();

    /// <summary>Handles a navigation request command.</summary>
    /// <param name="handler">The shell section handler.</param>
    /// <param name="view">The stack navigation view.</param>
    /// <param name="arg">The navigation request argument.</param>
    public static void RequestNavigation(ShellSectionHandler handler, IStackNavigation view, object? arg)
    {
        if (arg is NavigationRequest request)
            handler.HandleNavigationRequest(request);
    }

    private void OnNavigationRequested(object? sender, EventArgs e)
    {
        if (!_isNavigating)
            SyncNavigationStack();
    }
    
    private void HandleNavigationRequest(NavigationRequest request)
    {
        var stack = request.NavigationStack;
        if (stack == null || stack.Count == 0)
            return;

        _isNavigating = true;
        try
        {
            int oldCount = _navigationStack.Count;
            int newCount = stack.Count;
            bool isPush = newCount > oldCount;
            bool isPop = newCount < oldCount;

            // Capture the outgoing page before updating (needed for pop reverse animation)
            MauiPage? poppedPage = isPop ? _currentPage : null;

            // Update internal stack to match the request
            _navigationStack.Clear();
            foreach (var view in request.NavigationStack)
            {
                if (view is MauiPage page)
                    _navigationStack.Push(page);
            }

            if (_navigationStack.Count > 0)
            {
                var topPage = _navigationStack.Peek();
                DisplayPage(topPage, isPush, isPop, poppedPage);
            }
        }
        finally
        {
            _isNavigating = false;
        }

        // Notify MAUI that navigation completed (resolves internal TaskCompletionSource).
        // The try-catch handles edge cases where navigation state is out of sync
        // (e.g., external navigation requests that bypass Shell routing).
        try
        {
            if (VirtualView is IStackNavigation stackNavigation)
                stackNavigation.NavigationFinished(request.NavigationStack);
        }
        catch (InvalidOperationException)
        {
        }
    }
    
    /// <summary>Synchronizes the internal navigation stack with the MAUI navigation stack and displays the top page.</summary>
    public void SyncNavigationStack()
    {
        if (_isNavigating || VirtualView == null || _sectionContainer == null)
            return;

        var externalStack = VirtualView.Navigation?.NavigationStack;
        if (externalStack == null)
            return;

        int newCount = externalStack.Count;
        int oldCount = _navigationStack.Count;
        bool isPush = newCount > oldCount;
        bool isPop = newCount < oldCount;

        // Capture the outgoing page before updating
        MauiPage? poppedPage = isPop ? _currentPage : null;

        // Sync internal stack
        _navigationStack.Clear();
        foreach (var page in externalStack)
            _navigationStack.Push(page);

        // Determine top page
        MauiPage? topPage = _navigationStack.Count > 0 ? _navigationStack.Peek() : null;

        if (topPage == null && VirtualView.CurrentItem is IShellContentController cc)
            topPage = cc.GetOrCreateContent();

        if (topPage == null)
            return;

        // Skip if nothing changed and page is already displayed
        if (!isPush && !isPop && _currentPage == topPage)
            return;

        DisplayPage(topPage, isPush, isPop, poppedPage);
    }

    private void UpdateCurrentItem()
    {
        if (VirtualView?.CurrentItem == null || _sectionContainer == null || MauiContext == null)
            return;

        if (VirtualView.CurrentItem is IShellContentController contentController)
        {
            var page = contentController.GetOrCreateContent();
            if (page != null)
            {
                _navigationStack.Clear();
                _navigationStack.Push(page);
            }
        }

        SyncNavigationStack();
    }
    
    private void DisplayPage(MauiPage page, bool isPush, bool isPop, MauiPage? poppedPage)
    {
        if (_sectionContainer == null || MauiContext == null)
            return;

        // Skip if content is already correctly displayed
        if (_currentPage == page && page.Handler?.PlatformView is AvaloniaControl existing
            && _sectionContainer.Content == existing)
        {
            return;
        }

        _currentPage = page;


        // Read PresentationMode from the relevant page (incoming for push, outgoing for pop)
        var relevantPage = (isPop && poppedPage != null) ? poppedPage : page;
        var mode = MauiShell.GetPresentationMode(relevantPage);

        bool isNotAnimated = mode.HasFlag(PresentationMode.NotAnimated);
        bool isModal = mode.HasFlag(PresentationMode.Modal);

        // Determine transition based on presentation mode and direction
        IPageTransition? transition;

        if (isNotAnimated)
        {
            transition = null;
        }
        else if (isModal)
        {
            // Vertical slide (modal: bottom-to-top push, top-to-bottom pop)
            transition = isPop
                ? new ReversePageSlide(ModalTransitionDuration, PageSlide.SlideAxis.Vertical)
                : new PageSlide(ModalTransitionDuration, PageSlide.SlideAxis.Vertical);
        }
        else if (isPop)
        {
            // Standard horizontal slide (left-to-right pop)
            transition = new ReversePageSlide(PageSlideDuration, PageSlide.SlideAxis.Horizontal);
        }
        else if (isPush)
        {
            // Standard horizontal slide (right-to-left push)
            transition = new PageSlide(PageSlideDuration, PageSlide.SlideAxis.Horizontal);
        }
        else
        {
            // Default (tab switch, initial load): crossfade
            transition = new CrossFade(DefaultTransitionDuration);
        }

        // Clear old content without animation first to release any in-flight
        // transition's hidden presenter content (see ShellExtensions.UpdateCurrentItem).
        _sectionContainer.PageTransition = null;
        _sectionContainer.Content = null;

        _sectionContainer.PageTransition = transition;

        // Ensure focus is cleared before transition to avoid stale focus capturing input
        var topLevel = TopLevel.GetTopLevel(PlatformView);
        topLevel?.FocusManager?.ClearFocus();

        var pageHandler = page.ToHandler(MauiContext);
        if (pageHandler?.PlatformView is AvaloniaControl control)
        {
            // Detach from current parent if needed
            if (control.Parent != null && control.Parent != _sectionContainer)
            {
                control.DetachFromVisualTree();
            }

            _sectionContainer.Content = control;
        }
    }

    void IStackNavigation.RequestNavigation(NavigationRequest request)
        => HandleNavigationRequest(request);

    void IStackNavigation.NavigationFinished(IReadOnlyList<IView> newStack) { }
    
    private class ReversePageSlide : PageSlide
    {
        public ReversePageSlide(TimeSpan duration, SlideAxis orientation)
            : base(duration, orientation) { }

        public override Task Start(Visual? from, Visual? to, bool forward, CancellationToken cancellationToken)
            => base.Start(from, to, false, cancellationToken);
    }
}