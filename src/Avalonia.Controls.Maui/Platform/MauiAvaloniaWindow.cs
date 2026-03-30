using Avalonia.Controls;
using Avalonia.Controls.Chrome;
using Avalonia.Input;
using Avalonia.Layout;
using Avalonia.Media;
using Microsoft.Maui;
using Microsoft.Maui.Platform;
using System;
using System.Collections.Generic;

namespace Avalonia.Controls.Maui.Platform;

/// <summary>
/// Main window for MAUI applications running on Avalonia, providing root layout, title bar, and modal overlay support.
/// </summary>
/// <remarks>
/// The window uses a <see cref="DockPanel"/> inside a <see cref="Grid"/> as its root layout.
/// The <see cref="DockPanel"/> hosts the optional title bar and main content, while the outer
/// <see cref="Grid"/> allows modal overlays to be stacked on top of all other content.
/// </remarks>
public class MauiAvaloniaWindow : Window, IDisposable
{
    private readonly ModalOverlayHelper _modalHelper;
    private readonly DockPanel _rootPanel;
    private readonly Grid _rootGrid;
    private Control? _mainContent;
    private TitleBarView? _titleBarView;
    private IMauiContext? _mauiContext;

    /// <summary>
    /// Gets the TitleBarView if one is set.
    /// </summary>
    public TitleBarView? TitleBarView => _titleBarView;

    /// <summary>
    /// Initializes a new instance of <see cref="MauiAvaloniaWindow"/> with a root layout consisting of a
    /// <see cref="DockPanel"/> inside a <see cref="Grid"/> to support title bar docking and modal overlays.
    /// </summary>
    public MauiAvaloniaWindow()
    {
        _rootPanel = new DockPanel
        {
            LastChildFill = true
        };

        // Use a Grid as root so modal overlays can sit on top of everything.
        // Tag as "OverlayWrapper" so AlertManager adds its overlay as a sibling
        // instead of wrapping Window.Content in a new Grid — which would detach
        // the entire visual tree and trigger spurious MAUI Unloaded/Loaded events
        // that reset Shell state (e.g. hamburger button visibility).
        _rootGrid = new Grid { Tag = "OverlayWrapper" };
        _modalHelper = new ModalOverlayHelper(_rootGrid, () =>
        {
            var h = Bounds.Height;
            return h > 0 ? h : ClientSize.Height;
        });
        _rootGrid.Children.Add(_rootPanel);

        Content = _rootGrid;

        Activated += OnActivated;
        Deactivated += OnDeactivated;
        Closed += OnClosed;
    }

    /// <summary>
    /// Releases resources by unsubscribing from window lifecycle event handlers.
    /// </summary>
    public void Dispose()
    {
        Activated -= OnActivated;
        Deactivated -= OnDeactivated;
        Closed -= OnClosed;
    }

    /// <summary>
    /// Handles window activation by updating the title bar to its active visual state.
    /// </summary>
    /// <param name="sender">The event source.</param>
    /// <param name="e">The event data.</param>
    protected virtual void OnActivated(object? sender, EventArgs e)
    {
        _titleBarView?.SetActiveState(true);
    }

    /// <summary>
    /// Handles window deactivation by updating the title bar to its inactive visual state.
    /// </summary>
    /// <param name="sender">The event source.</param>
    /// <param name="e">The event data.</param>
    protected virtual void OnDeactivated(object? sender, EventArgs e)
    {
        _titleBarView?.SetActiveState(false);
    }

    /// <summary>
    /// Handles the window close event.
    /// </summary>
    /// <param name="sender">The event source.</param>
    /// <param name="e">The event data.</param>
    protected virtual void OnClosed(object? sender, EventArgs e)
    {
    }

    /// <summary>
    /// Sets the MAUI context for the window.
    /// </summary>
    public void SetMauiContext(IMauiContext? mauiContext)
    {
        _mauiContext = mauiContext;
        if (_titleBarView != null)
        {
            _titleBarView.MauiContext = mauiContext;
        }
    }

    /// <summary>
    /// Sets the main content of the window.
    /// </summary>
    public void SetMainContent(object? content)
    {
        // Remove old main content
        if (_mainContent != null)
        {
            _rootPanel.Children.Remove(_mainContent);
        }

        _mainContent = content as Control;

        if (_mainContent != null)
        {
            // Main content fills the remaining space (after TitleBar)
            _rootPanel.Children.Add(_mainContent);
        }
    }

    /// <summary>
    /// Sets the TitleBar for the window.
    /// </summary>
    public void SetTitleBar(ITitleBar? titleBar, IMauiContext? mauiContext)
    {
        _mauiContext = mauiContext;

        // Remove existing TitleBar
        if (_titleBarView != null)
        {
            WindowDecorationProperties.SetElementRole(_titleBarView, WindowDecorationsElementRole.None);
            _rootPanel.Children.Remove(_titleBarView);
            _titleBarView = null;

            // Reset window decorations when removing title bar
            ExtendClientAreaToDecorationsHint = false;
            ExtendClientAreaTitleBarHeightHint = -1;
        }

        if (titleBar == null || mauiContext == null)
        {
            return;
        }

        // Create the TitleBar view using the handler
        var handler = titleBar.ToHandler(mauiContext);
        if (handler?.PlatformView is TitleBarView titleBarView)
        {
            _titleBarView = titleBarView;
            _titleBarView.MauiContext = mauiContext;
            DockPanel.SetDock(_titleBarView, Dock.Top);

            // Enable extended client area for custom title bar
            ExtendClientAreaToDecorationsHint = true;
            ExtendClientAreaTitleBarHeightHint = 32; // Match TitleBarView height

            // Mark the title bar view with the TitleBar role for platform hit-testing.
            // The platform will handle window dragging and non-client area behavior.
            WindowDecorationProperties.SetElementRole(_titleBarView, WindowDecorationsElementRole.TitleBar);

            // Insert at the beginning so it's at the top
            _rootPanel.Children.Insert(0, _titleBarView);

            // Set initial active state
            _titleBarView.SetActiveState(IsActive);
        }
        else if (handler?.PlatformView is Control control)
        {
            // If handler returns a different control type, wrap it
            DockPanel.SetDock(control, Dock.Top);
            _rootPanel.Children.Insert(0, control);
        }
    }

    /// <summary>
    /// Sets the visibility of the TitleBar.
    /// </summary>
    public void SetTitleBarVisibility(bool isVisible)
    {
        if (_titleBarView != null)
        {
            _titleBarView.IsVisible = isVisible;
        }
    }

    /// <summary>
    /// Presents a modal page as a fullscreen overlay within the same window.
    /// Covers all shell chrome (flyout, navbar, tabs) for a true modal experience.
    /// </summary>
    public void PresentModal(Control modalContent, bool animated = true)
    {
        _modalHelper.Present(modalContent, animated);
    }

    /// <summary>
    /// Dismisses the top-most modal overlay.
    /// </summary>
    public void DismissModal(bool animated = true)
    {
        _modalHelper.Dismiss(animated);
    }

    /// <summary>
    /// Removes all modal overlays from the visual tree and clears the modal stack.
    /// </summary>
    /// <remarks>
    /// This is used when the window's page changes, because MAUI's ModalNavigationManager
    /// clears its internal stack without firing ModalPopped events, leaving stale overlay
    /// controls in the root grid.
    /// </remarks>
    public void ClearAllModals()
    {
        _modalHelper.ClearAll();
    }

    /// <summary>
    /// Gets the current number of presented modals.
    /// </summary>
    public int ModalCount => _modalHelper.Count;
}
