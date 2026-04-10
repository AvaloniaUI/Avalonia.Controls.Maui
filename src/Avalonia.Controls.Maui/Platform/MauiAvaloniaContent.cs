using Avalonia.Controls;
using System;

namespace Avalonia.Controls.Maui.Platform;

/// <summary>
/// A content container for MAUI applications running in single-view mode (browser, mobile, etc.)
/// This class doesn't inherit from Window to avoid windowing platform dependencies.
/// Uses a <see cref="Grid"/> as its root layout so that modal overlays can be stacked
/// on top of the main content.
/// </summary>
public class MauiAvaloniaContent : ContentControl, IDisposable
{
    private readonly ModalOverlayHelper _modalHelper;
    private readonly Grid _rootGrid;
    private Control? _mainContent;

    /// <summary>
    /// Initializes a new instance of <see cref="MauiAvaloniaContent"/>.
    /// </summary>
    public MauiAvaloniaContent()
    {
        // Use a Grid as root so modal overlays can sit on top of everything.
        // Tag as "OverlayWrapper" so AlertManager adds its overlay as a sibling
        // instead of wrapping Content in a new Grid.
        _rootGrid = new Grid { Tag = "OverlayWrapper" };
        _modalHelper = new ModalOverlayHelper(_rootGrid, () => Bounds.Height);
        Content = _rootGrid;
    }

    /// <summary>
    /// Releases any resources held by this content container.
    /// </summary>
    public void Dispose()
    {
        ClearAllModals();

        if (_mainContent != null)
        {
            _rootGrid.Children.Remove(_mainContent);
            _mainContent = null;
        }
    }

    /// <summary>
    /// Sets the main content.
    /// </summary>
    public void SetMainContent(object? content)
    {
        if (_mainContent != null)
        {
            _rootGrid.Children.Remove(_mainContent);
        }

        _mainContent = content as Control;

        if (_mainContent != null)
        {
            // Insert at index 0 so it stays behind any modal overlays
            _rootGrid.Children.Insert(0, _mainContent);
        }
    }

    /// <summary>
    /// Presents a modal page as a fullscreen overlay within the content area.
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
    public void ClearAllModals()
    {
        _modalHelper.ClearAll();
    }

    /// <summary>
    /// Gets the current number of presented modals.
    /// </summary>
    public int ModalCount => _modalHelper.Count;
}
