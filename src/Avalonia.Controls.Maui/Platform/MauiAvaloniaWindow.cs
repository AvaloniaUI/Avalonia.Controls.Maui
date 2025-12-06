using Avalonia.Controls;
using Avalonia.Layout;
using Microsoft.Maui;
using Microsoft.Maui.Platform;
using System;
using System.Collections.Generic;

namespace Avalonia.Controls.Maui.Platform;

public class MauiAvaloniaWindow : Window, IDisposable
{
    private readonly Stack<Window> _modalWindows = new Stack<Window>();
    private readonly DockPanel _rootPanel;
    private Control? _mainContent;
    private TitleBarView? _titleBarView;
    private IMauiContext? _mauiContext;

    /// <summary>
    /// Gets the TitleBarView if one is set.
    /// </summary>
    public TitleBarView? TitleBarView => _titleBarView;

    public MauiAvaloniaWindow()
    {
        _rootPanel = new DockPanel
        {
            LastChildFill = true
        };
        Content = _rootPanel;

        Activated += OnActivated;
        Deactivated += OnDeactivated;
        Closed += OnClosed;
    }

    public void Dispose()
    {
        Activated -= OnActivated;
        Deactivated -= OnDeactivated;
        Closed -= OnClosed;
    }

    protected virtual void OnActivated(object? sender, EventArgs e)
    {
        _titleBarView?.SetActiveState(true);
    }

    protected virtual void OnDeactivated(object? sender, EventArgs e)
    {
        _titleBarView?.SetActiveState(false);
    }

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
            _rootPanel.Children.Remove(_titleBarView);
            _titleBarView = null;
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
    /// Presents a modal page as a modal window.
    /// </summary>
    public async void PresentModal(Control modalContent)
    {
        var modalWindow = new Window
        {
            Content = modalContent,
            Width = Width,
            Height = Height,
            WindowStartupLocation = WindowStartupLocation.CenterOwner,
            CanResize = CanResize,
            SystemDecorations = SystemDecorations.Full
        };

        _modalWindows.Push(modalWindow);

        await modalWindow.ShowDialog(this);
    }

    /// <summary>
    /// Dismisses the top-most modal window.
    /// </summary>
    public void DismissModal()
    {
        if (_modalWindows.Count == 0)
            return;

        var modalWindow = _modalWindows.Pop();
        modalWindow.Close();
    }

    /// <summary>
    /// Gets the current number of presented modals.
    /// </summary>
    public int ModalCount => _modalWindows.Count;
}
