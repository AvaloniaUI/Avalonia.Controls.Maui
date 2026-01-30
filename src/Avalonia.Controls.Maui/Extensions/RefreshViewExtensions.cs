using Avalonia.Controls.Maui.Handlers;
using Avalonia.Media;
using Avalonia.VisualTree;
using Microsoft.Maui;
using PlatformView = Avalonia.Controls.RefreshContainer;

namespace Avalonia.Controls.Maui.Platform;

/// <summary>
/// Provides extension methods for updating <see cref="RefreshContainer"/> properties from cross-platform
/// .NET MAUI refresh view interfaces. These methods handle the platform-specific implementation details
/// for rendering refresh functionality on Avalonia-based platforms.
/// </summary>
/// <remarks>
/// This class is primarily used by <see cref="RefreshViewHandler"/> to apply property changes from the
/// cross-platform virtual view to the native Avalonia control. Each method is designed to be
/// lightweight and perform only the necessary updates to maintain optimal performance during
/// property change notifications.
/// </remarks>
public static class RefreshViewExtensions
{
    /// <summary>
    /// Updates the content of the refresh view.
    /// </summary>
    /// <param name="platformView">The platform refresh container control.</param>
    /// <param name="refreshView">The cross-platform refresh view.</param>
    /// <param name="mauiContext">The MAUI context for converting views.</param>
    /// <remarks>
    /// The content should typically be a scrollable control such as ScrollView, CollectionView, or ListView
    /// to enable pull-to-refresh gesture recognition.
    /// </remarks>
    public static void UpdateContent(this PlatformView platformView, IRefreshView refreshView, IMauiContext? mauiContext)
    {
        if (mauiContext == null)
            return;

        var contentView = refreshView as IContentView;
        var presentedContent = contentView?.PresentedContent ?? refreshView.Content;

        if (presentedContent == null)
        {
            platformView.Content = null;
            return;
        }

        var content = presentedContent.ToPlatform(mauiContext);

        platformView.Content = content;
    }

    /// <summary>
    /// Updates the color of the refresh indicator.
    /// </summary>
    /// <param name="platformView">The platform refresh container control.</param>
    /// <param name="refreshView">The cross-platform refresh view.</param>
    /// <remarks>
    /// The refresh color is applied to the foreground of the RefreshVisualizer component,
    /// which displays the spinning progress indicator during refresh operations.
    /// </remarks>
    public static void UpdateRefreshColor(this PlatformView platformView, IRefreshView refreshView)
    {
        var visualizer = platformView.Visualizer;
        
        if (visualizer == null)
            return;
        
        IBrush? brush = refreshView.RefreshColor?.ToPlatform();
        
        if (brush != null)
            visualizer.Foreground = brush;
        else
            visualizer.ClearValue(Primitives.TemplatedControl.ForegroundProperty);

        if (visualizer is Visual visual)
        {
            var descendants = visual.GetVisualDescendants();
            foreach (var shape in descendants.OfType<Shapes.Shape>())
            {
                if (brush != null)
                {
                    shape.Stroke = brush;
                    if (shape is Shapes.Path path)
                    {
                        path.Fill = brush;
                    }
                }
                else
                {
                    shape.ClearValue(Shapes.Shape.StrokeProperty);
                    shape.ClearValue(Shapes.Shape.FillProperty);
                }
            }
        }
    }

    /// <summary>
    /// Updates the background of the refresh view.
    /// </summary>
    /// <param name="platformView">The platform refresh container control.</param>
    /// <param name="refreshView">The cross-platform refresh view.</param>
    /// <remarks>
    /// The background is applied to the RefreshContainer itself, providing a backdrop
    /// for the refresh indicator and content.
    /// </remarks>
    public static void UpdateBackground(this PlatformView platformView, IRefreshView refreshView)
    {
        platformView.Background = refreshView.Background?.ToPlatform() ?? Media.Brushes.Transparent;
    }

    /// <summary>
    /// Updates the enabled state of the refresh view.
    /// </summary>
    /// <param name="platformView">The platform refresh container control.</param>
    /// <param name="refreshView">The cross-platform refresh view.</param>
    /// <remarks>
    /// When disabled, the pull-to-refresh gesture is not recognized, but the content
    /// remains interactive for normal scrolling and user interaction.
    /// </remarks>
    public static void UpdateIsEnabled(this PlatformView platformView, IRefreshView refreshView)
    {
        platformView.IsEnabled = refreshView.IsEnabled;
    }
}