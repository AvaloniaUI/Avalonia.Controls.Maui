using Microsoft.Maui.Handlers;
using Avalonia.Animation;
using Microsoft.Maui;
using Microsoft.Maui.Controls;
using AvaloniaControl = Avalonia.Controls.Control;
using MauiShell = Microsoft.Maui.Controls.Shell;
using Avalonia.Controls.Maui.Extensions;

namespace Avalonia.Controls.Maui.Handlers.Shell;

public partial class ShellSectionHandler : ElementHandler<ShellSection, AvaloniaControl>, IStackNavigation
{
    public static IPropertyMapper<ShellSection, ShellSectionHandler> Mapper =
        new PropertyMapper<ShellSection, ShellSectionHandler>(ElementHandler.ElementMapper)
        {
            [nameof(ShellSection.CurrentItem)] = MapCurrentItem,
            [nameof(ShellSection.Items)] = MapItems,
        };

    public static CommandMapper<ShellSection, ShellSectionHandler> CommandMapper =
        new CommandMapper<ShellSection, ShellSectionHandler>(ElementHandler.ElementCommandMapper)
        {
            [nameof(IStackNavigation.RequestNavigation)] = RequestNavigation
        };

    TransitioningContentControl? _sectionContainer;
    readonly Stack<Page> _navigationStack = new();
    Page? _currentPage;
    bool _isNavigating;

    internal static readonly TimeSpan DefaultTransitionDuration = TimeSpan.FromMilliseconds(250);
    internal static readonly TimeSpan PageSlideDuration = TimeSpan.FromMilliseconds(300);
    internal static readonly TimeSpan ModalTransitionDuration = TimeSpan.FromMilliseconds(400);

    public ShellSectionHandler() : base(Mapper, CommandMapper) { }

    public ShellSectionHandler(IPropertyMapper? mapper, CommandMapper? commandMapper)
        : base(mapper ?? Mapper, commandMapper ?? CommandMapper) { }

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

    protected override void ConnectHandler(AvaloniaControl platformView)
    {
        base.ConnectHandler(platformView);

        if (VirtualView is IShellSectionController sectionController)
            sectionController.NavigationRequested += OnNavigationRequested;

        UpdateCurrentItem();
        platformView.AttachedToVisualTree += OnAttachedToVisualTree;
    }

    protected override void DisconnectHandler(AvaloniaControl platformView)
    {
        if (VirtualView is IShellSectionController sectionController)
            sectionController.NavigationRequested -= OnNavigationRequested;

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

    public static void MapCurrentItem(ShellSectionHandler handler, ShellSection section)
        => handler.UpdateCurrentItem();

    public static void MapItems(ShellSectionHandler handler, ShellSection section)
        => handler.UpdateCurrentItem();

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
        if (request.NavigationStack == null || request.NavigationStack.Count == 0)
            return;

        _isNavigating = true;
        try
        {
            int oldCount = _navigationStack.Count;
            int newCount = request.NavigationStack.Count;
            bool isPush = newCount > oldCount;
            bool isPop = newCount < oldCount;

            // Capture the outgoing page before updating (needed for pop reverse animation)
            Page? poppedPage = isPop ? _currentPage : null;

            // Update internal stack to match the request
            _navigationStack.Clear();
            foreach (var view in request.NavigationStack)
            {
                if (view is Page page)
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
        Page? poppedPage = isPop ? _currentPage : null;

        // Sync internal stack
        _navigationStack.Clear();
        foreach (var page in externalStack)
            _navigationStack.Push(page);

        // Determine top page
        Page? topPage = _navigationStack.Count > 0 ? _navigationStack.Peek() : null;

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
    
    private void DisplayPage(Page page, bool isPush, bool isPop, Page? poppedPage)
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

        _sectionContainer.PageTransition = transition;

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