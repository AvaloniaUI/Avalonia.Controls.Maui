using Microsoft.Maui;
using Microsoft.Maui.Platform;

namespace Avalonia.Controls.Maui;

/// <summary>
/// Extension helpers to keep <see cref="ScrollViewer"/> mapping logic in one place.
/// </summary>
public static class ScrollViewExtensions
{
    /// <summary>
    /// Converts .NET MAUI scroll bar visibility to the Avalonia equivalent.
    /// </summary>
    /// <param name="visibility">The .NET MAUI scroll bar visibility value to convert.</param>
    /// <returns>The corresponding Avalonia scroll bar visibility value.</returns>
    public static global::Avalonia.Controls.Primitives.ScrollBarVisibility ToAvaloniaScrollBarVisibility(this ScrollBarVisibility visibility)
    {
        return visibility switch
        {
            ScrollBarVisibility.Default => Primitives.ScrollBarVisibility.Auto,
            ScrollBarVisibility.Never => Primitives.ScrollBarVisibility.Hidden,
            ScrollBarVisibility.Always => Primitives.ScrollBarVisibility.Visible,
            _ => Primitives.ScrollBarVisibility.Auto,
        };
    }

    /// <summary>
    /// Applies the current .NET MAUI content into the Avalonia <see cref="ScrollViewer"/>.
    /// </summary>
    /// <param name="platformScrollView">The Avalonia scroll viewer to update.</param>
    /// <param name="scrollView">The .NET MAUI scroll view containing the content.</param>
    /// <param name="mauiContext">The MAUI context for creating platform views.</param>
    public static void UpdateContent(this ScrollViewer platformScrollView, IScrollView scrollView, IMauiContext? mauiContext)
    {
        if (platformScrollView == null)
        {
            return;
        }

        if (mauiContext == null || scrollView.Content is not IView content)
        {
            platformScrollView.Content = null;
            return;
        }

        platformScrollView.Content = content.ToPlatform(mauiContext) as Control;
    }

    /// <summary>
    /// Updates scroll bar visibility honoring the configured orientation.
    /// </summary>
    /// <param name="platformScrollView">The Avalonia scroll viewer to update.</param>
    /// <param name="scrollView">The .NET MAUI scroll view with visibility and orientation settings.</param>
    public static void UpdateScrollBars(this ScrollViewer platformScrollView, IScrollView scrollView)
    {
        if (platformScrollView == null)
        {
            return;
        }

        var horizontalVisibility = scrollView.HorizontalScrollBarVisibility.ToAvaloniaScrollBarVisibility();
        var verticalVisibility = scrollView.VerticalScrollBarVisibility.ToAvaloniaScrollBarVisibility();

        switch (scrollView.Orientation)
        {
            case ScrollOrientation.Horizontal:
                platformScrollView.HorizontalScrollBarVisibility = horizontalVisibility;
                platformScrollView.VerticalScrollBarVisibility = Primitives.ScrollBarVisibility.Disabled;
                break;
            case ScrollOrientation.Vertical:
                platformScrollView.HorizontalScrollBarVisibility = Primitives.ScrollBarVisibility.Disabled;
                platformScrollView.VerticalScrollBarVisibility = verticalVisibility;
                break;
            case ScrollOrientation.Both:
                platformScrollView.HorizontalScrollBarVisibility = horizontalVisibility;
                platformScrollView.VerticalScrollBarVisibility = verticalVisibility;
                break;
            default:
                platformScrollView.HorizontalScrollBarVisibility = Primitives.ScrollBarVisibility.Disabled;
                platformScrollView.VerticalScrollBarVisibility = Primitives.ScrollBarVisibility.Disabled;
                break;
        }
    }

    /// <summary>
    /// Applies the configured orientation, keeping scroll bar visibility in sync.
    /// </summary>
    /// <param name="platformScrollView">The Avalonia scroll viewer to update.</param>
    /// <param name="scrollView">The .NET MAUI scroll view with the orientation setting.</param>
    public static void UpdateOrientation(this ScrollViewer platformScrollView, IScrollView scrollView)
    {
        platformScrollView.UpdateScrollBars(scrollView);
    }

    /// <summary>
    /// Moves the scroll viewer to the requested offsets.
    /// </summary>
    /// <param name="platformScrollView">The Avalonia scroll viewer to scroll.</param>
    /// <param name="request">The scroll request containing horizontal and vertical offset values.</param>
    public static void ScrollTo(this ScrollViewer platformScrollView, ScrollToRequest request)
    {
        if (platformScrollView == null)
        {
            return;
        }

        var targetOffset = new Vector(request.HorizontalOffset, request.VerticalOffset);
        platformScrollView.SetValue(ScrollViewer.OffsetProperty, targetOffset);
    }

    /// <summary>
    /// Pushes the current platform offsets into the virtual view and notifies completion.
    /// </summary>
    /// <param name="platformScrollView">The Avalonia scroll viewer to read offsets from.</param>
    /// <param name="scrollView">The .NET MAUI scroll view to update with the current offset values.</param>
    public static void UpdateOffsets(this ScrollViewer platformScrollView, IScrollView scrollView)
    {
        if (platformScrollView == null)
        {
            return;
        }

        var offset = platformScrollView.Offset;
        scrollView.HorizontalOffset = offset.X;
        scrollView.VerticalOffset = offset.Y;
        scrollView.ScrollFinished();
    }

    /// <summary>
    /// Updates offsets from a scroll request directly, useful when the platform view's offset cannot be reliably read back.
    /// </summary>
    /// <param name="platformScrollView">The Avalonia scroll viewer (not used, included for API consistency).</param>
    /// <param name="scrollView">The .NET MAUI scroll view to update with offset values.</param>
    /// <param name="request">The scroll request containing the offset values to apply.</param>
    public static void UpdateOffsetsFromRequest(this ScrollViewer platformScrollView, IScrollView scrollView, ScrollToRequest request)
    {
        if (platformScrollView == null)
        {
            return;
        }

        scrollView.HorizontalOffset = request.HorizontalOffset;
        scrollView.VerticalOffset = request.VerticalOffset;
        scrollView.ScrollFinished();
    }
}
