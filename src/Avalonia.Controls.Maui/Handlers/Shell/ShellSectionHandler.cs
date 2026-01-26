using Microsoft.Maui.Handlers;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using Avalonia.Controls;
using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Platform;
using AvaloniaControl = Avalonia.Controls.Control;

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

    ContentControl? _sectionContainer;
    ShellContentHandler? _currentContentHandler;
    readonly Stack<Microsoft.Maui.Controls.Page> _navigationStack = new();

    public ShellSectionHandler() : base(Mapper, CommandMapper)
    {
    }

    public ShellSectionHandler(IPropertyMapper? mapper, CommandMapper? commandMapper)
        : base(mapper ?? Mapper, commandMapper ?? CommandMapper)
    {
    }

    protected override AvaloniaControl CreatePlatformElement()
    {
        _sectionContainer = new ContentControl
        {
            HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Stretch,
            VerticalAlignment = Avalonia.Layout.VerticalAlignment.Stretch,
            HorizontalContentAlignment = Avalonia.Layout.HorizontalAlignment.Stretch,
            VerticalContentAlignment = Avalonia.Layout.VerticalAlignment.Stretch,
            MinWidth = 0,
            MinHeight = 0
        };

        return _sectionContainer;
    }

    protected override void ConnectHandler(AvaloniaControl platformView)
    {
        base.ConnectHandler(platformView);

        if (VirtualView is IShellSectionController sectionController)
        {
            sectionController.NavigationRequested += OnNavigationRequested;
        }

        UpdateCurrentItem();
    }

    protected override void DisconnectHandler(AvaloniaControl platformView)
    {
        if (VirtualView is IShellSectionController sectionController)
        {
            sectionController.NavigationRequested -= OnNavigationRequested;
        }

        _currentContentHandler = null;

        base.DisconnectHandler(platformView);
    }

    public static void MapCurrentItem(ShellSectionHandler handler, ShellSection section)
    {
        handler.UpdateCurrentItem();
    }

    public static void MapItems(ShellSectionHandler handler, ShellSection section)
    {
        handler.UpdateCurrentItem();
    }

    public static void RequestNavigation(ShellSectionHandler handler, IStackNavigation view, object? arg)
    {
        if (arg is NavigationRequest navigationRequest)
        {
            handler.HandleNavigationRequest(navigationRequest);
        }
    }

    private void OnNavigationRequested(object? sender, EventArgs e)
    {
        // Handle navigation request from ShellSection
        SyncNavigationStack();
    }

    public void SyncNavigationStack()
    {
        if (VirtualView == null || _sectionContainer == null)
            return;

        // Use DisplayedPage which MAUI maintains correctly throughout navigation
        // Stack can contain null entries as placeholders, so DisplayedPage is the source of truth
        var topPage = VirtualView.DisplayedPage;

        if (topPage != null)
        {
            DisplayPage(topPage);
        }
    }

    private void HandleNavigationRequest(NavigationRequest request)
    {
        if (request.NavigationStack != null && request.NavigationStack.Count > 0)
        {
            // Full stack replacement
            _navigationStack.Clear();
            foreach (var view in request.NavigationStack.Reverse())
            {
                if (view is Microsoft.Maui.Controls.Page page)
                {
                    _navigationStack.Push(page);
                }
            }

            if (_navigationStack.Count > 0)
            {
                var topPage = _navigationStack.Peek();
                DisplayPage(topPage);
            }
        }
    }

    private void DisplayPage(Microsoft.Maui.Controls.Page page)
    {
        if (_sectionContainer == null || MauiContext == null)
            return;

        var pageHandler = page.ToHandler(MauiContext);
        if (pageHandler?.PlatformView is AvaloniaControl control)
        {
            // Only update if the content has changed
            if (_sectionContainer.Content != control)
            {
                // Remove from current parent if it has one
                if (control.Parent is ContentControl parentContainer)
                {
                    parentContainer.Content = null;
                }

                _sectionContainer.Content = control;
            }
        }
    }

    private void UpdateCurrentItem()
    {
        if (VirtualView?.CurrentItem == null || _sectionContainer == null || MauiContext == null)
            return;

        // Create handler for current content
        var handler = VirtualView.CurrentItem.ToHandler(MauiContext);
        _currentContentHandler = handler as ShellContentHandler;

        // Initialize navigation stack with the current page
        if (VirtualView.CurrentItem is IShellContentController contentController)
        {
            var page = contentController.GetOrCreateContent();
            if (page != null)
            {
                _navigationStack.Clear();
                _navigationStack.Push(page);
            }
        }

        // Use SyncNavigationStack to display the current page
        // This ensures we display pages directly, not through ShellContent handler
        SyncNavigationStack();
    }

    void IStackNavigation.RequestNavigation(NavigationRequest request)
    {
        HandleNavigationRequest(request);
    }

    void IStackNavigation.NavigationFinished(IReadOnlyList<IView> newStack)
    {
        // Navigation completed callback
    }
}
