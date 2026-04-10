using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Media;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Avalonia.Controls.Maui.Platform;

/// <summary>
/// Manages modal overlay presentation and dismissal within a <see cref="Grid"/> container.
/// Used by both <see cref="MauiAvaloniaWindow"/> and <see cref="MauiAvaloniaContent"/>
/// to provide consistent modal behavior across desktop and single-view (browser) platforms.
/// </summary>
internal sealed class ModalOverlayHelper
{
    private readonly Stack<(Control Content, Panel Scrim)> _modalStack = new();
    private readonly Grid _rootGrid;
    private readonly Func<double> _getHeight;

    /// <summary>
    /// Initializes a new instance of <see cref="ModalOverlayHelper"/>.
    /// </summary>
    /// <param name="rootGrid">The root <see cref="Grid"/> that modal overlays are added to.</param>
    /// <param name="getHeight">A function that returns the current container height for animations.</param>
    public ModalOverlayHelper(Grid rootGrid, Func<double> getHeight)
    {
        _rootGrid = rootGrid;
        _getHeight = getHeight;
    }

    /// <summary>
    /// Gets the current number of presented modals.
    /// </summary>
    public int Count => _modalStack.Count;

    /// <summary>
    /// Presents a modal page as a fullscreen overlay.
    /// </summary>
    public async void Present(Control modalContent, bool animated = true)
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
            var height = _getHeight();
            if (height <= 0) height = 800;

            var transform = new TranslateTransform(0, height);
            wrapper.RenderTransform = transform;

            var duration = TimeSpan.FromMilliseconds(400);
            var startTime = DateTime.Now;

            while (DateTime.Now - startTime < duration)
            {
                var progress = (DateTime.Now - startTime).TotalMilliseconds / duration.TotalMilliseconds;
                progress = Math.Min(1.0, progress);
                progress = 1 - Math.Pow(1 - progress, 3); // ease out cubic

                transform.Y = height * (1 - progress);
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
    public async void Dismiss(bool animated = true)
    {
        if (_modalStack.Count == 0)
            return;

        var (modalContent, scrim) = _modalStack.Pop();

        try
        {
            if (animated)
            {
                var height = _getHeight();
                if (height <= 0) height = 800;

                var transform = new TranslateTransform(0, 0);
                modalContent.RenderTransform = transform;

                var duration = TimeSpan.FromMilliseconds(400);
                var startTime = DateTime.Now;

                while (DateTime.Now - startTime < duration)
                {
                    var progress = (DateTime.Now - startTime).TotalMilliseconds / duration.TotalMilliseconds;
                    progress = Math.Min(1.0, progress);
                    progress = 1 - Math.Pow(1 - progress, 3); // ease out cubic

                    transform.Y = height * progress;
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
    public void ClearAll()
    {
        while (_modalStack.Count > 0)
        {
            var (modalContent, scrim) = _modalStack.Pop();
            _rootGrid.Children.Remove(modalContent);
            _rootGrid.Children.Remove(scrim);
        }
    }
}
