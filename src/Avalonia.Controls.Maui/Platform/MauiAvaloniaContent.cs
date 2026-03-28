using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Media;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Avalonia.Controls.Maui.Platform;

/// <summary>
/// A content container for MAUI applications running in single-view mode (browser, mobile, etc.)
/// This class doesn't inherit from Window to avoid windowing platform dependencies.
/// Uses a <see cref="Grid"/> as its root layout so that modal overlays can be stacked
/// on top of the main content.
/// </summary>
public class MauiAvaloniaContent : ContentControl, IDisposable
{
    private readonly Stack<(Control Content, Panel Scrim)> _modalStack = new();
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
        Content = _rootGrid;
    }

    /// <summary>
    /// Releases any resources held by this content container.
    /// </summary>
    public void Dispose()
    {
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
    public async void PresentModal(Control modalContent, bool animated = true)
    {
        int zBase = 100 + _modalStack.Count * 2;

        var scrim = new Panel
        {
            Background = new SolidColorBrush(Colors.Black, 0.5),
            ZIndex = zBase,
            IsHitTestVisible = true,
            Opacity = animated ? 0 : 1
        };

        var theme = Application.Current?.ActualThemeVariant;
        var defaultBg = theme == Avalonia.Styling.ThemeVariant.Dark
            ? new SolidColorBrush(Color.Parse("#1F1F1F"))
            : (IBrush)Brushes.White;

        var wrapper = new Border
        {
            Child = modalContent,
            Background = defaultBg,
            ZIndex = zBase + 1,
            HorizontalAlignment = HorizontalAlignment.Stretch,
            VerticalAlignment = VerticalAlignment.Stretch
        };

        _modalStack.Push((wrapper, scrim));
        _rootGrid.Children.Add(scrim);
        _rootGrid.Children.Add(wrapper);

        if (animated)
        {
            var containerHeight = Bounds.Height;
            if (containerHeight <= 0) containerHeight = 800;

            var transform = new TranslateTransform(0, containerHeight);
            wrapper.RenderTransform = transform;

            var duration = TimeSpan.FromMilliseconds(400);
            var startTime = DateTime.Now;

            while (DateTime.Now - startTime < duration)
            {
                var progress = (DateTime.Now - startTime).TotalMilliseconds / duration.TotalMilliseconds;
                progress = Math.Min(1.0, progress);
                progress = 1 - Math.Pow(1 - progress, 3); // ease out cubic

                transform.Y = containerHeight * (1 - progress);
                scrim.Opacity = progress;
                await Task.Delay(16);
            }

            transform.Y = 0;
            scrim.Opacity = 1;
            wrapper.RenderTransform = null;
        }
    }

    /// <summary>
    /// Dismisses the top-most modal overlay.
    /// </summary>
    public async void DismissModal(bool animated = true)
    {
        if (_modalStack.Count == 0)
            return;

        var (modalContent, scrim) = _modalStack.Pop();

        try
        {
            if (animated)
            {
                var containerHeight = Bounds.Height;
                if (containerHeight <= 0) containerHeight = 800;

                var transform = new TranslateTransform(0, 0);
                modalContent.RenderTransform = transform;

                var duration = TimeSpan.FromMilliseconds(400);
                var startTime = DateTime.Now;

                while (DateTime.Now - startTime < duration)
                {
                    var progress = (DateTime.Now - startTime).TotalMilliseconds / duration.TotalMilliseconds;
                    progress = Math.Min(1.0, progress);
                    progress = 1 - Math.Pow(1 - progress, 3); // ease out cubic

                    transform.Y = containerHeight * progress;
                    scrim.Opacity = 1 - progress;
                    await Task.Delay(16);
                }
            }
        }
        finally
        {
            _rootGrid.Children.Remove(modalContent);
            _rootGrid.Children.Remove(scrim);
        }
    }

    /// <summary>
    /// Removes all modal overlays from the visual tree and clears the modal stack.
    /// </summary>
    public void ClearAllModals()
    {
        while (_modalStack.Count > 0)
        {
            var (modalContent, scrim) = _modalStack.Pop();
            _rootGrid.Children.Remove(modalContent);
            _rootGrid.Children.Remove(scrim);
        }
    }

    /// <summary>
    /// Gets the current number of presented modals.
    /// </summary>
    public int ModalCount => _modalStack.Count;
}
