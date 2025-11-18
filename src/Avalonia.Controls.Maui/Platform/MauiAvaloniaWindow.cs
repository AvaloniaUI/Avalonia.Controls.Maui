using Avalonia.Controls;
using System;
using System.Collections.Generic;

namespace Avalonia.Controls.Maui.Platform;

public class MauiAvaloniaWindow : Window, IDisposable
{
    private readonly Stack<Window> _modalWindows = new Stack<Window>();

    public MauiAvaloniaWindow()
    {
        Activated += OnActivated;
        Closed += OnClosed;
    }

    public void Dispose()
    {
        Activated -= OnActivated;
        Closed -= OnClosed;
    }

    protected virtual void OnActivated(object? sender, EventArgs e)
    {
    }

    protected virtual void OnClosed(object? sender, EventArgs e)
    {
    }

    /// <summary>
    /// Sets the main content of the window.
    /// </summary>
    public void SetMainContent(object? content)
    {
        Content = content;
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
