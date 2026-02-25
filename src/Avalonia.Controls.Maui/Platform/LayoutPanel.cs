using Avalonia.Controls;

using AvaloniaSize = Avalonia.Size;
using MauiSize = Microsoft.Maui.Graphics.Size;
using MauiRect = Microsoft.Maui.Graphics.Rect;

namespace Avalonia.Controls.Maui.Platform;

public class LayoutPanel : Panel
{
    internal Func<double, double, MauiSize>? CrossPlatformMeasure { get; set; }
    internal Func<MauiRect, MauiSize>? CrossPlatformArrange { get; set; }

    protected override AvaloniaSize MeasureOverride(AvaloniaSize availableSize)
    {
        if (CrossPlatformMeasure == null)
        {
            return base.MeasureOverride(availableSize);
        }

        var width = availableSize.Width;
        var height = availableSize.Height;

        var crossPlatformSize = CrossPlatformMeasure(width, height);

        width = crossPlatformSize.Width;
        height = crossPlatformSize.Height;

        return new AvaloniaSize(width, height);
    }

    protected override AvaloniaSize ArrangeOverride(AvaloniaSize finalSize)
    {
        if (CrossPlatformArrange == null)
        {
            return base.ArrangeOverride(finalSize);
        }

        var width = finalSize.Width;
        var height = finalSize.Height;

        CrossPlatformArrange(new MauiRect(0, 0, width, height));

        // Always return finalSize rather than the cross-platform arrange result.
        // MAUI layout managers may return a smaller size (or even zero) from ArrangeChildren,
        // but native platform containers always fill the space given by the parent.
        // In Avalonia, returning a smaller size would cause the panel to be repositioned
        // (e.g., centered for Stretch alignment), shifting all children.
        return finalSize;
    }
}