using Avalonia.Controls;

using AvaloniaSize = Avalonia.Size;
using MauiSize = Microsoft.Maui.Graphics.Size;
using MauiRect = Microsoft.Maui.Graphics.Rect;

namespace Avalonia.Controls.Maui.Platform;

/// <summary>
/// Avalonia content presenter control used as a container for hosting MAUI ContentView content.
/// When used as a wrapper container (e.g. for Clip or Shadow), arranges its children to fill the available space.
/// </summary>
public class ContentView : MauiView
{
    /// <inheritdoc/>
    protected override AvaloniaSize MeasureOverride(AvaloniaSize availableSize)
    {
        // If a cross-platform layout is set, delegate to it (normal ContentView handler path).
        if (CrossPlatformLayout is not null)
            return base.MeasureOverride(availableSize);

        // Container wrapper path: measure all children and return the max size.
        AvaloniaSize maxSize = default;
        foreach (var child in Children)
        {
            child.Measure(availableSize);
            maxSize = new AvaloniaSize(
                Math.Max(maxSize.Width, child.DesiredSize.Width),
                Math.Max(maxSize.Height, child.DesiredSize.Height));
        }

        return maxSize;
    }

    /// <inheritdoc/>
    protected override AvaloniaSize ArrangeOverride(AvaloniaSize finalSize)
    {
        // If a cross-platform layout is set, delegate to it (normal ContentView handler path).
        if (CrossPlatformLayout is not null)
            return base.ArrangeOverride(finalSize);

        // Container wrapper path: arrange all children to fill the available space.
        foreach (var child in Children)
        {
            child.Arrange(new Rect(finalSize));
        }

        return finalSize;
    }
}